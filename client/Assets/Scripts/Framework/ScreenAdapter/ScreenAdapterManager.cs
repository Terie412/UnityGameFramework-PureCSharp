using System.Threading.Tasks;
using UnityEngine;

namespace Framework.ScreenAdapter
{
    public class ScreenAdapterManager : SingleTon<ScreenAdapterManager>
    {
        // 该Canvas的模式为 ScreenSpace，大小用来指代当前屏幕分辨率（对于编辑器下，Screen.width/height 不能取到正确的分辨率）
        // 在 Unity 2021 上已经可以正确获取到当前 GameView 设置的分辨率
        // public RectTransform ReferenceCanvasRT; 
        
        // public ScreenSize screenSize; // 屏幕分辨率
        public float aspectRatio => (float) Screen.width / Screen.height; // 屏幕分辨率比值
        private const float maxSafeAreaInsetWidthInPixel = 100; // 最大的安全区侧边宽度大小，像素单位
        public readonly float referenceAspectRatio = 2.0f; // 参考的宽高比。想开发相机效果的时候，屏幕分辨率的比例应当是这个比例，而在实际的运行时，相机的FOV会参考该分辨率进行调整

        private float safeAreaInsetWidthNormalized // 归一化的安全区侧边宽度，为安全区侧边的实际宽度:屏幕宽度
        {
            get
            {
#if UNITY_EDITOR
                // return IPhoneSimulator.Instance.safeAreaInsetWidthNormalized;
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

        public async Task Init()
        {
            // if (ReferenceCanvasRT != null) return;

            // var go = GameObject.Find("ReferenceCanvas");
            // if (go == null)
            // {
                // go = await AssetManager.Instance.LoadAndInstantiateGameObjectAsync("ReferenceCanvas", null);
            // }

            // ReferenceCanvasRT = go.GetComponent<RectTransform>();
            // UpdateScreenSize();
            
#if UNITY_EDITOR
            // await IPhoneSimulator.Instance.Init(GetCurrentScreenSize());
#elif UNITY_ANDROID
            AndroidScreenSupport.Instance.SetScreenSize(Screen.width, Screen.height);
#elif UNITY_IOS
#else
#endif
        }

        private void UpdateSafeAreaRect()
        {
            float width = Mathf.Min(safeAreaInsetWidthNormalized, maxSafeAreaInsetWidthInPixel);
            m_safeAreaRect = new Rect(width, 0, 1 - width, 1);
            float antiWidth = -width / (1f - 2f * width);
            m_antiSafeAreaRect = new Rect(antiWidth, 0, 1 - antiWidth, 1);
        }

        /// 获取最新的屏幕分辨率
        // public ScreenSize GetCurrentScreenSize()
        // {
        //     if (IsScreenSizeChanged())
        //     {
        //         UpdateScreenSize();
        //     }
        //
        //     return screenSize;
        // }

        /// 判断当前屏幕分辨率是否发生改变
        // public bool IsScreenSizeChanged()
        // {
        //     Debug.Assert(ReferenceCanvasRT != null, this);
        //
        //     var rect = ReferenceCanvasRT.rect;
        //     var curScreenSize = new ScreenSize((int) rect.width, (int) rect.height);
        //     return curScreenSize.Equals(screenSize);
        // }

        /// 更新当前屏幕分辨率
        // public void UpdateScreenSize()
        // {
        //     Debug.Assert(ReferenceCanvasRT != null, this);
        //     var rect = ReferenceCanvasRT.rect;
        //     screenSize = new ScreenSize((int) rect.width, (int) rect.height);
        // }
    }
}