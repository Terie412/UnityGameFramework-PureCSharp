﻿using UnityEngine;

namespace Core
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        private void OnEnable()
        {
            if (transform.parent == null)
                DontDestroyOnLoad(this);
        }
    }
}