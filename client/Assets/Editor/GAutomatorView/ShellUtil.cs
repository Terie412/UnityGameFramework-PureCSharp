using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GAutomatorView.Editor
{
	public static class Luban_ShellUtil
	{
		public static bool isRunning;
		public static Action<string> onProcessExited;
		public static Process p;
		public static string receivedData;

		public static void OnProcessOutputDataReceived(object sender, DataReceivedEventArgs  e)
		{
			receivedData += e.Data + "\n";
		}
		
		public static void OnProcessErrorDataReceived(object sender, DataReceivedEventArgs  e)
		{
			if (!string.IsNullOrEmpty(e.Data))
			{
				Debug.LogError(e.Data);			
			}
		}
		
		public static void AsyncExecute(string fileName, string args, Action<string> callback = null)
		{
			if (isRunning)
			{
				Debug.LogError("请等待上一个命令返回");
			}
			
			onProcessExited = callback;
			
			// ReSharper disable once UseObjectOrCollectionInitializer
			p = new Process();
			p.StartInfo.FileName = fileName;
			p.StartInfo.Arguments = args;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.CreateNoWindow = true;

			isRunning = true;

			receivedData = "";
			p.OutputDataReceived += OnProcessOutputDataReceived;
			p.ErrorDataReceived += OnProcessErrorDataReceived;
			p.EnableRaisingEvents = true;
			p.Exited += ExitedHandler;
			
			p.Start();
			
			p.BeginOutputReadLine();
			p.BeginErrorReadLine();
		}
		
		public static string Execute(string fileName, string cmd)
		{
			// ReSharper disable once UseObjectOrCollectionInitializer
			Process p2 = new Process();
			p2.StartInfo.FileName = fileName;
			p2.StartInfo.Arguments = cmd;
			p2.StartInfo.UseShellExecute = false;
			p2.StartInfo.RedirectStandardOutput = true;
			p2.StartInfo.RedirectStandardError = true;
			p2.StartInfo.CreateNoWindow = true;
			
			p2.Start();
			
			string output = p2.StandardOutput.ReadToEnd();
			p2.WaitForExit();
			p2.Close();

			Debug.Log($"adb {cmd}\n--- output ---\n{output}");
			return output;
		}
		
		public static void ExitedHandler(object sender, EventArgs e)
		{
			Debug.Log($"执行返回：{receivedData}");
			p.Close();
			isRunning = false;
			var handle = onProcessExited;
			onProcessExited = null;
			handle?.Invoke(receivedData);
		}
	}
}