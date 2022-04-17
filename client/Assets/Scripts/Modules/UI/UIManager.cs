using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace QTC.Modules.UI
{
    public class UIManager : SingleTon<UIManager>
    {
        public bool isInit = false;
        public GameObject uiRoot;
        public Canvas LoadingEffectCanvas;
        public Canvas FullScreenEffectCanvas;
        public Canvas SystemUICanvas;
        public Canvas GuideUICanvas;
        public Canvas UpperUICanvas;
        public Canvas MainUICanvas;
        public Canvas HUDCanvas;

        private const string UIROOT_NAME = "UIRoot";
        private Queue<LoadRequest> loadingQueue;
        private Stack<UIWindow> mainUIStack;
        private Stack<UIWindow> upperUIStack;
        private Stack<UIWindow> systemUIStack;

        public enum E_LOAD_STATE
        {
            loading,
            loaded,
            dead
        }

        public class LoadRequest
        {
            public UIWindow target;
            public E_LOAD_STATE state = E_LOAD_STATE.loading;
            public UIWindow parent;
            public object[] args;
        }
        
        public UIManager()
        {
            loadingQueue = new Queue<LoadRequest>();
            mainUIStack = new Stack<UIWindow>();
            upperUIStack = new Stack<UIWindow>();
            systemUIStack = new Stack<UIWindow>();
        }
        
        public async Task InitAsync()
        {
            uiRoot = await AssetManager.Instance.LoadAndInstantiateGameObjectAsync(UIROOT_NAME, null);
            uiRoot.GetOrAddComponent<DontDestroyOnLoad>();

            LoadingEffectCanvas = uiRoot.transform.Find("LoadingEffectCanvas").GetComponent<Canvas>();
            FullScreenEffectCanvas = uiRoot.transform.Find("FullScreenEffectCanvas").GetComponent<Canvas>();
            SystemUICanvas = uiRoot.transform.Find("SystemUICanvas").GetComponent<Canvas>();
            GuideUICanvas = uiRoot.transform.Find("GuideUICanvas").GetComponent<Canvas>();
            UpperUICanvas = uiRoot.transform.Find("UpperUICanvas").GetComponent<Canvas>();
            MainUICanvas = uiRoot.transform.Find("MainUICanvas").GetComponent<Canvas>();
            HUDCanvas = uiRoot.transform.Find("HUDCanvas").GetComponent<Canvas>();

            isInit = true;
        }

        public void OpenWindow(string wndName, UIWindow parent, params object[] args)
        {
            bool hasParent = false;
            if (parent != null)
            {
                if (parent.sysType != E_WINDOW_TYPE.Main)
                {
                    GameLogger.Error($"不支持将生命周期绑定到其他类型的窗口上");
                    return;
                }

                if (!mainUIStack.TryPeek(out var peekWnd) || peekWnd != parent)
                {
                    GameLogger.Error($"窗口栈顶上下文丢失");
                    return;
                }

                hasParent = true;
            }

            GameLogger.Info($"Try to open window: {wndName}");
            var request = new LoadRequest() {state = E_LOAD_STATE.loading, parent = parent, args = args};
            AssetManager.Instance.LoadAndInstantiateGameObjectAsync(wndName, MainUICanvas.transform, gos =>
            {
                var go = gos[1];
                if (request.state == E_LOAD_STATE.dead)
                {
                    Object.Destroy(go);
                    return;
                }

                if (hasParent && parent == null)
                {
                    // 预期有父级，但是此时父级已经不存在了
                    request.state = E_LOAD_STATE.dead;
                    Object.Destroy(go);
                    return;
                }

                var uiWindow = go.GetComponent<UIWindow>();
                if (uiWindow == null)
                {
                    request.state = E_LOAD_STATE.dead;
                    Object.Destroy(go);
                    return;
                }

                request.state = E_LOAD_STATE.loaded;
                request.target = uiWindow;
                go.SetActive(false);
            });
            
            loadingQueue.Enqueue(request);
            Update();
        }

        public void CloseWindow(UIWindow window)
        {
            if ((window.sysType == E_WINDOW_TYPE.Main && mainUIStack.Count == 0) 
                || (window.sysType == E_WINDOW_TYPE.Upper && upperUIStack.Count == 0)
                || (window.sysType == E_WINDOW_TYPE.System && systemUIStack.Count == 0))
            {
                GameLogger.Error($"尝试关闭窗口：{window.name}，但是窗口堆栈为空");
                return;
            }

            Stack<UIWindow> stack = window.sysType switch
            {
                E_WINDOW_TYPE.Main => mainUIStack,
                E_WINDOW_TYPE.Upper => upperUIStack,
                _ => systemUIStack
            };

            var count = 1;
            while (true)
            {
                count++;
                if (count > 100)
                {
                    GameLogger.Error("死循环");
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
            while (true)
            {
                if (mainUIStack.TryPop(out peek))
                {
                    peek.Close();
                }
                else
                {
                    break;
                }
            }

            while (true)
            {
                if (upperUIStack.TryPop(out peek))
                {
                    peek.Close();
                }
                else
                {
                    break;
                }
            }

            while (true)
            {
                if (systemUIStack.TryPop(out peek))
                {
                    peek.Close();
                }
                else
                {
                    break;
                }
            }
            
            while (true)
            {
                if (loadingQueue.TryDequeue(out var request))
                {
                    request.state = E_LOAD_STATE.dead;
                }
                else
                {
                    break;
                }
            }
        }
        
        public void Update()
        {
            while (true)
            {
                if (loadingQueue.Count == 0)
                    break;

                var request = loadingQueue.Peek();
                if (request.state == E_LOAD_STATE.loading)
                    break;

                if (request.state == E_LOAD_STATE.dead)
                {
                    loadingQueue.Dequeue();
                    if (request.target != null)
                    {
                        Object.Destroy(request.target.gameObject);
                    }

                    break;
                }

                loadingQueue.Dequeue();
                var target = request.target;
                if (request.parent != null && request.parent == null)
                {
                    Object.Destroy(target.gameObject);
                    break;
                }

                switch (target.sysType)
                {
                    case E_WINDOW_TYPE.Upper:
                        target.transform.SetParent(UpperUICanvas.transform);
                        break;
                    case E_WINDOW_TYPE.System:
                        target.transform.SetParent(SystemUICanvas.transform);
                        break;
                    case E_WINDOW_TYPE.Main:
                        target.gameObject.SetActive(true);
                        target.transform.localScale = Vector3.one;
                        var ret = RegisterWindow(target);
                        if (!ret)
                        {
                            GameLogger.Error($"窗口入栈失败: {target.name}");
                            Object.Destroy(target.gameObject);
                        }
                        else
                        {
                            target.OnOpen(request.args);
                            GameEvent.Publish(GameEventID.EvtOpenWindow, target.name);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private bool RegisterWindow(UIWindow window)
        {
            switch (window.sysType)
            {
                case E_WINDOW_TYPE.Upper:
                    upperUIStack.Push(window);
                    return true;
                case E_WINDOW_TYPE.System when systemUIStack.Count >= 1:
                    GameLogger.Error($"默认只能存在一个系统级弹窗"); // 这里的做法有待商榷，可能把栈内的窗口都销毁，保留新的系统弹窗更合理
                    return false;
                case E_WINDOW_TYPE.System:
                    systemUIStack.Push(window);
                    return true;
                case E_WINDOW_TYPE.Main:
                    var parentCanvas = window.GetComponentInParent<Canvas>();
                    var canvas = window.GetOrAddComponent<Canvas>();
                    window.GetOrAddComponent<GraphicRaycaster>();
                    canvas.overrideSorting = true;
                    canvas.sortingLayerName = parentCanvas.sortingLayerName;
                    canvas.sortingOrder = parentCanvas.sortingOrder + 5 + mainUIStack.Count * 50;

                    mainUIStack.Push(window);
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}