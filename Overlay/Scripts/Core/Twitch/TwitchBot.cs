
namespace Overlay
{
	using Godot;
	using System;
	using System.Collections.Generic;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
	using static Godot.HttpClient;
    using ColorType = PastelInterpolator.ColorType;
    using FragmentType = TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragment.FragmentType;
	using NodeType = NodeDirectory.NodeType;
	using RainbowColorIndexType = PastelInterpolator.RainbowColorIndexType;
    using RequiredFileType = ApplicationManager.RequiredFileType;
	using UILayoutType = UIManager.UILayoutType;

    [SupportedOSPlatform(platformName: "windows")]
    public sealed partial class TwitchBot : Node
	{
		public override void _EnterTree()
		{
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
            RequestAccessTokenWithRefreshToken();
		}

        private enum TwitchChatAutomatedMessageType : uint
        {
            Commands = 0u,
            Discord,
            Rules,
            Steam,
            Supporter,
            TwitchFollow,
            TwitchSubscribe,
            YouTube
        }

        private enum TwitchChatCommandType : uint
        {
            AccountAge = 0u,
            Commands,
            CS,
            Current,
            CurrentSong,
            Date,
            Discord,
            FollowAge,
			Layout,
            Lurk,
            Queue,
            Rules,
			SetLayout,
            SetColor,
            SetColour,
            Skip,
            Song,
            SongQueue,
            SongRequest,
            SongSkip,
            Specs,
            SR,
            Steam,
            TextToSpeech,
            Time,
            Unlurk,
            YouTube,
        }

        private enum TwitchChatCommandInfoMessageType : uint
        {
            SetColorSelection = 0u,
            SetColorUsage,
            TextToSpeech,
        }

        private enum TwitchChatCommandValidityType : uint
        {
            ValidChatCommand = 0u,
            InvalidChatCommand,
            NotAChatCommand,
        }

        private const int c_automatedMessageDelayInMilliseconds = 600000;
        private const int c_maxSpotifyQueueCount = 3;
		private const int c_minimumMessageCount = 5;
        private const int c_twitchMessageDelimiterLength = 2;
        private const int c_webSocketMessageDelimiterLength = 2;
        private const uint c_maxPacketSize = 8192u;
		private const ulong c_minimumMessageTimerInMilliseconds = 900000u;

        private const string c_twitchUriOAuth = "https://id.twitch.tv/oauth2/token";
		private const string c_twitchUriRedirect = "http://localhost:3000";
        private const string c_twitchWebSocketAddress = "wss://irc-ws.chat.twitch.tv:443";
        private const string c_twitchWebSocketMessagedelimiter = "\r\n";

        private const string c_twitchBadges = "moderator/1";
        private const string c_twitchDisplayName = "SmoothGPT";
        private const string c_twitchOAuthAccessCode = "3ajsbvh2psae7ryn3crkkcbi1oqfas";
        private const string c_twitchUserAccessScopes = "chat:read chat:edit";
        private const string c_twitchUserName = "smoothgpt";

