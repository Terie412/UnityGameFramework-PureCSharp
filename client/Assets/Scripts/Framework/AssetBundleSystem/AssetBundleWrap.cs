using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework
{
    public class AssetBundleWrap
    {
        public readonly string                  assetBundleName;
        public readonly string                  assetBundleFullName;
        public          Action<AssetBundleWrap> onLoaded;

        public List<AssetBundleWrap> deps;    // 依赖的AB列表，加载时，这个列表需要都被先加载完成
        public List<AssetBundleWrap> abRefs;  // 被该列表的AB依赖，与 deps 意思相反，在卸载时，这个列表要为空
        public List<Object>          objRefs; // 被该列表的Object依赖，在卸载时，这个列表要为空

        private AssetBundle              syncAB;  // 存放同步加载的产物，和 request 是互斥的
        private AssetBundleCreateRequest request; // 存放异步加载的产物，和 syncAB 是互斥的

        public AssetBundle loadedAssetBundle => syncAB != null ? syncAB : request?.assetBundle;

        // 当且仅当所有的依赖AB都加载完成，当前的AB才算加载完成
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
            this.assetBundleName     = assetBundleName;
            this.assetBundleFullName = assetBundleFullName;
            this.onLoaded            = onLoaded;

            deps    = new List<AssetBundleWrap>();
            abRefs  = new List<AssetBundleWrap>();
            objRefs = new List<Object>();
        }

        public void LoadAsync()
        {
            if (loadedAssetBundle != null)
            {
                return;
            }

            GameLogger.Log($"Start LoadAsync AssetBundleAsync {assetBundleName}");
            request = AssetBundle.LoadFromFileAsync(assetBundleFullName);
        }

        public void Load()
        {
            if (loadedAssetBundle != null)
            {
                return;
            }

            GameLogger.Log($"Start Load AssetBundleAsync {assetBundleName}");
            syncAB = AssetBundle.LoadFromFile(assetBundleFullName);
        }

        public void UnLoad()
        {
            // 这里要把同步和异步的资源都卸载掉
            loadedAssetBundle.Unload(true);
        }

        public AssetWrap LoadAsset<T>(string assetName, string assetFullName, Object objRef) where T : Object
        {
            if (loadedAssetBundle == null)
            {
                Debug.LogError("Synchronously load a asset while assetbundle is not prepared!");
                return null;
            }

            var asset     = loadedAssetBundle.LoadAsset<T>(assetFullName);
            var assetWrap = new AssetWrap(assetName, assetFullName, null, asset, _ => { });
            return assetWrap;
        }

        public AssetWrap LoadAssetAsync<T>(string assetName, string assetFullName, Object objRef, Action<T> onLoaded2) where T : Object
        {
            if (loadedAssetBundle == null)
            {
                Debug.LogError("AssetBundleWrap.LoadAssetAsync while assetBundle is not prepared!");
                return null;
            }

            AssetBundleRequest assetRequest = loadedAssetBundle.LoadAssetAsync<T>(assetFullName);
            var assetWrap = new AssetWrap(assetName, assetFullName, assetRequest, null, obj =>
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

            return assetWrap;
        }
    }
}