using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : SingleTon<VideoManager>
{
    public VideoPlayer videoPlayer;

    private RawImage displayUI;

    public void Init(string videoName, GameObject displayGo, bool isLoop)
    {
        if (displayGo == null)
        {
            GameLogger.LogError("Display GameObject can not be null!");
            return;
        }

        videoPlayer = displayGo.GetOrAddComponent<VideoPlayer>();
        displayUI = displayGo.GetOrAddComponent<RawImage>();

        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.isLooping = isLoop;
        videoPlayer.skipOnDrop = true;
        videoPlayer.playbackSpeed = 1;

        displayUI.gameObject.SetActive(false);
        AssetManager.Instance.LoadAssetAsync<VideoClip>(videoName, videoPlayer, videoClip =>
        {
            videoPlayer.clip = videoClip;
            RenderTexture rt = RenderTexture.GetTemporary((int) videoClip.width, (int) videoClip.height);
            videoPlayer.targetTexture = rt;
            displayUI.texture = rt;
        });
    }

    public void Play()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            displayUI.gameObject.SetActive(true);
            videoPlayer.Play();
        }
    }

    public void Pause()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }
    }
}