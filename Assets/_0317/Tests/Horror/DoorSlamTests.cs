using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class DoorSlamRuntimeTests
{
    private GameObject root;
    private Transform door;
    private Component horrorEvent;
    private Quaternion openRotation;
    private Quaternion closedRotation;
    private int slamCount;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return new EnterPlayMode();
        slamCount = 0;
        root = new GameObject("HE04 automated test");
        horrorEvent = root.AddComponent(HorrorTestAccess.Type("DoorSlamHorrorEvent"));
        door = new GameObject("Door prototype").transform;
        door.SetParent(root.transform);
        // Nontrivial parent rotation verifies that the slam uses local space.
        root.transform.rotation = Quaternion.Euler(0f, 35f, 0f);
        openRotation = Quaternion.Euler(0f, 90f, 0f);
        closedRotation = Quaternion.Euler(0f, 10f, 0f);
        door.localRotation = openRotation;
        HorrorTestAccess.Set(horrorEvent, "doorTransform", door);
        var serialized = new SerializedObject(horrorEvent);
        serialized.FindProperty("closedLocalEulerAngles").vector3Value = new Vector3(0f, 10f, 0f);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        var callback = (UnityEvent)horrorEvent.GetType().GetField("onDoorSlam",
            BindingFlags.Instance | BindingFlags.NonPublic).GetValue(horrorEvent);
        callback.AddListener(() => slamCount++);
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (root != null) Object.Destroy(root);
        yield return null;
        yield return new ExitPlayMode();
    }

    private void AssertClosed()
    {
        Assert.That(Quaternion.Angle(door.localRotation, closedRotation), Is.LessThan(0.001f));
        Assert.That(door.gameObject.activeInHierarchy, Is.True);
        Assert.That(HorrorTestAccess.State(horrorEvent, "IsPlaying"), Is.False);
    }

    [UnityTest]
    public IEnumerator InheritsBaseAndNullDoorIsSafe()
    {
        Assert.That(horrorEvent.GetType().IsSubclassOf(HorrorTestAccess.Type("HorrorEventTrigger")), Is.True);
        HorrorTestAccess.Set(horrorEvent, "doorTransform", null);
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(horrorEvent);
        Assert.That(slamCount, Is.Zero);
        Assert.That(Quaternion.Angle(door.localRotation, openRotation), Is.LessThan(0.001f));
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator PositiveDurationInterpolatesWithoutAudioOrColliderChanges()
    {
        HorrorTestAccess.Set(horrorEvent, "slamDuration", 0.25f);
        var collider = door.gameObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(1.2f, 2.4f, 0.15f);
        Vector3 initialPosition = door.localPosition;
        Vector3 initialScale = door.localScale;
        float began = Time.time;
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        Assert.That(HorrorTestAccess.State(horrorEvent, "IsPlaying"), Is.True);
        yield return null;
        bool observedIntermediate = false;
        while (HorrorTestAccess.State(horrorEvent, "IsPlaying"))
        {
            float fromOpen = Quaternion.Angle(door.localRotation, openRotation);
            float fromClosed = Quaternion.Angle(door.localRotation, closedRotation);
            if (fromOpen > 0.001f && fromClosed > 0.001f) observedIntermediate = true;
            Assert.That(Time.time - began, Is.LessThan(5f), "Slam timed out");
            yield return null;
        }
        Assert.That(observedIntermediate, Is.True, "Positive duration must interpolate rather than snap");
        Assert.That(Time.time - began, Is.GreaterThanOrEqualTo(0.25f));
        AssertClosed();
        Assert.That(slamCount, Is.EqualTo(1));
        Assert.That(collider.enabled, Is.True);
        Assert.That(collider.isTrigger, Is.False);
        Assert.That(collider.size, Is.EqualTo(new Vector3(1.2f, 2.4f, 0.15f)));
        Assert.That(door.localPosition, Is.EqualTo(initialPosition));
        Assert.That(door.localScale, Is.EqualTo(initialScale));
        yield return null;
        AssertClosed();
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator AssignedAudioWithoutClipDoesNotBlockSlam()
    {
        var audio = root.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        HorrorTestAccess.Set(horrorEvent, "slamAudio", audio);
        Assert.That(new SerializedObject(horrorEvent).FindProperty("slamAudio").objectReferenceValue, Is.EqualTo(audio));
        Assert.That(audio.clip, Is.Null);
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(horrorEvent);
        AssertClosed();
        Assert.That(slamCount, Is.EqualTo(1));
        Assert.That(audio.isPlaying, Is.False);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator NonPositiveDurationSnapsSafely()
    {
        foreach (float duration in new[] { 0f, -1f })
        {
            HorrorTestAccess.Call(horrorEvent, "ResetEventForTest");
            door.localRotation = openRotation;
            HorrorTestAccess.Set(horrorEvent, "slamDuration", duration);
            HorrorTestAccess.Call(horrorEvent, "TryPlay");
            yield return HorrorTestAccess.Complete(horrorEvent);
            AssertClosed();
        }
        Assert.That(slamCount, Is.EqualTo(2));
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator PlayOnceBlocksSecondSlamAndResetAllowsReplay()
    {
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(horrorEvent);
        AssertClosed();
        Assert.That(slamCount, Is.EqualTo(1));
        door.localRotation = openRotation;
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return null;
        Assert.That(slamCount, Is.EqualTo(1));
        Assert.That(Quaternion.Angle(door.localRotation, openRotation), Is.LessThan(0.001f));
        HorrorTestAccess.Call(horrorEvent, "ResetEventForTest");
        HorrorTestAccess.Call(horrorEvent, "TryPlay");
        yield return HorrorTestAccess.Complete(horrorEvent);
        AssertClosed();
        Assert.That(slamCount, Is.EqualTo(2));
        LogAssert.NoUnexpectedReceived();
    }
}

public class DoorSlamDependencyTests
{
    [Test]
    public void ProductionHasNoDoorSystemOrKeyDependency()
    {
        string source = System.IO.File.ReadAllText("Assets/_0317/Scripts/Horror/DoorSlamHorrorEvent.cs");
        foreach (string forbidden in new[] { "DoorSystem", "IInteractable", "FpsHorrorKit", "StoryStage", "Game0317Manager", "Inventory" })
            Assert.That(source, Does.Not.Contain(forbidden));
        var type = HorrorTestAccess.Type("DoorSlamHorrorEvent");
        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
        {
            Assert.That(field.FieldType.Assembly.GetName().Name == "Assembly-CSharp"
                && field.FieldType != type, Is.False, "Unexpected gameplay dependency: " + field.Name);
        }
    }
}
