
namespace Overlay
{
	using Godot;
	using System.Collections.Generic;
	using System.Runtime.Versioning;
    using System.Text.RegularExpressions;
    using NodeType = NodeDirectory.NodeType;

    [SupportedOSPlatform(platformName: "windows")]
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
			string userName,
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
				var isTwitchChatMessageLegal = IsTwitchChatMessageIllegal(
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
                var customUserData = m_twitchManager.GetCustomUserData(
                    userName: userName
                );
                if (customUserData is not null)
                {
					var customTextColor = customUserData.CustomTextColor;
					if (
						PastelInterpolator.IsColorHexTheRainbowColorType(
							hexCode: customTextColor
						) is true
					)
					{
						messageColor = PastelInterpolator.GetRainbowColorTag();
					}
					else
					{
						messageColor = $"[color={customTextColor}]";
                    }
                }
            }

			var twitchChatMessageData = new TwitchChatMessageData(
				name: name,
				nameColor: nameColor,
				message: message,
				messageColor: messageColor,
				emotes: emotes,
				badges: badges,
				isSubscriber: isSubscriber,
				isSmoothGPT: isSmoothGPT
			);
            lock (m_pendingTwitchChatMessagesLock)
			{
                m_pendingTwitchChatMessageDatas.Enqueue(
				    item: twitchChatMessageData
                );
            }
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

        private const int c_maxPixelCount = 420;
		private const int c_pixelSpacing = 2;
		private const string c_illegalBbCodePattern =
			$"\\[(" +
			$"alm" +
            $"|b" +
            $"|bgcolor" +
            $"|cell" +
            $"|center" +
            $"|code" +
            $"|color" +
			$"|dropcap" +
			$"|fade" +
			$"|fgcolor" +
			$"|fill" +
			$"|font" +
			$"|font_size" +
			$"|fsi" +
			$"|hint" +
			$"|i" +
			$"|img" +
			$"|indent" +
			$"|lb" +
			$"|left" +
			$"|lre" +
			$"|lri" +
			$"|lrm" +
			$"|lro" +
			$"|ol" +
			$"|opentype_features" +
			$"|outline_color" +
			$"|outline_size" +
			$"|p" +
			$"|pdf" +
			$"|pdi" +
			$"|rainbow" +
			$"|rb" +
			$"|right" +
			$"|rle" +
			$"|rli" +
			$"|rlm" +
			$"|rlo" +
			$"|s" +
			$"|shake" +
			$"|shy" +
			$"|table" +
			$"|tornado" +
			$"|u" +
			$"|ul" +
			$"|url" +
			$"|wave" +
			$"|wj" +
			$"|zwj" +
			$"|zwnj" +
			$")[^\\]]*\\]";

        private readonly Queue<TwitchChatMessageData> m_pendingTwitchChatMessageDatas = new();
        private readonly Queue<TwitchChatMessage> m_displayedTwitchChatMessages = new();
        private readonly Queue<TwitchChatMessage> m_queuedTwitchChatMessages = new();

        private readonly object m_displayedTwitchChatMessagesLock = new();
        private readonly object m_pendingTwitchChatMessagesLock = new();
        private readonly object m_queuedTwitchChatMessagesLock = new();

        private Control m_chatPivot = null;
		private HttpManager m_httpManager = null;
		private PastelInterpolator m_pastelInterpolator = null;
		private TwitchManager m_twitchManager = null;
		private int m_currentPixel = 0;

        private static bool IsTwitchChatMessageIllegal(
			string message
        )
		{
            var match = Regex.Match(
                input: message,
                pattern: c_illegalBbCodePattern,
                options: RegexOptions.IgnoreCase
            );

            return
				match is not null &&
                match.Success is true;
		}

