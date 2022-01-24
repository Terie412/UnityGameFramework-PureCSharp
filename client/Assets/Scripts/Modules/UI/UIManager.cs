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
            GameObject uiroot = await AssetManager.Instance.LoadAssetAsync<GameObject>(UIROOT_NAME, null);
            GameLogger.Info($"成功加载: {uiroot.name}");

            GameLogger.Info("再次加载");
            uiroot = await AssetManager.Instance.LoadAssetAsync<GameObject>(UIROOT_NAME, null);
            GameLogger.Info($"再次成功加载: {uiroot.name}");
            GameObject.Instantiate(uiroot);
        }
    }
}


