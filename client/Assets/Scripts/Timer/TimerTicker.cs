using System;
using System.Collections.Generic;

/// 为Timer提供一个Unity运行时生命周期的环境
public class TimerTicker : SingletonBehaviour<TimerTicker>
{
    private List<UnityTimer> registeredTimer = new(); // 注册中的定时器
    private List<UnityTimer> timerToRegister = new(); // 防止遍历当中修改
    private List<UnityTimer> timerToRemove = new(); // 因为异常情况要被移除的Timer

    private void Update()
    {
        registeredTimer.AddRange(timerToRegister);
        timerToRegister.Clear();

        foreach (var timer in registeredTimer)
        {
            if (!timer.isDone)
            {
                try
                {
                    timer.Update();
                }
                catch (Exception)
                {
                    timerToRemove.Add(timer); // 引发异常的Timer直接会被抛弃
                }
            }
        }

        registeredTimer.RemoveAll(timer => timer.isDone);
        foreach (var timer in timerToRemove)
        {
            registeredTimer.Remove(timer);
        }

        timerToRemove.Clear();
    }

    public void RegisterTimer(UnityTimer unityTimer)
    {
        timerToRemove.Remove(unityTimer); // 一个即将被 Remove 的Timer可能被即使 Restart 从而复活
        timerToRegister.Add(unityTimer);

        // 如果时间 <= 0，Timer的回调会转成同步，当帧处理
        if (unityTimer.duration <= 0)
        {
            unityTimer.Update();
        }
    }

    public bool IsTimerRegistered(UnityTimer timer, bool includingToRegister = false)
    {
        if (timer == null)
        {
            return false;
        }

        return registeredTimer.Contains(timer) || (includingToRegister && timerToRegister.Contains(timer));
    }
}