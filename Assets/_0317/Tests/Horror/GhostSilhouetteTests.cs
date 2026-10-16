using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class GhostSilhouetteRuntimeTests
{
    private GameObject root;
    private GameObject ghostObject;
    private GameObject visual;
    private Component ghost;
    private Component horrorEvent;
    private Transform spawn;
    private int started;
    private Scene isolated;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return new EnterPlayMode();
        started = 0;
        isolated = default(Scene);
        root = new GameObject("HE03 automated test");
        horrorEvent = root.AddComponent(HorrorTestAccess.Type("GhostSilhouetteHorrorEvent"));
        ghostObject = new GameObject("Ghost_Prototype");
        ghostObject.transform.SetParent(root.transform);
        ghost = ghostObject.AddComponent(HorrorTestAccess.Type("GhostActor"));
        visual = new GameObject("Visual");
        visual.transform.SetParent(ghostObject.transform);
        HorrorTestAccess.Set(ghost, "visualRoot", visual);
        HorrorTestAccess.Call(ghost, "Hide");
        spawn = new GameObject("Spawn").transform;
        spawn.SetParent(root.transform);
        spawn.position = new Vector3(2f, 1f, 7f);
        spawn.rotation = Quaternion.Euler(0f, 125f, 0f);
        HorrorTestAccess.Set(horrorEvent, "ghost", ghost);
        HorrorTestAccess.Set(horrorEvent, "spawnPoint", spawn);
        HorrorTestAccess.Callback(horrorEvent, "onEventStarted").AddListener(() => started++);
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

    private void AssertHiddenAndUsable()
    {
        Assert.That(visual.activeSelf, Is.False);
        Assert.That(HorrorTestAccess.State(ghost, "IsVisible"), Is.False);
        Assert.That(ghostObject.activeInHierarchy, Is.True);
        Assert.That(ghostObject.GetComponent(HorrorTestAccess.Type("GhostActor")), Is.SameAs(ghost));
        Assert.That(((Behaviour)ghost).enabled, Is.True);
        HorrorTestAccess.Call(ghost, "Show");
        Assert.That(visual.activeSelf, Is.True);
        Assert.That(HorrorTestAccess.State(ghost, "IsVisible"), Is.True);
        HorrorTestAccess.Call(ghost, "Hide");
    }

    [UnityTest]
    public IEnumerator InheritsBaseAndNullGhostIsSafe()
    {
        Assert.That(horrorEvent.GetType().IsSubclassOf(HorrorTestAccess.Type("HorrorEventTrigger")), Is.True);
        HorrorTestAccess.Set(horrorEvent, "ghost", null);
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(horrorEvent);
        Assert.That(visual.activeSelf, Is.False);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator NullSpawnPointIsSafe()
    {
        HorrorTestAccess.Set(horrorEvent, "spawnPoint", null);
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(horrorEvent);
        Assert.That(visual.activeSelf, Is.False);
        LogAssert.NoUnexpectedReceived();
    }

    private int GhostCount()
    {
        return root.scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
            .Count(t => t.name == "Ghost_Prototype");
    }

    [UnityTest]
    public IEnumerator ShowsAtSpawnWaitsHidesAndReusesWithoutDuplicates()
    {
        // An isolated additive scene makes the exact ghost count independent of
        // whichever scene the developer had open before running tests.
        isolated = SceneManager.CreateScene("HE03 isolated ghost test");
        SceneManager.MoveGameObjectToScene(root, isolated);
        Assert.That(GhostCount(), Is.EqualTo(1));
        HorrorTestAccess.Set(horrorEvent, "visibleDuration", 0.25f);
        float began = Time.time;
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return null;
        Assert.That(HorrorTestAccess.State(horrorEvent, "IsPlaying"), Is.True);
        Assert.That(visual.activeSelf, Is.True);
        Assert.That(HorrorTestAccess.State(ghost, "IsVisible"), Is.True);
        Assert.That(Vector3.Distance(ghostObject.transform.position, spawn.position), Is.LessThan(0.0001f));
        Assert.That(Quaternion.Angle(ghostObject.transform.rotation, spawn.rotation), Is.LessThan(0.001f));
        Vector3 shownPosition = ghostObject.transform.position;
        Quaternion shownRotation = ghostObject.transform.rotation;
        // Verify another attempt during the sequence does not start a duplicate.
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        Assert.That(started, Is.EqualTo(1));
        yield return HorrorTestAccess.Complete(horrorEvent);
        Assert.That(Time.time - began, Is.GreaterThanOrEqualTo(0.25f));
        Assert.That(ghostObject.transform.position, Is.EqualTo(shownPosition));
        Assert.That(Quaternion.Angle(ghostObject.transform.rotation, shownRotation), Is.LessThan(0.001f));
        AssertHiddenAndUsable();
        Assert.That(GhostCount(), Is.EqualTo(1));
        Assert.That(HorrorTestAccess.State(horrorEvent, "HasPlayed"), Is.True);
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return null;
        Assert.That(started, Is.EqualTo(1));
        Assert.That(visual.activeSelf, Is.False);
        Assert.That(GhostCount(), Is.EqualTo(1));
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator ZeroAndNegativeDurationsFinishHiddenSafely()
    {
        foreach (float duration in new[] { 0f, -1f })
        {
            HorrorTestAccess.Call(horrorEvent, "ResetEventForTest");
            HorrorTestAccess.Set(horrorEvent, "visibleDuration", duration);
            HorrorTestAccess.Call(horrorEvent, "TryPlay");
            yield return HorrorTestAccess.Complete(horrorEvent);
            Assert.That(HorrorTestAccess.State(horrorEvent, "IsPlaying"), Is.False);
            AssertHiddenAndUsable();
        }
        LogAssert.NoUnexpectedReceived();
    }
}
