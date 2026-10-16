using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

// Runtime scripts remain in Assembly-CSharp. Resolve them by name because an
// asmdef cannot reference that predefined assembly without moving runtime code.
internal static class HorrorTestAccess
{
    public static Type Type(string name)
    {
        return AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name))
            .First(t => t != null);
    }

    public static void Call(Component component, string method)
    {
        component.GetType().GetMethod(method).Invoke(component, null);
    }

    public static bool State(Component component, string property)
    {
        return (bool)component.GetType().GetProperty(property).GetValue(component);
    }

    public static void Set(Component component, string field, object value)
    {
        var serialized = new SerializedObject(component);
        var property = serialized.FindProperty(field);
        Assert.That(property, Is.Not.Null, field);
        if (value is bool) property.boolValue = (bool)value;
        else if (value is float) property.floatValue = (float)value;
        else property.objectReferenceValue = (Object)value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    public static UnityEvent Callback(Component component, string field)
    {
        Assert.That(component, Is.Not.Null, "Component must not be null");

        FieldInfo fieldInfo = null;
        for (var type = component.GetType(); type != null; type = type.BaseType)
        {
            fieldInfo = type.GetField(
                field,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.DeclaredOnly);
            if (fieldInfo != null)
                break;
        }
        Assert.That(
            fieldInfo,
            Is.Not.Null,
            $"Field '{field}' not found on {component.GetType().Name}");
        Assert.That(
            typeof(UnityEvent).IsAssignableFrom(fieldInfo.FieldType),
            Is.True,
            $"Field '{field}' on {component.GetType().Name} is not a UnityEvent");

        var callback = fieldInfo.GetValue(component) as UnityEvent;
        Assert.That(
            callback,
            Is.Not.Null,
            $"Field '{field}' on {component.GetType().Name} is null");
        return callback;
    }

    public static IEnumerator Complete(Component component)
    {
        const float completionTimeoutSeconds = 6f;
        float deadline = Time.realtimeSinceStartup + completionTimeoutSeconds;
        do
        {
            yield return null;
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), "Sequence timed out");
        } while (State(component, "IsPlaying"));
    }
}

