namespace Overlay
{
	using Godot;
	using System.Collections.Generic;
	using NodeType = NodeDirectory.NodeType;

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
			string emotes
		)
		{
			m_pendingtwitchChatMessageDatas.Enqueue(
				item: new(
					name: name,
					color: color,
					message: message,
					emotes: emotes
				)
			);
		}

		private struct TwitchChatMessageData
		{
			public string Name = string.Empty;
			public string Color = string.Empty;
			public string Message = string.Empty;
			public string Emotes = string.Empty;

			public TwitchChatMessageData(
				string name,
				string color,
				string message,
				string emotes
			)
			{
				this.Name = name;
				this.Color = color;
				this.Message = message;
				this.Emotes = emotes;
			}
		};

		private const int c_maxPixelCount = 420;
		private const int c_pixelSpacing = 2;

        private readonly Queue<TwitchChatMessageData> m_pendingtwitchChatMessageDatas = new();
        private readonly Queue<TwitchChatMessage> m_displayedTwitchChatMessages = new();
        private readonly Queue<TwitchChatMessage> m_queuedTwitchChatMessages = new();

        private Control m_chatPivot = null;
		private HttpManager m_httpManager = null;
		private PastelInterpolator m_pastelInterpolator = null;
		private int m_currentPixel = 0;

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
				int labelHeight = twitchChatMessage.GetLabelHeightInPixels();
				m_currentPixel = m_currentPixel + labelHeight + c_pixelSpacing;
				while (m_currentPixel > c_maxPixelCount)
				{
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

					// destroy old chat message
					oldestTwitchChatMessage.QueueFree();
				}
			}
		}

		private void ProcessQueuedTwitchChatMessageData()
		{
			if (m_pendingtwitchChatMessageDatas.Count > 0u)
			{
				var messageData = m_pendingtwitchChatMessageDatas.Dequeue();
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
					emotes: messageData.Emotes
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