#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FpsHorrorKit.EditorTools
{
    [InitializeOnLoad]
    public static class ExorcismCircleCreator
    {
        private const string RootName = "Vong Tron Tru Ta";
        private const string MaterialFolder = "Assets/FpsHorrorKit/Materials/ExorcismCircle";
        private static readonly string AutoCreateKey = "Codex.ExorcismCircle.Created." + Application.dataPath;

        static ExorcismCircleCreator()
        {
            EditorApplication.delayCall += CreateOnceAfterCompile;
        }

        private static void CreateOnceAfterCompile()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorPrefs.GetBool(AutoCreateKey, false))
                return;

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(scene.path))
                return;

            if (GameObject.Find(RootName) != null)
            {
                EditorPrefs.SetBool(AutoCreateKey, true);
                return;
            }

            CreateExorcismCircle();
            EditorPrefs.SetBool(AutoCreateKey, true);
        }

        [MenuItem("Tools/Codex/Create Exorcism Circle")]
        public static void CreateExorcismCircle()
        {
            GameObject existing = GameObject.Find(RootName);
            if (existing != null)
            {
                Selection.activeGameObject = existing;
                SceneView.lastActiveSceneView?.FrameSelected();
                Debug.Log("[Codex] Exorcism circle already exists; selected the existing object.");
                return;
            }

            EnsureMaterialFolder();
            Material chalkMaterial = GetOrCreateMaterial(
                "ExorcismCircle_Chalk.mat",
                new Color(0.74f, 0.69f, 0.57f, 1f),
                new Color(0.09f, 0.075f, 0.045f, 1f)
            );
            Material bloodMaterial = GetOrCreateMaterial(
                "ExorcismCircle_Blood.mat",
                new Color(0.28f, 0.012f, 0.008f, 1f),
                new Color(0.18f, 0.006f, 0.002f, 1f)
            );
            Material waxMaterial = GetOrCreateMaterial(
                "ExorcismCircle_Wax.mat",
                new Color(0.48f, 0.43f, 0.31f, 1f),
                Color.black
            );
            Material flameMaterial = GetOrCreateMaterial(
                "ExorcismCircle_Flame.mat",
                new Color(1f, 0.28f, 0.025f, 1f),
                new Color(5.5f, 0.72f, 0.04f, 1f)
            );

            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Exorcism Circle");
            root.transform.position = FindPlacementPoint();

            CreateChalkGeometry(root.transform, chalkMaterial);
            CreateBloodGeometry(root.transform, bloodMaterial);
            CreateCandles(root.transform, waxMaterial, flameMaterial);

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(root.scene);
            EditorSceneManager.SaveScene(root.scene);
            SceneView.lastActiveSceneView?.FrameSelected();
            Debug.Log("[Codex] Exorcism circle created and scene saved at " + root.transform.position + ".");
        }

        private static void CreateChalkGeometry(Transform parent, Material material)
        {
            FlatLineMeshBuilder builder = new FlatLineMeshBuilder();
            builder.AddRing(1.82f, 0.075f, 128, 0.02f);
            builder.AddRing(1.56f, 0.038f, 112, 0.021f);
            builder.AddRing(0.53f, 0.032f, 64, 0.022f);

            Vector3[] star = BuildCirclePoints(1.39f, 5, -90f, 0.023f);
            for (int i = 0; i < star.Length; i++)
                builder.AddLine(star[i], star[(i + 2) % star.Length], 0.047f);

            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f * Mathf.Deg2Rad;
                Vector3 radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                Vector3 tangent = new Vector3(-radial.z, 0f, radial.x);
                Vector3 center = radial * 1.68f + Vector3.up * 0.024f;

                builder.AddLine(center - radial * 0.10f, center + radial * 0.10f, 0.035f);
                if (i % 3 == 0)
                {
                    builder.AddLine(center - tangent * 0.105f, center + tangent * 0.105f, 0.035f);
                }
                else
                {
                    Vector3 tip = center + radial * 0.105f;
                    builder.AddLine(center - tangent * 0.085f - radial * 0.075f, tip, 0.032f);
                    builder.AddLine(center + tangent * 0.085f - radial * 0.075f, tip, 0.032f);
                }
            }

            builder.AddLine(new Vector3(-0.34f, 0.025f, 0f), new Vector3(0.34f, 0.025f, 0f), 0.035f);
            builder.AddLine(new Vector3(0f, 0.025f, -0.34f), new Vector3(0f, 0.025f, 0.34f), 0.035f);

            Random.State previousState = Random.state;
            Random.InitState(317);
            for (int i = 0; i < 85; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float radius = Random.Range(1.46f, 1.92f);
                Vector3 point = new Vector3(Mathf.Cos(angle) * radius, 0.019f, Mathf.Sin(angle) * radius);
                Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
                builder.AddLine(point - direction * Random.Range(0.012f, 0.03f), point + direction * Random.Range(0.012f, 0.055f), Random.Range(0.012f, 0.025f));
            }
            Random.state = previousState;

            CreateMeshObject("Old Chalk Runes", parent, builder.Build("Exorcism Chalk Mesh"), material);
        }

        private static void CreateBloodGeometry(Transform parent, Material material)
        {
            FlatLineMeshBuilder builder = new FlatLineMeshBuilder();
            builder.AddRing(1.18f, 0.027f, 96, 0.028f);

            Vector3[] star = BuildCirclePoints(0.92f, 5, -90f, 0.029f);
            for (int i = 0; i < star.Length; i++)
                builder.AddLine(star[i], star[(i + 2) % star.Length], 0.026f);

            for (int i = 0; i < 6; i++)
            {
                float angle = (i * 60f + 30f) * Mathf.Deg2Rad;
                Vector3 a = new Vector3(Mathf.Cos(angle), 0.03f, Mathf.Sin(angle)) * 1.21f;
                Vector3 b = new Vector3(Mathf.Cos(angle), 0.03f, Mathf.Sin(angle)) * 1.39f;
                builder.AddLine(a, b, 0.025f);
            }

            CreateMeshObject("Dark Binding Sigil", parent, builder.Build("Binding Sigil Mesh"), material);
        }

        private static void CreateCandles(Transform parent, Material waxMaterial, Material flameMaterial)
        {
            GameObject candleRoot = new GameObject("Ritual Candles");
            candleRoot.transform.SetParent(parent, false);

            Random.State previousState = Random.state;
            Random.InitState(1703);

            for (int i = 0; i < 6; i++)
            {
                float angle = (i * 60f + 15f) * Mathf.Deg2Rad;
                float radius = 2.08f + Random.Range(-0.04f, 0.05f);
                float height = Random.Range(0.17f, 0.29f);
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, height * 0.5f, Mathf.Sin(angle) * radius);

                GameObject candle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                candle.name = "Old Candle " + (i + 1);
                candle.transform.SetParent(candleRoot.transform, false);
                candle.transform.localPosition = position;
                candle.transform.localScale = new Vector3(Random.Range(0.07f, 0.095f), height * 0.5f, Random.Range(0.07f, 0.095f));
                candle.transform.localRotation = Quaternion.Euler(Random.Range(-2f, 2f), Random.Range(0f, 360f), Random.Range(-2f, 2f));
                candle.GetComponent<MeshRenderer>().sharedMaterial = waxMaterial;
                Object.DestroyImmediate(candle.GetComponent<Collider>());

                GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flame.name = "Flame";
                flame.transform.SetParent(candle.transform, false);
                flame.transform.localPosition = new Vector3(0f, 1.08f, 0f);
                flame.transform.localScale = new Vector3(0.34f, 0.62f, 0.34f);
                flame.GetComponent<MeshRenderer>().sharedMaterial = flameMaterial;
                Object.DestroyImmediate(flame.GetComponent<Collider>());

                if (i % 2 == 0)
                {
                    Light light = flame.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = new Color(1f, 0.31f, 0.08f);
                    light.range = 2.2f;
                    light.intensity = 32f;
                    light.shadows = LightShadows.None;
                }
            }

            Random.state = previousState;
        }

        private static Vector3 FindPlacementPoint()
        {
            FpsController player = Object.FindFirstObjectByType<FpsController>();
            if (player != null)
            {
                Vector3 forward = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up).normalized;
                if (forward.sqrMagnitude < 0.1f)
                    forward = Vector3.forward;

                Vector3 desired = player.transform.position + forward * 3.2f;
                if (Physics.Raycast(desired + Vector3.up * 3f, Vector3.down, out RaycastHit hit, 12f, ~0, QueryTriggerInteraction.Ignore))
                    return hit.point + Vector3.up * 0.018f;

                return new Vector3(desired.x, player.transform.position.y - 0.9f, desired.z);
            }

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && sceneView.camera != null)
            {
                Vector3 desired = sceneView.pivot;
                if (Physics.Raycast(desired + Vector3.up * 8f, Vector3.down, out RaycastHit hit, 20f, ~0, QueryTriggerInteraction.Ignore))
                    return hit.point + Vector3.up * 0.018f;
                return new Vector3(desired.x, 0.02f, desired.z);
            }

            return new Vector3(0f, 0.02f, 0f);
        }

        private static Vector3[] BuildCirclePoints(float radius, int count, float startAngleDegrees, float y)
        {
            Vector3[] points = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                float angle = (startAngleDegrees + i * 360f / count) * Mathf.Deg2Rad;
                points[i] = new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
            }
            return points;
        }

        private static void CreateMeshObject(string name, Transform parent, Mesh mesh, Material material)
        {
            GameObject gameObject = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            gameObject.transform.SetParent(parent, false);
            gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            gameObject.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void EnsureMaterialFolder()
        {
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
                AssetDatabase.CreateFolder("Assets/FpsHorrorKit/Materials", "ExorcismCircle");
        }

        private static Material GetOrCreateMaterial(string fileName, Color baseColor, Color emissionColor)
        {
            string path = MaterialFolder + "/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            Shader shader = Shader.Find("HDRP/Unlit") ?? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            material = new Material(shader) { name = System.IO.Path.GetFileNameWithoutExtension(fileName) };

            SetColorIfPresent(material, "_BaseColor", baseColor);
            SetColorIfPresent(material, "_UnlitColor", baseColor);
            SetColorIfPresent(material, "_Color", baseColor);
            SetColorIfPresent(material, "_EmissionColor", emissionColor);
            if (material.HasProperty("_EmissiveColor"))
                material.SetColor("_EmissiveColor", emissionColor);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", 0.08f);
            material.enableInstancing = true;

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void SetColorIfPresent(Material material, string property, Color color)
        {
            if (material.HasProperty(property))
                material.SetColor(property, color);
        }

        private sealed class FlatLineMeshBuilder
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<Vector2> uvs = new List<Vector2>();
            private readonly List<int> triangles = new List<int>();

            public void AddRing(float radius, float width, int segments, float y)
            {
                for (int i = 0; i < segments; i++)
                {
                    float angleA = i * Mathf.PI * 2f / segments;
                    float angleB = (i + 1) * Mathf.PI * 2f / segments;
                    Vector3 a = new Vector3(Mathf.Cos(angleA) * radius, y, Mathf.Sin(angleA) * radius);
                    Vector3 b = new Vector3(Mathf.Cos(angleB) * radius, y, Mathf.Sin(angleB) * radius);
                    AddLine(a, b, width);
                }
            }

            public void AddLine(Vector3 start, Vector3 end, float width)
            {
                Vector3 direction = end - start;
                direction.y = 0f;
                if (direction.sqrMagnitude < 0.000001f)
                    return;

                Vector3 side = Vector3.Cross(Vector3.up, direction.normalized) * (width * 0.5f);
                int index = vertices.Count;
                vertices.Add(start - side);
                vertices.Add(start + side);
                vertices.Add(end + side);
                vertices.Add(end - side);
                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(0f, 1f));
                uvs.Add(new Vector2(1f, 1f));
                uvs.Add(new Vector2(1f, 0f));
                triangles.Add(index);
                triangles.Add(index + 1);
                triangles.Add(index + 2);
                triangles.Add(index);
                triangles.Add(index + 2);
                triangles.Add(index + 3);
            }

            public Mesh Build(string name)
            {
                Mesh mesh = new Mesh { name = name };
                mesh.SetVertices(vertices);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
#endif
