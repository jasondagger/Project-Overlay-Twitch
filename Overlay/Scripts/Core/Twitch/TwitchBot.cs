namespace Overlay
{
	using Godot;
	using System;
	using System.Collections.Generic;
    using System.Net.NetworkInformation;
    using System.Net.WebSockets;
    using System.Text;
	using System.Threading;
    using System.Threading.Tasks;
    using FragmentType = TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragment.FragmentType;
	using NodeType = NodeDirectory.NodeType;

	public sealed partial class TwitchBot : Node
	{
		public override void _EnterTree()
		{
			// retrieve user access token
			//OS.ShellOpen(
			//    $"https://id.twitch.tv/oauth2/authorize" +
			//    $"?response_type=token" +
			//    $"&client_id={TwitchData.ClientId}" +
			//    $"&redirect_uri=http://localhost:3000" +
			//    $"&scope=" +
			//        $"chat%3Aread%20" +    // chat:read
			//        $"chat%3Aedit"         // chat:edit
			//);
			RetrieveResources();
		}

		public override void _ExitTree()
		{
			m_shutdown = true;
		}

		public override void _Process(
			double delta
		)
		{
			foreach (var command in c_commandTimers)
			{
				var commandType = command.Key;
				if (
					IsCommandAvailable(
						commandType: commandType
					) is true
				)
				{
					var commandTimer = command.Value;
					commandTimer -= delta;
					if (commandTimer < 0d)
					{
						commandTimer = 0d;
					}
					c_commandTimers[commandType] = commandTimer;
				}
			}
			if (m_messageTimestamps.Count > 0u)
			{
				var elapsedMilliseconds = Time.GetTicksMsec();
				if (m_messageTimestamps.Peek() + c_minimumMessageTimerInMilliseconds < elapsedMilliseconds)
				{
					m_messageTimestamps.Dequeue();
				}
			}
		}

		public override void _Ready()
		{
			ConnectWebSocket();
		}

		private enum ApplicationCommandType : uint
		{
			Overlay = 0u,
			StreamAvatars,
			Invalid,
		}

		private enum AutomatedMessageType : uint
		{
            Commands = 0u,
			Discord,
			Rules,
			Steam,
			StreamAvatars,
            Twitch,
			YouTube,
			Count
		}

		private enum CommandType : uint
		{
			Age = 0u,
			Commands,
			Date,
			Discord,
            FollowAge,
            Rules,
            Steam,
            StreamAvatars,
            TextToSpeech,
            Time,
            YouTube,

			// Stream Avatars
			Actions,
			Attack,
            Avatar,
            Avatars,
			Basketball,
			BattleRoyale,
			Bet,
			Blacklist,
			Bomb,
			Boss,
			Buy,
			Change,
			Color,
			Currency,
			Dance,
			Duel,
			Explode,
			Extension,
			Fart,
			Freeze,
			Game,
			Gear,
			Gift,
			HideAvatar,
			Hug,
			Jump,
			Leaderboard,
			Mass,
			Mod,
			NameTags,
			Pin,
			Quote,
			Random,
			Remove,
			Roll,
			Scale,
			ScreenSaver,
			Shop,
			Shoutout,
			Show,
			Sit,
			Sling,
			Slots,
			Sounds,
			Spawn,
			Throw,
			Whitelist,
		}

		private const string c_websocketAddress = "wss://irc-ws.chat.twitch.tv:443";
		private const string c_webSocketMessagedelimiter = "\r\n";
		private const string c_twitchBotDisplayName = "SmoothGPT";
		private const string c_twitchBotUsername = "smoothgpt";
		private const int c_webSocketMessageDelimiterLength = 2;
        private const int c_twitchMessageDelimiterLength = 2;
        private const uint c_maxPacketSize = 8192u;
		private const uint c_minimumMessageCount = 5u;
		private const ulong c_minimumMessageTimerInMilliseconds = 900000u;

		private readonly Dictionary<AutomatedMessageType, string> c_automatedMessages = new()
		{
            { AutomatedMessageType.Commands,	  "Check the Socials section below for a list of available bot commands @ https://www.twitch.tv/SmoothDagger/About" },
			{ AutomatedMessageType.Discord,		  "Interested in chatting? Join the Discord @ https://www.discord.gg/SmoothCrew" },
			{ AutomatedMessageType.Rules,		  "Make sure you're following the rules! Find them below in the rules section @ https://www.twitch.tv/SmoothDagger/About" },
			{ AutomatedMessageType.StreamAvatars, "Want to customize your stream avatar? Select an avatar below in the Stream Avatars section @ https://www.twitch.tv/SmoothDagger/About" },
            { AutomatedMessageType.Steam,		  "Come play with us! Add me on Steam @ https://steamcommunity.com/id/SmoothDagger/" },
            { AutomatedMessageType.Twitch,		  "Enjoying the stream? Tap the follow button to get notified for any live streams!" },
			{ AutomatedMessageType.YouTube,		  "Want more SmoothDagger content? Subscribe on YouTube @ https://www.youtube.com/@SmoothDagger" },
		};
		private readonly Dictionary<CommandType, string> c_commands = new()
		{
			// Bot
			{ CommandType.Age,           "!age" },
            { CommandType.Commands,      "!commands" },
            { CommandType.Date,          "!date" },
            { CommandType.Discord,       "!discord" },
            { CommandType.FollowAge,     "!followage" },
            { CommandType.Rules,         "!rules" },
            { CommandType.Steam,         "!steam" },
            { CommandType.StreamAvatars, "!streamavatars" },
            { CommandType.TextToSpeech,  "!tts" },
            { CommandType.Time,          "!time" },
            { CommandType.YouTube,       "!youtube" },

			// Stream Avatars
            { CommandType.Actions,		 "!actions" },
            { CommandType.Attack,		 "!attack" },
            { CommandType.Avatar,		 "!avatar" },
            { CommandType.Avatars,		 "!avatars" },
            { CommandType.Basketball,	 "!basketball" },
            { CommandType.BattleRoyale,	 "!battleroyale" },
            { CommandType.Bet,			 "!bet" },
            { CommandType.Blacklist,	 "!blacklist" },
            { CommandType.Bomb,			 "!bomb" },
            { CommandType.Boss,			 "!boss" },
            { CommandType.Buy,			 "!buy" },
            { CommandType.Change,		 "!change" },
            { CommandType.Color,		 "!color" },
            { CommandType.Currency,		 "!currency" },
            { CommandType.Dance,		 "!dance" },
            { CommandType.Duel,			 "!duel" },
            { CommandType.Explode,		 "!explode" },
            { CommandType.Extension,	 "!extension" },
            { CommandType.Fart,			 "!fart" },
            { CommandType.Freeze,		 "!freeze" },
            { CommandType.Game,			 "!game" },
            { CommandType.Gear,			 "!gear" },
            { CommandType.Gift,			 "!gift" },
            { CommandType.HideAvatar,	 "!hideavatar" },
            { CommandType.Hug,			 "!hug" },
            { CommandType.Jump,			 "!jump" },
            { CommandType.Leaderboard,	 "!leaderboard" },
            { CommandType.Mass,			 "!mass" },
            { CommandType.Mod,			 "!mod" },
            { CommandType.NameTags,		 "!nametags" },
            { CommandType.Pin,			 "!pin" },
            { CommandType.Quote,		 "!quote" },
            { CommandType.Random,		 "!random" },
            { CommandType.Remove,		 "!remove" },
            { CommandType.Roll,			 "!roll" },
            { CommandType.Scale,		 "!scale" },
            { CommandType.ScreenSaver,	 "!screensaver" },
            { CommandType.Shop,			 "!shop" },
            { CommandType.Shoutout,		 "!shoutout" },
            { CommandType.Show,			 "!show" },
            { CommandType.Sit,			 "!sit" },
            { CommandType.Sling,		 "!sling" },
            { CommandType.Slots,		 "!slots" },
            { CommandType.Sounds,		 "!sounds" },
            { CommandType.Spawn,		 "!spawn" },
            { CommandType.Throw,		 "!throw" },
            { CommandType.Whitelist,     "!whitelist" },
        };
		private readonly Dictionary<CommandType, double> c_commandCooldowns = new()
		{
			// Bot
			{ CommandType.Age,           0d  },
            { CommandType.Commands,      30d },
            { CommandType.Date,          0d  },
            { CommandType.Discord,       30d },
            { CommandType.FollowAge,     0d  },
            { CommandType.Rules,         30d },
            { CommandType.Steam,         30d },
            { CommandType.StreamAvatars, 30d },
            { CommandType.TextToSpeech,  0d  },
            { CommandType.Time,          0d  },
            { CommandType.YouTube,       30d },

			// Stream Avatars
            { CommandType.Actions,       0d },
            { CommandType.Attack,        0d },
            { CommandType.Avatar,        0d },
            { CommandType.Avatars,       0d },
            { CommandType.Basketball,    0d },
            { CommandType.BattleRoyale,  0d },
            { CommandType.Bet,           0d },
            { CommandType.Blacklist,     0d },
            { CommandType.Bomb,          0d },
            { CommandType.Boss,          0d },
            { CommandType.Buy,           0d },
            { CommandType.Change,        0d },
            { CommandType.Color,         0d },
            { CommandType.Currency,      0d },
            { CommandType.Dance,         0d },
            { CommandType.Duel,          0d },
            { CommandType.Explode,       0d },
            { CommandType.Extension,     0d },
            { CommandType.Fart,          0d },
            { CommandType.Freeze,        0d },
            { CommandType.Game,          0d },
            { CommandType.Gear,          0d },
            { CommandType.Gift,          0d },
            { CommandType.HideAvatar,    0d },
            { CommandType.Hug,           0d },
            { CommandType.Jump,          0d },
            { CommandType.Leaderboard,   0d },
            { CommandType.Mass,          0d },
            { CommandType.Mod,           0d },
            { CommandType.NameTags,      0d },
            { CommandType.Pin,           0d },
            { CommandType.Quote,         0d },
            { CommandType.Random,        0d },
            { CommandType.Remove,        0d },
            { CommandType.Roll,          0d },
            { CommandType.Scale,         0d },
            { CommandType.ScreenSaver,   0d },
            { CommandType.Shop,          0d },
            { CommandType.Shoutout,      0d },
            { CommandType.Show,          0d },
            { CommandType.Sit,           0d },
            { CommandType.Sling,         0d },
            { CommandType.Slots,         0d },
            { CommandType.Sounds,        0d },
            { CommandType.Spawn,         0d },
            { CommandType.Throw,         0d },
            { CommandType.Whitelist,     0d },
        };
        private readonly Dictionary<CommandType, double> c_commandTimers = new()
		{
			// Bot
			{ CommandType.Age,           0d },
            { CommandType.Commands,      0d },
            { CommandType.Date,          0d },
            { CommandType.Discord,       0d },
            { CommandType.FollowAge,     0d },
            { CommandType.Rules,         0d },
            { CommandType.Steam,         0d },
            { CommandType.StreamAvatars, 0d },
            { CommandType.TextToSpeech,  0d },
            { CommandType.Time,          0d },
            { CommandType.YouTube,       0d },

			// Stream Avatars
			{ CommandType.Actions,       0d },
            { CommandType.Attack,        0d },
            { CommandType.Avatar,        0d },
            { CommandType.Avatars,       0d },
            { CommandType.Basketball,    0d },
            { CommandType.BattleRoyale,  0d },
            { CommandType.Bet,           0d },
            { CommandType.Blacklist,     0d },
            { CommandType.Bomb,          0d },
            { CommandType.Boss,          0d },
            { CommandType.Buy,           0d },
            { CommandType.Change,        0d },
            { CommandType.Color,         0d },
            { CommandType.Currency,      0d },
            { CommandType.Dance,         0d },
            { CommandType.Duel,          0d },
            { CommandType.Explode,       0d },
            { CommandType.Extension,     0d },
            { CommandType.Fart,          0d },
            { CommandType.Freeze,        0d },
            { CommandType.Game,          0d },
            { CommandType.Gear,          0d },
            { CommandType.Gift,          0d },
            { CommandType.HideAvatar,    0d },
            { CommandType.Hug,           0d },
            { CommandType.Jump,          0d },
            { CommandType.Leaderboard,   0d },
            { CommandType.Mass,          0d },
            { CommandType.Mod,           0d },
            { CommandType.NameTags,      0d },
            { CommandType.Pin,           0d },
            { CommandType.Quote,         0d },
            { CommandType.Random,        0d },
            { CommandType.Remove,        0d },
            { CommandType.Roll,          0d },
            { CommandType.Scale,         0d },
            { CommandType.ScreenSaver,   0d },
            { CommandType.Shop,          0d },
            { CommandType.Shoutout,      0d },
            { CommandType.Show,          0d },
            { CommandType.Sit,           0d },
            { CommandType.Sling,         0d },
            { CommandType.Slots,         0d },
            { CommandType.Sounds,        0d },
            { CommandType.Spawn,         0d },
            { CommandType.Throw,         0d },
            { CommandType.Whitelist,     0d },
        };

		private struct WebSocketMessage
		{
			public Dictionary<string, string> Tags = new();
			public string Username = string.Empty;
			public string Command = string.Empty;
			public string Text = string.Empty;

			public WebSocketMessage(
				Dictionary<string, string> tags,
				string username,
				string command,
				string text
			)
			{
				this.Tags = tags;
				this.Username = username;
				this.Command = command;
				this.Text = text;
			}
		};

        private readonly Dictionary<string, string> m_usernameColors = new();
        private readonly List<string> m_subscribersWhoUsedTextToSpeech = new();
        private readonly Queue<ulong> m_messageTimestamps = new();
        private readonly ClientWebSocket m_webSocket = new();

        private AudioManager m_audioManager = null;
		private PastelInterpolator m_pastelInterpolator = null;
		private TwitchChannelPointRewardsManager m_twitchChannelPointRewardsManager = null;
		private TwitchChatManager m_twitchChatManager = null;
		private TwitchManager m_twitchManager = null;
		private AutomatedMessageType m_currentAutomatedMessage = AutomatedMessageType.Count;
		private bool m_shutdown = false;

        private void AddBotChatMessage(
			string message
		)
		{
			m_twitchChatManager.AddTwitchChatMessage(
                name: c_twitchBotDisplayName,
                color: string.Empty,
                message: message,
                emotes: string.Empty
			);
		}

		private async void ConnectWebSocket()
		{
			// connect to Twitch IRC websocket
			var uri = new Uri(
                uriString: c_websocketAddress
			);

			await m_webSocket.ConnectAsync(
                uri: uri,
                cancellationToken: default
			);

			await SendWebSocketMessage(
                message: $"CAP REQ :twitch.tv/commands twitch.tv/tags"
			);
			await SendWebSocketMessage(
                message: $"PASS oauth:{TwitchData.BotAccessToken}"
			);
			await SendWebSocketMessage(
                message: $"NICK {TwitchData.BotUsername}"
			);

			var bytes = new byte[c_maxPacketSize];
			var result = await m_webSocket.ReceiveAsync(
                buffer: bytes,
                cancellationToken: default
			);

			ParseWebSocketMessage(
                message: Encoding.UTF8.GetString(
                    bytes: bytes,
                    index: 0,
                    count: result.Count
				),
                webSocketMessages: out List<WebSocketMessage> webSocketMessages
			);
			foreach (var webSocketMessage in webSocketMessages)
			{
				if (
					webSocketMessage.Command is "NOTICE"
				)
				{
#if DEBUG
					GD.PrintErr(
                        what: $"{nameof(TwitchBot)}.{nameof(ConnectWebSocket)}() - Web socket connect failed."
					);
#endif
					return;
				}
			}

#if DEBUG
			GD.Print(
                what: $"{nameof(TwitchBot)}.{nameof(ConnectWebSocket)}() - Web socket connect successful."
			);
#endif

			await SendWebSocketMessage(
                message: $"JOIN #{TwitchData.TwitchChannel}"
			);

			StartWebSocketMessageReader();
			StartWebSocketAutomatedMessageDispatcher();
		}

        private static ApplicationCommandType GetApplicationCommandType(
		    CommandType commandType
		)
        {
            return commandType switch
            {
                CommandType.Age or 
				CommandType.Date or 
				CommandType.Discord or 
				CommandType.Commands or 
				CommandType.FollowAge or 
				CommandType.Rules or 
				CommandType.Steam or 
				CommandType.StreamAvatars or 
				CommandType.TextToSpeech or 
				CommandType.Time or 
				CommandType.YouTube => 
					ApplicationCommandType.Overlay,

                CommandType.Actions or
                CommandType.Attack or
                CommandType.Avatar or
                CommandType.Avatars or
                CommandType.Basketball or
                CommandType.BattleRoyale or
                CommandType.Bet or
                CommandType.Blacklist or
                CommandType.Bomb or
                CommandType.Boss or
                CommandType.Buy or
                CommandType.Change or
                CommandType.Color or
                CommandType.Currency or
                CommandType.Dance or
                CommandType.Duel or
                CommandType.Explode or
                CommandType.Extension or
                CommandType.Fart or
                CommandType.Freeze or
                CommandType.Game or
                CommandType.Gear or
                CommandType.Gift or
                CommandType.HideAvatar or
                CommandType.Hug or
                CommandType.Jump or
                CommandType.Leaderboard or
                CommandType.Mass or
                CommandType.Mod or
                CommandType.NameTags or
                CommandType.Pin or
                CommandType.Quote or
                CommandType.Random or
                CommandType.Remove or
                CommandType.Roll or
                CommandType.Scale or
                CommandType.ScreenSaver or
                CommandType.Shop or
                CommandType.Shoutout or
                CommandType.Show or
                CommandType.Sit or
                CommandType.Sling or
                CommandType.Slots or
                CommandType.Sounds or
                CommandType.Spawn or
                CommandType.Throw or
                CommandType.Whitelist =>
					ApplicationCommandType.StreamAvatars,
                
				_ => 
					ApplicationCommandType.Invalid,
            };
        }

        private void HandleApplicationCommandOverlay(
			CommandType commandType,
			WebSocketMessage webSocketMessage
		)
		{
            var commandText = c_commands[commandType];
            if (
                IsCommandAvailable(
                    commandType: commandType
                ) is false
            )
            {
                HandleWebsSocketMessageCommandOnCooldown(
                    webSocketMessage: webSocketMessage,
                    commandText: commandText
                );
            }
            else
            {
                c_commandTimers[commandType] = c_commandCooldowns[commandType];
                switch (commandType)
                {
                    case CommandType.Age:
                    case CommandType.FollowAge:
                        HandleWebSocketMessagePrivMsgFollowAge(
                            webSocketMessage: webSocketMessage
                        );
                        break;

                    case CommandType.Commands:
                        HandleWebSocketMessagePrivMsgCommands(
                            webSocketMessage: webSocketMessage
                        );
                        break;

                    case CommandType.Date:
                        HandleWebSocketMessagePrivMsgDate(
                            webSocketMessage: webSocketMessage
                        );
                        break;

                    case CommandType.Discord:
                        HandleWebSocketMessagePrivMsgDiscord(
                            webSocketMessage: webSocketMessage
                        );
                        break;

                    case CommandType.Rules:
                        HandleWebSocketMessagePrivMsgRules(
                            webSocketMessage: webSocketMessage
                        );
                        break;

                    case CommandType.Steam:
                        HandleWebSocketMessagePrivMsgSteam(
                            webSocketMessage: webSocketMessage
                        );
                        break;

                    case CommandType.StreamAvatars:
                        HandleWebSocketMessagePrivMsgStreamAvatars(
                            webSocketMessage: webSocketMessage
                        );
                        break;

                    case CommandType.TextToSpeech:
                        HandleWebSocketMessagePrivMsgTextToSpeech(
                            webSocketMessage: webSocketMessage
                        );
                        break;

                    case CommandType.Time:
                        HandleWebSocketMessagePrivMsgTime(
                            webSocketMessage: webSocketMessage
                        );
                        break;

                    case CommandType.YouTube:
                        HandleWebSocketMessagePrivMsgYouTube(
                            webSocketMessage: webSocketMessage
                        );
                        break;

                    case CommandType.Actions:
                    case CommandType.Attack:
                    case CommandType.Avatar:
                    case CommandType.Avatars:
                    case CommandType.Basketball:
                    case CommandType.BattleRoyale:
                    case CommandType.Bet:
                    case CommandType.Blacklist:
                    case CommandType.Bomb:
                    case CommandType.Boss:
                    case CommandType.Buy:
                    case CommandType.Change:
                    case CommandType.Color:
                    case CommandType.Currency:
                    case CommandType.Dance:
                    case CommandType.Duel:
                    case CommandType.Explode:
                    case CommandType.Extension:
                    case CommandType.Fart:
                    case CommandType.Freeze:
                    case CommandType.Game:
                    case CommandType.Gear:
                    case CommandType.Gift:
                    case CommandType.HideAvatar:
                    case CommandType.Hug:
                    case CommandType.Jump:
                    case CommandType.Leaderboard:
                    case CommandType.Mass:
                    case CommandType.Mod:
                    case CommandType.NameTags:
                    case CommandType.Pin:
                    case CommandType.Quote:
                    case CommandType.Random:
                    case CommandType.Remove:
                    case CommandType.Roll:
                    case CommandType.Scale:
                    case CommandType.ScreenSaver:
                    case CommandType.Shop:
                    case CommandType.Shoutout:
                    case CommandType.Show:
                    case CommandType.Sit:
                    case CommandType.Sling:
                    case CommandType.Slots:
                    case CommandType.Sounds:
                    case CommandType.Spawn:
                    case CommandType.Throw:
                    case CommandType.Whitelist:
                    default:
                        break;
                }
            }
        }

        private async void HandleChannelChatNotificationBitsBadgeTier(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			var bitsBadgeTier = @event.BitsBadgeTier;
			var message = @event.Message;
			var fragments = message.Fragments;
            var totalBits = 0;
			foreach (var fragment in fragments)
			{
				var fragmentType = fragment.GetFragmentType();
				if (fragmentType is FragmentType.Cheermote)
				{
					var cheermote = fragment.Cheermote;
					totalBits += cheermote.Bits ?? 0;
				}
			}

            var username = $"@{@event.ChatterUsername}";
            var bitsBadgeTierText = $" Thank you so much for the {totalBits} bits! Congratulations on achieving the {bitsBadgeTier.Tier} bit tier!";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{TwitchData.TwitchChannel} :{username}{bitsBadgeTierText}"
			);
		}

		private async void HandleChannelChatNotificationCharityDonation(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			// todo
			var charityDonation = @event.CharityDonation;
            var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
            var username = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUsername}";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{TwitchData.TwitchChannel} :{username} This event was not set up yet. Shame @SmoothDagger for being lazy! SHAME HIM"
			);
		}

		private async void HandleChannelChatNotificationCommunitySubGift(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			var communitySubGift = @event.CommunitySubGift;
            var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
			var communitySubGiftCumulativeTotal = communitySubGift.CumulativeTotal ?? 0;
			var communitySubGiftTotal = communitySubGift.Total ?? 0;
            var communitySubGiftTier = int.Parse(
                s: communitySubGift.SubTier[0].ToString()
			);
			var username = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUsername}";
			var communitySubGiftText = $" Thank you so much for the {communitySubGiftTotal} tier {communitySubGiftTier} gifted community sub{(communitySubGiftTotal > 1u ? "s" : string.Empty)}!";
			var communitySubGiftCumulativeText = $" {username} has gifted a total of {communitySubGiftCumulativeTotal} community sub{(communitySubGiftCumulativeTotal > 1u ? "s" : string.Empty)}!";
            var communitySubGiftCommandsText = " Make sure to check out the available sub commands in the Social section below for your sub benefits @ https://www.twitch.tv/smoothdagger/about";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{TwitchData.TwitchChannel} :{username}{communitySubGiftText}{communitySubGiftCumulativeText}{communitySubGiftCommandsText}"
			);
		}

		private async void HandleChannelChatNotificationGiftPaidUpgrade(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			// todo
			var giftPaidUpgrade = @event.GiftPaidUpgrade;
			var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
            var username = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUsername}";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{TwitchData.TwitchChannel} :{username} This event was not set up yet. Shame @SmoothDagger for being lazy! SHAME HIM"
			);
		}

		private async void HandleChannelChatNotificationPayItForward(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			// todo
			var payItForward = @event.PayItForward;
            var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
            var username = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUsername}";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{TwitchData.TwitchChannel} :{username} This event was not set up yet. Shame @SmoothDagger for being lazy! SHAME HIM"
			);
		}

		private async void HandleChannelChatNotificationPrimePaidUpgrade(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			// todo
			var primePaidUpgrade = @event.PrimePaidUpgrade;
            var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
            var username = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUsername}";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{TwitchData.TwitchChannel} :{username} This event was not set up yet. Shame @SmoothDagger for being lazy! SHAME HIM"
			);
		}

		private async void HandleChannelChatNotificationRaid(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			var raid = @event.Raid;
            var username = $"@{raid.Username}";
            var twitchChannel = $"https://www.twitch.tv/{raid.UserLogin}";
            await SendWebSocketMessage(
                message: $"PRIVMSG #{TwitchData.TwitchChannel} :Hello & welcome in, raiders! Shoutout to {username} for the raid! Make sure to go check out them out @ {twitchChannel}"
			);
		}

		private async void HandleChannelChatNotificationResub(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			var resub = @event.Resub;
			var isGifterAnonymous = resub.GifterIsAnonymous ?? false;
            var isResubGift = resub.IsGift ?? false;
			var resubDuration = resub.DurationMonths ?? 0;
			var resubCumulativeMonths = resub.CumulativeMonths ?? 0;
			var resubStreakMonths = resub.StreakMonths ?? 0;
            var resubTier = int.Parse(
                s: resub.SubTier[0].ToString()
			);
            var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
			var username = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUsername}";
			var usernameGifter = isGifterAnonymous ? "@Anonymous" : $"@{resub.GifterUsername}";
            var isResubPrime = resub.IsPrime ?? false;
            var resubText = isResubPrime ?
				$" Thank you so much for the prime resub!" :
				$" Thank you so much for the tier {resubTier} {resubDuration} month {(isResubGift ? "gifted" : string.Empty)} resub!";
			var resubGiftText = isResubGift ? $" Shoutout to {usernameGifter} for the gifted resub!" : string.Empty;
			var resubMonthsText = $" {username} has been subbed for a total of {resubCumulativeMonths} months{(resubStreakMonths > 1u ? $" & is on a {resubStreakMonths} month sub streak!" : "!")}";
			var resubCommandsText = " Make sure to check out the available sub commands in the Social section below for your sub benefits @ https://www.twitch.tv/smoothdagger/about";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{TwitchData.TwitchChannel} :{username}{resubText}{resubGiftText}{resubMonthsText}{resubCommandsText}"
			);
		}

		private async void HandleChannelChatNotificationSub(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			var sub = @event.Sub;
			var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
			var subDuration = sub.DurationMonths ?? 0;
			var subTier = int.Parse(
                s: sub.SubTier[0].ToString()
			);
			var username = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUsername}";
			var isPrimeSub = sub.IsPrime ?? false;
			var subText = isPrimeSub ?
				$" Thank you so much for the prime sub!" :
				$" Thank you so much for the tier {subTier} {subDuration} month sub!";
			var subCommandsText = " Make sure to check out the available sub commands in the Social section below for your sub benefits @ https://www.twitch.tv/smoothdagger/about";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{TwitchData.TwitchChannel} :{username}{subText}{subCommandsText}"
			);
		}

		private async void HandleChannelChatNotificationSubGift(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			var subGift = @event.SubGift;
			var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
			var subGiftCumulativeTotal = subGift.CumulativeTotal ?? 0;
			var subGiftDuration = subGift.DurationMonths ?? 0;
			var subGiftTier = int.Parse(
				subGift.SubTier[0].ToString()
			);
			var username = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUsername}";
			var usernameRecipient = $"@{subGift.RecipientUsername}";
			var subGiftText = $" Thank you so much for the tier {subGiftTier} {subGiftDuration} month gifted sub to @{usernameRecipient}!";
			var subGiftMonthsText = isChatterAnonymous ? string.Empty : $"{username} has gifted a total of {subGiftCumulativeTotal} sub{(subGiftCumulativeTotal > 1u ? "s" : string.Empty)}!";
			var subCommandsText = " Make sure to check out the available sub commands in the Social section below for your sub benefits @ https://www.twitch.tv/smoothdagger/about";
			await SendWebSocketMessage(
				$"PRIVMSG #{TwitchData.TwitchChannel} :{username}{subGiftText}{subGiftMonthsText}{subCommandsText}"
			);
		}

		private void HandleWebSocketMessage(
			string message
		)
		{
			ParseWebSocketMessage(
                message: message,
                webSocketMessages: out var webSocketMessages
			);
			foreach (var webSocketMessage in webSocketMessages)
			{
				switch (webSocketMessage.Command)
				{
					case "CLEARMCHAT":
						break;

					case "CLEARMSG":
						break;

					case "GLOBALUSERSTATE":
						break;

					case "HOSTTARGET":
						break;

					case "NOTICE":
						break;

					case "PING":
						HandleWebSocketMessagePing(
                            webSocketMessage: webSocketMessage
						);
						break;

					case "PRIVMSG":
						HandleWebSocketMessagePrivMsg(
                            webSocketMessage: webSocketMessage
						);
						break;

					case "RECONNECT":
						break;

					case "ROOMSTATE":
						break;

					case "USERNOTICE":
						break;

					case "WHISPER":
						break;

					default:
						break;
				}
			}
		}

		private async void HandleWebSocketMessagePing(
			WebSocketMessage webSocketMessage
		)
		{
			await SendWebSocketMessage(
                message: $"PONG :{webSocketMessage.Text}"
			);
		}

		private void HandleWebSocketMessagePrivMsg(
			WebSocketMessage webSocketMessage
		)
		{
			if (m_messageTimestamps.Count == c_minimumMessageCount)
			{
				m_messageTimestamps.Dequeue();
			}
			m_messageTimestamps.Enqueue(
                item: Time.GetTicksMsec()
			);

			ProcessChatMessage(
                webSocketMessage: webSocketMessage
			);
			ProcessChatCommand(
                webSocketMessage: webSocketMessage
			);
		}

		private async void HandleWebsSocketMessageCommandOnCooldown(
			WebSocketMessage webSocketMessage,
			string commandText
		)
		{
			var message = $"{commandText} is currently on cooldown.";
			await SendWebSocketMessage(
                message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
			);
			AddBotChatMessage(
                message: message
			);
		}

		private async void HandleWebSocketMessagePrivMsgCommands(
			WebSocketMessage webSocketMessage
		)
		{
			var message = c_automatedMessages[AutomatedMessageType.Commands];
			await SendWebSocketMessage(
                message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
			);
			AddBotChatMessage(
                message: message
			);
		}

		private async void HandleWebSocketMessagePrivMsgDate(
			WebSocketMessage webSocketMessage
		)
		{
			var text = webSocketMessage.Text;
			text = text.Replace(
				oldValue: c_commands[CommandType.Date],
				newValue: string.Empty
			);
			text = text.Replace(
				oldValue: "\r\n",
				newValue: string.Empty
			);
			if (
				string.IsNullOrEmpty(
					value: text
				)
			)
			{
				var dateTime = DateTime.UtcNow;
				var message = $"The current date in UTC is {dateTime:d-MMM-yyyy}.";
				message = message.Replace(
					oldChar: '-',
					newChar: ' '
				);
				await SendWebSocketMessage(
					message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
				);
				AddBotChatMessage(
					message: message
				);
			}
			else
			{
				text = text.Replace(
					oldValue: " ",
					newValue: string.Empty
				).ToUpper();

				var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(
					id: text
				);
				var dateTime = TimeZoneInfo.ConvertTimeFromUtc(
					dateTime: DateTime.UtcNow,
					destinationTimeZone: timeZoneInfo
				);
				var message = $"The current date in {text} is {dateTime:d-MMM-yyyy}.";
				message = message.Replace(
					oldChar: '-',
					newChar: ' '
				);
				await SendWebSocketMessage(
					message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
				);
				AddBotChatMessage(
					message: message
				);
			}
		}

		private async void HandleWebSocketMessagePrivMsgDiscord(
			WebSocketMessage webSocketMessage
		)
		{
			var message = c_automatedMessages[AutomatedMessageType.Discord];
			await SendWebSocketMessage(
				message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
			);
			AddBotChatMessage(
				message: message
			);
		}

		private async void HandleWebSocketMessagePrivMsgFollowAge(
			WebSocketMessage webSocketMessage
		)
		{
			string message;
			var channelFollowers = m_twitchManager.GetChannelFollowers();
			foreach (var channelFollower in channelFollowers)
			{
				var username = channelFollower.UserLogin;
				if (
					string.Compare(
						strA: username, 
						strB: webSocketMessage.Username
					) is 0
				)
				{
					var utcNow = Time.GetDatetimeStringFromSystem(
						utc: true
					);
					var dateLength = DateCalculator.CalculateTimeDifference(
						timeStart: channelFollower.FollowedAt,
						timeEnd: utcNow
					);

					var followTime = string.Empty;
					if (dateLength.Year > 0u)
					{
						followTime += $"{dateLength.Year} year{(dateLength.Year > 1u ? "s" : "")}";
					}
					if (dateLength.Month > 0u)
					{
						followTime += followTime == string.Empty ? "" : " ";
						followTime += $"{dateLength.Month} month{(dateLength.Month > 1u ? "s" : "")}";
					}
					if (dateLength.Day > 0u)
					{
						followTime += followTime == string.Empty ? "" : " ";
						followTime += $"{dateLength.Day} day{(dateLength.Day > 1u ? "s" : "")}";
					}
					if (dateLength.Hour > 0u)
					{
						followTime += followTime == string.Empty ? "" : " ";
						followTime += $"{dateLength.Hour} hour{(dateLength.Hour > 1u ? "s" : "")}";
					}
					if (dateLength.Minute > 0u)
					{
						followTime += followTime == string.Empty ? "" : " ";
						followTime += $"{dateLength.Minute} minute{(dateLength.Minute > 1u ? "s" : "")}";
					}
					if (dateLength.Second > 0u)
					{
						followTime += followTime == string.Empty ? "" : " ";
						followTime += $"{dateLength.Second} second{(dateLength.Second > 1u ? "s" : "")}";
					}

					message = $"You have been following @SmoothDagger for {followTime}! Thanks for following!";
					await SendWebSocketMessage(
						message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
					);
					AddBotChatMessage(
						message: message
					);
					return;
				}
			}

			message = $"You are not currently following @SmoothDagger.";
			await SendWebSocketMessage(
				message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
			);
			AddBotChatMessage(
				message: message
			);
		}

		private async void HandleWebSocketMessagePrivMsgRules(
			WebSocketMessage webSocketMessage
		)
		{
			var message = c_automatedMessages[AutomatedMessageType.Rules];
			await SendWebSocketMessage(
                message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
			);
			AddBotChatMessage(
                message: message
            );
		}

		private async void HandleWebSocketMessagePrivMsgSteam(
			WebSocketMessage webSocketMessage
		)
		{
			var message = c_automatedMessages[AutomatedMessageType.Steam];
			await SendWebSocketMessage(
				message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
			);
			AddBotChatMessage(
                message: message
            );
		}

        private async void HandleWebSocketMessagePrivMsgStreamAvatars(
		    WebSocketMessage webSocketMessage
		)
        {
            var message = c_automatedMessages[AutomatedMessageType.StreamAvatars];
            await SendWebSocketMessage(
                message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
            );
            AddBotChatMessage(
                message: message
            );
        }

        private async void HandleWebSocketMessagePrivMsgTextToSpeech(
			WebSocketMessage webSocketMessage
		)
		{
			string message;
			var username = webSocketMessage.Username;
			if (
				string.Compare(
					strA: username,
					strB: TwitchData.AccountUsername
				) is 0
			)
			{
				var text = webSocketMessage.Text;
				text = text.Replace(
					oldValue: c_commands[CommandType.TextToSpeech] + ' ',
					newValue: string.Empty
				);
				m_audioManager.PlayTextToSpeech(
					text: text
				);
				return;
			}
			else if (
				m_subscribersWhoUsedTextToSpeech.Contains(
					item: username
				)
			)
			{
				message = $"You can only use the subscriber !tts command once per stream.";
				await SendWebSocketMessage(
                    message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
				);
				AddBotChatMessage(
                    message: message
                );
				return;
			}

			var channelSubscribers = m_twitchManager.GetChannelSubscribers();
			foreach (var channelSubscriber in channelSubscribers)
			{
				var subscriberUsername = channelSubscriber.Username;
				var subsciberUsernameAdjusted = subscriberUsername.ToLower();

				if (
					string.Compare(
						strA: subsciberUsernameAdjusted,
						strB: username
					) is 0
				)
				{
					var text = webSocketMessage.Text;
					text = text.Replace(
						oldValue: c_commands[CommandType.TextToSpeech],
						newValue: string.Empty
					);
					m_audioManager.PlayTextToSpeech(
						text: text
					);
					m_subscribersWhoUsedTextToSpeech.Add(
						item: username
					);
					c_commandTimers[CommandType.TextToSpeech] = c_commandCooldowns[CommandType.TextToSpeech];
					return;
				}
			}

			message = $"You must be subscribed in order to use this command.";
			await SendWebSocketMessage(
                message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
			);
			AddBotChatMessage(
                message: message
            );
		}

		private async void HandleWebSocketMessagePrivMsgTime(
			WebSocketMessage webSocketMessage
		)
		{
			var text = webSocketMessage.Text;
			text = text.Replace(
				c_commands[CommandType.Time],
				string.Empty
			);
			text = text.Replace(
				"\r\n",
				string.Empty
			);
			if (
				string.IsNullOrEmpty(
					value: text
				)
			)
			{
				var dateTime = DateTime.UtcNow;
				var message = $"The current time in UTC is {dateTime:HH:mm:ss}.";
				await SendWebSocketMessage(
					message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
				);
				AddBotChatMessage(
					message: message
				);
			}
			else
			{
				text = text.Replace(
					oldValue: " ",
					newValue: string.Empty
				).ToUpper();

				var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(
					id: text
				);
				var dateTime = TimeZoneInfo.ConvertTimeFromUtc(
					dateTime: DateTime.UtcNow,
					destinationTimeZone: timeZoneInfo
				);
				var message = $"The current time in {text} is {dateTime:HH:mm:ss}.";
				await SendWebSocketMessage(
                    message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
				);
				AddBotChatMessage(
                    message: message
                );
			}
		}

		private async void HandleWebSocketMessagePrivMsgYouTube(
			WebSocketMessage webSocketMessage
		)
		{
			var message = c_automatedMessages[AutomatedMessageType.YouTube];
			await SendWebSocketMessage(
                message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
			);
			AddBotChatMessage(
                message: message
            );
		}

        private bool IsApplicationCommandOverlayValid(
		    CommandType commandType,
		    string text
		)
        {
            var commandText = c_commands[commandType];
            var commandLength = commandText.Length;

            return commandType switch
            {
                CommandType.Age or
				CommandType.Discord or
				CommandType.Commands or
				CommandType.FollowAge or
				CommandType.Rules or
				CommandType.Steam or
				CommandType.StreamAvatars or
				CommandType.YouTube =>
					IsOverlayCommandAutomatedValid(
                        commandType: commandType,
                        text: text,
                        commandLength: commandLength
                    ),

                CommandType.Date or
				CommandType.Time =>
					IsOverlayCommandDateTimeValid(
                        commandType: commandType,
                        text: text,
                        commandLength: commandLength
                    ),

                CommandType.TextToSpeech =>
					IsOverlayCommandTextToSpeechValid(
                        commandType: commandType,
                        text: text,
                        commandLength: commandLength
                    ),

                _ => 
				false,
            };
        }

		private bool IsApplicationCommandStreamAvatarsTextValid(
			CommandType commandType,
			string text
		)
		{
            var normalizedText = text.ToLower();
            return normalizedText.StartsWith(
                value: c_commands[commandType]
            );
        }

        private bool IsApplicationCommandStreamAvatarsValid(
		    CommandType commandType,
		    string text
		)
        {
            return commandType switch
            {
                CommandType.Actions or
                CommandType.Attack or
                CommandType.Avatar or
                CommandType.Avatars or
                CommandType.Basketball or
                CommandType.BattleRoyale or
                CommandType.Bet or
                CommandType.Blacklist or
                CommandType.Bomb or
                CommandType.Boss or
                CommandType.Buy or
                CommandType.Change or
                CommandType.Color or
                CommandType.Currency or
                CommandType.Dance or
                CommandType.Duel or
                CommandType.Explode or
                CommandType.Extension or
                CommandType.Fart or
                CommandType.Freeze or
                CommandType.Game or
                CommandType.Gear or
                CommandType.Gift or
                CommandType.HideAvatar or
                CommandType.Hug or
                CommandType.Jump or
                CommandType.Leaderboard or
                CommandType.Mass or
                CommandType.Mod or
                CommandType.NameTags or
                CommandType.Pin or
                CommandType.Quote or
                CommandType.Random or
                CommandType.Remove or
                CommandType.Roll or
                CommandType.Scale or
                CommandType.ScreenSaver or
                CommandType.Shop or
                CommandType.Shoutout or
                CommandType.Show or
                CommandType.Sit or
                CommandType.Sling or
                CommandType.Slots or
                CommandType.Sounds or
                CommandType.Spawn or
                CommandType.Throw or
                CommandType.Whitelist =>
					IsApplicationCommandStreamAvatarsTextValid(
						commandType: commandType,
						text: text
					),

                CommandType.Age or
                CommandType.Commands or
                CommandType.Date or
                CommandType.Discord or
                CommandType.FollowAge or
                CommandType.Rules or
                CommandType.Steam or
                CommandType.StreamAvatars or
                CommandType.TextToSpeech or
                CommandType.Time or
                CommandType.YouTube or
                _ =>
					false,
            };
        }

        private bool IsCommandAvailable(
			CommandType commandType
		)
		{
			return Mathf.IsEqualApprox(
				a: c_commandTimers[commandType],
				b: 0d
			);
		}

		private bool IsOverlayCommandAutomatedValid(
			CommandType commandType,
			string text,
			int commandLength
		)
		{
			return
				text.Length - c_twitchMessageDelimiterLength == commandLength &&
				string.Compare(
				    strA: text.Substr(
				        from: 0,
				        len: commandLength
				    ).ToLower(),
				    strB: c_commands[commandType]
				) is 0;
		}

		private bool IsOverlayCommandDateTimeValid(
			CommandType commandType,
			string text,
			int commandLength
		)
		{
            const int dateTimeExactLength = 9;
            const int dateTimeSpaceIndex = 5;
            const int dateTimeAbbreviationLength = 3;
            var textLength = text.Length - c_twitchMessageDelimiterLength;
            return
                (
                    textLength == commandLength &&
                    string.Compare(
                        strA: text.Substr(
                            from: 0,
                            len: commandLength
                        ).ToLower(),
                        strB: c_commands[commandType]
                    ) is 0
                ) ||
                (
                    textLength == dateTimeExactLength &&
                    text[dateTimeSpaceIndex] is ' ' &&
                    string.Compare(
                        strA: text.Substr(
                            from: 0,
                            len: commandLength
                        ).ToLower(),
                        strB: c_commands[commandType]
                    ) is 0 &&
                    TimeZones.IsTimeZoneAbbreviationValid(
                        abbreviation: text.Split(
                            separator: ' '
                        )[1].Substr(
                            from: 0,
                            len: dateTimeAbbreviationLength
                        ).ToUpper()
                    )
                );
        }

		private bool IsOverlayCommandTextToSpeechValid(
			CommandType commandType,
			string text,
			int commandLength
		)
		{
            const int ttsMinimumLength = 6;
            const int ttsSpaceIndex = 4;
            const int ttsFirstCharacterIndex = 5;
            return
                text.Length >= ttsMinimumLength &&
				text[ttsSpaceIndex] is ' ' &&
                string.Compare(
                    strA: text.Substr(
                        from: 0,
						len: commandLength
                    ).ToLower(),
                    strB: c_commands[commandType]
                ) is 0 &&
                char.IsLetterOrDigit(
                    c: text[ttsFirstCharacterIndex]
                );
        }

		private static bool IsSmoothGPT(
			string username
		)
		{
			return username is c_twitchBotUsername;
		}

		private void OnChannelChatNotification(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			switch (@event.NoticeType)
			{
				case "bits_badge_tier":
					HandleChannelChatNotificationBitsBadgeTier(
						@event: @event
					);
					break;

				case "charity_donation":
					HandleChannelChatNotificationCharityDonation(
                        @event: @event
                    );
					break;

				case "community_sub_gift":
					HandleChannelChatNotificationCommunitySubGift(
                        @event: @event
                    );
					break;

				case "gift_paid_upgrade":
					HandleChannelChatNotificationGiftPaidUpgrade(
                        @event: @event
                    );
					break;

				case "pay_it_forward":
					HandleChannelChatNotificationPayItForward(
                        @event: @event
                    );
					break;

				case "prime_paid_upgrade":
					HandleChannelChatNotificationPrimePaidUpgrade(
                        @event: @event
                    );
					break;

				case "raid":
					HandleChannelChatNotificationRaid(
                        @event: @event
                    );
					break;

				case "resub":
					HandleChannelChatNotificationResub(
                        @event: @event
                    );
					break;

				case "sub":
					HandleChannelChatNotificationSub(
                        @event: @event
                    );
					break;

				case "sub_gift":
					HandleChannelChatNotificationSubGift(
                        @event: @event
                    );
					break;

				case "announcement":
				case "unraid":
				default:
					return;
			}
		}

		private static string ParseTextSubCommand(
			string text
		)
		{
			var subCommand = string.Empty;
			var index = 0;
			while (text[index++] is not ' ') ;

			while (index < text.Length)
			{
				subCommand += text[index++];
			}

			subCommand = subCommand.Remove(
				startIndex: subCommand.Length - c_webSocketMessageDelimiterLength,
				count: c_webSocketMessageDelimiterLength
			);

			return subCommand;
		}

		private static void ParseWebSocketMessage(
			string message,
			out List<WebSocketMessage> webSocketMessages
		)
		{
			webSocketMessages = new();

			var index = 0;
			while (index < message.Length)
			{
				var webSocketMessage = new WebSocketMessage(
					tags: new(),
					username: string.Empty,
					command: string.Empty,
					text: string.Empty
				);

				// parse tags
				if (message[index] is '@')
				{
					index++;
					while (true)
					{
						var key = string.Empty;
						while (message[index] is not '=')
						{
							key += message[index++];
						}
						index++;

						var value = string.Empty;
						while (message[index] is not ';' && message[index] is not ' ')
						{
							value += message[index++];
						}

						webSocketMessage.Tags.Add(
							key: key,
							value: value
						);

						if (message[index++] is ' ')
						{
							break;
						}
					}
				}

				// parse username
				if (message[index] is ':')
				{
					while (message[index] is not '@' && message[index] is not ' ')
					{
						index++;
					}
					if (message[index++] is '@')
					{
						while (message[index] is not '.')
						{
							webSocketMessage.Username += message[index++];
						}
						while (message[index] is not ' ')
						{
							index++;
						}
						index++;
					}
				}

				// parse command
				while (
					index < message.Length &&
					message[index] is not ' '
				)
				{
					webSocketMessage.Command += message[index++];
				}

				if (
					webSocketMessage.Command.EndsWith(
						value: c_webSocketMessagedelimiter
					)
				)
				{
					webSocketMessage.Command = webSocketMessage.Command.Remove(
						startIndex: webSocketMessage.Command.Length - c_webSocketMessageDelimiterLength,
						count: c_webSocketMessageDelimiterLength
					);
				}
				else
				{
					// parse extraneous information up to possible text message
					var parse = string.Empty;
					while (message[index] is not ':')
					{
						parse += message[index++];
						if (
							parse.EndsWith(
								value: c_webSocketMessagedelimiter
							)
						)
						{
							break;
						}
					}

					// parse text message up to delimiter
					if (
						parse.EndsWith(
							value: c_webSocketMessagedelimiter
						) is false
					)
					{
						index++;
						while (index < message.Length)
						{
							if (
								webSocketMessage.Text.EndsWith(
									value: c_webSocketMessagedelimiter
								)
							)
							{
								webSocketMessage.Text = webSocketMessage.Text.Remove(
									startIndex: webSocketMessage.Text.Length - c_webSocketMessageDelimiterLength,
									count: c_webSocketMessageDelimiterLength
								);
								break;
							}

							webSocketMessage.Text += message[index++];
						}
					}
				}

				webSocketMessages.Add(
					item: webSocketMessage
				);
			}
		}

		private async void ProcessChatCommand(
			WebSocketMessage webSocketMessage
		)
		{
			var text = webSocketMessage.Text;
			if (text[0] is '!')
			{
				foreach (var command in c_commands)
				{
					var commandType = command.Key;
					var applicationCommandType = GetApplicationCommandType(
						commandType: commandType
					);

					switch (applicationCommandType)
					{
						case ApplicationCommandType.Overlay:
							if (
								IsApplicationCommandOverlayValid(
									commandType: commandType,
									text: text
								)
							)
							{
								HandleApplicationCommandOverlay(
									commandType: commandType,
									webSocketMessage: webSocketMessage
								);
								return;
							}
							break;
						case ApplicationCommandType.StreamAvatars:
							if (
                                IsApplicationCommandStreamAvatarsValid(
									commandType: commandType,
									text: text
								)
							)
							{
								return;
							}
							break;

						case ApplicationCommandType.Invalid:
						default:
							// should never hit, something went wrong
							break;
					}
				}

				var message = "This is not a valid chat command. Check the Social section below to see the available bot commands @ https://www.twitch.tv/SmoothDagger/About";
				await SendWebSocketMessage(
					message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
				);
				AddBotChatMessage(
					message: message
				);
			}
		}

		private void ProcessChatMessage(
			WebSocketMessage webSocketMessage
		)
		{
			// process message for on-screen chat
			var username = webSocketMessage.Username;
			var isSubscriber = webSocketMessage.Tags["subscriber"].ToInt() > 0u;
			string color;
			if (isSubscriber)
			{
				color = string.Empty;
			}
			else if (
				m_usernameColors.ContainsKey(
					key: username
				)
			)
			{
				color = m_usernameColors[username];
			}
			else
			{
				var userColorInHex = webSocketMessage.Tags["color"];
				if (
					string.IsNullOrWhiteSpace(
						userColorInHex
						) is true
					)
				{
					color = m_pastelInterpolator.GetColorAsHex();
				}
				else
				{
					var userColor = Color.FromString(
					    str: userColorInHex,
						@default: default
					);
					color = $"{(int)((userColor.R8 + 255) / 2f):X}" +
							$"{(int)((userColor.G8 + 255) / 2f):X}" +
							$"{(int)((userColor.B8 + 255) / 2f):X}" +
							$"FF";
				}

                m_usernameColors.Add(
					key: username,
					value: color
                );
			}
			var name = webSocketMessage.Tags["display-name"];
			if (
				string.Compare(
					strA: name.ToLower(), 
					strB: username
				) is not 0
			)
			{
				name += $" ({webSocketMessage.Username})";
			}
			var message = webSocketMessage.Text;
			var emotes = webSocketMessage.Tags.ContainsKey(
				key: "emotes"
			) ? webSocketMessage.Tags["emotes"] : string.Empty;

			m_twitchChatManager.AddTwitchChatMessage(
				name: name,
				color: color,
				message: message,
				emotes: emotes
			);
		}

		private void RetrieveResources()
		{
			m_audioManager = GetNode<AudioManager>(
				path: NodeDirectory.NodePaths[NodeType.AudioManager]
			);
			m_pastelInterpolator = GetNode<PastelInterpolator>(
                path: NodeDirectory.NodePaths[NodeType.PastelInterpolator]
			);
			m_twitchChannelPointRewardsManager = GetNode<TwitchChannelPointRewardsManager>(
				path: NodeDirectory.NodePaths[NodeType.TwitchChannelPointRewardsManager]
			);
			m_twitchChatManager = GetNode<TwitchChatManager>(
                path: NodeDirectory.NodePaths[NodeType.TwitchChatManager]
			);
			m_twitchManager = GetNode<TwitchManager>(
                path: NodeDirectory.NodePaths[NodeType.TwitchManager]
			);

			m_twitchManager.ChannelChatNotification += OnChannelChatNotification;
		}

		private async void SendAutomatedMessage()
		{
			var message = c_automatedMessages[m_currentAutomatedMessage];
			await SendWebSocketMessage(
				message: $"PRIVMSG #{TwitchData.TwitchChannel} :{message}"
			);
			AddBotChatMessage(
				message: message
			);
		}

		private async Task SendWebSocketMessage(
			string message
		)
		{
			await m_webSocket.SendAsync(
				buffer: Encoding.UTF8.GetBytes(
					s: message
				),
				messageType: WebSocketMessageType.Text,
				endOfMessage: true,
				cancellationToken: default
			);
		}

		private async void StartWebSocketAutomatedMessageDispatcher()
		{
			await Task.Run(
				function: async () =>
				{
#if DEBUG
					GD.Print(
						what: $"{nameof(TwitchBot)}.{nameof(StartWebSocketAutomatedMessageDispatcher)}() - Web socket automated message dispatcher starting."
					);
#endif

					// 10 minute delay before next message is sent
					const int delayInMilliseconds = 600000;
					while (m_shutdown is false)
					{
						if (m_webSocket.State is WebSocketState.Open)
						{
							await Task.Delay(
								millisecondsDelay: delayInMilliseconds
							);

							if (m_messageTimestamps.Count < c_minimumMessageCount)
							{
								continue;
							}

							var lastMessage = m_currentAutomatedMessage;
							while (m_currentAutomatedMessage == lastMessage)
							{
								m_currentAutomatedMessage = (AutomatedMessageType)(GD.Randi() % (int)AutomatedMessageType.Count);
							}
							SendAutomatedMessage();
						}
					}
				}
			);
		}

		private async void StartWebSocketMessageReader()
		{
			await Task.Run(
				function: async () =>
				{
#if DEBUG
					GD.Print(
						what: $"{nameof(TwitchBot)}.{nameof(StartWebSocketMessageReader)}() - Web socket message reader starting."
					);
#endif

					var cancellationToken = new CancellationToken();
					while (m_shutdown is false)
					{
						if (
							m_webSocket.State is WebSocketState.Open && 
							cancellationToken.IsCancellationRequested is false
						)
						{
							var bytes = new byte[c_maxPacketSize];
							var result = await m_webSocket.ReceiveAsync(
								buffer: bytes,
								cancellationToken: cancellationToken
							);

							if (result.Count > 0u)
							{
								HandleWebSocketMessage(
									message: Encoding.UTF8.GetString(
										bytes: bytes,
										index: 0,
										count: result.Count
									)
								);
							}
						}
					}
				}
			);
		}
    }
}