public class HorrorEventRuntimeTests
{
    private GameObject root;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return new EnterPlayMode();
        root = new GameObject("Horror automated test");
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (root != null) Object.Destroy(root);
        yield return null;
        yield return new ExitPlayMode();
    }

    private Component Add(string name)
    {
        return root.AddComponent(HorrorTestAccess.Type(name));
    }

    [UnityTest]
    public IEnumerator ArmDisarmAndStates()
    {
        var trigger = Add("HorrorEventTrigger");
        Assert.That(HorrorTestAccess.State(trigger, "HasPlayed"), Is.False);
        Assert.That(HorrorTestAccess.State(trigger, "IsPlaying"), Is.False);
        HorrorTestAccess.Call(trigger, "Disarm");
        Assert.That(new SerializedObject(trigger).FindProperty("armed").boolValue, Is.False);
        HorrorTestAccess.Call(trigger, "TryPlay");
        Assert.That(HorrorTestAccess.State(trigger, "HasPlayed"), Is.False);
        Assert.That(HorrorTestAccess.State(trigger, "IsPlaying"), Is.False);
        HorrorTestAccess.Call(trigger, "Arm");
        Assert.That(new SerializedObject(trigger).FindProperty("armed").boolValue, Is.True);
        HorrorTestAccess.Call(trigger, "TryPlay");
        Assert.That(HorrorTestAccess.State(trigger, "HasPlayed"), Is.True);
        yield return HorrorTestAccess.Complete(trigger);
        Assert.That(HorrorTestAccess.State(trigger, "IsPlaying"), Is.False);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator PlayOnceAndReset()
    {
        var trigger = Add("HorrorEventTrigger");
        int started = 0, completed = 0;
        HorrorTestAccess.Callback(trigger, "onEventStarted").AddListener(() => started++);
        HorrorTestAccess.Callback(trigger, "onEventCompleted").AddListener(() => completed++);
        HorrorTestAccess.Call(trigger, "TryPlay");
        yield return HorrorTestAccess.Complete(trigger);
        HorrorTestAccess.Call(trigger, "TryPlay");
        yield return null;
        Assert.That(started, Is.EqualTo(1));
        Assert.That(completed, Is.EqualTo(1));
        HorrorTestAccess.Call(trigger, "ResetEventForTest");
        Assert.That(HorrorTestAccess.State(trigger, "HasPlayed"), Is.False);
        Assert.That(HorrorTestAccess.State(trigger, "IsPlaying"), Is.False);
        HorrorTestAccess.Call(trigger, "TryPlay");
        yield return HorrorTestAccess.Complete(trigger);
        Assert.That(started, Is.EqualTo(2));
        Assert.That(completed, Is.EqualTo(2));
    }

    [UnityTest]
    public IEnumerator FootstepWithoutClipIsSafeAndCannotReplay()
    {
        var trigger = Add("FootstepHorrorEvent");
        Assert.That(trigger.GetType().IsSubclassOf(HorrorTestAccess.Type("HorrorEventTrigger")), Is.True);
        var audio = root.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        HorrorTestAccess.Set(trigger, "footstepAudio", audio);
        Assert.That(new SerializedObject(trigger).FindProperty("footstepAudio").objectReferenceValue, Is.EqualTo(audio));
        Assert.That(audio.clip, Is.Null);
        int started = 0;
        HorrorTestAccess.Callback(trigger, "onEventStarted").AddListener(() => started++);
        HorrorTestAccess.Call(trigger, "TryPlay");
        yield return HorrorTestAccess.Complete(trigger);
        HorrorTestAccess.Call(trigger, "TryPlay");
        yield return null;
        Assert.That(started, Is.EqualTo(1));
        Assert.That(HorrorTestAccess.State(trigger, "HasPlayed"), Is.True);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator NullLightIsSafe()
    {
        var trigger = Add("LightFlickerHorrorEvent");
        Assert.That(trigger.GetType().IsSubclassOf(HorrorTestAccess.Type("HorrorEventTrigger")), Is.True);
        HorrorTestAccess.Call(trigger, "TryPlay");
        yield return HorrorTestAccess.Complete(trigger);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator RestoresInitiallyEnabledLight()
    {
        yield return CheckFlicker(true);
    }

    [UnityTest]
    public IEnumerator RestoresInitiallyDisabledLight()
    {
        yield return CheckFlicker(false);
    }

    private IEnumerator CheckFlicker(bool initial)
    {
        var trigger = Add("LightFlickerHorrorEvent");
        var light = root.AddComponent<Light>();
        root.AddComponent(HorrorTestAccess.Type("UnityEngine.Rendering.HighDefinition.HDAdditionalLightData"));
        light.enabled = initial;
        light.intensity = 1.7f;
        light.color = Color.yellow;
        HorrorTestAccess.Set(trigger, "targetLight", light);
        HorrorTestAccess.Set(trigger, "duration", 0.2f);
        HorrorTestAccess.Set(trigger, "minInterval", 0.05f);
        HorrorTestAccess.Set(trigger, "maxInterval", 0.05f);
        int started = 0, completed = 0;
        HorrorTestAccess.Callback(trigger, "onEventStarted").AddListener(() => started++);
        HorrorTestAccess.Callback(trigger, "onEventCompleted").AddListener(() => completed++);
        // Allow HDRP to synchronize its light units before capturing the baseline.
        yield return null;
        float initialIntensity = light.intensity;
        Color initialColor = light.color;
        bool initialEnabled = light.enabled;
        HorrorTestAccess.Call(trigger, "TryPlay");
        // A coroutine starts synchronously up to its first yield.
        yield return null;
        Assert.That(HorrorTestAccess.State(trigger, "IsPlaying"), Is.True);
        HorrorTestAccess.Call(trigger, "ResetEventForTest");
        Assert.That(HorrorTestAccess.State(trigger, "HasPlayed"), Is.True);
        HorrorTestAccess.Call(trigger, "TryPlay");
        Assert.That(started, Is.EqualTo(1));
        yield return HorrorTestAccess.Complete(trigger);
        Assert.That(light.enabled, Is.EqualTo(initialEnabled));
        Assert.That(light.intensity, Is.EqualTo(initialIntensity).Within(0.0001f));
        Assert.That(light.color, Is.EqualTo(initialColor));
        Assert.That(completed, Is.EqualTo(1));
        HorrorTestAccess.Call(trigger, "TryPlay");
        yield return null;
        Assert.That(started, Is.EqualTo(1));
        // Repeatable events must also reject attempts while already running.
        HorrorTestAccess.Call(trigger, "ResetEventForTest");
        HorrorTestAccess.Set(trigger, "playOnce", false);
        HorrorTestAccess.Call(trigger, "TryPlay");
        yield return null;
        HorrorTestAccess.Call(trigger, "TryPlay");
        Assert.That(started, Is.EqualTo(2));
        yield return HorrorTestAccess.Complete(trigger);
        Assert.That(completed, Is.EqualTo(2));
        Assert.That(light.enabled, Is.EqualTo(initialEnabled));
        Assert.That(light.intensity, Is.EqualTo(initialIntensity).Within(0.0001f));
        Assert.That(light.color, Is.EqualTo(initialColor));
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator InvalidIntervalsAreClampedSafely()
    {
        var trigger = Add("LightFlickerHorrorEvent");
        var light = root.AddComponent<Light>();
        HorrorTestAccess.Set(trigger, "targetLight", light);
        foreach (var pair in new[] { new Vector2(-1f, -2f), new Vector2(0.05f, 0.01f), new Vector2(0f, 0f), new Vector2(float.NaN, float.PositiveInfinity) })
        {
            HorrorTestAccess.Call(trigger, "ResetEventForTest");
            var serialized = new SerializedObject(trigger);
            serialized.FindProperty("duration").floatValue = 0.08f;
            serialized.FindProperty("minInterval").floatValue = pair.x;
            serialized.FindProperty("maxInterval").floatValue = pair.y;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            HorrorTestAccess.Call(trigger, "TryPlay");
            yield return HorrorTestAccess.Complete(trigger);
            serialized.Update();
            Assert.That(serialized.FindProperty("minInterval").floatValue, Is.GreaterThan(0f));
            Assert.That(serialized.FindProperty("maxInterval").floatValue,
                Is.GreaterThanOrEqualTo(serialized.FindProperty("minInterval").floatValue));
            Assert.That(light.enabled, Is.True);
        }
        LogAssert.NoUnexpectedReceived();
    }
}

public class HorrorFlickerStepTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void SequenceTogglesLightBeforeFirstWait(bool initial)
    {
        var go = new GameObject("Flicker step test");
        IEnumerator sequence = null;
        try
        {
            var light = go.AddComponent<Light>();
            light.enabled = initial;
            var component = go.AddComponent(HorrorTestAccess.Type("LightFlickerHorrorEvent"));
            HorrorTestAccess.Set(component, "targetLight", light);
            sequence = (IEnumerator)component.GetType().GetMethod("PlaySequence",
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(component, null);
            Assert.That(sequence.MoveNext(), Is.True);
            Assert.That(light.enabled, Is.EqualTo(!initial));
            Assert.That(sequence.Current, Is.InstanceOf<WaitForSeconds>());
        }
        finally
        {
            if (sequence is IDisposable) ((IDisposable)sequence).Dispose();
            Object.DestroyImmediate(go);
        }
    }
}
