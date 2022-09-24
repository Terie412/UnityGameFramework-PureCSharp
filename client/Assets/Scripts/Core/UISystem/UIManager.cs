using System.Collections.Generic;
using System.Threading.Tasks;
using Framework;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Core
{
    public class UIManager : SingleTon<UIManager>
    {
        public bool       isInit = false;
        public GameObject uiRoot;

        public Canvas ReferenceCanvas;        // 该Canvas的模式为 ScreenSpace，大小用来指代当前屏幕分辨率（对于编辑器下，Screen.width/height 不能取到正确的分辨率）
        public Canvas LoadingEffectCanvas;    // 转场 Loading
        public Canvas FullScreenEffectCanvas; // 全屏特效所在的 Canvas。全屏特效不能 Block 屏幕的点击事件
        public Canvas SystemUICanvas;         // 系统级的 Window 所在的 Canvas，一般用来表达程序错误相关的重要事项，例如断线或者异常错误需要退回到登录界面等
        public Canvas GuideUICanvas;          // 引导层 Window 所在的 Canvas
        public Canvas UpperUICanvas;          // 处于 Main 之上的 Window 所在的 Canvas，暂时没有想到有需要用到的地方
        public Canvas MainUICanvas;           // 所有常见的功能性 Window 所在的 Canvas
        public Canvas HUDCanvas;              // HUD 所在的 Canvas

        private const string UIROOT_NAME = "UIRoot";

        private Queue<LoadRequest> loadingQueue            = new();
        private Stack<UIWindow>    fullScreenEffectUIStack = new();
        private Stack<UIWindow>    systemUIStack           = new();
        private Stack<UIWindow>    guideUIStack            = new();
        private Stack<UIWindow>    upperUIStack            = new();
        private Stack<UIWindow>    mainUIStack             = new();
        private Stack<UIWindow>    hudUIStack              = new();
        
        public async Task InitAsync()
        {
            if (isInit)
            {
                GameLogger.LogWarning("Do not init UIManager twice!");
                return;    
            }
            
            uiRoot = await AssetManager.Instance.LoadAndInstantiateGameObjectAsync(UIROOT_NAME, null);
            uiRoot.GetOrAddComponent<DontDestroyOnLoad>();

            LoadingEffectCanvas    = uiRoot.transform.Find("LoadingEffectCanvas").GetComponent<Canvas>();
            FullScreenEffectCanvas = uiRoot.transform.Find("FullScreenEffectCanvas").GetComponent<Canvas>();
            SystemUICanvas         = uiRoot.transform.Find("SystemUICanvas").GetComponent<Canvas>();
            GuideUICanvas          = uiRoot.transform.Find("GuideUICanvas").GetComponent<Canvas>();
            UpperUICanvas          = uiRoot.transform.Find("UpperUICanvas").GetComponent<Canvas>();
            MainUICanvas           = uiRoot.transform.Find("MainUICanvas").GetComponent<Canvas>();
            HUDCanvas              = uiRoot.transform.Find("HUDCanvas").GetComponent<Canvas>();

            isInit = true;
        }

        public void OpenWindow(string wndName, UIWindow parent, params object[] args)
        {
            bool hasParent = false;
            if (parent != null)
            {
                if (parent.sysType != UIWindow.E_WINDOW_TYPE.Main)
                {
                    GameLogger.LogError($"Fail to open window {wndName}: Does not support opening window upon parent with type = {parent.sysType}");
                    return;
                }

                if (!mainUIStack.TryPeek(out var peekWnd) || peekWnd != parent)
                {
                    GameLogger.LogError($"Fail to open window {wndName}: Parent does not exist at the top of MainUIStack");
                    return;
                }

                hasParent = true;
            }

            GameLogger.Log($"Try open window: {wndName}");
            var request = new LoadRequest { state = E_LOAD_STATE.loading, hasParent = hasParent, parent = parent, args = args };
            loadingQueue.Enqueue(request);
            AssetManager.Instance.LoadAndInstantiateGameObjectAsync(wndName, MainUICanvas.transform, gos =>
            {
                OnWindowLoadedAndInstantiated(request, gos);
            });

            Update();
        }

        public void CloseWindow(UIWindow window)
        {
            if ((window.sysType == UIWindow.E_WINDOW_TYPE.Main && mainUIStack.Count == 0)
                || (window.sysType == UIWindow.E_WINDOW_TYPE.Upper && upperUIStack.Count == 0)
                || (window.sysType == UIWindow.E_WINDOW_TYPE.System && systemUIStack.Count == 0))
            {
                GameLogger.LogError($"尝试关闭窗口：{window.name}，但是窗口堆栈为空");
                return;
            }

            Stack<UIWindow> stack = window.sysType switch
            {
                UIWindow.E_WINDOW_TYPE.Main => mainUIStack,
                UIWindow.E_WINDOW_TYPE.Upper => upperUIStack,
                _ => systemUIStack
            };

            var count = 1;
            while (true)
            {
                count++;
                if (count > 100)
                {
                    GameLogger.LogError("死循环");
                    return;
                }

                if (stack.Count == 0)
                {
                    break;
                }

                var peekWindow = stack.Pop();
                if (peekWindow != window)
                {
                    Object.Destroy(window.gameObject);
                }
                else
                {
                    Object.Destroy(peekWindow.gameObject);
                    break;
                }
            }
        }

        public void Clear()
        {
            UIWindow peek;
            while (fullScreenEffectUIStack.TryPop(out peek)) peek.Close();
            while (systemUIStack.TryPop(out peek)) peek.Close();
            while (guideUIStack.TryPop(out peek)) peek.Close();
            while (upperUIStack.TryPop(out peek)) peek.Close();
            while (mainUIStack.TryPop(out peek)) peek.Close();
            while (hudUIStack.TryPop(out peek)) peek.Close();
            
            fullScreenEffectUIStack.Clear();
            systemUIStack.Clear();
            guideUIStack.Clear();
            upperUIStack.Clear();
            mainUIStack.Clear();
            hudUIStack.Clear();
        }

        public void Update()
        {
            while (true)
            {
                if (loadingQueue.Count == 0)
                    break;

                var request = loadingQueue.Peek();
                var target  = request.target;

                // 如果队头的窗口还在加载，后面的窗口即使加载出来也是不管的
                if (request.state == E_LOAD_STATE.loading)
                {
                    break;
                }
                
                loadingQueue.Dequeue();

                // 请求已经失效了
                if (request.state == E_LOAD_STATE.dead)
                {
                    loadingQueue.Dequeue();
                    if (request.target != null)
                    {
                        Object.Destroy(request.target.gameObject);
                        request.target = null;
                    }

                    continue;
                }

                // 上下文已丢失
                if (request.hasParent && request.parent == null)
                {
                    Object.Destroy(request.target.gameObject);
                    request.target = null;
                    continue;
                }
                
                var (ret, parent) = TryAddWindowToStack(target);
                if (!ret)
                {
                    Object.Destroy(target.gameObject);
                }
                else
                {
                    target.transform.SetParent(parent, false);
                    target.gameObject.SetActive(true);
                    target.OnOpen(request.args);
                    GameEvent.Publish(GameEventID.EvtOpenWindow, target.name);
                }
            }
        }
        
        private void OnWindowLoadedAndInstantiated(LoadRequest request, GameObject[] gos)
        {
            var wndGo = gos[1];
            if (request.state == E_LOAD_STATE.dead)
            {
                // 加载请求已失效
                Object.Destroy(wndGo);
                return;
            }

            if (request.hasParent && request.parent == null)
            {
                // 预期有父级，但是此时父级已经不存在了。即上下文丢失
                request.state = E_LOAD_STATE.dead;
                Object.Destroy(wndGo);
                return;
            }

            var uiWindow = wndGo.GetComponent<UIWindow>();
            if (uiWindow == null)
            {
                request.state = E_LOAD_STATE.dead;
                Object.Destroy(wndGo);
                return;
            }

            request.state  = E_LOAD_STATE.loaded;
            request.target = uiWindow;
            wndGo.AddComponent<SafeAreaRectTransform>(); // UI的屏幕适配：窗口需要保持在安全区以内
            wndGo.SetActive(false);                      // 加载完之后先隐藏，然后由 UIManager 统一驱动窗口的显示时机（走 Update()）
        }

        /// <summary>
        /// 尝试将窗口添加到对应的窗口栈，并为其添加必要的组件
        /// </summary>
        /// <returns>(是否成功添加到窗口栈，窗口Transform的预期父级)</returns>
        private (bool, Transform) TryAddWindowToStack(UIWindow window)
        {
            Stack<UIWindow> targetStack = mainUIStack;
            Transform       parent      = MainUICanvas.transform;

            switch (window.sysType)
            {
                case UIWindow.E_WINDOW_TYPE.Upper:
                    parent      = UpperUICanvas.transform;
                    targetStack = upperUIStack;
                    break;
                case UIWindow.E_WINDOW_TYPE.System:
                    while (systemUIStack.TryPeek(out var curTopSystemWindow))
                    {
                        GameLogger.LogWarning($"System window {curTopSystemWindow} is forced to close for another system window is trying to open");
                        CloseWindow(curTopSystemWindow);
                    }

                    parent      = SystemUICanvas.transform;
                    targetStack = systemUIStack;
                    break;
                case UIWindow.E_WINDOW_TYPE.Main:
                    parent      = MainUICanvas.transform;
                    targetStack = mainUIStack;
                    break;
                case UIWindow.E_WINDOW_TYPE.FullScreenEffect:
                    while (fullScreenEffectUIStack.TryPeek(out var curTopFullScreenEffectWindow))
                    {
                        GameLogger.LogWarning($"System window {curTopFullScreenEffectWindow} is forced to close for another system window is trying to open");
                        CloseWindow(curTopFullScreenEffectWindow);
                    }

                    parent      = FullScreenEffectCanvas.transform;
                    targetStack = fullScreenEffectUIStack;
                    break;
                case UIWindow.E_WINDOW_TYPE.Guide:
                    while (guideUIStack.TryPeek(out var guideWindow))
                    {
                        GameLogger.LogWarning($"System window {guideWindow} is forced to close for another system window is trying to open");
                        CloseWindow(guideWindow);
                    }

                    parent      = GuideUICanvas.transform;
                    targetStack = guideUIStack;
                    break;
                case UIWindow.E_WINDOW_TYPE.HUD:
                    parent      = HUDCanvas.transform;
                    targetStack = hudUIStack;
                    break;
                default:
                    GameLogger.LogError($"No Implementation of type = {window.sysType}");
                    return (false, null);
            }
            
            var parentCanvas = window.GetComponentInParent<Canvas>();
            var canvas       = window.GetOrAddComponent<Canvas>();
            
            window.GetOrAddComponent<GraphicRaycaster>();
            
            canvas.overrideSorting  = true;
            canvas.sortingLayerName = parentCanvas.sortingLayerName;
            canvas.sortingOrder     = parentCanvas.sortingOrder + 5 + mainUIStack.Count * 50; // 窗口下的层级为 [-5, 44] 共 50 个层级，其中默认从 0 开始
            targetStack.Push(window);
            return (true, parent);
        }
        
        private enum E_LOAD_STATE
        {
            loading,
            loaded,
            dead
        }

        private class LoadRequest
        {
            public UIWindow     target;
            public E_LOAD_STATE state = E_LOAD_STATE.loading;
            public bool         hasParent;
            public UIWindow     parent;
            public object[]     args;
        }
    }
}