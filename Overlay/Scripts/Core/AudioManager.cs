using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChannelPointRewardsType = TwitchChannelPointRewardsManager.ChannelPointRewardsType;
using FragmentType = TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragment.FragmentType;
using NodeType = NodeDirectory.NodeType;

public sealed partial class AudioManager : Node
{
    public override void _EnterTree()
    {
        RetrieveResources();
        RetrieveSoundAlerts();
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
    }

    public enum BusLayoutType : uint
    {
        Master = 0u,
        SoundAlert
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

    public Action ChangedSoundtrack = null;

    public void PlayTextToSpeech(
        string text
    )
    {
        DisplayServer.TtsSpeak(
            text,
            m_textToSpeechId
        );
    }

    private const int c_minimumBitsForTextToSpeech = 50;
    private const int c_soundAlertDelayInMilliseconds = 1000;
    private const int c_songStartIndex = -1;

    private Dictionary<SoundAlertType, AudioStreamPlayer> m_soundAlerts = new();
    private Queue<SoundAlertType> m_soundAlertsQueue = new();
    private bool m_isSoundAlertPlaying = false;
    private string m_textToSpeechId = string.Empty;

    private void BindTwitchChannelPointRewards()
    {
        var twitchChannelPointRewardsManager = GetNode<TwitchChannelPointRewardsManager>(
            NodeDirectory.NodePaths[NodeType.TwitchChannelPointRewardsManager]
        );

        twitchChannelPointRewardsManager.RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertApplause] = OnChannelPointRewardsRedeemedApplause;
        twitchChannelPointRewardsManager.RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertFirstBlood] = OnChannelPointRewardsRedeemedFirstBlood;
        twitchChannelPointRewardsManager.RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertGodlike] = OnChannelPointRewardsRedeemedGodlike;
        twitchChannelPointRewardsManager.RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertHeartbeat] = OnChannelPointRewardsRedeemedHeartbeat;
        twitchChannelPointRewardsManager.RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertHolyShit] = OnChannelPointRewardsRedeemedHolyShit;
        twitchChannelPointRewardsManager.RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertHowdy] = OnChannelPointRewardsRedeemedHowdy;
        twitchChannelPointRewardsManager.RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertKegExplosion] = OnChannelPointRewardsRedeemedKegExplosion;
        twitchChannelPointRewardsManager.RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertKegFuse] = OnChannelPointRewardsRedeemedKegFuse;
        twitchChannelPointRewardsManager.RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertNice] = OnChannelPointRewardsRedeemedNice;

        twitchChannelPointRewardsManager.ReedambleRewardsWithStringInput[ChannelPointRewardsType.TextToSpeech] = OnChannelPointRewardsRedeemedTextToSpeech;
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
           length * secondsToMilliseconds
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

    private void OnChannelPointRewardsRedeemedTextToSpeech(
        string text    
    )
    {
        PlayTextToSpeech(
            text    
        );
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

    private void RetrieveResources()
    {
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
}