        private void OnTwitchChatMessageDestroyed()
		{
			TwitchChatMessage oldestTwitchChatMessage;
            lock (m_displayedTwitchChatMessagesLock)
			{
                oldestTwitchChatMessage = m_displayedTwitchChatMessages.Dequeue();
            }

            var oldestLabelHeight = oldestTwitchChatMessage.GetLabelHeightInPixels() + c_pixelSpacing;
            m_currentPixel -= oldestLabelHeight;

            lock (m_displayedTwitchChatMessagesLock)
			{ 
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
		}

		private void OnTwitchChatMessageGenerated(
			TwitchChatMessage twitchChatMessage
		)
		{
			lock (m_queuedTwitchChatMessagesLock)
			{
                m_queuedTwitchChatMessages.Enqueue(
				    item: twitchChatMessage
				);
            }
		}

        private void ProcessQueuedTwitchChatMessage()
		{
            TwitchChatMessage twitchChatMessage = null;
            lock (m_queuedTwitchChatMessagesLock)
            {
                if (m_queuedTwitchChatMessages.Count > 0u)
                {
                    twitchChatMessage = m_queuedTwitchChatMessages.Dequeue();
                }
            }
            if (twitchChatMessage is not null)
			{
				twitchChatMessage.Position = new(
					x: 0u,
					y: m_currentPixel
				);
				twitchChatMessage.ShowLabel();

                lock (m_displayedTwitchChatMessagesLock)
                {
                    m_displayedTwitchChatMessages.Enqueue(
                        item: twitchChatMessage
                    );
                }

				var labelHeight = twitchChatMessage.GetLabelHeightInPixels();
				m_currentPixel = m_currentPixel + labelHeight + c_pixelSpacing;
				while (m_currentPixel > c_maxPixelCount)
				{
                    TwitchChatMessage oldestTwitchChatMessage;
                    lock (m_displayedTwitchChatMessagesLock)
					{
                        oldestTwitchChatMessage = m_displayedTwitchChatMessages.Dequeue();
                    }

					var oldestLabelHeight = oldestTwitchChatMessage.GetLabelHeightInPixels() + c_pixelSpacing;
					m_currentPixel -= oldestLabelHeight;

					var offset = new Vector2(
						x: 0u,
						y: oldestLabelHeight
					);

                    lock (m_displayedTwitchChatMessagesLock)
                    {
                        foreach (var displayedTwitchChatMessage in m_displayedTwitchChatMessages)
                        {
                            displayedTwitchChatMessage.Position -= offset;
                        }
                    }

					oldestTwitchChatMessage.QueueFree();
				}
			}
		}

		private void ProcessQueuedTwitchChatMessageData()
		{
            TwitchChatMessageData? messageData = null;
            lock (m_pendingTwitchChatMessagesLock)
			{
                if (m_pendingTwitchChatMessageDatas.Count > 0u)
				{
                    messageData = m_pendingTwitchChatMessageDatas.Dequeue();
                }
            }

			if (messageData.HasValue is true)
			{
				var latestMessageData = messageData.Value;
                var twitchChatMessage = new TwitchChatMessage();
                m_chatPivot.AddChild(
                    node: twitchChatMessage
                );
                twitchChatMessage.Generated = OnTwitchChatMessageGenerated;
                twitchChatMessage.Destroyed = OnTwitchChatMessageDestroyed;
                twitchChatMessage.Generate(
                    httpManager: m_httpManager,
                    pastelInterpolator: m_pastelInterpolator,
                    name: latestMessageData.Name,
                    nameColor: latestMessageData.NameColor,
                    message: latestMessageData.Message,
                    messageColor: latestMessageData.MessageColor,
                    emotes: latestMessageData.Emotes,
                    badges: latestMessageData.Badges,
                    isSubscriber: latestMessageData.IsSubscriber,
                    isSmoothGPT: latestMessageData.IsSmoothGPT
                );
            }
		}

        private void RetrieveResources()
		{
			m_chatPivot = GetNode<Control>(
				path: "ChatPivot"
			);
			m_httpManager = GetNode<HttpManager>(
				path: NodeDirectory.NodePaths[key: NodeType.HttpManager]
			);
			m_pastelInterpolator = GetNode<PastelInterpolator>(
				path: NodeDirectory.NodePaths[key: NodeType.PastelInterpolator]
			);
			m_twitchManager = GetNode<TwitchManager>(
                path: NodeDirectory.NodePaths[key: NodeType.TwitchManager]
			);
		}
    }
}