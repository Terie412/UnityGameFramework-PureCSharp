using GameProtocol;

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
        GameLogger.Info($"登录成功 {ack.Sid}");
    }
}