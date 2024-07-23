
namespace Overlay
{
	using Godot;
	using System;
	using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
	using ColorType = PastelInterpolator.ColorType;
    using FragmentType = TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragment.FragmentType;
	using NodeType = NodeDirectory.NodeType;
	using RainbowColorIndexType = PastelInterpolator.RainbowColorIndexType;
    using RequiredFileType = ApplicationManager.RequiredFileType;

    [SupportedOSPlatform(platformName: "windows")]
    public sealed partial class TwitchBot : Node
	{
		public override void _EnterTree()
		{
			// retrieve user access token
			//OS.ShellOpen(
			//    $"https://id.twitch.tv/oauth2/authorize" +
			//    $"?response_type=token" +
			//    $"&client_id={m_twitchData.ClientId}" +
			//    $"&redirect_uri=http://localhost:3000" +
			//    $"&scope=" +
			//        $"chat%3Aread%20" +    // chat:read
			//        $"chat%3Aedit"         // chat:edit
			//);
			RetrieveResources();
			SubscribeToSpotifyManagerEvents();
            SubscribeToTwitchManagerEvents();
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
			Supporter,
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
			SetColorSelection = 0u,
            SetColorUsage,
            TextToSpeech,
        }

        private enum CommandType : uint
        {
            AccountAge = 0u,
            Commands,
			CS,
			Current,
			CurrentSong,
            Date,
            Discord,
            FollowAge,
            Lurk,
			Queue,
            Rules,
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
            StreamAvatars,
            TextToSpeech,
            Time,
			Unlurk,
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
		private const int c_maxSpotifyQueueCount = 3;
		private const int c_minimumMessageCount = 5;
        private const int c_twitchMessageDelimiterLength = 2;
        private const int c_webSocketMessageDelimiterLength = 2;
        private const uint c_maxPacketSize = 8192u;
		private const ulong c_minimumMessageTimerInMilliseconds = 900000u;

