using System;
using System.IO;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

// 基于 AVProboard 的视频播放管理器
public class AVManager : SingleTon<AVManager>
{
    public const string videoPath = "Content/Videos";
    public DisplayUGUI displayUGUI;

    public MediaPlayer mediaPlayer => displayUGUI == null ? null : displayUGUI.CurrentMediaPlayer;
    public IMediaControl control => mediaPlayer == null ? null : mediaPlayer.Control;
    public IMediaInfo mediaInfo => mediaPlayer == null ? null : mediaPlayer.Info;
    public IMediaSubtitles subtitles => mediaPlayer == null ? null : mediaPlayer.Subtitles;

    public Action onMetaDataReady;
    public Action onReadyToPlay;
    public Action onStarted;
    public Action onFirstFrameReady;
    public Action onFinishedPlaying;
    public Action onClosing;
    public Action onError;
    public Action onSubtitleChange;
    public Action onStalled;
    public Action onUnstalled;
    public Action onResolutionChanged;
    public Action onStartedSeeking;
    public Action onFinishedSeeking;
    public Action onStartedBuffering;
    public Action onFinishedBuffering;
    public Action onPropertiesChanged;
    public Action onPlaylistItemChanged;
    public Action onPlaylistFinished;
    public Action onTextTracksChanged;
    public Action onTextCueChanged;

    public Action<double> onTimeInMilliSecondUpdate; // 每帧调用，告知当前视频所处的时间，单位毫秒

    /// <summary>
    /// 初始化，如果MediaPlayer面板上的 "Auto Play" 被勾上，在初始化完毕之后会自动播放视频
    /// 这里只提供两个回调的初始化，其他回调可以自己设置
    /// </summary>
    /// <param name="videoName">视频名称，带后缀</param>
    /// <param name="displayUGUIGO">挂载DisplayUGUI和MediaPlayer组件的节点</param>
    /// <param name="volume">初始声音</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="subTitleName">字幕文件的名称，带后缀</param>
    /// <param name="onFinishedPlaying">视频结束的回调</param>
    /// <param name="onSubtitleChange">字幕改变的回调，回调中需要自己调用 GetSubtitleText() 来获取当前的字幕信息</param>
    public void Init(string videoName, GameObject displayUGUIGO, float volume = 1f, bool isLoop = false, string subTitleName = null, Action onFinishedPlaying = null, Action onSubtitleChange = null)
    {
        if (displayUGUIGO == null)
        {
            Debug.LogError($"param [displayUGUIGO] is not allowed to be null");
            return;
        }

        Reset();

        // 初始化组件
        displayUGUI = displayUGUIGO.GetOrAddComponent<DisplayUGUI>();
        displayUGUI.CurrentMediaPlayer = displayUGUIGO.GetOrAddComponent<MediaPlayer>();

        // 加载视频
        OpenVideoFromFile(videoName);
        if (!string.IsNullOrEmpty(subTitleName))
        {
            EnableSubtitles(subTitleName);
        }

        // 一些默认设置
        displayUGUI.color = Color.white;
        control.MuteAudio(false);
        control.SetVolume(volume);
        control.SetLooping(isLoop);
        control.SetPlaybackRate(1f);

        // 初始化事件
        var ticker = displayUGUIGO.GetOrAddComponent<AVTicker>();
        ticker.Init(mediaPlayer);
        ticker.onTimeInMilliSecondUpdate = OnTimeInMilliSecondUpdate;
        this.onFinishedPlaying = onFinishedPlaying;
        this.onSubtitleChange = onSubtitleChange;
        InitDefaultEvent();
        mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
    }

    public void Play()
    {
        control?.Play();
    }

    public void Pause()
    {
        control?.Pause();
    }

    public void Stop()
    {
        control?.Stop();
    }

    public void Rewind()
    {
        control?.Rewind();
    }

    /// 关闭视频，但是不会走 onFinishedPlaying 回调
    public void Close()
    {
        control?.CloseMedia();

        Reset();
    }

    /// 手动结束视频，会走 onFinishedPlaying 回调
    public void Finish()
    {
        control?.CloseMedia();
        onFinishedPlaying?.Invoke();

        Reset();
    }

    public void SetVolume(float volume)
    {
        control?.SetVolume(volume);
    }

    public void SetSpeed(float speed)
    {
        control?.SetPlaybackRate(speed);
    }

    public void SetLooping(bool isLoop)
    {
        control?.SetLooping(isLoop);
    }

    public void Seek(float timeMs)
    {
        control?.Seek(timeMs);
    }

    public void SeekFast(float timeMs)
    {
        control?.SeekFast(timeMs);
    }

    public void MuteAudio(bool isMute)
    {
        control?.MuteAudio(isMute);
    }

