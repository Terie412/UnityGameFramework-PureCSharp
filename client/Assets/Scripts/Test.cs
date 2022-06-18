using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public RectTransform rt;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (rt != null)
            {
                Debug.Log($"{LayoutUtility.GetPreferredHeight(rt)}");
            }
        }
    }
}
