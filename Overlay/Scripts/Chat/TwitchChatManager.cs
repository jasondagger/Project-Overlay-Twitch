namespace Overlay
{
	using Godot;
	using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using NodeType = NodeDirectory.NodeType;

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public sealed partial class TwitchChatManager : Node
	{
		public override void _Ready()
		{
			RetrieveResources();
		}

		public override void _Process(
			double elapsed
		)
		{
			ProcessQueuedTwitchChatMessage();
			ProcessQueuedTwitchChatMessageData();
		}

		public void AddTwitchChatMessage(
			string username,
			string name,
			string nameColor,
			string message,
			string emotes,
			string badges,
			bool isSmoothGPT
		)
		{
			if (isSmoothGPT is false)
			{
				var isTwitchChatMessageLegal = DoesTwitchChatMessageContainIllegalBbCode(
					message: message
				) is false;
				if (isTwitchChatMessageLegal is false)
				{
					return;
				}
			}

			var messageColor = string.Empty;
            var isSubscriber = string.IsNullOrEmpty(
                value: nameColor
            ) is true;
            if (isSubscriber is true)
            {
				var customSubscriberData = m_twitchManager.GetCustomSubscriberData(
					username: username
				);
				if (customSubscriberData is not null)
				{
                    messageColor = $"[color={customSubscriberData.CustomTextColor}]";
                }
            }

            m_pendingTwitchChatMessageDatas.Enqueue(
                item: new(
                    name: name,
                    nameColor: nameColor,
                    message: message,
					messageColor: messageColor,
                    emotes: emotes,
                    badges: badges,
                    isSubscriber: isSubscriber,
                    isSmoothGPT: isSmoothGPT
                )
            );
        }

		private struct TwitchChatMessageData
		{
			public string Name = string.Empty;
			public string NameColor = string.Empty;
			public string Message = string.Empty;
			public string MessageColor = string.Empty;
			public string Emotes = string.Empty;
			public string Badges = string.Empty;
			public bool IsSubscriber = false;
            public bool IsSmoothGPT = false;

            public TwitchChatMessageData(
				string name,
				string nameColor,
				string message,
				string messageColor,
				string emotes,
				string badges,
				bool isSubscriber,
				bool isSmoothGPT
			)
			{
				this.Name = name;
				this.NameColor = nameColor;
				this.Message = message;
				this.MessageColor = messageColor;
                this.Emotes = emotes;
				this.Badges = badges;
				this.IsSubscriber = isSubscriber;
				this.IsSmoothGPT = isSmoothGPT;
            }
        };

        private static readonly HashSet<string> c_illegalBbCodes = new()
        {
            "alm",
            "b",
            "bgcolor",
            "cell",
            "center",
            "code",
            "color",
            "dropcap",
            "fade",
            "fgcolor",
            "fill",
            "font",
            "font_size",
            "fsi",
            "hint",
            "i",
            "img",
            "indent",
            "lb",
            "left",
            "lre",
            "lri",
            "lrm",
            "lro",
            "ol",
            "opentype_features",
            "outline_color",
            "outline_size",
            "p",
            "pdf",
            "pdi",
            "rainbow",
            "rb",
            "right",
            "rle",
            "rli",
            "rlm",
            "rlo",
            "s",
            "shake",
            "shy",
            "table",
            "tornado",
            "u",
            "ul",
            "url",
            "wave",
            "wj",
            "zwj",
            "zwnj",
        };

        private const int c_maxPixelCount = 420;
		private const int c_pixelSpacing = 2;

        private readonly Queue<TwitchChatMessageData> m_pendingTwitchChatMessageDatas = new();
        private readonly Queue<TwitchChatMessage> m_displayedTwitchChatMessages = new();
        private readonly Queue<TwitchChatMessage> m_queuedTwitchChatMessages = new();

        private Control m_chatPivot = null;
		private HttpManager m_httpManager = null;
		private PastelInterpolator m_pastelInterpolator = null;
		private TwitchManager m_twitchManager = null;
		private int m_currentPixel = 0;

		private static bool DoesTwitchChatMessageContainIllegalBbCode(
			string message
        )
		{
			foreach (var bbCode in c_illegalBbCodes)
			{
				var pattern = $"\\[{bbCode}[^\\]]*\\]";
                var match = Regex.Match(
					input: message, 
					pattern: pattern,
					options: RegexOptions.IgnoreCase
				);
				if (
					match is not null && 
					match.Success is true
				)
				{
					return true;
				}
			}

			return false;
		}

		private void OnTwitchChatMessageDestroyed()
		{
			// remove oldest message
			var oldestTwitchChatMessage = m_displayedTwitchChatMessages.Dequeue();
			int oldestLabelHeight = oldestTwitchChatMessage.GetLabelHeightInPixels() + c_pixelSpacing;
			m_currentPixel -= oldestLabelHeight;

			// adjust position of other messages
			foreach (var displayedTwitchChatMessage in m_displayedTwitchChatMessages)
			{
				var position = displayedTwitchChatMessage.Position;
				position -= new Vector2(
					x: 0u,
					y: oldestLabelHeight
				);

				displayedTwitchChatMessage.Position = position;
			}
		}

		private void OnTwitchChatMessageGenerated(
			TwitchChatMessage twitchChatMessage
		)
		{
			m_queuedTwitchChatMessages.Enqueue(
				item: twitchChatMessage
			);
		}

		private void ProcessQueuedTwitchChatMessage()
		{
			if (m_queuedTwitchChatMessages.Count > 0u)
			{
                // move newest message
                var twitchChatMessage = m_queuedTwitchChatMessages.Dequeue();
				twitchChatMessage.Position = new Vector2(
					x: 0u,
					y: m_currentPixel
				);
				twitchChatMessage.ShowLabel();

				m_displayedTwitchChatMessages.Enqueue(
					twitchChatMessage
				);

				// move messages upward & remove old messages
				var labelHeight = twitchChatMessage.GetLabelHeightInPixels();
				m_currentPixel = m_currentPixel + labelHeight + c_pixelSpacing;
				while (m_currentPixel > c_maxPixelCount)
				{
					var oldestTwitchChatMessage = m_displayedTwitchChatMessages.Dequeue();
					var oldestLabelHeight = oldestTwitchChatMessage.GetLabelHeightInPixels() + c_pixelSpacing;
					m_currentPixel -= oldestLabelHeight;

					// adjust position of other messages
					var offset = new Vector2(
						x: 0u,
						y: oldestLabelHeight
					);
                    foreach (var displayedTwitchChatMessage in m_displayedTwitchChatMessages)
					{
						displayedTwitchChatMessage.Position -= offset;
					}

					// destroy old chat message
					oldestTwitchChatMessage.QueueFree();
				}
			}
		}

		private void ProcessQueuedTwitchChatMessageData()
		{
			if (m_pendingTwitchChatMessageDatas.Count > 0u)
			{
				var messageData = m_pendingTwitchChatMessageDatas.Dequeue();
				var twitchChatMessage = new TwitchChatMessage();
				m_chatPivot.AddChild(
					node: twitchChatMessage
				);
				twitchChatMessage.Generated = OnTwitchChatMessageGenerated;
				twitchChatMessage.Destroyed = OnTwitchChatMessageDestroyed;
				twitchChatMessage.Generate(
					httpManager: m_httpManager,
					pastelInterpolator: m_pastelInterpolator,
					name: messageData.Name,
					nameColor: messageData.NameColor,
					message: messageData.Message,
					messageColor: messageData.MessageColor,
                    emotes: messageData.Emotes,
					badges: messageData.Badges,
					isSubscriber: messageData.IsSubscriber,
					isSmoothGPT: messageData.IsSmoothGPT
				);
			}
		}

		private void RetrieveResources()
		{
			m_chatPivot = GetNode<Control>(
				path: "ChatPivot"
			);
			m_httpManager = GetNode<HttpManager>(
				path: NodeDirectory.NodePaths[NodeType.HttpManager]
			);
			m_pastelInterpolator = GetNode<PastelInterpolator>(
				path: NodeDirectory.NodePaths[NodeType.PastelInterpolator]
			);
			m_twitchManager = GetNode<TwitchManager>(
                path: NodeDirectory.NodePaths[NodeType.TwitchManager]
			);
		}
	}
}