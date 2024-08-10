
namespace Overlay
{
	using Godot;
	using System;
	using System.Collections.Generic;
	using ColorType = PastelInterpolator.ColorType;
    using NodeType = NodeDirectory.NodeType;

	public sealed partial class TwitchChannelPointRewardsManager : Node
	{
        public enum ChannelPointRewardsType : uint
        {
            CommandRequestSong = 0u,
            CommandSkipSong,
            CommandTextToSpeech,

            IRLHydrate,
            IRLPostureCheck,
            IRLShowKitty,
            IRLShowPuppy,
            IRLStreeeeeetch,

            SoundAlertApplause,
            SoundAlertFirstBlood,
            SoundAlertGodlike,
            SoundAlertHeartbeat,
            SoundAlertHolyShit,
            SoundAlertHowdy,
            SoundAlertKegExplosion,
            SoundAlertKegFuse,
            SoundAlertNice,
        }

        public struct ChannelPointRewardData
        {
            public ChannelPointRewardsType TwitchChannelPointRewardsType { get; set; } = ChannelPointRewardsType.SoundAlertApplause;
            public string Id { get; set; } = string.Empty;

            public ChannelPointRewardData(
                ChannelPointRewardsType twitchChannelPointRewardsType,
                string id
            )
            {
                this.TwitchChannelPointRewardsType = twitchChannelPointRewardsType;
                this.Id = id;
            }
        };

        public struct ChannelPointReward
        {
            public string BackgroundColor { get; set; } = string.Empty;
            public long Cost { get; set; } = 0;
            public long GlobalCooldownSeconds { get; set; } = 0;
            public bool IsEnabled { get; set; } = false;
            public bool IsGlobalCooldownEnabled { get; set; } = false;
            public bool IsMaxPerStreamEnabled { get; set; } = false;
            public bool IsMaxPerUserPerStreamEnabled { get; set; } = false;
            public bool IsPaused { get; set; } = false;
            public bool IsUserInputRequired { get; set; } = false;
            public long MaxPerStream { get; set; } = 0;
            public long MaxPerUserPerStream { get; set; } = 0;
            public string Prompt { get; set; } = string.Empty;
            public bool ShouldRedemptionsSkipRequestQueue { get; set; } = false;
            public string Title { get; set; } = string.Empty;

            public ChannelPointReward(
                string backgroundColor,
                long cost,
                long globalCooldownSeconds,
                bool isEnabled,
                bool isGlobalCooldownEnabled,
                bool isMaxPerStreamEnabled,
                bool isMaxPerUserPerStreamEnabled,
                bool isPaused,
                bool isUserInputRequired,
                long maxPerStream,
                long maxPerUserPerStream,
                string prompt,
                bool shouldRedemptionSkipRequestQueue,
                string title
            )
            {
                this.BackgroundColor = backgroundColor;
                this.Cost = cost;
                this.GlobalCooldownSeconds = globalCooldownSeconds;
                this.IsEnabled = isEnabled;
                this.IsGlobalCooldownEnabled = isGlobalCooldownEnabled;
                this.IsMaxPerStreamEnabled = isMaxPerStreamEnabled;
                this.IsMaxPerUserPerStreamEnabled = isMaxPerUserPerStreamEnabled;
                this.IsPaused = isPaused;
                this.IsUserInputRequired = isUserInputRequired;
                this.MaxPerStream = maxPerStream;
                this.MaxPerUserPerStream = maxPerUserPerStream;
                this.Prompt = prompt;
                this.ShouldRedemptionsSkipRequestQueue = shouldRedemptionSkipRequestQueue;
                this.Title = title;
            }
        };

