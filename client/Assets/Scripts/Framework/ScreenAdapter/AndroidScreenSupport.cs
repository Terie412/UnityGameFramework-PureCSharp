using UnityEngine;

namespace Framework.ScreenAdapter
{
    public class AndroidScreenSupport : SingleTon<AndroidScreenSupport>
    {
        public enum AndroidPhoneType
        {
            NONE,

            // 华为
            HUAWEI,

            // 小米
            XIAOMI,

            //oppo
            OPPO,

            //vivo
            VIVO,

            // 三星
            SAMSUNG,

            MAX,
        }

        public const int NOTCH_IN_SCREEN_VIVO_MARK = 0x00000020; //是否有凹槽
        public const int ROUNDED_IN_SCREEN_VIVO_MARK = 0x00000008; //是否有圆角
        private const int SAMSUNG_COCKTAIL_PANEL = 7;

        #region 属性

        private int m_CurAndroidSDKVersion = -1; // 当前的 SDK 版本
        public int curAndroidSDKVersion
        {
            get
            {
                if (m_CurAndroidSDKVersion >= 0) return m_CurAndroidSDKVersion;

                using AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION");
                m_CurAndroidSDKVersion = version.GetStatic<int>("SDK_INT");
                return m_CurAndroidSDKVersion;
            }
        }
        private AndroidPhoneType m_CurAndroidPhoneType = AndroidPhoneType.MAX; // 当前的设备品牌
        public AndroidPhoneType curAndroidPhoneType
        {
            get
            {
                if (m_CurAndroidPhoneType != AndroidPhoneType.MAX) return m_CurAndroidPhoneType;

                m_CurAndroidPhoneType = AndroidPhoneType.NONE;
                string phoneUpperModel = SystemInfo.deviceModel.ToUpper();

                for (int i = (int) AndroidPhoneType.NONE + 1; i < (int) AndroidPhoneType.MAX; i++)
                {
                    AndroidPhoneType current = (AndroidPhoneType) i;
                    if (!phoneUpperModel.Contains(current.ToString())) continue;

                    m_CurAndroidPhoneType = current;
                    break;
                }

                return m_CurAndroidPhoneType;
            }
        }

        public int screenWidth = -1; // 分辨率
        public int screenHeight = -1;
        private float screenRatio => screenWidth * 1.0f / screenHeight;

        private bool m_IsSafeAreaSupported;
        public bool isSafeAreaSupported
        {
            get
            {
                m_IsSafeAreaSupported = HasSafeAreaInset(out m_SafeAreaInsetWidthNormalized);
                return m_IsSafeAreaSupported;
            }
        }
        private float m_SafeAreaInsetWidthNormalized = -1;
        public float safeAreaInsetWidthNormalized // 安全区边侧的宽度
        {
            get
            {
                m_IsSafeAreaSupported = HasSafeAreaInset(out m_SafeAreaInsetWidthNormalized);
                return m_SafeAreaInsetWidthNormalized;
            }
        }
        private bool isSafeAreaChecked;

        #endregion

        #region 方法

        public void SetScreenSize(int width, int height)
        {
            if (screenWidth >= 0) return;
            screenWidth = width;
            screenHeight = height;
        }

        public bool HasSafeAreaInset(out float edge)
        {
            if (isSafeAreaChecked)
            {
                edge = m_SafeAreaInsetWidthNormalized;
                return m_IsSafeAreaSupported;
            }

            edge = 0;
            m_SafeAreaInsetWidthNormalized = edge;
            isSafeAreaChecked = true;

            if (curAndroidSDKVersion >= 28)
            {
                m_IsSafeAreaSupported = HasSafeAreaInset_AndroidP(out edge);
                m_SafeAreaInsetWidthNormalized = edge;

                if (m_IsSafeAreaSupported)
                {
                    return m_IsSafeAreaSupported;
                }
            }

            AndroidPhoneType phoneType = curAndroidPhoneType;
            m_IsSafeAreaSupported = phoneType switch
            {
                AndroidPhoneType.XIAOMI => HasSafeAreaInset_XIAOMI(out edge),
                AndroidPhoneType.HUAWEI => HasSafeAreaInset_Huawei(out edge),
                AndroidPhoneType.VIVO => HasSafeAreaInset_Vivo(out edge),
                AndroidPhoneType.OPPO => HasSafeAreaInset_Oppo(out edge),
                AndroidPhoneType.SAMSUNG => HasSafeAreaInset_Samsung(out edge),
                _ => m_IsSafeAreaSupported
            };

            if (!m_IsSafeAreaSupported)
            {
                m_IsSafeAreaSupported = HasSafeAreaInset_Notch(out edge);
            }

            m_SafeAreaInsetWidthNormalized = edge;
            return true;
        }

