using UnityEngine;

namespace Framework.ScreenAdapter
{
    /// 对于处于父级是 SafeAreaRectTransform 的UI，锚点设置为全屏时，依然是处于安全区之外的
    /// 但是对于一些背景类的UI，它需要扩展到安全区里
    /// 使用这个组件可以抵消父级 SafeAreaRectTransform 的锚点效果，把当前组件的锚点设置为包含安全区的全屏
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class AntiSafeAreaRectTransform : MonoBehaviour
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
            RectTransform rc = GetComponent<RectTransform>();
            rc.anchorMin = ScreenAdapterManager.Instance.antiSafeAreaRect.min;
            rc.anchorMax = ScreenAdapterManager.Instance.antiSafeAreaRect.size;
            rc.anchoredPosition = Vector2.zero;
            rc.sizeDelta = Vector2.zero;
        }
    }
}