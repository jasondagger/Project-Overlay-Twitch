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
            new(
                name,
                color,
                message,
                emotes
            )
        );
    }

    private struct TwitchChatMessageData
    {
        public string name;
        public string color;
        public string message;
        public string emotes;

        public TwitchChatMessageData(
            string name,
            string color,
            string message,
            string emotes
        )
        {
            this.name = name;
            this.color = color;
            this.message = message;
            this.emotes = emotes;
        }
    };

    private const int c_maxPixelCount = 420;
    private const int c_pixelSpacing = 2;

    private Control m_chatPivot = null;
    private HttpManager m_httpManager = null;
    private PastelInterpolator m_pastelInterpolator = null;

    private Queue<TwitchChatMessageData> m_pendingtwitchChatMessageDatas = new();
    private Queue<TwitchChatMessage> m_displayedTwitchChatMessages = new();
    private Queue<TwitchChatMessage> m_queuedTwitchChatMessages = new();
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
            Vector2 position = displayedTwitchChatMessage.Position;

            position -= new Vector2(
                0u,
                oldestLabelHeight
            );

            displayedTwitchChatMessage.Position = position;
        }
    }

    private void OnTwitchChatMessageGenerated(
        TwitchChatMessage twitchChatMessage
    )
    {
        m_queuedTwitchChatMessages.Enqueue(
            twitchChatMessage
        );
    }

    private void ProcessQueuedTwitchChatMessage()
    {
        if (m_queuedTwitchChatMessages.Count > 0u)
        {
            TwitchChatMessage twitchChatMessage = m_queuedTwitchChatMessages.Dequeue();
            // move newest message
            twitchChatMessage.Position = new Vector2(
                0u,
                m_currentPixel
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
                    Vector2 position = displayedTwitchChatMessage.Position;

                    position -= new Vector2(
                        0u,
                        oldestLabelHeight
                    );

                    displayedTwitchChatMessage.Position = position;
                }

                // destroy old chat mesasge
                oldestTwitchChatMessage.QueueFree();
            }
        }
    }

    private void ProcessQueuedTwitchChatMessageData()
    {
        if (m_pendingtwitchChatMessageDatas.Count > 0u)
        {
            var messageData = m_pendingtwitchChatMessageDatas.Dequeue();

            TwitchChatMessage twitchChatMessage = new();
            m_chatPivot.AddChild(
                twitchChatMessage
            );
            twitchChatMessage.Generated = OnTwitchChatMessageGenerated;
            twitchChatMessage.Destroyed = OnTwitchChatMessageDestroyed;
            twitchChatMessage.Generate(
                m_httpManager,
                m_pastelInterpolator,
                messageData.name,
                messageData.color,
                messageData.message,
                messageData.emotes
            );
        }
    }

    private void RetrieveResources()
    {
        m_chatPivot = GetNode<Control>(
            "ChatPivot"
        );
        m_httpManager = GetNode<HttpManager>(
            NodeDirectory.NodePaths[NodeType.HttpManager]
        );
        m_pastelInterpolator = GetNode<PastelInterpolator>(
            NodeDirectory.NodePaths[NodeType.PastelInterpolator]
        );
    }
}