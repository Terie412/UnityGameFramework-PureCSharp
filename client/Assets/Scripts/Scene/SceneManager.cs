using System;
using System.Threading.Tasks;
using QTC.Modules.UI;
using UnityEngine;

public class SceneManager : SingleTon<SceneManager>
{
    public class WindowInfo
    {
        public string wndName;
        public object[] param;
    }
    
    public class LoadParam
    {
        public object[] sceneParams;
        public string[] preloadList;
        public string loadingEffectName;
        public bool ignoreSameSceneCheck = false;
        public Action onLoaded;
    }

    public Camera mainCamera;
    private SceneBase curSceneType = null;

    public async Task LoadAsync(SceneBase sceneType, LoadParam param)
    {
        if (curSceneType != null)
        {
            GameLogger.Error("【场景切换】切换场景失败：当前正在切换场景");
            return;
        }

        if (!param.ignoreSameSceneCheck && curSceneType != null && curSceneType == sceneType)
        {
            GameLogger.Info($"【场景切换】切换场景失败：已经处于目标场景: {sceneType.name}");
            return;
        }

        await StartLoad(sceneType, param);
    }

    private async Task StartLoad(SceneBase sceneType, LoadParam param)
    {
        // 状态变更
        curSceneType = null;
        mainCamera = null;
        
        GameLogger.Info($"【场景切换】场景类初始化");
        curSceneType.Init(param.sceneParams);

        await OpenAndWaitForSecondStage(param.loadingEffectName == null ? sceneType.loadingEffectName : param.loadingEffectName);
        
        
    }

    private async Task OpenAndWaitForSecondStage(string effectName)
    {
        GameObject go = await LoadingEffectManager.Instance.LoadAsync(effectName);
        ISceneLoadingEffect effect = go.GetComponent<ISceneLoadingEffect>();
        await effect.WaitForEnterSecondStage();
    }
}