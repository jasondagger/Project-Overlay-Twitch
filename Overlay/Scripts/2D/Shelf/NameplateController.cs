
namespace Overlay
{
    using Godot;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using RichTextLabelType = RichTextLabelSampler.RichTextLabelType;

    public sealed partial class NameplateController : Control
    {
        public override void _ExitTree()
        {
            m_isRunning = false;
        }

        public override void _Process(
            double elapsed
        )
        {
            if (m_isRichTextLabelTitleAddTitle)
            {
                AddRichTextLabelTitle();
            }
            if (m_isRichTextLabelNameAnimating)
            {
                AnimateNameScale(
                    elapsed: (float)elapsed
                );
            }

            if (m_isRichTextLabelTitleScrolling)
            {
                AnimateTitleScroll(
                    elapsed: (float)elapsed
                );
            }
            else if (m_isRichTextLabelTitleWaving)
            {
                AnimateTitleWave(
                    elapsed: (float)elapsed
                );
            }

            AnimateIcon(
                elapsed: (float)elapsed
            );
        }

        public override void _Ready()
        {
            RetrieveResources();
            AnimateName();
            AnimateTitles();
        }

        private enum ImageIconAnimationState : uint
        {
            Showing = 0u,
            Hiding,
            Idle
        }

        private enum NameRichTextLabelFontSizeState : uint
        {
            Idle = 0u,
            IncreaseFontSize,
            DecreaseFontSize
        }

        private enum TitleRichTextLabelScrollState : uint
        {
            Idle = 0u,
            ScrollCenterToEnd,
            ScrollStartToCenter
        }

        private enum TitleRichTextLabelWaveState : uint
        {
            Center = 0u,
            CenterToTop,
            TopToBottom,
            BottomToCenter
        }

        private struct NameLetter
        {
            public RichTextLabel RichTextLabel = null;
            public NameRichTextLabelFontSizeState State = NameRichTextLabelFontSizeState.Idle;
            public float Max = 0f;
            public float Min = 0f;

            public NameLetter(
                RichTextLabel richTextLabel,
                NameRichTextLabelFontSizeState state,
                float max,
                float min
            )
            {
                this.RichTextLabel = richTextLabel;
                this.State = state;
                this.Max = max;
                this.Min = min;
            }
        }

        private struct NameplateTitle
        {
            public string textTitle = string.Empty;
            public CompressedTexture2D textureIcon = null;

            public NameplateTitle(
                string textTitle,
                CompressedTexture2D textureIcon
            )
            {
                this.textTitle = textTitle;
                this.textureIcon = textureIcon;
            }
        }

        private struct TitleLetter
        {
            public char letter = '\0';
            public RichTextLabel richTextLabel = null;
            public TitleRichTextLabelScrollState scrollState = TitleRichTextLabelScrollState.Idle;
            public float scrollCenter = 0f;
            public float scrollEnd = 0f;
            public float scrollStart = 0f;
            public TitleRichTextLabelWaveState waveState = TitleRichTextLabelWaveState.Center;
            public float waveCenter = 0f;
            public float waveMax = 0f;
            public float waveMin = 0f;

            public TitleLetter(
                char letter,
                RichTextLabel richTextLabel,
                TitleRichTextLabelScrollState scrollState,
                float scrollCenter,
                float scrollEnd,
                float scrollStart,
                TitleRichTextLabelWaveState waveState,
                float waveCenter,
                float waveMax,
                float waveMin
            )
            {
                this.letter = letter;
                this.richTextLabel = richTextLabel;
                this.scrollState = scrollState;
                this.scrollCenter = scrollCenter;
                this.scrollEnd = scrollEnd;
                this.scrollStart = scrollStart;
                this.waveState = waveState;
                this.waveCenter = waveCenter;
                this.waveMax = waveMax;
                this.waveMin = waveMin;
            }
        }

        private const float c_imageIconSpeed = 2f;
        private const float c_imageIconRotation = -2160f;