        private static readonly Dictionary<TwitchChatAutomatedMessageType, string> c_automatedMessages = new()
		{
            { TwitchChatAutomatedMessageType.Commands,		  $"Check the Socials section below for a list of available bot commands @ https://www.twitch.tv/SmoothDagger/About" },
			{ TwitchChatAutomatedMessageType.Discord,		  $"Interested in chatting? Join the Discord @ https://www.discord.gg/SmoothCrew" },
			{ TwitchChatAutomatedMessageType.Rules,			  $"Make sure you're following the rules! Find them below in the rules section @ https://www.twitch.tv/SmoothDagger/About" },
            { TwitchChatAutomatedMessageType.Steam,			  $"Come play with us! Add me on Steam @ https://steamcommunity.com/id/SmoothDagger/" },
            { TwitchChatAutomatedMessageType.Supporter,       $"Are you a follower or subscriber? Check the Socials section below for exclusive chat commands @ https://www.twitch.tv/SmoothDagger/About" },
            { TwitchChatAutomatedMessageType.TwitchFollow,    $"Enjoying the stream? Tap the follow button to get notified for any live streams!" },
            { TwitchChatAutomatedMessageType.TwitchSubscribe, $"Want ad-free viewing? Subscribe on Twitch @ https://www.twitch.tv/subs/SmoothDagger" },
            { TwitchChatAutomatedMessageType.YouTube,		  $"Looking for more content? Subscribe on YouTube @ https://www.youtube.com/@SmoothDagger" },
		};
        private static readonly Dictionary<TwitchChatAutomatedMessageType, string> c_onScreenAutomatedMessages = new()
        {
            { TwitchChatAutomatedMessageType.Commands,        $"Check the Socials section below for a list of available chat commands @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/SmoothDagger/About")}" },
            { TwitchChatAutomatedMessageType.Discord,         $"Interested in chatting? Join the Discord @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.discord.gg/SmoothCrew")}" },
            { TwitchChatAutomatedMessageType.Rules,           $"Make sure you're following the rules! Find them below in the rules section @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/SmoothDagger/About")}" },
            { TwitchChatAutomatedMessageType.Steam,           $"Come play with us! Add me on Steam @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://steamcommunity.com/id/SmoothDagger/")}" },
            { TwitchChatAutomatedMessageType.Supporter,       $"Are you a follower or subscriber? Check the Socials section below for exclusive chat commands @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/SmoothDagger/About")}" },
            { TwitchChatAutomatedMessageType.TwitchFollow,    $"Enjoying the stream? {TwitchChatColorCodes.ConvertToLinkMessage(message: "Tap the follow button to get notified for any live streams")}!" },
            { TwitchChatAutomatedMessageType.TwitchSubscribe, $"Want ad-free viewing? Subscribe on Twitch @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/subs/SmoothDagger")}" },
            { TwitchChatAutomatedMessageType.YouTube,         $"Looking for more content? Subscribe on YouTube @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.youtube.com/@SmoothDagger")}" },
        };
		private static readonly Dictionary<TwitchChatAutomatedMessageType, string> c_twitchChatCommandMessages = new()
		{
            { TwitchChatAutomatedMessageType.Commands,	      $"check the Socials section below for a list of available bot commands @ https://www.twitch.tv/SmoothDagger/About" },
			{ TwitchChatAutomatedMessageType.Discord,		  $"interested in chatting? Join the Discord @ https://www.discord.gg/SmoothCrew" },
			{ TwitchChatAutomatedMessageType.Rules,		      $"make sure you're following the rules! Find them below in the rules section @ https://www.twitch.tv/SmoothDagger/About" },
            { TwitchChatAutomatedMessageType.Steam,		      $"come play with us! Add me on Steam @ https://steamcommunity.com/id/SmoothDagger/" },
            { TwitchChatAutomatedMessageType.YouTube,		  $"looking for more content? Subscribe on YouTube @ https://www.youtube.com/@SmoothDagger" },
		};
        private static readonly Dictionary<TwitchChatAutomatedMessageType, string> c_onScreenTwitchChatCommandMessages = new()
        {
            { TwitchChatAutomatedMessageType.Commands,        $"check the Socials section below for a list of available chat commands @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/SmoothDagger/About")}" },
            { TwitchChatAutomatedMessageType.Discord,         $"interested in chatting? Join the Discord @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.discord.gg/SmoothCrew")}" },
            { TwitchChatAutomatedMessageType.Rules,           $"make sure you're following the rules! Find them below in the rules section @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/SmoothDagger/About")}" },
            { TwitchChatAutomatedMessageType.Steam,           $"come play with us! Add me on Steam @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://steamcommunity.com/id/SmoothDagger/")}" },
            { TwitchChatAutomatedMessageType.YouTube,         $"looking for more content? Subscribe on YouTube @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.youtube.com/@SmoothDagger")}" },
        };

        private static readonly Dictionary<ColorType, string> c_colorTypesAsStrings = new()
        {
            { ColorType.Red,       $"{nameof(ColorType.Red).ToLower()}"       },
            { ColorType.Orange,    $"{nameof(ColorType.Orange).ToLower()}"    },
            { ColorType.Yellow,    $"{nameof(ColorType.Yellow).ToLower()}"    },
            { ColorType.Lime,      $"{nameof(ColorType.Lime).ToLower()}"      },
            { ColorType.Green,     $"{nameof(ColorType.Green).ToLower()}"     },
            { ColorType.Turquoise, $"{nameof(ColorType.Turquoise).ToLower()}" },
            { ColorType.Cyan,      $"{nameof(ColorType.Cyan).ToLower()}"      },
            { ColorType.Teal,      $"{nameof(ColorType.Teal).ToLower()}"      },
            { ColorType.Blue,      $"{nameof(ColorType.Blue).ToLower()}"      },
            { ColorType.Purple,    $"{nameof(ColorType.Purple).ToLower()}"    },
            { ColorType.Magenta,   $"{nameof(ColorType.Magenta).ToLower()}"   },
            { ColorType.Pink,      $"{nameof(ColorType.Pink).ToLower()}"      },
            { ColorType.White,     $"{nameof(ColorType.White).ToLower()}"     },
            { ColorType.Rainbow,   $"{nameof(ColorType.Rainbow).ToLower()}"   },
        };
        private static readonly Dictionary<string, ColorType> c_colorStringsAsTypes = new()
        {
            { c_colorTypesAsStrings[ key: ColorType.Red       ], ColorType.Red       },
            { c_colorTypesAsStrings[ key: ColorType.Orange    ], ColorType.Orange    },
            { c_colorTypesAsStrings[ key: ColorType.Yellow    ], ColorType.Yellow    },
            { c_colorTypesAsStrings[ key: ColorType.Lime      ], ColorType.Lime      },
            { c_colorTypesAsStrings[ key: ColorType.Green     ], ColorType.Green     },
            { c_colorTypesAsStrings[ key: ColorType.Turquoise ], ColorType.Turquoise },
            { c_colorTypesAsStrings[ key: ColorType.Cyan      ], ColorType.Cyan      },
            { c_colorTypesAsStrings[ key: ColorType.Teal      ], ColorType.Teal      },
            { c_colorTypesAsStrings[ key: ColorType.Blue      ], ColorType.Blue      },
            { c_colorTypesAsStrings[ key: ColorType.Purple    ], ColorType.Purple    },
            { c_colorTypesAsStrings[ key: ColorType.Magenta   ], ColorType.Magenta   },
            { c_colorTypesAsStrings[ key: ColorType.Pink      ], ColorType.Pink      },
            { c_colorTypesAsStrings[ key: ColorType.White     ], ColorType.White     },
            { c_colorTypesAsStrings[ key: ColorType.Rainbow   ], ColorType.Rainbow   },
        };

		private static readonly Dictionary<UILayoutType, string> c_uiLayoutTypesAsStrings = new()
		{
			{ UILayoutType.Code,    $"{nameof(UILayoutType.Code).ToLower()}"    },
			{ UILayoutType.Default, $"{nameof(UILayoutType.Default).ToLower()}" },
			{ UILayoutType.MTG,     $"{nameof(UILayoutType.MTG).ToLower()}"     },
            { UILayoutType.TF2,     $"{nameof(UILayoutType.TF2).ToLower()}"     },
		};
		private static readonly Dictionary<string, UILayoutType> c_uiLayoutStringsAsTypes = new()
		{
			{ c_uiLayoutTypesAsStrings[ key: UILayoutType.Code    ], UILayoutType.Code    },
			{ c_uiLayoutTypesAsStrings[ key: UILayoutType.Default ], UILayoutType.Default },
			{ c_uiLayoutTypesAsStrings[ key: UILayoutType.MTG     ], UILayoutType.MTG     },
            { c_uiLayoutTypesAsStrings[ key: UILayoutType.TF2     ], UILayoutType.TF2     },
		};

        private static readonly Dictionary<TwitchChatCommandType, string> c_commands = new()
		{
			// Bot
			{ TwitchChatCommandType.AccountAge,   "!accountage"	 },
            { TwitchChatCommandType.Commands,     "!commands"	 },
            { TwitchChatCommandType.CS,		      "!cs"			 },
            { TwitchChatCommandType.Current,	  "!current"	 },
            { TwitchChatCommandType.CurrentSong,  "!currentsong" },
            { TwitchChatCommandType.Date,         "!date"		 },
            { TwitchChatCommandType.Discord,      "!discord"	 },
            { TwitchChatCommandType.FollowAge,    "!followage"	 },
            { TwitchChatCommandType.Layout,       "!layout"	     },
            { TwitchChatCommandType.Lurk,		  "!lurk"		 },
            { TwitchChatCommandType.Queue,	      "!queue"		 },
            { TwitchChatCommandType.Rules,        "!rules"		 },
            { TwitchChatCommandType.SetColor,     "!setcolor"	 },
            { TwitchChatCommandType.SetColour,    "!setcolour"	 },
            { TwitchChatCommandType.SetLayout,    "!setlayout"	 },
            { TwitchChatCommandType.Skip,		  "!skip"		 },
            { TwitchChatCommandType.Song,		  "!song"		 },
            { TwitchChatCommandType.SongQueue,    "!songqueue"	 },
            { TwitchChatCommandType.SongRequest,  "!songrequest" },
            { TwitchChatCommandType.SongSkip,	  "!songskip"	 },
            { TwitchChatCommandType.Specs,		  "!specs"		 },
            { TwitchChatCommandType.SR,			  "!sr"			 },
            { TwitchChatCommandType.Steam,        "!steam"		 },
            { TwitchChatCommandType.TextToSpeech, "!tts"		 },
            { TwitchChatCommandType.Time,         "!time"		 },
            { TwitchChatCommandType.YouTube,      "!youtube"	 },
        };
        private static readonly Dictionary<TwitchChatCommandInfoMessageType, string> c_commandInfoMessages = new()
        {
            {
				TwitchChatCommandInfoMessageType.SetColorSelection,
				$"Available Colors: " +
					$"{c_colorTypesAsStrings[ key: ColorType.Red	   ]}, " +
					$"{c_colorTypesAsStrings[ key: ColorType.Orange	   ]}, " +
					$"{c_colorTypesAsStrings[ key: ColorType.Yellow	   ]}, " +
					$"{c_colorTypesAsStrings[ key: ColorType.Lime	   ]}, " +
					$"{c_colorTypesAsStrings[ key: ColorType.Green	   ]}, " +
					$"{c_colorTypesAsStrings[ key: ColorType.Turquoise ]}, " +
					$"{c_colorTypesAsStrings[ key: ColorType.Cyan	   ]}, " +
					$"{c_colorTypesAsStrings[ key: ColorType.Teal	   ]}, " +
					$"{c_colorTypesAsStrings[ key: ColorType.Blue	   ]}, " +
					$"{c_colorTypesAsStrings[ key: ColorType.Purple	   ]}, " +
					$"{c_colorTypesAsStrings[ key: ColorType.Magenta   ]}, " +
					$"{c_colorTypesAsStrings[ key: ColorType.Pink	   ]}, " +
					$"{c_colorTypesAsStrings[ key: ColorType.White	   ]}, " +
					$"{c_colorTypesAsStrings[ key: ColorType.Rainbow   ]}."
			},
            {
				TwitchChatCommandInfoMessageType.SetColorUsage,
				$"Type {c_commands[key: TwitchChatCommandType.SetColor]} or {c_commands[key: TwitchChatCommandType.SetColour]} followed by a valid color option, such as red or pink, " +
				$"e.g., {c_commands[key: TwitchChatCommandType.SetColor]} {c_colorTypesAsStrings[key: ColorType.Red]} or {c_commands[key: TwitchChatCommandType.SetColour]} {c_colorTypesAsStrings[key: ColorType.Pink]}, " +
				$"to set the color of your text on screen."
			},
            {
				TwitchChatCommandInfoMessageType.TextToSpeech,
				$"Type {c_commands[key: TwitchChatCommandType.TextToSpeech]} followed by the text you'd like to hear on stream, e.g., !tts this is a test message."
			},
        };
        private static readonly Dictionary<TwitchChatCommandInfoMessageType, string> c_onScreenCommandInfoMessages = new()
        {
            {
				TwitchChatCommandInfoMessageType.SetColorSelection,
				$"Available Colors: " +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Red		  )}]{c_colorTypesAsStrings[ key: ColorType.Red		  ]}[/color], " +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Orange	  )}]{c_colorTypesAsStrings[ key: ColorType.Orange	  ]}[/color], " +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Yellow	  )}]{c_colorTypesAsStrings[ key: ColorType.Yellow	  ]}[/color], " +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Lime	  )}]{c_colorTypesAsStrings[ key: ColorType.Lime	  ]}[/color], " +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Green	  )}]{c_colorTypesAsStrings[ key: ColorType.Green	  ]}[/color], " +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Turquoise )}]{c_colorTypesAsStrings[ key: ColorType.Turquoise ]}[/color], " +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Cyan	  )}]{c_colorTypesAsStrings[ key: ColorType.Cyan	  ]}[/color], " +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Teal	  )}]{c_colorTypesAsStrings[ key: ColorType.Teal	  ]}[/color], " +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Blue	  )}]{c_colorTypesAsStrings[ key: ColorType.Blue	  ]}[/color], " +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Purple	  )}]{c_colorTypesAsStrings[ key: ColorType.Purple	  ]}[/color], " +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Magenta	  )}]{c_colorTypesAsStrings[ key: ColorType.Magenta   ]}[/color], " +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Pink	  )}]{c_colorTypesAsStrings[ key: ColorType.Pink	  ]}[/color], " +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.White	  )}]{c_colorTypesAsStrings[ key: ColorType.White	  ]}[/color], " +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Red       )}]R[/color]" +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Orange    )}]a[/color]" +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Yellow    )}]i[/color]" +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Green     )}]n[/color]" +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Cyan      )}]b[/color]" +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Blue      )}]o[/color]" +
					$"[color={PastelInterpolator.GetColorAsHexByColorType( colorType: ColorType.Purple    )}]w[/color]" +
                    $"."
			},
            {
				TwitchChatCommandInfoMessageType.SetColorUsage,
				$"Type {c_commands[key: TwitchChatCommandType.SetColor]} or {c_commands[key: TwitchChatCommandType.SetColour]} followed by a valid color option, such as red or pink, " +
				$"e.g., {c_commands[key: TwitchChatCommandType.SetColor]} {c_colorTypesAsStrings[key: ColorType.Red]} or {c_commands[key: TwitchChatCommandType.SetColour]} {c_colorTypesAsStrings[key: ColorType.Pink]}, " +
				$"to set the color of your text on screen."
			},
            {
				TwitchChatCommandInfoMessageType.TextToSpeech,
                $"Type {c_commands[key: TwitchChatCommandType.TextToSpeech]} followed by the text you'd like to hear on stream, e.g., !tts this is a test message."
            },
        };

        private static readonly string c_setColorRegexPattern = 
			$"^(" +
			$"{c_commands[ key: TwitchChatCommandType.SetColor  ]}|" +
			$"{c_commands[ key: TwitchChatCommandType.SetColour ]}" +
            $") (" +
            $"{c_colorTypesAsStrings[ key: ColorType.Red	   ]}|" +
            $"{c_colorTypesAsStrings[ key: ColorType.Orange	   ]}|" +
            $"{c_colorTypesAsStrings[ key: ColorType.Yellow	   ]}|" +
            $"{c_colorTypesAsStrings[ key: ColorType.Lime	   ]}|" +
            $"{c_colorTypesAsStrings[ key: ColorType.Green	   ]}|" +
            $"{c_colorTypesAsStrings[ key: ColorType.Turquoise ]}|" +
            $"{c_colorTypesAsStrings[ key: ColorType.Cyan	   ]}|" +
            $"{c_colorTypesAsStrings[ key: ColorType.Teal	   ]}|" +
            $"{c_colorTypesAsStrings[ key: ColorType.Blue	   ]}|" +
            $"{c_colorTypesAsStrings[ key: ColorType.Purple	   ]}|" +
            $"{c_colorTypesAsStrings[ key: ColorType.Magenta   ]}|" +
            $"{c_colorTypesAsStrings[ key: ColorType.Pink	   ]}|" +
            $"{c_colorTypesAsStrings[ key: ColorType.White	   ]}|" +
			$"{c_colorTypesAsStrings[ key: ColorType.Rainbow   ]})$";

		private static readonly string c_setLayoutRegexPattern =
			$"^(" +
			$"{c_commands[ key: TwitchChatCommandType.Layout    ]}|" +
			$"{c_commands[ key: TwitchChatCommandType.SetLayout ]}" +
			$") (" +
			$"{c_uiLayoutTypesAsStrings[ key: UILayoutType.Code    ]}|" +
			$"{c_uiLayoutTypesAsStrings[ key: UILayoutType.Default ]}|" +
            $"{c_uiLayoutTypesAsStrings[ key: UILayoutType.MTG     ]}|" +
            $"{c_uiLayoutTypesAsStrings[ key: UILayoutType.TF2     ]})$";

        private readonly Dictionary<string, string> m_userNameColors = new();
		private readonly HashSet<string> m_usersLurking = new();
        private readonly Queue<ulong> m_messageTimestamps = new();
        private readonly ClientWebSocket m_webSocket = new();

        private AudioManager m_audioManager = null;
		private HttpManager m_httpManager = null;
		private PastelInterpolator m_pastelInterpolator = null;
		private SpotifyManager m_spotifyManager = null;
        private TwitchBotAccessToken m_twitchBotAccessToken = null;
        private TwitchChannelPointRewardsManager m_twitchChannelPointRewardsManager = null;
		private TwitchChatManager m_twitchChatManager = null;
        private TwitchGlobalData m_twitchGlobalData = null;
        private TwitchManager m_twitchManager = null;
		private UIManager m_uiManager = null;
		private TwitchChatAutomatedMessageType m_currentAutomatedMessageType =
            (TwitchChatAutomatedMessageType)(GD.Randi() % Enum.GetValues<TwitchChatAutomatedMessageType>().Length);
		private bool m_shutdown = false;

        private void AddBotChatMessage(
			string message
		)
		{
			_ = Task.Run(
				function:
				async () =>
				{
					await Task.Delay(
						millisecondsDelay: 5
					);
					m_twitchChatManager.AddTwitchChatMessage(
						userName: c_twitchUserName,
					    name: c_twitchDisplayName,
						nameColor: string.Empty,
						message: message,
						emotes: string.Empty,
						badges: c_twitchBadges,
						isSmoothGPT: true
					);
				}
			);
		}

		private async void ConnectWebSocket()
		{
			// connect to Twitch IRC web socket
			var uri = new Uri(
                uriString: c_twitchWebSocketAddress
			);

			await m_webSocket.ConnectAsync(
                uri: uri,
                cancellationToken: default
			);

			await SendWebSocketMessage(
                message: $"CAP REQ :twitch.tv/commands twitch.tv/tags"
			);
			await SendWebSocketMessage(
                message: $"PASS oauth:{m_twitchBotAccessToken.AccessToken}"
			);
			await SendWebSocketMessage(
                message: $"NICK {m_twitchGlobalData.BotUserName}"
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
                webSocketMessages: out List<TwitchChatWebSocketMessage> webSocketMessages
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
                message: $"JOIN #{m_twitchGlobalData.TwitchChannel}"
			);

			StartWebSocketMessageReader();
			StartWebSocketAutomatedMessageDispatcher();
		}

        private string GetTwitchChannelMessage(
			string twitchChatMessageId,
			string message
		)
		{
			var replyMessageTarget = 
				string.IsNullOrEmpty(
					value: twitchChatMessageId
				) is false ?
					$"@reply-parent-msg-id={twitchChatMessageId} " :
					string.Empty;

            return $"{replyMessageTarget}PRIVMSG #{m_twitchGlobalData.TwitchChannel} :{message}";
        }

		private static string GetTwitchUserName(
            TwitchChatWebSocketMessage webSocketMessage
        )
		{
            var userName = webSocketMessage.UserName;
            var displayName = webSocketMessage.Tags[key: "display-name"];
            var normalizedDisplayName = displayName.ToLower();
            if (
                normalizedDisplayName.Equals(
                    obj: userName
                ) is false
            )
            {
                displayName += $" ({userName})";
            }

			return displayName;
        }

        private void HandleApplicationCommandOverlay(
			TwitchChatCommandType commandType,
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
            switch (commandType)
            {
                case TwitchChatCommandType.AccountAge:
                    HandleWebSocketMessagePrivMsgAccountAge(
                        webSocketMessage: webSocketMessage
                    );
                    break;

				case TwitchChatCommandType.CS:
				case TwitchChatCommandType.Current:
                case TwitchChatCommandType.CurrentSong:
				case TwitchChatCommandType.Song:
                    HandleWebSocketMessagePrivMsgSongCurrent(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case TwitchChatCommandType.FollowAge:
                    HandleWebSocketMessagePrivMsgFollowAge(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case TwitchChatCommandType.Commands:
                    HandleWebSocketMessagePrivMsgCommands(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case TwitchChatCommandType.Date:
                    HandleWebSocketMessagePrivMsgDate(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case TwitchChatCommandType.Discord:
                    HandleWebSocketMessagePrivMsgDiscord(
                        webSocketMessage: webSocketMessage
                    );
                    break;

				case TwitchChatCommandType.Layout:
                case TwitchChatCommandType.SetLayout:
                    HandleWebSocketMessagePrivMsgLayout(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case TwitchChatCommandType.Lurk:
                    HandleWebSocketMessagePrivMsgLurk(
                        webSocketMessage: webSocketMessage
                    );
                    break;

				case TwitchChatCommandType.Queue:
				case TwitchChatCommandType.SongQueue:
				    HandleWebSocketMessagePrivMsgSongQueue(
                        webSocketMessage: webSocketMessage
                    );
					break;

                case TwitchChatCommandType.Rules:
                    HandleWebSocketMessagePrivMsgRules(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case TwitchChatCommandType.SetColor:
                case TwitchChatCommandType.SetColour:
                    HandleWebSocketMessagePrivMsgSetColor(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case TwitchChatCommandType.Skip:
                case TwitchChatCommandType.SongSkip:
                    HandleWebSocketMessagePrivMsgSongSkip(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case TwitchChatCommandType.SongRequest:
				case TwitchChatCommandType.SR:
                    HandleWebSocketMessagePrivMsgSongRequest(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case TwitchChatCommandType.Specs:
                    HandleWebSocketMessagePrivMsgSpecs(
                        webSocketMessage: webSocketMessage
                    );
					break;

                case TwitchChatCommandType.Steam:
                    HandleWebSocketMessagePrivMsgSteam(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case TwitchChatCommandType.TextToSpeech:
                    HandleWebSocketMessagePrivMsgTextToSpeech(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case TwitchChatCommandType.Time:
                    HandleWebSocketMessagePrivMsgTime(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case TwitchChatCommandType.YouTube:
                    HandleWebSocketMessagePrivMsgYouTube(
						webSocketMessage: webSocketMessage
                    );
                    break;

                default:
                    break;
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

            var userName = $"{@event.ChatterUserName}";
            var bitsBadgeTierText = $" Thank you so much for the {totalBits} bits! Congratulations on achieving the {bitsBadgeTier.Tier} bit tier!";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchGlobalData.TwitchChannel} :{userName}{bitsBadgeTierText}"
			);
		}

		private async void HandleChannelChatNotificationCharityDonation(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			// todo
			var charityDonation = @event.CharityDonation;
            var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
            var userName = isChatterAnonymous ? "Anonymous" : $"{@event.ChatterUserName}";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchGlobalData.TwitchChannel} :{userName} This event was not set up yet. Shame SmoothDagger for being lazy! SHAME HIM"
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
                s: communitySubGift.SubTier[index: 0].ToString()
			);
			var userName = isChatterAnonymous ? "Anonymous" : $"{@event.ChatterUserName}";
			var communitySubGiftText = $" Thank you so much for the {communitySubGiftTotal} tier {communitySubGiftTier} gifted community sub{(communitySubGiftTotal > 1u ? "s" : string.Empty)}!";
			var communitySubGiftCumulativeText = $" {userName} has gifted a total of {communitySubGiftCumulativeTotal} community sub{(communitySubGiftCumulativeTotal > 1u ? "s" : string.Empty)}!";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchGlobalData.TwitchChannel} :{userName}{communitySubGiftText}{communitySubGiftCumulativeText}"
			);
		}

		private async void HandleChannelChatNotificationGiftPaidUpgrade(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			// todo
			var giftPaidUpgrade = @event.GiftPaidUpgrade;
			var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
            var userName = isChatterAnonymous ? "Anonymous" : $"{@event.ChatterUserName}";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchGlobalData.TwitchChannel} :{userName} This event was not set up yet. Shame SmoothDagger for being lazy! SHAME HIM"
			);
		}

		private async void HandleChannelChatNotificationPayItForward(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			// todo
			var payItForward = @event.PayItForward;
            var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
            var userName = isChatterAnonymous ? "Anonymous" : $"{@event.ChatterUserName}";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchGlobalData.TwitchChannel} :{userName} This event was not set up yet. Shame SmoothDagger for being lazy! SHAME HIM"
			);
		}

		private async void HandleChannelChatNotificationPrimePaidUpgrade(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			// todo
			var primePaidUpgrade = @event.PrimePaidUpgrade;
            var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
            var userName = isChatterAnonymous ? "Anonymous" : $"{@event.ChatterUserName}";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchGlobalData.TwitchChannel} :{userName} This event was not set up yet. Shame SmoothDagger for being lazy! SHAME HIM"
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
                s: resub.SubTier[index: 0].ToString()
			);
            var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
			var userName = isChatterAnonymous ? "Anonymous" : $"{@event.ChatterUserName}";
			var userNameGifter = isGifterAnonymous ? "Anonymous" : $"{resub.GifterUserName}";
            var isResubPrime = resub.IsPrime ?? false;
            var resubText = isResubPrime ?
				$" Thank you so much for the prime resub!" :
				$" Thank you so much for the tier {resubTier} {resubDuration} month {(isResubGift ? "gifted" : string.Empty)} resub!";
			var resubGiftText = isResubGift ? $" Shoutout to {userNameGifter} for the gifted resub!" : string.Empty;
			var resubMonthsText = $" {userName} has been subbed for a total of {resubCumulativeMonths} months{(resubStreakMonths > 1u ? $" & is on a {resubStreakMonths} month sub streak!" : "!")}";
			var resubCommandsText = " Make sure to check out the available sub commands in the Social section below for your sub benefits @ https://www.twitch.tv/smoothdagger/about";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchGlobalData.TwitchChannel} :{userName}{resubText}{resubGiftText}{resubMonthsText}{resubCommandsText}"
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
                s: sub.SubTier[index: 0].ToString()
			);
			var userName = isChatterAnonymous ? "Anonymous" : $"{@event.ChatterUserName}";
			var isPrimeSub = sub.IsPrime ?? false;
			var subText = isPrimeSub ?
				$" Thank you so much for the prime sub!" :
				$" Thank you so much for the tier {subTier} {subDuration} month sub!";
			var subCommandsText = " Make sure to check out the available sub commands in the Social section below for your sub benefits @ https://www.twitch.tv/smoothdagger/about";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchGlobalData.TwitchChannel} :{userName}{subText}{subCommandsText}"
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
				subGift.SubTier[index: 0].ToString()
			);
			var userName = isChatterAnonymous ? "Anonymous" : $"{@event.ChatterUserName}";
			var userNameRecipient = $"{subGift.RecipientUserName}";
			var subGiftText = $" Thank you so much for the tier {subGiftTier} {subGiftDuration} month gifted sub to {userNameRecipient}!";
			var subGiftMonthsText = isChatterAnonymous ? string.Empty : $"{userName} has gifted a total of {subGiftCumulativeTotal} sub{(subGiftCumulativeTotal > 1u ? "s" : string.Empty)}!";
			await SendWebSocketMessage(
				message: $"PRIVMSG #{m_twitchGlobalData.TwitchChannel} :{userName}{subGiftText}{subGiftMonthsText}"
			);
		}

		private void HandleInvalidChatCommand(
			TwitchChatWebSocketMessage webSocketMessage
        )
		{
			var twitchUserName = GetTwitchUserName(
				webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message =
				$"{twitchUserName}, " +
				$"that is not a valid chat command. " +
				$"Check the Social section below to see the available bot commands @ " +
				$"\nhttps://www.twitch.tv/SmoothDagger/About";
			var onScreenMessage =
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
				$"{TwitchChatColorCodes.ConvertToErrorMessage(message: "that is not a valid chat command")}. " +
                $"Check the Social section below to see the available bot commands @ " +
                $"\n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/SmoothDagger/About")}";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

		private void HandleUserLurkingState(
			TwitchChatWebSocketMessage webSocketMessage
        )
        {
            var userName = webSocketMessage.UserName;
            if (
				m_usersLurking.Contains(
					userName
				) is true
			)
            {
				var twitchUserName = GetTwitchUserName(
				    webSocketMessage: webSocketMessage
				);
				var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
				var twitchMessage = webSocketMessage.Text;
				var twitchMessageTrimmed = twitchMessage.Remove(startIndex: twitchMessage.Length - 2, count: 2);
				var twitchMessageNormalized = twitchMessageTrimmed.ToLower();

                if (
                    twitchMessageNormalized.Equals(
						c_commands[key: TwitchChatCommandType.Lurk]
					) is false
				)
				{
                    var message = $"{twitchUserName} disengaged lurk mode!";
                    var onScreenMessage = $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)} disengaged lurk mode!";
                    SendTwitchChatMessages(
                        twitchChatMessageId: twitchChatMessageId,
                        message: message,
                        onScreenMessage: onScreenMessage
                    );

                    m_usersLurking.Remove(
                        item: userName
                    );
                }
            }
        }

		private void HandleUserNotFollowingMessage(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
			var twitchUserName = GetTwitchUserName(
				webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message = $"{twitchUserName}, you are currently not following SmoothDagger. Tap the FOLLOW button to gain access to this command!";
			var onScreenMessage = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
				$"{TwitchChatColorCodes.ConvertToErrorMessage(message: "you are currently not following ")}" +
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: "SmoothDagger")}. " +
				$"Tap the {TwitchChatColorCodes.ConvertToSuccessMessage(message: "FOLLOW")} " +
				$"button to gain access to this command!";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

		private void HandleUserPrivilegeError(
			TwitchChatWebSocketMessage webSocketMessage
        )
        {
			var twitchUserName = GetTwitchUserName(
				webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message = $"{twitchUserName}, you do not have privileges to use this command.";
			var onScreenMessage = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
				$"{TwitchChatColorCodes.ConvertToErrorMessage(message: " you do not have privileges to use this command")}.";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
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
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
			await SendWebSocketMessage(
                message: $"PONG :{webSocketMessage.Text}"
			);
		}

		private void HandleWebSocketMessagePrivMsg(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
			if (m_messageTimestamps.Count is c_minimumMessageCount)
			{
				m_messageTimestamps.Dequeue();
			}
			m_messageTimestamps.Enqueue(
                item: Time.GetTicksMsec()
			);

			HandleUserLurkingState(
				webSocketMessage: webSocketMessage	
			);
			
			var chatCommandValidityType = ProcessChatCommand(
                webSocketMessage: webSocketMessage
			);
			ProcessChatMessage(
                webSocketMessage: webSocketMessage
			);

			switch (chatCommandValidityType)
			{
				case TwitchChatCommandValidityType.InvalidChatCommand:
					HandleInvalidChatCommand(
						webSocketMessage: webSocketMessage	
					);
					break;

				case TwitchChatCommandValidityType.ValidChatCommand:
				case TwitchChatCommandValidityType.NotAChatCommand:
				default:
					break;
			}
		}

		private void HandleWebSocketMessagePrivMsgAccountAge(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
            var user = m_twitchManager.GetUser(
				userName: webSocketMessage.UserName
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
					accountTime += accountTime.Equals(
                        value: string.Empty
                    ) ? string.Empty : " ";
					accountTime += $"{dateLength.Month} month{(dateLength.Month > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Day > 0u)
				{
					accountTime += accountTime.Equals(
                        value: string.Empty
                    ) ? string.Empty : " ";
					accountTime += $"{dateLength.Day} day{(dateLength.Day > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Hour > 0u)
				{
					accountTime += accountTime.Equals(
                        value: string.Empty
                    ) ? string.Empty : " ";
					accountTime += $"{dateLength.Hour} hour{(dateLength.Hour > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Minute > 0u)
				{
					accountTime += accountTime.Equals(
                        value: string.Empty
                    ) ? string.Empty : " ";
					accountTime += $"{dateLength.Minute} minute{(dateLength.Minute > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Second > 0u)
				{
					accountTime += accountTime.Equals(
						value: string.Empty
					) ? string.Empty : " ";
					accountTime += $"{dateLength.Second} second{(dateLength.Second > 1u ? "s" : string.Empty)}";
				}

                var twitchUserName = GetTwitchUserName(
                    webSocketMessage: webSocketMessage
                );
                var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
                var message = $"{twitchUserName}, you've been lurking in the depths of Twitch for {accountTime}! Thanks for being here!";
                var onScreenMessage =
                    $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
                    $"you've been lurking in the depths of {TwitchChatColorCodes.ConvertToLinkMessage(message: "Twitch")} " +
                    $"for {TwitchChatColorCodes.ConvertToSuccessMessage(message: accountTime)}! " +
                    $"Thanks for being here!";

				SendTwitchChatMessages(
				    twitchChatMessageId: twitchChatMessageId,
				    message: message,
				    onScreenMessage: onScreenMessage
				);
            }
			else
			{
				HandleUserNotFollowingMessage(
					webSocketMessage: webSocketMessage	
				);
            }
		}

		private void HandleWebSocketMessagePrivMsgCommands(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
			var twitchUserName = GetTwitchUserName(
				webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message = $"{twitchUserName}, {c_twitchChatCommandMessages[key: TwitchChatAutomatedMessageType.Commands]}";
			var onScreenMessage = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
				$"{c_onScreenTwitchChatCommandMessages[key: TwitchChatAutomatedMessageType.Commands]}";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
		}

		private void HandleWebSocketMessagePrivMsgDate(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
            var twitchUserName = GetTwitchUserName(
			    webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];

            var text = webSocketMessage.Text;
			text = text.Replace(
				oldValue: c_commands[key: TwitchChatCommandType.Date],
				newValue: string.Empty
			);
			text = text.Replace(
				oldValue: "\r\n",
				newValue: string.Empty
			);
			if (
				string.IsNullOrEmpty(
					value: text
				) is true
			)
			{
				var dateTime = DateTime.UtcNow;
				var message = $"{twitchUserName}, the current date in UTC is {dateTime:d-MMM-yyyy}.";
				message = message.Replace(
					oldChar: '-',
					newChar: ' '
				);
                var onScreenMessage =
                    $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
                    $"the current date in " +
                    $"{TwitchChatColorCodes.ConvertToSuccessMessage(message: "UTC")} is " +
                    $"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"{dateTime:d-MMM-yyyy}")}.";
                onScreenMessage = onScreenMessage.Replace(
                    oldChar: '-',
                    newChar: ' '
                );

                SendTwitchChatMessages(
                    twitchChatMessageId: twitchChatMessageId,
                    message: message,
                    onScreenMessage: onScreenMessage
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
				var message = $"{twitchUserName}, the current date in {text} is {dateTime:d-MMM-yyyy}.";
				message = message.Replace(
					oldChar: '-',
					newChar: ' '
				);
                var onScreenMessage =
                    $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
                    $"the current date in " +
					$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: text)} is " +
					$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"{dateTime:d-MMM-yyyy}")}.";
                onScreenMessage = onScreenMessage.Replace(
					oldChar: '-',
					newChar: ' '
				);

                SendTwitchChatMessages(
                    twitchChatMessageId: twitchChatMessageId,
                    message: message,
                    onScreenMessage: onScreenMessage
                );
            }
		}

		private void HandleWebSocketMessagePrivMsgDiscord(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
			var twitchUserName = GetTwitchUserName(
				webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message = $"{twitchUserName}, {c_twitchChatCommandMessages[key: TwitchChatAutomatedMessageType.Discord]}";
			var onScreenMessage = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
				$"{c_onScreenTwitchChatCommandMessages[key: TwitchChatAutomatedMessageType.Discord]}";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
		}

		private void HandleWebSocketMessagePrivMsgFollowAge(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
			var userName = webSocketMessage.UserName;
			var userNameAdjusted = userName.ToLower();
			var channelFollowers = m_twitchManager.GetChannelFollowers();
			if (
				channelFollowers.ContainsKey(
					key: userNameAdjusted
                ) is true
			)
			{
				var channelFollower = channelFollowers[key: userNameAdjusted];
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
					followTime += followTime.Equals(
                        value: string.Empty
                    ) ? string.Empty : " ";
					followTime += $"{dateLength.Month} month{(dateLength.Month > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Day > 0u)
				{
					followTime += followTime.Equals(
                        value: string.Empty
                    ) ? string.Empty : " ";
					followTime += $"{dateLength.Day} day{(dateLength.Day > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Hour > 0u)
				{
					followTime += followTime.Equals(
                        value: string.Empty
                    ) ? string.Empty : " ";
					followTime += $"{dateLength.Hour} hour{(dateLength.Hour > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Minute > 0u)
				{
					followTime += followTime.Equals(
                        value: string.Empty
                    ) ? string.Empty : " ";
					followTime += $"{dateLength.Minute} minute{(dateLength.Minute > 1u ? "s" : string.Empty)}";
				}
				if (dateLength.Second > 0u)
				{
					followTime += followTime.Equals(
						value: string.Empty
					) ? string.Empty : " ";
					followTime += $"{dateLength.Second} second{(dateLength.Second > 1u ? "s" : string.Empty)}";
				}

				var twitchUserName = GetTwitchUserName(
				    webSocketMessage: webSocketMessage
				);
				var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
                var message = $"{twitchUserName}, you've been following SmoothDagger for {followTime}! Thanks for following!";
                var onScreenMessage = 
					$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
					$"you've been following {TwitchChatColorCodes.ConvertToUserMessage(message: "SmoothDagger")} " +
					$"for {TwitchChatColorCodes.ConvertToSuccessMessage(message: followTime)}! " +
					$"Thanks for following!";

                SendTwitchChatMessages(
                    twitchChatMessageId: twitchChatMessageId,
                    message: message,
                    onScreenMessage: onScreenMessage
                );
			}
			else
			{
				HandleUserNotFollowingMessage(
				    webSocketMessage: webSocketMessage
				);
			}
        }

		private void HandleWebSocketMessagePrivMsgLayout(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
			var userName = webSocketMessage.UserName;
			if (
                userName.Equals(
                     value: m_twitchGlobalData.AccountUserName
                ) is true
            )
            {
                var text = webSocketMessage.Text;
                var trimmedText = text.Remove(
                    startIndex: text.Length - c_webSocketMessageDelimiterLength
                );
                var normalizedText = trimmedText.ToLower();
				var parsedText = normalizedText.Split(
					separator: ' '
				);
				var uiLayoutText = parsedText[1];
				var uiLayoutType = c_uiLayoutStringsAsTypes[key: uiLayoutText];
                m_uiManager.ChangeLayoutType(
					uiLayoutType: uiLayoutType
                );

                var twitchUserName = GetTwitchUserName(
				    webSocketMessage: webSocketMessage
				);
				var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
                var message =
                    $"{twitchUserName}, overlay layout set to " +
					$"{uiLayoutType} Mode.";
                var onScreenMessage =
                    $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, overlay layout set to " +
                    $"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"{uiLayoutType} Mode")}.";

                SendTwitchChatMessages(
                    twitchChatMessageId: twitchChatMessageId,
                    message: message,
                    onScreenMessage: onScreenMessage
                );
            }
			else
			{
				HandleUserPrivilegeError(
					webSocketMessage: webSocketMessage
				);
            }
        }

		private void HandleWebSocketMessagePrivMsgLurk(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
			var userName = webSocketMessage.UserName;
            var twitchUserName = GetTwitchUserName(
                webSocketMessage: webSocketMessage
            );
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];

            if (
				m_usersLurking.Contains(
					item: userName
				) is true
			)
			{
                var message =
                    $"{twitchUserName} tried to engage lurk mode, " +
                    $"but little did they know SmoothDagger knew they were already lurking! " +
                    $"Rekt.";
                var onScreenMessage =
                    $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)} tried to engage lurk mode, " +
                    $"but little did they know {TwitchChatColorCodes.ConvertToUserMessage(message: "SmoothDagger")} knew they were already lurking! " +
                    $"Rekt.";

                SendTwitchChatMessages(
                    twitchChatMessageId: twitchChatMessageId,
                    message: message,
                    onScreenMessage: onScreenMessage
                );
            }
			else
			{
                var message = $"{twitchUserName} engaged lurk mode!";
                var onScreenMessage = $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)} engaged lurk mode!";

                SendTwitchChatMessages(
                    twitchChatMessageId: twitchChatMessageId,
                    message: message,
                    onScreenMessage: onScreenMessage
                );

                m_usersLurking.Add(
                    item: userName
                );
            }
        }

		private void HandleWebSocketMessagePrivMsgRules(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
			var twitchUserName = GetTwitchUserName(
				webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message = $"{twitchUserName}, {c_twitchChatCommandMessages[key: TwitchChatAutomatedMessageType.Rules]}";
			var onScreenMessage = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
				$"{c_onScreenTwitchChatCommandMessages[key: TwitchChatAutomatedMessageType.Rules]}";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

		private async void HandleWebSocketMessagePrivMsgSetColor(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
			var userName = webSocketMessage.UserName;
            var twitchUserName = GetTwitchUserName(
                webSocketMessage: webSocketMessage
            );
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];

            var channelSubscribers = m_twitchManager.GetChannelSubscribers();
            if (
                channelSubscribers.ContainsKey(
					key: userName
                ) is false
            )
            {
                var message =$"{twitchUserName}, you must be subscribed in order to use this command";
                var onScreenMessage =
                    $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
                    $"{TwitchChatColorCodes.ConvertToErrorMessage(message: "you must be subscribed in order to use this command")}.";

                SendTwitchChatMessages(
                    twitchChatMessageId: twitchChatMessageId,
                    message: message,
                    onScreenMessage: onScreenMessage
                );
                return;
            }

			var text = webSocketMessage.Text;
            var trimmedText = text.Remove(
                startIndex: text.Length - c_webSocketMessageDelimiterLength
            );
            var normalizedText = trimmedText.ToLower();
            if (
                normalizedText.Equals(
					obj: c_commands[key: TwitchChatCommandType.SetColor]
				) is true ||
                normalizedText.Equals(
                    obj: c_commands[key: TwitchChatCommandType.SetColour]
                ) is true
            )
			{
				var messageUsage = $"{twitchUserName}, {c_commandInfoMessages[key: TwitchChatCommandInfoMessageType.SetColorUsage]}";
				var onScreenMessageUsage =
					$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
					$"{c_onScreenCommandInfoMessages[key: TwitchChatCommandInfoMessageType.SetColorUsage]}";

				SendTwitchChatMessages(
				    twitchChatMessageId: twitchChatMessageId,
				    message: messageUsage,
				    onScreenMessage: onScreenMessageUsage
				);

				await Task.Delay(
					millisecondsDelay: 1
				);

                var messageSelection = $"{twitchUserName}, {c_commandInfoMessages[key: TwitchChatCommandInfoMessageType.SetColorSelection]}";
                var onScreenMessageSelection =
                    $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
                    $"{c_onScreenCommandInfoMessages[key: TwitchChatCommandInfoMessageType.SetColorSelection]}";

                SendTwitchChatMessages(
                    twitchChatMessageId: twitchChatMessageId,
                    message: messageSelection,
                    onScreenMessage: onScreenMessageSelection
                );
                return;
            }

            var parsedText = normalizedText.Split(
				separator: ' '
			);
			var colorText = parsedText[1];
			var colorType = c_colorStringsAsTypes[key: colorText];
			var colorCode = PastelInterpolator.GetColorAsHexByColorType(
				colorType: colorType
			);

            var customUserData = m_twitchManager.GetCustomUserData(
				userName: userName
            );
			if (customUserData is null)
			{
				m_twitchManager.SetCustomUserData(
					userName: userName,
					userData: new()
					{
						CustomTextColor = colorCode
                    }
                );
			}
			else
			{
				customUserData.CustomTextColor = colorCode;
				m_twitchManager.SetCustomUserData(
					userName: userName,
					userData: customUserData
				);
			}
		}

		private void HandleWebSocketMessagePrivMsgSongCurrent(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
            var twitchUserName = GetTwitchUserName(
                webSocketMessage: webSocketMessage
            );
			var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            m_spotifyManager.QueueRequestCurrentTrack(
                twitchUserName: twitchUserName,
				twitchChatMessageId: twitchChatMessageId
            );
		}

		private void HandleWebSocketMessagePrivMsgSongQueue(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
            var twitchUserName = GetTwitchUserName(
			    webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            m_spotifyManager.QueueRequestUserTrackQueue(
				twitchUserName: twitchUserName,
                twitchChatMessageId: twitchChatMessageId
            );
        }

		private void HandleWebSocketMessagePrivMsgSongRequest(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
            var userName = webSocketMessage.UserName;
            if (
                userName.Equals(
                     value: m_twitchGlobalData.AccountUserName
                ) is true
            )
            {
				RequestSpotifyTrack(
					webSocketMessage: webSocketMessage	
				);
				return;
            }

            var userNameAdjusted = userName.ToLower();
            var channelSubscribers = m_twitchManager.GetChannelSubscribers();
            if (
                channelSubscribers.ContainsKey(
                    key: userNameAdjusted
                ) is false
            )
            {
                var twitchUserName = GetTwitchUserName(
                    webSocketMessage: webSocketMessage
                );
                var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
                var message = $"{twitchUserName}, you must be subscribed in order to use this command.";
                var onScreenMessage =
                    $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
                    $"{TwitchChatColorCodes.ConvertToErrorMessage(message: "you must be subscribed in order to use this command")}.";

                SendTwitchChatMessages(
                    twitchChatMessageId: twitchChatMessageId,
                    message: message,
                    onScreenMessage: onScreenMessage
                );
				return;
            }

			var utcNow = DateTime.UtcNow;
            var customSubscriberData = m_twitchManager.GetCustomUserData(
				userName: userName
            );
            if (customSubscriberData is null)
			{
				m_twitchManager.SetCustomUserData(
					userName: userName,
					userData: new()
					{
						TimeStampSongRequestIsAvailable = 
							$"{utcNow.AddHours(value: TwitchCustomUserData.TimeStampDelay):yyyy-MM-dd HH:mm:ss}",
                    }
                );

				RequestSpotifyTrack(
					webSocketMessage: webSocketMessage	
				);
			}
			else
			{
				var timeStampSongRequestAsString = customSubscriberData.TimeStampSongRequestIsAvailable;
				var isSongRequestAvailable = DateTime.Compare(
				    t1: DateTime.UtcNow,
				    t2: DateTime.Parse(
				        s: timeStampSongRequestAsString
                    )
				) >= 0;

                if (isSongRequestAvailable is true)
				{
                    customSubscriberData.TimeStampSongRequestIsAvailable =
						$"{utcNow.AddHours(value: TwitchCustomUserData.TimeStampDelay):yyyy-MM-dd HH:mm:ss}";
                    m_twitchManager.SetCustomUserData(
                        userName: userName,
                        userData: customSubscriberData
                    );

					RequestSpotifyTrack(
						webSocketMessage: webSocketMessage
					);
                }
                else
				{
                    var dateLength = DateCalculator.CalculateTimeDifference(
                        timeStart: $"{utcNow:yyyy-MM-dd HH:mm:ss}",
                        timeEnd: timeStampSongRequestAsString
                    );

                    var remainingTime = string.Empty;
                    if (dateLength.Month > 0u)
                    {
                        remainingTime += $"{dateLength.Month} month{(dateLength.Month > 1u ? "s" : string.Empty)}";
                    }
                    if (dateLength.Day > 0u)
                    {
                        remainingTime += remainingTime.Equals(
                            value: string.Empty
                        ) ? string.Empty : " ";
                        remainingTime += $"{dateLength.Day} day{(dateLength.Day > 1u ? "s" : string.Empty)}";
                    }
                    if (dateLength.Hour > 0u)
                    {
                        remainingTime += remainingTime.Equals(
                            value: string.Empty
                        ) ? string.Empty : " ";
                        remainingTime += $"{dateLength.Hour} hour{(dateLength.Hour > 1u ? "s" : string.Empty)}";
                    }
                    if (dateLength.Minute > 0u)
                    {
                        remainingTime += remainingTime.Equals(
                            value: string.Empty
                        ) ? string.Empty : " ";
                        remainingTime += $"{dateLength.Minute} minute{(dateLength.Minute > 1u ? "s" : string.Empty)}";
                    }
                    if (dateLength.Second > 0u)
                    {
                        remainingTime += remainingTime.Equals(
                            value: string.Empty
                        ) ? string.Empty : " ";
                        remainingTime += $"{dateLength.Second} second{(dateLength.Second > 1u ? "s" : string.Empty)}";
                    }

                    var twitchUserName = GetTwitchUserName(
				        webSocketMessage: webSocketMessage
				    );
				    var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
				    var message =
						$"{twitchUserName}, " +
						$"you can use !songrequest or !sr in " +
						$"{remainingTime}.";
				    var onScreenMessage =
				        $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
				        $"you can use " +
						$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: "!songrequest")} or " +
						$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: "!sr")} in " +
						$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"{remainingTime}")}.";

				    SendTwitchChatMessages(
				        twitchChatMessageId: twitchChatMessageId,
				        message: message,
				        onScreenMessage: onScreenMessage
                    );
                }
			}
		}

		private void HandleWebSocketMessagePrivMsgSongSkip(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
            var userName = webSocketMessage.UserName;
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var twitchUserName = GetTwitchUserName(
                webSocketMessage: webSocketMessage
            );
            if (
                userName.Equals(
                     value: m_twitchGlobalData.AccountUserName
                ) is false
			)
			{
                var userNameAdjusted = userName.ToLower();
                var channelModerators = m_twitchManager.GetChannelModerators();
                if (
                    channelModerators.ContainsKey(
                        key: userNameAdjusted
                    ) is false
                )
                {
                    var message = $"{twitchUserName}, you must be a moderator in order to use this command.";
                    var onScreenMessage = 
						$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
						$"{TwitchChatColorCodes.ConvertToErrorMessage(message: "you must be a moderator in order to use this command")}.";

                    SendTwitchChatMessages(
                        twitchChatMessageId: twitchChatMessageId,
                        message: message,
                        onScreenMessage: onScreenMessage
                    );
                    return;
                }
            }

            m_spotifyManager.QueueRequestSkipTrack(
				twitchUserName: twitchUserName,
                twitchChatMessageId: twitchChatMessageId
            );
        }

		private void HandleWebSocketMessagePrivMsgSpecs(
            TwitchChatWebSocketMessage webSocketMessage
        )
        {
			var twitchUserName = GetTwitchUserName(
				webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message = 
				$"{twitchUserName}, check the Hardware section below for the list of PC rig parts & peripherals @ " +
				$"https://www.twitch.tv/SmoothDagger/About";
			var onScreenMessage =
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, check the Hardware section below for the list of rig parts & peripherals @ \n" +
				$"{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/SmoothDagger/About")}";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

		private void HandleWebSocketMessagePrivMsgSteam(
            TwitchChatWebSocketMessage webSocketMessage
        )
        {
			var twitchUserName = GetTwitchUserName(
				webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message = $"{twitchUserName}, {c_twitchChatCommandMessages[key: TwitchChatAutomatedMessageType.Steam]}";
			var onScreenMessage = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
				$"{c_onScreenTwitchChatCommandMessages[key: TwitchChatAutomatedMessageType.Steam]}";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

        private void HandleWebSocketMessagePrivMsgTextToSpeech(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
            var userName = webSocketMessage.UserName;
            if (
                userName.Equals(
                     value: m_twitchGlobalData.AccountUserName
                ) is true
            )
            {
                var text = webSocketMessage.Text;
                text = text.Replace(
                    oldValue: c_commands[key: TwitchChatCommandType.TextToSpeech],
                    newValue: string.Empty
                );
                m_audioManager.PlayTextToSpeech(
                    text: text
                );
                return;
            }

            var userNameAdjusted = userName.ToLower();
            var channelSubscribers = m_twitchManager.GetChannelSubscribers();
            if (
                channelSubscribers.ContainsKey(
                    key: userNameAdjusted
                ) is false
            )
            {
                var twitchUserName = GetTwitchUserName(
                    webSocketMessage: webSocketMessage
                );
                var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
                var message = $"{twitchUserName}, you must be subscribed in order to use this command.";
                var onScreenMessage =
                    $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
                    $"{TwitchChatColorCodes.ConvertToErrorMessage(message: "you must be subscribed in order to use this command")}.";

                SendTwitchChatMessages(
                    twitchChatMessageId: twitchChatMessageId,
                    message: message,
                    onScreenMessage: onScreenMessage
                );
                return;
            }

            var utcNow = DateTime.UtcNow;
            var customSubscriberData = m_twitchManager.GetCustomUserData(
                userName: userName
            );
            if (customSubscriberData is null)
            {
                m_twitchManager.SetCustomUserData(
                    userName: userName,
                    userData: new()
                    {
                        TimeStampTextToSpeechIsAvailable =
                            $"{utcNow.AddHours(value: TwitchCustomUserData.TimeStampDelay):yyyy-MM-dd HH:mm:ss}",
                    }
                );

                var text = webSocketMessage.Text;
                text = text.Replace(
                    oldValue: c_commands[key: TwitchChatCommandType.TextToSpeech],
                    newValue: string.Empty
                );
                m_audioManager.PlayTextToSpeech(
                    text: text
                );
            }
            else
            {
                var timeStampTextToSpeechAsString = customSubscriberData.TimeStampTextToSpeechIsAvailable;
                var isTextToSpeechAvailable = DateTime.Compare(
                    t1: DateTime.UtcNow,
                    t2: DateTime.Parse(
                        s: timeStampTextToSpeechAsString
                    )
                ) >= 0;

                if (isTextToSpeechAvailable is true)
                {
                    customSubscriberData.TimeStampTextToSpeechIsAvailable =
                        $"{utcNow.AddHours(value: TwitchCustomUserData.TimeStampDelay):yyyy-MM-dd HH:mm:ss}";
                    m_twitchManager.SetCustomUserData(
                        userName: userName,
                        userData: customSubscriberData
                    );

                    var text = webSocketMessage.Text;
                    text = text.Replace(
                        oldValue: c_commands[key: TwitchChatCommandType.TextToSpeech],
                        newValue: string.Empty
                    );
                    m_audioManager.PlayTextToSpeech(
                        text: text
                    );
                }
                else
                {
                    var dateLength = DateCalculator.CalculateTimeDifference(
                        timeStart: $"{utcNow:yyyy-MM-dd HH:mm:ss}",
                        timeEnd: timeStampTextToSpeechAsString
                    );

                    var remainingTime = string.Empty;
                    if (dateLength.Month > 0u)
                    {
                        remainingTime += $"{dateLength.Month} month{(dateLength.Month > 1u ? "s" : string.Empty)}";
                    }
                    if (dateLength.Day > 0u)
                    {
                        remainingTime += remainingTime.Equals(
                            value: string.Empty
                        ) ? string.Empty : " ";
                        remainingTime += $"{dateLength.Day} day{(dateLength.Day > 1u ? "s" : string.Empty)}";
                    }
                    if (dateLength.Hour > 0u)
                    {
                        remainingTime += remainingTime.Equals(
                            value: string.Empty
                        ) ? string.Empty : " ";
                        remainingTime += $"{dateLength.Hour} hour{(dateLength.Hour > 1u ? "s" : string.Empty)}";
                    }
                    if (dateLength.Minute > 0u)
                    {
                        remainingTime += remainingTime.Equals(
                            value: string.Empty
                        ) ? string.Empty : " ";
                        remainingTime += $"{dateLength.Minute} minute{(dateLength.Minute > 1u ? "s" : string.Empty)}";
                    }
                    if (dateLength.Second > 0u)
                    {
                        remainingTime += remainingTime.Equals(
                            value: string.Empty
                        ) ? string.Empty : " ";
                        remainingTime += $"{dateLength.Second} second{(dateLength.Second > 1u ? "s" : string.Empty)}";
                    }

                    var twitchUserName = GetTwitchUserName(
                        webSocketMessage: webSocketMessage
                    );
                    var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
                    var message =
                        $"{twitchUserName}, " +
                        $"you can use !tts in " +
                        $"{remainingTime}.";
                    var onScreenMessage =
                        $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
                        $"you can use " +
                        $"{TwitchChatColorCodes.ConvertToErrorMessage(message: "!tts")} in " +
                        $"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"{remainingTime}")}.";

                    SendTwitchChatMessages(
                        twitchChatMessageId: twitchChatMessageId,
                        message: message,
                        onScreenMessage: onScreenMessage
                    );
                }
            }
        }

		private void HandleWebSocketMessagePrivMsgTime(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
            var text = webSocketMessage.Text;
			text = text.Replace(
				oldValue: c_commands[key: TwitchChatCommandType.Time],
				newValue: string.Empty
			);
			text = text.Replace(
                oldValue: "\r\n",
				newValue: string.Empty
			);

			var twitchUserName = GetTwitchUserName(
				webSocketMessage: webSocketMessage	
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            if (
				string.IsNullOrEmpty(
					value: text
				) is true
			)
			{
				var dateTime = DateTime.UtcNow;
                var message = $"{twitchUserName}, the current time in UTC is {dateTime:HH:mm:ss}.";
                var onScreenMessage =
					$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
					$"the current time in {TwitchChatColorCodes.ConvertToSuccessMessage(message: $"UTC")} is " +
					$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"{dateTime:HH:mm:ss}")}.";

                SendTwitchChatMessages(
                    twitchChatMessageId: twitchChatMessageId,
                    message: message,
                    onScreenMessage: onScreenMessage
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
				var message = $"{twitchUserName}, the current time in {text} is {dateTime:HH:mm:ss}.";
                var onScreenMessage = 
					$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
					$"the current time in {TwitchChatColorCodes.ConvertToSuccessMessage(message: text)} is " +
					$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"{dateTime:HH:mm:ss}")}.";

                SendTwitchChatMessages(
                    twitchChatMessageId: twitchChatMessageId,
                    message: message,
                    onScreenMessage: onScreenMessage
                );
            }
		}

		private void HandleWebSocketMessagePrivMsgYouTube(
            TwitchChatWebSocketMessage webSocketMessage
        )
        {
            var twitchUserName = GetTwitchUserName(
                webSocketMessage: webSocketMessage
            );
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message = $"{twitchUserName}, {c_twitchChatCommandMessages[key: TwitchChatAutomatedMessageType.YouTube]}";
            var onScreenMessage =
                $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
                $"{c_onScreenTwitchChatCommandMessages[key: TwitchChatAutomatedMessageType.YouTube]}";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
		}

        private static bool IsCommandValid(
		    TwitchChatCommandType commandType,
		    string text
		)
        {
            var commandText = c_commands[key: commandType];
            var commandLength = commandText.Length;

            return commandType switch
            {
                TwitchChatCommandType.AccountAge or
				TwitchChatCommandType.CS or
                TwitchChatCommandType.Current or
                TwitchChatCommandType.CurrentSong or
                TwitchChatCommandType.Discord or
				TwitchChatCommandType.Commands or
				TwitchChatCommandType.FollowAge or
				TwitchChatCommandType.Lurk or
                TwitchChatCommandType.Queue or
                TwitchChatCommandType.Rules or
				TwitchChatCommandType.Skip or
				TwitchChatCommandType.Song or
                TwitchChatCommandType.SongQueue or
                TwitchChatCommandType.SongSkip or
				TwitchChatCommandType.Specs or
				TwitchChatCommandType.Steam or
				TwitchChatCommandType.YouTube =>
					IsOverlayCommandInputlessValid(
                        commandType: commandType,
                        text: text,
                        commandLength: commandLength
                    ),

                TwitchChatCommandType.Date or
				TwitchChatCommandType.Time =>
					IsOverlayCommandDateTimeValid(
                        commandType: commandType,
                        text: text,
                        commandLength: commandLength
                    ),

				TwitchChatCommandType.SetLayout =>
					IsOverlayCommandLayoutValid(
						text: text
					),

				TwitchChatCommandType.SetColor or
                TwitchChatCommandType.SetColour =>
					IsOverlayCommandSetColorValid(
						text: text	
					),

                TwitchChatCommandType.SongRequest or
				TwitchChatCommandType.SR =>
					IsOverlayCommandSongRequestValid(
						text: text
					),

                TwitchChatCommandType.TextToSpeech =>
					IsOverlayCommandTextToSpeechValid(
                        commandType: commandType,
                        text: text,
                        commandLength: commandLength
                    ),

                _ => 
					false,
            };
        }

		private static bool IsOverlayCommandDateTimeValid(
			TwitchChatCommandType commandType,
			string text,
			int commandLength
		)
		{
            const int dateTimeExactLength = 9;
            const int dateTimeSpaceIndex = 5;
            const int dateTimeAbbreviationLength = 3;
			var isCommandLengthCorrect = c_commands[key: commandType].Equals(
				obj: text.Substr(
					from: 0,
					len: commandLength
				).ToLower()
			);
			if (isCommandLengthCorrect is false)
			{
				return false;
			}

            var textLength = text.Length - c_twitchMessageDelimiterLength;
            return
                textLength.Equals(
					obj: commandLength
				) ||
                (
                    textLength.Equals(
						obj: dateTimeExactLength
					) &&
                    text[index: dateTimeSpaceIndex] is ' ' &&
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
			TwitchChatCommandType commandType,
			string text,
			int commandLength
		)
		{
			var textLength = text.Length - c_twitchMessageDelimiterLength;
            return
                textLength.Equals(
					obj: commandLength
				) &&
				c_commands[key: commandType].Equals(
					obj: text.Substr(
                        from: 0,
                        len: commandLength
                    ).ToLower()
                );
		}

		private static bool IsOverlayCommandLayoutValid(
			string text	
		)
		{
			var trimmedText = text.Remove(
				startIndex: text.Length - c_webSocketMessageDelimiterLength
			);
			var normalizedText = trimmedText.ToLower();

			return Regex.IsMatch(
				input: normalizedText,
				pattern: c_setLayoutRegexPattern
			);
		}

		private static bool IsOverlayCommandSetColorValid(
			string text
		)
		{
			var trimmedText = text.Remove(
				startIndex: text.Length - c_webSocketMessageDelimiterLength
			);
			var normalizedText = trimmedText.ToLower();
			if (
                normalizedText.Equals(
					obj: $"{c_commands[key: TwitchChatCommandType.SetColor]}"
                ) is true ||
                normalizedText.Equals(
                    obj: $"{c_commands[key: TwitchChatCommandType.SetColour]}"
                ) is true
            )
			{
				return true;
			}

			return Regex.IsMatch(
				input: normalizedText,
				pattern: c_setColorRegexPattern
			) is true;
        }

		private static bool IsOverlayCommandSongRequestValid(
			string text
		)
		{
            var normalizedText = text.ToLower();
            return normalizedText.StartsWith(
                value: $"{c_commands[key: TwitchChatCommandType.SR]} "
            ) is true ||
			normalizedText.StartsWith(
				value: $"{c_commands[key: TwitchChatCommandType.SongRequest]} "
			) is true;
        }

		private static bool IsOverlayCommandTextToSpeechValid(
			TwitchChatCommandType commandType,
			string text,
			int commandLength
		)
		{
            const int ttsMinimumLength = 6;
            const int ttsSpaceIndex = 4;
			return
				text.Length >= ttsMinimumLength &&
				text[index: ttsSpaceIndex] is ' ' &&
				c_commands[commandType].Equals(
					obj: text.Substr(
                        from: 0,
                        len: commandLength
                    ).ToLower()
                ) is true;
        }

		private static bool IsSmoothGPT(
			string userName
		)
		{
			return userName is c_twitchUserName;
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
				case "raid":
                case "unraid":
				default:
					return;
			}
		}

		private void OnChannelCheered(
            TwitchWebSocketMessagePayloadEventChannelCheer @event
        )
        {
			var userName = @event.IsAnonymous ? "Anonymous" : @event.UserName;
			var bitCount = @event.Bits;
			var bitMessage = $"{bitCount} bit{(bitCount > 1u ? "ties" : string.Empty)}";
            var message =
				$"{userName} " +
				$"cheered with " +
				$"{bitMessage}! " +
				$"Cheers!";
            var onScreenMessage =
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: userName)} " +
				$"cheered with " +
				$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"{bitMessage}")}! " +
				$"Cheers!";

			SendTwitchChatMessages(
				twitchChatMessageId: string.Empty,
				message: message,
				onScreenMessage: onScreenMessage
            );
        }

		private void OnChannelFollowed(
            TwitchWebSocketMessagePayloadEventChannelFollow @event
        )
        {
			var userName = @event.UserName;
            var message = 
				$"{userName} followed! " +
				$"Welcome to the Smooth Crew! " +
				$"Make sure to check the Social section below for available follower commands you can use! " +
				$"Stay smooth & enjoy your stay!";
			var onScreenMessage = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: userName)} followed! " +
				$"Welcome to the Smooth Crew! " +
				$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: "Make sure to check the Social section below for available follower commands you can use")}! " +
				$"Stay smooth & enjoy your stay!";

            SendTwitchChatMessages(
                twitchChatMessageId: string.Empty,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

		private void OnChannelPointsCustomRewardRedeemed(
            TwitchWebSocketMessagePayloadEventChannelPointsCustomRewardRedeemed @event
        )
        {
			var userName = @event.UserName;
			var reward = @event.Reward;
			var rewardTitle = reward.Title;
            var message = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: userName)} " +
				$"claimed " +
				$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: rewardTitle)}!";
            AddBotChatMessage(
                message: message
            );
        }

		private void OnChannelRaided(
            TwitchWebSocketMessagePayloadEventChannelRaid @event
        )
        {
			var userName = @event.FromBroadcasterUserName;
			var viewerCount = @event.Viewers;
            var message = 
				$"{userName} " +
				$"is raiding with " +
				$"{viewerCount} viewer{(viewerCount > 1u ? "s" : string.Empty)}! " +
				$"Welcome in & enjoy your stay, raiders! " +
				$"Make sure to check them out @ " +
				$"https://www.twitch.tv/{userName}";
			var onScreenMessage =
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: userName)} " +
				$"is raiding with " +
				$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"{viewerCount} viewer{(viewerCount > 1u ? "s" : string.Empty)}")}! " +
				$"Welcome in & enjoy your stay, raiders! " +
				$"Make sure to check them out @ " +
				$"{TwitchChatColorCodes.ConvertToLinkMessage(message: $"https://www.twitch.tv/{userName}")}";

            SendTwitchChatMessages(
                twitchChatMessageId: string.Empty,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

		private void OnChannelSubscribed(
            TwitchWebSocketMessagePayloadEventChannelSubscribe @event
        )
        {
            var userName = @event.UserName;
			var message = 
				$"{userName} subscribed! " +
				$"Thanks for supporting the channel! " +
				$"Make sure to check the Social section below for available subscriber commands you can use! " +
				$"Stay smooth & enjoy your stay!";
			var onScreenMessage =
                $"{TwitchChatColorCodes.ConvertToUserMessage(message: userName)} subscribed! " +
				$"Thanks for supporting the channel! " +
                $"{TwitchChatColorCodes.ConvertToSuccessMessage(message: "Make sure to check the Social section below for available subscriber commands you can use")}! " +
                $"Stay smooth & enjoy your stay!";

            SendTwitchChatMessages(
                twitchChatMessageId: string.Empty,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

		private void OnChannelSubscriptionGifted(
            TwitchWebSocketMessagePayloadEventChannelSubscriptionGift @event
        )
		{
            var userName = @event.IsAnonymous is true ? "Anonymous" : @event.UserName;
			var total = @event.Total;
            var message = 
				$"{userName} " +
				$"gifted {total} subscription{(total > 1 ? "s" : string.Empty)}! " +
				$"Let's give a big shoutout to " +
				$"{userName}! " +
				$"Thanks for supporting the channel! " +
				$"Make sure to check the Social section below for available subscriber commands you can use! " +
				$"Stay smooth & enjoy your stay!\"";
            var onScreenMessage =
                $"{TwitchChatColorCodes.ConvertToUserMessage(message: userName)} " +
				$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"gifted {total} subscription{(total > 1 ? "s" : string.Empty)}")}! " +
                $"{TwitchChatColorCodes.ConvertToSuccessMessage(message: "Let's give a big shoutout to ")} " +
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: userName)}! " +
                $"{TwitchChatColorCodes.ConvertToSuccessMessage(message: "Thanks for supporting the channel")}! " +
                $"{TwitchChatColorCodes.ConvertToSuccessMessage(message: "Make sure to check the Social section below for available subscriber commands you can use")}! " +
                $"{TwitchChatColorCodes.ConvertToSuccessMessage(message: "Stay smooth & enjoy your stay")}!";

            SendTwitchChatMessages(
                twitchChatMessageId: string.Empty,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

        private void OnRequestAccessTokenCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
			if (
				HttpManager.IsResponseCodeSuccessful(
					responseCode
				) is true
			)
			{
#if DEBUG
                GD.Print(
					what: $"{nameof(TwitchBot)}.{nameof(OnRequestAccessTokenCompleted)}() - Web request {responseCode} POST successful."
				);
#endif

				WriteAccessToken(
                    response: JsonSerializer.Deserialize<TwitchResponseAccessToken>(
						json: Encoding.UTF8.GetString(
						    bytes: body,
						    index: 0,
						    count: body.Length
						)
                    )
                );

                ConnectWebSocket();
            }
            else
			{
#if DEBUG
				GD.PrintErr(
					what: $"{nameof(TwitchBot)}.{nameof(OnRequestAccessTokenCompleted)}() - Web request POST failed with code {responseCode}."
				);
#endif
			}
        }

        private void OnRequestAccessTokenWithRefreshTokenCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
			if (
				HttpManager.IsResponseCodeSuccessful(
					responseCode
				) is true
			)
			{
#if DEBUG
                GD.Print(
					what: $"{nameof(TwitchBot)}.{nameof(OnRequestAccessTokenWithRefreshTokenCompleted)}() - Web request {responseCode} POST successful."
				);
#endif

                WriteAccessToken(
                    response: JsonSerializer.Deserialize<TwitchResponseAccessToken>(
                        json: Encoding.UTF8.GetString(
                            bytes: body,
                            index: 0,
                            count: body.Length
                        )
                    )
                );

                ConnectWebSocket();
            }
            else
			{
#if DEBUG
				GD.PrintErr(
					what: $"{nameof(TwitchBot)}.{nameof(OnRequestAccessTokenWithRefreshTokenCompleted)}() - Web request POST failed with code {responseCode}."
				);
#endif
			}
        }

        private void OnSpotifyCurrentTrackRetrieved(
            SpotifyTwitchData spotifyTwitchData
        )
        {
            var message = 
				$"{spotifyTwitchData.TwitchUserName}, " +
				$"the current song playing is \"" +
				$"{spotifyTwitchData.TrackName} " +
				$"by " +
				$"{spotifyTwitchData.ArtistName}.\"";
			var onScreenMessage =
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: spotifyTwitchData.TwitchUserName)}," +
				$" the current song playing is " +
				$" \"{TwitchChatColorCodes.ConvertToSuccessMessage(message: spotifyTwitchData.TrackName)}" +
				$" by" +
				$" {TwitchChatColorCodes.ConvertToSuccessMessage(message: spotifyTwitchData.ArtistName)}.\"";

            SendTwitchChatMessages(
                twitchChatMessageId: spotifyTwitchData.TwitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

		private void OnSpotifyErrored(
            SpotifyTwitchData spotifyTwitchData
        )
        {
            var message = $"{spotifyTwitchData.TwitchUserName}, {spotifyTwitchData.Message}";
			var onScreenMessage =
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: spotifyTwitchData.TwitchUserName)}, " +
				$"{TwitchChatColorCodes.ConvertToErrorMessage(message: spotifyTwitchData.OnScreenMessage)}";

            SendTwitchChatMessages(
                twitchChatMessageId: spotifyTwitchData.TwitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

		private void OnSpotifyTrackQueuedCompleted(
            SpotifyTwitchData spotifyTwitchData
        )
        {
            var message =
				$"{spotifyTwitchData.TwitchUserName}, " +
				$"{spotifyTwitchData.TrackName}" +
				$" by " +
				$"{spotifyTwitchData.ArtistName} was added to the queue at position {spotifyTwitchData.QueuePosition}.";
			var onScreenMessage =
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: spotifyTwitchData.TwitchUserName)}, " +
				$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: spotifyTwitchData.TrackName)}" +
				$" by " +
				$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: spotifyTwitchData.ArtistName)}" +
				$" was added to the queue at " +
				$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"position {spotifyTwitchData.QueuePosition}")}.";

			SendTwitchChatMessages(
				twitchChatMessageId: spotifyTwitchData.TwitchChatMessageId,
				message: message,
				onScreenMessage: onScreenMessage
			);
        }

        private void OnSpotifyTrackSkipCompleted(
		    SpotifyTwitchData spotifyTwitchData
		)
        {
            var message = $"{spotifyTwitchData.TwitchUserName}, " +
				$"{spotifyTwitchData.TrackName}" +
				$" by " +
				$"{spotifyTwitchData.ArtistName} " +
				$"was successfully skipped.";
			var onScreenMessage =
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: spotifyTwitchData.TwitchUserName)}, " +
				$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"{spotifyTwitchData.TrackName}")}" +
				$" by " +
				$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"{spotifyTwitchData.ArtistName}")} " +
				$"was successfully skipped.";

			SendTwitchChatMessages(
				twitchChatMessageId: spotifyTwitchData.TwitchChatMessageId,
				message: message,
				onScreenMessage: onScreenMessage
			);
        }

        private void OnSpotifyUserTrackQueueRetrieveCompleted(
		    SpotifyTwitchData spotifyTwitchData
		)
        {
			var trackList = spotifyTwitchData.SpotifyQueuedUserTracks.ToList();

			var trackMessage = $"{trackList.Count} track{(trackList.Count > 1 ? "s" : string.Empty)} {(trackList.Count > 1 ? "are" : "is")} in queue";
            var message =
				$"{spotifyTwitchData.TwitchUserName}, " +
				$"{trackMessage}. Upcoming Tracks:";
            var onScreenMessage =
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: spotifyTwitchData.TwitchUserName)}, " +
				$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: trackMessage)}." +
				$"Upcoming Tracks:";

			var trackCount = trackList.Count > c_maxSpotifyQueueCount ? c_maxSpotifyQueueCount : trackList.Count;
            for (var i = 0; i < trackCount; i++)
			{
				var trackQueuePosition = i + 1;
                message +=
					$" #{trackQueuePosition}" +
					$" {trackList[i].TrackName}" +
					$" by" +
					$" {trackList[i].ArtistName}" +
					$" requested by {trackList[i].TwitchUserName}.";
				onScreenMessage +=
					$" #{trackQueuePosition}" +
					$" {TwitchChatColorCodes.ConvertToSuccessMessage(message: trackList[i].TrackName)}" +
					$" by" +
					$" {TwitchChatColorCodes.ConvertToSuccessMessage(message: trackList[i].ArtistName)}" +
					$" requested by" +
					$" {TwitchChatColorCodes.ConvertToUserMessage(message: trackList[i].TwitchUserName)}.";
            }

            SendTwitchChatMessages(
                twitchChatMessageId: spotifyTwitchData.TwitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

        private void OnSpotifyUserTrackQueueRetrieveFailed(
		    SpotifyTwitchData spotifyTwitchData
		)
        {
            var message =
				$"{spotifyTwitchData.TwitchUserName}, " +
				$"{spotifyTwitchData.Message}";
            var onScreenMessage =
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: spotifyTwitchData.TwitchUserName)}, " +
				$"{TwitchChatColorCodes.ConvertToErrorMessage(message: spotifyTwitchData.OnScreenMessage)}";

            SendTwitchChatMessages(
                twitchChatMessageId: spotifyTwitchData.TwitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

        private static string ParseTextSubCommand(
			string text
		)
		{
			var subCommand = string.Empty;
			var index = 0;
			while (text[index: index++] is not ' ') ;

			while (index < text.Length)
			{
				subCommand += text[index: index++];
			}

			subCommand = subCommand.Remove(
				startIndex: subCommand.Length - c_webSocketMessageDelimiterLength,
				count: c_webSocketMessageDelimiterLength
			);

			return subCommand;
		}

		private static void ParseWebSocketMessage(
			string message,
			out List<TwitchChatWebSocketMessage> webSocketMessages
		)
		{
			webSocketMessages = new();

			var index = 0;
			while (index < message.Length)
			{
				var webSocketMessage = new TwitchChatWebSocketMessage(
					tags: new(),
					userName: string.Empty,
					command: string.Empty,
					text: string.Empty
				);

				// parse tags
				if (message[index: index] is '@')
				{
					index++;
					while (true)
					{
						var key = string.Empty;
						while (message[index: index] is not '=')
						{
							key += message[index: index++];
						}
						index++;

						var value = string.Empty;
						while (message[index: index] is not ';' && message[index: index] is not ' ')
						{
							value += message[index: index++];
						}

						webSocketMessage.Tags.Add(
							key: key,
							value: value
						);

						if (message[index: index++] is ' ')
						{
							break;
						}
					}
				}

				// parse user name
				if (message[index: index] is ':')
				{
					while (message[index: index] is not '@' && message[index: index] is not ' ')
					{
						index++;
					}
					if (message[index: index++] is '@')
					{
						while (message[index: index] is not '.')
						{
							webSocketMessage.UserName += message[index: index++];
						}
						while (message[index: index] is not ' ')
						{
							index++;
						}
						index++;
					}
				}

				// parse command
				while (
					index < message.Length &&
					message[index: index] is not ' '
				)
				{
					webSocketMessage.Command += message[index: index++];
				}

				if (
					webSocketMessage.Command.EndsWith(
						value: c_twitchWebSocketMessagedelimiter
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
					while (message[index: index] is not ':')
					{
						parse += message[index: index++];
						if (
							parse.EndsWith(
								value: c_twitchWebSocketMessagedelimiter
							) is true
						)
						{
							break;
						}
					}

					// parse text message up to delimiter
					if (
						parse.EndsWith(
							value: c_twitchWebSocketMessagedelimiter
						) is false
					)
					{
						index++;
						while (index < message.Length)
						{
							if (
								webSocketMessage.Text.EndsWith(
									value: c_twitchWebSocketMessagedelimiter
								) is true
							)
							{
								webSocketMessage.Text = webSocketMessage.Text.Remove(
									startIndex: webSocketMessage.Text.Length - c_webSocketMessageDelimiterLength,
									count: c_webSocketMessageDelimiterLength
								);
								break;
							}

							webSocketMessage.Text += message[index: index++];
						}
					}
				}

				webSocketMessages.Add(
					item: webSocketMessage
				);
			}
		}

		private TwitchChatCommandValidityType ProcessChatCommand(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
			var text = webSocketMessage.Text;
			if (text[index: 0] is '!')
			{
				foreach (var command in c_commands)
				{
					var commandType = command.Key;
                    if (
                        IsCommandValid(
                            commandType: commandType,
                            text: text
                        ) is true
                    )
                    {
                        HandleApplicationCommandOverlay(
                            commandType: commandType,
                            webSocketMessage: webSocketMessage
                        );
                        return TwitchChatCommandValidityType.ValidChatCommand;
                    }
                }
				return TwitchChatCommandValidityType.InvalidChatCommand;
			}
			return TwitchChatCommandValidityType.NotAChatCommand;
		}

		private void ProcessChatMessage(
			TwitchChatWebSocketMessage webSocketMessage
		)
		{
			var userName = webSocketMessage.UserName;
			var isSubscriber = webSocketMessage.Tags[key: "subscriber"].ToInt() > 0u;

            string color;
            if (isSubscriber is true)
			{
				color = string.Empty;
			}
			else if (
				m_userNameColors.ContainsKey(
					key: userName
				) is true
			)
			{
				color = m_userNameColors[key: userName];
			}
			else
			{
				var userColorInHex = webSocketMessage.Tags[key: "color"];
				if (
					string.IsNullOrWhiteSpace(
						value: userColorInHex
					) is true
				)
				{
					color = m_pastelInterpolator.GetColorAsHex(
						rainbowColorIndexType: RainbowColorIndexType.Color0	
					);
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

                m_userNameColors.Add(
					key: userName,
					value: color
                );
			}

            var twitchUserName = GetTwitchUserName(
                webSocketMessage: webSocketMessage
            );
            var message = webSocketMessage.Text;
			var emotes = webSocketMessage.Tags.ContainsKey(
				key: "emotes"
			) ? webSocketMessage.Tags[key: "emotes"] : string.Empty;
			var badges = webSocketMessage.Tags.ContainsKey(
				key: "badges"
			) ? webSocketMessage.Tags[key: "badges"] : string.Empty;

			m_twitchChatManager.AddTwitchChatMessage(
				userName: userName,
                name: twitchUserName,
				nameColor: color,
				message: message,
				emotes: emotes,
				badges: badges,
				isSmoothGPT: false
			);
		}

        private void RequestAccessToken()
        {
            var headers = new List<string>()
            {
                $"Content-Type: application/x-www-form-urlencoded",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_twitchUriOAuth}",
                headers: headers,
                method: Method.Post,
                json:
                    $"client_id={m_twitchGlobalData.ClientId}&" +
                    $"client_secret={m_twitchGlobalData.ClientSecret}&" +
                    $"code={c_twitchOAuthAccessCode}&" +
                    $"grant_type=authorization_code&" +
                    $"redirect_uri={c_twitchUriRedirect}",
                requestCompletedHandler: OnRequestAccessTokenCompleted
            );
        }

        private void RequestAccessTokenWithRefreshToken()
        {
            var headers = new List<string>()
            {
                $"Content-Type: application/x-www-form-urlencoded",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_twitchUriOAuth}",
                headers: headers,
                method: Method.Post,
                json:
                    $"client_id={m_twitchGlobalData.ClientId}&" +
                    $"client_secret={m_twitchGlobalData.ClientSecret}&" +
                    $"grant_type=refresh_token&" +
                    $"refresh_token={m_twitchBotAccessToken.RefreshToken}",
                requestCompletedHandler: OnRequestAccessTokenWithRefreshTokenCompleted
            );
        }

        private void RequestSpotifyTrack(
			TwitchChatWebSocketMessage webSocketMessage	
		)
		{
			var twitchUserName = GetTwitchUserName(
                webSocketMessage: webSocketMessage
            );
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var text = webSocketMessage.Text;
            var trimmedText = text.Remove(
                startIndex: text.Length - c_webSocketMessageDelimiterLength
            );
            var searchText = trimmedText.Remove(
                startIndex: 0,
                count: trimmedText.StartsWith(
                    c_commands[key: TwitchChatCommandType.SR]
                ) ? c_commands[key: TwitchChatCommandType.SR].Length + 1 :
                    c_commands[key: TwitchChatCommandType.SongRequest].Length + 1
            );
            if (
                SpotifyManager.StartsWithValidSpotifyUri(
                    uri: searchText
                ) is true
            )
            {
                var trackId = SpotifyManager.ParseSpotifyUriForTrackId(
                    uri: searchText
                );
                m_spotifyManager.QueueRequestTrackQueueByTrackId(
                    twitchUserName: twitchUserName,
                    twitchChatMessageId: twitchChatMessageId,
                    trackId: trackId
                );
            }
            else
            {
                m_spotifyManager.QueueRequestTrackQueueBySeachTerms(
                    twitchUserName: twitchUserName,
                    twitchChatMessageId: twitchChatMessageId,
                    searchParameters: searchText
                );
            }
        }

        private void RetrieveOAuthAccessCode()
        {
            _ = OS.ShellOpen(
                uri: $"https://id.twitch.tv/oauth2/authorize?" +
                     $"response_type=code&" +
                     $"client_id={m_twitchGlobalData.ClientId}&" +
                     $"redirect_uri=http://localhost:3000&" +
                     $"scope={Uri.EscapeDataString(stringToEscape: c_twitchUserAccessScopes)}"
            );
        }

        private void RetrieveResources()
		{
			var twitchGlobalDataBody = ApplicationManager.ReadRequiredFile(
                requiredFileType: RequiredFileType.TwitchGlobalData
            );
            m_twitchGlobalData = JsonSerializer.Deserialize<TwitchGlobalData>(
                json: Encoding.UTF8.GetString(
                    bytes: twitchGlobalDataBody,
                    index: 0,
                    count: twitchGlobalDataBody.Length
                )
            );

			var twitchBotAccessTokenBody = ApplicationManager.ReadRequiredFile(
                requiredFileType: RequiredFileType.TwitchBotAccessToken
            );
            m_twitchBotAccessToken = JsonSerializer.Deserialize<TwitchBotAccessToken>(
                json: Encoding.UTF8.GetString(
                    bytes: twitchBotAccessTokenBody,
                    index: 0,
                    count: twitchBotAccessTokenBody.Length
                )
            );

			m_audioManager = GetNode<AudioManager>(
				path: NodeDirectory.GetNodePath(
					nodeType: NodeType.AudioManager
				)
			);
			m_httpManager = GetNode<HttpManager>(
				path: NodeDirectory.GetNodePath(
					nodeType: NodeType.HttpManager
				)
			);
			m_pastelInterpolator = GetNode<PastelInterpolator>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.PastelInterpolator
                )
            );
			m_spotifyManager = GetNode<SpotifyManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.SpotifyManager
                )
            );
			m_twitchChannelPointRewardsManager = GetNode<TwitchChannelPointRewardsManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.TwitchChannelPointRewardsManager
                )
            );
			m_twitchChatManager = GetNode<TwitchChatManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.TwitchChatManager
                )
            );
			m_twitchManager = GetNode<TwitchManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.TwitchManager
                )
            );
			m_uiManager = GetNode<UIManager>(
				path: NodeDirectory.GetNodePath(
					nodeType: NodeType.UIManager
				)
			);

            SubscribeToSpotifyManagerEvents();
            SubscribeToTwitchManagerEvents();
        }

		private async void SendAutomatedMessage()
		{
			await SendWebSocketMessage(
				message: $"PRIVMSG #{m_twitchGlobalData.TwitchChannel} :{c_automatedMessages[key: m_currentAutomatedMessageType]}"
			);
			AddBotChatMessage(
				message: $"{c_onScreenAutomatedMessages[key: m_currentAutomatedMessageType]}"
			);
		}

		private async void SendTwitchChatMessages(
			string twitchChatMessageId,
			string message,
			string onScreenMessage
		)
		{
			await SendWebSocketMessage(
				message: GetTwitchChannelMessage(
					twitchChatMessageId: twitchChatMessageId,
					message: message
                )
            );
            AddBotChatMessage(
                message: onScreenMessage
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

        private void SubscribeToSpotifyManagerEvents()
        {
            m_spotifyManager.CurrentTrackRetrieved += OnSpotifyCurrentTrackRetrieved;
            m_spotifyManager.Errored += OnSpotifyErrored;
            m_spotifyManager.TrackQueuedCompleted += OnSpotifyTrackQueuedCompleted;
			m_spotifyManager.TrackSkipped += OnSpotifyTrackSkipCompleted;
            m_spotifyManager.UserTrackQueueRetrieveCompleted += OnSpotifyUserTrackQueueRetrieveCompleted;
			m_spotifyManager.UserTrackQueueRetrieveFailed += OnSpotifyUserTrackQueueRetrieveFailed;
        }

        private void SubscribeToTwitchManagerEvents()
		{
			m_twitchManager.ChannelChatNotification += OnChannelChatNotification;
			m_twitchManager.ChannelCheered += OnChannelCheered;
            m_twitchManager.ChannelFollowed += OnChannelFollowed;
			m_twitchManager.ChannelPointsCustomRewardRedeemed += OnChannelPointsCustomRewardRedeemed;
            m_twitchManager.ChannelRaided += OnChannelRaided;
			m_twitchManager.ChannelSubscribed += OnChannelSubscribed;
			m_twitchManager.ChannelSubscriptionGifted += OnChannelSubscriptionGifted;
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
					var twitchAutomatedMessageTypeCount = Enum.GetValues<TwitchChatAutomatedMessageType>().Length;
					while (m_shutdown is false)
					{
						if (m_webSocket.State is WebSocketState.Open)
						{
							await Task.Delay(
								millisecondsDelay: c_automatedMessageDelayInMilliseconds
							);

							if (m_messageTimestamps.Count < c_minimumMessageCount)
							{
								continue;
							}

							var lastMessage = m_currentAutomatedMessageType;
							while (
								m_currentAutomatedMessageType.Equals(
									obj: lastMessage
								)
							)
							{
								m_currentAutomatedMessageType = (TwitchChatAutomatedMessageType)(GD.Randi() % twitchAutomatedMessageTypeCount);
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
				function:
				async () =>
				{
#if DEBUG
					GD.Print(
						what: $"{nameof(TwitchBot)}.{nameof(StartWebSocketMessageReader)}() - Web socket message reader starting."
					);
#endif

					var cancellationToken = new CancellationToken();
					var bytes = new byte[c_maxPacketSize];
                    while (m_shutdown is false)
					{
						if (
							m_webSocket.State is WebSocketState.Open && 
							cancellationToken.IsCancellationRequested is false
						)
						{
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

		private void WriteAccessToken(
            TwitchResponseAccessToken response
        )
        {
            m_twitchBotAccessToken.AccessToken = response.AccessToken;
            m_twitchBotAccessToken.RefreshToken = response.RefreshToken;

            ApplicationManager.WriteRequiredFile(
                requiredFileType: RequiredFileType.TwitchBotAccessToken,
                bytes: Encoding.UTF8.GetBytes(
                    s: JsonSerializer.Serialize(
                        value: m_twitchBotAccessToken
                    )
                )
            );
        }
    }
}