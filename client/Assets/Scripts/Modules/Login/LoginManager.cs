using GameProtocol;
using Newtonsoft.Json;
using UnityEngine;

public class LoginManager: SingleTon<LoginManager>
{
    public LoginManager()
    {
        NetClient.Instance.RegisterProtocol("LoginAck", OnLoginAck);
    }

    public void StartLogin()
    {
        LoginReq msg = new LoginReq();
        NetClient.Instance.SendMessage(msg);
    }

    private void OnLoginAck(object msg)
    {
        var ack = msg as LoginAck;
        TimeUtils.LoginTimeServer = ack.LoginTime;
        TimeUtils.LoginRealTimeSinceStartUp = Time.realtimeSinceStartup;
        GameLogger.Info($"登录成功 {JsonConvert.SerializeObject(ack)}");
    }
}