using UnityEngine;
using UnityEngine.UI;
using networkengine;
using Google.Protobuf;
using Msg;
using System.IO;

public delegate void Connected();
public delegate void FailConnected();
public delegate void DisConnected();
public delegate void Data(Protocol data);

public class protonet
{
    public static Connected ConnectedHandler;
    public static FailConnected FailConnectedHandler;
    public static DisConnected DisConnectedHandler;
    public static Data DataHandler;

    public static void SetConnectedCall(Connected callback = null)
    {
        ConnectedHandler = callback;
    }

    public static void SetFailConnectedCall(FailConnected callback = null)
    {
        FailConnectedHandler = callback;
    }

    public static void SetDisConnectedCall(DisConnected callback = null)
    {
        DisConnectedHandler = callback;
    }

    public static void SetDataCall(Data callback = null)
    {
        DataHandler = callback;
    }


    public static void ConnectServer()
    {
		Client.Instance.Connect(Common.SServer, Common.SPort, eventDelegate);
    }

    public static void DisconnectServer()
    {
        Client.Instance.Disconnect();
    }

    public static void SendData(Protocol msg)
    {
        //Client.Instance.Send(new byte[5]);

        //ProtoBuf.Serializer.Serialize()

        //        Protocol msg = new Protocol();
        //        msg.Msgid = MessageID.LoginReq;
        //        msg.loginReq = new LoginReq();
        //        msg.loginReq.Type = LoginType.Facebook;
        //        msg.loginReq.Fb = new LoginFBReq();
        //        msg.loginReq.Fb.FbId = "00001";
        //        msg.loginReq.Fb.Token = "adsfasdf";
        //        msg.loginReq.Fb.Name = "Walter";
        using (var stream = new MemoryStream())
        {
			msg.WriteTo(stream);
            Client.Instance.Send(stream.ToArray());
        }
    }

    private static void eventDelegate(networkengine.Event e, byte[] data)
    {
        switch (e)
        {
            case networkengine.Event.Connected:
                showtext("Connected.");
                if (ConnectedHandler != null) { ConnectedHandler(); }
                break;

            case networkengine.Event.FailedConnect:
                showtext("Failed to connect.");
                if (FailConnectedHandler != null) { FailConnectedHandler(); }
                break;

            case networkengine.Event.Disconnect:
                showtext("Disconnected.");
                if (DisConnectedHandler != null) { DisConnectedHandler(); }
                break;

            case networkengine.Event.Data:
                Protocol msg = Protocol.Parser.ParseFrom(data);
                if (DataHandler != null) { DataHandler(msg); }
                break;
        }
    }

    private static void showtext(string txt)
    {
        Loom.QueueOnMainThread(() =>
        {
            Debug.Log(txt);
        });
    }
}
