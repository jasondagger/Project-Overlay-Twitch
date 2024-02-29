using Godot;
using System;
using System.Collections.Generic;
using NodeType = NodeDirectory.NodeType;

public partial class TwitchChannelPointRewardsManager : Node
{
    public override void _EnterTree()
    {
        SubscribeToTwitchEvents();
    }

    public enum ChannelPointRewardsType : uint
    {
        CommandRequestSong = 0u,

        IRLHydrate,
        IRLNoCursing,
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

        TextToSpeech,
    }

    public struct ChannelPointRewardData
    {
        public ChannelPointRewardsType twitchChannelPointRewardsType = ChannelPointRewardsType.SoundAlertApplause;
        public string id = string.Empty;

        public ChannelPointRewardData(
            ChannelPointRewardsType twitchChannelPointRewardsType,
            string id
        )
        {
            this.twitchChannelPointRewardsType = twitchChannelPointRewardsType;
            this.id = id;
        }
    };

    public struct ChannelPointReward
    {
        public string background_color = string.Empty;
        public long cost = 0;
        public long global_cooldown_seconds = 0;
        public bool is_enabled = false;
        public bool is_global_cooldown_enabled = false;
        public bool is_max_per_stream_enabled = false;
        public bool is_max_per_user_per_stream_enabled = false;
        public bool is_paused = false;
        public bool is_user_input_required = false;
        public long max_per_stream = 0;
        public long max_per_user_per_stream = 0;
        public string prompt = string.Empty;
        public bool should_redemptions_skip_request_queue = false;
        public string title = string.Empty;

        public ChannelPointReward(
            string background_color,
            long cost,
            long global_cooldown_seconds,
            bool is_enabled,
            bool is_global_cooldown_enabled,
            bool is_max_per_stream_enabled,
            bool is_max_per_user_per_stream_enabled,
            bool is_paused,
            bool is_user_input_required,
            long max_per_stream,
            long max_per_user_per_stream,
            string prompt,
            bool should_redemptions_skip_request_queue,
            string title
        )
        {
            this.background_color = background_color;
            this.cost = cost;
            this.global_cooldown_seconds = global_cooldown_seconds;
            this.is_enabled = is_enabled;
            this.is_global_cooldown_enabled = is_global_cooldown_enabled;
            this.is_max_per_stream_enabled = is_max_per_stream_enabled;
            this.is_max_per_user_per_stream_enabled = is_max_per_user_per_stream_enabled;
            this.is_paused = is_paused;
            this.is_user_input_required = is_user_input_required;
            this.max_per_stream = max_per_stream;
            this.max_per_user_per_stream = max_per_user_per_stream;
            this.prompt = prompt;
            this.should_redemptions_skip_request_queue = should_redemptions_skip_request_queue;
            this.title = title;
        }
    };

