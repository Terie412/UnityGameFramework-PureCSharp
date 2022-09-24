using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework
{
// 仅仅是用来打印信息的
    public struct AssetBundleWrapInfo
    {
        public string assetBundleName;
        public List<string> deps;
        public List<string> abRefs;
        public List<string> objRefs;
    }

    public class AssetWrap
    {
        public readonly string assetName;
        public readonly string assetFullName;
        public Action<Object> onLoaded;
        public AssetBundleRequest request;

        public bool isDone => request != null && request.isDone;

        public AssetWrap(string assetName, string assetFullName, Action<Object> onLoaded)
        {
            this.assetName = assetName;
            this.assetFullName = assetFullName;
            this.onLoaded = onLoaded;
        }
    }
}