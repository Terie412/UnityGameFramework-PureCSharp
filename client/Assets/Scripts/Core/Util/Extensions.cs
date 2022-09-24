using System;
using UnityEngine;

namespace Core
{
    public static class Extensions
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Behaviour
        {
            if (go == null)
            {
                return null;
            }

            var comp = go.GetComponent<T>();
            if (comp == null)
            {
                return go.AddComponent<T>();
            }

            return comp;
        }

        public static Component GetOrAddComponent(this GameObject go, Type t)
        {
            if (go == null)
            {
                return null;
            }

            var comp = go.GetComponent(t);
            if (comp == null)
            {
                return go.AddComponent(t);
            }

            return comp;
        }

        public static Component GetOrAddComponent(this Component comp, Type t)
        {
            return comp == null ? null : comp.gameObject.GetOrAddComponent(t);
        }

        public static T GetOrAddComponent<T>(this Component comp) where T : Behaviour
        {
            return comp == null ? null : comp.gameObject.GetOrAddComponent<T>();
        }

        public static void SetAdaptedFOV(this Camera camera, float fov)
        {
            var referenceAspectRatio = ScreenAdapterManager.Instance.referenceAspectRatio;
            var curAspectRatio = ScreenAdapterManager.Instance.aspectRatio;
            var needAdapt = Mathf.Abs(curAspectRatio - referenceAspectRatio) > 0.001f;
            if (!needAdapt) camera.fieldOfView = fov;

            var fovFactor = referenceAspectRatio / curAspectRatio;
            var tan = Mathf.Tan(fov / 2f * Mathf.Deg2Rad) * fovFactor;
            var newFov = Mathf.Atan(tan) * 2f * Mathf.Rad2Deg;
            camera.fieldOfView = newFov;
        }
    }
}