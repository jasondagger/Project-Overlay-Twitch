namespace Overlay
{
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
		public Action<TwitchChatMessage> Generated = null;
		public Action Destroyed = null;

		public override void _Process(
			double delta
		)
		{
			if (m_isSubscriber)
			{
				var color = m_pastelInterpolator.GetColorAsHex();
				m_richTextLabel.Text = m_text.Replace(
                    oldValue: c_labelSubscriberColor,
                    newValue: color
				);
			}

			switch (m_generatedState)
			{
				case GeneratedState.Generated:
					Generated?.Invoke(
						obj: this
					);
					m_generatedState = GeneratedState.Complete;
					break;

				case GeneratedState.Complete:
					HandleTextFade(
						delta: (float)delta
					);
					break;

				case GeneratedState.Generating:
				default:
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
				value: color
			) is true;
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
				$"{message.Remove(message.Length - 2, 2)}";

			var hasEmotes = string.IsNullOrEmpty(
				value: emotes
			) is false;
			if (hasEmotes is true)
			{
				// split each emote from twitch apis
				var emoteValues = emotes.Split(
					separator: '/'
				);
				var pngsToLoad = 0u;
				foreach (var emoteValue in emoteValues)
				{
					// split emote link & name ranges
					var emoteData = emoteValue.Split(
						separator: ':'
					);

                    var emoteLink = emoteData[0u];
					// retrieve emote name within message string
					var emoteRanges = emoteData[1u].Split(
						separator: ','
					);
					var emoteIndices = emoteRanges[0u].Split(
						separator: '-'
					);
                    var startIndex = emoteIndices[0u].ToInt();
                    var endIndex = emoteIndices[1u].ToInt();
                    var emoteName = message.Substring(
                        startIndex: startIndex,
                        length: endIndex - startIndex + 1
					);

                    var emotePath = $"{c_twitchEmoteDirectory}\\{emoteLink}.res";
					if (
						File.Exists(
                            path: emotePath
						) is true
					)
					{
						m_text = m_text.Replace(
                            oldValue: emoteName,
                            newValue: $"[img]{emotePath}[/img]"
						);
					}
					else
					{
						pngsToLoad++;
						httpManager.SendHttpRequest(
                            url: $"{c_twitchEmoteUrlPrefix}{emoteLink}{c_twitchEmoteUrlSuffix}",
                            headers: null,
                            method: Method.Get,
                            json: string.Empty,
                            requestCompletedHandler: (
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
                                    width: c_twitchEmoteWidth,
                                    height: c_twitchEmoteHeight,
                                    useMipmaps: false,
                                    format: Image.Format.Rgba8
								);
                                _ = image.LoadPngFromBuffer(
                                    buffer: body
                                );
								var imageTexture = ImageTexture.CreateFromImage(
                                    image: image
								);
                                _ = ResourceSaver.Save(
                                    resource: imageTexture,
                                    path: emotePath
                                );

								m_text = m_text.Replace(
                                    oldValue: emoteName,
                                    newValue: $"[img]{emotePath}[/img]"
								);

								pngsToLoad--;
								if (pngsToLoad is 0u)
								{
									GenerateRichTextLabel();
								}
							}
						);
					}
				}

				if (pngsToLoad is 0u)
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
			Fading,
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
		private const int c_twitchEmoteWidth = 16;
		private const int c_twitchEmoteHeight = 16;

		private readonly Dictionary<FadeState, float> c_fadeDelays = new()
		{
			{ FadeState.Visible, 32f },
			{ FadeState.Fading,  2f },
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
				size: new(
					x: c_labelWidth,
					y: 0f
				)
			);
			m_richTextLabel.BbcodeEnabled = true;
			m_richTextLabel.FitContent = true;
			m_richTextLabel.Text = m_text;
			m_richTextLabel.Visible = false;

			AddChild(
				node: m_richTextLabel
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
                    var height = m_richTextLabel.GetContentHeight();
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
}