        private const string c_nodePathRelativeRichTextLabelSampler = "RichTextLabelSampler";
        private const string c_nodePathRelativeRichTextLabelSamplerParentNodeTarget = "TitleViewportContainer/TitleViewport/Title";
        private const string c_nodePathRelativeTitleImageIcon = "ImageViewportContainer/ImageViewport/Icon";
        private const string c_nodePathRelativeViewportContainer = "NameViewportContainer/NameViewport";

        private const int c_richTextLabelNameDelayTimeInMilliseconds = 50;
        private const int c_richTextLabelNameAnimationDelayTimeInMillisecondsMin = 5000;
        private const int c_richTextLabelNameAnimationDelayTimeInMillisecondsMax = 9000;
        private const float c_richTextLabelNameScaleVelocity = 0.15f;
        private const float c_richTextLabelNameScaleMin = 1f;
        private const float c_richTextLabelNameScaleMax = 1.075f;

        private const int c_richTextLabelTitleDelayInMillisecondsScroll = 20;
        private const int c_richTextLabelTitleDelayInMillisecondsWave = 60;
        private const float c_richTextLabelTitleScrollDistance = 232f;
        private const float c_richTextLabelTitleScrollVelocity = 650f;

        private const int c_richTextLabelTitleDelayInMillisecondsMin = 3000;
        private const int c_richTextLabelTitleDelayInMillisecondsMax = 6000;
        private const int c_richTextLabelTitleDelayInMillisecondsWaveNext = 1000;
        private const float c_richTextLabelTitleWaveVelocity = 5f;
        private const float c_richTextLabelTitleWaveHeight = 1f;
        private const uint c_richTextLabelTitleWaveCount = 2u;

        private const int c_taskAwaitDelayTimeInMilliseconds = 20;

        private readonly Dictionary<int, NameLetter> m_nameLetters = new();
        private readonly Dictionary<int, TitleLetter> m_titleLetters = new();
        private readonly List<NameplateTitle> m_nameplateTitles = new()
        {
            new NameplateTitle(
                textTitle: "Pirate Philanthropist",
                textureIcon: GD.Load<CompressedTexture2D>(
                    path: "res://Overlay/Textures/Icons/Icon_PiratePhilanthropist.png"
                )
            ),
            new NameplateTitle(
                textTitle: "Corporate Spy",
                textureIcon: GD.Load<CompressedTexture2D>(
                    "res://Overlay/Textures/Icons/Icon_CorporateSpy.png"
                )
            ),
            new NameplateTitle(
                textTitle: "Professional Hooker",
                textureIcon: GD.Load<CompressedTexture2D>(
                    "res://Overlay/Textures/Icons/Icon_ProfessionalHooker.png"
                )
            )
        };

        private RichTextLabelSampler m_richTextLabelSampler = null;
        private TextureRect m_imageIconTitle = null;
        private ImageIconAnimationState m_imageIconAnimationState = ImageIconAnimationState.Idle;
        private float m_imageIconElapsed = 0f;
        private int m_currentTitleIndex = 0;

        private bool m_isRichTextLabelNameAnimating = false;
        private bool m_isRichTextLabelTitleAddTitle = false;
        private bool m_isRichTextLabelTitleScrolling = false;
        private bool m_isRichTextLabelTitleWaving = false;
        private bool m_isRunning = true;

