using Framework;
using UnityEngine;

public class GameMain_Video : MonoBehaviour
{
    public GameObject videoRoot;
    void Start()
    {
        AVManager.Instance.Init("video_wangzherongyao_lobbyfadein.mp4", videoRoot, 1, false, null, () => { GameLogger.Log("视频结束了");});
    }
}
