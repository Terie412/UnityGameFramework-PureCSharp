using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class AssetBundleWrap
{
    public string assetBundleName;
    public string assetBundleFullName;
    public Action<AssetBundleWrap> onLoaded;

    public List<AssetBundleWrap> deps; // 依赖的AB列表，加载时，这个列表需要都被先加载完成
    public List<AssetBundleWrap> abRefs; // 被该列表的AB依赖，与 deps 意思相反，在卸载时，这个列表要为空
    public List<Object> objRefs; // 被该列表的Object依赖，在卸载时，这个列表要为空

    public AssetBundleCreateRequest request; // 存放异步加载的产物

    public AssetBundle syncAB; // 存放同步加载的产物
    
    public bool isDone
    {
        get
        {
            if (syncAB == null && (request == null || !request.isDone))
            {
                return false;
            }

            return deps.All(dep => dep.isDone);
        }
    }

    public int refCount
    {
        get
        {
            abRefs.RemoveAll(abRef => abRef.refCount == 0);
            objRefs.RemoveAll(obj => obj == null);

            return abRefs.Count + objRefs.Count;
        }
    }

    public AssetBundleWrap(string assetBundleName, string assetBundleFullName, Action<AssetBundleWrap> onLoaded)
    {
        this.assetBundleName = assetBundleName;
        this.assetBundleFullName = assetBundleFullName;
        this.onLoaded = onLoaded;

        deps = new List<AssetBundleWrap>();
        abRefs = new List<AssetBundleWrap>();
        objRefs = new List<Object>();
    }

    public void Load()
    {
        if (request != null)
        {
            return;
        }

        request = AssetBundle.LoadFromFileAsync(assetBundleFullName);
    }

    public void UnLoad()
    {
        request.assetBundle.Unload(true);
    }

    public T LoadAsset<T>(string assetName, string assetFullName, Object objRef) where T : Object
    {
        if (syncAB == null)
        {
            Debug.LogError("Synchronously load a asset while assetbundle is not prepared!");
            return null;
        }

        return syncAB.LoadAsset<T>(assetFullName);
    }

    public AssetWrap LoadAssetAsync<T>(string assetName, string assetFullName, Object objRef, Action<T> onLoaded2) where T : Object
    {
        if (request == null || request.assetBundle == null)
        {
            Debug.LogError("AssetBundleWrap.LoadAssetAsync while assetBundleCreateRequest is null or not completed!");
            return null;
        }

        AssetBundleRequest assetRequest = request.assetBundle.LoadAssetAsync<T>(assetFullName);
        var assetWrap = new AssetWrap(assetName, assetFullName, obj =>
        {
            var asset = obj as T;
            if (asset == null)
            {
                Debug.LogError($"Asset({obj.name}) fail to be converted to type {typeof(T).Name}");
                return;
            }

            onLoaded2(asset);

            if (objRef != null)
            {
                objRefs.Add(objRef);
            }
        });

        assetWrap.request = assetRequest;

        return assetWrap;
    }
}