        private void AddRichTextLabelTitle()
        {
            var title = m_nameplateTitles[m_currentTitleIndex].textTitle;
            var positionX = 0f;
            for (var i = 0; i < title.Length; i++)
            {
                var letter = title[i];
                var richTextLabel = m_richTextLabelSampler.DequeueRichTextLabel(
                    letter: letter
                );
                richTextLabel.SetPosition(
                    position: new Vector2(
                        x: positionX,
                        y: 0f
                    )
                );

                m_titleLetters.Add(
                    key: i,
                    value: new(
                        letter: letter,
                        richTextLabel: richTextLabel,
                        scrollState: TitleRichTextLabelScrollState.Idle,
                        scrollCenter: positionX - c_richTextLabelTitleScrollDistance,
                        scrollEnd: positionX - c_richTextLabelTitleScrollDistance * 2u,
                        scrollStart: positionX,
                        waveState: TitleRichTextLabelWaveState.Center,
                        waveCenter: 0f,
                        waveMax: c_richTextLabelTitleWaveHeight,
                        waveMin: -c_richTextLabelTitleWaveHeight
                    )
                );

                positionX += richTextLabel.GetContentWidth();
            }

            m_isRichTextLabelTitleAddTitle = false;
        }

        private void AnimateIcon(
            float elapsed
        )
        {
            switch (m_imageIconAnimationState)
            {
                case ImageIconAnimationState.Showing:
                    ShowIcon(
                        delta: elapsed
                    );
                    break;

                case ImageIconAnimationState.Hiding:
                    HideIcon(
                        delta: elapsed
                    );
                    break;

                case ImageIconAnimationState.Idle:
                default:
                    break;
            }
        }

        private void AnimateName()
        {
            _ = Task.Run(
                function:
                    async () =>
                    {
                        var random = new Random();
                        while (m_isRunning)
                        {
                            await Task.Delay(
                                random.Next(
                                    minValue: c_richTextLabelNameAnimationDelayTimeInMillisecondsMin,
                                    maxValue: c_richTextLabelNameAnimationDelayTimeInMillisecondsMax
                                )
                            );

                            m_isRichTextLabelNameAnimating = true;
                            for (var i = 0; i < m_nameLetters.Count; i++)
                            {
                                var nameLetter = m_nameLetters[i];
                                nameLetter.State = NameRichTextLabelFontSizeState.IncreaseFontSize;
                                m_nameLetters[i] = nameLetter;
                                await Task.Delay(
                                    millisecondsDelay: c_richTextLabelNameDelayTimeInMilliseconds
                                );
                            }

                            while (m_isRichTextLabelNameAnimating)
                            {
                                await Task.Delay(
                                    millisecondsDelay: c_taskAwaitDelayTimeInMilliseconds
                                );
                            }
                        }
                    }
                );
        }

        private void AnimateNameScale(
            float elapsed
        )
        {
            for (var i = 0; i < m_nameLetters.Count; i++)
            {
                var nameLetter = m_nameLetters[i];
                switch (nameLetter.State)
                {
                    case NameRichTextLabelFontSizeState.IncreaseFontSize:
                        IncreaseNameLetterFontSize(
                            index: i,
                            delta: elapsed
                        );
                        break;

                    case NameRichTextLabelFontSizeState.DecreaseFontSize:
                        DecreaseNameLetterFontSize(
                            index: i,
                            delta: elapsed
                        );
                        break;

                    case NameRichTextLabelFontSizeState.Idle:
                    default:
                        break;
                }
            }

            if (
                m_nameLetters.All(
                    predicate: x => x.Value.State is NameRichTextLabelFontSizeState.Idle
                ) is true
            )
            {
                m_isRichTextLabelNameAnimating = false;
            }
        }

