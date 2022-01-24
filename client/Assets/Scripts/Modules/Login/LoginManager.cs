using System;
using GameProtocol;
using Newtonsoft.Json;
using UnityEngine;

public class LoginManager: SingleTon<LoginManager>
{
    private double loginReqSendTime;
    
    public LoginManager()
    {
        NetClient.Instance.RegisterProtocol("LoginAck", OnLoginAck);
    }

    public void StartLogin()
    {
        loginReqSendTime = Time.realtimeSinceStartupAsDouble;
        LoginReq msg = new LoginReq();
        NetClient.Instance.SendMessage(msg);
    }

    private void OnLoginAck(object msg)
    {
        var ack = msg as LoginAck;
        TimeUtils.LoginTimeServer = ack.LoginTime;
        TimeUtils.LoginRealTimeSinceStartUp = Time.realtimeSinceStartup;
        GameLogger.Info($"登录成功 {JsonConvert.SerializeObject(ack)}, 登录时间为：UTC{TimeUtils.MilliTimeStampToDateTime(TimeUtils.LoginTimeServer)}, 协议延迟:{(TimeUtils.LoginRealTimeSinceStartUp - loginReqSendTime) * 1000} ms");
    }
}