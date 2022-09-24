using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 日志系统
    /// 1. 将 UnityEngine.Debug 输出的日志写到文件内
    /// 2. 游戏启动时调用 GameLogger.Instance.Init() 进行初始化
    /// 3. 定时调用 GameLogger.Instance.Update() 以驱动日志系统正常运作
    /// 4. 走 GameLogger.Log() 静态接口可以打印出额外的信息，如帧率
    /// </summary>
    public class GameLogger : SingleTon<GameLogger>
    {
        private struct Message
        {
            public string aCondition;
            public string aStackTrace;
            public LogType aType;
        }

        public bool isInit = false;

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
        private ConcurrentQueue<Message> messages = new();

        public void Init()
        {
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            var dateStr = DateTime.Now.ToString(CultureInfo.InvariantCulture).Replace("/", "_").Replace(":", "_").Replace(" ", "_");
            curLogPath = logDir + $"/Log_{dateStr}.txt";
            File.Create(curLogPath).Close();
            Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
            isInit = true;
        }

        private void OnLogMessageReceivedThreaded(string aCondition, string aStackTrace, LogType aType)
        {
            messages.Enqueue(new Message { aCondition = aCondition, aStackTrace = aStackTrace, aType = aType });
        }

        public void Update()
        {
            Flush();
        }

        public void OnApplicationQuit()
        {
            Flush();
        }

        private void Flush()
        {
            if (messages.IsEmpty)
            {
                return;
            }

            var messageArray = messages.ToArray();
            Task.Run(() => { WriteMessagesToFile(messageArray); });
        }

        private void WriteMessagesToFile(IEnumerable<Message> msgs)
        {
            foreach (var msg in msgs)
            {
                try
                {
                    // 添加\t是方便某些能够以\t作为纯文本文件块折叠依据的文本编辑器查看日志
                    var stackTraceSB = new StringBuilder(msg.aStackTrace).Replace("\n", "\n\t");
                    var res = new StringBuilder().Append(DateTime.Now);

                    switch (msg.aType)
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
                        case LogType.Assert:
                            res.Append(" Assert:");
                            break;
                        case LogType.Exception:
                            res.Append(" Exception:");
                            break;
                        default:
                            res.Append(" UNKNOWN:");
                            break;
                    }

                    res.Append(msg.aCondition).Append("\nstacktrace:\n\t").Append(stackTraceSB);
                    res.Replace("\n", "\n\t").Append("\n");
                    byte[] bytes = new UTF8Encoding(true).GetBytes(res.ToString());
                    using var curFileStream = File.Open(curLogPath, FileMode.OpenOrCreate);
                    curFileStream.Seek(0, SeekOrigin.End);
                    curFileStream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

        public static void Log(string msg)
        {
            Debug.Log($"[{Time.frameCount}]" + msg);
        }

        public static void LogWarning(string msg)
        {
            Debug.LogWarning($"[{Time.frameCount}]" + msg);
        }

        public static void LogError(string msg)
        {
            Debug.LogError($"[{Time.frameCount}]" + msg);
        }


    }
}