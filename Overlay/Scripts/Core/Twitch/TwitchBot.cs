using Godot;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using ChannelPointRewardsType = TwitchChannelPointRewardsManager.ChannelPointRewardsType;
using FragmentType = TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragment.FragmentType;
using NodeType = NodeDirectory.NodeType;
using PlaylistType = AudioManager.PlaylistType;

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
            CommandType commandType = command.Key;
            if (
                !IsCommandAvailable(
                    commandType
                )
            )
            {
                double commandTimer = command.Value;
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
            ulong elapsedMilliseconds = Time.GetTicksMsec();
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

    private enum AutomatedMessageType : uint
    {
        Commands = 0u,
        Discord,
        Rules,
        Twitch,
        YouTube,
        Count
    }

    private enum CommandType : uint
    {
        Age = 0u,
        Commands,
        CurrentSong,
        Date,
        Discord,
        FollowAge,
        RequestSong,
        Rules,
        SetPlaylist,
        SkipSong,
        TextToSpeech,
        Time,
        YouTube
    }

    private const string c_websocketAddress = "wss://irc-ws.chat.twitch.tv:443";
    private const string c_webSocketMessagedelimiter = "\r\n";
    private const string c_twitchBotDisplayName = "SmoothGPT";
    private const string c_twitchBotUsername = "smoothgpt";
    private const int c_webSocketMessageDelimiterLength = 2;
    private const uint c_maxPacketSize = 8192u;
    private const uint c_minimumMessageCount = 5u;
    private const ulong c_minimumMessageTimerInMilliseconds = 900000u;

    private readonly Dictionary<CommandType, string> c_commands = new()
    {
        { CommandType.Age,          "!age"},
        { CommandType.Commands,     "!commands"},
        { CommandType.CurrentSong,  "!currentsong"},
        { CommandType.Date,         "!date" },
        { CommandType.Discord,      "!discord"},
        { CommandType.FollowAge,    "!followage"},
        { CommandType.RequestSong,  "!requestsong"},
        { CommandType.Rules,        "!rules"},
        { CommandType.SetPlaylist,  "!setplaylist"},
        { CommandType.SkipSong,     "!skipsong"},
        { CommandType.TextToSpeech, "!tts" },
        { CommandType.Time,         "!time" },
        { CommandType.YouTube,      "!youtube" }
    };

    private readonly Dictionary<AutomatedMessageType, string> c_automatedMessages = new()
    {
        { AutomatedMessageType.Commands, "Check the Socials section below for a list of available bot commands @ https://www.twitch.tv/SmoothDagger/About" },
        { AutomatedMessageType.Discord,  "Join the Discord @ \nhttps://www.discord.gg/SmoothCrew" },
        { AutomatedMessageType.Rules,    "Make sure you're following the rules! Find them below in the rules section @ https://www.twitch.tv/SmoothDagger/About" },
        { AutomatedMessageType.Twitch,   "Enjoying the stream? Tap that follow button to get notified for any live streams!" },
        { AutomatedMessageType.YouTube,  "Subscribe on YouTube @ \nhttps://www.youtube.com/@SmoothDagger" },
    };
    private readonly Dictionary<CommandType, double> c_commandTimers = new()
    {
        { CommandType.Age,          0d },
        { CommandType.Commands,     0d },
        { CommandType.CurrentSong,  0d },
        { CommandType.Date,         0d },
        { CommandType.Discord,      0d },
        { CommandType.FollowAge,    0d },
        { CommandType.RequestSong,  0d },
        { CommandType.Rules,        0d },
        { CommandType.SetPlaylist,  0d },
        { CommandType.SkipSong,     0d },
        { CommandType.TextToSpeech, 0d },
        { CommandType.Time,         0d },
        { CommandType.YouTube,      0d },
    };
    private readonly Dictionary<CommandType, double> c_commandCooldowns = new()
    {
        { CommandType.Age,          1d   },
        { CommandType.Commands,     30d  },
        { CommandType.CurrentSong,  10d  },
        { CommandType.Date,         10d  },
        { CommandType.Discord,      30d  },
        { CommandType.FollowAge,    1d   },
        { CommandType.RequestSong,  10d  },
        { CommandType.Rules,        30d  },
        { CommandType.SetPlaylist,  300d },
        { CommandType.SkipSong,     30d  },
        { CommandType.TextToSpeech, 1d   },
        { CommandType.Time,         10d  },
        { CommandType.YouTube,      30d  },
    };

    private struct WebSocketMessage
    {
        public Dictionary<string, string> tags = new();
        public string username = string.Empty;
        public string command = string.Empty;
        public string text = string.Empty;

        public WebSocketMessage(
            Dictionary<string, string> tags,
            string username,
            string command,
            string text
        )
        {
            this.tags = tags;
            this.username = username;
            this.command = command;
            this.text = text;
        }
    };

    private AudioManager m_audioManager = null;
    private ClientWebSocket m_webSocket = new();
    private PastelInterpolator m_pastelInterpolator = null;
    private TwitchChannelPointRewardsManager m_twitchChannelPointRewardsManager = null;
    private TwitchChatManager m_twitchChatManager = null;
    private TwitchManager m_twitchManager = null;
    private Dictionary<string, string> m_usernameColors = new();
    private List<string> m_subscribersWhoUsedTextToSpeech = new();
    private Queue<ulong> m_messageTimestamps = new();

    private AutomatedMessageType m_currentAutomatedMessage = AutomatedMessageType.Count;
    private bool m_shutdown = false;

    private void AddBotChatMessage(
        string message
    )
    {
        m_twitchChatManager.AddTwitchChatMessage(
            c_twitchBotDisplayName,
            string.Empty,
            message,
            string.Empty
        );
    }

    private async void ConnectWebSocket()
    {
        // connect to Twitch IRC websocket
        Uri uri = new(
            c_websocketAddress
        );

        await m_webSocket.ConnectAsync(
            uri,
            default
        );

        await SendWebSocketMessage(
            $"CAP REQ :twitch.tv/commands twitch.tv/tags"
        );
        await SendWebSocketMessage(
            $"PASS oauth:{TwitchData.BotAccessToken}"
        );
        await SendWebSocketMessage(
            $"NICK {TwitchData.BotUsername}"
        );

        var bytes = new byte[c_maxPacketSize];
        var result = await m_webSocket.ReceiveAsync(
            bytes,
            default
        );

        ParseWebSocketMessage(
            Encoding.UTF8.GetString(
                bytes,
                0,
                result.Count
            ),
            out List<WebSocketMessage> webSocketMessages
        );
        foreach (var webSocketMessage in webSocketMessages)
        {
            if (
                webSocketMessage.command == "NOTICE"
            )
            {
#if DEBUG
                GD.PrintErr(
                    $"{nameof(TwitchBot)}.{nameof(ConnectWebSocket)}() - Web socket connect failed."
                );
#endif
                return;
            }
        }

#if DEBUG
        GD.Print(
            $"{nameof(TwitchBot)}.{nameof(ConnectWebSocket)}() - Web socket connect successful."
        );
#endif

        await SendWebSocketMessage(
            $"JOIN #{TwitchData.TwitchChannel}"
        );

        StartWebSocketMessageReader();
        StartWebSocketAutomatedMessageDispatcher();
    }

    private async void HandleChannelChatNotificationBitsBadgeTier(
        TwitchWebSocketMessagePayloadEventChannelChatNotification @event
    )
    {
        var bitsBadgeTier = @event.bits_badge_tier;

        var message = @event.message;
        var fragments = message.fragments;
        int totalBits = 0;
        foreach (var fragment in fragments) 
        {
            var fragmentType = fragment.GetFragmentType();
            if (fragmentType == FragmentType.Cheermote)
            {
                var cheermote = fragment.cheermote;
                totalBits += cheermote.bits;
            }
        }

        string username = $"@{@event.chatter_user_name}";
        string bitsBadgeTierText = $" Thank you so much for the {totalBits} bits! Congratulations on achieving the {bitsBadgeTier.tier} bit tier!";
        await SendWebSocketMessage(
            $"PRIVMSG #{TwitchData.TwitchChannel} :{username}{bitsBadgeTierText}"
        );
    }

    private async void HandleChannelChatNotificationCharityDonation(
        TwitchWebSocketMessagePayloadEventChannelChatNotification @event
    )
    {
        // todo
        var charityDonation = @event.charity_donation;
        string username = @event.chatter_is_anonymous ? "@Anonymous" : $"@{@event.chatter_user_name}";
        await SendWebSocketMessage(
            $"PRIVMSG #{TwitchData.TwitchChannel} :{username} This event was not set up yet. Shame @SmoothDagger for being lazy! SHAME HIM"
        );
    }

    private async void HandleChannelChatNotificationCommunitySubGift(
        TwitchWebSocketMessagePayloadEventChannelChatNotification @event
    )
    {
        var communitySubGift = @event.community_sub_gift;
        bool isAnonymous = @event.chatter_is_anonymous;
        int communitySubGiftCumulativeTotal = communitySubGift.cumulative_total;
        int communitySubGiftTotal = communitySubGift.total;
        int communitySubGiftTier = int.Parse(
            communitySubGift.sub_tier[0].ToString()
        );
        string username = isAnonymous ? "@Anonymous" : $"@{@event.chatter_user_name}";
        string communitySubGiftText = $" Thank you so much for the {communitySubGiftTotal} tier {communitySubGiftTier} gifted community sub{(communitySubGiftTotal > 1u ? "s" : string.Empty)}!";
        string communitySubGiftCumulativeText = $" {username} has gifted a total of {communitySubGiftCumulativeTotal} community sub{(communitySubGiftCumulativeTotal > 1u ? "s" : string.Empty)}!";
        string communitySubGiftCommandsText = " Make sure to check out the available sub commands in the Social section below for your sub benefits @ https://www.twitch.tv/smoothdagger/about";
        await SendWebSocketMessage(
            $"PRIVMSG #{TwitchData.TwitchChannel} :{username}{communitySubGiftText}{communitySubGiftCumulativeText}{communitySubGiftCommandsText}"
        );
    }

    private async void HandleChannelChatNotificationGiftPaidUpgrade(
        TwitchWebSocketMessagePayloadEventChannelChatNotification @event
    )
    {
        // todo
        var giftPaidUpgrade = @event.gift_paid_upgrade;
        string username = @event.chatter_is_anonymous ? "@Anonymous" : $"@{@event.chatter_user_name}";
        await SendWebSocketMessage(
            $"PRIVMSG #{TwitchData.TwitchChannel} :{username} This event was not set up yet. Shame @SmoothDagger for being lazy! SHAME HIM"
        );
    }

    private async void HandleChannelChatNotificationPayItForward(
        TwitchWebSocketMessagePayloadEventChannelChatNotification @event
    )
    {
        // todo
        var payItForward = @event.pay_it_forward;
        string username = @event.chatter_is_anonymous ? "@Anonymous" : $"@{@event.chatter_user_name}";
        await SendWebSocketMessage(
            $"PRIVMSG #{TwitchData.TwitchChannel} :{username} This event was not set up yet. Shame @SmoothDagger for being lazy! SHAME HIM"
        );
    }

    private async void HandleChannelChatNotificationPrimePaidUpgrade(
        TwitchWebSocketMessagePayloadEventChannelChatNotification @event
    )
    {
        // todo
        var primePaidUpgrade = @event.prime_paid_upgrade;
        string username = @event.chatter_is_anonymous ? "@Anonymous" : $"@{@event.chatter_user_name}";
        await SendWebSocketMessage(
            $"PRIVMSG #{TwitchData.TwitchChannel} :{username} This event was not set up yet. Shame @SmoothDagger for being lazy! SHAME HIM"
        );
    }

    private async void HandleChannelChatNotificationRaid(
        TwitchWebSocketMessagePayloadEventChannelChatNotification @event
    )
    {
        var raid = @event.raid;
        string username = $"@{raid.user_name}";
        string twitchChannel = $"https://www.twitch.tv/{raid.user_login}";
        await SendWebSocketMessage(
            $"PRIVMSG #{TwitchData.TwitchChannel} :Hello & welcome in, raiders! Shoutout to {username} for the raid! Make sure to go check out them out @ {twitchChannel}"
        );
    }

    private async void HandleChannelChatNotificationResub(
        TwitchWebSocketMessagePayloadEventChannelChatNotification @event
    )
    {
        var resub = @event.resub;
        bool isGifterAnonymous = resub.gifter_is_anonymous;
        bool isResubGift = resub.is_gift;
        int resubDuration = resub.duration_months;
        int resubCumulativeMonths = resub.cumulative_months;
        int resubStreakMonths = resub.streak_months;
        int resubTier = int.Parse(
            resub.sub_tier[0].ToString()
        );
        string username = @event.chatter_is_anonymous ? "@Anonymous" : $"@{@event.chatter_user_name}";
        string usernameGifter = isGifterAnonymous ? "@Anonymous" : $"@{resub.gifter_user_name}";
        string resubText = resub.is_prime ?
            $" Thank you so much for the prime resub!" :
            $" Thank you so much for the tier {resubTier} {resubDuration} month {(isResubGift ? "gifted" : string.Empty)} resub!";
        string resubGiftText = isResubGift ? $" Shoutout to {usernameGifter} for the gifted resub!" : string.Empty;
        string resubMonthsText = $" {username} has been subbed for a total of {resubCumulativeMonths} months{(resubStreakMonths > 1u ? $" & is on a {resubStreakMonths} month sub streak!" : "!")}";
        string resubCommandsText = " Make sure to check out the available sub commands in the Social section below for your sub benefits @ https://www.twitch.tv/smoothdagger/about";
        await SendWebSocketMessage(
            $"PRIVMSG #{TwitchData.TwitchChannel} :{username}{resubText}{resubGiftText}{resubMonthsText}{resubCommandsText}"
        );
    }

    private async void HandleChannelChatNotificationSub(
        TwitchWebSocketMessagePayloadEventChannelChatNotification @event
    )
    {
        var sub = @event.sub;
        bool isAnonymous = @event.chatter_is_anonymous;
        int subDuration = sub.duration_months;
        int subTier = int.Parse(
            sub.sub_tier[0].ToString()
        );
        string username = isAnonymous ? "@Anonymous" : $"@{@event.chatter_user_name}";
        string subText = sub.is_prime ?
            $" Thank you so much for the prime sub!" :
            $" Thank you so much for the tier {subTier} {subDuration} month sub!";
        string subCommandsText = " Make sure to check out the available sub commands in the Social section below for your sub benefits @ https://www.twitch.tv/smoothdagger/about";
        await SendWebSocketMessage(
            $"PRIVMSG #{TwitchData.TwitchChannel} :{username}{subText}{subCommandsText}"
        );
    }

    private async void HandleChannelChatNotificationSubGift(
        TwitchWebSocketMessagePayloadEventChannelChatNotification @event
    )
    {
        var subGift = @event.sub_gift;
        bool isAnonymous = @event.chatter_is_anonymous;
        int subGiftCumulativeTotal = subGift.cumulative_total;
        int subGiftDuration = subGift.duration_months;
        int subGiftTier = int.Parse(
            subGift.sub_tier[0].ToString()
        );
        string username = isAnonymous ? "@Anonymous" : $"@{@event.chatter_user_name}";
        string usernameRecipient = $"@{subGift.recipient_user_name}";
        string subGiftText = $" Thank you so much for the tier {subGiftTier} {subGiftDuration} month gifted sub to @{usernameRecipient}!";
        string subGiftMonthsText = isAnonymous ? string.Empty : $"{username} has gifted a total of {subGiftCumulativeTotal} sub{(subGiftCumulativeTotal > 1u ? "s" : string.Empty)}!";
        string subCommandsText = " Make sure to check out the available sub commands in the Social section below for your sub benefits @ https://www.twitch.tv/smoothdagger/about";
        await SendWebSocketMessage(
            $"PRIVMSG #{TwitchData.TwitchChannel} :{username}{subGiftText}{subGiftMonthsText}{subCommandsText}"
        );
    }

    private void HandleWebSocketMessage(
        string message
    )
    {
        ParseWebSocketMessage(
            message,
            out List<WebSocketMessage> webSocketMessages
        );

        foreach (var webSocketMessage in webSocketMessages)
        {
            switch (webSocketMessage.command)
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
                        webSocketMessage
                    );
                    break;

                case "PRIVMSG":
                    HandleWebSocketMessagePrivMsg(
                        webSocketMessage
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
            $"PONG :{webSocketMessage.text}"
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
            Time.GetTicksMsec()
        );

        ProcessChatMessage(
            webSocketMessage    
        );
        ProcessChatCommand(
            webSocketMessage
        );
    }

    private async void HandleWebsSocketMessageCommandOnCooldown(
        WebSocketMessage webSocketMessage,
        string commandText
    )
    {
        string message = $"{commandText} is currently on cooldown.";
        await SendWebSocketMessage(
            $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
        );
        AddBotChatMessage(
            message
        );
    }

    private async void HandleWebSocketMessagePrivMsgCommands(
        WebSocketMessage webSocketMessage
    )
    {
        string message = c_automatedMessages[AutomatedMessageType.Commands];
        await SendWebSocketMessage(
            $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
        );
        AddBotChatMessage(
            message
        );
    }

    private async void HandleWebSocketMessagePrivMsgCurrentSong(
        WebSocketMessage webSocketMessage
    )
    {
        string message = $"Current Song: '{m_audioManager.GetCurrentSongName()}'";
        await SendWebSocketMessage(
            $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
        );
        AddBotChatMessage(
            message
        );
    }

    private async void HandleWebSocketMessagePrivMsgDate(
        WebSocketMessage webSocketMessage    
    )
    {
        string text = webSocketMessage.text;
        text = text.Replace(
            c_commands[CommandType.Date],
            string.Empty
        );
        text = text.Replace(
            "\r\n", 
            string.Empty
        );
        if (
            string.IsNullOrEmpty(
                text
            )
        )
        {
            DateTime dateTime = DateTime.UtcNow;
            string message = $"The current date in UTC is {dateTime.ToString("d-MMM-yyyy")}.";
            message = message.Replace(
                '-',
                ' '
            );
            await SendWebSocketMessage(
                $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
            );
            AddBotChatMessage(
                message
            );
        }
        else
        {
            text = text.Replace(
                " ",
                string.Empty
            ).ToUpper();

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(
                text
            );
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow, 
                timeZoneInfo
            );
            string message = $"The current date in {text} is {dateTime.ToString("d-MMM-yyyy")}.";
            message = message.Replace(
                '-',
                ' '
            );
            await SendWebSocketMessage(
                $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
            );
            AddBotChatMessage(
                message
            );
        }
    }

    private async void HandleWebSocketMessagePrivMsgDiscord(
        WebSocketMessage webSocketMessage
    )
    {
        string message = c_automatedMessages[AutomatedMessageType.Discord];
        await SendWebSocketMessage(
            $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
        );
        AddBotChatMessage(
            message
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
            string username = channelFollower.user_login;
            if (username == webSocketMessage.username)
            {
                string utcNow = Time.GetDatetimeStringFromSystem(
                    true
                );
                DateLength dateLength = DateCalculator.CalculateTimeDifference(
                    channelFollower.followed_at,
                    utcNow
                );

                string followTime = string.Empty;
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
                    $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
                );
                AddBotChatMessage(
                    message
                );
                return;
            }
        }

        message = $"You are not currently following @SmoothDagger.";
        await SendWebSocketMessage(
            $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
        );
        AddBotChatMessage(
            message
        );
    }

    private async void HandleWebSocketMessagePrivMsgRequestSong(
        WebSocketMessage webSocketMessage
    )
    {
        string message = $"Current Song: '{m_audioManager.GetCurrentSongName()}'";
        await SendWebSocketMessage(
            $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
        );
        AddBotChatMessage(
            message
        );
    }

    private async void HandleWebSocketMessagePrivMsgRules(
        WebSocketMessage webSocketMessage
    )
    {
        string message = c_automatedMessages[AutomatedMessageType.Rules];
        await SendWebSocketMessage(
            $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
        );
        AddBotChatMessage(
            message
        );
    }

    private async void HandleWebSocketMessagePrivMsgSetPlaylist(
        WebSocketMessage webSocketMessage
    )
    {
        if (
            m_twitchChannelPointRewardsManager.HasRewardAvailable(
                webSocketMessage.username,
                ChannelPointRewardsType.CommandSetPlaylist
            )
        )
        {
            string subCommand = ParseTextSubCommand(
                webSocketMessage.text
            );

            var currentPlaylistName = m_audioManager.GetCurrentPlaylistName();
            if (subCommand == currentPlaylistName)
            {
                m_twitchManager.RefundCustomChannelPointReward(
                    ChannelPointRewardsType.CommandSetPlaylist,
                    webSocketMessage.tags["id"]
                );
                await SendWebSocketMessage(
                    $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :This playlist is already playing. Your points have been refunded. Please check for valid playlist names in the Channel Point Reward prompt."
                );
                return;
            }

            bool isSubCommandValid = false;
            PlaylistType targetPlaylistType = PlaylistType.Count;
            var playlistTypes = Enum.GetValues<PlaylistType>();
            foreach (var playlistType in playlistTypes)
            {
                if (playlistType == PlaylistType.Count)
                {
                    continue;
                }

                if (subCommand.ToLower() == playlistType.ToString().ToLower())
                {
                    isSubCommandValid = true;
                    targetPlaylistType = playlistType;
                    break;
                }
            }

            string id = m_twitchChannelPointRewardsManager.GetRewardId(
                webSocketMessage.username,
                ChannelPointRewardsType.CommandSetPlaylist
            );
            if (isSubCommandValid)
            {
                m_audioManager.QueuePlaylist(
                    targetPlaylistType
                );
                m_twitchManager.ClaimCustomChannelPointReward(
                    ChannelPointRewardsType.CommandSetPlaylist,
                    id
                );
                await SendWebSocketMessage(
                    $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :Playlist updated to '{targetPlaylistType}'."
                );
                c_commandTimers[CommandType.SetPlaylist] = c_commandCooldowns[CommandType.SetPlaylist];
            }
            else
            {
                m_twitchManager.RefundCustomChannelPointReward(
                    ChannelPointRewardsType.CommandSetPlaylist,
                    id
                );
                await SendWebSocketMessage(
                    $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :Invalid playlist name. Your points have been refunded. Please check for valid playlist names in the Channel Point Reward prompt."
                );
            }

            m_twitchChannelPointRewardsManager.ClaimReward(
                webSocketMessage.username,
                id
            );
        }
        else
        {
            await SendWebSocketMessage(
                $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :Claim Channel Point Reward 'Set Playlist' to use this command."
            );
        }
    }

    private async void HandleWebSocketMessagePrivMsgTextToSpeech(
        WebSocketMessage webSocketMessage
    )
    {
        string message;
        string username = webSocketMessage.username;
        if (username == TwitchData.AccountUsername)
        {
            string text = webSocketMessage.text;
            text = text.Replace(
                c_commands[CommandType.TextToSpeech] + ' ',
                string.Empty
            );
            m_audioManager.PlayTextToSpeech(
                text
            );
            return;
        }
        else if (
            m_subscribersWhoUsedTextToSpeech.Contains(
                username
            )
        )
        {
            message = $"You can only use the subscriber !tts command once per stream.";
            await SendWebSocketMessage(
                $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
            );
            AddBotChatMessage(
                message
            );
            return;
        }

        var channelSubscribers = m_twitchManager.GetChannelSubscribers();
        foreach (var channelSubscriber in channelSubscribers)
        {
            string subscriberUsername = channelSubscriber.user_name;
            string subsciberUsernameAdjusted = subscriberUsername.ToLower();

            if (subsciberUsernameAdjusted == username)
            {
                string text = webSocketMessage.text;
                text = text.Replace(
                    c_commands[CommandType.TextToSpeech],
                    string.Empty
                );
                m_audioManager.PlayTextToSpeech(
                    text
                );
                m_subscribersWhoUsedTextToSpeech.Add(
                    username
                );
                c_commandTimers[CommandType.TextToSpeech] = c_commandCooldowns[CommandType.TextToSpeech];
                return;
            }
        }

        message = $"You must be subscribed in order to use this command.";
        await SendWebSocketMessage(
            $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
        );
        AddBotChatMessage(
            message
        );
    }

    private async void HandleWebSocketMessagePrivMsgTime(
        WebSocketMessage webSocketMessage
    )
    {
        string text = webSocketMessage.text;
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
                text
            )
        )
        {
            DateTime dateTime = DateTime.UtcNow;
            string message = $"The current time in UTC is {dateTime.ToString("HH:mm:ss")}.";
            await SendWebSocketMessage(
                $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
            );
            AddBotChatMessage(
                message
            );
        }
        else
        {
            text = text.Replace(
                " ",
                string.Empty
            ).ToUpper();

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(
                text
            );
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                timeZoneInfo
            );
            string message = $"The current time in {text} is {dateTime.ToString("HH:mm:ss")}.";
            await SendWebSocketMessage(
                $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
            );
            AddBotChatMessage(
                message
            );
        }
    }

    private async void HandleWebSocketMessagePrivMsgYouTube(
        WebSocketMessage webSocketMessage
    )
    {
        string message = c_automatedMessages[AutomatedMessageType.YouTube];
        await SendWebSocketMessage(
            $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
        );
        AddBotChatMessage(
            message
        );
    }

    private bool IsChatCommandValid(
        CommandType commandType,
        string text
    )
    {
        const int twitchMessageDelimiterLength = 2;
        string commandText = c_commands[commandType];
        int commandLength = commandText.Length;
        switch (commandType)
        {
            case CommandType.Age:
            case CommandType.CurrentSong:
            case CommandType.Discord:
            case CommandType.Commands:
            case CommandType.FollowAge:
            case CommandType.Rules:
            case CommandType.YouTube:
                // \n\r
                return
                    text.Length - twitchMessageDelimiterLength == commandLength &&
                    string.Compare(
                        text.Substr(
                            0,
                            commandLength
                        ).ToLower(),
                        c_commands[commandType]
                    ) == 0;

            case CommandType.Date:
            case CommandType.Time:
                const int dateTimeExactLength = 9;
                const int dateTimeSpaceIndex = 5;
                const int dateTimeAbbreviationLength = 3;
                int textLength = text.Length - twitchMessageDelimiterLength;
                return 
                    (
                        textLength == commandLength &&
                        string.Compare(
                            text.Substr(
                                0,
                                commandLength
                            ).ToLower(),
                            c_commands[commandType]
                        ) == 0
                    ) ||
                    (
                        textLength == dateTimeExactLength &&
                        text[dateTimeSpaceIndex] == ' ' &&
                        string.Compare(
                            text.Substr(
                                0,
                                commandLength
                            ).ToLower(),
                            c_commands[commandType]
                        ) == 0 &&
                        TimeZones.IsTimeZoneAbbreviationValid(
                            text.Split(
                                ' '
                            )[1].Substr(
                                0, 
                                dateTimeAbbreviationLength
                            ).ToUpper()
                        )
                    );

            case CommandType.TextToSpeech:
                const int ttsMinimumLength = 6;
                const int ttsSpaceIndex = 4;
                const int ttsFirstCharacterIndex = 5;
                return 
                    text.Length >= ttsMinimumLength &&
                    text[ttsSpaceIndex] == ' ' &&
                    string.Compare(
                        text.Substr(
                            0,
                            commandLength
                        ).ToLower(),
                        c_commands[commandType]
                    ) == 0 &&
                    char.IsLetterOrDigit(
                        text[ttsFirstCharacterIndex]
                    );

            case CommandType.SetPlaylist:
            default:
                return false;
        }
    }

    private bool IsCommandAvailable(
        CommandType commandType
    )
    {
        return Mathf.IsEqualApprox(
            c_commandTimers[commandType],
            0d
        );
    }

    private bool IsTwitchBot(
        string username
    )
    {
        return username == c_twitchBotUsername;
    }

    private void OnChannelChatNotification(
        TwitchWebSocketMessagePayloadEventChannelChatNotification @event
    )
    {
        switch (@event.notice_type)
        {
            case "bits_badge_tier":
                HandleChannelChatNotificationBitsBadgeTier(
                    @event
                );
                break;

            case "charity_donation":
                HandleChannelChatNotificationCharityDonation(
                    @event
                );
                break;

            case "community_sub_gift":
                HandleChannelChatNotificationCommunitySubGift(
                    @event
                );
                break;

            case "gift_paid_upgrade":
                HandleChannelChatNotificationGiftPaidUpgrade(
                    @event
                );
                break;

            case "pay_it_forward":
                HandleChannelChatNotificationPayItForward(
                    @event
                );
                break;

            case "prime_paid_upgrade":
                HandleChannelChatNotificationPrimePaidUpgrade(
                    @event
                );
                break;

            case "raid":
                HandleChannelChatNotificationRaid(
                    @event
                );
                break;

            case "resub":
                HandleChannelChatNotificationResub(
                    @event
                );
                break;

            case "sub":
                HandleChannelChatNotificationSub(
                    @event
                );
                break;

            case "sub_gift":
                HandleChannelChatNotificationSubGift(
                    @event
                );
                break;

            case "announcement":
            case "unraid":
            default:
                return;
        }
    }

    private string ParseTextSubCommand(
        string text    
    )
    {
        string subCommand = string.Empty;

        int index = 0;
        while (text[index++] != ' ');

        while (index < text.Length)
        {
            subCommand += text[index++];
        }

        subCommand = subCommand.Remove(
            subCommand.Length - c_webSocketMessageDelimiterLength,
            c_webSocketMessageDelimiterLength
        );

        return subCommand;
    }

    private void ParseWebSocketMessage(
        string message,
        out List<WebSocketMessage> webSocketMessages
    )
    {
        webSocketMessages = new();

        int index = 0;
        while (index < message.Length)
        {
            WebSocketMessage webSocketMessage = new(
                new(),
                string.Empty,
                string.Empty,
                string.Empty
            );

            // parse tags
            if (message[index] == '@')
            {
                index++;
                while (true)
                {
                    string key = string.Empty;
                    while (message[index] != '=')
                    {
                        key += message[index++];
                    }
                    index++;

                    string value = string.Empty;
                    while (message[index] != ';' && message[index] != ' ')
                    {
                        value += message[index++];
                    }

                    webSocketMessage.tags.Add(
                        key,
                        value
                    );

                    if (message[index++] == ' ')
                    {
                        break;
                    }
                }
            }

            // parse username
            if (message[index] == ':')
            {
                while (message[index] != '@' && message[index] != ' ')
                {
                    index++;
                }
                if (message[index++] == '@')
                {
                    while (message[index] != '.')
                    {
                        webSocketMessage.username += message[index++];
                    }
                    while (message[index] != ' ')
                    {
                        index++;
                    }
                    index++;
                }
            }

            // parse command
            while (
                index < message.Length &&
                message[index] != ' '
            )
            {
                webSocketMessage.command += message[index++];
            }

            if (
                webSocketMessage.command.EndsWith(
                    c_webSocketMessagedelimiter
                )
            )
            {
                webSocketMessage.command = webSocketMessage.command.Remove(
                    webSocketMessage.command.Length - c_webSocketMessageDelimiterLength,
                    c_webSocketMessageDelimiterLength
                );
            }
            else
            {
                // parse extraneous information up to possible text message
                string parse = string.Empty;
                while (message[index] != ':')
                {
                    parse += message[index++];
                    if (
                        parse.EndsWith(
                            c_webSocketMessagedelimiter
                        )
                    )
                    {
                        break;
                    }
                }

                // parse text message up to delimiter
                if (
                    !parse.EndsWith(
                        c_webSocketMessagedelimiter
                    )
                )
                {
                    index++;

                    while (index < message.Length)
                    {
                        if (
                            webSocketMessage.text.EndsWith(
                                c_webSocketMessagedelimiter
                            )
                        )
                        {
                            webSocketMessage.text = webSocketMessage.text.Remove(
                                webSocketMessage.text.Length - c_webSocketMessageDelimiterLength,
                                c_webSocketMessageDelimiterLength
                            );
                            break;
                        }

                        webSocketMessage.text += message[index++];
                    }
                }
            }

            webSocketMessages.Add(
                webSocketMessage
            );
        }
    }

    private async void ProcessChatCommand(
        WebSocketMessage webSocketMessage    
    )
    {
        string text = webSocketMessage.text;
        if (text[0] == '!')
        {
            foreach (var command in c_commands)
            {
                CommandType commandType = command.Key;
                if (
                    IsChatCommandValid(
                        commandType,
                        text
                    )
                )
                {
                    string commandText = c_commands[commandType];
                    text = text.Replace(
                        commandText,
                        string.Empty
                    );

                    if (
                        !IsCommandAvailable(
                            commandType
                        )
                    )
                    {
                        HandleWebsSocketMessageCommandOnCooldown(
                            webSocketMessage,
                            commandText
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
                                    webSocketMessage
                                );
                                break;

                            case CommandType.Commands:
                                HandleWebSocketMessagePrivMsgCommands(
                                    webSocketMessage
                                );
                                break;

                            case CommandType.CurrentSong:
                                HandleWebSocketMessagePrivMsgCurrentSong(
                                    webSocketMessage
                                );
                                break;

                            case CommandType.Date:
                                HandleWebSocketMessagePrivMsgDate(
                                    webSocketMessage
                                );
                                break;

                            case CommandType.Discord:
                                HandleWebSocketMessagePrivMsgDiscord(
                                    webSocketMessage
                                );
                                break;

                            case CommandType.RequestSong:
                                HandleWebSocketMessagePrivMsgRequestSong(
                                    webSocketMessage
                                );
                                break;

                            case CommandType.Rules:
                                HandleWebSocketMessagePrivMsgRules(
                                    webSocketMessage
                                );
                                break;

                            case CommandType.SetPlaylist:
                                HandleWebSocketMessagePrivMsgSetPlaylist(
                                    webSocketMessage
                                );
                                break;

                            case CommandType.SkipSong:
                                break;

                            case CommandType.TextToSpeech:
                                HandleWebSocketMessagePrivMsgTextToSpeech(
                                    webSocketMessage
                                );
                                break;

                            case CommandType.Time:
                                HandleWebSocketMessagePrivMsgTime(
                                    webSocketMessage    
                                );
                                break;

                            case CommandType.YouTube:
                                HandleWebSocketMessagePrivMsgYouTube(
                                    webSocketMessage
                                );
                                break;

                            default:
                                break;
                        }
                    }

                    return;
                }
            }

            string message = "This is not a valid chat command. Check the Social section below to see the available bot commands @ https://www.twitch.tv/SmoothDagger/About";
            await SendWebSocketMessage(
                $"@reply-parent-msg-id={webSocketMessage.tags["id"]} PRIVMSG #{TwitchData.TwitchChannel} :{message}"
            );
            AddBotChatMessage(
                message
            );
        }
    }

    private void ProcessChatMessage(
        WebSocketMessage webSocketMessage
    )
    {
        // process message for on-screen chat
        string username = webSocketMessage.username;
        bool isSubscriber = webSocketMessage.tags["subscriber"].ToInt() > 0u;
        string color;
        if (isSubscriber)
        {
            color = string.Empty;
        }
        else if (
            m_usernameColors.ContainsKey(
                username
            )
        )
        {
            color = m_usernameColors[username];
        }
        else
        {
            color = m_pastelInterpolator.GetColorAsHex();
            m_usernameColors.Add(
                username,
                color
            );
        }
        string name = webSocketMessage.tags["display-name"];
        if (name.ToLower() != username)
        {
            name += $" ({webSocketMessage.username})";
        }
        string message = webSocketMessage.text;
        string emotes = webSocketMessage.tags.ContainsKey(
            "emotes"
        ) ? webSocketMessage.tags["emotes"] : string.Empty;

        m_twitchChatManager.AddTwitchChatMessage(
            name,
            color,
            message,
            emotes
        );
    }

    private void RetrieveResources()
    {
        m_audioManager = GetNode<AudioManager>(
            NodeDirectory.NodePaths[NodeType.AudioManager]
        );
        m_pastelInterpolator = GetNode<PastelInterpolator>(
            NodeDirectory.NodePaths[NodeType.PastelInterpolator]
        );
        m_twitchChannelPointRewardsManager = GetNode<TwitchChannelPointRewardsManager>(
            NodeDirectory.NodePaths[NodeType.TwitchChannelPointRewardsManager]
        );
        m_twitchChatManager = GetNode<TwitchChatManager>(
            NodeDirectory.NodePaths[NodeType.TwitchChatManager]
        );
        m_twitchManager = GetNode<TwitchManager>(
            NodeDirectory.NodePaths[NodeType.TwitchManager]
        );

        m_twitchManager.ChannelChatNotification += OnChannelChatNotification;
    }

    private async void SendAutomatedMessage()
    {
        string message = c_automatedMessages[m_currentAutomatedMessage];
        await SendWebSocketMessage(
            $"PRIVMSG #{TwitchData.TwitchChannel} :{message}"
        );
        AddBotChatMessage(
            message
        );
    }

    private async Task SendWebSocketMessage(
        string message
    )
    {
        await m_webSocket.SendAsync(
            Encoding.UTF8.GetBytes(
                message
            ),
            WebSocketMessageType.Text,
            true,
            default
        );
    }

    private async void StartWebSocketAutomatedMessageDispatcher()
    {
        await Task.Run(
            async () =>
            {
#if DEBUG
                GD.Print(
                    $"{nameof(TwitchBot)}.{nameof(StartWebSocketAutomatedMessageDispatcher)}() - Web socket automated message dispatcher starting."
                );
#endif

                // 10 minute delay before next message is sent
                const int delayInMilliseconds = 600000;
                while (!m_shutdown)
                {
                    if (m_webSocket.State == WebSocketState.Open)
                    {
                        await Task.Delay(
                            delayInMilliseconds
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
            async () =>
            {
#if DEBUG
                GD.Print(
                    $"{nameof(TwitchBot)}.{nameof(StartWebSocketMessageReader)}() - Web socket message reader starting."
                );
#endif

                while (!m_shutdown)
                {
                    if (m_webSocket.State == WebSocketState.Open)
                    {
                        var bytes = new byte[c_maxPacketSize];
                        var result = await m_webSocket.ReceiveAsync(
                            bytes,
                            default
                        );

                        HandleWebSocketMessage(
                            Encoding.UTF8.GetString(
                                bytes,
                                0,
                                result.Count
                            )
                        );
                    }
                }
            }
        );
    }
}