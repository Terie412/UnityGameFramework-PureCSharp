using System.Collections.Generic;
using UnityEngine;

namespace QTC.Modules.UI
{
    public class UIManager: SingleTon<UIManager>
    {
        private bool isInit = false;
        private Queue<LoadWrap> loadingQueue;
        private Stack<UIBase> mainUIStack;
        private Stack<UIBase> upperUIStack;
        private Stack<UIBase> systemUIStack;

        private const string UIROOT_NAME = "UIRoot";
        
        public enum LoadState
        {
            loading,
            loaded,
            dead
        }

        public UIManager()
        {
            
        }
        
        public class LoadWrap
        {
            public LoadState state = LoadState.loading;
            public UIBase parent;
            public object[] args;
        }

        public async void InitAsync()
        {
            GameLogger.Info("开始加载");
            GameObject uiroot = await AssetManager.Instance.LoadAssetAsync(UIROOT_NAME, null) as GameObject;
            GameLogger.Info($"成功加载: {uiroot.name}");

            GameLogger.Info("再次加载");
            uiroot = await AssetManager.Instance.LoadAssetAsync(UIROOT_NAME, null) as GameObject;
            GameLogger.Info($"再次成功加载: {uiroot.name}");
            GameObject.Instantiate(uiroot);
        }
        
        public void Init()
        {
            GameLogger.Info("开始加载");
            AssetManager.Instance.LoadAssetAsync<GameObject>(UIROOT_NAME, null, o =>
            {
                GameLogger.Info($"成功加载: {o.name}");
                
                GameLogger.Info("再次加载");
                AssetManager.Instance.LoadAssetAsync<GameObject>(UIROOT_NAME, null, o2 =>
                {
                    GameLogger.Info($"再次成功加载: {o2.name}");
                    GameObject.Instantiate(o2);
                });
            });

            
        }
    }
    
    
}


