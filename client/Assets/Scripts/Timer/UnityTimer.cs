using System;
using UnityEngine;

// 仿照 https://github.com/akbiggs/UnityTimer 重新写了一版，有所修改，思路上没有太大的变化
public class UnityTimer
{
    public float duration;          // 定时器的时长
    public float interval;          // 定时器的onUpdate回调的间隔
    public bool isLooped;           // 是否循环（在Completed之后自动开启下一轮）。注意即使是Loop的Timer，onComplete 至始至终只会执行一次
    public bool isCompleted;        // 是否正常结束
    public bool useTimeScale;       // 是否受到TimeScale影响
    public bool isPause;            // 是否暂停
    public bool isCancelled;        // 是否被主动取消
    public bool isBoundToObject;    // 是否将生命周期绑定到某个Object上
    public bool useRealTime;        // 是否使用物理时间，否则计时器每帧只会累加 Time.time，这在应用程序处于后台的时候，这段时间是不参与累加的
    public UnityEngine.Object obj;  // 生命周期会绑定到这个Object上。如果你是绑定到 GameObject 上，注意 Object.Destroy 的实际销毁会延续到帧尾

    public bool isStarted => TimerTicker.Instance.IsTimerRegistered(this, true);            // 标识当前Timer是否已经启动了。该字段会在 TimerTicker 注销自己之后返回 false
    public bool isDone => isCompleted || isCancelled || (isBoundToObject && obj == null);   // 是否结束了，包括正常结束和非正常结束（主动取消或绑定消失）。其实自身被注销，该状态也不会改变。

    public Action onComplete;           // 完成的回调，在非正常结束的情况下，该回调不会被调用
    public Action<float> onUpdate;      // 每帧回调，返回从定时器 Start 或 Restart 到当前为止实际的运行时长；定时器开始的当帧会调用一次

    private bool hasCalledOnComplete;   // 是否调用过 onComplete。用来防止 Loop 的情况下多次调用
    private float lastUpdateTime;       // 最近一次更新定时器的时间戳，只要没有isDone，这个每帧都会更新
    private float lastOnUpdateTime;     // 最近一次调用 onUpdate 的时间，与 deltaTime 是一个维度的时间概念。初始化为 -1 并且会被即时调用。
    private float deltaTime;            // 当前定时器存活的总时长，在没有isDone，没有isPause的情况下会被更新

    /// <summary>
    /// 定时器
    /// </summary>
    /// <param name="duration">持续时间</param>
    /// <param name="onComplete">定时器正常结束的回调，在Loop模式下会多次调用</param>
    /// <param name="onUpdate">定时器每帧更新的回调</param>
    /// <param name="interval">定时器的onUpdate回调的间隔</param>
    /// <param name="obj">与一个Object的生命周期绑定，在Object生命周期结束的时候，定时器也会自动结束（非正常）</param>
    /// <param name="useRealTime">是否使用物理时间，否则计时器每帧只会累加 Time.time，这在应用程序处于后台的时候，这段时间是不参与累加的</param>
    /// <param name="isLooped">是否循环</param>
    /// <param name="useTimeScale">是否受TimeScale影响</param>
    public UnityTimer(float duration, Action onComplete = null, Action<float> onUpdate = null, float interval = 0, UnityEngine.Object obj = null, bool useRealTime = false, bool isLooped = false, bool useTimeScale = false)
    {
        this.duration = duration;
        this.onComplete = onComplete;
        this.onUpdate = onUpdate;
        this.interval = interval;
        this.isLooped = isLooped;
        this.useTimeScale = useTimeScale;
        this.useRealTime = useRealTime;
        this.obj = obj;
       
        deltaTime = 0f;
        lastOnUpdateTime = -1f;
        isCompleted = false;
        isPause = false;
        isCancelled = false;
        hasCalledOnComplete = false;
        isBoundToObject = this.obj != null;
    }

    public void Start()
    {
        if (isStarted)
        {
            Debug.LogWarning("Start timer fails for it has been started. Try using Restart() to restart a timer");
            return;
        }

        if (isDone)
        {
            Debug.LogWarning("Timer is done. Try using Restart() to restart a timer");
            return;
        }
        
        if (onUpdate == null && onComplete == null)
        {
            Debug.LogWarning("Starting a timer without registering onUpdate and onComplete callback is meaningless.");
            return;
        }

        lastUpdateTime = GetTimeNow();
        TimerTicker.Instance.RegisterTimer(this);
    }

    public void Restart()
    {
        deltaTime = 0f;
        lastOnUpdateTime = -1f;
        isCompleted = false;
        isPause = false;
        isCancelled = false;
        hasCalledOnComplete = false;
        lastUpdateTime = GetTimeNow();
        if (!isStarted)
        {
            Start();
        }
    }

    // 暂停过程中的时间不会计入Timer的运行时长
    public void Pause()
    {
        isPause = true;
    }

    public void Resume(bool isForceUpdate = false)
    {
        isPause = false;
        lastUpdateTime = GetTimeNow();
        if (isForceUpdate)
        {
            Update();
        }
    }

    public void Cancel(bool isForceComplete = false)
    {
        isCancelled = true;
        if(isForceComplete) CallComplete();
    }

    public void Update()
    {
        var timeNow = GetTimeNow();
        if (!isPause)
        {
            deltaTime += timeNow - lastUpdateTime;
            if (lastOnUpdateTime < 0 || deltaTime - lastOnUpdateTime > interval)
            {
                lastOnUpdateTime = deltaTime;
                onUpdate?.Invoke(deltaTime);
            }
            if (deltaTime >= duration)
            {
                CallComplete();
                isCompleted = !isLooped;
            }
        }

        lastUpdateTime = timeNow;
    }

    private void CallComplete()
    {
        if (hasCalledOnComplete) return;
        hasCalledOnComplete = true;
        onComplete?.Invoke();
    }
    
    private float GetTimeNow()
    {
        if (useRealTime)
        {
            return useTimeScale ? Time.realtimeSinceStartup * Time.timeScale : Time.realtimeSinceStartup;
        }

        return useTimeScale ? Time.time : Time.unscaledTime;
    }
}