        private void AnimateTitles()
        {
            _ = Task.Run(
                function:
                async () =>
                {
                    var random = new Random();
                    while (m_isRunning)
                    {
                        m_isRichTextLabelTitleAddTitle = true;
                        while (m_isRichTextLabelTitleAddTitle) ;

                        m_imageIconAnimationState = ImageIconAnimationState.Showing;
                        m_isRichTextLabelTitleScrolling = true;
                        for (var i = 0; i < m_titleLetters.Count; i++)
                        {
                            var titleLetter = m_titleLetters[i];
                            titleLetter.scrollState = TitleRichTextLabelScrollState.ScrollStartToCenter;
                            m_titleLetters[i] = titleLetter;
                            await Task.Delay(
                                millisecondsDelay: c_richTextLabelTitleDelayInMillisecondsScroll
                            );
                        }

                        while (m_isRichTextLabelTitleScrolling) ;

                        for (var i = 0u; i < c_richTextLabelTitleWaveCount; i++)
                        {
                            await Task.Delay(
                                millisecondsDelay: random.Next(
                                    minValue: c_richTextLabelTitleDelayInMillisecondsMin,
                                    maxValue: c_richTextLabelTitleDelayInMillisecondsMax
                                )
                            );

                            m_isRichTextLabelTitleWaving = true;
                            for (var j = 0; j < m_titleLetters.Count; j++)
                            {
                                var titleLetter = m_titleLetters[j];
                                titleLetter.waveState = TitleRichTextLabelWaveState.CenterToTop;
                                m_titleLetters[j] = titleLetter;
                                await Task.Delay(
                                    millisecondsDelay: c_richTextLabelTitleDelayInMillisecondsWave
                                );
                            }

                            while (m_isRichTextLabelTitleWaving) ;
                        }

                        await Task.Delay(
                            millisecondsDelay: random.Next(
                                minValue: c_richTextLabelTitleDelayInMillisecondsMin,
                                maxValue: c_richTextLabelTitleDelayInMillisecondsMax
                            )
                        );

                        m_imageIconAnimationState = ImageIconAnimationState.Hiding;
                        m_isRichTextLabelTitleScrolling = true;
                        for (var i = 0; i < m_titleLetters.Count; i++)
                        {
                            var titleLetter = m_titleLetters[i];
                            titleLetter.scrollState = TitleRichTextLabelScrollState.ScrollCenterToEnd;
                            m_titleLetters[i] = titleLetter;
                            await Task.Delay(
                                millisecondsDelay: c_richTextLabelTitleDelayInMillisecondsScroll
                            );
                        }

                        while (m_isRichTextLabelTitleScrolling) ;

                        RemoveRichTextLabelTitle();
                        await Task.Delay(
                            millisecondsDelay: c_richTextLabelTitleDelayInMillisecondsWaveNext
                        );

                        if (++m_currentTitleIndex == m_nameplateTitles.Count)
                        {
                            m_currentTitleIndex = 0;
                        }
                    }
                }
            );
        }

        private void AnimateTitleScroll(
            float elapsed
        )
        {
            for (var i = 0; i < m_titleLetters.Count; i++)
            {
                var titleLetter = m_titleLetters[i];
                switch (titleLetter.scrollState)
                {
                    case TitleRichTextLabelScrollState.ScrollCenterToEnd:
                        ScrollTitleLetterCenterToEnd(
                            index: i,
                            delta: elapsed
                        );
                        break;

                    case TitleRichTextLabelScrollState.ScrollStartToCenter:
                        ScrollTitleLetterStartToCenter(
                            index: i,
                            delta: elapsed
                        );
                        break;

                    case TitleRichTextLabelScrollState.Idle:
                    default:
                        break;
                }
            }

            if (
                m_titleLetters.All(
                    x => x.Value.scrollState is TitleRichTextLabelScrollState.Idle
                ) is true
            )
            {
                m_isRichTextLabelTitleScrolling = false;
            }
        }

        private void AnimateTitleWave(
            float elapsed
        )
        {
            for (var i = 0; i < m_titleLetters.Count; i++)
            {
                var titleLetter = m_titleLetters[i];
                switch (titleLetter.waveState)
                {
                    case TitleRichTextLabelWaveState.CenterToTop:
                        WaveTitleLetterToTop(
                            index: i,
                            delta: elapsed
                        );
                        break;

                    case TitleRichTextLabelWaveState.TopToBottom:
                        WaveTitleLetterToBottom(
                            index: i,
                            delta: elapsed
                        );
                        break;

                    case TitleRichTextLabelWaveState.BottomToCenter:
                        WaveTitleLetterToCenter(
                            index: i,
                            delta: elapsed
                        );
                        break;

                    case TitleRichTextLabelWaveState.Center:
                    default:
                        break;
                }
            }

            if (
                m_titleLetters.All(
                    x => x.Value.waveState is TitleRichTextLabelWaveState.Center
                ) is true
            )
            {
                m_isRichTextLabelTitleWaving = false;
            }
        }

