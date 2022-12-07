using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

// 在这个管理器中，我们有以下命名约定：
// fileFullName =         "Assets/Content/Environment/Prefabs/prefab_cube.prefab"       是指带后缀的文件全路径
// fileName =             "prefab_cube.prefab"                                          是指带后缀的文件名
// filePath =             "Assets/Content/Environment/Prefabs"                          是指文件所在的目录路径
// assetName =            "prefab_cube"                                                 是指资源名，也同时是不带后缀的文件名
// assetFullName =	      "Assets/Content/Environment/Prefabs/prefab_cube.prefab"       与fileFullName，带后缀的文件全路径
// assetBundleName =	  "content_environment_prefabs"                                 AssetBundle名称，是经过转换的filePath
// assetBundleFullName =  "Assets/StreamingAssets/content_environment_prefabs"          AssetBundle全路径名称

// 打包策略里面，我们规定一个目录会被打成一个assetBundle，故目录下不能存在同名的资源（即使不同扩展名，不过绝大多数情况下都是按照同类型的资产来划分目录）
// assetBundle的名字由目录的路径唯一确定
// 上层只需要传文件名，即可加载出资源，由上层业务决定资源具体是什么类型。比如 icon.png 可以是 Sprite，也可以是 Texture2D

namespace Framework
{
    /// <summary>
    /// 基于AssetBundle的资产管理
    /// 该管理器提供了两种加载资产的模式，分别是走 AssetDataBase（仅编辑器下可用）和走 AssetBundle
    /// 走 AssetBundle 的模式需要先通过 AssetBundleBuilder 构建出 AssetBundle
    /// 无论是 AssetDataBase 还是 AssetBundle，规则都是统一的。其中最重要的规则是资产不能重名。
    /// 资产不能重名的目的是在资产命名时多花费一些心思，从而去除运行时加载资产时需要关心资产路径的麻烦
    ///
    /// 在这个管理器当中，有两个重要的对象：AssetBundleWrap 和 AssetWrap，分别是对 AssetBundle 和 Asset 的封装。
    /// 这是因为这两种对象的加载都有异步和同步两种方式，我们希望对 AssetManager 本身的接口屏蔽一些细节
    /// 原则上，AssetManager 不直接操作 AssetBundle 和 Asset，这些操作被封装到 AssetBundleWrap 和 AssetWrap 里。AssetManager 本身则直接操作 Wrap。
    ///
    /// 管理器允许上层同步API和异步API混用，但是需要上层自己关注其中可能会发生的问题，例如不能对一个正在异步加载的AssetBundle或Asset进行异步或同步加载
    /// 不能保证上一帧加载过并且缓存的资源这一帧还在，更不能因此保证这一帧的异步加载会同步返回，缓存的资源是不能保证一直存在的
    /// </summary>
    public class AssetManager : SingleTon<AssetManager>
    {
        #region 属性

        public readonly List<string> assetDirs = new() // 定义哪些目录的资产将会被打成AssetBundle作为合法的资产加载目录
        {
            "Assets/Content/Animations",
            "Assets/Content/AnimatorController",
            "Assets/Content/Audios",
            "Assets/Content/Fonts",
            "Assets/Content/Materials",
            "Assets/Content/Prefabs",
            "Assets/Content/ScriptableObjects",
            "Assets/Content/Shaders",
            "Assets/Content/Sprites",
            "Assets/Content/Others",
        };

        public readonly List<string> singleAssetDirs = new()
        {
            // "Assets/Content/Sprites/UI/Story"
        }; // 这里定义的目录下的所有文件，都会相应地单独打成一个包

        public readonly List<string> residentAssetDirs = new(); // 定义哪些目录下的资产将会常驻内存，不会被卸载掉

        private string m_ASSETBUNDLE_DIR;

        public string ASSETBUNDLE_DIR // AssetBundle的生成目录
        {
            get
            {
                if (string.IsNullOrEmpty(m_ASSETBUNDLE_DIR))
                    m_ASSETBUNDLE_DIR = Path.Combine(Application.streamingAssetsPath, "AssetBundles").Replace("\\", "/");
                return m_ASSETBUNDLE_DIR;
            }
        }

