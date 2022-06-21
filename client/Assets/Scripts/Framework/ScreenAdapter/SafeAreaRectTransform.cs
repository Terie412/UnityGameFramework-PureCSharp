using UnityEngine;

namespace Framework.ScreenAdapter
{
    /// 将这个脚本挂载在某个RectTransform下面。它会修改当前 RectTransform 的锚点，使之成为不包含的安全区的全屏UI
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class SafeAreaRectTransform : MonoBehaviour
    {
        private void Awake()
        {
            ScreenAdapterManager.Instance.onScreenResolutionChanged.AddListener(_ =>
            {
                Refresh();
            });
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            RectTransform rtf = GetComponent<RectTransform>();
            if (rtf == null) return;

            float offsetMaxX = rtf.offsetMax.x;
            float offsetMaxY = rtf.offsetMax.y;
            float offsetMinX = rtf.offsetMin.x;
            float offsetMinY = rtf.offsetMin.y;
            rtf.anchorMin = ScreenAdapterManager.Instance.safeAreaRect.min;
            rtf.anchorMax = ScreenAdapterManager.Instance.safeAreaRect.size;
            rtf.anchoredPosition = Vector2.zero;
            rtf.sizeDelta = Vector2.zero;
            if (offsetMaxX == 0.0f && offsetMaxY == 0.0f && offsetMinX == 0.0f && offsetMinY == 0.0f) return;

            rtf.offsetMax = new Vector2(offsetMaxX, offsetMaxY);
            rtf.offsetMin = new Vector2(offsetMinX, offsetMinY);
        }
    }
}