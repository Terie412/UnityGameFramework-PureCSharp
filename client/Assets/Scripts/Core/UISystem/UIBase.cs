using System;
using UnityEngine;

namespace Core
{
    public class UIBase: MonoBehaviour
    {
        public UnityTimer StartTimer(float duration, Action onComplete = null, Action<float> onUpdate = null, bool isLooped = false, bool useTimeScale = false)
        {
            var timer = new UnityTimer(duration, onComplete, onUpdate, isLooped:isLooped, useTimeScale:useTimeScale, obj:gameObject);
            timer.Start();
            return timer;
        }
    }
}

