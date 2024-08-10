
namespace Overlay
{
	using Godot;
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using System.Text.RegularExpressions;
    using NodeType = NodeDirectory.NodeType;
    using UILayoutType = UIManager.UILayoutType;

	[SupportedOSPlatform(platformName: "windows")]
    public sealed partial class TwitchChatManager : UILayoutObject
	{
		public override void _Ready()
		{
			base._Ready();

			RetrieveResources();
			PopulateTwitchChatMessageCache();
        }

		public override void _Process(
			double elapsed
		)
		{
			base._Process(
				elapsed: elapsed
			);

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
			lock (m_pendingTwitchChatMessageDatasLock)
			{
				m_pendingTwitchChatMessageDatas.Enqueue(
					item: twitchChatMessageData
				);
			}
		}

        protected override void HandleSwapToUILayoutToCode()
        {
            m_chatPivot.SetPosition(
                position: m_uiLayoutChatPivotPositions[key: UILayoutType.Code]
            );
        }

        protected override void HandleSwapToUILayoutToDefault()
        {
            m_chatPivot.SetPosition(
                position: m_uiLayoutChatPivotPositions[key: UILayoutType.Default]
            );
        }

        protected override void HandleSwapToUILayoutToMTG()
        {
            m_chatPivot.SetPosition(
                position: m_uiLayoutChatPivotPositions[key: UILayoutType.MTG]
            );
        }

        protected override void HandleSwapToUILayoutToTF2()
        {
            m_chatPivot.SetPosition(
                position: m_uiLayoutChatPivotPositions[key: UILayoutType.TF2]
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

		private const int c_twitchChatMessageCacheSize = 20;
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

		private readonly Dictionary<UILayoutType, Vector2> m_uiLayoutChatPivotPositions = new()
		{
			{ UILayoutType.Code,    new(x: 38, y: 1000) },
			{ UILayoutType.Default, new(x: 38, y: 912)  },
			{ UILayoutType.MTG,     new(x: 38, y: 1000) },
            { UILayoutType.TF2,	    new(x: 38, y: 912)  },
		};

		private readonly Queue<TwitchChatMessage> m_availableTwitchChatMessages = new();
        private readonly Queue<TwitchChatMessage> m_displayedTwitchChatMessages = new();
        private readonly Queue<TwitchChatMessage> m_queuedTwitchChatMessages = new();
        private readonly Queue<TwitchChatMessageData> m_pendingTwitchChatMessageDatas = new();

        private readonly object m_availableTwitchChatMessagesLock = new();
        private readonly object m_displayedTwitchChatMessagesLock = new();
        private readonly object m_pendingTwitchChatMessageDatasLock = new();
        private readonly object m_queuedTwitchChatMessagesLock = new();

        private Control m_chatPivot = null;
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

			RecycleTwitchChatMessage(
			    twitchChatMessage: oldestTwitchChatMessage
			);
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

		private void PopulateTwitchChatMessageCache()
		{
			for (var i = 0; i < c_twitchChatMessageCacheSize; i++)
			{
                var twitchChatMessage = new TwitchChatMessage
                {
                    Generated = OnTwitchChatMessageGenerated,
                    Destroyed = OnTwitchChatMessageDestroyed
                };

				m_chatPivot.AddChild(
				    node: twitchChatMessage
				);

                m_availableTwitchChatMessages.Enqueue(
					item: twitchChatMessage	
				);
            }
		}

        private void ProcessQueuedTwitchChatMessage()
		{
            TwitchChatMessage twitchChatMessage;
            lock (m_queuedTwitchChatMessagesLock)
            {
                if (m_queuedTwitchChatMessages.Count > 0u)
                {
                    twitchChatMessage = m_queuedTwitchChatMessages.Dequeue();
                }
				else
				{
					return;
				}
            }

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

				RecycleTwitchChatMessage(
					twitchChatMessage: oldestTwitchChatMessage	
				);
            }
		}

		private void ProcessQueuedTwitchChatMessageData()
		{
            TwitchChatMessageData messageData;
            lock (m_pendingTwitchChatMessageDatasLock)
			{
                if (m_pendingTwitchChatMessageDatas.Count > 0u)
				{
                    messageData = m_pendingTwitchChatMessageDatas.Dequeue();
                }
				else
				{
					return;
				}
            }

            TwitchChatMessage twitchChatMessage;
            lock (m_availableTwitchChatMessagesLock)
            {
                twitchChatMessage = m_availableTwitchChatMessages.Dequeue();
            }

            twitchChatMessage.Generate(
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

        private void RecycleTwitchChatMessage(
            TwitchChatMessage twitchChatMessage
        )
        {
            lock (m_availableTwitchChatMessagesLock)
            {
                m_availableTwitchChatMessages.Enqueue(
                    item: twitchChatMessage
                );
            }
			twitchChatMessage.Reset();
        }

        private void RetrieveResources()
		{
			m_chatPivot = GetNode<Control>(
				path: "ChatPivot"
			);
			m_twitchManager = GetNode<TwitchManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.TwitchManager
                )
            );
		}
    }
}