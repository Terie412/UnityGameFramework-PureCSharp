using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework
{
    // 仅仅是用来打印信息的
    public struct AssetBundleWrapInfo
    {
        public string       assetBundleName;
        public List<string> deps;
        public List<string> abRefs;
        public List<string> objRefs;
    }

    public class AssetWrap
    {
        public string assetName { get; }
        public string assetFullName { get; }
        public Action<Object> onLoaded;

        private Object             syncAsset; // 同步加载的结果。和下面的 request 是互斥的
        private AssetBundleRequest request;   // 异步请求的结果，和上面的 syncAsset 是互斥的
        public Object loadedAsset => syncAsset != null ? syncAsset : request?.asset;

        public bool isDone => syncAsset || (request != null && request.isDone);

        public AssetWrap(string assetName, string assetFullName, AssetBundleRequest request, Object syncAsset, Action<Object> onLoaded)
        {
            this.assetName     = assetName;
            this.assetFullName = assetFullName;
            this.onLoaded      = onLoaded;
            this.request       = request;
            this.syncAsset     = syncAsset;
        }
    }
}