using System;
using Framework;
using UnityEngine;

public class SceneBase: SingleTon<SceneBase>
{
    public string name = "SceneBase";
    public string loadingEffectName = "CommonProgressEffect";
    public virtual void Init(params object[] param){}
    public virtual void OnUnitySceneLoaded(Action callback){}
    public virtual void OnPreloaded(Action callback){}
    public virtual void OnWindowStackOpen(Action callback){}
    public virtual void Final(){}
    public virtual Camera GetMainCamera() { return null; }
    public virtual void Reload(){}
}