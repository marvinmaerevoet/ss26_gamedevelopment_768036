using CustomApproachDemo.Police;
using UnityEngine;
using UnityEngine.Serialization;

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
        private const string DefaultVisualOriginName = "VisionLightCone";
        private const string DefaultConeName = "VisionCone";
        private const string DefaultConeMaterialPath = "Assets/CustomApproachDemo/Art/Lighting/PoliceVisionCone.mat";
        private const int MinimumSegments = 12;

        [Header("References")]
        [SerializeField] private PoliceAIContext context;
        [Tooltip("Independent visual position. Move this Transform to place the lamp; direction follows EyePoint.")]
        [SerializeField] private Transform visualOrigin;
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
        [SerializeField, Range(0.001f, 0.5f)] private float nearFade = 0.08f;
        [SerializeField, Range(0.05f, 2f)] private float edgeSoftness = 0.55f;
        [SerializeField, Range(0.25f, 4f)] private float farFadePower = 1.6f;
        [SerializeField, Range(0f, 2f)] private float centerBoost = 0.8f;
        [SerializeField, Range(0f, 4f)] private float depthFade = 1.25f;
        [SerializeField, Range(0f, 1f)] private float noiseStrength = 0.22f;
        [SerializeField, Range(0.1f, 12f)] private float noiseScale = 3.5f;
        [SerializeField, Range(32, 128)] private int coneSegments = 80;
        [SerializeField, Range(3, 16)] private int coneLengthSteps = 8;
        [FormerlySerializedAs("coneSheets")]
        [SerializeField, Range(3, 8)] private int coneRadialLayers = 5;

        private Mesh coneMesh;
        private float lastRange = -1f;
        private float lastAngle = -1f;
        private int lastSegments = -1;
        private int lastLengthSteps = -1;
        private int lastRadialLayers = -1;

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
            coneRadialLayers = Mathf.Max(3, coneRadialLayers);
            #if UNITY_EDITOR
            EditorApplication.delayCall -= SyncAfterValidation;
            EditorApplication.delayCall += SyncAfterValidation;
            #endif
        }

        #if UNITY_EDITOR
        private void SyncAfterValidation()
        {
            if (this != null && isActiveAndEnabled)
            {
                SyncVisuals();
            }
        }
        #endif

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

            if (visualOrigin == null)
            {
                visualOrigin = transform.Find(DefaultVisualOriginName);
            }

            if (visualOrigin == null)
            {
                visualOrigin = new GameObject(DefaultVisualOriginName).transform;
                visualOrigin.SetParent(transform, false);
                // Initial placement only. Inspector edits are never overwritten.
                visualOrigin.localPosition = new Vector3(0f, 1.4f, 0.3f);
            }

            Transform parent = visualOrigin;

            if (lightRoot == null)
            {
                lightRoot = parent.Find(DefaultLightName);
                if (lightRoot == null && context.EyePoint != null)
                {
                    // Reuse the existing visual when migrating from EyePoint parenting.
                    lightRoot = context.EyePoint.Find(DefaultLightName);
                }
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
            Transform parent = visualOrigin;

            if (lightRoot.parent != parent)
            {
                lightRoot.SetParent(parent, false);
            }

            lightRoot.localPosition = Vector3.zero;
            lightRoot.rotation = context.EyePoint != null ? context.EyePoint.rotation : transform.rotation;
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
                && lastRadialLayers == coneRadialLayers)
            {
                return;
            }

            RebuildConeMesh(coneMesh, range, angle, coneSegments, coneLengthSteps, coneRadialLayers);
            lastRange = range;
            lastAngle = angle;
            lastSegments = coneSegments;
            lastLengthSteps = coneLengthSteps;
            lastRadialLayers = coneRadialLayers;
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

            if (material.HasProperty("_NearFade"))
            {
                material.SetFloat("_NearFade", nearFade);
            }

            if (material.HasProperty("_FarFadePower"))
            {
                material.SetFloat("_FarFadePower", farFadePower);
            }

            if (material.HasProperty("_CenterBoost"))
            {
                material.SetFloat("_CenterBoost", centerBoost);
            }

            if (material.HasProperty("_DepthFade"))
            {
                material.SetFloat("_DepthFade", depthFade);
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

        private static void RebuildConeMesh(Mesh mesh, float range, float angle, int segments, int lengthSteps, int radialLayers)
        {
            segments = Mathf.Max(MinimumSegments, segments);
            lengthSteps = Mathf.Max(3, lengthSteps);
            radialLayers = Mathf.Max(3, radialLayers);

            int rows = lengthSteps + 1;
            int radialRows = radialLayers;
            int verticesPerRing = segments + 1;
            Vector3[] vertices = new Vector3[rows * radialRows * verticesPerRing];
            Vector2[] uvs = new Vector2[vertices.Length];
            Color[] colors = new Color[vertices.Length];
            int[] triangles = new int[lengthSteps * radialRows * segments * 6];

            float radius = Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad) * range;

            for (int row = 0; row < rows; row++)
            {
                float length01 = row / (float) lengthSteps;
                float easedLength = Mathf.Lerp(0.02f, 1f, length01);
                float rowRadius = radius * easedLength;

                for (int radial = 0; radial < radialRows; radial++)
                {
                    float radial01 = (radial + 1f) / radialRows;
                    float easedRadial = Mathf.Pow(radial01, 0.78f);
                    float ringRadius = rowRadius * easedRadial;
                    float radialAlpha = Mathf.Lerp(0.95f, 0.04f, Mathf.SmoothStep(0f, 1f, radial01));

                    for (int segment = 0; segment <= segments; segment++)
                    {
                        float segment01 = segment / (float) segments;
                        float radians = segment01 * Mathf.PI * 2f;
                        float wobble = 1f + Mathf.Sin(segment01 * Mathf.PI * 6f + length01 * Mathf.PI * 2.7f) * 0.012f;
                        int vertexIndex = VertexIndex(row, radial, segment, radialRows, verticesPerRing);

                        vertices[vertexIndex] = new Vector3(
                            Mathf.Cos(radians) * ringRadius * wobble,
                            Mathf.Sin(radians) * ringRadius * wobble,
                            range * easedLength);

                        uvs[vertexIndex] = new Vector2(radial01, length01);
                        colors[vertexIndex] = new Color(1f, 1f, 1f, radialAlpha);
                    }
                }
            }

            int triangleIndex = 0;
            for (int row = 0; row < lengthSteps; row++)
            {
                for (int radial = 0; radial < radialRows; radial++)
                {
                    for (int segment = 0; segment < segments; segment++)
                    {
                        int current = VertexIndex(row, radial, segment, radialRows, verticesPerRing);
                        int next = VertexIndex(row, radial, segment + 1, radialRows, verticesPerRing);
                        int forward = VertexIndex(row + 1, radial, segment, radialRows, verticesPerRing);
                        int forwardNext = VertexIndex(row + 1, radial, segment + 1, radialRows, verticesPerRing);

                        triangles[triangleIndex++] = current;
                        triangles[triangleIndex++] = forward;
                        triangles[triangleIndex++] = next;

                        triangles[triangleIndex++] = next;
                        triangles[triangleIndex++] = forward;
                        triangles[triangleIndex++] = forwardNext;
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

        private static int VertexIndex(int row, int radial, int segment, int radialRows, int verticesPerRing)
        {
            return (row * radialRows + radial) * verticesPerRing + segment;
        }
    }
}
