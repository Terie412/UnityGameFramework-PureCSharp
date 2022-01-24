namespace QTC.Modules.UI
{
    public class UIWindow : UIBase
    {
        public E_WINDOW_TYPE sysType;
    
        public virtual void OnFocus() { }
        public virtual void OnLostFocus() { }
    }

    public enum E_WINDOW_TYPE
    {
        Main,
    }
}
