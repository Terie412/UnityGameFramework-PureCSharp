using Framework;
using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    public class ScreenAdapterManager : SingleTon<ScreenAdapterManager>
    {
        public bool isInit;
        
        public float aspectRatio => (float) Screen.width / Screen.height; // 屏幕分辨率比值
        private const float maxSafeAreaInsetWidthInPixel = 100; // 最大的安全区侧边宽度大小，像素单位
        public readonly float referenceAspectRatio = 2.0f; // 参考的宽高比。想开发相机效果的时候，屏幕分辨率的比例应当是这个比例，而在实际的运行时，相机的FOV会参考该分辨率进行调整

        private float safeAreaInsetWidthNormalized // 归一化的安全区侧边宽度，为安全区侧边的实际宽度:屏幕宽度
        {
            get
            {
#if UNITY_EDITOR
                return Screen.safeArea.x / Screen.width; // Unity 2019 开始有一个 Simulator 的功能，用来模拟真机的设备情况，所以不再需要自己做一个 IPhoneSimulator 来模拟存在安全区的设备了
#elif UNITY_ANDROID
                return AndroidScreenSupport.Instance.safeAreaInsetWidthNormalized;
#elif UNITY_IOS
                return UnityEngine.Screen.safeArea.x / UnityEngine.Screen.width;
#else
                return 0;
#endif
            }
        }
        private Rect m_safeAreaRect = new(-1, -1, -1, -1);
        private Rect m_antiSafeAreaRect = new(-1, -1, -1, -1);
        public Rect safeAreaRect
        {
            get
            {
                if (!(m_safeAreaRect.x < 0)) return m_safeAreaRect;

                UpdateSafeAreaRect();
                return m_safeAreaRect;
            }
        }
        public Rect antiSafeAreaRect
        {
            get
            {
                if (!(m_antiSafeAreaRect.x < 0)) return m_antiSafeAreaRect;

                UpdateSafeAreaRect();
                return m_antiSafeAreaRect;
            }
        }

        private int[] lastScreenResolution = {0, 0};
        public UnityEvent<int[]> onScreenResolutionChanged = new();

        public void Init()
        {
            lastScreenResolution = new[] {Screen.width, Screen.height};

            isInit = true;
        }
        
        private void UpdateSafeAreaRect()
        {
            float width = Mathf.Min(safeAreaInsetWidthNormalized, maxSafeAreaInsetWidthInPixel);
            m_safeAreaRect = new Rect(width, 0, 1 - width, 1);
            float antiWidth = -width / (1f - 2f * width);
            m_antiSafeAreaRect = new Rect(antiWidth, 0, 1 - antiWidth, 1);
        }

        public void Update()
        {
            if (lastScreenResolution[0] == Screen.width || lastScreenResolution[1] == Screen.height) return;

            int[] resolution = {lastScreenResolution[0], lastScreenResolution[1]};
            lastScreenResolution = new[] {Screen.width, Screen.height};
            UpdateSafeAreaRect();
            onScreenResolutionChanged.Invoke(resolution);
        }
    }
}