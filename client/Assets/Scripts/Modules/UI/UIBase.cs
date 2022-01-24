using System;
using UnityEngine;

namespace QTC.Modules.UI
{
    public class UIBase: MonoBehaviour
    {
        public UnityTimer StartTimer(float duration, Action onComplete = null, Action<float> onUpdate = null, bool isLooped = false, bool useTimeScale = false)
        {
            var timer = new UnityTimer(duration, onComplete, onUpdate, isLooped, useTimeScale, this);
            timer.Start();
            return timer;
        }
    }
}

