using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChannelPointRewardsType = TwitchChannelPointRewardsManager.ChannelPointRewardsType;
using FragmentType = TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragment.FragmentType;
using KeyBindType = InputManager.KeyBindType;
using NodeType = NodeDirectory.NodeType;

public sealed partial class AudioManager : Node
{
    public override void _EnterTree()
    {
        RetrieveResources();
        RetrieveSoundAlerts();
        RetrievePlaylists();
        BindInputEvents();
        BindTwitchChannelPointRewards();
        BindTwitchCheer();
    }

    public override void _Process(
        double delta
    )
    {
        if (m_soundAlertsQueue.Count > 0u && !m_isSoundAlertPlaying)
        {
            SoundAlertType soundAlertType = m_soundAlertsQueue.Dequeue();
            PlaySoundAlert(
                soundAlertType
            );
        }

        if (m_isPlaylistQueued)
        {
            PlayQueuedPlaylist(
                m_queuedPlaylistType    
            );
        }
        else if (!m_currentSong.Playing)
        {
            PlayNextSong();
        }
    }

    public override void _Ready()
    {
        PlaylistType randomPlaylist = (PlaylistType)(GD.Randi() % (uint)PlaylistType.Count);
        PlayQueuedPlaylist(
            randomPlaylist
        );
    }

    public enum BusLayoutType : uint
    {
        Master = 0u,
        Soundtrack,
        SoundAlert
    }

    public enum BusLayoutSoundtrackEffectType : uint
    {
        SpectrumAnalyzer = 0u
    }

    public enum PlaylistType : uint
    {
        Gaming = 0u,
        Lofi,
        Count
    }

    public enum SoundAlertType : uint
    {
        Applause = 0u,
        FirstBlood,
        Godlike,
        Heartbeat,
        HolyShit,
        Howdy,
        KegExplosion,
        KegFuse,
        Nice,
    }

    public Action ChangedPlaylist = null;
    public Action ChangedSoundtrack = null;

    public string GetCurrentPlaylistName()
    {
        return m_currentPlaylistType.ToString();
    }

    public PlaylistType GetCurrentPlaylistType()
    {
        return m_currentPlaylistType;
    }

    public string GetCurrentSongName()
    {
        return m_playlists[m_currentPlaylistType][m_currentSongIndex].Name;
    }

    public bool IsMuted()
    {
        return m_isMuted || m_currentVolume == 0f;
    }

    public void PlayTextToSpeech(
        string text    
    )
    {
        DisplayServer.TtsSpeak(
            text,
            m_textToSpeechId
        );
    }

    public void QueuePlaylist(
        PlaylistType playlistType
    )
    {
        m_queuedPlaylistType = playlistType;
        m_isPlaylistQueued = true;
    }

    public void SkipSong()
    {
        StopCurrentSong();
    }

    private const float c_volumeIncrement = 0.0075f;
    private const int c_minimumBitsForTextToSpeech = 50;
    private const int c_soundAlertDelayInMilliseconds = 1000;
    private const int c_songStartIndex = -1;
    private const string c_audioBusSong = "Song";

    private Dictionary<PlaylistType, List<AudioStreamPlayer>> m_playlists = new();
    private Dictionary<SoundAlertType, AudioStreamPlayer> m_soundAlerts = new();
    private Queue<SoundAlertType> m_soundAlertsQueue = new();
    private PlaylistType m_currentPlaylistType = PlaylistType.Lofi;
    private PlaylistType m_queuedPlaylistType = PlaylistType.Lofi;
    private AudioStreamPlayer m_currentSong = null;
    private bool m_isMuted = false;
    private bool m_isSoundAlertPlaying = false;
    private bool m_isPlaylistQueued = false;
    private float m_currentVolume = 1f;
    private int m_audioBusIndexSong = 0;
    private int m_currentSongIndex = c_songStartIndex;
    private string m_textToSpeechId = string.Empty;

