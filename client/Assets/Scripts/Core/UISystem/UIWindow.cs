namespace Core
{
    public class UIWindow : UIBase
    {
        public enum E_WINDOW_TYPE
        {
            FullScreenEffect,
            System,
            Guide,
            Upper,
            Main,
            HUD,
        }

        public E_WINDOW_TYPE sysType = E_WINDOW_TYPE.Main;
        
        public virtual void OnOpen(params object[] args) { }
        public virtual void OnFocus() { }
        public virtual void OnLostFocus() { }

        public void Close()
        {
            UIManager.Instance.CloseWindow(this);
        }
    }
}