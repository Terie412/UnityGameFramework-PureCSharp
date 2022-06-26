using System;
using UnityEngine;

// 当前类所有的接口和字段都表示的是UTC时间，以服务器为标准，误差大致为登录协议的网络时延
public static class TimeUtils
{
    // 下面这两个时间戳的差，等于两台机器计时系统的相对误差 + 协议的下行延迟
    // 两台机器计时系统客观存在误差，即相同时刻两台机器的 DateTime.UtcNow 存在毫秒级误差
    // 它决定了必须在客户端和服务器之间传送时间戳的时候，无法采用各自实现。例如客户端记录时间戳 t1 并发送上行包，服务器收到上行包记录 t2，无法保证 t1 < t2
    // 所以我们必须采用同一个标准，即所有客户端都以服务器时间为准
    // 协议的传输延迟决定了我们无法在不同机器之间统一时间。例如服务器在统一的服务器时间开放一个协议，深处差网络环境的机器注定较晚收到活动的开启通知
    
    public static ulong LoginTimeServer;    // 服务器收到当前客户端登录请求的服务器机器 UTC0 时间戳，单位 ms
    public static float LoginRealTimeSinceStartUp;    // 客户端收到登录回包时的客户端机器时间戳，单位 ms

    /// 获取当前的时间戳，不同的客户端这个接口的差异取决于登录协议的网络延迟（主要）+ 客户端和服务器两台设备计时系统的误差（次要）
    /// 因为我们无法获取协议的上行延迟/下行延迟，所以网络延迟部分带来的误差我们无法消除
    public static ulong GetTimeNow()
    {
        // 客户端不能使用 DateTime 来记录时间，原因是 DateTime 跟当前操作系统时间有关
        var now = Time.realtimeSinceStartupAsDouble;
        return LoginTimeServer + (ulong) ((now - LoginRealTimeSinceStartUp) * 1000);
    }

    // 毫秒时间戳 -> DateTime
    public static DateTime MilliTimeStampToDateTime(ulong ts)
    {
        var offset = DateTimeOffset.FromUnixTimeMilliseconds((long) ts);
        return offset.UtcDateTime;
    }
}