    private void BindInputEvents()
    {
        var inputManager = GetNode<InputManager>(
            NodeDirectory.NodePaths[NodeType.InputManager]
        );

        inputManager.KeyBindPressed[KeyBindType.AudioManagerPlaylistGaming] += OnPressedPlaylistGaming;
        inputManager.KeyBindPressed[KeyBindType.AudioManagerPlaylistLofi] += OnPressedPlaylistLofi;
        inputManager.KeyBindPressed[KeyBindType.AudioManagerSoundtrackNext] += OnPressedSongNext;
        inputManager.KeyBindPressed[KeyBindType.AudioManagerToggleMute] += OnPressedSongToggleMute;
        inputManager.KeyBindPressing[KeyBindType.AudioManagerVolumeDown] += OnPressingSongVolumeDecrease;
        inputManager.KeyBindPressing[KeyBindType.AudioManagerVolumeUp] += OnPressingSongVolumeIncrease;
    }

    private void BindTwitchChannelPointRewards()
    {
        var twitchChannelPointRewardsManager = GetNode<TwitchChannelPointRewardsManager>(
            NodeDirectory.NodePaths[NodeType.TwitchChannelPointRewardsManager]
        );

        twitchChannelPointRewardsManager.RedeemableRewards[ChannelPointRewardsType.SoundAlertApplause] = OnChannelPointRewardsRedeemedApplause;
        twitchChannelPointRewardsManager.RedeemableRewards[ChannelPointRewardsType.SoundAlertFirstBlood] = OnChannelPointRewardsRedeemedFirstBlood;
        twitchChannelPointRewardsManager.RedeemableRewards[ChannelPointRewardsType.SoundAlertGodlike] = OnChannelPointRewardsRedeemedGodlike;
        twitchChannelPointRewardsManager.RedeemableRewards[ChannelPointRewardsType.SoundAlertHeartbeat] = OnChannelPointRewardsRedeemedHeartbeat;
        twitchChannelPointRewardsManager.RedeemableRewards[ChannelPointRewardsType.SoundAlertHolyShit] = OnChannelPointRewardsRedeemedHolyShit;
        twitchChannelPointRewardsManager.RedeemableRewards[ChannelPointRewardsType.SoundAlertHowdy] = OnChannelPointRewardsRedeemedHowdy;
        twitchChannelPointRewardsManager.RedeemableRewards[ChannelPointRewardsType.SoundAlertKegExplosion] = OnChannelPointRewardsRedeemedKegExplosion;
        twitchChannelPointRewardsManager.RedeemableRewards[ChannelPointRewardsType.SoundAlertKegFuse] = OnChannelPointRewardsRedeemedKegFuse;
        twitchChannelPointRewardsManager.RedeemableRewards[ChannelPointRewardsType.SoundAlertNice] = OnChannelPointRewardsRedeemedNice;
    }

    private void BindTwitchCheer()
    {
        var twitchManager = GetNode<TwitchManager>(
            NodeDirectory.NodePaths[NodeType.TwitchManager]
        );

        twitchManager.ChannelChatNotification += OnChannelChatNotification;
    }

    private int GetStreamLengthInMilliseconds(
        AudioStream stream
    )
    {
        const int secondsToMilliseconds = 1000;
        double length = stream.GetLength();
        return Mathf.RoundToInt(
           stream.GetLength() * secondsToMilliseconds
        );
    }

    private void OnChannelChatNotification(
        TwitchWebSocketMessagePayloadEventChannelChatNotification @event
    )
    {
        var message = @event.message;
        int totalBits = 0;
        string text = message.text;

        foreach (var fragment in message.fragments)
        {
            var fragmentType = fragment.GetFragmentType();
            if (fragmentType == FragmentType.Cheermote)
            {
                var cheermote = fragment.cheermote;
                totalBits += cheermote.bits;

                text = text.Replace(
                    cheermote.prefix,
                    string.Empty
                );
            }
        }

        if (totalBits >= c_minimumBitsForTextToSpeech)
        {
            PlayTextToSpeech(
                text
            );
        }
    }

    private void OnChannelPointRewardsRedeemedApplause()
    {
        m_soundAlertsQueue.Enqueue(
            SoundAlertType.Applause
        );
    }

    private void OnChannelPointRewardsRedeemedFirstBlood()
    {
        m_soundAlertsQueue.Enqueue(
            SoundAlertType.FirstBlood
        );
    }

