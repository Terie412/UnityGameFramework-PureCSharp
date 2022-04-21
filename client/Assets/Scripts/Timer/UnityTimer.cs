using System;
using UnityEngine;

// 仿照 https://github.com/akbiggs/UnityTimer 重新写了一版，有所修改，思路上没有太大的变化
public class UnityTimer
{
    public float duration; // 定时器的时长
    public bool isLooped; // 是否循环（在Completed之后自动开启下一轮）
    public bool isCompleted; // 是否正常结束
    public bool useTimeScale; // 是否受到TimeScale影响
    public bool isPause; // 是否暂停
    public bool isCancelled; // 是否被主动取消
    public bool isBoundToObject; // 是否将生命周期绑定到某个Object上
    public UnityEngine.Object obj; // 生命周期会绑定到这个Object上。如果你是绑定到 GameObject 上，注意 Object.Destroy 的实际销毁会延续到帧尾

    public bool isDone => isCompleted || isCancelled || (isBoundToObject && obj == null); // 是否结束了，包括正常结束和非正常结束（主动取消或绑定消失）

    public Action onComplete; // 完成的回调，在非正常结束的情况下，该回调不会被调用
    public Action<float> onUpdate; // 每帧回调，返回从定时器开始到当前回调的帧开始；定时器开始的当帧会调用一次

    private float lastUpdateTime; // 最近一次更新定时器的时间戳，只要没有isDone，这个每帧都会更新
    private float deltaTime; // 当前定时器存活的总时长，在没有isDone，没有isPause的情况下会被更新

    /// <summary>
    /// 定时器
    /// </summary>
    /// <param name="duration">持续时间</param>
    /// <param name="onComplete">定时器正常结束的回调，在Loop模式下会多次调用</param>
    /// <param name="onUpdate">定时器每帧更新的回调</param>
    /// <param name="isLooped">是否循环</param>
    /// <param name="useTimeScale">是否受TimeScale影响</param>
    /// <param name="obj">与一个Object的生命周期绑定，在Object生命周期结束的时候，定时器也会自动结束（非正常）</param>
    public UnityTimer(float duration, Action onComplete = null, Action<float> onUpdate = null, bool isLooped = false, bool useTimeScale = false, UnityEngine.Object obj = null)
    {
        this.duration = duration;
        this.onComplete = onComplete;
        this.onUpdate = onUpdate;
        this.isLooped = isLooped;
        this.useTimeScale = useTimeScale;
        this.obj = obj;
        this.deltaTime = 0f;
        
        isCompleted = false;
        isPause = false;
        isCancelled = false;
        isBoundToObject = this.obj != null;
    }

    public void Start()
    {
        if (onUpdate == null && onComplete == null)
        {
            GameLogger.Warning("Starting a timer without registering onUpdate and onComplete callback is meaningless.");
            return;
        }

        lastUpdateTime = GetTimeNow();
        TimerTicker.Instance.RegisterTimer(this);
    }

    public void Update()
    {
        var timeNow = GetTimeNow();
        if (!isPause)
        {
            deltaTime += timeNow - lastUpdateTime;
            onUpdate?.Invoke(deltaTime);
            if (deltaTime >= duration)
            {
                CallComplete();
            }
        }

        lastUpdateTime = timeNow;
    }

    public void Pause()
    {
        isPause = true;
    }

    public void Resume()
    {
        isPause = false;
        lastUpdateTime = GetTimeNow();
    }

    public void Cancel()
    {
        isCancelled = true;
    }

    private void CallComplete()
    {
        onComplete?.Invoke();
        isCompleted = !isLooped;
    }

    private float GetTimeNow()
    {
        return useTimeScale ? Time.time : Time.unscaledTime; // 原著在这里使用的是realTimeSinceStartup，这个时间虽然不受时间影响，但是这个时间在同一帧内多次调用未必能够得到相同的结果
    }
}