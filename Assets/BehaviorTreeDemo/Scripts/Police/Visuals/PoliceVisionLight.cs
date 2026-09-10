using BehaviorTreeDemo.Police;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BehaviorTreeDemo.Police.Visuals
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class PoliceVisionLight : MonoBehaviour
    {
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

        private bool warnedMissingContext;
        private bool warnedMissingVisualReferences;

        private void Reset()
        {
            context = GetComponent<PoliceAIContext>();
            SyncVisuals();
        }

        private void Awake()
        {
            SyncVisuals();
        }

        private void OnEnable()
        {
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
            if (!ResolveConfiguredReferences())
            {
                return;
            }

            float range = Mathf.Max(0.1f, context.viewDistance);
            float angle = Mathf.Clamp(context.viewAngle, 1f, 179f);

            SyncTransform();
            SyncSpotLight(range, angle);
        }

        private bool ResolveConfiguredReferences()
        {
            if (context == null)
            {
                context = GetComponent<PoliceAIContext>();
            }

            if (context == null)
            {
                if (!warnedMissingContext)
                {
                    Debug.LogError("PoliceVisionLight needs a PoliceAIContext on the same GameObject.", this);
                    warnedMissingContext = true;
                }

                return false;
            }

            warnedMissingContext = false;

            if (visualOrigin == null || lightRoot == null || spotLight == null)
            {
                if (!warnedMissingVisualReferences)
                {
                    Debug.LogError("PoliceVisionLight needs assigned Visual Origin, Light Root, and Spot Light references.", this);
                    warnedMissingVisualReferences = true;
                }

                return false;
            }

            warnedMissingVisualReferences = false;
            return true;
        }

        private void SyncTransform()
        {
            Quaternion rotation = context.EyePoint != null ? context.EyePoint.rotation : transform.rotation;
            if (lightRoot.parent == visualOrigin)
            {
                lightRoot.localPosition = Vector3.zero;
                lightRoot.rotation = rotation;
            }
            else
            {
                lightRoot.SetPositionAndRotation(visualOrigin.position, rotation);
            }

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
