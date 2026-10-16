using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HorrorSceneTests
{
    private Scene scene;
    private bool openedByTest;

    [SetUp]
    public void LoadScene()
    {
        const string path = "Assets/_0317/Scenes/Test/Quan_HorrorTest.unity";
        scene = SceneManager.GetSceneByPath(path);
        openedByTest = !scene.IsValid() || !scene.isLoaded;
        if (openedByTest)
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
    }

    [TearDown]
    public void CloseScene()
    {
        if (openedByTest && scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
    }

    private GameObject Root(string name)
    {
        return scene.GetRootGameObjects().Single(g => g.name == name);
    }

    private static Component Event(GameObject go, string typeName)
    {
        var component = go.GetComponent(HorrorTestAccess.Type(typeName));
        Assert.That(component, Is.Not.Null);
        Assert.That(go.GetComponents(HorrorTestAccess.Type("HorrorEventTrigger")).Length, Is.EqualTo(1));
        var serialized = new SerializedObject(component);
        Assert.That(serialized.FindProperty("playOnce").boolValue, Is.True);
        Assert.That(serialized.FindProperty("armed").boolValue, Is.True);
        return component;
    }

    [Test]
    public void HE01HasCorrectTriggerAndAudio()
    {
        var go = Root("HE_01_Footstep");
        Assert.That(go.GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(go.GetComponent<BoxCollider>().isTrigger, Is.True);
        var component = Event(go, "FootstepHorrorEvent");
        var child = go.transform.Find("FootstepAudio");
        Assert.That(child, Is.Not.Null);
        var audio = child.GetComponent<AudioSource>();
        Assert.That(audio, Is.Not.Null);
        Assert.That(audio.playOnAwake, Is.False);
        Assert.That(audio.loop, Is.False);
        Assert.That(audio.spatialBlend, Is.EqualTo(1f));
        Assert.That(new SerializedObject(component).FindProperty("footstepAudio").objectReferenceValue, Is.EqualTo(audio));
    }

    [Test]
    public void HE02HasCorrectTriggerAndLight()
    {
        var go = Root("HE_02_LightFlicker");
        Assert.That(go.GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(go.GetComponent<BoxCollider>().isTrigger, Is.True);
        var component = Event(go, "LightFlickerHorrorEvent");
        var child = go.transform.Find("FlickerLight");
        Assert.That(child, Is.Not.Null);
        var light = child.GetComponent<Light>();
        Assert.That(light, Is.Not.Null);
        Assert.That(light.type, Is.EqualTo(LightType.Point));
        Assert.That(new SerializedObject(component).FindProperty("targetLight").objectReferenceValue, Is.EqualTo(light));
    }

    [Test]
    public void HE03HasCorrectTriggerSpawnAndExistingGhost()
    {
        var go = Root("HE_03_GhostSilhouette");
        Assert.That(go.GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(go.GetComponent<BoxCollider>().isTrigger, Is.True);
        var component = Event(go, "GhostSilhouetteHorrorEvent");
        var spawn = go.transform.Find("GhostSpawnPoint_HE03");
        Assert.That(spawn, Is.Not.Null);
        var ghost = Root("Ghost_Prototype").GetComponent(HorrorTestAccess.Type("GhostActor"));
        Assert.That(ghost, Is.Not.Null);
        var serialized = new SerializedObject(component);
        Assert.That(serialized.FindProperty("ghost").objectReferenceValue, Is.EqualTo(ghost));
        Assert.That(serialized.FindProperty("spawnPoint").objectReferenceValue, Is.EqualTo(spawn));
        Assert.That(serialized.FindProperty("visibleDuration").floatValue, Is.EqualTo(1.5f));
        Assert.That(scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
            .Count(t => t.name == "Ghost_Prototype"), Is.EqualTo(1));
    }

    [Test]
    public void HE04HasCorrectTriggerDoorAndAudio()
    {
        var go = Root("HE_04_DoorSlam");
        Assert.That(go.GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(go.GetComponent<BoxCollider>().isTrigger, Is.True);
        var component = Event(go, "DoorSlamHorrorEvent");
        var door = go.transform.Find("DoorSlamPrototype");
        var audioChild = go.transform.Find("DoorSlamAudio");
        Assert.That(door, Is.Not.Null);
        Assert.That(door.Find("DoorVisual"), Is.Not.Null);
        Assert.That(audioChild, Is.Not.Null);
        var audio = audioChild.GetComponent<AudioSource>();
        Assert.That(audio, Is.Not.Null);
        Assert.That(audio.playOnAwake, Is.False);
        Assert.That(audio.loop, Is.False);
        Assert.That(audio.spatialBlend, Is.EqualTo(1f));
        var serialized = new SerializedObject(component);
        Assert.That(serialized.FindProperty("doorTransform").objectReferenceValue, Is.EqualTo(door));
        Assert.That(serialized.FindProperty("slamAudio").objectReferenceValue, Is.EqualTo(audio));
        Assert.That(Quaternion.Angle(door.localRotation,
            Quaternion.Euler(serialized.FindProperty("closedLocalEulerAngles").vector3Value)), Is.GreaterThan(45f));
        foreach (var transform in go.GetComponentsInChildren<Transform>(true))
            Assert.That(transform.GetComponents<Component>().Any(c => c != null && c.GetType().Name == "DoorSystem"), Is.False);
    }

    [Test]
    public void HE05HasCorrectGhostLightAudioAndSpawn()
    {
        var go = Root("HE_05_GhostApparition");
        Assert.That(go.GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(go.GetComponent<BoxCollider>().isTrigger, Is.True);
        var component = Event(go, "GhostApparitionHorrorEvent");
        var spawn = go.transform.Find("GhostSpawnPoint_HE05");
        var lightChild = go.transform.Find("ApparitionLight");
        var audioChild = go.transform.Find("ApparitionAudio");
        Assert.That(spawn, Is.Not.Null);
        Assert.That(lightChild, Is.Not.Null);
        Assert.That(audioChild, Is.Not.Null);
        var light = lightChild.GetComponent<Light>();
        var audio = audioChild.GetComponent<AudioSource>();
        Assert.That(light, Is.Not.Null);
        Assert.That(light.type, Is.EqualTo(LightType.Point));
        Assert.That(lightChild.GetComponent(HorrorTestAccess.Type("UnityEngine.Rendering.HighDefinition.HDAdditionalLightData")), Is.Not.Null);
        Assert.That(audio, Is.Not.Null);
        Assert.That(audio.playOnAwake, Is.False);
        Assert.That(audio.loop, Is.False);
        Assert.That(audio.spatialBlend, Is.EqualTo(1f));
        var serialized = new SerializedObject(component);
        Assert.That(serialized.FindProperty("ghost").objectReferenceValue,
            Is.EqualTo(Root("Ghost_Prototype").GetComponent(HorrorTestAccess.Type("GhostActor"))));
        Assert.That(serialized.FindProperty("spawnPoint").objectReferenceValue, Is.EqualTo(spawn));
        Assert.That(serialized.FindProperty("targetLight").objectReferenceValue, Is.EqualTo(light));
        Assert.That(serialized.FindProperty("apparitionAudio").objectReferenceValue, Is.EqualTo(audio));
        var camera = Root("Main Camera").transform;
        Assert.That(Vector3.Distance(camera.position, spawn.position),
            Is.LessThan(Vector3.Distance(camera.position, Root("HE_03_GhostSilhouette").transform.Find("GhostSpawnPoint_HE03").position)));
        Assert.That(scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
            .Count(t => t.name == "Ghost_Prototype"), Is.EqualTo(1));
    }

    [Test]
    public void HE06HasCorrectGhostSpawnsLightAndAudio()
    {
        var go = Root("HE_06_Basement");
        Assert.That(go.GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(go.GetComponent<BoxCollider>().isTrigger, Is.True);
        var component = Event(go, "BasementHorrorEvent");
        var first = go.transform.Find("GhostSpawnPoint_HE06_A");
        var second = go.transform.Find("GhostSpawnPoint_HE06_B");
        var lightChild = go.transform.Find("BasementEventLight");
        var audioChild = go.transform.Find("BasementEventAudio");
        Assert.That(first, Is.Not.Null);
        Assert.That(second, Is.Not.Null);
        Assert.That(lightChild, Is.Not.Null);
        Assert.That(audioChild, Is.Not.Null);
        var light = lightChild.GetComponent<Light>();
        var audio = audioChild.GetComponent<AudioSource>();
        Assert.That(light, Is.Not.Null);
        Assert.That(light.type, Is.EqualTo(LightType.Point));
        Assert.That(lightChild.GetComponent(HorrorTestAccess.Type("UnityEngine.Rendering.HighDefinition.HDAdditionalLightData")), Is.Not.Null);
        Assert.That(audio, Is.Not.Null);
        Assert.That(audio.playOnAwake, Is.False);
        Assert.That(audio.loop, Is.False);
        Assert.That(audio.spatialBlend, Is.EqualTo(1f));
        var serialized = new SerializedObject(component);
        Assert.That(serialized.FindProperty("ghost").objectReferenceValue,
            Is.EqualTo(Root("Ghost_Prototype").GetComponent(HorrorTestAccess.Type("GhostActor"))));
        Assert.That(serialized.FindProperty("firstSpawnPoint").objectReferenceValue, Is.EqualTo(first));
        Assert.That(serialized.FindProperty("secondSpawnPoint").objectReferenceValue, Is.EqualTo(second));
        Assert.That(serialized.FindProperty("targetLight").objectReferenceValue, Is.EqualTo(light));
        Assert.That(serialized.FindProperty("basementAudio").objectReferenceValue, Is.EqualTo(audio));
        var camera = Root("Main Camera").transform;
        Assert.That(Vector3.Distance(camera.position, second.position),
            Is.LessThan(Vector3.Distance(camera.position, first.position)));
        Assert.That(Vector3.Distance(camera.position, second.position), Is.GreaterThan(1f));
        Assert.That(scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
            .Count(t => t.name == "Ghost_Prototype"), Is.EqualTo(1));
    }

    [Test]
    public void HE07HasCorrectPrototypeRouteAndReferences()
    {
        var go = Root("HE_07_FinalChasePrototype");
        Assert.That(go.GetComponent<BoxCollider>(), Is.Not.Null);
        Assert.That(go.GetComponent<BoxCollider>().isTrigger, Is.True);
        var component = Event(go, "FinalChaseHorrorEvent");
        var spawn = go.transform.Find("GhostSpawnPoint_HE07");
        var audioChild = go.transform.Find("ChaseAudio");
        var route = Root("ChasePrototypeRoute");
        var manager = Root("GhostChasePrototypeManager");
        Assert.That(spawn, Is.Not.Null);
        Assert.That(audioChild, Is.Not.Null);
        Assert.That(route, Is.Not.Null);
        Assert.That(manager.GetComponent(HorrorTestAccess.Type("GhostChaseController")), Is.Not.Null);
        var names = new[] { "ChaseWaypoint_01", "ChaseWaypoint_02", "ChaseWaypoint_03", "ChaseWaypoint_04" };
        Assert.That(names.Select(n => route.transform.Find(n)).All(t => t != null), Is.True);
        var audio = audioChild.GetComponent<AudioSource>();
        Assert.That(audio, Is.Not.Null);
        Assert.That(audio.playOnAwake, Is.False);
        Assert.That(audio.loop, Is.False);
        Assert.That(audio.spatialBlend, Is.EqualTo(1f));
        var ghost = Root("Ghost_Prototype").GetComponent(HorrorTestAccess.Type("GhostActor"));
        var eventSerialized = new SerializedObject(component);
        Assert.That(eventSerialized.FindProperty("ghost").objectReferenceValue, Is.EqualTo(ghost));
        Assert.That(eventSerialized.FindProperty("apparitionSpawnPoint").objectReferenceValue, Is.EqualTo(spawn));
        Assert.That(eventSerialized.FindProperty("chaseController").objectReferenceValue,
            Is.EqualTo(manager.GetComponent(HorrorTestAccess.Type("GhostChaseController"))));
        Assert.That(eventSerialized.FindProperty("chaseAudio").objectReferenceValue, Is.EqualTo(audio));
        var controller = new SerializedObject(manager.GetComponent(HorrorTestAccess.Type("GhostChaseController")));
        Assert.That(controller.FindProperty("ghost").objectReferenceValue, Is.EqualTo(ghost));
        Assert.That(controller.FindProperty("waypoints").arraySize, Is.EqualTo(4));
        for (int i = 0; i < 4; i++)
            Assert.That(controller.FindProperty("waypoints").GetArrayElementAtIndex(i).objectReferenceValue,
                Is.EqualTo(route.transform.Find(names[i])));
        Assert.That(scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
            .Count(t => t.name == "Ghost_Prototype"), Is.EqualTo(1));
    }

    [TestCase("HE_06_Basement")]
    [TestCase("GhostSpawnPoint_HE06_A")]
    [TestCase("GhostSpawnPoint_HE06_B")]
    [TestCase("BasementEventLight")]
    [TestCase("BasementEventAudio")]
    [TestCase("HE_05_GhostApparition")]
    [TestCase("HE_07_FinalChasePrototype")]
    [TestCase("GhostSpawnPoint_HE07")]
    [TestCase("ChasePrototypeRoute")]
    [TestCase("GhostChasePrototypeManager")]
    [TestCase("ChaseAudio")]
    [TestCase("GhostSpawnPoint_HE05")]
    [TestCase("ApparitionLight")]
    [TestCase("ApparitionAudio")]
    [TestCase("HE_04_DoorSlam")]
    [TestCase("DoorSlamPrototype")]
    [TestCase("DoorVisual")]
    [TestCase("DoorSlamAudio")]
    [TestCase("HE_03_GhostSilhouette")]
    [TestCase("GhostSpawnPoint_HE03")]
    [TestCase("Ghost_Prototype")]
    [TestCase("HorrorTestManager")]
    [TestCase("HE_01_Footstep")]
    [TestCase("FootstepAudio")]
    [TestCase("HE_02_LightFlicker")]
    [TestCase("FlickerLight")]
    public void HorrorObjectsHaveNoMissingScripts(string name)
    {
        var go = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
            .Single(t => t.name == name).gameObject;
        foreach (var transform in go.GetComponentsInChildren<Transform>(true))
            Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject), Is.Zero);
    }
}
