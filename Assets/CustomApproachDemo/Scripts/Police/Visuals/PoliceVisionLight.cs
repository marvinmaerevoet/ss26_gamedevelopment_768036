using CustomApproachDemo.Police;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomApproachDemo.Police.Visuals
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class PoliceVisionLight : MonoBehaviour
    {
        private const string DefaultLightName = "VisionLight";
        private const string DefaultConeName = "VisionCone";
        private const string DefaultConeMaterialPath = "Assets/CustomApproachDemo/Art/Lighting/PoliceVisionCone.mat";
        private const int MinimumSegments = 12;

        [Header("References")]
        [SerializeField] private PoliceAIContext context;
        [SerializeField] private Transform lightRoot;
        [SerializeField] private Light spotLight;
        [SerializeField] private MeshFilter coneMeshFilter;
        [SerializeField] private MeshRenderer coneRenderer;
        [SerializeField] private Material coneMaterial;

        [Header("Light")]
        [SerializeField] private Color lightColor = new Color(1f, 0.72f, 0.36f, 1f);
        [SerializeField, Min(0f)] private float lightIntensity = 1.8f;
        [SerializeField, Range(0f, 1f)] private float shadowStrength = 0.25f;
        [SerializeField] private bool castShadows;

        [Header("Cone")]
        [SerializeField] private bool showCone = true;
        [SerializeField] private Color coneColor = new Color(1f, 0.66f, 0.3f, 1f);
        [SerializeField, Range(0f, 1f)] private float coneAlpha = 0.2f;
        [SerializeField, Range(0.05f, 2f)] private float edgeSoftness = 0.55f;
        [SerializeField, Range(0.25f, 4f)] private float farFadePower = 1.6f;
        [SerializeField, Range(0f, 1f)] private float noiseStrength = 0.22f;
        [SerializeField, Range(0.1f, 12f)] private float noiseScale = 3.5f;
        [SerializeField, Range(12, 96)] private int coneSegments = 48;
        [SerializeField, Range(3, 16)] private int coneLengthSteps = 8;
        [SerializeField, Range(1, 8)] private int coneSheets = 5;

        private Mesh coneMesh;
        private float lastRange = -1f;
        private float lastAngle = -1f;
        private int lastSegments = -1;
        private int lastLengthSteps = -1;
        private int lastSheets = -1;

        private void Reset()
        {
            context = GetComponent<PoliceAIContext>();
            EnsureVisualObjects();
            SyncVisuals();
        }

        private void Awake()
        {
            EnsureVisualObjects();
            SyncVisuals();
        }

        private void OnEnable()
        {
            EnsureVisualObjects();
            SyncVisuals();
        }

            private void OnValidate()
        {
            coneSegments = Mathf.Max(MinimumSegments, coneSegments);
            coneLengthSteps = Mathf.Max(3, coneLengthSteps);
            coneSheets = Mathf.Max(1, coneSheets);
            EnsureVisualObjects();
            SyncVisuals();
        }

        private void LateUpdate()
        {
            SyncVisuals();
        }

        [ContextMenu("Sync Vision Light")]
        public void SyncVisuals()
        {
            if (!ResolveContext())
            {
                return;
            }

            EnsureVisualObjects();

            float range = Mathf.Max(0.1f, context.viewDistance);
            float angle = Mathf.Clamp(context.viewAngle, 1f, 179f);

            SyncTransform();
            SyncSpotLight(range, angle);
            SyncCone(range, angle);
        }

        private bool ResolveContext()
        {
            if (context == null)
            {
                context = GetComponent<PoliceAIContext>();
            }

            return context != null;
        }

        private void EnsureVisualObjects()
        {
            if (!ResolveContext())
            {
                return;
            }

            Transform parent = context.EyePoint != null ? context.EyePoint : transform;

            if (lightRoot == null)
            {
                lightRoot = parent.Find(DefaultLightName);
            }

            if (lightRoot == null)
            {
                GameObject lightObject = new GameObject(DefaultLightName);
                lightRoot = lightObject.transform;
                lightRoot.SetParent(parent, false);
                lightRoot.localPosition = Vector3.zero;
                lightRoot.localRotation = Quaternion.identity;
            }

            if (spotLight == null)
            {
                spotLight = lightRoot.GetComponent<Light>();
            }

            if (spotLight == null)
            {
                spotLight = lightRoot.gameObject.AddComponent<Light>();
            }

            Transform coneTransform = lightRoot.Find(DefaultConeName);
            if (coneTransform == null)
            {
                GameObject coneObject = new GameObject(DefaultConeName);
                coneTransform = coneObject.transform;
                coneTransform.SetParent(lightRoot, false);
                coneTransform.localPosition = Vector3.zero;
                coneTransform.localRotation = Quaternion.identity;
                coneTransform.localScale = Vector3.one;
            }

            if (coneMeshFilter == null)
            {
                coneMeshFilter = coneTransform.GetComponent<MeshFilter>();
            }

            if (coneMeshFilter == null)
            {
                coneMeshFilter = coneTransform.gameObject.AddComponent<MeshFilter>();
            }

            if (coneRenderer == null)
            {
                coneRenderer = coneTransform.GetComponent<MeshRenderer>();
            }

            if (coneRenderer == null)
            {
                coneRenderer = coneTransform.gameObject.AddComponent<MeshRenderer>();
            }

            #if UNITY_EDITOR
            if (coneMaterial == null)
            {
                coneMaterial = AssetDatabase.LoadAssetAtPath<Material>(DefaultConeMaterialPath);
            }
            #endif

            if (coneMaterial != null && coneRenderer.sharedMaterial != coneMaterial)
            {
                coneRenderer.sharedMaterial = coneMaterial;
            }
        }

        private void SyncTransform()
        {
            Transform parent = context.EyePoint != null ? context.EyePoint : transform;

            if (lightRoot.parent != parent)
            {
                lightRoot.SetParent(parent, false);
            }

            lightRoot.localPosition = Vector3.zero;
            lightRoot.localRotation = Quaternion.identity;
            lightRoot.localScale = Vector3.one;
        }

        private void SyncSpotLight(float range, float angle)
        {
            if (spotLight == null)
            {
                return;
            }

            spotLight.type = LightType.Spot;
            spotLight.color = lightColor;
            spotLight.intensity = lightIntensity;
            spotLight.range = range;
            spotLight.spotAngle = angle;
            spotLight.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
            spotLight.shadowStrength = shadowStrength;
        }

        private void SyncCone(float range, float angle)
        {
            if (coneMeshFilter == null || coneRenderer == null)
            {
                return;
            }

            coneRenderer.enabled = showCone;

            if (coneMaterial != null)
            {
                coneRenderer.sharedMaterial = coneMaterial;
                SyncConeMaterial(coneRenderer.sharedMaterial, range, angle);
            }

            if (!showCone)
            {
                return;
            }

            if (coneMesh == null)
            {
                coneMesh = new Mesh
                {
                    name = "Police Vision Cone",
                    hideFlags = HideFlags.DontSave
                };
                coneMesh.MarkDynamic();
                coneMeshFilter.sharedMesh = coneMesh;
            }

            if (Mathf.Approximately(lastRange, range)
                && Mathf.Approximately(lastAngle, angle)
                && lastSegments == coneSegments
                && lastLengthSteps == coneLengthSteps
                && lastSheets == coneSheets)
            {
                return;
            }

            RebuildConeMesh(coneMesh, range, angle, coneSegments, coneLengthSteps, coneSheets);
            lastRange = range;
            lastAngle = angle;
            lastSegments = coneSegments;
            lastLengthSteps = coneLengthSteps;
            lastSheets = coneSheets;
        }

        private void SyncConeMaterial(Material material, float range, float angle)
        {
            if (material.HasProperty("_Color"))
            {
                Color color = coneColor;
                color.a = 1f;
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_Alpha"))
            {
                material.SetFloat("_Alpha", coneAlpha);
            }

            if (material.HasProperty("_Range"))
            {
                material.SetFloat("_Range", range);
            }

            if (material.HasProperty("_ConeAngle"))
            {
                material.SetFloat("_ConeAngle", angle);
            }

            if (material.HasProperty("_EdgeSoftness"))
            {
                material.SetFloat("_EdgeSoftness", edgeSoftness);
            }

            if (material.HasProperty("_FarFadePower"))
            {
                material.SetFloat("_FarFadePower", farFadePower);
            }

            if (material.HasProperty("_NoiseStrength"))
            {
                material.SetFloat("_NoiseStrength", noiseStrength);
            }

            if (material.HasProperty("_NoiseScale"))
            {
                material.SetFloat("_NoiseScale", noiseScale);
            }
        }

        private static void RebuildConeMesh(Mesh mesh, float range, float angle, int segments, int lengthSteps, int sheetCount)
        {
            segments = Mathf.Max(MinimumSegments, segments);
            lengthSteps = Mathf.Max(3, lengthSteps);
            sheetCount = Mathf.Max(1, sheetCount);

            int columns = Mathf.Max(4, segments / 8);
            int rows = lengthSteps + 1;
            int verticesPerSheet = rows * columns;
            Vector3[] vertices = new Vector3[sheetCount * verticesPerSheet];
            Vector2[] uvs = new Vector2[vertices.Length];
            Color[] colors = new Color[vertices.Length];
            int[] triangles = new int[sheetCount * lengthSteps * (columns - 1) * 6];

            float radius = Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad) * range;

            for (int sheet = 0; sheet < sheetCount; sheet++)
            {
                float rotation = sheet * Mathf.PI / sheetCount;
                Vector3 right = new Vector3(Mathf.Cos(rotation), Mathf.Sin(rotation), 0f);

                for (int row = 0; row < rows; row++)
                {
                    float length01 = row / (float) lengthSteps;
                    float softenedLength = Mathf.Lerp(0.015f, 1f, length01);
                    float halfWidth = radius * softenedLength;

                    for (int column = 0; column < columns; column++)
                    {
                        float across01 = column / (float) (columns - 1);
                        float centered = across01 * 2f - 1f;
                        float wobble = Mathf.Sin(across01 * Mathf.PI * 5f + length01 * Mathf.PI * 3f + sheet) * 0.035f;
                        int vertexIndex = sheet * verticesPerSheet + row * columns + column;

                        vertices[vertexIndex] = right * centered * halfWidth * (1f + wobble)
                            + Vector3.forward * range * softenedLength;

                        uvs[vertexIndex] = new Vector2(across01, length01);
                        colors[vertexIndex] = new Color(1f, 1f, 1f, 1f);
                    }
                }
            }

            int triangleIndex = 0;
            for (int sheet = 0; sheet < sheetCount; sheet++)
            {
                int sheetOffset = sheet * verticesPerSheet;

                for (int row = 0; row < lengthSteps; row++)
                {
                    for (int column = 0; column < columns - 1; column++)
                    {
                        int current = sheetOffset + row * columns + column;
                        int next = current + 1;
                        int upper = current + columns;
                        int upperNext = upper + 1;

                        triangles[triangleIndex++] = current;
                        triangles[triangleIndex++] = upper;
                        triangles[triangleIndex++] = next;

                        triangles[triangleIndex++] = next;
                        triangles[triangleIndex++] = upper;
                        triangles[triangleIndex++] = upperNext;
                    }
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
    }
}