    private void OnChannelPointRewardsRedeemedGodlike()
    {
        m_soundAlertsQueue.Enqueue(
            SoundAlertType.Godlike
        );
    }

    private void OnChannelPointRewardsRedeemedHeartbeat()
    {
        m_soundAlertsQueue.Enqueue(
            SoundAlertType.Heartbeat
        );
    }

    private void OnChannelPointRewardsRedeemedHolyShit()
    {
        m_soundAlertsQueue.Enqueue(
            SoundAlertType.HolyShit
        );
    }

    private void OnChannelPointRewardsRedeemedHowdy()
    {
        m_soundAlertsQueue.Enqueue(
            SoundAlertType.Howdy
        );
    }

    private void OnChannelPointRewardsRedeemedKegExplosion()
    {
        m_soundAlertsQueue.Enqueue(
            SoundAlertType.KegExplosion
        );
    }

    private void OnChannelPointRewardsRedeemedKegFuse()
    {
        m_soundAlertsQueue.Enqueue(
            SoundAlertType.KegFuse
        );
    }

    private void OnChannelPointRewardsRedeemedNice()
    {
        m_soundAlertsQueue.Enqueue(
            SoundAlertType.Nice
        );
    }

    private void OnPressedPlaylistGaming()
    {
        QueuePlaylist(
            PlaylistType.Gaming
        );
    }

    private void OnPressedPlaylistLofi()
    {
        QueuePlaylist(
            PlaylistType.Lofi
        );
    }

    private void OnPressedSongNext()
    {
        StopCurrentSong();
    }

    private void OnPressedSongToggleMute()
    {
        SetAudioBusVolume(
            m_audioBusIndexSong,
            m_isMuted ? m_currentVolume : 0f
        );
        m_isMuted = !m_isMuted;

#if DEBUG
        GD.Print(
            $"{nameof(AudioManager)}.{nameof(OnPressedSongToggleMute)}() - Song {(m_isMuted ? "muted" : "unmuted")}."
        );
#endif
    }

    private void OnPressingSongVolumeIncrease()
    {
        if (m_currentVolume < 1f)
        {
            m_currentVolume += c_volumeIncrement;
            if (m_currentVolume > 1f)
            {
                m_currentVolume = 1f;
            }

            SetAudioBusVolume(
                m_audioBusIndexSong,
                m_currentVolume
            );
        }
    }

    private void OnPressingSongVolumeDecrease()
    {
        if (m_currentVolume > 0f)
        {
            m_currentVolume -= c_volumeIncrement;
            if (m_currentVolume < 0f)
            {
                m_currentVolume = 0f;
            }

            SetAudioBusVolume(
                m_audioBusIndexSong,
                m_currentVolume
            );
        }
    }

    private void PlayNextSong()
    {
        if (++m_currentSongIndex == m_playlists[m_currentPlaylistType].Count)
        {
            m_currentSongIndex = 0;
        }

        m_currentSong = m_playlists[m_currentPlaylistType][m_currentSongIndex];
        m_currentSong.Play();
        ChangedSoundtrack?.Invoke();

#if DEBUG
        GD.Print(
            $"{nameof(AudioManager)}.{nameof(PlayNextSong)}() - Playing song '{m_currentSong.Name}'."
        );
#endif
    }

    private void PlayQueuedPlaylist(
        PlaylistType playlistType
    )
    {
        StopCurrentSong();

        m_currentPlaylistType = playlistType;
        m_currentSongIndex = c_songStartIndex;
        m_isPlaylistQueued = false;

        PlayNextSong();

        ChangedPlaylist?.Invoke();
    }

    private async void PlaySoundAlert(
        SoundAlertType soundAlertType
    )
    {
        AudioStreamPlayer soundAlert = m_soundAlerts[soundAlertType];
        soundAlert.Play();
        m_isSoundAlertPlaying = true;

        int streamLength = GetStreamLengthInMilliseconds(
            soundAlert.Stream
        );
        await Task.Delay(
            streamLength
        );

        soundAlert.Stop();

        await Task.Delay(
            c_soundAlertDelayInMilliseconds
        );

        m_isSoundAlertPlaying = false;
    }

