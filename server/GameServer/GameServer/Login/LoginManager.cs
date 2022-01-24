using GameProtocol;
using KCPNet;

public class LoginManager: SingleTon<LoginManager>
{
    public void Init()
    {
        ServerMain.RegisterProtocol("LoginReq", OnLoginReq);
    }

    private void OnLoginReq(object obj, KCPSession session)
    {
        KCPNetLogger.Info($"OnLoginReq: {session.sid}");

        LoginAck ack = new LoginAck();
        ack.Sid = session.sid;
        ack.LoginTime = TimeUtils.GetTimeStamp();
        ack.PlayerInfo = new PlayInfo();
        ack.PlayerInfo.Name = "qintianchen";
        ack.PlayerInfo.Uid = 15614884;
            
        ServerMain.SendMessage(ack, session);
    }
}