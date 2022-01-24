using System;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;


public class GameLogger : SingleTon<GameLogger>
{
    private string logDir
    {
        get
        {
#if UNITY_EDITOR
            return "./GameLogs";
#else
            return Application.persistentDataPath + "/Logs";
#endif
        }
    }

    private string curLogPath;
    private FileStream curFileStream;

    public void Init()
    {
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        var dateStr = DateTime.Now.ToString().Replace("/", "_").Replace(":", "_").Replace(" ", "_");
        curLogPath = logDir + $"/Log_{dateStr}.txt";
        File.Create(curLogPath).Close();
        Application.logMessageReceived += Output;
    }

    private void Output(string aCondition, string aStackTrace, LogType aType)
    {
        try
        {
            // 添加\t是方便某些能够以\t作为纯文本文件块折叠依据的文本编辑器查看日志
            var stackTraceSB = new StringBuilder(aStackTrace).Replace("\n", "\n\t");
            var res = new StringBuilder().Append(DateTime.Now);

            switch (aType)
            {
                case LogType.Log:
                    res.Append(" LOG:");
                    break;
                case LogType.Warning:
                    res.Append(" WARNING:");
                    break;
                case LogType.Error:
                    res.Append(" ERROR:");
                    break;
                default:
                    res.Append(" UNKNOWN:");
                    break;
            }

            res.Append(aCondition).Append("\nstacktrace:\n\t").Append(stackTraceSB);
            res.Replace("\n", "\n\t").Append("\n");
            byte[] bytes = new UTF8Encoding(true).GetBytes(res.ToString());
            using (curFileStream = File.Open(curLogPath, FileMode.OpenOrCreate))
            {
                curFileStream.Seek(0, SeekOrigin.End);
                curFileStream.Write(bytes, 0, bytes.Length);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public static void Info(string msg)
    {
        msg = "[" + Time.frameCount + "] " + msg;
        Debug.Log(msg);
    }

    public static void Warning(string msg)
    {
        msg = "[" + Time.frameCount + "] " + msg;
        Debug.LogWarning(msg);
    }

    public static void Error(string msg)
    {
        msg = "[" + Time.frameCount + "] " + msg;
        Debug.LogError(msg);
    }
}