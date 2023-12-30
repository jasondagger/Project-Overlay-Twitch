using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using static Godot.HttpClient;

// max Twitch chat character limit = 500
// max Twitch username = 24

// emotesv2_450:10-20,30-40/emotesv2_420:60-75/emotesv2_69:4-9
// emotename:range,range/emotename:range/emotename:range,range

public sealed partial class TwitchChatMessage : Node2D
{
    public Action<TwitchChatMessage> Generated;
    public Action Destroyed;

    public override void _Process(
        double delta
    )
    {
        if (m_isSubscriber)
        {
            string color = m_pastelInterpolator.GetColorAsHex();
            m_richTextLabel.Text = m_text.Replace(
                c_labelSubscriberColor,
                color
            );
        }
        switch (m_generatedState)
        {
            case GeneratedState.Generating:
                break;
            case GeneratedState.Generated:
                Generated?.Invoke(
                    this    
                );
                m_generatedState = GeneratedState.Complete;
                break;
            case GeneratedState.Complete:
                HandleTextFade(
                    (float)delta
                );
                break;

            default:
                // shouldn't hit, something went wrong
                break;
        }
    }

    public void Generate(
        HttpManager httpManager,
        PastelInterpolator pastelInterpolator,
        string name,
        string color,
        string message,
        string emotes
    )
    {
        m_pastelInterpolator = pastelInterpolator;
        m_isSubscriber = string.IsNullOrEmpty(
            color
        );
        m_text =
            $"{c_labelFontSize}" +
            $"{c_labelOutlineColor}" +
            $"{c_labelOutlineSize}" +
            $"{c_labelNameFont}" +
            $"[color=#{(m_isSubscriber ? c_labelSubscriberColor : color)}]" +
            $"{name}" +
            $"[/color]" +
            $"[/font]" +
            $"  " +
            $"{c_labelMessageFont}" +
            $"{c_labelMessageColor}" +
            $"{message}";

        bool hasEmotes = !string.IsNullOrEmpty(
            emotes
        );
        if (hasEmotes)
        {
            // split each emote from twitch apis
            string[] emoteValues = emotes.Split(
                '/'
            );
            uint pngsToLoad = 0u;
            foreach (var emoteValue in emoteValues)
            {
                // split emote link & name ranges
                string[] emoteData = emoteValue.Split(
                    ':'
                );

                string emoteLink = emoteData[0u];
                // retrieve emote name within message string
                string[] emoteRanges = emoteData[1u].Split(
                    ','
                );
                string[] emoteIndices = emoteRanges[0u].Split(
                    '-'
                );
                int startIndex = emoteIndices[0u].ToInt();
                int endIndex = emoteIndices[1u].ToInt();
                string emoteName = message.Substring(
                    startIndex,
                    endIndex - startIndex + 1
                );
               
                string emotePath = $"{c_twitchEmoteDirectory}\\{emoteLink}.res";
                if (
                    File.Exists(
                        emotePath   
                    )
                )
                {
                    m_text = m_text.Replace(
                        emoteName,
                        $"[img]{emotePath}[/img]"
                    );
                }
                else
                {
                    pngsToLoad++;
                    httpManager.SendHttpRequest(
                        $"{c_twitchEmoteUrlPrefix}{emoteLink}{c_twitchEmoteUrlSuffix}",
                        null,
                        Method.Get,
                        string.Empty,
                        (
                            long result,
                            long responseCode,
                            string[] headers,
                            byte[] body
                        ) =>
                        {
                            // failed web request
                            if (responseCode >= 300u)
                            {
                                QueueFree();
                                return;
                            }

                            var image = Image.Create(
                                c_twitchEmoteWidth,
                                c_twitchEmoteHeight, 
                                false, 
                                Image.Format.Rgba8
                            );
                            image.LoadPngFromBuffer(
                                body
                            );
                            var imageTexture = ImageTexture.CreateFromImage(
                                image
                            );
                            ResourceSaver.Save(
                                imageTexture,
                                emotePath 
                            );

                            m_text = m_text.Replace(
                                emoteName,
                                $"[img]{emotePath}[/img]"
                            );

                            pngsToLoad--;
                            if (pngsToLoad == 0u)
                            {
                                GenerateRichTextLabel();
                            }
                        }
                    );
                }
            }

            if (pngsToLoad == 0u)
            {
                GenerateRichTextLabel();
            }
        }
        else
        {
            GenerateRichTextLabel();
        }
    }

    public int GetLabelHeightInPixels()
    {
        return m_richTextLabel.GetContentHeight();
    }

    public void ShowLabel()
    {
        m_richTextLabel.Visible = true;
    }

    private enum GeneratedState : uint
    {
        Generating = 0u,
        Generated,
        Complete,
    }

    private enum FadeState : uint
    {
        Visible = 0u,
        Fading
    }

    private const string c_twitchEmoteDirectory = "user://";
    private const string c_twitchEmoteUrlPrefix = "https://static-cdn.jtvnw.net/emoticons/v2/";
    private const string c_twitchEmoteUrlSuffix = "/static/light/1.0";

    private const string c_labelSubscriberColor = $"00000000";
    private const string c_labelFontSize = $"[font_size=22]";
    private const string c_labelOutlineColor = $"[outline_color=#202020FF]";
    private const string c_labelOutlineSize = $"[outline_size=1]";
    private const string c_labelNameFont = $"[font=res://Overlay/Fonts/Roboto-Black.ttf]";
    private const string c_labelMessageFont = $"[font=res://Overlay/Fonts/Roboto-Bold.ttf]";
    private const string c_labelMessageColor = $"[color=#F2F2F2FF]";

    private const uint c_labelWidth = 678u;
    private const int c_labelNameColorStartIndex = 107;
    private const int c_twitchEmoteWidth = 16;
    private const int c_twitchEmoteHeight = 16;

    private readonly Dictionary<FadeState, float> c_fadeDelays = new()
    {
        { FadeState.Visible, 32f },
        { FadeState.Fading,  2f }
    };

    private PastelInterpolator m_pastelInterpolator = null;
    private RichTextLabel m_richTextLabel = new();
    private GeneratedState m_generatedState = GeneratedState.Generating;
    private FadeState m_fadeState = FadeState.Visible;
    private float m_fadeElapsed = 0f;

    private string m_color = string.Empty;
    private string m_text = string.Empty;
    private bool m_isSubscriber = false;

    private void GenerateRichTextLabel()
    {
        m_richTextLabel.SetSize(
            new(
                c_labelWidth,
                0f
            )
        );
        m_richTextLabel.BbcodeEnabled = true;
        m_richTextLabel.FitContent = true;
        m_richTextLabel.Text = m_text;
        m_richTextLabel.Visible = false;

        AddChild(
            m_richTextLabel
        );

        m_generatedState = GeneratedState.Generated;
    }

    private void HandleTextFade(
        float delta
    )
    {
        m_fadeElapsed += delta;
        switch (m_fadeState)
        {
            case FadeState.Visible:
                if (m_fadeElapsed >= c_fadeDelays[FadeState.Visible])
                {
                    m_fadeState = FadeState.Fading;
                    m_fadeElapsed = 0f;
                }
                break;
            case FadeState.Fading:
                // todo: transparency
                int height = m_richTextLabel.GetContentHeight();
                if (m_fadeElapsed >= c_fadeDelays[FadeState.Fading])
                {
                    Destroyed?.Invoke();
                    QueueFree();
                }
                break;

            default:
                // something went wrong, shouldn't hit
                break;
        }
    }
}