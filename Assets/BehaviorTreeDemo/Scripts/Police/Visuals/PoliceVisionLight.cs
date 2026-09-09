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
        private const string DefaultVisualOriginName = "VisionLightCone";

        [Header("References")]
        [SerializeField] private PoliceAIContext context;
        [Tooltip("Independent visual position. Move this Transform to place the lamp; direction follows EyePoint.")]
        [SerializeField] private Transform visualOrigin;
        [SerializeField] private Transform lightRoot;
        [SerializeField] private Light spotLight;

        [Header("Light")]
        [SerializeField] private Color lightColor = new Color(1f, 0.72f, 0.36f, 1f);
        [SerializeField, Min(0f)] private float lightIntensity = 1.8f;
        [SerializeField, Range(0f, 1f)] private float shadowStrength = 0.25f;
        [SerializeField] private bool castShadows;

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
                visualOrigin.localPosition = new Vector3(0f, 1.4f, 0.3f);
            }

            if (lightRoot == null)
            {
                lightRoot = visualOrigin.Find(DefaultLightName);
            }

            if (lightRoot == null)
            {
                lightRoot = new GameObject(DefaultLightName).transform;
                lightRoot.SetParent(visualOrigin, false);
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
        }

        private void SyncTransform()
        {
            if (lightRoot.parent != visualOrigin)
            {
                lightRoot.SetParent(visualOrigin, false);
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
    }
}
