using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class GhostApparitionRuntimeTests
{
    private GameObject root;
    private Scene isolated;
    private Component horrorEvent;
    private Component ghost;
    private GameObject ghostObject;
    private GameObject visual;
    private Transform spawn;
    private Light light;
    private AudioSource audio;
    private int started;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return new EnterPlayMode();
        started = 0;
        isolated = SceneManager.CreateScene("HE05 isolated test");
        root = new GameObject("HE05 test");
        SceneManager.MoveGameObjectToScene(root, isolated);
        horrorEvent = root.AddComponent(HorrorTestAccess.Type("GhostApparitionHorrorEvent"));
        ghostObject = new GameObject("Ghost_Prototype");
        ghostObject.transform.SetParent(root.transform);
        ghost = ghostObject.AddComponent(HorrorTestAccess.Type("GhostActor"));
        visual = new GameObject("Visual");
        visual.transform.SetParent(ghostObject.transform);
        HorrorTestAccess.Set(ghost, "visualRoot", visual);
        HorrorTestAccess.Call(ghost, "Hide");
        spawn = new GameObject("Spawn").transform;
        spawn.SetParent(root.transform);
        spawn.position = new Vector3(1f, 1.3f, 3f);
        spawn.rotation = Quaternion.Euler(0f, 135f, 0f);
        light = root.AddComponent<Light>();
        root.AddComponent(HorrorTestAccess.Type("UnityEngine.Rendering.HighDefinition.HDAdditionalLightData"));
        light.color = Color.cyan;
        audio = root.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        HorrorTestAccess.Set(horrorEvent, "ghost", ghost);
        HorrorTestAccess.Set(horrorEvent, "spawnPoint", spawn);
        HorrorTestAccess.Set(horrorEvent, "targetLight", light);
        HorrorTestAccess.Set(horrorEvent, "apparitionAudio", audio);
        Timing(0f, 0.05f, 0.05f);
        HorrorTestAccess.Callback(horrorEvent, "onEventStarted").AddListener(() => started++);
        yield return null; // Complete HDRP setup before any intensity/color baseline.
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (root != null) Object.Destroy(root);
        yield return null;
        if (isolated.IsValid() && isolated.isLoaded)
            yield return SceneManager.UnloadSceneAsync(isolated);
        yield return new ExitPlayMode();
    }

    private void Timing(float delay, float flicker, float visible)
    {
        HorrorTestAccess.Set(horrorEvent, "preDelay", delay);
        HorrorTestAccess.Set(horrorEvent, "flickerDuration", flicker);
        HorrorTestAccess.Set(horrorEvent, "visibleDuration", visible);
        HorrorTestAccess.Set(horrorEvent, "minFlickerInterval", 0.05f);
        HorrorTestAccess.Set(horrorEvent, "maxFlickerInterval", 0.05f);
    }

    private void AssertHidden()
    {
        Assert.That(visual.activeSelf, Is.False);
        Assert.That(HorrorTestAccess.State(ghost, "IsVisible"), Is.False);
        Assert.That(ghostObject.activeInHierarchy, Is.True);
        Assert.That(((Behaviour)ghost).enabled, Is.True);
    }

    [UnityTest]
    public IEnumerator InheritsBaseAndNullGhostStillRestoresLight()
    {
        Assert.That(horrorEvent.GetType().IsSubclassOf(HorrorTestAccess.Type("HorrorEventTrigger")), Is.True);
        HorrorTestAccess.Set(horrorEvent, "ghost", null);
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(horrorEvent);
        AssertHidden();
        Assert.That(light.enabled, Is.True);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator NullSpawnStillRestoresLight()
    {
        HorrorTestAccess.Set(horrorEvent, "spawnPoint", null);
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(horrorEvent);
        AssertHidden();
        Assert.That(light.enabled, Is.True);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator MissingLightAndAudioDoNotBlockGhost()
    {
        HorrorTestAccess.Set(horrorEvent, "targetLight", null);
        HorrorTestAccess.Set(horrorEvent, "apparitionAudio", null);
        Timing(0f, 0f, 0.2f);
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return null;
        Assert.That(visual.activeSelf, Is.True);
        yield return HorrorTestAccess.Complete(horrorEvent);
        AssertHidden();
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator TimelineRestoresInitiallyEnabledLight()
    {
        yield return CheckTimeline(true);
    }

    [UnityTest]
    public IEnumerator TimelineRestoresInitiallyDisabledLight()
    {
        yield return CheckTimeline(false);
    }

    private IEnumerator CheckTimeline(bool initiallyEnabled)
    {
        light.enabled = initiallyEnabled;
        Timing(0.15f, 0.15f, 0.2f);
        yield return null;
        float initialIntensity = light.intensity;
        Color initialColor = light.color;
        bool initialEnabled = light.enabled;
        Assert.That(audio.clip, Is.Null);
        Assert.That(new SerializedObject(horrorEvent).FindProperty("apparitionAudio").objectReferenceValue, Is.EqualTo(audio));
        float began = Time.time;
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        bool sawFlicker = false, sawGhost = false;
        do
        {
            float elapsed = Time.time - began;
            if (elapsed < 0.15f)
            {
                AssertHidden();
                Assert.That(light.enabled, Is.EqualTo(initialEnabled));
            }
            if (light.enabled != initialEnabled) sawFlicker = true;
            if (visual.activeSelf)
            {
                sawGhost = true;
                Assert.That(elapsed, Is.GreaterThanOrEqualTo(0.3f - 0.001f));
                Assert.That(Vector3.Distance(ghostObject.transform.position, spawn.position), Is.LessThan(0.0001f));
                Assert.That(Quaternion.Angle(ghostObject.transform.rotation, spawn.rotation), Is.LessThan(0.001f));
            }
            if (sawGhost && elapsed < 0.5f - 0.001f)
                Assert.That(visual.activeSelf, Is.True, "Ghost must remain visible until its timer expires");
            Assert.That(elapsed, Is.LessThan(5f), "Apparition timed out");
            yield return null;
        } while (HorrorTestAccess.State(horrorEvent, "IsPlaying"));
        Assert.That(sawFlicker, Is.True);
        Assert.That(sawGhost, Is.True);
        Assert.That(Time.time - began, Is.GreaterThanOrEqualTo(0.5f - 0.001f));
        AssertHidden();
        Assert.That(light.enabled, Is.EqualTo(initialEnabled));
        Assert.That(light.intensity, Is.EqualTo(initialIntensity).Within(0.0001f));
        Assert.That(light.color, Is.EqualTo(initialColor));
        Assert.That(isolated.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
            .Count(t => t.name == "Ghost_Prototype"), Is.EqualTo(1));
        Assert.That(ghostObject.GetComponent(HorrorTestAccess.Type("GhostActor")), Is.SameAs(ghost));
        HorrorTestAccess.Call(ghost, "Show");
        Assert.That(visual.activeSelf, Is.True);
        HorrorTestAccess.Call(ghost, "Hide");
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator InvalidTimingFinishesSafely()
    {
        foreach (var pair in new[] { new Vector2(-1f, -2f), new Vector2(0f, 0f), new Vector2(0.05f, 0.01f), new Vector2(float.NaN, float.PositiveInfinity) })
        {
            HorrorTestAccess.Call(horrorEvent, "ResetEventForTest");
            var serialized = new SerializedObject(horrorEvent);
            serialized.FindProperty("preDelay").floatValue = -1f;
            serialized.FindProperty("visibleDuration").floatValue = -1f;
            serialized.FindProperty("flickerDuration").floatValue = 0.04f;
            serialized.FindProperty("minFlickerInterval").floatValue = pair.x;
            serialized.FindProperty("maxFlickerInterval").floatValue = pair.y;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            HorrorTestAccess.Call(horrorEvent, "TryPlay");
            yield return HorrorTestAccess.Complete(horrorEvent);
            AssertHidden();
            Assert.That(light.enabled, Is.True);
            serialized.Update();
            Assert.That(serialized.FindProperty("preDelay").floatValue, Is.GreaterThanOrEqualTo(0f));
            Assert.That(serialized.FindProperty("visibleDuration").floatValue, Is.GreaterThanOrEqualTo(0f));
            Assert.That(serialized.FindProperty("minFlickerInterval").floatValue, Is.GreaterThan(0f));
            Assert.That(serialized.FindProperty("maxFlickerInterval").floatValue,
                Is.GreaterThanOrEqualTo(serialized.FindProperty("minFlickerInterval").floatValue));
        }
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator AllNullReferencesAndNonPositiveDurationsAreSafe()
    {
        HorrorTestAccess.Set(horrorEvent, "ghost", null);
        HorrorTestAccess.Set(horrorEvent, "spawnPoint", null);
        HorrorTestAccess.Set(horrorEvent, "targetLight", null);
        HorrorTestAccess.Set(horrorEvent, "apparitionAudio", null);
        foreach (float duration in new[] { 0f, -1f })
        {
            HorrorTestAccess.Call(horrorEvent, "ResetEventForTest");
            Timing(duration, duration, duration);
            HorrorTestAccess.Call(horrorEvent, "TryPlay");
            yield return HorrorTestAccess.Complete(horrorEvent);
            Assert.That(HorrorTestAccess.State(horrorEvent, "IsPlaying"), Is.False);
            Assert.That(HorrorTestAccess.State(horrorEvent, "HasPlayed"), Is.True);
            var serialized = new SerializedObject(horrorEvent);
            foreach (string field in new[] { "preDelay", "flickerDuration", "visibleDuration" })
                Assert.That(serialized.FindProperty(field).floatValue, Is.GreaterThanOrEqualTo(0f));
            AssertHidden();
        }
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator PlayOnceBlocksReplayAndResetAllowsIt()
    {
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(horrorEvent);
        Assert.That(started, Is.EqualTo(1));
        AssertHidden();
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return null;
        Assert.That(started, Is.EqualTo(1));
        AssertHidden();
        HorrorTestAccess.Call(horrorEvent, "ResetEventForTest");
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(horrorEvent);
        Assert.That(started, Is.EqualTo(2));
        AssertHidden();
        LogAssert.NoUnexpectedReceived();
    }
}