		private static readonly Dictionary<AutomatedMessageType, string> c_automatedMessages = new()
		{
            { AutomatedMessageType.Commands,	    $"Check the Socials section below for a list of available bot commands @ https://www.twitch.tv/SmoothDagger/About" },
			{ AutomatedMessageType.Discord,		    $"Interested in chatting? Join the Discord @ https://www.discord.gg/SmoothCrew" },
			{ AutomatedMessageType.Rules,		    $"Make sure you're following the rules! Find them below in the rules section @ https://www.twitch.tv/SmoothDagger/About" },
            { AutomatedMessageType.Steam,		    $"Come play with us! Add me on Steam @ https://steamcommunity.com/id/SmoothDagger/" },
            { AutomatedMessageType.StreamAvatars,   $"Want to customize your stream avatar? Select an avatar below in the Stream Avatars section @ https://www.twitch.tv/SmoothDagger/About" },
            { AutomatedMessageType.Supporter,       $"Are you a follower or subscriber? Check the Socials section below for exclusive chat commands @ https://www.twitch.tv/SmoothDagger/About" },
            { AutomatedMessageType.TwitchFollow,    $"Enjoying the stream? Tap the follow button to get notified for any live streams!" },
            { AutomatedMessageType.TwitchSubscribe, $"Want ad-free viewing? Subscribe on Twitch @ https://www.twitch.tv/subs/SmoothDagger" },
            { AutomatedMessageType.YouTube,		    $"Looking for more content? Subscribe on YouTube @ https://www.youtube.com/@SmoothDagger" },
		};
        private static readonly Dictionary<AutomatedMessageType, string> c_onScreenAutomatedMessages = new()
        {
            { AutomatedMessageType.Commands,        $"Check the Socials section below for a list of available chat commands @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/SmoothDagger/About")}" },
            { AutomatedMessageType.Discord,         $"Interested in chatting? Join the Discord @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.discord.gg/SmoothCrew")}" },
            { AutomatedMessageType.Rules,           $"Make sure you're following the rules! Find them below in the rules section @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/SmoothDagger/About")}" },
            { AutomatedMessageType.Steam,           $"Come play with us! Add me on Steam @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://steamcommunity.com/id/SmoothDagger/")}" },
            { AutomatedMessageType.StreamAvatars,   $"Want to customize your stream avatar? Select an avatar below in the Stream Avatars section @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/SmoothDagger/About")}" },
            { AutomatedMessageType.Supporter,       $"Are you a follower or subscriber? Check the Socials section below for exclusive chat commands @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/SmoothDagger/About")}" },
            { AutomatedMessageType.TwitchFollow,    $"Enjoying the stream? {TwitchChatColorCodes.ConvertToLinkMessage(message: "Tap the follow button to get notified for any live streams")}!" },
            { AutomatedMessageType.TwitchSubscribe, $"Want ad-free viewing? Subscribe on Twitch @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/subs/SmoothDagger")}" },
            { AutomatedMessageType.YouTube,         $"Looking for more content? Subscribe on YouTube @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.youtube.com/@SmoothDagger")}" },
        };
		private static readonly Dictionary<AutomatedMessageType, string> c_twitchChatCommandMessages = new()
		{
            { AutomatedMessageType.Commands,	    $"check the Socials section below for a list of available bot commands @ https://www.twitch.tv/SmoothDagger/About" },
			{ AutomatedMessageType.Discord,		    $"interested in chatting? Join the Discord @ https://www.discord.gg/SmoothCrew" },
			{ AutomatedMessageType.Rules,		    $"make sure you're following the rules! Find them below in the rules section @ https://www.twitch.tv/SmoothDagger/About" },
            { AutomatedMessageType.Steam,		    $"come play with us! Add me on Steam @ https://steamcommunity.com/id/SmoothDagger/" },
            { AutomatedMessageType.StreamAvatars,   $"want to customize your stream avatar? Select an avatar below in the Stream Avatars section @ https://www.twitch.tv/SmoothDagger/About" },
            { AutomatedMessageType.YouTube,		    $"looking for more content? Subscribe on YouTube @ https://www.youtube.com/@SmoothDagger" },
		};
        private static readonly Dictionary<AutomatedMessageType, string> c_onScreenTwitchChatCommandMessages = new()
        {
            { AutomatedMessageType.Commands,        $"check the Socials section below for a list of available chat commands @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/SmoothDagger/About")}" },
            { AutomatedMessageType.Discord,         $"interested in chatting? Join the Discord @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.discord.gg/SmoothCrew")}" },
            { AutomatedMessageType.Rules,           $"make sure you're following the rules! Find them below in the rules section @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/SmoothDagger/About")}" },
            { AutomatedMessageType.Steam,           $"come play with us! Add me on Steam @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://steamcommunity.com/id/SmoothDagger/")}" },
            { AutomatedMessageType.StreamAvatars,   $"want to customize your stream avatar? Select an avatar below in the Stream Avatars section @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.twitch.tv/SmoothDagger/About")}" },
            { AutomatedMessageType.YouTube,         $"looking for more content? Subscribe on YouTube @ \n{TwitchChatColorCodes.ConvertToLinkMessage(message: "https://www.youtube.com/@SmoothDagger")}" },
        };
        private static readonly Dictionary<string, ColorType> c_colorStringsAsTypes = new()
        {
            { $"{ColorType.Red.ToString().ToLower()}",		 ColorType.Red		 },
            { $"{ColorType.Orange.ToString().ToLower()}",	 ColorType.Orange	 },
            { $"{ColorType.Yellow.ToString().ToLower()}",	 ColorType.Yellow	 },
            { $"{ColorType.Lime.ToString().ToLower()}",		 ColorType.Lime		 },
            { $"{ColorType.Green.ToString().ToLower()}",	 ColorType.Green	 },
            { $"{ColorType.Turquoise.ToString().ToLower()}", ColorType.Turquoise },
            { $"{ColorType.Cyan.ToString().ToLower()}",		 ColorType.Cyan		 },
            { $"{ColorType.Teal.ToString().ToLower()}",		 ColorType.Teal		 },
            { $"{ColorType.Blue.ToString().ToLower()}",		 ColorType.Blue		 },
            { $"{ColorType.Purple.ToString().ToLower()}",	 ColorType.Purple	 },
            { $"{ColorType.Magenta.ToString().ToLower()}",	 ColorType.Magenta	 },
            { $"{ColorType.Pink.ToString().ToLower()}",		 ColorType.Pink		 },
            { $"{ColorType.White.ToString().ToLower()}",	 ColorType.White	 },
            { $"{ColorType.Rainbow.ToString().ToLower()}",	 ColorType.Rainbow   },
        };
        private static readonly Dictionary<ColorType, string> c_colorTypesAsStrings = new()
        {
			{ ColorType.Red,	   $"{ColorType.Red.ToString().ToLower()}"		 },
			{ ColorType.Orange,	   $"{ColorType.Orange.ToString().ToLower()}"	 },
            { ColorType.Yellow,	   $"{ColorType.Yellow.ToString().ToLower()}"	 },
			{ ColorType.Lime,	   $"{ColorType.Lime.ToString().ToLower()}"		 },
            { ColorType.Green,	   $"{ColorType.Green.ToString().ToLower()}"	 },
			{ ColorType.Turquoise, $"{ColorType.Turquoise.ToString().ToLower()}" },
            { ColorType.Cyan,	   $"{ColorType.Cyan.ToString().ToLower()}"		 },
			{ ColorType.Teal,	   $"{ColorType.Teal.ToString().ToLower()}"		 },
            { ColorType.Blue,	   $"{ColorType.Blue.ToString().ToLower()}"		 },
			{ ColorType.Purple,	   $"{ColorType.Purple.ToString().ToLower()}"	 },
            { ColorType.Magenta,   $"{ColorType.Magenta.ToString().ToLower()}"	 },
			{ ColorType.Pink,	   $"{ColorType.Pink.ToString().ToLower()}"		 },
            { ColorType.White,	   $"{ColorType.White.ToString().ToLower()}"	 },
            { ColorType.Rainbow,   $"{ColorType.Rainbow.ToString().ToLower()}"	 },
        };
        private static readonly Dictionary<CommandType, string> c_commands = new()
		{
			// Bot
			{ CommandType.AccountAge,    "!accountage"	  },
            { CommandType.Commands,      "!commands"	  },
            { CommandType.CS,			 "!cs"			  },
            { CommandType.Current,		 "!current"		  },
            { CommandType.CurrentSong,   "!currentsong"	  },
            { CommandType.Date,          "!date"		  },
            { CommandType.Discord,       "!discord"		  },
            { CommandType.FollowAge,     "!followage"	  },
            { CommandType.Lurk,			 "!lurk"		  },
            { CommandType.Queue,		 "!queue"		  },
            { CommandType.Rules,         "!rules"		  },
            { CommandType.SetColor,      "!setcolor"	  },
            { CommandType.SetColour,     "!setcolour"	  },
            { CommandType.Skip,			 "!skip"		  },
            { CommandType.Song,		     "!song"		  },
            { CommandType.SongQueue,	 "!songqueue"	  },
            { CommandType.SongRequest,   "!songrequest"	  },
            { CommandType.SongSkip,		 "!songskip"	  },
            { CommandType.Specs,		 "!specs"		  },
            { CommandType.SR,			 "!sr"			  },
            { CommandType.Steam,         "!steam"		  },
            { CommandType.StreamAvatars, "!streamavatars" },
            { CommandType.TextToSpeech,  "!tts"			  },
            { CommandType.Time,          "!time"		  },
            { CommandType.Unlurk,        "!unlurk"		  },
            { CommandType.YouTube,       "!youtube"		  },
														  
			// Stream Avatars							  
            { CommandType.Accept,		 "!accept"		  },
            { CommandType.Actions,		 "!actions"		  },
            { CommandType.Attack,		 "!attack"		  },
            { CommandType.Avatar,		 "!avatar"		  },
            { CommandType.Avatars,		 "!avatars"		  },
            { CommandType.Basketball,	 "!basketball"	  },
            { CommandType.BattleRoyale,	 "!battleroyale"  },
            { CommandType.Bet,			 "!bet"			  },
            { CommandType.Blacklist,	 "!blacklist"	  },
            { CommandType.Bomb,			 "!bomb"		  },
            { CommandType.Boss,			 "!boss"		  },
            { CommandType.Buy,			 "!buy"			  },
            { CommandType.Change,		 "!change"		  },
            { CommandType.Color,		 "!color"		  },
            { CommandType.Currency,		 "!currency"	  },
            { CommandType.Dance,		 "!dance"		  },
            { CommandType.Decline,		 "!decline"		  },
            { CommandType.Duel,			 "!duel"		  },
            { CommandType.Explode,		 "!explode"		  },
            { CommandType.Extension,	 "!extension"	  },
            { CommandType.Fart,			 "!fart"		  },
            { CommandType.Freeze,		 "!freeze"		  },
            { CommandType.Game,			 "!game"		  },
            { CommandType.Gear,			 "!gear"		  },
            { CommandType.Gift,			 "!gift"		  },
            { CommandType.HideAvatar,	 "!hideavatar"	  },
            { CommandType.Hug,			 "!hug"			  },
            { CommandType.Jump,			 "!jump"		  },
            { CommandType.Leaderboard,	 "!leaderboard"	  },
            { CommandType.Mass,			 "!mass"		  },
            { CommandType.Mod,			 "!mod"			  },
            { CommandType.NameTags,		 "!nametags"	  },
            { CommandType.Pin,			 "!pin"			  },
            { CommandType.Quote,		 "!quote"		  },
            { CommandType.Random,		 "!random"		  },
            { CommandType.Remove,		 "!remove"		  },
            { CommandType.Roll,			 "!roll"		  },
            { CommandType.Scale,		 "!scale"		  },
            { CommandType.ScreenSaver,	 "!screensaver"	  },
            { CommandType.Shop,			 "!shop"		  },
            { CommandType.Shoutout,		 "!shoutout"	  },
            { CommandType.Show,			 "!show"		  },
            { CommandType.Sit,			 "!sit"			  },
            { CommandType.Sling,		 "!sling"		  },
            { CommandType.Slots,		 "!slots"		  },
            { CommandType.Sounds,		 "!sounds"		  },
            { CommandType.Spawn,		 "!spawn"		  },
            { CommandType.Throw,		 "!throw"		  },
            { CommandType.Whitelist,     "!whitelist"	  },
        };
        private static readonly Dictionary<CommandInfoMessageType, string> c_commandInfoMessages = new()
        {
            {
				CommandInfoMessageType.SetColorSelection,
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
				CommandInfoMessageType.SetColorUsage,
				$"Type {c_commands[key: CommandType.SetColor]} or {c_commands[key: CommandType.SetColour]} followed by a valid color option, such as red or pink, " +
				$"e.g., {c_commands[key: CommandType.SetColor]} {c_colorTypesAsStrings[key: ColorType.Red]} or {c_commands[key: CommandType.SetColour]} {c_colorTypesAsStrings[key: ColorType.Pink]}, " +
				$"to set the color of your text on screen."
			},
            {
				CommandInfoMessageType.TextToSpeech,
				$""
			},
        };
        private static readonly Dictionary<CommandInfoMessageType, string> c_onScreenCommandInfoMessages = new()
        {
            {
				CommandInfoMessageType.SetColorSelection,
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
				CommandInfoMessageType.SetColorUsage,
				$"Type {c_commands[key: CommandType.SetColor]} or {c_commands[key: CommandType.SetColour]} followed by a valid color option, such as red or pink, " +
				$"e.g., {c_commands[key: CommandType.SetColor]} {c_colorTypesAsStrings[key: ColorType.Red]} or {c_commands[key: CommandType.SetColour]} {c_colorTypesAsStrings[key: ColorType.Pink]}, " +
				$"to set the color of your text on screen."
			},
            {
				CommandInfoMessageType.TextToSpeech,
				$""
			},
        };

		private static readonly string c_setColorRegexPattern = 
			$"^(" +
			$"{c_commands[ key: CommandType.SetColor  ]}|" +
			$"{c_commands[ key: CommandType.SetColour ]}" +
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

        private struct WebSocketMessage
		{
			public Dictionary<string, string> Tags = new();
			public string UserName = string.Empty;
			public string Command = string.Empty;
			public string Text = string.Empty;

			public WebSocketMessage(
				Dictionary<string, string> tags,
				string userName,
				string command,
				string text
			)
			{
				this.Tags = tags;
				this.UserName = userName;
				this.Command = command;
				this.Text = text;
			}
		};

        private readonly Dictionary<string, string> m_userNameColors = new();
		private readonly HashSet<string> m_usersLurking = new();
        private readonly Queue<ulong> m_messageTimestamps = new();
        private readonly ClientWebSocket m_webSocket = new();

        private AudioManager m_audioManager = null;
		private PastelInterpolator m_pastelInterpolator = null;
		private SpotifyManager m_spotifyManager = null;
		private TwitchChannelPointRewardsManager m_twitchChannelPointRewardsManager = null;
		private TwitchChatManager m_twitchChatManager = null;
        private TwitchData m_twitchData = null;
        private TwitchManager m_twitchManager = null;
		private AutomatedMessageType m_currentAutomatedMessage = AutomatedMessageType.Count;
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
						username: c_twitchBotUsername,
					    name: c_twitchBotDisplayName,
						nameColor: string.Empty,
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
                message: $"PASS oauth:{m_twitchData.BotAccessToken}"
			);
			await SendWebSocketMessage(
                message: $"NICK {m_twitchData.BotUsername}"
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
                message: $"JOIN #{m_twitchData.TwitchChannel}"
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
				CommandType.CS or
                CommandType.Current or 
				CommandType.CurrentSong or
                CommandType.Date or
				CommandType.Discord or
				CommandType.Commands or
				CommandType.FollowAge or
				CommandType.Lurk or
                CommandType.Queue or
                CommandType.Rules or
				CommandType.SetColor or
				CommandType.SetColour or
				CommandType.Skip or
                CommandType.Song or
                CommandType.SongQueue or
                CommandType.SongRequest or
                CommandType.SongSkip or
                CommandType.Specs or
				CommandType.SR or
                CommandType.Steam or
				CommandType.StreamAvatars or
				CommandType.TextToSpeech or
				CommandType.Time or
				CommandType.Unlurk or
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

            return $"{replyMessageTarget}PRIVMSG #{m_twitchData.TwitchChannel} :{message}";
        }

		private static string GetTwitchUserName(
            WebSocketMessage webSocketMessage
        )
		{
            var username = webSocketMessage.UserName;
            var name = webSocketMessage.Tags[key: "display-name"];
            var normalizedName = name.ToLower();
            if (
                normalizedName.Equals(
                    obj: username
                ) is false
            )
            {
                name += $" ({username})";
            }

			return name;
        }

        private void HandleApplicationCommandOverlay(
			CommandType commandType,
			WebSocketMessage webSocketMessage
		)
		{
            switch (commandType)
            {
                case CommandType.AccountAge:
                    HandleWebSocketMessagePrivMsgAccountAge(
                        webSocketMessage: webSocketMessage
                    );
                    break;

				case CommandType.CS:
				case CommandType.Current:
                case CommandType.CurrentSong:
				case CommandType.Song:
                    HandleWebSocketMessagePrivMsgSongCurrent(
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

				case CommandType.Queue:
				case CommandType.SongQueue:
				    HandleWebSocketMessagePrivMsgSongQueue(
                        webSocketMessage: webSocketMessage
                    );
					break;

                case CommandType.Rules:
                    HandleWebSocketMessagePrivMsgRules(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case CommandType.SetColor:
                case CommandType.SetColour:
                    HandleWebSocketMessagePrivMsgSetColor(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case CommandType.Skip:
                case CommandType.SongSkip:
                    HandleWebSocketMessagePrivMsgSongSkip(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case CommandType.SongRequest:
				case CommandType.SR:
                    HandleWebSocketMessagePrivMsgSongRequest(
                        webSocketMessage: webSocketMessage
                    );
                    break;

                case CommandType.Specs:
                    HandleWebSocketMessagePrivMsgSpecs(
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

                case CommandType.Unlurk:
                    HandleWebSocketMessagePrivMsgUnlurk(
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

            var userName = $"@{@event.ChatterUserName}";
            var bitsBadgeTierText = $" Thank you so much for the {totalBits} bits! Congratulations on achieving the {bitsBadgeTier.Tier} bit tier!";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchData.TwitchChannel} :{userName}{bitsBadgeTierText}"
			);
		}

		private async void HandleChannelChatNotificationCharityDonation(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			// todo
			var charityDonation = @event.CharityDonation;
            var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
            var userName = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUserName}";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchData.TwitchChannel} :{userName} This event was not set up yet. Shame @SmoothDagger for being lazy! SHAME HIM"
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
			var userName = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUserName}";
			var communitySubGiftText = $" Thank you so much for the {communitySubGiftTotal} tier {communitySubGiftTier} gifted community sub{(communitySubGiftTotal > 1u ? "s" : string.Empty)}!";
			var communitySubGiftCumulativeText = $" {userName} has gifted a total of {communitySubGiftCumulativeTotal} community sub{(communitySubGiftCumulativeTotal > 1u ? "s" : string.Empty)}!";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchData.TwitchChannel} :{userName}{communitySubGiftText}{communitySubGiftCumulativeText}"
			);
		}

		private async void HandleChannelChatNotificationGiftPaidUpgrade(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			// todo
			var giftPaidUpgrade = @event.GiftPaidUpgrade;
			var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
            var userName = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUserName}";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchData.TwitchChannel} :{userName} This event was not set up yet. Shame @SmoothDagger for being lazy! SHAME HIM"
			);
		}

		private async void HandleChannelChatNotificationPayItForward(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			// todo
			var payItForward = @event.PayItForward;
            var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
            var userName = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUserName}";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchData.TwitchChannel} :{userName} This event was not set up yet. Shame @SmoothDagger for being lazy! SHAME HIM"
			);
		}

		private async void HandleChannelChatNotificationPrimePaidUpgrade(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			// todo
			var primePaidUpgrade = @event.PrimePaidUpgrade;
            var isChatterAnonymous = @event.ChatterIsAnonymous ?? false;
            var username = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUserName}";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchData.TwitchChannel} :{username} This event was not set up yet. Shame @SmoothDagger for being lazy! SHAME HIM"
			);
		}

		private async void HandleChannelChatNotificationRaid(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		)
		{
			var raid = @event.Raid;
            var userName = $"{raid.UserName}";
            var twitchChannel = $"https://www.twitch.tv/{raid.UserLogin}";
            await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchData.TwitchChannel} :Hello & welcome in, raiders! Shoutout to {userName} for the raid! Make sure to go check out them out @ {twitchChannel}"
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
			var userName = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUserName}";
			var userNameGifter = isGifterAnonymous ? "@Anonymous" : $"@{resub.GifterUserName}";
            var isResubPrime = resub.IsPrime ?? false;
            var resubText = isResubPrime ?
				$" Thank you so much for the prime resub!" :
				$" Thank you so much for the tier {resubTier} {resubDuration} month {(isResubGift ? "gifted" : string.Empty)} resub!";
			var resubGiftText = isResubGift ? $" Shoutout to {userNameGifter} for the gifted resub!" : string.Empty;
			var resubMonthsText = $" {userName} has been subbed for a total of {resubCumulativeMonths} months{(resubStreakMonths > 1u ? $" & is on a {resubStreakMonths} month sub streak!" : "!")}";
			var resubCommandsText = " Make sure to check out the available sub commands in the Social section below for your sub benefits @ https://www.twitch.tv/smoothdagger/about";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchData.TwitchChannel} :{userName}{resubText}{resubGiftText}{resubMonthsText}{resubCommandsText}"
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
			var username = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUserName}";
			var isPrimeSub = sub.IsPrime ?? false;
			var subText = isPrimeSub ?
				$" Thank you so much for the prime sub!" :
				$" Thank you so much for the tier {subTier} {subDuration} month sub!";
			var subCommandsText = " Make sure to check out the available sub commands in the Social section below for your sub benefits @ https://www.twitch.tv/smoothdagger/about";
			await SendWebSocketMessage(
                message: $"PRIVMSG #{m_twitchData.TwitchChannel} :{username}{subText}{subCommandsText}"
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
			var userName = isChatterAnonymous ? "@Anonymous" : $"@{@event.ChatterUserName}";
			var userNameRecipient = $"@{subGift.RecipientUserName}";
			var subGiftText = $" Thank you so much for the tier {subGiftTier} {subGiftDuration} month gifted sub to @{userNameRecipient}!";
			var subGiftMonthsText = isChatterAnonymous ? string.Empty : $"{userName} has gifted a total of {subGiftCumulativeTotal} sub{(subGiftCumulativeTotal > 1u ? "s" : string.Empty)}!";
			await SendWebSocketMessage(
				$"PRIVMSG #{m_twitchData.TwitchChannel} :{userName}{subGiftText}{subGiftMonthsText}"
			);
		}

		private void HandleInvalidChatCommand(
			WebSocketMessage webSocketMessage
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

		private void HandleUserNotFollowingMessage(
			WebSocketMessage webSocketMessage
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
			if (m_messageTimestamps.Count is c_minimumMessageCount)
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

		private void HandleWebSocketMessagePrivMsgAccountAge(
			WebSocketMessage webSocketMessage
		)
		{
            var user = m_twitchManager.GetUser(
				username: webSocketMessage.UserName
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
                    $"ou've been lurking in the depths of {TwitchChatColorCodes.ConvertToLinkMessage(message: "Twitch")} " +
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
			WebSocketMessage webSocketMessage
		)
		{
			var twitchUserName = GetTwitchUserName(
				webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message = $"{twitchUserName}, {c_twitchChatCommandMessages[key: AutomatedMessageType.Commands]}";
			var onScreenMessage = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
				$"{c_onScreenTwitchChatCommandMessages[key: AutomatedMessageType.Commands]}";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
		}

		private void HandleWebSocketMessagePrivMsgDate(
			WebSocketMessage webSocketMessage
		)
		{
            var twitchUserName = GetTwitchUserName(
			    webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];

            var text = webSocketMessage.Text;
			text = text.Replace(
				oldValue: c_commands[key: CommandType.Date],
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
			WebSocketMessage webSocketMessage
		)
		{
			var twitchUserName = GetTwitchUserName(
				webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message = $"{twitchUserName}, {c_twitchChatCommandMessages[key: AutomatedMessageType.Discord]}";
			var onScreenMessage = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
				$"{c_onScreenTwitchChatCommandMessages[key: AutomatedMessageType.Discord]}";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
		}

		private void HandleWebSocketMessagePrivMsgFollowAge(
			WebSocketMessage webSocketMessage
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

		private void HandleWebSocketMessagePrivMsgLurk(
			WebSocketMessage webSocketMessage
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
                    $"{twitchUserName} tried to disengage lurk mode, " +
                    $"but little did they know SmoothDagger knew they weren't lurking! " +
                    $"Rekt. " +
                    $"Try using !lurk first, noobie.";
                var onScreenMessage =
                    $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)} tried to engage lurk mode, " +
                    $"but little did they know {TwitchChatColorCodes.ConvertToUserMessage(message: "SmoothDagger")} knew they were already lurking! " +
                    $"Rekt. " +
                    $"Try using {TwitchChatColorCodes.ConvertToSuccessMessage(message: "!unlurk")} first, noobie.";

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
			WebSocketMessage webSocketMessage
		)
		{
			var twitchUserName = GetTwitchUserName(
				webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message = $"{twitchUserName}, {c_twitchChatCommandMessages[key: AutomatedMessageType.Rules]}";
			var onScreenMessage = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
				$"{c_onScreenTwitchChatCommandMessages[key: AutomatedMessageType.Rules]}";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

		private async void HandleWebSocketMessagePrivMsgSetColor(
			WebSocketMessage webSocketMessage
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
					obj: c_commands[key: CommandType.SetColor]
				) is true ||
                normalizedText.Equals(
                    obj: c_commands[key: CommandType.SetColour]
                ) is true
            )
			{
				var messageUsage = $"{twitchUserName}, {c_commandInfoMessages[key: CommandInfoMessageType.SetColorUsage]}";
				var onScreenMessageUsage =
					$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
					$"{c_onScreenCommandInfoMessages[key: CommandInfoMessageType.SetColorUsage]}";

				SendTwitchChatMessages(
				    twitchChatMessageId: twitchChatMessageId,
				    message: messageUsage,
				    onScreenMessage: onScreenMessageUsage
				);

				await Task.Delay(
					millisecondsDelay: 1
				);

                var messageSelection = $"{twitchUserName}, {c_commandInfoMessages[key: CommandInfoMessageType.SetColorSelection]}";
                var onScreenMessageSelection =
                    $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
                    $"{c_onScreenCommandInfoMessages[key: CommandInfoMessageType.SetColorSelection]}";

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

            var customSubscriberData = m_twitchManager.GetCustomSubscriberData(
				username: userName
            );
			if (customSubscriberData is null)
			{
				m_twitchManager.SetCustomSubscriberData(
					userName: userName,
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
					userName: userName,
					data: customSubscriberData
				);
			}
		}

		private void HandleWebSocketMessagePrivMsgSongCurrent(
			WebSocketMessage webSocketMessage
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
			WebSocketMessage webSocketMessage
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
			WebSocketMessage webSocketMessage
		)
		{
            var userName = webSocketMessage.UserName;
            if (
                userName.Equals(
                     value: m_twitchData.AccountUserName
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
            var customSubscriberData = m_twitchManager.GetCustomSubscriberData(
				username: userName
            );
            if (customSubscriberData is null)
			{
				m_twitchManager.SetCustomSubscriberData(
					userName: userName,
					data: new()
					{
						TimeStampSongRequestIsAvailable = 
							$"{utcNow.AddHours(value: TwitchCustomSubscriberData.TimeStampDelay):yyyy-MM-dd HH:mm:ss}",
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
						$"{utcNow.AddHours(value: TwitchCustomSubscriberData.TimeStampDelay):yyyy-MM-dd HH:mm:ss}";
                    m_twitchManager.SetCustomSubscriberData(
                        userName: userName,
                        data: customSubscriberData
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
			WebSocketMessage webSocketMessage
		)
		{
            var userName = webSocketMessage.UserName;
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var twitchUserName = GetTwitchUserName(
                webSocketMessage: webSocketMessage
            );
            if (
                userName.Equals(
                     value: m_twitchData.AccountUserName
                ) is false
			)
			{
                var usernameAdjusted = userName.ToLower();
                var channelModerators = m_twitchManager.GetChannelModerators();
                if (
                    channelModerators.ContainsKey(
                        key: usernameAdjusted
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
            WebSocketMessage webSocketMessage
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
            WebSocketMessage webSocketMessage
        )
        {
			var twitchUserName = GetTwitchUserName(
				webSocketMessage: webSocketMessage
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message = $"{twitchUserName}, {c_twitchChatCommandMessages[key: AutomatedMessageType.Steam]}";
			var onScreenMessage = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
				$"{c_onScreenTwitchChatCommandMessages[key: AutomatedMessageType.Steam]}";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

        private void HandleWebSocketMessagePrivMsgStreamAvatars(
			WebSocketMessage webSocketMessage		
		)
        {
			var twitchUserName = GetTwitchUserName(
				webSocketMessage: webSocketMessage	
			);
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message = $"{twitchUserName}, {c_twitchChatCommandMessages[key: AutomatedMessageType.StreamAvatars]}";
			var onScreenMessage = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
				$"{c_onScreenTwitchChatCommandMessages[key: AutomatedMessageType.StreamAvatars]}";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
        }

        private void HandleWebSocketMessagePrivMsgTextToSpeech(
			WebSocketMessage webSocketMessage
		)
		{
            var userName = webSocketMessage.UserName;
            if (
                userName.Equals(
                     value: m_twitchData.AccountUserName
                ) is true
            )
            {
                var text = webSocketMessage.Text;
                text = text.Replace(
                    oldValue: c_commands[key: CommandType.TextToSpeech],
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
            var customSubscriberData = m_twitchManager.GetCustomSubscriberData(
                username: userName
            );
            if (customSubscriberData is null)
            {
                m_twitchManager.SetCustomSubscriberData(
                    userName: userName,
                    data: new()
                    {
                        TimeStampTextToSpeechIsAvailable =
                            $"{utcNow.AddHours(value: TwitchCustomSubscriberData.TimeStampDelay):yyyy-MM-dd HH:mm:ss}",
                    }
                );

                RequestSpotifyTrack(
                    webSocketMessage: webSocketMessage
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
                        $"{utcNow.AddHours(value: TwitchCustomSubscriberData.TimeStampDelay):yyyy-MM-dd HH:mm:ss}";
                    m_twitchManager.SetCustomSubscriberData(
                        userName: userName,
                        data: customSubscriberData
                    );

                    var text = webSocketMessage.Text;
                    text = text.Replace(
                        oldValue: c_commands[key: CommandType.TextToSpeech],
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
			WebSocketMessage webSocketMessage
		)
		{
            var text = webSocketMessage.Text;
			text = text.Replace(
				oldValue: c_commands[key: CommandType.Time],
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

		private void HandleWebSocketMessagePrivMsgUnlurk(
			WebSocketMessage webSocketMessage
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
				) is false
			)
			{
				var message = 
					$"{twitchUserName} tried to disengage lurk mode, " +
					$"but little did they know SmoothDagger knew they weren't lurking! " +
					$"Rekt. " +
					$"Try using !lurk first, noobie.";
				var onScreenMessage =
                    $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)} tried to disengage lurk mode, " +
					$"but little did they know {TwitchChatColorCodes.ConvertToUserMessage(message: "SmoothDagger")} knew they weren't lurking! " +
                    $"Rekt. " +
                    $"Try using {TwitchChatColorCodes.ConvertToSuccessMessage(message: "!lurk")} first, noobie.";

                SendTwitchChatMessages(
                    twitchChatMessageId: twitchChatMessageId,
                    message: message,
                    onScreenMessage: onScreenMessage
                );
			}
			else
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

		private void HandleWebSocketMessagePrivMsgYouTube(
            WebSocketMessage webSocketMessage
        )
        {
            var twitchUserName = GetTwitchUserName(
                webSocketMessage: webSocketMessage
            );
            var twitchChatMessageId = webSocketMessage.Tags[key: "id"];
            var message = $"{twitchUserName}, {c_twitchChatCommandMessages[key: AutomatedMessageType.YouTube]}";
            var onScreenMessage =
                $"{TwitchChatColorCodes.ConvertToUserMessage(message: twitchUserName)}, " +
                $"{c_onScreenTwitchChatCommandMessages[key: AutomatedMessageType.YouTube]}";

            SendTwitchChatMessages(
                twitchChatMessageId: twitchChatMessageId,
                message: message,
                onScreenMessage: onScreenMessage
            );
		}

        private static bool IsApplicationCommandOverlayValid(
		    CommandType commandType,
		    string text
		)
        {
            var commandText = c_commands[commandType];
            var commandLength = commandText.Length;

            return commandType switch
            {
                CommandType.AccountAge or
				CommandType.CS or
                CommandType.Current or
                CommandType.CurrentSong or
                CommandType.Discord or
				CommandType.Commands or
				CommandType.FollowAge or
				CommandType.Lurk or
                CommandType.Queue or
                CommandType.Rules or
				CommandType.Skip or
				CommandType.Song or
                CommandType.SongQueue or
                CommandType.SongSkip or
				CommandType.Specs or
				CommandType.Steam or
				CommandType.StreamAvatars or
				CommandType.Unlurk or
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

				CommandType.SetColor or
                CommandType.SetColour =>
					IsOverlayCommandSetColorValid(
						text: text	
					),

                CommandType.SongRequest or
				CommandType.SR =>
					IsOverlayCommandSongRequestValid(
						text: text
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

		private static bool IsApplicationCommandStreamAvatarsTextValid(
			CommandType commandType,
			string text
		)
		{
            var normalizedText = text.ToLower();
            return normalizedText.StartsWith(
                value: c_commands[commandType]
            );
        }

        private static bool IsApplicationCommandStreamAvatarsValid(
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
                CommandType.CS or
                CommandType.Current or
				CommandType.CurrentSong or
                CommandType.Date or
                CommandType.Discord or
                CommandType.FollowAge or
				CommandType.Lurk or
                CommandType.Queue or
                CommandType.Rules or
                CommandType.SetColor or
                CommandType.SetColour or
                CommandType.Skip or
                CommandType.Song or
                CommandType.SongQueue or
                CommandType.SongRequest or
                CommandType.SongSkip or
                CommandType.Specs or
                CommandType.SR or
                CommandType.Steam or
                CommandType.StreamAvatars or
                CommandType.TextToSpeech or
                CommandType.Time or
                CommandType.Unlurk or
                CommandType.YouTube or
                _ =>
					false,
            };
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
			var trimmedText = text.Remove(
				startIndex: text.Length - c_webSocketMessageDelimiterLength
			);
			var normalizedText = trimmedText.ToLower();
			if (
                normalizedText.Equals(
					obj: $"{c_commands[key: CommandType.SetColor]}"
                ) is true ||
                normalizedText.Equals(
                    obj: $"{c_commands[key: CommandType.SetColour]}"
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
                value: $"{c_commands[key: CommandType.SR]} "
            ) ||
			normalizedText.StartsWith(
				value: $"{c_commands[key: CommandType.SongRequest]} "
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

		private void OnChannelCheered(
            TwitchWebSocketMessagePayloadEventChannelCheer @event
        )
        {
			var userName = @event.IsAnonymous ? "Anonymous" : @event.UserName;
			var bitCount = @event.Bits;
            var message = 
				$"{userName} " +
				$"cheered with " +
				$"{bitCount} bit{(bitCount > 1u ? string.Empty : "ties")}! " +
				$"Cheers!";
            var onScreenMessage = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: userName)} " +
				$"cheered with " +
				$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"{bitCount} bit{(bitCount > 1u ? string.Empty : "ties")}")}! " +
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
				$"Welcome in & enjoy your stay, raiders!";
			var onScreenMessage = 
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: userName)} " +
				$"is raiding with " +
				$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: $"{viewerCount} viewer{(viewerCount > 1u ? "s" : string.Empty)}")}! " +
				$"Welcome in & enjoy your stay, raiders!";

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
            var message = $"{spotifyTwitchData.TwitchUserName}, track was successfully skipped.";
			var onScreenMessage =
				$"{TwitchChatColorCodes.ConvertToUserMessage(message: spotifyTwitchData.TwitchUserName)}, " +
				$"{TwitchChatColorCodes.ConvertToSuccessMessage(message: "track was successfully skipped")}.";

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
			out List<WebSocketMessage> webSocketMessages
		)
		{
			webSocketMessages = new();

			var index = 0;
			while (index < message.Length)
			{
				var webSocketMessage = new WebSocketMessage(
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

				// parse username
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
					while (message[index: index] is not ':')
					{
						parse += message[index: index++];
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

							webSocketMessage.Text += message[index: index++];
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
			if (text[index: 0] is '!')
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
			var userName = webSocketMessage.UserName;
			var isSubscriber = webSocketMessage.Tags[key: "subscriber"].ToInt() > 0u;

            string color;
            if (isSubscriber)
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
				username: userName,
                name: twitchUserName,
				nameColor: color,
				message: message,
				emotes: emotes,
				badges: badges,
				isSmoothGPT: false
			);
		}

		private void RequestSpotifyTrack(
			WebSocketMessage webSocketMessage	
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
                    c_commands[key: CommandType.SR]
                ) ? c_commands[key: CommandType.SR].Length + 1 :
                    c_commands[key: CommandType.SongRequest].Length + 1
            );
            if (
                SpotifyManager.StartsWithValidSpotifyUrl(
                    url: searchText
                ) is true
            )
            {
                var trackId = SpotifyManager.ParseSpotifyUrlForTrackId(
                    url: searchText
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

		private void RetrieveResources()
		{
			var body = ApplicationManager.ReadRequiredFile(
                requiredFileType: RequiredFileType.TwitchData
            );
            m_twitchData = JsonSerializer.Deserialize<TwitchData>(
                json: Encoding.UTF8.GetString(
                    bytes: body,
                    index: 0,
                    count: body.Length
                )
            );

			m_audioManager = GetNode<AudioManager>(
				path: NodeDirectory.NodePaths[key: NodeType.AudioManager]
			);
			m_pastelInterpolator = GetNode<PastelInterpolator>(
                path: NodeDirectory.NodePaths[key: NodeType.PastelInterpolator]
			);
			m_spotifyManager = GetNode<SpotifyManager>(
                path: NodeDirectory.NodePaths[key: NodeType.SpotifyManager]
			);
			m_twitchChannelPointRewardsManager = GetNode<TwitchChannelPointRewardsManager>(
				path: NodeDirectory.NodePaths[key: NodeType.TwitchChannelPointRewardsManager]
			);
			m_twitchChatManager = GetNode<TwitchChatManager>(
                path: NodeDirectory.NodePaths[key: NodeType.TwitchChatManager]
			);
			m_twitchManager = GetNode<TwitchManager>(
                path: NodeDirectory.NodePaths[key: NodeType.TwitchManager]
			);
		}

		private async void SendAutomatedMessage()
		{
			await SendWebSocketMessage(
				message: $"PRIVMSG #{m_twitchData.TwitchChannel} :{c_automatedMessages[key: m_currentAutomatedMessage]}"
			);
			AddBotChatMessage(
				message: $"{c_onScreenAutomatedMessages[key: m_currentAutomatedMessage]}"
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
				function: 
				async () =>
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