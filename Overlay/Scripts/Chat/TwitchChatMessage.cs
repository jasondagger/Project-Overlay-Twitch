
namespace Overlay
{
	using Godot;
    using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
    using System.Linq;
	using System.Runtime.Versioning;
    using System.Text;
    using static Godot.HttpClient;
    using RainbowColorIndexType = PastelInterpolator.RainbowColorIndexType;

    [SupportedOSPlatform(platformName: "windows")]
    public sealed partial class TwitchChatMessage : Node2D
	{
		public Action<TwitchChatMessage> Generated = null;
		public Action Destroyed = null;

        public override void _Process(
			double delta
		)
		{
            switch (m_generatedState)
			{
				case GeneratedState.Generated:
					Generated?.Invoke(
						obj: this
					);
					m_generatedState = GeneratedState.Complete;
					break;
				case GeneratedState.Complete:
					HandleTextAnimation(
						delta: (float)delta	
					);
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
			string nameColor,
			string message,
			string messageColor,
			string emotes,
			string badges,
			bool isSubscriber,
			bool isSmoothGPT
		)
		{
			m_pastelInterpolator = pastelInterpolator;
			m_isSubscriber = isSubscriber;
            m_text =
                $"{c_labelFontSize}" +
                $"{c_labelNameFont}" +
                $"[color=#{(m_isSubscriber ? c_labelSubscriberColor : nameColor)}]" +
                $"{name}" +
                $"[/color]" +
                $"[/font]" +
                $"{c_labelMessageFont}" +
                $"  ";

            var parsedEmotes = string.IsNullOrEmpty(
                value: emotes
            ) is false ? emotes.Split(
                separator: '/'
            ).ToList() : null;

            if (
                messageColor.Equals(
                    value: c_rainbowColorTag
                ) is true
            )
            {
                var trimmedMessage = message.Remove(
                    startIndex: message.Length - 2,
                    count: 2
                );
                var containsEmotes = parsedEmotes is not null;
                if (containsEmotes is true)
                {
                    var emoteIndices = RetrieveEmoteIndices(
                        emotes: parsedEmotes    
                    );
                    var rainbowifiedMessage = PastelInterpolator.RainbowifyTextWithEmotes(
                        text: trimmedMessage,
                        emoteIndices: emoteIndices
                    );
                    m_text += $"{rainbowifiedMessage}";
                }
                else
                {
                    var rainbowifiedMessage = PastelInterpolator.RainbowifyText(
                        text: trimmedMessage
                    );
                    m_text += $"{rainbowifiedMessage}";
                }
            }
            else
            {
                m_text += 
                    $"{(messageColor.Equals(value: string.Empty) is true ? c_labelMessageColor : messageColor)}" +
				    $"{(isSmoothGPT ? message : message.Remove(startIndex: message.Length - 2, count: 2))}";
            }

			InsertImages(
				httpManager: httpManager,
				message: message,
				emotes: parsedEmotes,
				badges: badges
			);
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

		private const string c_labelSubscriberColor = $"00000000";
		private const string c_labelFontSize = $"[font_size=22]";
		private const string c_labelNameFont = $"[font=res://Overlay/Fonts/Roboto-Black.ttf]";
		private const string c_labelMessageFont = $"[font=res://Overlay/Fonts/Roboto-Bold.ttf]";
		private const string c_labelMessageColor = $"[color=#F2F2F2FF]";
        private const string c_nameMessageDivisor = "[[[]]]";

        private const string c_twitchBadgeDirectory = "user://Badges";
        private const string c_twitchEmoteDirectoryAnimated = "user://Emotes/Animated";
        private const string c_twitchEmoteDirectoryStatic = "user://Emotes/Static";
        private const string c_twitchEmoteUrlPrefix = "https://static-cdn.jtvnw.net/emoticons/v2";
        private const string c_twitchEmoteUrlSuffix = "default/light/1.0";

		private const int c_twitchEmoteWidth = 16;
		private const int c_twitchEmoteHeight = 16;
        private const int c_gifFrameRateIndex0 = 804;
        private const int c_gifFrameRateIndex1 = 805;
        private const uint c_labelWidth = 678u;
        private const float c_defaultEmoteFramesPerSecondInMilliseconds = 0.04167f;

        private static readonly Dictionary<FadeState, float> c_fadeDelays = new()
		{
			{ FadeState.Visible, 32f },
			{ FadeState.Fading,  2f },
		};
        private static readonly Dictionary<string, string> c_twitchBaseEmojiIds = new()
        {
            { "555555599", "0" },
            { "555555597", "0" },
            { "555555595", "0" },
            { "555555593", "0" },
            { "555555591", "0" },
            { "555555589", "0" },
            { "555555587", "0" },
            { "555555585", "0" },
            { "555555584", "0" },
            { "555555582", "0" },
            { "555555580", "0" },
            { "555555577", "0" },
            { "555555575", "0" },
            { "555555573", "0" },
            { "6",         "0" },
            { "555555571", "0" },
            { "555555563", "0" },
            { "555555562", "0" },
            { "555555560", "0" },
            { "555555558", "0" },
            { "1",         "0" },

            { "555555608", "1" },
            { "555555635", "1" },
            { "555555633", "1" },
            { "555555624", "1" },
            { "555555622", "1" },
            { "439",       "1" },
            { "555555604", "1" },
            { "433",       "1" },
            { "445",       "1" },
            { "555555612", "1" },
            { "436",       "1" },
            { "441",       "1" },
            { "555555620", "1" },
            { "555555618", "1" },
            { "555555614", "1" },
            { "555555616", "1" },
            { "444",       "1" },
            { "555555601", "1" },
            { "443",       "1" },
            { "434",       "1" },
            { "555555628", "1" },
            
            { "484",       "2" },
            { "555555667", "2" },
            { "491",       "2" },
            { "555555663", "2" },
            { "490",       "2" },
            { "501",       "2" },
            { "555555675", "2" },
            { "493",       "2" },
            { "483",       "2" },
            { "555555671", "2" },
            { "492",       "2" },
            { "500",       "2" },
            { "555555695", "2" },
            { "555555693", "2" },
            { "497",       "2" },
            { "555555691", "2" },
            { "494",       "2" },
            { "498",       "2" },
            { "496",       "2" },
            { "489",       "2" },
            { "499",       "2" },
        };
        private static readonly Dictionary<string, string> c_twitchBaseEmojis = new()
        {
            { "R)",  "Twitch0"  },
            { ";p",  "Twitch1"  },
            { ";P",  "Twitch1"  },
            { ":p",  "Twitch2"  },
            { ":P",  "Twitch2"  },
            { ";)",  "Twitch3"  },
            { ":\\", "Twitch4"  },
            { ":/",  "Twitch4"  },
            { "<3",  "Twitch5"  },
            { ":o",  "Twitch6"  },
            { ":O",  "Twitch6"  },
            { "B)",  "Twitch7"  },
            { "o_o", "Twitch8"  },
            { "o_O", "Twitch8"  },
            { "O_o", "Twitch8"  },
            { "O_O", "Twitch8"  },
            { ":|",  "Twitch9"  },
            { ">(",  "Twitch10" },
            { ":D",  "Twitch11" },
            { ":(",  "Twitch12" },
            { ":)",  "Twitch13" },
        };
        private static readonly HashSet<string> c_twitchBaseEmojiIdsWithFileSyntax = new()
        {
            "555555585",
            "433",
            "493",
        };
        private static readonly string c_rainbowColorTag = PastelInterpolator.GetRainbowColorTag();

		private readonly Dictionary<string, int> m_animatedEmoteCurrentFrameCounts = new();
        private readonly Dictionary<string, int> m_animatedEmoteMaxFrameCounts = new();
        private readonly Dictionary<string, float> m_animatedEmoteCurrentFrameRates = new();
        private readonly Dictionary<string, float> m_animatedEmoteMaxFrameRates = new();
        private readonly HashSet<string> m_animatedEmotes = new();
        private readonly object m_textLock = new();

        private PastelInterpolator m_pastelInterpolator = null;
		private RichTextLabel m_richTextLabel = new();
		private GeneratedState m_generatedState = GeneratedState.Generating;
		private FadeState m_fadeState = FadeState.Visible;
		private float m_fadeElapsed = 0f;

        private bool m_hasAnimatedEmotes = false;
        private bool m_isSubscriber = false;
        private string m_color = string.Empty;
		private string m_text = string.Empty;
        private uint m_emotesToLoad = 0u;

        private void GeneratePngFromStaticEmote(
			byte[] body,
            string lookUpEmoteName,
			string originalEmoteName,
			string emoteDirectory
		)
		{
			ApplicationManager.CreateStaticEmoteDirectory(
                emoteName: lookUpEmoteName
            );

			var image = Godot.Image.Create(
                width: c_twitchEmoteWidth,
                height: c_twitchEmoteHeight,
                useMipmaps: false,
                format: Godot.Image.Format.Rgba8
            );
            _ = image.LoadPngFromBuffer(
                buffer: body
            );
            var imageTexture = ImageTexture.CreateFromImage(
                image: image
            );

			var emotePath = $"{emoteDirectory}\\static_0.res";
            _ = ResourceSaver.Save(
                resource: imageTexture,
                path: emotePath
            );
            ReplaceTextMessageWithEmotes(
                originalEmoteName: originalEmoteName,
                emotePath: emotePath
            );

            m_emotesToLoad--;
            if (m_emotesToLoad is 0u)
            {
                GenerateRichTextLabel();
            }
		}

        private void GeneratePngsFromAnimatedEmote(
			byte[] body,
            string lookUpEmoteName,
            string originalEmoteName,
			string emoteDirectory
		)
        {
			ApplicationManager.CreateAnimatedEmoteDirectory(
                emoteName: lookUpEmoteName
            );

            var frameDelay = BitConverter.ToInt16(
                value: new byte[]
                {
                    body[c_gifFrameRateIndex0],
                    body[c_gifFrameRateIndex1],
                },
                startIndex: 0
            );

            var normalizedFrameDelay = 
                frameDelay > 0.1f ||
                frameDelay < .001f ?
                    c_defaultEmoteFramesPerSecondInMilliseconds :
                    frameDelay / 100f;
            var frameDelayText = $"{normalizedFrameDelay}";
            var targetFrameDelayDirectory = ApplicationManager.GetAnimatedEmoteDirectory(
                emoteName: lookUpEmoteName
            );
            var frameDelayFile = $"{targetFrameDelayDirectory}\\frame_rate.txt";
            File.WriteAllText(
                path: frameDelayFile,
                contents: frameDelayText
            );

            using var gifMemoryStream = new MemoryStream(
                buffer: body
            );
            using var gifImage = System.Drawing.Image.FromStream(
                stream: gifMemoryStream
            );
            var frameDimension = new FrameDimension(
				guid: gifImage.FrameDimensionsList[0]
            );
			var frameCount = gifImage.GetFrameCount(
				dimension: frameDimension
			);
			var totalFrames = frameCount - 1;

			for (var i = 0; i < frameCount; i++)
			{
				_ = gifImage.SelectActiveFrame(
					dimension: frameDimension, 
					frameIndex: i
				);

				using var frame = new System.Drawing.Bitmap(
					width: gifImage.Width, 
					height: gifImage.Height
				);
                using (
					var graphics = Graphics.FromImage(
						image: frame
					)
				)
                {
                    graphics.DrawImage(
						image: gifImage, 
						point: Point.Empty
					);
                }

                using var frameStream = new MemoryStream();
				frame.Save(
					stream: frameStream,
					format: ImageFormat.Png
				);

                var imageBytes = frameStream.ToArray();
                var image = Godot.Image.Create(
                    width: c_twitchEmoteWidth,
                    height: c_twitchEmoteHeight,
                    useMipmaps: false,
                    format: Godot.Image.Format.Rgba8
                );
                _ = image.LoadPngFromBuffer(
                    buffer: imageBytes
                );
                var imageTexture = ImageTexture.CreateFromImage(
                    image: image
                );
				var emoteFile = $"{emoteDirectory}\\animated_{i}.res";
                _ = ResourceSaver.Save(
                    resource: imageTexture,
                    path: emoteFile
                );
            }

			var emotePath = $"{emoteDirectory}/animated_0.res";
            ReplaceTextMessageWithEmotes(
                originalEmoteName: originalEmoteName,
                emotePath: emotePath
            );

			m_animatedEmotes.Add(
                item: originalEmoteName
            );
            m_animatedEmoteCurrentFrameCounts.Add(
				key: originalEmoteName,
				value: 0
			);
            m_animatedEmoteMaxFrameCounts.Add(
                key: originalEmoteName,
                value: totalFrames
            );
            m_animatedEmoteCurrentFrameRates.Add(
                key: originalEmoteName,
                value: 0f
            );
            m_animatedEmoteMaxFrameRates.Add(
                key: originalEmoteName,
                value: frameDelayText.ToFloat()
            );
            m_hasAnimatedEmotes = true;

            m_emotesToLoad--;
            if (m_emotesToLoad is 0u)
            {
                GenerateRichTextLabel();
            }
        }

        private void GenerateRichTextLabel()
		{
            /*
             * [img]user://Badges\broadcaster\1.res[/img]  
             * [img]user://Badges\subscriber\3066.res[/img]  
             * [img]user://Badges\twitch-recap-2023\1.res[/img]  
             * [font_size=22]
             * [font=res://Overlay/Fonts/Roboto-Black.ttf]
             * [color=#00000000]
             * SmoothDagger
             * [/color]
             * [/font]
             * [font=res://Overlay/Fonts/Roboto-Bold.ttf]  
             * [img]user://Emotes/Animated/smooth210Backstab/animated_0.res[/img] 
             * [img]user://Emotes/Animated/smooth210Brain/animated_0.res[/img] 
             * [img]user://Emotes/Animated/smooth210Cannons/animated_0.res[/img] 
             * [img]user://Emotes/Animated/smooth210Brain/animated_0.res[/img] 
             * [img]user://Emotes/Animated/smooth210Backstab/animated_0.res[/img] 
             * [img]user://Emotes/Static/LUL/static_0.res[/img] 
             * [img]user://Emotes/Animated/PopNemo/animated_0.res[/img] 
             * [img]user://Emotes/Static/Twitch11_2/static_0.res[/img] 
             * [img]user://Emotes/Static/Twitch8_2/static_0.res[/img] 
             * [img]E:\Programs\AppData\Roaming\Godot\app_userdata\Overlay/Emotes/Static/Twitch7_2\static_0.res[/img]
             * 
             */
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

        private HashSet<int> RetrieveEmoteIndices(
            List<string> emotes    
        )
        {
            var parsedEmoteIndices = new HashSet<int>();

            foreach (var emoteValue in emotes)
            {
                var emoteData = emoteValue.Split(
                    separator: ':'
                );

                var emoteLink = emoteData[0u];
                var emoteRanges = emoteData[1u].Split(
                    separator: ','
                );
                foreach (var emoteRange in emoteRanges)
                {
                    var emoteIndices = emoteRange.Split(
                        separator: '-'
                    );
                    var startIndex = emoteIndices[0u].ToInt();
                    var endIndex = emoteIndices[1u].ToInt();

                    for (var i = startIndex; i <= endIndex; i++)
                    {
                        _ = parsedEmoteIndices.Add(
                            item: i    
                        );
                    }
                }
            }

            return parsedEmoteIndices;
        }

        private void HandleTextAnimation(
			float delta
		)
		{
            if (m_hasAnimatedEmotes)
            {
                foreach (var animatedEmote in m_animatedEmotes)
                {
                    m_animatedEmoteCurrentFrameRates[key: animatedEmote] += delta;

                    if (m_animatedEmoteCurrentFrameRates[key: animatedEmote] >= m_animatedEmoteMaxFrameRates[key: animatedEmote])
                    {
                        var previousFrame = m_animatedEmoteCurrentFrameCounts[key: animatedEmote];
                        var currentFrame = previousFrame + 1;
                        if (currentFrame > m_animatedEmoteMaxFrameCounts[key: animatedEmote])
                        {
                            currentFrame = 0;
                        }

                        m_text = m_text.Replace(
                            oldValue: $"{animatedEmote}/animated_{previousFrame}.res",
                            newValue: $"{animatedEmote}/animated_{currentFrame}.res"
                        );

                        m_animatedEmoteCurrentFrameCounts[key: animatedEmote] = currentFrame;
                        m_animatedEmoteCurrentFrameRates[key: animatedEmote] = 0f;
                    }
                }
            }

            if (m_isSubscriber)
            {
                var color = m_pastelInterpolator.GetColorAsHex(
                    rainbowColorIndexType: RainbowColorIndexType.Color0    
                );
                m_richTextLabel.Text = m_text.Replace(
                    oldValue: c_labelSubscriberColor,
                    newValue: color
                );
            }
			else if (m_hasAnimatedEmotes)
			{
				m_richTextLabel.Text = m_text;
			}
        }

		private void HandleTextFade(
			float delta
		)
		{
			m_fadeElapsed += delta;
			switch (m_fadeState)
			{
				case FadeState.Visible:
					if (m_fadeElapsed >= c_fadeDelays[key: FadeState.Visible])
					{
						m_fadeState = FadeState.Fading;
						m_fadeElapsed = 0f;
					}
					break;
				case FadeState.Fading:
                    // todo: transparency
                    var height = m_richTextLabel.GetContentHeight();
					if (m_fadeElapsed >= c_fadeDelays[key: FadeState.Fading])
					{
						Destroyed?.Invoke();
						QueueFree();
					}
					break;

				default:
					break;
			}
		}

        private void InsertBadges(
            string badges
        )
        {
            var badgesList = badges.Split(
                separator: ','
            ).Reverse().ToList();
            foreach (var badge in badgesList)
            {
                var badgeData = badge.Split(
                    separator: '/'
                );
                var badgeSet = badgeData[0];
                var badgeVersion = badgeData[1];

                var badgePath = $"{c_twitchBadgeDirectory}\\{badgeSet}\\{badgeVersion}.res";
                m_text = m_text.Insert(
                    startIndex: 0,
                    value: $"[img]{badgePath}[/img]  "
                );
            }
        }

        private void InsertEmotes(
            HttpManager httpManager,
            string message,
            List<string> emotes
        )
        {
            for (var i = 0; i < emotes.Count; i++)
            {
                var emoteData = emotes[index: i].Split(
                    separator: ':'
                );
                var emoteLink = emoteData[0u];
                if (
                    c_twitchBaseEmojiIdsWithFileSyntax.Contains(
                        value: emoteLink
                    )
                )
                {
                    var emoteRanges = emoteData[1u].Split(
                        separator: ','
                    );
                    var emoteIndices = emoteRanges[0u].Split(
                        separator: '-'
                    );
                    var startIndex = emoteIndices[0u].ToInt();
                    var endIndex = emoteIndices[1u].ToInt();
                    var originalEmoteName = message.Substring(
                        startIndex: startIndex,
                        length: endIndex - startIndex + 1
                    );

                    var baseEmoteName = c_twitchBaseEmojis[key: originalEmoteName];
                    var baseEmoteId = c_twitchBaseEmojiIds[key: emoteLink];
                    var lookUpEmoteName = $"{baseEmoteName}_{baseEmoteId}";

                    var emotePath = $"{c_twitchEmoteDirectoryStatic}/{lookUpEmoteName}/static_0.res";
                    ReplaceTextMessageWithEmotes(
                        originalEmoteName: originalEmoteName,
                        emotePath: emotePath
                    );
                    emotes.RemoveAt(
                        index: i
                    );
                }
            }

            foreach (var emoteValue in emotes)
            {
                var emoteData = emoteValue.Split(
                    separator: ':'
                );

                var emoteLink = emoteData[0u];
                var emoteRanges = emoteData[1u].Split(
                    separator: ','
                );
                var emoteIndices = emoteRanges[0u].Split(
                    separator: '-'
                );
                var startIndex = emoteIndices[0u].ToInt();
                var endIndex = emoteIndices[1u].ToInt();
                var originalEmoteName = message.Substring(
                    startIndex: startIndex,
                    length: endIndex - startIndex + 1
                );

                var lookUpEmoteName = originalEmoteName;
                if (
                    c_twitchBaseEmojis.ContainsKey(
                        key: originalEmoteName
                    ) is true
                )
                {
                    var baseEmoteName = c_twitchBaseEmojis[key: originalEmoteName];
                    var baseEmoteId = c_twitchBaseEmojiIds[key: emoteLink];

                    lookUpEmoteName = $"{baseEmoteName}_{baseEmoteId}";
                }

                var emotePathStatic = ApplicationManager.GetStaticEmoteDirectory(
                    emoteName: lookUpEmoteName
                );
                if (
                    Directory.Exists(
                        path: emotePathStatic
                    ) is true
                )
                {
                    var emotePath = $"{c_twitchEmoteDirectoryStatic}/{lookUpEmoteName}/static_0.res";
                    ReplaceTextMessageWithEmotes(
                        originalEmoteName: originalEmoteName,
                        emotePath: emotePath
                    );
                    continue;
                }

                var emotePathAnimated = ApplicationManager.GetAnimatedEmoteDirectory(
                    emoteName: lookUpEmoteName
                );
                if (
                    Directory.Exists(
                        path: emotePathAnimated
                    ) is true
                )
                {
                    var emotePath = $"{c_twitchEmoteDirectoryAnimated}/{lookUpEmoteName}/animated_0.res";
                    ReplaceTextMessageWithEmotes(
                        originalEmoteName: originalEmoteName,
                        emotePath: emotePath
                    );

                    m_animatedEmotes.Add(
                        item: originalEmoteName
                    );
                    m_animatedEmoteCurrentFrameCounts.Add(
                        key: originalEmoteName,
                        value: 0
                    );
                    m_animatedEmoteCurrentFrameRates.Add(
                        key: originalEmoteName,
                        value: 0f
                    );

                    var files = Directory.GetFiles(
                        path: emotePathAnimated
                    );
                    var frameCount = files.Length - 2;
                    m_animatedEmoteMaxFrameCounts.Add(
                        key: originalEmoteName,
                        value: frameCount
                    );

                    var file = files.Last();
                    var text = File.ReadAllText(
                        path: file
                    );
                    m_animatedEmoteMaxFrameRates.Add(
                        key: originalEmoteName,
                        value: text.ToFloat()
                    );

                    m_hasAnimatedEmotes = true;
                    continue;
                }

                m_emotesToLoad++;
                var uri = new Uri(
                    $"{c_twitchEmoteUrlPrefix}/{emoteLink}/{c_twitchEmoteUrlSuffix}"
                );
                httpManager.SendHttpRequest(
                    url: uri.OriginalString,
                    headers: new List<string>(),
                    method: Method.Get,
                    json: string.Empty,
                    requestCompletedHandler:
                    (
                        long result,
                        long responseCode,
                        string[] headers,
                        byte[] body
                    ) =>
                    {
                        if (responseCode >= 300u)
                        {
                            QueueFree();
                            return;
                        }

                        var contentTypeHeader = headers[0];
                        if (
                            contentTypeHeader.Contains(
                                value: "png"
                            )
                        )
                        {
                            GeneratePngFromStaticEmote(
                                body: body,
                                lookUpEmoteName: lookUpEmoteName,
                                originalEmoteName: originalEmoteName,
                                emoteDirectory: emotePathStatic
                            );
                        }
                        else
                        {
                            GeneratePngsFromAnimatedEmote(
                                body: body,
                                lookUpEmoteName: lookUpEmoteName,
                                originalEmoteName: originalEmoteName,
                                emoteDirectory: emotePathAnimated
                            );
                        }
                    }
                );
            }
        }

        private void InsertImages(
			HttpManager httpManager,
            string message,
            List<string> emotes,
            string badges
        )
		{
            var hasBadges = string.IsNullOrEmpty(
				value: badges
			) is false;
			if (hasBadges is true)
			{
                InsertBadges(
				    badges: badges
				);
            }

			var hasEmotes = emotes is not null;
			if (hasEmotes is true)
			{
                InsertEmotes(
                    httpManager: httpManager,
                    message: message,
                    emotes: emotes
                );
            }

            if (m_emotesToLoad is 0u)
            {
                GenerateRichTextLabel();
            }
        }

        private void ReplaceTextMessageWithEmotes(
            string originalEmoteName,
            string emotePath
        )
        {
            lock (m_textLock)
            {
                var splitText = m_text.Split(
                    divisor: "  "
                );
                m_text = splitText[0];
                var i = 1;

                if (
                    m_text.EndsWith(
                        value: c_labelMessageFont
                    ) is false
                )
                {
                    for (; i < splitText.Length; i++)
                    {
                        m_text += $"  {splitText[i]}";
                        if (
                            splitText[i].EndsWith(
                                value: c_labelMessageFont
                            ) is true
                        )
                        {
                            break;
                        }
                    }
                    i++;
                }

                for (; i < splitText.Length; i++)
                {
                    m_text += $"  {splitText[i].Replace(oldValue: originalEmoteName, newValue: $"[img]{emotePath}[/img]")}";
                }
            }
        }
    }
}