using System.Threading.Tasks;
using UnityEngine;

public class ScreenAdapterManager: SingleTon<ScreenAdapterManager>
{
    public struct ScreenSize
    {
        public int width;
        public int height;

        public ScreenSize(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
        
        public bool Equals(ScreenSize other)
        {
            return width == other.width && height == other.height;
        }
    }
    
    public RectTransform ReferenceCanvasRT; // 该Canvas的模式为 ScreenSpace，大小用来指代当前屏幕分辨率（对于编辑器下，Screen.width/height 不能取到正确的分辨率）
    public ScreenSize screenSize;

    public async Task Init()
    {
        if (ReferenceCanvasRT != null) return;
        
        var go = GameObject.Find("ReferenceCanvas");
        if (go == null)
        {
            go = await AssetManager.Instance.LoadAndInstantiateGameObjectAsync("ReferenceCanvas", null);
        }

        ReferenceCanvasRT = go.GetComponent<RectTransform>();
        UpdateScreenSize();

#if UNITY_EDITOR
        
#elif UNITY_ANDROID
        AndroidScreenSupport.Instance.SetScreenSize(screenSize.width, screenSize.height);
#elif UNITY_IOS

#else

#endif
    }
    
    /// 判断当前屏幕分辨率是否发生改变
    public bool IsScreenSizeChanged()
    {
        Debug.Assert(ReferenceCanvasRT != null, this);
        
        var rect = ReferenceCanvasRT.rect;
        var curScreenSize = new ScreenSize((int) rect.width, (int) rect.height);
        return curScreenSize.Equals(screenSize);
    }

    /// 更新当前屏幕分辨率
    public void UpdateScreenSize()
    {
        Debug.Assert(ReferenceCanvasRT != null, this);
        var rect = ReferenceCanvasRT.rect;
        screenSize = new ScreenSize((int) rect.width, (int) rect.height);
    }
}