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
			string name,
			string color,
			string message,
			string emotes,
			string badges,
			bool isSmoothGPT
		)
		{
			var isSubscriber = string.IsNullOrEmpty(
				value: color
			) is true;
			var isTwitchChatMessageLegal = DoesTwitchChatMessageContainIllegalBbCode(
				message: message,
				isSubscriber: isSubscriber
			) is false;
			if (isTwitchChatMessageLegal is true)
			{
				// todo: insert subscriber color
				if (isSubscriber is true)
				{

				}

				m_pendingTwitchChatMessageDatas.Enqueue(
					item: new(
						name: name,
						color: color,
						message: message,
						emotes: emotes,
						badges: badges,
                        isSubscriber: isSubscriber,
						isSmoothGPT: isSmoothGPT
                    )
				);
			}
		}

		private struct TwitchChatMessageData
		{
			public string Name = string.Empty;
			public string Color = string.Empty;
			public string Message = string.Empty;
			public string Emotes = string.Empty;
			public string Badges = string.Empty;
			public bool IsSubscriber = false;
            public bool IsSmoothGPT = false;

            public TwitchChatMessageData(
				string name,
				string color,
				string message,
				string emotes,
				string badges,
				bool isSubscriber,
				bool isSmoothGPT
			)
			{
				this.Name = name;
				this.Color = color;
				this.Message = message;
				this.Emotes = emotes;
				this.Badges = badges;
				this.IsSubscriber = isSubscriber;
				this.IsSmoothGPT = isSmoothGPT;
            }
        };

        private static readonly HashSet<string> c_bbCodesForSubscribers = new()
        {
            "b",
            "bgcolor",
            "color",
            "i",
            "s",
            "u",
        };
        private static readonly HashSet<string> c_bbCodesMarkedIllegal = new()
        {
            "alm",
            "cell",
            "center",
            "code",
            "dropcap",
            "fade",
            "fgcolor",
            "fill",
            "font",
            "font_size",
            "fsi",
            "hint",
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
            "shake",
            "shy",
            "table",
            "tornado",
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
		private int m_currentPixel = 0;

		private static bool DoesTwitchChatMessageContainIllegalBbCode(
			string message,
			bool isSubscriber
        )
		{
			foreach (var bbCode in c_bbCodesMarkedIllegal)
			{
				var pattern = GetBbCodeRegexPattern(
					bbCode: bbCode
				);
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
			if (isSubscriber is false)
			{
                foreach (var bbCode in c_bbCodesForSubscribers)
                {
                    var pattern = GetBbCodeRegexPattern(
                        bbCode: bbCode
                    );
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
            }

			return false;
		}

		private static string GetBbCodeRegexPattern(
			string bbCode	
		)
		{
			return $"\\[{bbCode}[^\\]]*\\]";
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
					color: messageData.Color,
					message: messageData.Message,
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
		}
	}
}