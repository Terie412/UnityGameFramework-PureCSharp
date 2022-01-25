using System;
using System.Collections.Generic;

public static class GameEvent
{
    private static Dictionary<GameEventID, Action<object[]>> id_handler = new();

    public static void AddListener(GameEventID id, Action<object[]> callback)
    {
        if (!id_handler.ContainsKey(id))
        {
            id_handler.Add(id, callback);
        }
        else
        {
            id_handler[id] += callback;
        }
    }

    public static void Publish(GameEventID id, params object[] args)
    {
        if (!id_handler.ContainsKey(id))
            return;
        
        id_handler[id].Invoke(args);
    }

    public static void RemoveListener(GameEventID id, Action<object[]> callback)
    {
        if (!id_handler.ContainsKey(id))
            return;
        
        id_handler[id] -= callback;
    }
    
    public static void RemoveAllListenerByID(GameEventID id)
    {
        id_handler.Remove(id);
    }
}