    private void RetrievePlaylists()
    {
#if DEBUG
        GD.Print(
            $"{nameof(AudioManager)}.{nameof(RetrievePlaylists)}() - Retrieving playlists."
        );
#endif

        Random random = new();
        const string nodeNamePlaylists = "Playlists";
        var nodePlaylists = GetNode(
            nodeNamePlaylists
        );
        var playlistTypes = Enum.GetValues<PlaylistType>();
        foreach (var playlistType in playlistTypes)
        {
            if (playlistType == PlaylistType.Count)
            {
                continue;
            }

            var nodes = nodePlaylists.GetNode(
                playlistType.ToString()
            ).GetChildren();

            m_playlists.Add(
                playlistType,
                new()
            );

            List<AudioStreamPlayer> randomizedList = new();
            foreach (var node in nodes)
            {
                var song = node as AudioStreamPlayer;

                randomizedList.Add(
                    song
                );

#if DEBUG
                GD.Print(
                    $"{nameof(AudioManager)}.{nameof(RetrievePlaylists)}() - Song retrieved: {song.Name}."
                );
#endif
            }

            while (randomizedList.Count > 0)
            {
                int randomIndex = (int)(GD.Randi() % randomizedList.Count);
                var song = randomizedList[randomIndex];
                m_playlists[playlistType].Add(
                    song
                );

#if DEBUG
                GD.Print(
                    $"{nameof(AudioManager)}.{nameof(RetrievePlaylists)}() - Song added: {song.Name}."
                );
#endif

                randomizedList.RemoveAt(randomIndex);
            }

#if DEBUG
            GD.Print(
                $"{nameof(AudioManager)}.{nameof(RetrievePlaylists)}() - Number of {playlistType} Soundtracks: {m_playlists[playlistType].Count}."
            );
#endif
        }

#if DEBUG
        GD.Print(
            $"{nameof(AudioManager)}.{nameof(RetrievePlaylists)}() - Number of playlists: {m_playlists.Count}."
        );
#endif
    }

    private void RetrieveResources()
    {
        m_audioBusIndexSong = AudioServer.GetBusIndex(
            c_audioBusSong
        );

        var voices = DisplayServer.TtsGetVoicesForLanguage(
            "en"
        );
        m_textToSpeechId = voices[0u];
    }

    private void RetrieveSoundAlerts()
    {
#if DEBUG
        GD.Print(
            $"{nameof(AudioManager)}.{nameof(RetrieveSoundAlerts)}() - Retrieving sound alerts."
        );
#endif

        const string nodeNameSoundAlerts = "SoundAlerts";
        var soundAlertsNode = GetNode(
            nodeNameSoundAlerts
        );

        var soundAlertTypes = Enum.GetValues<SoundAlertType>();
        foreach (var soundAlertType in soundAlertTypes)
        {
            var soundAlert = soundAlertsNode.GetNode<AudioStreamPlayer>(
                soundAlertType.ToString()
            );

            m_soundAlerts.Add(
                soundAlertType,
                soundAlert
            );

#if DEBUG
            GD.Print(
                $"{nameof(AudioManager)}.{nameof(RetrieveSoundAlerts)}() - Sound Alert retrieved: {soundAlert.Name}."
            );
#endif
        }

#if DEBUG
        GD.Print(
            $"{nameof(AudioManager)}.{nameof(RetrieveSoundAlerts)}() - Number of Sound Alerts: {m_soundAlerts.Count}."
        );
#endif
    }

    private void SetAudioBusVolume(
        int audioBusIndex,
        float volume
    )
    {
        float decibels = Mathf.LinearToDb(
            volume
        );
        AudioServer.SetBusVolumeDb(
            audioBusIndex,
            decibels
        );
    }

    private void StopCurrentSong()
    {
        if (m_currentSongIndex == c_songStartIndex)
        {
            return;
        }

        AudioStreamPlayer songPrevious = m_playlists[m_currentPlaylistType][m_currentSongIndex];
        if (songPrevious.Playing)
        {
#if DEBUG
            GD.Print(
                $"{nameof(AudioManager)}.{nameof(PlayQueuedPlaylist)}() - Stopping previous playlist song '{songPrevious.Name}'."
            );
#endif
            songPrevious.Stop();
        }
    }
}