    public readonly Dictionary<ChannelPointRewardsType, ChannelPointReward> TwitchChannelPointRewards = new()
    {
        {
            ChannelPointRewardsType.CommandRequestSong,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.Command],
                1000,
                0,
                false,
                false,
                false,
                false,
                false,
                false,
                0,
                0,
                string.Empty,
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.CommandRequestSong]
            )
        },
        {
            ChannelPointRewardsType.IRLHydrate,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.IRL],
                300,
                120,
                true,
                true,
                false,
                false,
                false,
                false,
                0,
                0,
                "Sips water..",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.IRLHydrate]
            )
        },
        {
            ChannelPointRewardsType.IRLNoCursing,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.IRL],
                1000,
                900,
                true,
                true,
                false,
                false,
                false,
                false,
                0,
                0,
                "Cursing is bad, don't do it.",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.IRLNoCursing]
            )
        },
        {
            ChannelPointRewardsType.IRLPostureCheck,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.IRL],
                300,
                300,
                true,
                true,
                false,
                false,
                false,
                false,
                0,
                0,
                "Good posture is good habit.",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.IRLPostureCheck]
            )
        },
        {
            ChannelPointRewardsType.IRLShowKitty,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.IRL],
                1500,
                0,
                true,
                false,
                false,
                false,
                false,
                false,
                0,
                0,
                "Show the cutest of kitties! Disclaimer: May appear on stream anyways.",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.IRLShowKitty]
            )
        },
        {
            ChannelPointRewardsType.IRLShowPuppy,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.IRL],
                1500,
                0,
                true,
                false,
                false,
                false,
                false,
                false,
                0,
                0,
                "Show the goodest of puppies! Disclaimer: May appear on stream anyways.",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.IRLShowPuppy]
            )
        },
        {
            ChannelPointRewardsType.IRLStreeeeeetch,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.IRL],
                300,
                300,
                true,
                true,
                false,
                false,
                false,
                false,
                0,
                0,
                "Good streeeeeetch is best streeeeeetch!",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.IRLStreeeeeetch]
            )
        },
        {
            ChannelPointRewardsType.SoundAlertApplause,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                300,
                0,
                true,
                false,
                false,
                false,
                false,
                false,
                0,
                0,
                "Nice round of applause for the chap on stream.",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertApplause]
            )
        },
        {
            ChannelPointRewardsType.SoundAlertFirstBlood,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                1,
                0,
                true,
                false,
                true,
                false,
                false,
                false,
                1,
                0,
                "First is the worst, though.",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertFirstBlood]
            )
        },
        {
            ChannelPointRewardsType.SoundAlertGodlike,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                500,
                0,
                true,
                false,
                false,
                false,
                false,
                false,
                0,
                0,
                "We're basically gods here.",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertGodlike]
            )
        },
        {
            ChannelPointRewardsType.SoundAlertHeartbeat,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                300,
                0,
                true,
                false,
                false,
                false,
                false,
                false,
                0,
                0,
                "The beat goes on.",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertHeartbeat]
            )
        },
        {
            ChannelPointRewardsType.SoundAlertHolyShit,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                500,
                0,
                true,
                false,
                false,
                false,
                false,
                false,
                0,
                0,
                "Even gods can poop.",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertHolyShit]
            )
        },
        {
            ChannelPointRewardsType.SoundAlertHowdy,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                50,
                0,
                true,
                false,
                false,
                true,
                false,
                false,
                0,
                1,
                "Howdy, friend!",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertHowdy]
            )
        },
        {
            ChannelPointRewardsType.SoundAlertKegExplosion,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                500,
                0,
                true,
                false,
                false,
                false,
                false,
                false,
                0,
                0,
                "Spontaneous exploderinos!",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertKegExplosion]
            )
        },
        {
            ChannelPointRewardsType.SoundAlertKegFuse,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                500,
                0,
                true,
                false,
                false,
                false,
                false,
                false,
                0,
                0,
                "The suspense is killing me!",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertKegFuse]
            )
        },
        {
            ChannelPointRewardsType.SoundAlertNice,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.SoundAlert],
                69,
                0,
                true,
                false,
                false,
                true,
                false,
                false,
                0,
                1,
                "Is nice, no?",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.SoundAlertNice]
            )
        },
        {
            ChannelPointRewardsType.TextToSpeech,
            new(
                c_channelRewardPointColorTypes[ChannelRewardPointColorType.TextToSpeech],
                2000,
                5,
                true,
                true,
                false,
                false,
                false,
                true,
                0,
                0,
                "What would you like to hear?",
                false,
                c_channelRewardPointNames[ChannelPointRewardsType.TextToSpeech]
            )
        },
    };

    public readonly Dictionary<ChannelPointRewardsType, Action> RedeemableRewardsWithNoInput = new()
    {
        { ChannelPointRewardsType.IRLHydrate,             null },
        { ChannelPointRewardsType.IRLNoCursing,           null },
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

    public readonly Dictionary<ChannelPointRewardsType, Action<string>> ReedambleRewardsWithStringInput = new()
    {
        { ChannelPointRewardsType.CommandRequestSong, null },
        { ChannelPointRewardsType.TextToSpeech,       null },
    };

    public void ClaimReward(
        string username,
        string id
    )
    {
        for (int i = 0; i < m_pendingUserRewards[username].Count; i++)
        {
            if (m_pendingUserRewards[username][i].id == id)
            {
                m_pendingUserRewards[username].RemoveAt(
                    i
                );

                if (m_pendingUserRewards[username].Count == 0)
                {
                    m_pendingUserRewards.Remove(
                        username
                    );
                }
                break;
            }
        }
    }

    public List<ChannelPointRewardData> GetPendingRewards()
    {
        List<ChannelPointRewardData> pendingChannelPointRewardDatas = new();

        foreach (var pendingUserRewards in m_pendingUserRewards)
        {
            pendingChannelPointRewardDatas.AddRange(
                pendingUserRewards.Value
            );
        }

        return pendingChannelPointRewardDatas;
    }

    public string GetRewardId(
        string username,
        ChannelPointRewardsType channelPointRewardsType
    )
    {
        foreach (var channelPointRewardData in m_pendingUserRewards[username])
        {
            if (channelPointRewardData.twitchChannelPointRewardsType == channelPointRewardsType)
            {
                return channelPointRewardData.id;
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
                username 
            ) 
        )
        {
            foreach (var channelPointRewardData in m_pendingUserRewards[username])
            {
                if (channelPointRewardData.twitchChannelPointRewardsType == channelPointRewardsType)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public ChannelPointRewardsType GetRewardTypeByName(
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
        TextToSpeech,
    }

    private static readonly Dictionary<ChannelRewardPointColorType, string> c_channelRewardPointColorTypes = new()
    {
        { ChannelRewardPointColorType.Command,      "#FFA9FF" },
        { ChannelRewardPointColorType.IRL,          "#FFFFA9" },
        { ChannelRewardPointColorType.SoundAlert,   "#A9FFFF" },
        { ChannelRewardPointColorType.TextToSpeech, "#FFA9A9" },
    };

    private static readonly Dictionary<ChannelPointRewardsType, string> c_channelRewardPointNames = new()
    {
        { ChannelPointRewardsType.CommandRequestSong,     "Command: Request Song"      },
        { ChannelPointRewardsType.IRLHydrate,             "IRL: Hydrate"               },
        { ChannelPointRewardsType.IRLNoCursing,           "IRL: No Cursing"            },
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
        { ChannelPointRewardsType.TextToSpeech,           "Text To Speech"             },
    };

    private Dictionary<string, List<ChannelPointRewardData>> m_pendingUserRewards = new();

    private void SubscribeToTwitchEvents()
	{
		var twitchManager = GetNode<TwitchManager>(
            NodeDirectory.NodePaths[NodeType.TwitchManager]
        );

        twitchManager.ChannelPointsCustomRewardRedeemed += OnChannelPointsCustomRewardRedemptionAdded;
	}

    private void OnChannelPointsCustomRewardRedemptionAdded(
        TwitchWebSocketMessagePayloadEventChannelPointsCustomRewardRedeemed @event
    )
    {
        switch (@event.reward.title)
        {
            case "Command: Request Song":
                ReedambleRewardsWithStringInput[ChannelPointRewardsType.CommandRequestSong]?.Invoke(
                    @event.user_input    
                );
                break;

            case "IRL: Hydrate":
                RedeemableRewardsWithNoInput[ChannelPointRewardsType.IRLHydrate]?.Invoke();
                break;

            case "IRL: No Cursing":
                RedeemableRewardsWithNoInput[ChannelPointRewardsType.IRLNoCursing]?.Invoke();
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

            case "Text To Speech":
                ReedambleRewardsWithStringInput[ChannelPointRewardsType.TextToSpeech]?.Invoke(
                    @event.user_input
                );
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
            !m_pendingUserRewards.ContainsKey(
                username
            )
        )
        {
            m_pendingUserRewards.Add(
                username,
                new()
            );
        }

        m_pendingUserRewards[username].Add(
            new(
                channelPointRewardsType,
                channelPointRewardsId
            )
        );
    }
}