    /// 启用字幕
    public void EnableSubtitles(string subTitleName)
    {
#if UNITY_EDITOR
        mediaPlayer.EnableSubtitles(new MediaPath(Path.Combine(videoPath, subTitleName), MediaPathType.RelativeToDataFolder));
#else
            mediaPlayer.EnableSubtitles(new MediaPath(Path.Combine(videoPath, subTitleName), MediaPathType.RelativeToPersistentDataFolder));
#endif
    }

    public string GetSubtitleText()
    {
        return subtitles?.GetSubtitleText();
    }

    public double GetCurrentTimeMs()
    {
        return control?.GetCurrentTime() ?? 0;
    }

    public bool IsMuted()
    {
        return control?.IsMuted() ?? false;
    }

    public bool IsLooping()
    {
        return control?.IsLooping() ?? false;
    }

    public bool IsPlaying()
    {
        return control?.IsPlaying() ?? false;
    }

    public bool IsPaused()
    {
        return control?.IsPaused() ?? false;
    }

    public bool IsFinished()
    {
        return control?.IsFinished() ?? false;
    }

    public bool CanPlay()
    {
        return control?.CanPlay() ?? false;
    }

    public bool IsSeeking()
    {
        return control?.IsSeeking() ?? false;
    }

    #region private

    /// 加载和打开视频文件
    private void OpenVideoFromFile(string videoName)
    {
#if UNITY_EDITOR
        mediaPlayer.OpenMedia(new MediaPath(Path.Combine(videoPath, videoName), MediaPathType.RelativeToDataFolder));
#else
            mediaPlayer.OpenMedia(new MediaPath(Path.Combine(videoPath, videoName), MediaPathType.RelativeToPersistentDataFolder));
#endif
    }

    /// 绑定所有的视频事件
    private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        switch (et)
        {
            case MediaPlayerEvent.EventType.MetaDataReady:
                onMetaDataReady?.Invoke();
                break;
            case MediaPlayerEvent.EventType.ReadyToPlay:
                onReadyToPlay?.Invoke();
                break;
            case MediaPlayerEvent.EventType.Started:
                onStarted?.Invoke();
                break;
            case MediaPlayerEvent.EventType.FirstFrameReady:
                onFirstFrameReady?.Invoke();
                break;
            case MediaPlayerEvent.EventType.FinishedPlaying:
                onFinishedPlaying?.Invoke();
                break;
            case MediaPlayerEvent.EventType.Closing:
                onClosing?.Invoke();
                break;
            case MediaPlayerEvent.EventType.Error:
                onError?.Invoke();
                break;
            case MediaPlayerEvent.EventType.SubtitleChange:
                onSubtitleChange?.Invoke();
                break;
            case MediaPlayerEvent.EventType.Stalled:
                onStalled?.Invoke();
                break;
            case MediaPlayerEvent.EventType.Unstalled:
                onUnstalled?.Invoke();
                break;
            case MediaPlayerEvent.EventType.ResolutionChanged:
                onResolutionChanged?.Invoke();
                break;
            case MediaPlayerEvent.EventType.StartedSeeking:
                onStartedSeeking?.Invoke();
                break;
            case MediaPlayerEvent.EventType.FinishedSeeking:
                onFinishedSeeking?.Invoke();
                break;
            case MediaPlayerEvent.EventType.StartedBuffering:
                onStartedBuffering?.Invoke();
                break;
            case MediaPlayerEvent.EventType.FinishedBuffering:
                onFinishedBuffering?.Invoke();
                break;
            case MediaPlayerEvent.EventType.PropertiesChanged:
                onPropertiesChanged?.Invoke();
                break;
            case MediaPlayerEvent.EventType.PlaylistItemChanged:
                onPlaylistItemChanged?.Invoke();
                break;
            case MediaPlayerEvent.EventType.PlaylistFinished:
                onPlaylistFinished?.Invoke();
                break;
            case MediaPlayerEvent.EventType.TextTracksChanged:
                onTextTracksChanged?.Invoke();
                break;
        }
    }

    /// 每帧都会触发的回调，返回当前视频的所在的毫秒数
    private void OnTimeInMilliSecondUpdate(double mSec)
    {
        onTimeInMilliSecondUpdate?.Invoke(mSec);
    }

    /// 初始化一些默认的事件
    private void InitDefaultEvent()
    {
        onError = () => { Close(); };
    }

    /// 销毁和重置
    private void Reset()
    {
        if (mediaPlayer != null)
        {
            mediaPlayer.Events.RemoveAllListeners();
        }

        displayUGUI = null;
        onReadyToPlay = null;
        onStarted = null;
        onFirstFrameReady = null;
        onFinishedPlaying = null;
        onClosing = null;
        onError = null;
        onSubtitleChange = null;
        onStalled = null;
        onUnstalled = null;
        onResolutionChanged = null;
        onStartedSeeking = null;
        onFinishedSeeking = null;
        onStartedBuffering = null;
        onFinishedBuffering = null;
        onPropertiesChanged = null;
        onPlaylistItemChanged = null;
        onPlaylistFinished = null;
    }

    #endregion
}