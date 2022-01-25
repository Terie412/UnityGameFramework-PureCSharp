using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain_GameEvent : MonoBehaviour
{
    void Start()
    {
        GameEvent.AddListener(GameEventID.EvtOpenWindow, F1);
        GameEvent.AddListener(GameEventID.EvtOpenWindow, F2);
        
        
        GameEvent.Publish(GameEventID.EvtOpenWindow);

        Debug.Log("Again");
        
        GameEvent.RemoveAllListenerByID(GameEventID.EvtOpenWindow);
        GameEvent.Publish(GameEventID.EvtOpenWindow);
    }

    private void F1(params object[] args)
    {
        Debug.Log("F1");
    }
    
    private void F2(params object[] args)
    {
        Debug.Log("F2");
        
        GameEvent.RemoveListener(GameEventID.EvtOpenWindow, F2);
    }
}