        public readonly Dictionary<ChannelPointRewardsType, ChannelPointReward> TwitchChannelPointRewards = new()
        {
            {
                ChannelPointRewardsType.CommandRequestSong,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.Command],
                    cost:                             1000,
                    globalCooldownSeconds:            0,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          false,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              true,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              0,
                    prompt:                           "Want to listen to something? Enter a spotify song url or try your luck searching!",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.CommandRequestSong]
                )
            },
            {
                ChannelPointRewardsType.CommandSkipSong,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.Command],
                    cost:                             2000,
                    globalCooldownSeconds:            5,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          true,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              0,
                    prompt:                           "Don't like what's playing? Skip it!",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.CommandSkipSong]
                )
            },
            {
                ChannelPointRewardsType.CommandTextToSpeech,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.Command],
                    cost:                             1000,
                    globalCooldownSeconds:            0,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          false,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              true,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              0,
                    prompt:                           "Have a good joke? Have AI read it for you outloud!",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.CommandTextToSpeech]
                )
            },
            {
                ChannelPointRewardsType.IRLHydrate,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.IRL],
                    cost:                             50,
                    globalCooldownSeconds:            60,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          true,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              0,
                    prompt:                           "Sips water..",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.IRLHydrate]
                )
            },
            {
                ChannelPointRewardsType.IRLPostureCheck,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.IRL],
                    cost:                             50,
                    globalCooldownSeconds:            60,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          true,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              0,
                    prompt:                           "Good posture is good habit.",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.IRLPostureCheck]
                )
            },
            {
                ChannelPointRewardsType.IRLShowKitty,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.IRL],
                    cost:                             1500,
                    globalCooldownSeconds:            300,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          false,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              0,
                    prompt:                           "Show the cutest of kitties! Disclaimer: May appear on stream anyways.",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.IRLShowKitty]
                )
            },
            {
                ChannelPointRewardsType.IRLShowPuppy,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.IRL],
                    cost:                             2500,
                    globalCooldownSeconds:            300,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          false,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              0,
                    prompt:                           "Show the goodest of puppies! Disclaimer: May appear on stream anyways.",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.IRLShowPuppy]
                )
            },
            {
                ChannelPointRewardsType.IRLStreeeeeetch,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.IRL],
                    cost:                             50,
                    globalCooldownSeconds:            60,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          true,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              0,
                    prompt:                           "Good streeeeeetch is best streeeeeetch!",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.IRLStreeeeeetch]
                )
            },
            {
                ChannelPointRewardsType.SoundAlertApplause,
                new(
                    backgroundColor:c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                    cost:                             100,
                    globalCooldownSeconds:            0,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          false,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              0,
                    prompt:                           "Nice round of applause for the chap on stream.",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertApplause]
                )
            },
            {
                ChannelPointRewardsType.SoundAlertFirstBlood,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                    cost:                             1,
                    globalCooldownSeconds:            0,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          false,
                    isMaxPerStreamEnabled:            true,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     1,
                    maxPerUserPerStream:              0,
                    prompt:                           "First is the worst, though.",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertFirstBlood]
                )
            },
            {
                ChannelPointRewardsType.SoundAlertGodlike,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                    cost:                             300,
                    globalCooldownSeconds:            0,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          false,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              0,
                    prompt:                           "We're basically gods here.",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertGodlike]
                )
            },
            {
                ChannelPointRewardsType.SoundAlertHeartbeat,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                    cost:                             300,
                    globalCooldownSeconds:            0,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          false,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              0,
                    prompt:                           "The beat goes on.",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertHeartbeat]
                )
            },
            {
                ChannelPointRewardsType.SoundAlertHolyShit,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                    cost:                             300,
                    globalCooldownSeconds:            0,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          false,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              0,
                    prompt:                           "Even gods can poop.",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertHolyShit]
                )
            },
            {
                ChannelPointRewardsType.SoundAlertHowdy,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                    cost:                             50,
                    globalCooldownSeconds:            0,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          false,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     true,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              1,
                    prompt:                           "Howdy, friend!",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertHowdy]
                )
            },
            {
                ChannelPointRewardsType.SoundAlertKegExplosion,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                    cost:                             300,
                    globalCooldownSeconds:            0,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          false,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              0,
                    prompt:                           "Spontaneous exploderinos!",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertKegExplosion]
                )
            },
            {
                ChannelPointRewardsType.SoundAlertKegFuse,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                    cost:                             300,
                    globalCooldownSeconds:            0,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          false,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     false,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              0,
                    prompt:                           "The suspense is killing me!",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertKegFuse]
                )
            },
            {
                ChannelPointRewardsType.SoundAlertNice,
                new(
                    backgroundColor:                  c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                    cost:                             69,
                    globalCooldownSeconds:            0,
                    isEnabled:                        true,
                    isGlobalCooldownEnabled:          false,
                    isMaxPerStreamEnabled:            false,
                    isMaxPerUserPerStreamEnabled:     true,
                    isPaused:                         false,
                    isUserInputRequired:              false,
                    maxPerStream:                     0,
                    maxPerUserPerStream:              1,
                    prompt:                           "Is nice, no?",
                    shouldRedemptionSkipRequestQueue: false,
                    title:                            c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertNice]
                )
            },
        };

        public readonly Dictionary<ChannelPointRewardsType, Action> RedeemableRewardsWithNoInput = new()
        {
            { ChannelPointRewardsType.IRLHydrate,             null },
            { ChannelPointRewardsType.IRLPostureCheck,        null },
            { ChannelPointRewardsType.IRLShowKitty,           null },
            { ChannelPointRewardsType.IRLShowPuppy,           null },
            { ChannelPointRewardsType.IRLStreeeeeetch,        null },
            { ChannelPointRewardsType.SoundAlertApplause,     null },
            { ChannelPointRewardsType.SoundAlertFirstBlood,   null },
            { ChannelPointRewardsType.SoundAlertGodlike,      null },
            { ChannelPointRewardsType.SoundAlertHeartbeat,    null },
            { ChannelPointRewardsType.SoundAlertHolyShit,     null },
            { ChannelPointRewardsType.SoundAlertHowdy,        null },
            { ChannelPointRewardsType.SoundAlertKegExplosion, null },
            { ChannelPointRewardsType.SoundAlertKegFuse,      null },
            { ChannelPointRewardsType.SoundAlertNice,         null },
        };
        public Action<string, string> CommandRequestSongClaimed = null;
        public Action<string> CommandSkipSongClaimed = null;
        public Action<string> CommandTextToSpeedClaimed = null;

        public override void _EnterTree()
		{
			SubscribeToTwitchEvents();
		}

		public void ClaimReward(
			string username,
			string id
		)
		{
			for (var i = 0; i < m_pendingUserRewards[key: username].Count; i++)
			{
				if (
					m_pendingUserRewards[key: username][index: i].Id.Equals(
						value: id
					) is true
				)
				{
					m_pendingUserRewards[key: username].RemoveAt(
						index: i
					);

					if (m_pendingUserRewards[key: username].Count is 0)
					{
						m_pendingUserRewards.Remove(
							key: username
						);
					}
					break;
				}
			}
		}

		public List<ChannelPointRewardData> GetPendingRewards()
		{
			var pendingChannelPointRewardDatas = new List<ChannelPointRewardData>();

			foreach (var pendingUserRewards in m_pendingUserRewards)
			{
				pendingChannelPointRewardDatas.AddRange(
					collection: pendingUserRewards.Value
				);
			}

			return pendingChannelPointRewardDatas;
		}

		public string GetRewardId(
			string username,
			ChannelPointRewardsType channelPointRewardsType
		)
		{
			foreach (var channelPointRewardData in m_pendingUserRewards[key: username])
			{
				if (
					channelPointRewardData.TwitchChannelPointRewardsType.Equals(
						obj: channelPointRewardsType
					) is true
				)
				{
					return channelPointRewardData.Id;
				}
			}

			return string.Empty;
		}

		public bool HasRewardAvailable(
			string username,
			ChannelPointRewardsType channelPointRewardsType
		)
		{
			if (
				m_pendingUserRewards.ContainsKey(
					key: username
				) is true
			)
			{
				foreach (var channelPointRewardData in m_pendingUserRewards[key: username])
				{
					if (
						channelPointRewardData.TwitchChannelPointRewardsType.Equals(
							obj: channelPointRewardsType
						) is true
					)
					{
						return true;
					}
				}
			}

			return false;
		}

		public static ChannelPointRewardsType GetRewardTypeByName(
			string rewardName
		)
		{
			foreach (var channelPointName in c_channelRewardPointNames)
			{
				if (channelPointName.Value == rewardName)
				{
					return channelPointName.Key;
				}
			}

			return ChannelPointRewardsType.CommandRequestSong;
		}

		private enum ChannelRewardPointColorType : uint
		{
			Command = 0u,
			IRL,
			SoundAlert,
		}

		private static readonly Dictionary<ChannelRewardPointColorType, string> c_channelRewardPointColorTypes = new()
		{
			{ ChannelRewardPointColorType.Command,    $"#{PastelInterpolator.GetColorAsHexByColorType(colorType: ColorType.Magenta)}" },
			{ ChannelRewardPointColorType.IRL,        $"#{PastelInterpolator.GetColorAsHexByColorType(colorType: ColorType.Yellow)}" },
			{ ChannelRewardPointColorType.SoundAlert, $"#{PastelInterpolator.GetColorAsHexByColorType(colorType: ColorType.Cyan)}" },
		};
		private static readonly Dictionary<ChannelPointRewardsType, string> c_channelRewardPointNames = new()
		{
			{ ChannelPointRewardsType.CommandRequestSong,     "Command: Request Song"      },
			{ ChannelPointRewardsType.CommandSkipSong,        "Command: Skip Song"         },
            { ChannelPointRewardsType.CommandTextToSpeech,    "Command: Text To Speech"    },
            { ChannelPointRewardsType.IRLHydrate,             "IRL: Hydrate"               },
			{ ChannelPointRewardsType.IRLPostureCheck,        "IRL: Posture Check"         },
			{ ChannelPointRewardsType.IRLShowKitty,           "IRL: Show Kitty"            },
			{ ChannelPointRewardsType.IRLShowPuppy,           "IRL: Show Puppy"            },
			{ ChannelPointRewardsType.IRLStreeeeeetch,        "IRL: Streeeeeetch"          },
			{ ChannelPointRewardsType.SoundAlertApplause,     "Sound Alert: Applause"      },
			{ ChannelPointRewardsType.SoundAlertFirstBlood,   "Sound Alert: First Blood"   },
			{ ChannelPointRewardsType.SoundAlertGodlike,      "Sound Alert: Godlike"       },
			{ ChannelPointRewardsType.SoundAlertHeartbeat,    "Sound Alert: Heartbeat"     },
			{ ChannelPointRewardsType.SoundAlertHolyShit,     "Sound Alert: Holy Shit"     },
			{ ChannelPointRewardsType.SoundAlertHowdy,        "Sound Alert: Howdy"         },
			{ ChannelPointRewardsType.SoundAlertKegExplosion, "Sound Alert: Keg Explosion" },
			{ ChannelPointRewardsType.SoundAlertKegFuse,      "Sound Alert: Keg Fuse"      },
			{ ChannelPointRewardsType.SoundAlertNice,         "Sound Alert: Nice"          },
		};

		private readonly Dictionary<string, List<ChannelPointRewardData>> m_pendingUserRewards = new();

		private void SubscribeToTwitchEvents()
		{
			var twitchManager = GetNode<TwitchManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.TwitchManager
                )
            );

			twitchManager.ChannelPointsCustomRewardRedeemed += OnChannelPointsCustomRewardRedemptionAdded;
		}

		private void OnChannelPointsCustomRewardRedemptionAdded(
			TwitchWebSocketMessagePayloadEventChannelPointsCustomRewardRedeemed @event
		)
		{
			switch (@event.Reward.Title)
			{
				case "Command: Request Song":
                    CommandRequestSongClaimed?.Invoke(
						@event.UserInput,
                        @event.UserName
					);
					break;

                case "Command: Skip Song":
                    CommandSkipSongClaimed?.Invoke(
                        @event.UserName
                    );
                    break;

                case "Command: Text To Speech":
                    CommandTextToSpeedClaimed?.Invoke(
                        @event.UserInput
                    );
                    break;

                case "IRL: Hydrate":
					RedeemableRewardsWithNoInput[ChannelPointRewardsType.IRLHydrate]?.Invoke();
					break;

				case "IRL: Posture Check":
					RedeemableRewardsWithNoInput[ChannelPointRewardsType.IRLPostureCheck]?.Invoke();
					break;

				case "IRL: Streeeeeetch":
					RedeemableRewardsWithNoInput[ChannelPointRewardsType.IRLStreeeeeetch]?.Invoke();
					break;

				case "Sound Alert: Applause":
					RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertApplause]?.Invoke();
					break;

				case "Sound Alert: First Blood":
					RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertFirstBlood]?.Invoke();
					break;

				case "Sound Alert: Godlike":
					RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertGodlike]?.Invoke();
					break;

				case "Sound Alert: Heartbeat":
					RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertHeartbeat]?.Invoke();
					break;

				case "Sound Alert: Holy Shit":
					RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertHolyShit]?.Invoke();
					break;

				case "Sound Alert: Howdy":
					RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertHowdy]?.Invoke();
					break;

				case "Sound Alert: Keg Explosion":
					RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertKegExplosion]?.Invoke();
					break;

				case "Sound Alert: Keg Fuse":
					RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertKegFuse]?.Invoke();
					break;

				case "Sound Alert: Nice":
					RedeemableRewardsWithNoInput[ChannelPointRewardsType.SoundAlertNice]?.Invoke();
					break;

				default:
					break;
			}
		}

		private void AddPendingUserReward(
			string username,
			ChannelPointRewardsType channelPointRewardsType,
			string channelPointRewardsId
		)
		{
			if (
				m_pendingUserRewards.ContainsKey(
					key: username
				) is false
			)
			{
				m_pendingUserRewards.Add(
					key: username,
					value: new()
				);
			}

			m_pendingUserRewards[username].Add(
				item: new(
					twitchChannelPointRewardsType: channelPointRewardsType,
					id: channelPointRewardsId
				)
			);
		}
	}
}