        private void DecreaseNameLetterFontSize(
            int index,
            float delta
        )
        {
            var nameLetter = m_nameLetters[index];
            var scale = nameLetter.RichTextLabel.Scale;

            scale.X -= c_richTextLabelNameScaleVelocity * delta;
            if (scale.X <= c_richTextLabelNameScaleMin)
            {
                scale.X = c_richTextLabelNameScaleMin;
                nameLetter.State = NameRichTextLabelFontSizeState.Idle;
            }
            scale.Y = scale.X;

            nameLetter.RichTextLabel.Scale = scale;
            m_nameLetters[index] = nameLetter;
        }

        private void IncreaseNameLetterFontSize(
            int index,
            float delta
        )
        {
            var nameLetter = m_nameLetters[index];
            var scale = nameLetter.RichTextLabel.Scale;

            scale.X += c_richTextLabelNameScaleVelocity * delta;
            if (scale.X >= c_richTextLabelNameScaleMax)
            {
                scale.X = c_richTextLabelNameScaleMax;
                nameLetter.State = NameRichTextLabelFontSizeState.DecreaseFontSize;
            }
            scale.Y = scale.X;

            nameLetter.RichTextLabel.Scale = scale;
            m_nameLetters[index] = nameLetter;
        }

        private void RemoveRichTextLabelTitle()
        {
            foreach (var titleLetter in m_titleLetters.Values)
            {
                m_richTextLabelSampler.RequeueRichTextLabel(
                    letter: titleLetter.letter
                );
            }

            m_titleLetters.Clear();
        }

        private void RetrieveResources()
        {
            m_richTextLabelSampler = GetNode<RichTextLabelSampler>(
                path: c_nodePathRelativeRichTextLabelSampler
            );

            var richTextLabelSamplerParentNodeTarget = GetNode(
                path: c_nodePathRelativeRichTextLabelSamplerParentNodeTarget
            );
            m_richTextLabelSampler.LoadRichTextLabelsAndAttachToParentNode(
                richTextLabelType: RichTextLabelType.NameplateTitle,
                parent: richTextLabelSamplerParentNodeTarget
            );

            m_imageIconTitle = GetNode<TextureRect>(
                path: c_nodePathRelativeTitleImageIcon
            );

            var nodeNameMask = GetNode(
                path: c_nodePathRelativeViewportContainer
            );
            var nodeLetters = nodeNameMask.GetChildren();
            for (var i = 0; i < nodeLetters.Count; i++)
            {
                m_nameLetters.Add(
                    key: i,
                    value: new(
                        richTextLabel: nodeLetters[i] as RichTextLabel,
                        state: NameRichTextLabelFontSizeState.Idle,
                        max: c_richTextLabelNameScaleMin,
                        min: c_richTextLabelNameScaleMax
                    )
                );
            }
        }

        private void ScrollTitleLetterCenterToEnd(
            int index,
            float delta
        )
        {
            var textLetter = m_titleLetters[index];
            var position = textLetter.richTextLabel.Position;

            position.X -= c_richTextLabelTitleScrollVelocity * delta;
            if (position.X <= textLetter.scrollEnd)
            {
                position.X = textLetter.scrollEnd;
                textLetter.scrollState = TitleRichTextLabelScrollState.Idle;
                textLetter.richTextLabel.Visible = false;
            }

            textLetter.richTextLabel.Position = position;
            m_titleLetters[index] = textLetter;
        }

