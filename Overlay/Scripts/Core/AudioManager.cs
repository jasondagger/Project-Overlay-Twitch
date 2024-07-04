namespace Overlay
{
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
			if (m_soundAlertsQueue.Count > 0u && m_isSoundAlertPlaying is false)
			{
				var soundAlertType = m_soundAlertsQueue.Dequeue();
				PlaySoundAlert(
					soundAlertType: soundAlertType
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
				text: text,
				voice: m_textToSpeechId,
				volume: 100,
				pitch: 1.3f,
				rate: 1.2f,
				utteranceId: 0,
				interrupt: false
			);
		}

		private const int c_minimumBitsForTextToSpeech = 50;
		private const int c_soundAlertDelayInMilliseconds = 1000;
		private const int c_songStartIndex = -1;

		private readonly Dictionary<SoundAlertType, AudioStreamPlayer> m_soundAlerts = new();
		private readonly Queue<SoundAlertType> m_soundAlertsQueue = new();

		private bool m_isSoundAlertPlaying = false;
		private string m_textToSpeechId = string.Empty;

		private void BindTwitchChannelPointRewards()
		{
			var twitchChannelPointRewardsManager = GetNode<TwitchChannelPointRewardsManager>(
				path: NodeDirectory.NodePaths[NodeType.TwitchChannelPointRewardsManager]
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

			twitchChannelPointRewardsManager.RedeemableRewardsWithStringInput[ChannelPointRewardsType.TextToSpeech] = OnChannelPointRewardsRedeemedTextToSpeech;
		}

		private void BindTwitchCheer()
		{
			var twitchManager = GetNode<TwitchManager>(
				path: NodeDirectory.NodePaths[NodeType.TwitchManager]
			);

			twitchManager.ChannelChatNotification += OnChannelChatNotification;
		}

		private static int GetStreamLengthInMilliseconds(
			AudioStream stream
		)
		{
			const int secondsToMilliseconds = 1000;
			var length = stream.GetLength();
			return Mathf.RoundToInt(
			   s: length * secondsToMilliseconds
			);
		}

		private void OnChannelChatNotification(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			var message = @event.Message;
			if (message is null)
			{
				return;
			}

			var totalBits = 0;
			var text = message.Text;

			foreach (var fragment in message.Fragments)
			{
				var fragmentType = fragment.GetFragmentType();
				if (fragmentType is FragmentType.Cheermote)
				{
					var cheermote = fragment.Cheermote;
					totalBits += cheermote.Bits ?? 0;

					text = text.Replace(
						oldValue: cheermote.Prefix,
						newValue: string.Empty
					);
				}
			}

			if (totalBits >= c_minimumBitsForTextToSpeech)
			{
				PlayTextToSpeech(
					text: text
				);
			}
		}

		private void OnChannelPointRewardsRedeemedApplause()
		{
			m_soundAlertsQueue.Enqueue(
                item: SoundAlertType.Applause
			);
		}

		private void OnChannelPointRewardsRedeemedFirstBlood()
		{
			m_soundAlertsQueue.Enqueue(
                item: SoundAlertType.FirstBlood
			);
		}

		private void OnChannelPointRewardsRedeemedGodlike()
		{
			m_soundAlertsQueue.Enqueue(
                item: SoundAlertType.Godlike
			);
		}

		private void OnChannelPointRewardsRedeemedHeartbeat()
		{
			m_soundAlertsQueue.Enqueue(
                item: SoundAlertType.Heartbeat
			);
		}

		private void OnChannelPointRewardsRedeemedHolyShit()
		{
			m_soundAlertsQueue.Enqueue(
                item: SoundAlertType.HolyShit
			);
		}

		private void OnChannelPointRewardsRedeemedHowdy()
		{
			m_soundAlertsQueue.Enqueue(
                item: SoundAlertType.Howdy
			);
		}

		private void OnChannelPointRewardsRedeemedKegExplosion()
		{
			m_soundAlertsQueue.Enqueue(
                item: SoundAlertType.KegExplosion
			);
		}

		private void OnChannelPointRewardsRedeemedKegFuse()
		{
			m_soundAlertsQueue.Enqueue(
                item: SoundAlertType.KegFuse
			);
		}

		private void OnChannelPointRewardsRedeemedNice()
		{
			m_soundAlertsQueue.Enqueue(
				item: SoundAlertType.Nice
			);
		}

		private void OnChannelPointRewardsRedeemedTextToSpeech(
			string text
		)
		{
			PlayTextToSpeech(
				text: text
			);
		}

		private async void PlaySoundAlert(
			SoundAlertType soundAlertType
		)
		{
			var soundAlert = m_soundAlerts[soundAlertType];
			soundAlert.Play();
			m_isSoundAlertPlaying = true;

			var streamLength = GetStreamLengthInMilliseconds(
				stream: soundAlert.Stream
			);
			await Task.Delay(
				millisecondsDelay: streamLength
			);

			soundAlert.Stop();

			await Task.Delay(
				millisecondsDelay: c_soundAlertDelayInMilliseconds
			);

			m_isSoundAlertPlaying = false;
		}

		private void RetrieveResources()
		{
			var voices = DisplayServer.TtsGetVoicesForLanguage(
				language: "en"
			);
			m_textToSpeechId = voices[0u];
		}

		private void RetrieveSoundAlerts()
		{
#if DEBUG
			GD.Print(
				what: $"{nameof(AudioManager)}.{nameof(RetrieveSoundAlerts)}() - Retrieving sound alerts."
			);
#endif

			const string nodeNameSoundAlerts = "SoundAlerts";
			var soundAlertsNode = GetNode(
				path: nodeNameSoundAlerts
			);

			var soundAlertTypes = Enum.GetValues<SoundAlertType>();
			foreach (var soundAlertType in soundAlertTypes)
			{
				var soundAlert = soundAlertsNode.GetNode<AudioStreamPlayer>(
					path: soundAlertType.ToString()
				);

				m_soundAlerts.Add(
					key: soundAlertType,
					value: soundAlert
				);

#if DEBUG
				GD.Print(
					what: $"{nameof(AudioManager)}.{nameof(RetrieveSoundAlerts)}() - Sound Alert retrieved: {soundAlert.Name}."
				);
#endif
			}

#if DEBUG
			GD.Print(
				what: $"{nameof(AudioManager)}.{nameof(RetrieveSoundAlerts)}() - Number of Sound Alerts: {m_soundAlerts.Count}."
			);
#endif
		}
	}
}