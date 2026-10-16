using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class GhostChaseRuntimeTests
{
    private GameObject root;
    private Component controller;
    private Component ghost;
    private GameObject ghostObject;
    private GameObject visual;
    private Transform[] points;
    private int started;
    private int ended;

    private IEnumerator CompleteChase()
    {
        float deadline = Time.realtimeSinceStartup + 5f;
        while (HorrorTestAccess.State(controller, "IsChasing"))
        {
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), "Chase timed out");
            yield return null;
        }
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return new EnterPlayMode();
        started = 0;
        ended = 0;
        root = new GameObject("Chase test");
        controller = root.AddComponent<HorrorTestAccessMarker>().gameObject.AddComponent(HorrorTestAccess.Type("GhostChaseController"));
        Object.DestroyImmediate(root.GetComponent<HorrorTestAccessMarker>());
        ghostObject = new GameObject("Ghost_Prototype");
        ghostObject.transform.SetParent(root.transform);
        ghost = ghostObject.AddComponent(HorrorTestAccess.Type("GhostActor"));
        visual = new GameObject("Visual");
        visual.transform.SetParent(ghostObject.transform);
        HorrorTestAccess.Set(ghost, "visualRoot", visual);
        HorrorTestAccess.Call(ghost, "Hide");
        points = Enumerable.Range(1, 4).Select(i =>
        {
            var point = new GameObject("Waypoint_" + i).transform;
            point.SetParent(root.transform);
            point.position = new Vector3(i, 0f, 0f);
            return point;
        }).ToArray();
        HorrorTestAccess.Set(controller, "ghost", ghost);
        var serialized = new SerializedObject(controller);
        var array = serialized.FindProperty("waypoints");
        array.arraySize = points.Length;
        for (int i = 0; i < points.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
        serialized.FindProperty("moveSpeed").floatValue = 10f;
        serialized.FindProperty("rotationSpeed").floatValue = 720f;
        serialized.FindProperty("waypointReachDistance").floatValue = 0.05f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        HorrorTestAccess.Callback(controller, "onChaseStarted").AddListener(() => started++);
        HorrorTestAccess.Callback(controller, "onChaseEnded").AddListener(() => ended++);
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (root != null) Object.Destroy(root);
        yield return null;
        yield return new ExitPlayMode();
    }

    [UnityTest]
    public IEnumerator StartsAtFirstWaypointMovesSkipsNullAndEnds()
    {
        HorrorTestAccess.Call(controller, "StartChase");
        Assert.That(HorrorTestAccess.State(controller, "IsChasing"), Is.True);
        Assert.That(started, Is.EqualTo(1));
        Assert.That(ghostObject.transform.position, Is.EqualTo(points[0].position));
        var serialized = new SerializedObject(controller);
        serialized.FindProperty("waypoints").GetArrayElementAtIndex(1).objectReferenceValue = null;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        yield return CompleteChase();
        Assert.That(ended, Is.EqualTo(1));
        Assert.That(HorrorTestAccess.State(controller, "IsChasing"), Is.False);
        Assert.That(visual.activeSelf, Is.False);
        Assert.That(ghostObject.activeSelf, Is.True);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator DuplicateStartIsIgnoredAndStopHides()
    {
        HorrorTestAccess.Call(controller, "StartChase");
        HorrorTestAccess.Call(controller, "StartChase");
        yield return null;
        Assert.That(started, Is.EqualTo(1));
        HorrorTestAccess.Call(controller, "StopChase");
        Assert.That(HorrorTestAccess.State(controller, "IsChasing"), Is.False);
        Assert.That(ended, Is.EqualTo(1));
        Assert.That(visual.activeSelf, Is.False);
        HorrorTestAccess.Call(controller, "StopChase");
        Assert.That(ended, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator NullGhostAndEmptyWaypointsAreSafe()
    {
        HorrorTestAccess.Set(controller, "ghost", null);
        HorrorTestAccess.Call(controller, "StartChase");
        yield return null;
        Assert.That(HorrorTestAccess.State(controller, "IsChasing"), Is.False);
        HorrorTestAccess.Set(controller, "ghost", ghost);
        var serialized = new SerializedObject(controller);
        serialized.FindProperty("waypoints").arraySize = 0;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        HorrorTestAccess.Call(controller, "StartChase");
        yield return null;
        Assert.That(HorrorTestAccess.State(controller, "IsChasing"), Is.False);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator ResetReturnsIndexAndAllowsReuse()
    {
        HorrorTestAccess.Call(controller, "StartChase");
        yield return null;
        HorrorTestAccess.Call(controller, "ResetChaseForTest");
        Assert.That(HorrorTestAccess.State(controller, "IsChasing"), Is.False);
        Assert.That((int)controller.GetType().GetProperty("CurrentWaypointIndex").GetValue(controller), Is.EqualTo(0));
        HorrorTestAccess.Call(ghost, "Show");
        Assert.That(visual.activeSelf, Is.True);
        HorrorTestAccess.Call(ghost, "Hide");
    }

    [UnityTest]
    public IEnumerator NullWaypointArrayIsSafe()
    {
        var nullWaypoints = new SerializedObject(controller);
        nullWaypoints.FindProperty("waypoints").arraySize = 0;
        nullWaypoints.ApplyModifiedPropertiesWithoutUndo();
        HorrorTestAccess.Call(controller, "StartChase");
        yield return null;
        Assert.That(HorrorTestAccess.State(controller, "IsChasing"), Is.False);
        LogAssert.NoUnexpectedReceived();
    }
}

// Gives tests a runtime Component attach point without changing production code.
public sealed class HorrorTestAccessMarker : MonoBehaviour { }

public class FinalChaseHorrorEventRuntimeTests
{
    private GameObject root;
    private Component finalEvent;
    private Component controller;
    private Component ghost;
    private GameObject visual;
    private Transform spawn;
    private int started;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return new EnterPlayMode();
        root = new GameObject("Final chase event test");
        finalEvent = root.AddComponent(HorrorTestAccess.Type("FinalChaseHorrorEvent"));
        controller = root.AddComponent(HorrorTestAccess.Type("GhostChaseController"));
        var ghostObject = new GameObject("Ghost_Prototype");
        ghostObject.transform.SetParent(root.transform);
        ghost = ghostObject.AddComponent(HorrorTestAccess.Type("GhostActor"));
        visual = new GameObject("Visual");
        visual.transform.SetParent(ghostObject.transform);
        HorrorTestAccess.Set(ghost, "visualRoot", visual);
        spawn = new GameObject("Apparition").transform;
        spawn.SetParent(root.transform);
        spawn.position = new Vector3(0f, 0f, -1f);
        var waypoint = new GameObject("Route waypoint").transform;
        waypoint.SetParent(root.transform);
        waypoint.position = spawn.position;
        HorrorTestAccess.Set(controller, "ghost", ghost);
        var controllerSerialized = new SerializedObject(controller);
        controllerSerialized.FindProperty("waypoints").arraySize = 1;
        controllerSerialized.FindProperty("waypoints").GetArrayElementAtIndex(0).objectReferenceValue = waypoint;
        controllerSerialized.FindProperty("moveSpeed").floatValue = 10f;
        controllerSerialized.ApplyModifiedPropertiesWithoutUndo();
        HorrorTestAccess.Set(finalEvent, "chaseController", controller);
        HorrorTestAccess.Set(finalEvent, "ghost", ghost);
        HorrorTestAccess.Set(finalEvent, "apparitionSpawnPoint", spawn);
        HorrorTestAccess.Set(finalEvent, "apparitionDuration", 0.5f);
        HorrorTestAccess.Callback(controller, "onChaseStarted").AddListener(() => started++);
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (root != null) Object.Destroy(root);
        yield return null;
        yield return new ExitPlayMode();
    }

    [UnityTest]
    public IEnumerator ShowsThenStartsChaseAndPlayOnceBlocksReplay()
    {
        Assert.That(finalEvent.GetType().IsSubclassOf(HorrorTestAccess.Type("HorrorEventTrigger")), Is.True);
        HorrorTestAccess.Call(finalEvent, "TryPlay");
        yield return null;
        Assert.That(visual.activeSelf, Is.True);
        Assert.That((Vector3)root.transform.Find("Ghost_Prototype").position, Is.EqualTo(spawn.position));
        Assert.That(started, Is.EqualTo(0));
        yield return HorrorTestAccess.Complete(finalEvent);
        Assert.That(visual.activeSelf, Is.False);
        Assert.That(started, Is.EqualTo(1));
        HorrorTestAccess.Call(finalEvent, "TryPlay");
        yield return null;
        Assert.That(started, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator NullControllerAndNullApparitionAreSafe()
    {
        HorrorTestAccess.Set(finalEvent, "chaseController", null);
        HorrorTestAccess.Set(finalEvent, "ghost", null);
        HorrorTestAccess.Set(finalEvent, "apparitionSpawnPoint", null);
        HorrorTestAccess.Call(finalEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(finalEvent);
        Assert.That(started, Is.EqualTo(0));
        LogAssert.NoUnexpectedReceived();
    }
}