        //private AssetMode assetModeInEditor = AssetMode.AssetDataBase; // 指示编辑器使用AssetBundleMode，否则默认走AssetDataBase机制

        private AssetMode assetModeInEditor = AssetMode.AssetBundle;

        // 这里开始会建立一些名称的映射关系，其中，assetName是上层代码用来索引资源的标识符，因为我们规定了打进包的资源是不能重名的
        // assetFullName 用来从AssetBundle中加载资源
        // assetBundleName 是AssetBundle的标识符，由于我们AB的命名规则是按照目录全路径取名的，所以不会发生冲突
        // assetBundleFullName 用来从文件系统中加载AssetBundle
        public Dictionary<string, string> assetName_assetFullName;
        public Dictionary<string, string> assetName_assetBundleName;
        public Dictionary<string, string> assetBundleName_assetBundleFullName;
        public Dictionary<string, bool>   assetBundleName_resident; // 标识哪些AssetBundle是常驻内存的

        private Queue<AssetBundleWrap>              assetBundlesToLoad;                  // 准备被加载的AssetBundle队列
        private Dictionary<string, AssetBundleWrap> assetBundleName_assetBundleToLoad;   // 与 assetBundlesToLoad 对应，索引结构表示
        private Dictionary<string, AssetBundleWrap> assetBundleName_loadingAssetBundle;  // 正在加载的AssetBundle
        private Dictionary<string, AssetBundleWrap> assetBundleName_loadedAssetBundle;   // 加载完成的AssetBundle
        private Dictionary<string, AssetBundleWrap> assetBundleName_assetBundleToRemove; // 准备卸载的AssetBundle
        private Dictionary<string, AssetWrap>       assetName_loadingAsset;              // 正在加载的Asset
        private Dictionary<string, AssetWrap>       assetName_loadedAsset;               // 缓存完成加载的Asset，注意这里能够被缓存的资产原则上应当是只读的，但是这里没有做限制，需要上层注意

        private AssetBundleManifest assetBundleManifest; // 资源的依赖关系

        public int MAX_ASSETBUNDLE_LOAD_PER_FRAME = 32;

        public bool isInit = false; // 指示资源管理器是否初始化过

        private Dictionary<string, List<string>> duplicatedAssetName_assetFullNameList; // Debug属性。用于在 InitAssetNameMap() 的时候输出哪些资源出现了冗余

        #endregion

        #region public

