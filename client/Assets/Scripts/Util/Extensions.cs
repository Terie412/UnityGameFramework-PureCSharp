using System;
using UnityEngine;

public static class Extensions
{
    public static T GetOrAddComponent<T>(this GameObject go) where T: Behaviour
    {
        if (go == null)
        {
            return null;
        }

        var comp = go.GetComponent<T>();
        if (comp == null)
        {
            return go.AddComponent<T>();
        }

        return comp;
    }
    
    public static Component GetOrAddComponent(this GameObject go, Type t)
    {
        if (go == null)
        {
            return null;
        }

        var comp = go.GetComponent(t);
        if (comp == null)
        {
            return go.AddComponent(t);
        }

        return comp;
    }
    
    public static Component GetOrAddComponent(this Component comp, Type t)
    {
        return comp == null ? null : comp.gameObject.GetOrAddComponent(t);
    }
    
    public static T GetOrAddComponent<T>(this Component comp) where T: Behaviour
    {
        return comp == null ? null : comp.gameObject.GetOrAddComponent<T>();
    }
}