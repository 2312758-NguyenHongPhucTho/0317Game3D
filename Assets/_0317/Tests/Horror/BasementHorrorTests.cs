using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class BasementHorrorRuntimeTests
{
    private GameObject root;
    private Scene isolated;
    private Component horrorEvent;
    private Component ghost;
    private GameObject ghostObject;
    private GameObject visual;
    private Transform firstSpawn;
    private Transform secondSpawn;
    private Light light;
    private AudioSource audio;
    private int started;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return new EnterPlayMode();
        started = 0;
        isolated = SceneManager.CreateScene("HE06 isolated test");
        root = new GameObject("HE06 test");
        SceneManager.MoveGameObjectToScene(root, isolated);
        horrorEvent = root.AddComponent(HorrorTestAccess.Type("BasementHorrorEvent"));
        ghostObject = new GameObject("Ghost_Prototype");
        ghostObject.transform.SetParent(root.transform);
        ghost = ghostObject.AddComponent(HorrorTestAccess.Type("GhostActor"));
        visual = new GameObject("Visual");
        visual.transform.SetParent(ghostObject.transform);
        HorrorTestAccess.Set(ghost, "visualRoot", visual);
        HorrorTestAccess.Call(ghost, "Hide");
        firstSpawn = new GameObject("First spawn").transform;
        firstSpawn.SetParent(root.transform);
        firstSpawn.position = new Vector3(0f, 1.3f, 5f);
        firstSpawn.rotation = Quaternion.Euler(0f, 160f, 0f);
        secondSpawn = new GameObject("Second spawn").transform;
        secondSpawn.SetParent(root.transform);
        secondSpawn.position = new Vector3(1f, 1.3f, 1f);
        secondSpawn.rotation = Quaternion.Euler(0f, 200f, 0f);
        light = root.AddComponent<Light>();
        root.AddComponent(HorrorTestAccess.Type("UnityEngine.Rendering.HighDefinition.HDAdditionalLightData"));
        light.color = Color.cyan;
        audio = root.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.loop = false;
        audio.spatialBlend = 1f;
        HorrorTestAccess.Set(horrorEvent, "ghost", ghost);
        HorrorTestAccess.Set(horrorEvent, "firstSpawnPoint", firstSpawn);
        HorrorTestAccess.Set(horrorEvent, "secondSpawnPoint", secondSpawn);
        HorrorTestAccess.Set(horrorEvent, "targetLight", light);
        HorrorTestAccess.Set(horrorEvent, "basementAudio", audio);
        Timing(0f, 0.1f, 0.1f, 0.1f);
        HorrorTestAccess.Callback(horrorEvent, "onEventStarted").AddListener(() => started++);
        yield return null;
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

    private void Timing(float delay, float first, float blackout, float second)
    {
        HorrorTestAccess.Set(horrorEvent, "preDelay", delay);
        HorrorTestAccess.Set(horrorEvent, "firstVisibleDuration", first);
        HorrorTestAccess.Set(horrorEvent, "blackoutDuration", blackout);
        HorrorTestAccess.Set(horrorEvent, "secondVisibleDuration", second);
    }

    private void AssertAt(Transform spawn)
    {
        Assert.That(visual.activeSelf, Is.True);
        Assert.That(HorrorTestAccess.State(ghost, "IsVisible"), Is.True);
        Assert.That(Vector3.Distance(ghostObject.transform.position, spawn.position), Is.LessThan(0.0001f));
        Assert.That(Quaternion.Angle(ghostObject.transform.rotation, spawn.rotation), Is.LessThan(0.001f));
    }

    private void AssertFinal()
    {
        Assert.That(visual.activeSelf, Is.False);
        Assert.That(HorrorTestAccess.State(ghost, "IsVisible"), Is.False);
        Assert.That(ghostObject.activeInHierarchy, Is.True);
        Assert.That(((Behaviour)ghost).enabled, Is.True);
        Assert.That(ghostObject.GetComponent(HorrorTestAccess.Type("GhostActor")), Is.SameAs(ghost));
        Assert.That(isolated.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
            .Count(t => t.name == "Ghost_Prototype"), Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator InheritsBaseAndNullGhostStillHandlesLightAndAudio()
    {
        Assert.That(horrorEvent.GetType().IsSubclassOf(HorrorTestAccess.Type("HorrorEventTrigger")), Is.True);
        HorrorTestAccess.Set(horrorEvent, "ghost", null);
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(horrorEvent);
        AssertFinal();
        Assert.That(light.enabled, Is.True);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator MissingFirstSpawnOnlySkipsFirstApparition()
    {
        HorrorTestAccess.Set(horrorEvent, "firstSpawnPoint", null);
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        bool sawSecond = false;
        float deadline = Time.realtimeSinceStartup + 5f;
        do
        {
            if (visual.activeSelf) { AssertAt(secondSpawn); sawSecond = true; }
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline));
            yield return null;
        } while (HorrorTestAccess.State(horrorEvent, "IsPlaying"));
        Assert.That(sawSecond, Is.True);
        AssertFinal();
        Assert.That(light.enabled, Is.True);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator MissingSecondSpawnOnlySkipsSecondApparition()
    {
        HorrorTestAccess.Set(horrorEvent, "secondSpawnPoint", null);
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        bool sawFirst = false;
        float deadline = Time.realtimeSinceStartup + 5f;
        do
        {
            if (visual.activeSelf) { AssertAt(firstSpawn); sawFirst = true; }
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline));
            yield return null;
        } while (HorrorTestAccess.State(horrorEvent, "IsPlaying"));
        Assert.That(sawFirst, Is.True);
        AssertFinal();
        Assert.That(light.enabled, Is.True);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator NullLightAndAudioStillCompleteBothApparitions()
    {
        HorrorTestAccess.Set(horrorEvent, "targetLight", null);
        HorrorTestAccess.Set(horrorEvent, "basementAudio", null);
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        bool sawFirst = false, sawSecond = false;
        float deadline = Time.realtimeSinceStartup + 5f;
        do
        {
            if (visual.activeSelf)
            {
                if (Vector3.Distance(ghostObject.transform.position, firstSpawn.position) < 0.0001f)
                { AssertAt(firstSpawn); sawFirst = true; }
                else { AssertAt(secondSpawn); sawSecond = true; }
            }
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline));
            yield return null;
        } while (HorrorTestAccess.State(horrorEvent, "IsPlaying"));
        Assert.That(sawFirst && sawSecond, Is.True);
        AssertFinal();
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator SequenceRestoresInitiallyEnabledLight()
    {
        yield return CheckSequence(true);
    }

    [UnityTest]
    public IEnumerator SequenceRestoresInitiallyDisabledLight()
    {
        yield return CheckSequence(false);
    }

    private IEnumerator CheckSequence(bool enabled)
    {
        light.enabled = enabled;
        Timing(0.15f, 0.2f, 0.15f, 0.2f);
        yield return null;
        float initialIntensity = light.intensity;
        Color initialColor = light.color;
        bool initialEnabled = light.enabled;
        Assert.That(audio.clip, Is.Null);
        Assert.That(new SerializedObject(horrorEvent).FindProperty("basementAudio").objectReferenceValue, Is.EqualTo(audio));
        float began = Time.time;
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        bool sawFirst = false, sawBlackout = false, sawSecond = false;
        do
        {
            float elapsed = Time.time - began;
            if (elapsed < 0.15f)
            {
                Assert.That(visual.activeSelf, Is.False);
                Assert.That(light.enabled, Is.EqualTo(initialEnabled));
            }
            if (visual.activeSelf)
            {
                if (Vector3.Distance(ghostObject.transform.position, firstSpawn.position) < 0.0001f)
                {
                    AssertAt(firstSpawn);
                    Assert.That(sawSecond, Is.False);
                    Assert.That(elapsed, Is.GreaterThanOrEqualTo(0.15f - 0.001f));
                    Assert.That(light.enabled, Is.EqualTo(initialEnabled));
                    sawFirst = true;
                }
                else
                {
                    AssertAt(secondSpawn);
                    Assert.That(sawFirst && sawBlackout, Is.True);
                    Assert.That(elapsed, Is.GreaterThanOrEqualTo(0.5f - 0.001f));
                    Assert.That(light.enabled, Is.EqualTo(initialEnabled));
                    sawSecond = true;
                }
            }
            else if (sawFirst && !sawSecond)
            {
                Assert.That(elapsed, Is.GreaterThanOrEqualTo(0.35f - 0.001f));
                Assert.That(HorrorTestAccess.State(ghost, "IsVisible"), Is.False);
                Assert.That(light.enabled, Is.False);
                sawBlackout = true;
            }
            Assert.That(elapsed, Is.LessThan(5f), "Basement sequence timed out");
            yield return null;
        } while (HorrorTestAccess.State(horrorEvent, "IsPlaying"));
        Assert.That(sawFirst && sawBlackout && sawSecond, Is.True);
        Assert.That(Time.time - began, Is.GreaterThanOrEqualTo(0.7f - 0.001f));
        AssertFinal();
        Assert.That(light.enabled, Is.EqualTo(initialEnabled));
        Assert.That(light.intensity, Is.EqualTo(initialIntensity).Within(0.0001f));
        Assert.That(light.color, Is.EqualTo(initialColor));
        Assert.That(audio.isPlaying, Is.False);
        HorrorTestAccess.Call(ghost, "Show");
        Assert.That(visual.activeSelf, Is.True);
        HorrorTestAccess.Call(ghost, "Hide");
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator NonPositiveDurationsFinishSafely()
    {
        foreach (float value in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
        {
            HorrorTestAccess.Call(horrorEvent, "ResetEventForTest");
            Timing(value, value, value, value);
            HorrorTestAccess.Call(horrorEvent, "TryPlay");
            yield return HorrorTestAccess.Complete(horrorEvent);
            AssertFinal();
            Assert.That(light.enabled, Is.True);
            var serialized = new SerializedObject(horrorEvent);
            foreach (string field in new[] { "preDelay", "firstVisibleDuration", "blackoutDuration", "secondVisibleDuration" })
                Assert.That(serialized.FindProperty(field).floatValue, Is.GreaterThanOrEqualTo(0f));
        }
        HorrorTestAccess.Call(horrorEvent, "ResetEventForTest");
        HorrorTestAccess.Set(horrorEvent, "firstSpawnPoint", null);
        HorrorTestAccess.Set(horrorEvent, "secondSpawnPoint", null);
        HorrorTestAccess.Call(ghost, "Show");
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(horrorEvent);
        AssertFinal();
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator PlayOnceBlocksReplayAndResetAllowsIt()
    {
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(horrorEvent);
        Assert.That(started, Is.EqualTo(1));
        AssertFinal();
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return null;
        Assert.That(started, Is.EqualTo(1));
        AssertFinal();
        HorrorTestAccess.Call(horrorEvent, "ResetEventForTest");
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(horrorEvent);
        Assert.That(started, Is.EqualTo(2));
        AssertFinal();
        LogAssert.NoUnexpectedReceived();
    }
}