        private void ScrollTitleLetterStartToCenter(
            int index,
            float delta
        )
        {
            var textLetter = m_titleLetters[index];
            if (textLetter.richTextLabel.Visible is false)
            {
                textLetter.richTextLabel.Visible = true;
            }

            var position = textLetter.richTextLabel.Position;
            position.X -= c_richTextLabelTitleScrollVelocity * delta;
            if (position.X <= textLetter.scrollCenter)
            {
                position.X = textLetter.scrollCenter;
                textLetter.scrollState = TitleRichTextLabelScrollState.Idle;
            }

            textLetter.richTextLabel.Position = position;
            m_titleLetters[index] = textLetter;
        }

        private void ShowIcon(
            float delta
        )
        {
            if (m_imageIconTitle.Visible is false)
            {
                m_imageIconTitle.Texture = m_nameplateTitles[m_currentTitleIndex].textureIcon;
                m_imageIconTitle.Visible = true;
            }

            const float startRotation = 0f;
            const float endRotation = c_imageIconRotation;

            m_imageIconElapsed += c_imageIconSpeed * delta;
            m_imageIconTitle.RotationDegrees = Mathf.Lerp(
                from: startRotation,
                to: endRotation,
                weight: m_imageIconElapsed
            );
            m_imageIconTitle.Scale = Vector2.Zero.Lerp(
                to: Vector2.One,
                weight: m_imageIconElapsed
            );

            if (m_imageIconElapsed >= 1f)
            {
                m_imageIconAnimationState = ImageIconAnimationState.Idle;
                m_imageIconTitle.RotationDegrees = endRotation;
                m_imageIconTitle.Scale = Vector2.One;
                m_imageIconElapsed = 0f;
            }
        }

        private void WaveTitleLetterToBottom(
            int index,
            float delta
        )
        {
            var textLetter = m_titleLetters[index];
            var position = textLetter.richTextLabel.Position;

            position.Y -= c_richTextLabelTitleWaveVelocity * delta;
            if (position.Y <= textLetter.waveMin)
            {
                position.Y = textLetter.waveMin;
                textLetter.waveState = TitleRichTextLabelWaveState.BottomToCenter;
            }

            textLetter.richTextLabel.Position = position;
            m_titleLetters[index] = textLetter;
        }

        private void WaveTitleLetterToCenter(
            int index,
            float delta
        )
        {
            var textLetter = m_titleLetters[index];
            var position = textLetter.richTextLabel.Position;

            position.Y += c_richTextLabelTitleWaveVelocity * delta;
            if (position.Y >= textLetter.waveCenter)
            {
                position.Y = textLetter.waveCenter;
                textLetter.waveState = TitleRichTextLabelWaveState.Center;
            }

            textLetter.richTextLabel.Position = position;
            m_titleLetters[index] = textLetter;
        }

        private void WaveTitleLetterToTop(
            int index,
            float delta
        )
        {
            var textLetter = m_titleLetters[index];
            var position = textLetter.richTextLabel.Position;

            position.Y += c_richTextLabelTitleWaveVelocity * delta;
            if (position.Y >= textLetter.waveMax)
            {
                position.Y = textLetter.waveMax;
                textLetter.waveState = TitleRichTextLabelWaveState.TopToBottom;
            }

            textLetter.richTextLabel.Position = position;
            m_titleLetters[index] = textLetter;
        }

        private void HideIcon(
            float delta
        )
        {
            const float startRotation = c_imageIconRotation;
            const float endRotation = 0f;

            m_imageIconElapsed += c_imageIconSpeed * delta;
            m_imageIconTitle.RotationDegrees = Mathf.Lerp(
                from: startRotation,
                to: endRotation,
                weight: m_imageIconElapsed
            );
            m_imageIconTitle.Scale = Vector2.One.Lerp(
                to: Vector2.Zero,
                weight: m_imageIconElapsed
            );

            if (m_imageIconElapsed >= 1f)
            {
                m_imageIconAnimationState = ImageIconAnimationState.Idle;
                m_imageIconTitle.RotationDegrees = endRotation;
                m_imageIconTitle.Scale = Vector2.Zero;
                m_imageIconTitle.Visible = false;
                m_imageIconElapsed = 0f;
            }
        }
    }
}