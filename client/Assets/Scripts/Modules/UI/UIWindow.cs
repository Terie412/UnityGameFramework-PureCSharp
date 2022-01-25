namespace QTC.Modules.UI
{
    public class UIWindow : UIBase
    {
        public E_WINDOW_TYPE sysType;
    
        public virtual void OnFocus() { }
        public virtual void OnLostFocus() { }
        public virtual void OnOpen(params object[] args){}
        public void Close()
        {
            UIManager.Instance.CloseWindow(this);
        }
    }

    public enum E_WINDOW_TYPE
    {
        Main,
        Upper,
        System
    }
}