        public async UniTask InitAsync()
        {
            if (isInit)
            {
                GameLogger.LogError($"Do not init AssetManager twice!");
                return;
            }

            // 初始化一系列名字的映射关系，这个映射关系体现了当前工程的打包策略
            await InitAssetNameMap();
            assetBundlesToLoad                  = new Queue<AssetBundleWrap>();
            assetBundleName_assetBundleToLoad   = new();
            assetBundleName_loadingAssetBundle  = new Dictionary<string, AssetBundleWrap>();
            assetBundleName_loadedAssetBundle   = new Dictionary<string, AssetBundleWrap>();
            assetBundleName_assetBundleToRemove = new Dictionary<string, AssetBundleWrap>();
            assetName_loadingAsset              = new Dictionary<string, AssetWrap>();
            assetName_loadedAsset               = new();
            keysToRemove                        = new List<string>();

            if (!Application.isEditor)
            {
                var assetBundle = AssetBundle.LoadFromFile(assetBundleName_assetBundleFullName[assetName_assetBundleName["AssetBundleManifest"]]);
                assetBundleManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
            else
            {
                if (assetModeInEditor == AssetMode.AssetBundle)
                {
                    var assetBundle = AssetBundle.LoadFromFile(assetBundleName_assetBundleFullName[assetName_assetBundleName["AssetBundleManifest"]]);
                    assetBundleManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                }
            }

            // 在内存不足的时候，清理所有的缓存的资源
            Application.lowMemory += () =>
            {
                UnloadAllLoadedAsset();
            };

            isInit = true;
        }
        
        /// 同步加载资产
        public T LoadAsset<T>(string assetName, Object objRef) where T : Object
        {
#if UNITY_EDITOR
            if (assetModeInEditor == AssetMode.AssetDataBase)
            {
                if (!assetName_assetFullName.TryGetValue(assetName, out var fullName))
                {
                    return null;
                }

                return AssetDatabase.LoadAssetAtPath<T>(fullName);
            }
#endif

            // 非法资源
            if (!assetName_assetBundleName.TryGetValue(assetName, out var assetBundleName) || !assetName_assetFullName.TryGetValue(assetName, out var assetFullName))
            {
                GameLogger.LogError($"Failed to find asset, assetName = {assetName}");
                return null;
            }

            // 缓存了的资源
            if (assetName_loadedAsset.TryGetValue(assetName, out var loadedAssetWrap))
            {
                return loadedAssetWrap.loadedAsset as T;
            }

            // 不能同步加载一个正在异步加载的资源
            if (assetName_loadingAsset.TryGetValue(assetName, out var assetWrap))
            {
                GameLogger.LogError($"You are trying to load an asset synchronously while it has been loaded in asynchronous way");
                return null;
            }

            // 资源没有加载出来，但是资源所在的AssetBundle已经加载出来了
            if (!assetBundleName_loadedAssetBundle.TryGetValue(assetBundleName, out var assetBundleWrap))
            {
                assetBundleWrap = LoadAssetBundle(assetBundleName);
            }

            // 资源和AssetBundle都没有加载出来
            if (assetBundleWrap == null)
            {
                GameLogger.LogError($"Fail to load assetBundle: {assetBundleName}");
                return null;
            }

            assetBundleWrap.Load();
            if (assetBundleWrap.loadedAssetBundle == null)
            {
                return null;
            }

            assetWrap = assetBundleWrap.LoadAsset<T>(assetName, assetFullName, objRef);
            assetName_loadedAsset[assetName] = assetWrap;
            return assetWrap.loadedAsset as T;
        }

        /// 同步加载和实例化GameObject
        public GameObject LoadAndInstantiateGameObject(string assetName, Object objRef)
        {
            var go = LoadAsset<GameObject>(assetName, objRef);
            if (go == null)
            {
                return null;
            }

            return Object.Instantiate(go);
        }

        /// 异步加载资产
        public AssetWrap LoadAssetAsync<T>(string assetName, Object objRef, Action<T> onLoaded) where T : Object
        {
#if UNITY_EDITOR
            if (assetModeInEditor == AssetMode.AssetDataBase)
            {
                onLoaded(LoadAsset<T>(assetName, objRef));
                return null;
            }
#endif
            // GameLogger.Log($"LoadAssetAsync 111 {assetName}");
            // 非法资源
            if (!assetName_assetBundleName.TryGetValue(assetName, out var assetBundleName) || !assetName_assetFullName.TryGetValue(assetName, out var assetFullName))
            {
                GameLogger.LogError($"Failed to find asset, assetName = {assetName}");
                return null;
            }

            // GameLogger.Log($"LoadAssetAsync 222 {assetName}");
            // 资源已经完成加载
            if (assetName_loadedAsset.TryGetValue(assetName, out var loadedAssetWrap))
            {
                var asset = loadedAssetWrap.loadedAsset as T;
                if (asset == null)
                {
                    GameLogger.LogError($"Fail to convert asset({asset.name}) to type({typeof(T).Name})");
                    onLoaded(null);
                }
                else
                {
                    onLoaded(asset);
                }

                return loadedAssetWrap;
            }

            // GameLogger.Log($"LoadAssetAsync 333 {assetName}");
            // 资源已经在加载中啦，别催啦
            if (assetName_loadingAsset.TryGetValue(assetName, out var assetWrap))
            {
                assetWrap.onLoaded += obj =>
                {
                    var asset = obj as T;
                    if (asset == null)
                    {
                        GameLogger.LogError($"Fail to convert asset({asset.name}) to type({typeof(T).Name})");
                        return;
                    }

                    onLoaded(asset);
                };
                return assetWrap;
            }

            // GameLogger.Log($"LoadAssetAsync 444 {assetName}");
            // 资源没有在加载中，但是资源所在AssetBundle加载出来了
            if (assetBundleName_loadedAssetBundle.TryGetValue(assetBundleName, out var assetBundleWrap))
            {
                assetWrap                         = assetBundleWrap.LoadAssetAsync<T>(assetName, assetFullName, objRef, onLoaded);
                assetName_loadingAsset[assetName] = assetWrap;
                return assetWrap;
            }

            // GameLogger.Log($"LoadAssetAsync 555 {assetName}");
            // 资源没有在加载中，资源所在的AssetBundle也在加载中，别催啦
            if (assetBundleName_loadingAssetBundle.TryGetValue(assetBundleName, out var assetBundleWrap2))
            {
                assetBundleWrap2.onLoaded += assetBundle =>
                {
                    assetWrap                         = assetBundleWrap2.LoadAssetAsync<T>(assetName, assetFullName, objRef, onLoaded);
                    assetName_loadingAsset[assetName] = assetWrap;
                };
                return null;
            }

            // GameLogger.Log($"LoadAssetAsync 666 {assetName}");
            // 资源没有在加载中，资源所在的AssetBundle也在加载中，别催啦
            if (assetBundleName_assetBundleToLoad.TryGetValue(assetBundleName, out var assetBundleWrap3))
            {
                assetBundleWrap3.onLoaded += assetBundle =>
                {
                    assetWrap                         = assetBundleWrap3.LoadAssetAsync<T>(assetName, assetFullName, objRef, onLoaded);
                    assetName_loadingAsset[assetName] = assetWrap;
                };
                return null;
            }

            // GameLogger.Log($"LoadAssetAsync 777 {assetName}");
            // 资源没在加载，资源所在的AssetBundle也没在加载
            LoadAssetBundleAsync(assetBundleName, assetBundleWrap4 =>
            {
                assetWrap                         = assetBundleWrap4.LoadAssetAsync(assetName, assetFullName, objRef, onLoaded);
                assetName_loadingAsset[assetName] = assetWrap;
            });

            return null;
        }

        /// 异步加载资产
        public UniTask<T> LoadAssetAsync<T>(string assetName, Object objRef) where T : Object
        {
            var tcs = new UniTaskCompletionSource<T>();
            LoadAssetAsync<Object>(assetName, objRef, o => { tcs.TrySetResult(o as T); });

            return tcs.Task;
        }

        /// 这个接口会返回GameObject的一个实例，以及实例对应的Asset
        public void LoadAndInstantiateGameObjectAsync(string assetName, Transform parent, Action<GameObject[]> callback)
        {
            LoadAssetAsync<GameObject>(assetName, parent, go =>
            {
                var instance = Object.Instantiate(go, parent);
                instance.name = go.name;
                callback?.Invoke(new[] { go, instance });
            });
        }

        /// 返回实例化出来的GameObject
        public async UniTask<GameObject> LoadAndInstantiateGameObjectAsync(string assetName, Transform parent)
        {
            GameObject go       = await LoadAssetAsync<GameObject>(assetName, parent);
            var        instance = Object.Instantiate(go, parent);
            instance.name = go.name;
            return instance;
        }

        /// 清理所有未被引用的AssetBundle
        public void UnloadAllUnusedAssetBundle(bool includeResidentAsset = false)
        {
            List<string> keys = new();
            foreach (var keyValuePair in assetBundleName_loadedAssetBundle)
            {
                var wrap = keyValuePair.Value;
                // 清理所有引用计数为0的资产（根据 includeResidentAsset 判定是否绕过 Resident 资产）
                if ((includeResidentAsset || !assetBundleName_resident.ContainsKey(wrap.assetBundleName)) && wrap.refCount == 0)
                {
                    keys.Add(wrap.assetBundleName);
                    assetBundleName_assetBundleToRemove[wrap.assetBundleName] = wrap;
                }
            }

            foreach (var key in keys)
            {
                assetBundleName_loadedAssetBundle.Remove(key);
            }

            // 总感觉以后要对 ToRemove 做什么事情，现在就直接立即清空好了
            foreach (var keyValuePair in assetBundleName_assetBundleToRemove)
            {
                keyValuePair.Value.UnLoad();
            }

            assetBundleName_assetBundleToRemove.Clear();
        }

        /// 清理所有的缓存起来的资产
        public void UnloadAllLoadedAsset(bool includeResidentAsset = false)
        {
            assetName_loadedAsset.Clear();
        }

        /// 调试用，输出已经加载了AssetBundle信息，Json形式
        public string GetLoadedAssetBundlesInfo()
        {
            List<AssetBundleWrapInfo> infos = new();
            foreach (var keyValuePair in assetBundleName_loadedAssetBundle)
            {
                var wrap = keyValuePair.Value;
                var info = new AssetBundleWrapInfo();
                info.assetBundleName = wrap.assetBundleName;
                info.deps            = new List<string>();
                info.abRefs          = new List<string>();
                info.objRefs         = new List<string>();

                foreach (var dep in wrap.deps)
                {
                    info.deps.Add(dep.assetBundleName);
                }

                foreach (var abRef in wrap.abRefs)
                {
                    info.abRefs.Add(abRef.assetBundleName);
                }

                foreach (var objRef in wrap.objRefs)
                {
                    info.objRefs.Add(objRef.name);
                }

                infos.Add(info);
            }

            return JsonConvert.SerializeObject(infos, Formatting.Indented);
        }

        public void Update()
        {
            // 从预加载队列中取出指定数量的AssetBundle进行加载
            for (int i = 0; i < MAX_ASSETBUNDLE_LOAD_PER_FRAME; i++)
            {
                if (assetBundlesToLoad.Count == 0)
                    break;

                AssetBundleWrap wrap = assetBundlesToLoad.Dequeue();
                assetBundleName_assetBundleToLoad.Remove(wrap.assetBundleName);
                StartLoadAssetBundleWrapAsync(wrap);
            }

            // 遍历正在加载的AssetBundle列表，检查其是否加载完成，并从列表中移除掉所有加载完成的AssetBundle
            foreach (var keyValuePair in assetBundleName_loadingAssetBundle)
            {
                var wrap = keyValuePair.Value;
                if (wrap.isDone)
                {
                    keysToRemove.Add(keyValuePair.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                var wrap = assetBundleName_loadingAssetBundle[key];
                assetBundleName_loadedAssetBundle[wrap.assetBundleName] = wrap;
                assetBundleName_loadingAssetBundle.Remove(key);
                wrap.onLoaded?.Invoke(wrap);
            }

            keysToRemove.Clear();

            // 遍历所有正在加载的Asset，检查其是否加载完成，并从列表中移除掉所有加载完成的AssetBundle
            foreach (var keyValuePair in assetName_loadingAsset)
            {
                var assetWrap = keyValuePair.Value;
                if (assetWrap.isDone)
                {
                    keysToRemove.Add(assetWrap.assetName);
                }
            }

            foreach (var key in keysToRemove)
            {
                var assetWrap = assetName_loadingAsset[key];
                assetName_loadedAsset[assetWrap.assetName] = assetWrap;
                assetName_loadingAsset.Remove(key);
                assetWrap.onLoaded?.Invoke(assetWrap.loadedAsset);
            }

            keysToRemove.Clear();
        }

        #endregion

        #region private

        private List<string> keysToRemove;
        
        private async UniTask InitAssetNameMap()
        {
            assetName_assetFullName             = new Dictionary<string, string>();
            assetName_assetBundleName           = new Dictionary<string, string>();
            assetBundleName_assetBundleFullName = new Dictionary<string, string>();
            assetBundleName_resident            = new Dictionary<string, bool>();

            if (Application.isEditor)
            {
                if (assetModeInEditor == AssetMode.AssetBundle)
                {
                    await InitAssetNameMapInAssetBundleModeAsync();
                }
                else
                {
                    InitAssetNameMapInAssetDataBaseMode();
                }
            }
            else
            {
                await InitAssetNameMapInAssetBundleModeAsync();
            }

            GameLogger.Log($"assetName_assetFullName = {JsonConvert.SerializeObject(assetName_assetFullName, Formatting.Indented)}");
            GameLogger.Log($"assetName_assetBundleFullName = {JsonConvert.SerializeObject(assetName_assetBundleName, Formatting.Indented)}");
        }

        private UniTask<DownloadHandler> UnityWebRequestGetAsync(string path)
        {
            var tcs     = new UniTaskCompletionSource<DownloadHandler>();
            var request = UnityWebRequest.Get(path);
            var ret     = request.SendWebRequest();
            ret.completed += _ =>
            {
                tcs.TrySetResult(request.downloadHandler);
            };
            return tcs.Task;
        }
        
        private async UniTask InitAssetNameMapInAssetBundleModeAsync()
        {
            var handler = await UnityWebRequestGetAsync(Path.Combine(ASSETBUNDLE_DIR, "fileName_dirName_assetBundleName.csv"));
            string    text    = handler.text;
            var       lines   = text.Split('\n');
            foreach (var line in lines)
            {
                var newLine = line.Trim();
                var cells   = newLine.Split(',');
                if (cells.Length != 3 || string.IsNullOrEmpty(cells[0]) || string.IsNullOrEmpty(cells[1]) || string.IsNullOrEmpty(cells[2])) continue;

                var assetBundleName = cells[2];
                var assetName       = Path.GetFileNameWithoutExtension(cells[0]);
                assetName_assetFullName[assetName]                   = Path.Combine(cells[1], cells[0]).Replace("\\", "/");
                assetName_assetBundleName[assetName]                 = assetBundleName;
                assetBundleName_assetBundleFullName[assetBundleName] = Path.Combine(ASSETBUNDLE_DIR, assetBundleName).Replace("\\", "/");
            }

            var dirs                    = ASSETBUNDLE_DIR.Split('/');
            var manifestAssetBundleName = dirs[^1];
            assetName_assetFullName["AssetBundleManifest"]               = "AssetBundleManifest";
            assetName_assetBundleName["AssetBundleManifest"]             = manifestAssetBundleName;
            assetBundleName_assetBundleFullName[manifestAssetBundleName] = Path.Combine(ASSETBUNDLE_DIR, manifestAssetBundleName).Replace("\\", "/");
            handler.Dispose();
        }

        private void InitAssetNameMapInAssetDataBaseMode()
        {
            duplicatedAssetName_assetFullNameList = new Dictionary<string, List<string>>();

            foreach (var assetDir in assetDirs)
            {
                AddFiles(assetDir);
            }

            foreach (var assetDir in singleAssetDirs)
            {
                AddFiles(assetDir);
            }

            foreach (var assetDir in residentAssetDirs)
            {
                var assetBundleName = AssetHelper.DirectoryPathToAssetBundleName(assetDir);
                assetBundleName_resident[assetBundleName] = true;
            }

            if (duplicatedAssetName_assetFullNameList.Count > 0)
            {
                var str = JsonConvert.SerializeObject(duplicatedAssetName_assetFullNameList);
                GameLogger.LogError($"以下资源出现重名，在加载的时候可能会找不到：{str}");
            }
        }

        private void AddFiles(string dir)
        {
            var files = Directory.GetFiles(dir);
            foreach (var file in files)
            {
                var ext                      = Path.GetExtension(file);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                if (ext.Equals(".meta"))
                {
                    continue;
                }

                if (assetName_assetFullName.ContainsKey(fileNameWithoutExtension))
                {
                    if (!duplicatedAssetName_assetFullNameList.ContainsKey(fileNameWithoutExtension))
                    {
                        duplicatedAssetName_assetFullNameList[fileNameWithoutExtension] = new();
                        duplicatedAssetName_assetFullNameList[fileNameWithoutExtension].Add(assetName_assetFullName[fileNameWithoutExtension]);
                    }

                    duplicatedAssetName_assetFullNameList[fileNameWithoutExtension].Add(file.Replace("\\", "/"));
                }
                else
                {
                    assetName_assetFullName[fileNameWithoutExtension] = file.Replace("\\", "/");
                }
            }

            var dirs = Directory.GetDirectories(dir);
            foreach (var dir2 in dirs)
            {
                AddFiles(dir2);
            }
        }

        private AssetBundleWrap LoadAssetBundle(string assetBundleName)
        {
            if (!assetBundleName_assetBundleFullName.TryGetValue(assetBundleName, out var assetBundleFullName))
            {
                GameLogger.LogError($"Fail to find assetBundle, assetBundleName ={assetBundleName}");
                return null;
            }

            if (assetBundleName_loadedAssetBundle.TryGetValue(assetBundleName, out var assetBundleWrap))
            {
                return assetBundleWrap;
            }

            if (assetBundleName_loadingAssetBundle.TryGetValue(assetBundleName, out _))
            {
                GameLogger.LogError($"Never try to load an asset synchronously while its assetbundle is loading asynchronously: {assetBundleName}");
                return null;
            }
            
            if (assetBundleName_assetBundleToLoad.TryGetValue(assetBundleName, out _))
            {
                GameLogger.LogError($"Never try to load an asset synchronously while its assetbundle is loading asynchronously: {assetBundleName}");
                return null;
            }

            var assetBundleWrap2 = new AssetBundleWrap(assetBundleName, assetBundleFullName, _ => { });
            assetBundleWrap2.Load();

            // 先添加依赖到队列
            string[] assetBundleNames = assetBundleManifest.GetDirectDependencies(assetBundleWrap2.assetBundleName);
            foreach (var assetBundleName2 in assetBundleNames)
            {
                var wrap2 = LoadAssetBundle(assetBundleName2);
                assetBundleWrap2.deps.Add(wrap2);
                wrap2.abRefs.Add(assetBundleWrap2);
            }

            assetBundleName_loadedAssetBundle.TryAdd(assetBundleName, assetBundleWrap2);
            return assetBundleWrap2;
        }

        private AssetBundleWrap LoadAssetBundleAsync(string assetBundleName, Action<AssetBundleWrap> onLoaded)
        {
            if (!assetBundleName_assetBundleFullName.TryGetValue(assetBundleName, out var assetBundleFullName))
            {
                GameLogger.LogError($"Fail to find assetBundle, assetBundleName ={assetBundleName}");
                return null;
            }

            if (assetBundleName_loadedAssetBundle.TryGetValue(assetBundleName, out var assetBundleWrap))
            {
                return assetBundleWrap;
            }

            if (assetBundleName_loadingAssetBundle.TryGetValue(assetBundleName, out var assetBundleWrap2))
            {
                return assetBundleWrap2;
            }

            if (assetBundleName_assetBundleToLoad.TryGetValue(assetBundleName, out var assetBundleWrap3))
            {
                return assetBundleWrap3;
            }

            // 生成AssetBundleWrap
            var wrap = new AssetBundleWrap(assetBundleName, assetBundleFullName, onLoaded);

            // 先添加依赖到队列
            string[] assetBundleNames = assetBundleManifest.GetDirectDependencies(wrap.assetBundleName);
            foreach (var assetBundleName2 in assetBundleNames)
            {
                var wrap2 = LoadAssetBundleAsync(assetBundleName2, null);
                wrap.deps.Add(wrap2);
                wrap2.abRefs.Add(wrap);
            }

            // 再把自己添加进队列
            assetBundlesToLoad.Enqueue(wrap);
            assetBundleName_assetBundleToLoad[assetBundleName] = wrap;
            return wrap;
        }
        
        private void StartLoadAssetBundleWrapAsync(AssetBundleWrap wrap)
        {
            if (wrap == null)
            {
                return;
            }

            if (wrap.isDone)
            {
                if (wrap.loadedAssetBundle == null)
                {
                    GameLogger.LogError($"加载AssetBundle失败: {wrap.assetBundleFullName}");
                    return;
                }

                GameLogger.Log($"完成加载AB:{wrap.loadedAssetBundle.name}");
                wrap.onLoaded?.Invoke(wrap);
                assetBundleName_loadedAssetBundle[wrap.assetBundleName] = wrap;
            }
            else
            {
                wrap.LoadAsync();
                if (wrap.isDone)
                {
                    GameLogger.Log($"完成加载AB:{wrap.loadedAssetBundle.name}");
                    wrap.onLoaded?.Invoke(wrap);
                    assetBundleName_loadedAssetBundle[wrap.assetBundleName] = wrap;
                }
                else
                {
                    assetBundleName_loadingAssetBundle[wrap.assetBundleName] = wrap;
                }
            }
        }

        #endregion
    }
    
    public enum AssetMode
    {
        AssetBundle,
        AssetDataBase
    }
}