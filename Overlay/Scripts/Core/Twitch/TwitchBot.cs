namespace Overlay
{
	using Godot;
	using System;
	using System.Collections.Generic;
    using System.Net.WebSockets;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Net.Mime.MediaTypeNames;
    using FragmentType = TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragment.FragmentType;
	using NodeType = NodeDirectory.NodeType;

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
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
					) is false
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
            TwitchFollow,
			TwitchSubscribe,
			YouTube,
			Count
		}

		private enum ChatCommandValidityType : uint
		{
			ValidChatCommand = 0u,
			InvalidChatCommand,
			NotAChatCommand,
		}

        private enum CommandInfoMessageType : uint
        {
            SetColor = 0u,
            TextToSpeech,
        }

        private enum CommandType : uint
        {
            AccountAge = 0u,
            Commands,
            Date,
            Discord,
            FollowAge,
            Lurk,
            Rules,
            SetColor,
            Steam,
            StreamAvatars,
            TextToSpeech,
            Time,
            YouTube,

            // Stream Avatars
            Accept,
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
            Decline,
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
		private const string c_twitchBotBadges = "moderator/1";
        private const int c_webSocketMessageDelimiterLength = 2;
        private const int c_twitchMessageDelimiterLength = 2;
        private const uint c_maxPacketSize = 8192u;
		private const uint c_minimumMessageCount = 5u;
		private const ulong c_minimumMessageTimerInMilliseconds = 900000u;

		private static readonly Dictionary<AutomatedMessageType, string> c_automatedMessages = new()
		{
            { AutomatedMessageType.Commands,	    "Check the Socials section below for a list of available bot commands @ https://www.twitch.tv/SmoothDagger/About" },
			{ AutomatedMessageType.Discord,		    "Interested in chatting? Join the Discord @ https://www.discord.gg/SmoothCrew" },
			{ AutomatedMessageType.Rules,		    "Make sure you're following the rules! Find them below in the rules section @ https://www.twitch.tv/SmoothDagger/About" },
			{ AutomatedMessageType.StreamAvatars,   "Want to customize your stream avatar? Select an avatar below in the Stream Avatars section @ https://www.twitch.tv/SmoothDagger/About" },
            { AutomatedMessageType.Steam,		    "Come play with us! Add me on Steam @ https://steamcommunity.com/id/SmoothDagger/" },
            { AutomatedMessageType.TwitchFollow,    "Enjoying the stream? Tap the follow button to get notified for any live streams!" },
            { AutomatedMessageType.TwitchSubscribe, "Want ad-free viewing? Subscribe on Twitch @ https://www.twitch.tv/subs/SmoothDagger" },
            { AutomatedMessageType.YouTube,		    "Want more SmoothDagger content? Subscribe on YouTube @ https://www.youtube.com/@SmoothDagger" },
		};
        private static readonly Dictionary<AutomatedMessageType, string> c_onScreenAutomatedMessages = new()
        {
            { AutomatedMessageType.Commands,        "Check the Socials section below for a list of available bot commands @ \n[color=9BF6FF]https://www.twitch.tv/SmoothDagger/About" },
            { AutomatedMessageType.Discord,         "Interested in chatting? Join the Discord @ \n[color=9BF6FF]https://www.discord.gg/SmoothCrew" },
            { AutomatedMessageType.Rules,           "Make sure you're following the rules! Find them below in the rules section @ \n[color=9BF6FF]https://www.twitch.tv/SmoothDagger/About" },
            { AutomatedMessageType.StreamAvatars,   "Want to customize your stream avatar? Select an avatar below in the Stream Avatars section @ \n[color=9BF6FF]https://www.twitch.tv/SmoothDagger/About" },
            { AutomatedMessageType.Steam,           "Come play with us! Add me on Steam @ \n[color=9BF6FF]https://steamcommunity.com/id/SmoothDagger/" },
            { AutomatedMessageType.TwitchFollow,    "Enjoying the stream? Tap the [color=F898A4]follow[/color] button to get notified for any live streams!" },
            { AutomatedMessageType.TwitchSubscribe, "Want ad-free viewing? Subscribe on Twitch @ \n[color=9BF6FF]https://www.twitch.tv/subs/SmoothDagger" },
            { AutomatedMessageType.YouTube,         "Want more SmoothDagger content? Subscribe on YouTube @ \n[color=9BF6FF]https://www.youtube.com/@SmoothDagger" },
        };
		private static readonly Dictionary<CommandInfoMessageType, string> c_commandInfoMessages = new()
		{
			{ CommandInfoMessageType.SetColor,	   $"" },
			{ CommandInfoMessageType.TextToSpeech, $"" },
        };
        private static readonly Dictionary<CommandInfoMessageType, string> c_onScreenCommandInfoMessages = new()
        {
            { CommandInfoMessageType.SetColor,	   $"" },
            { CommandInfoMessageType.TextToSpeech, $"" },
        };
        private static readonly Dictionary<CommandType, string> c_commands = new()
		{
			// Bot
			{ CommandType.AccountAge,    "!accountage" },
            { CommandType.Commands,      "!commands" },
            { CommandType.Date,          "!date" },
            { CommandType.Discord,       "!discord" },
            { CommandType.FollowAge,     "!followage" },
            { CommandType.Lurk,			 "!lurk" },
            { CommandType.Rules,         "!rules" },
            { CommandType.SetColor,      "!setcolor" },
            { CommandType.Steam,         "!steam" },
            { CommandType.StreamAvatars, "!streamavatars" },
            { CommandType.TextToSpeech,  "!tts" },
            { CommandType.Time,          "!time" },
            { CommandType.YouTube,       "!youtube" },

			// Stream Avatars
            { CommandType.Accept,		 "!accept" },
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
            { CommandType.Decline,		 "!decline" },
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
		private static readonly Dictionary<CommandType, double> c_commandCooldowns = new()
		{
			// Bot
			{ CommandType.AccountAge,    0d  },
            { CommandType.Commands,      10d },
            { CommandType.Date,          0d  },
            { CommandType.Discord,       10d },
            { CommandType.FollowAge,     0d  },
            { CommandType.Lurk,			 0d  },
            { CommandType.Rules,         10d },
            { CommandType.SetColor,      0d  },
            { CommandType.Steam,         10d },
            { CommandType.StreamAvatars, 10d },
            { CommandType.TextToSpeech,  0d  },
            { CommandType.Time,          0d  },
            { CommandType.YouTube,       10d },

			// Stream Avatars
            { CommandType.Accept,        0d },
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
            { CommandType.Decline,       0d },
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
        private static readonly Dictionary<CommandType, double> c_commandTimers = new()
		{
			// Bot
			{ CommandType.AccountAge,    0d },
            { CommandType.Commands,      0d },
            { CommandType.Date,          0d },
            { CommandType.Discord,       0d },
            { CommandType.FollowAge,     0d },
            { CommandType.Lurk,			 0d },
            { CommandType.Rules,         0d },
            { CommandType.SetColor,      0d },
            { CommandType.Steam,         0d },
            { CommandType.StreamAvatars, 0d },
            { CommandType.TextToSpeech,  0d },
            { CommandType.Time,          0d },
            { CommandType.YouTube,       0d },

			// Stream Avatars
			{ CommandType.Accept,        0d },
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
            { CommandType.Decline,       0d },
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
			Task.Run(
				async () =>
				{
					await Task.Delay(
						millisecondsDelay: 5	
					);
					m_twitchChatManager.AddTwitchChatMessage(
						username: c_twitchBotUsername,
					    name: c_twitchBotDisplayName,
						color: string.Empty,
						message: message,
						emotes: string.Empty,
						badges: c_twitchBotBadges,
						isSmoothGPT: true
					);
				}	
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
                CommandType.AccountAge or
				CommandType.Date or
				CommandType.Discord or
				CommandType.Commands or
				CommandType.FollowAge or
				CommandType.Lurk or
				CommandType.Rules or
				CommandType.SetColor or
                CommandType.Steam or
				CommandType.StreamAvatars or
				CommandType.TextToSpeech or
				CommandType.Time or
				CommandType.YouTube => 
					ApplicationCommandType.Overlay,

                CommandType.Accept or
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
                CommandType.Decline or
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
                    case CommandType.AccountAge:
						HandleWebSocketMessagePrivMsgAccountAge(
                            webSocketMessage: webSocketMessage
                        );
                        break;

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

                    case CommandType.Lurk:
                        HandleWebSocketMessagePrivMsgLurk(
                            webSocketMessage: webSocketMessage
                        );
                        break;

                    case CommandType.Rules:
                        HandleWebSocketMessagePrivMsgRules(
                            webSocketMessage: webSocketMessage
                        );
                        break;

                    case CommandType.SetColor:
                        HandleWebSocketMessagePrivMsgSetColor(
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

                    case CommandType.Accept:
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
                    case CommandType.Decline:
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

		private async void HandleInvalidChatCommand(
			WebSocketMessage webSocketMessage
        )
		{
            var messagePrefix = "This is not a valid chat command. Check the Social section below to see the available bot commands @ ";
            var messageSuffix = "\nhttps://www.twitch.tv/SmoothDagger/About";
            await SendWebSocketMessage(
                message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{messagePrefix}{messageSuffix}"
            );
            AddBotChatMessage(
                message: $"{messagePrefix}[color=9BF6FF]{messageSuffix}"
            );
        }

		private async void HandleUserNotFollowingMessage(
			WebSocketMessage webSocketMessage
		)
		{
            var messageToChat = $"You are currently not following @SmoothDagger. Tap the follow button to gain access to this command!";
            var messageToOverlay = $"You are currently [color=F898A4]not[/color] following @SmoothDagger. Tap the [color=CAFFBF]follow[/color] button to gain access to this command!";
            await SendWebSocketMessage(
                message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{messageToChat}"
            );
            AddBotChatMessage(
                message: messageToOverlay
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
			
			var chatCommandValidityType = ProcessChatCommand(
                webSocketMessage: webSocketMessage
			);
			ProcessChatMessage(
                webSocketMessage: webSocketMessage
			);

			switch (chatCommandValidityType)
			{
				case ChatCommandValidityType.InvalidChatCommand:
					HandleInvalidChatCommand(
						webSocketMessage: webSocketMessage	
					);
					break;

				case ChatCommandValidityType.ValidChatCommand:
				case ChatCommandValidityType.NotAChatCommand:
				default:
					break;
			}
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

		private async void HandleWebSocketMessagePrivMsgAccountAge(
			WebSocketMessage webSocketMessage
		)
		{
			var user = m_twitchManager.GetUser(
				username: webSocketMessage.Username
			);
			if (user is not null)
			{
				var utcNow = Time.GetDatetimeStringFromSystem(
					utc: true
				);
				var dateLength = DateCalculator.CalculateTimeDifference(
					timeStart: user.CreatedAt,
					timeEnd: utcNow
				);

				var accountTime = string.Empty;
				if (dateLength.Year > 0u)
				{
					accountTime += $"{dateLength.Year} year{(dateLength.Year > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Month > 0u)
				{
					accountTime += accountTime == string.Empty ? string.Empty : " ";
					accountTime += $"{dateLength.Month} month{(dateLength.Month > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Day > 0u)
				{
					accountTime += accountTime == string.Empty ? string.Empty : " ";
					accountTime += $"{dateLength.Day} day{(dateLength.Day > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Hour > 0u)
				{
					accountTime += accountTime == string.Empty ? string.Empty : " ";
					accountTime += $"{dateLength.Hour} hour{(dateLength.Hour > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Minute > 0u)
				{
					accountTime += accountTime == string.Empty ? string.Empty : " ";
					accountTime += $"{dateLength.Minute} minute{(dateLength.Minute > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Second > 0u)
				{
					accountTime += accountTime == string.Empty ? string.Empty : " ";
					accountTime += $"{dateLength.Second} second{(dateLength.Second > 1u ? "s" : string.Empty)}";
				}

                var messageToChat = $"You've been lurking in the depths of Twitch for {accountTime}! Thanks for being here!";
                var messageToOverlay = $"You've been lurking in the depths of Twitch for [color=CAFFBF]{accountTime}[/color]! Thanks for being here!";
                await SendWebSocketMessage(
                    message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{messageToChat}"
                );
                AddBotChatMessage(
                    message: messageToOverlay
                );
            }
			else
			{
				HandleUserNotFollowingMessage(
					webSocketMessage: webSocketMessage	
				);
            }
		}

		private async void HandleWebSocketMessagePrivMsgCommands(
			WebSocketMessage webSocketMessage
		)
		{
			await SendWebSocketMessage(
                message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{c_automatedMessages[AutomatedMessageType.Commands]}"
			);
			AddBotChatMessage(
                message: $"{c_onScreenAutomatedMessages[AutomatedMessageType.Commands]}"
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
			await SendWebSocketMessage(
				message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{c_automatedMessages[AutomatedMessageType.Discord]}"
			);
			AddBotChatMessage(
				message: $"{c_onScreenAutomatedMessages[AutomatedMessageType.Discord]}"
			);

		}

		private async void HandleWebSocketMessagePrivMsgLurk(
			WebSocketMessage webSocketMessage
		)
		{
			var username = webSocketMessage.Username;
            var name = webSocketMessage.Tags["display-name"];
            if (
                string.Compare(
                    strA: name.ToLower(),
                    strB: username
                ) is not 0
            )
            {
                name += $" ({username})";
            }
            var message = $"{name} engaged lurk mode!";
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
			var username = webSocketMessage.Username;
			var usernameAdjusted = username.ToLower();
			var channelFollowers = m_twitchManager.GetChannelFollowers();
			if (
				channelFollowers.ContainsKey(
					key: usernameAdjusted
                ) is true
			)
			{
				var channelFollower = channelFollowers[usernameAdjusted];
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
					followTime += $"{dateLength.Year} year{(dateLength.Year > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Month > 0u)
				{
					followTime += followTime == string.Empty ? string.Empty : " ";
					followTime += $"{dateLength.Month} month{(dateLength.Month > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Day > 0u)
				{
					followTime += followTime == string.Empty ? string.Empty : " ";
					followTime += $"{dateLength.Day} day{(dateLength.Day > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Hour > 0u)
				{
					followTime += followTime == string.Empty ? string.Empty : " ";
					followTime += $"{dateLength.Hour} hour{(dateLength.Hour > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Minute > 0u)
				{
					followTime += followTime == string.Empty ? string.Empty : " ";
					followTime += $"{dateLength.Minute} minute{(dateLength.Minute > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Second > 0u)
				{
					followTime += followTime == string.Empty ? string.Empty : " ";
					followTime += $"{dateLength.Second} second{(dateLength.Second > 1u ? "s" : string.Empty)}";
				}

				var messageToChat = $"You've been following @SmoothDagger for {followTime}! Thanks for following!";
                var messageToOverlay = $"You've been following @SmoothDagger for [color=CAFFBF]{followTime}[/color]! Thanks for following!";
                await SendWebSocketMessage(
					message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{messageToChat}"
				);
				AddBotChatMessage(
					message: messageToOverlay
                );
				return;
			}

            HandleUserNotFollowingMessage(
                webSocketMessage: webSocketMessage
            );
        }

		private async void HandleWebSocketMessagePrivMsgRules(
			WebSocketMessage webSocketMessage
		)
		{
			await SendWebSocketMessage(
                message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{c_automatedMessages[AutomatedMessageType.Rules]}"
			);
			AddBotChatMessage(
                message: $"{c_onScreenAutomatedMessages[AutomatedMessageType.Rules]}"
            );
		}

		private async void HandleWebSocketMessagePrivMsgSetColor(
			WebSocketMessage webSocketMessage
		)
		{
			const int indexCommand = 0;
			const int indexColor = 1;

            //var channelSubscribers = m_twitchManager.GetChannelSubscribers();
            //foreach (var channelSubscriber in channelSubscribers)
            //{
            //    var subscriberUsername = channelSubscriber.Username;
            //    var subsciberUsernameAdjusted = subscriberUsername.ToLower();
			//
            //    if (
            //        string.Compare(
            //            strA: subsciberUsernameAdjusted,
            //            strB: username
            //        ) is 0
            //    )
            //    {
            //        var text = webSocketMessage.Text;
            //        text = text.Replace(
            //            oldValue: c_commands[CommandType.TextToSpeech],
            //            newValue: string.Empty
            //        );
            //        m_audioManager.PlayTextToSpeech(
            //            text: text
            //        );
            //        m_subscribersWhoUsedTextToSpeech.Add(
            //            item: username
            //        );
            //        c_commandTimers[CommandType.TextToSpeech] = c_commandCooldowns[CommandType.TextToSpeech];
            //        return;
            //    }
            //}
			//
            //message = $"You must be subscribed in order to use this command.";
            //await SendWebSocketMessage(
            //    message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
            //);
            //AddBotChatMessage(
            //    message: message
            //);

            var parsedText = webSocketMessage.Text.Split(
				separator: ' '
			);

            var commandText = parsedText[indexCommand].ToLower();
            if (
                string.Compare(
                    strA: $"{c_commands[CommandType.SetColor]}",
                    strB: commandText
                ) is 0
            )
            {
				await SendWebSocketMessage(
					message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{c_commandInfoMessages[CommandInfoMessageType.SetColor]}"
				);
				AddBotChatMessage(
				    message: $"{c_onScreenCommandInfoMessages[CommandInfoMessageType.SetColor]}"
				);
                return;
            }

            var colorCode = parsedText[indexColor].Remove(
                startIndex: parsedText[indexColor].Length - c_webSocketMessageDelimiterLength
            );
            var username = webSocketMessage.Username;
			var customSubscriberData = m_twitchManager.GetCustomSubscriberData(
				username: username
            );
			if (customSubscriberData is null)
			{
				m_twitchManager.SetCustomSubscriberData(
					username: username,
					data: new()
					{
						CustomTextColor = colorCode
                    }
                );
			}
			else
			{
				customSubscriberData.CustomTextColor = colorCode;
				m_twitchManager.SetCustomSubscriberData(
					username: username,
					data: customSubscriberData
				);
			}
		}

		private async void HandleWebSocketMessagePrivMsgSteam(
			WebSocketMessage webSocketMessage
		)
		{
			await SendWebSocketMessage(
				message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{c_automatedMessages[AutomatedMessageType.Steam]}"
			);
			AddBotChatMessage(
                message: $"{c_onScreenAutomatedMessages[AutomatedMessageType.Steam]}"
            );
		}

        private async void HandleWebSocketMessagePrivMsgStreamAvatars(
		    WebSocketMessage webSocketMessage
		)
        {
            await SendWebSocketMessage(
                message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{c_automatedMessages[AutomatedMessageType.StreamAvatars]}"
            );
            AddBotChatMessage(
                message: $"{c_onScreenAutomatedMessages[AutomatedMessageType.StreamAvatars]}"
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
			if (
				channelSubscribers.ContainsKey(
                    key: username
                )
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
				oldValue: c_commands[CommandType.Time],
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
			await SendWebSocketMessage(
                message: $"@reply-parent-msg-id={webSocketMessage.Tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{c_automatedMessages[AutomatedMessageType.YouTube]}"
			);
			AddBotChatMessage(
                message: $"{c_onScreenAutomatedMessages[AutomatedMessageType.YouTube]}"
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
                CommandType.AccountAge or
				CommandType.Discord or
				CommandType.Commands or
				CommandType.FollowAge or
				CommandType.Lurk or
				CommandType.Rules or
				CommandType.Steam or
				CommandType.StreamAvatars or
				CommandType.YouTube =>
					IsOverlayCommandInputlessValid(
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

                CommandType.SetColor =>
					IsOverlayCommandSetColorValid(
						text: text	
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
                CommandType.Accept or
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
                CommandType.Decline or
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

                CommandType.AccountAge or
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

        private static bool IsCommandAvailable(
			CommandType commandType
		)
		{
			return Mathf.IsEqualApprox(
				a: c_commandTimers[commandType],
				b: 0d
			);
		}

		private static bool IsOverlayCommandDateTimeValid(
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

		private static bool IsOverlayCommandInputlessValid(
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

		private static bool IsOverlayCommandSetColorValid(
			string text
		)
		{
			var normalizedText = text.ToLower();
			if (
				string.Compare(
					strA: $"{c_commands[CommandType.SetColor]}",
					strB: text
				) is 0
			)
			{
				return true;
			}

            var pattern = $"^{c_commands[CommandType.SetColor]} ([0-9A-Fa-f]{{6}})$";
			return Regex.IsMatch(
				input: normalizedText.Remove(
                    normalizedText.Length - c_webSocketMessageDelimiterLength
                ),
				pattern: pattern
			);
        }

		private static bool IsOverlayCommandTextToSpeechValid(
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

		private ChatCommandValidityType ProcessChatCommand(
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
								return ChatCommandValidityType.ValidChatCommand;
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
								return ChatCommandValidityType.ValidChatCommand;
							}
							break;

						case ApplicationCommandType.Invalid:
						default:
							// should never hit, something went wrong
							break;
					}
				}
				return ChatCommandValidityType.InvalidChatCommand;
			}
			return ChatCommandValidityType.NotAChatCommand;
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
				name += $" ({username})";
			}
			var message = webSocketMessage.Text;
			var emotes = webSocketMessage.Tags.ContainsKey(
				key: "emotes"
			) ? webSocketMessage.Tags["emotes"] : string.Empty;
			var badges = webSocketMessage.Tags.ContainsKey(
				key: "badges"
			) ? webSocketMessage.Tags["badges"] : string.Empty;

			m_twitchChatManager.AddTwitchChatMessage(
				username: username,
                name: name,
				color: color,
				message: message,
				emotes: emotes,
				badges: badges,
				isSmoothGPT: false
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
			await SendWebSocketMessage(
				message: $"PRIVMSG #{TwitchData.TwitchChannel} :{c_automatedMessages[m_currentAutomatedMessage]}"
			);
			AddBotChatMessage(
				message: $"{c_onScreenAutomatedMessages[m_currentAutomatedMessage]}"
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