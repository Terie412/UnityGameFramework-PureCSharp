
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Test : MonoBehaviour
{
    private IEnumerator Start()
    {
        CancellationTokenSource cts = new();
        Task t = new(() =>
        {
            Debug.Log($"运行1");
            Thread.Sleep(3000);
            Debug.Log(cts.Token.IsCancellationRequested ? $"取消了" : $"运行2");
        });
        t.Start();

        yield return new WaitForSeconds(1);

        Debug.Log($"取消");
        cts.Cancel();
    }
}