using System.Collections.Generic;
using System.IO;
using GameProtocol;
using Google.Protobuf.Reflection;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class GenerateProtocolCode
{
    public static string Client_ProtocolHandlerPath = "Assets/Scripts/Net/ProtocolHandler.cs";
    public static string Client_ProtocolDispatcherPath = "Assets/Scripts/Net/ProtocolDispatcher.cs";
    public static string Server_ProtocolHandlerPath = "../server/GameServer/GameServer/ProtocolHandler.cs";
    public static string Server_ProtocolDispatcherPath = "../server/GameServer/GameServer/ProtocolDispatcher.cs";

    [MenuItem("Tools/ReadProtobufFile")]
    static void DoRead()
    {
        IList<MessageDescriptor> messageTypes = ProtocolAllReflection.Descriptor.MessageTypes;
        List<string> msgNameList = new();
        foreach (var messageDescriptor in messageTypes)
        {
            var name = messageDescriptor.Name;
            if (name.EndsWith("Ntf") || name.EndsWith("Ack") || name.EndsWith("Req"))
            {
                msgNameList.Add(name);
            }
        }

        Debug.Log(JsonConvert.SerializeObject(msgNameList));

        GenerateProtocolHandler(msgNameList, true);
        GenerateProtocolDispatcher(msgNameList, true);
        GenerateProtocolHandler(msgNameList, false);
        GenerateProtocolDispatcher(msgNameList, false);
    }

    static void GenerateProtocolDispatcher(List<string> msgNameList, bool isClient)
    {
        File.Create(isClient ? Client_ProtocolDispatcherPath : Server_ProtocolDispatcherPath).Close();
        var ret = "using System;\nusing System.Collections.Generic;\nusing GameProtocol;\nusing KCPNet;\n\npublic class ProtocolDispatcher\n{\n";

        ret += "\tpublic static Dictionary<string, uint> name_id = new Dictionary<string, uint>()\n\t{\n";
        for (int i = 0; i < msgNameList.Count; i++)
        {
            var name = msgNameList[i];
            var line = $"\t\t{{\"{name}\", {i + 1}}},\n";
            ret += line;
        }

        ret += "\t};\n";

        ret += "\n";

        if (isClient)
        {
            ret += "\tprivate static Dictionary<uint, Action<byte[]>> id_parser = new Dictionary<uint, Action<byte[]>>()\n\t{\n";
        }
        else
        {
            ret += "\tprivate static Dictionary<uint, Action<byte[], KCPSession>> id_parser = new Dictionary<uint, Action<byte[], KCPSession>>()\n\t{\n";
        }

        for (int i = 0; i < msgNameList.Count; i++)
        {
            var name = msgNameList[i];
            var line = $"\t\t{{{i + 1}, {name}Parser}},\n";
            ret += line;
        }

        ret += "\t};\n";

        ret += "\n";

        if (isClient)
        {
            ret += "\tpublic static void Dispatch(byte[] bytes)\n";
        }
        else
        {
            ret += "\tpublic static void Dispatch(byte[] bytes, KCPSession session)\n";
        }

        ret += "\t{\n";
        ret += "\t\ttry\n";
        ret += "\t\t{\n";
        ret += "\t\t\tProtocol p = Protocol.Parser.ParseFrom(bytes);\n";
        if (isClient)
        {
            ret += "\t\t\tid_parser[p.Id]?.Invoke(p.Data.ToByteArray());\n";
        }
        else
        {
            ret += "\t\t\tid_parser[p.Id]?.Invoke(p.Data.ToByteArray(), session);\n";
        }

        ret += "\t\t}\n";
        ret += "\t\tcatch(Exception e)\n";
        ret += "\t\t{\n";
        ret += "\t\t\tKCPNetLogger.Error(e.ToString());\n";
        ret += "\t\t}\n";

        ret += "\t}\n";

        ret += "\n";

        if (isClient)
        {
            ret += "\tpublic static void RegisterProtocol(string protocolName, Action<object> callback)\n";
        }
        else
        {
            ret += "\tpublic static void RegisterProtocol(string protocolName, Action<object, KCPSession> callback)\n";
        }

        ret += "\t{\n";
        ret += "\t\tif (!name_id.TryGetValue(protocolName, out var id)) return;\n";
        ret += "\n";
        ret += "\t\tswitch (id)\n";
        ret += "\t\t{\n";
        for (int i = 0; i < msgNameList.Count; i++)
        {
            var name = msgNameList[i];
            ret += $"\t\t\tcase {i + 1}: ProtocolHandler.on{name} += callback; break;\n";
        }
        ret += "\t\t}\n";
        ret += "\t}\n";
        
        ret += "\n";
        
        for (int i = 0; i < msgNameList.Count; i++)
        {
            var name = msgNameList[i];
            if (isClient)
            {
                ret += $"\tprivate static void {name}Parser(byte[] bytes) {{ object p = {name}.Parser.ParseFrom(bytes); ProtocolHandler.on{name}?.Invoke(p); }}\n";
            }
            else
            {
                ret += $"\tprivate static void {name}Parser(byte[] bytes, KCPSession session) {{ object p = {name}.Parser.ParseFrom(bytes); ProtocolHandler.on{name}?.Invoke(p, session); }}\n";
            }
        }

        ret += "}\n";
        
            
        File.WriteAllText(isClient ? Client_ProtocolDispatcherPath : Server_ProtocolDispatcherPath, ret);
    }

    static void GenerateProtocolHandler(List<string> msgNameList, bool isClient)
    {
        File.Create(isClient ? Client_ProtocolHandlerPath : Server_ProtocolHandlerPath).Close();
        var ret = "using System;\nusing KCPNet;\n\npublic static class ProtocolHandler\n{\n";
        for (int i = 0; i < msgNameList.Count; i++)
        {
            var name = msgNameList[i];
            string line;
            if (isClient)
            {
                line = $"\tpublic static Action<object> on{name};\n";
            }
            else
            {
                line = $"\tpublic static Action<object, KCPSession> on{name};\n";
            }

            ret += line;
        }

        ret += "}";
        File.WriteAllText(isClient ? Client_ProtocolHandlerPath : Server_ProtocolHandlerPath, ret);
    }
}