        private bool HasSafeAreaInset_AndroidP(out float edge)
        {
            edge = 0;

            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        using (AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow"))
                        {
                            using (AndroidJavaObject decorView = window.Call<AndroidJavaObject>("getDecorView"))
                            {
                                using (AndroidJavaObject windowInsets = decorView.Call<AndroidJavaObject>("getRootWindowInsets"))
                                {
                                    AndroidJavaObject displayCutout = windowInsets.Call<AndroidJavaObject>("getDisplayCutout");
                                    if (displayCutout != null)
                                    {
                                        int notchHeight = Mathf.Max(displayCutout.Call<int>("getSafeInsetLeft"), displayCutout.Call<int>("getSafeInsetRight"));
                                        edge = notchHeight * 1.0f / screenWidth;
                                        displayCutout.Dispose();
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Android P hasNotch occurred error: " + e);
            }

            return false;
        }

        private bool HasSafeAreaInset_XIAOMI(out float edge)
        {
            edge = 0;

            try
            {
                using AndroidJavaClass jo = new AndroidJavaClass("android/os/SystemProperties");
                string hasNotch = jo.CallStatic<string>("get", "ro.miui.notch");
                if (hasNotch != "1")
                {
                    return true;
                }

                using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
                using AndroidJavaObject resources = context.Call<AndroidJavaObject>("getResources");

                // MIUI 10 支持动态获取Notch高度
                float notchHeight = 89;
                int resourceId = resources.Call<int>("getIdentifier", "notch_height", "dimen", "android");
                if (resourceId > 0)
                {
                    notchHeight = resources.Call<int>("getDimensionPixelSize", resourceId);
                }
                else
                {
                    resourceId = resources.Call<int>("getIdentifier", "status_bar_height", "dimen", "android");
                    if (resourceId > 0)
                    {
                        notchHeight = resources.Call<int>("getDimensionPixelSize", resourceId);
                        if (notchHeight > 100)
                        {
                            notchHeight -= 20;
                        }
                    }
                }

                edge = notchHeight * 1.0f / screenWidth;

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("MI hasNotch occurred error: " + e);
            }

            return false;
        }

        private bool HasSafeAreaInset_Huawei(out float edge)
        {
            edge = 0;
            try
            {
                using (AndroidJavaClass jo = new AndroidJavaClass("com.huawei.android.util.HwNotchSizeUtil"))
                {
                    bool hasNotchInScreen = jo.CallStatic<bool>("hasNotchInScreen");
                    int[] notchSize = jo.CallStatic<int[]>("getNotchSize");

                    edge = notchSize[1] * 1.0f / screenWidth;
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Huawei hasNotch occurred error: " + e);
            }

            return false;
        }

        private bool HasSafeAreaInset_Vivo(out float edge)
        {
            edge = 0;
            try
            {
                using (AndroidJavaClass jo = new AndroidJavaClass("android.util.FtFeature"))
                {
                    bool hasNotchInScreen = jo.CallStatic<bool>("isFeatureSupport", NOTCH_IN_SCREEN_VIVO_MARK);
                    //                bool hasRoundInScreen = jo.CallStatic<bool>("isFeatureSupport", ROUNDED_IN_SCREEN_VOIO_MARK);

                    if (hasNotchInScreen)
                    {
                        edge = 80 * 1.0f / screenWidth;
                    }

                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Vivo hasNotch occurred error: " + e);
            }

            return false;
        }

        private bool HasSafeAreaInset_Oppo(out float edge)
        {
            edge = 0;

            try
            {
                using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
                using AndroidJavaObject manager = activity.Call<AndroidJavaObject>("getPackageManager");
                using AndroidJavaObject resources = context.Call<AndroidJavaObject>("getResources");

                bool hasNotchInScreen = manager.Call<bool>("hasSystemFeature", "com.oppo.feature.screen.heteromorphism");
                int resourceId = resources.Call<int>("getIdentifier", "status_bar_height", "dimen", "android");
                int notchHeight = 80;
                if (resourceId > 0)
                {
                    notchHeight = resources.Call<int>("getDimensionPixelSize", resourceId);
                }

                if (!hasNotchInScreen)
                {
                    if (screenRatio > 2.1f)
                    {
                        hasNotchInScreen = true;
                    }
                }

                if (hasNotchInScreen)
                {
                    edge = notchHeight * 1.0f / screenWidth;
                }

                return hasNotchInScreen;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Oppo hasNotch occurred error: " + e);
            }

            return false;
        }

        private bool HasSafeAreaInset_Samsung(out float edge)
        {
            edge = 0;

            try
            {
                using AndroidJavaClass jo = new AndroidJavaClass("com.samsung.android.sdk.look.SlookImpl");
                bool hasNotchInScreen = jo.CallStatic<bool>("isFeatureEnabled", SAMSUNG_COCKTAIL_PANEL);
                if (hasNotchInScreen)
                {
                    edge = 0.03612f; // 88.0f / 2436
                }

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Samsung hasNotch occurred error: " + e);
            }

            return false;
        }

        /// <summary>
        /// https://source.android.com/devices/tech/display/display-cutouts
        /// status_bar_height_portrait: In most devices, this defaults to 24dp. When there is a cutout, set this value to the height of the cutout.
        /// Can optionally be taller than the cutout if desired.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private bool HasSafeAreaInset_Notch(out float edge)
        {
            edge = 0;

            try
            {
                using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
                using AndroidJavaObject resources = context.Call<AndroidJavaObject>("getResources");
                using AndroidJavaObject dm = resources.Call<AndroidJavaObject>("getDisplayMetrics");

                var scale = dm.Get<float>("density");
                var resourceId = resources.Call<int>("getIdentifier", "status_bar_height", "dimen", "android");
                if (resourceId > 0)
                {
                    var notchHeight = resources.Call<int>("getDimensionPixelSize", resourceId);

                    if (scale > 1 && notchHeight / scale > 25)
                    {
                        edge = notchHeight * 1.0f / screenWidth;
                        return true;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Check statusBar height occurred error: " + e);
            }

            return false;
        }

        #endregion
    }
}