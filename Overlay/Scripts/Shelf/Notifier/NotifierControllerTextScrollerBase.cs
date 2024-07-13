
namespace Overlay
{
    using Godot;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using RichTextLabelType = RichTextLabelSampler.RichTextLabelType;

    public abstract partial class NotifierControllerTextScrollerBase : NotifierControllerBase
    {
        public override void _EnterTree()
        {
            RetrieveResources();
        }

        public override void _Process(
            double elapsed
        )
        {
            if (m_createHeader)
            {
                CreateHeaderTextLetters();
            }
            else if (m_moveHeader)
            {
                var position = m_controlHeader.Position;
                position.X -= c_velocityScrollControl * (float)elapsed;

                if (position.X <= m_headerTargetX)
                {
                    position.X = m_headerTargetX;
                    m_moveHeader = false;
                }

                m_controlHeader.Position = position;
            }

            if (m_createFooter)
            {
                CreateFooterTextLetters();
            }
            else if (m_moveFooter)
            {
                var position = m_controlFooter.Position;
                position.X -= c_velocityScrollControl * (float)elapsed;

                if (position.X <= m_footerTargetX)
                {
                    position.X = m_footerTargetX;
                    m_moveFooter = false;
                }

                m_controlFooter.Position = position;
            }

            if (m_isRichTextLabelFooterScrolling)
            {
                AnimateFooterScroll(
                    elapsed: (float)elapsed
                );
            }
            if (m_isRichTextLabelHeaderScrolling)
            {
                AnimateHeaderScroll(
                    elapsed: (float)elapsed
                );
            }

            AnimateIcon(
                elapsed: (float)elapsed
            );

            if (m_resetFooter)
            {
                ResetFooterToInitialPosition();
            }
            if (m_resetHeader)
            {
                ResetHeaderToInitialPosition();
            }
        }

        public override void _Ready()
        {
            CreateHeaderTextLetters();
            CreateFooterTextLetters();
        }

        protected enum ImageIconAnimationState : uint
        {
            Showing = 0u,
            Hiding,
            Idle
        }

        protected enum TextLetterScrollState : uint
        {
            Idle = 0u,
            ScrollCenterToEnd,
            ScrollStartToCenter
        }

        protected enum TextLetterType : uint
        {
            Header = 0u,
            Footer
        }

        protected struct TextLetter
        {
            public RichTextLabel RichTextLabel;
            public TextLetterScrollState TextLetterScrollState;
            public float Center;
            public float Start;
            public float End;

            public TextLetter(
                RichTextLabel richTextLabel,
                TextLetterScrollState textLetterScrollState,
                float center,
                float start,
                float end
            )
            {
                this.RichTextLabel = richTextLabel;
                this.TextLetterScrollState = textLetterScrollState;
                this.Center = center;
                this.Start = start;
                this.End = end;
            }
        }

        protected const float c_headerScrollDistance = 328f;
        protected const float c_footerScrollDistance = 300f;

        protected const int c_richTextLabelOnScreenDuration = 8000;
        protected const int c_richTextLabelTitleDelayInMillisecondsFooter = 500;
        protected const int c_richTextLabelTitleDelayInMillisecondsCompletion = 2000;
        protected const float c_velocityScrollControl = 150f;
        protected const float c_velocityScrollControlInMilliseconds = c_velocityScrollControl * 1000f;

        private const string c_nodePathRelativeTitleImageIcon = "ImageViewportContainer/ImageViewport/Icon";

        private const int c_richTextLabelTitleDelayInMillisecondsScroll = 20;
        private const float c_velocityScrollLetter = 650f;

        private const float c_imageIconSpeed = 2f;
        private const float c_imageIconRotation = -2160f;

        protected virtual string FooterText { get; set; } = string.Empty;
        protected virtual string HeaderText { get; set; } = string.Empty;

        protected float FooterDistanceOffScreen { get { return m_footerDistanceOffScreen; } }
        protected float HeaderDistanceOffScreen { get { return m_headerDistanceOffScreen; } }

        protected Dictionary<int, TextLetter> m_footerTextLetters = new();
        protected Dictionary<int, TextLetter> m_headerTextLetters = new();

        protected RichTextLabelSampler m_richTextLabelSamplerFooter = null;
        protected bool m_createFooter = false;
        protected bool m_createHeader = false;
        protected bool m_moveFooter = false;
        protected bool m_moveHeader = false;

        protected void DestroyFooterTextLetters()
        {
            for (var i = 0; i < FooterText.Length; i++)
            {
                m_richTextLabelSamplerFooter.RequeueRichTextLabel(
                    letter: FooterText[i]
                );
            }
            m_footerTextLetters.Clear();
        }

        protected void DestroyHeaderTextLetters()
        {
            for (var i = 0; i < HeaderText.Length; i++)
            {
                m_richTextLabelSamplerHeader.RequeueRichTextLabel(
                    letter: HeaderText[i]
                );
            }
            m_headerTextLetters.Clear();
        }

        protected void ResetFooter()
        {
            m_resetFooter = true;
        }

        protected void ResetHeader()
        {
            m_resetHeader = true;
        }

        private RichTextLabelSampler m_richTextLabelSamplerHeader = null;
        private Control m_controlFooter = null;
        private Control m_controlHeader = null;

        private Vector2 m_initialPositionFooter = Vector2.Zero;
        private Vector2 m_initialPositionHeader = Vector2.Zero;
        private float m_footerTargetX = 0f;
        private float m_headerTargetX = 0f;
        private float m_footerDistanceOffScreen = 0f;
        private float m_headerDistanceOffScreen = 0f;
        private bool m_resetFooter = false;
        private bool m_resetHeader = false;

        private TextureRect m_imageIcon = null;
        private ImageIconAnimationState m_imageIconAnimationState = ImageIconAnimationState.Idle;
        private float m_imageIconElapsed = 0f;

        private bool m_isRichTextLabelFooterScrolling = false;
        private bool m_isRichTextLabelHeaderScrolling = false;

        private void AnimateFooterScroll(
            float elapsed
        )
        {
            for (var i = 0; i < m_footerTextLetters.Count; i++)
            {
                var textLetter = m_footerTextLetters[i];
                switch (textLetter.TextLetterScrollState)
                {
                    case TextLetterScrollState.ScrollCenterToEnd:
                        ScrollCenterToEnd(
                            textLetterType: TextLetterType.Footer,
                            index: i,
                            elapsed: (float)elapsed
                        );
                        break;
                    case TextLetterScrollState.ScrollStartToCenter:
                        ScrollStartToCenter(
                            textLetterType: TextLetterType.Footer,
                            index: i,
                            elapsed: (float)elapsed
                        );
                        break;

                    case TextLetterScrollState.Idle:
                    default:
                        break;
                }
            }

            if (
                m_footerTextLetters.All(
                    predicate: x => 
                    x.Value.TextLetterScrollState is TextLetterScrollState.Idle
                ) is true
            )
            {
                m_isRichTextLabelFooterScrolling = false;
            }
        }

        private void AnimateHeaderScroll(
            float elapsed
        )
        {
            for (var i = 0; i < m_headerTextLetters.Count; i++)
            {
                var textLetter = m_headerTextLetters[i];
                switch (textLetter.TextLetterScrollState)
                {
                    case TextLetterScrollState.ScrollCenterToEnd:
                        ScrollCenterToEnd(
                            textLetterType: TextLetterType.Header,
                            index: i,
                            elapsed: elapsed
                        );
                        break;
                    case TextLetterScrollState.ScrollStartToCenter:
                        ScrollStartToCenter(
                            textLetterType: TextLetterType.Header,
                            index: i,
                            elapsed: elapsed
                        );
                        break;

                    case TextLetterScrollState.Idle:
                    default:
                        break;
                }
            }

            if (
                m_headerTextLetters.All(
                    predicate: x => 
                    x.Value.TextLetterScrollState == TextLetterScrollState.Idle
                ) is true
            )
            {
                m_isRichTextLabelHeaderScrolling = false;
            }
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

        private void CreateFooterTextLetters()
        {
            var positionX = 0f;
            for (var i = 0; i < FooterText.Length; i++)
            {
                var letter = FooterText[i];
                var richTextLabel = m_richTextLabelSamplerFooter.DequeueRichTextLabel(
                    letter: letter
                );
                richTextLabel.Position = new Vector2(
                    x: positionX,
                    y: 0f
                );

                m_footerTextLetters.Add(
                    key: i,
                    value: new(
                        richTextLabel: richTextLabel,
                        textLetterScrollState: TextLetterScrollState.Idle,
                        center: richTextLabel.Position.X,
                        start: richTextLabel.Position.X + c_footerScrollDistance,
                        end: richTextLabel.Position.X - c_footerScrollDistance - positionX
                    )
                );

                var position = m_footerTextLetters[i].RichTextLabel.Position;
                position.X = m_footerTextLetters[i].Start;
                m_footerTextLetters[i].RichTextLabel.Position = position;

                positionX += richTextLabel.GetContentWidth();
            }

            m_footerDistanceOffScreen = positionX - c_footerScrollDistance;
            m_footerTargetX = m_initialPositionFooter.X - m_footerDistanceOffScreen;
            m_createFooter = false;
        }

        private void CreateHeaderTextLetters()
        {
            if (m_headerTextLetters.Count > 0u)
            {
                for (var i = 0; i < HeaderText.Length; i++)
                {
                    m_richTextLabelSamplerHeader.RequeueRichTextLabel(
                        letter: HeaderText[i]
                    );
                }
                m_headerTextLetters.Clear();
            }

            var positionX = 0f;
            for (var i = 0; i < HeaderText.Length; i++)
            {
                var letter = HeaderText[i];
                var richTextLabel = m_richTextLabelSamplerHeader.DequeueRichTextLabel(
                    letter: letter
                );
                richTextLabel.Position = new Vector2(
                    x: positionX,
                    y: 0f
                );

                m_headerTextLetters.Add(
                    key: i,
                    value: new(
                        richTextLabel: richTextLabel,
                        textLetterScrollState: TextLetterScrollState.Idle,
                        center: richTextLabel.Position.X,
                        start: richTextLabel.Position.X + c_headerScrollDistance,
                        end: richTextLabel.Position.X - c_headerScrollDistance - positionX
                    )
                );

                var position = m_headerTextLetters[i].RichTextLabel.Position;
                position.X = m_headerTextLetters[i].Start;
                m_headerTextLetters[i].RichTextLabel.Position = position;

                positionX += richTextLabel.GetContentWidth();
            }

            m_headerDistanceOffScreen = positionX - c_headerScrollDistance;
            m_headerTargetX = m_initialPositionHeader.X - m_headerDistanceOffScreen;
            m_createHeader = false;
        }

        private void HideIcon(
            float delta
        )
        {
            const float startRotation = c_imageIconRotation;
            const float endRotation = 0f;

            m_imageIconElapsed += c_imageIconSpeed * delta;
            m_imageIcon.RotationDegrees = Mathf.Lerp(
                from: startRotation,
                to: endRotation,
                weight: m_imageIconElapsed
            );
            m_imageIcon.Scale = Vector2.One.Lerp(
                to: Vector2.Zero,
                weight: m_imageIconElapsed
            );

            if (m_imageIconElapsed >= 1f)
            {
                m_imageIconAnimationState = ImageIconAnimationState.Idle;
                m_imageIcon.RotationDegrees = endRotation;
                m_imageIcon.Scale = Vector2.Zero;
                m_imageIcon.Visible = false;
                m_imageIconElapsed = 0f;
            }
        }

        protected bool IsFooterTextCentered()
        {
            foreach (var footerTextLetter in m_footerTextLetters)
            {
                var textLetter = footerTextLetter.Value;
                if (textLetter.TextLetterScrollState is not TextLetterScrollState.Idle)
                {
                    return false;
                }
            }
            return true;
        }

        private void ResetFooterToInitialPosition()
        {
            m_controlFooter.Position = m_initialPositionFooter;
            m_resetFooter = false;
        }

        private void ResetHeaderToInitialPosition()
        {
            m_controlHeader.Position = m_initialPositionHeader;
            m_resetHeader = false;
        }

        private void RetrieveResources()
        {
            m_imageIcon = GetNode<TextureRect>(
                path: c_nodePathRelativeTitleImageIcon
            );

            // header letters
            m_controlHeader = GetNode<Control>(
                path: "HeaderViewportContainer/HeaderViewport/Header"
            );
            m_richTextLabelSamplerHeader = GetNode<RichTextLabelSampler>(
                path: "RichTextLabelSamplerHeader"
            );
            m_richTextLabelSamplerHeader.LoadRichTextLabelsAndAttachToParentNode(
                richTextLabelType: RichTextLabelType.NotifierHeader,
                parent: m_controlHeader
            );
            m_initialPositionHeader = m_controlHeader.Position;

            // footer letters
            m_controlFooter = GetNode<Control>(
                path: "FooterViewportContainer/FooterViewport/Footer"
            );
            m_richTextLabelSamplerFooter = GetNode<RichTextLabelSampler>(
                path: "RichTextLabelSamplerFooter"
            );
            m_richTextLabelSamplerFooter.LoadRichTextLabelsAndAttachToParentNode(
                richTextLabelType: RichTextLabelType.NotifierFooter,
                parent: m_controlFooter
            );
            m_initialPositionFooter = m_controlFooter.Position;
        }

        private void ScrollCenterToEnd(
            TextLetterType textLetterType,
            int index,
            float elapsed
        )
        {
            Dictionary<int, TextLetter> textLetters;
            switch (textLetterType)
            {
                case TextLetterType.Header:
                    textLetters = m_headerTextLetters;
                    break;

                case TextLetterType.Footer:
                    textLetters = m_footerTextLetters;
                    break;

                default:
                    return;
            }

            var textLetter = textLetters[index];
            var position = textLetter.RichTextLabel.Position;
            position.X -= c_velocityScrollLetter * elapsed;
            if (position.X <= textLetter.End)
            {
                position.X = textLetter.Start;
                textLetter.TextLetterScrollState = TextLetterScrollState.Idle;
                textLetter.RichTextLabel.Visible = false;
            }

            textLetter.RichTextLabel.Position = position;
            textLetters[index] = textLetter;
        }

        private void ScrollStartToCenter(
            TextLetterType textLetterType,
            int index,
            float elapsed
        )
        {
            Dictionary<int, TextLetter> textLetters;
            switch (textLetterType)
            {
                case TextLetterType.Header:
                    textLetters = m_headerTextLetters;
                    break;

                case TextLetterType.Footer:
                    textLetters = m_footerTextLetters;
                    break;

                default:
                    return;
            }

            var textLetter = textLetters[index];
            if (textLetter.RichTextLabel.Visible is false)
            {
                textLetter.RichTextLabel.Visible = true;
            }

            var position = textLetter.RichTextLabel.Position;
            position.X -= c_velocityScrollLetter * elapsed;
            if (position.X <= textLetter.Center)
            {
                position.X = textLetter.Center;
                textLetter.TextLetterScrollState = TextLetterScrollState.Idle;
            }

            textLetter.RichTextLabel.Position = position;
            textLetters[index] = textLetter;
        }

        protected void SetImageIconState(
            ImageIconAnimationState imageIconAnimationState
        )
        {
            m_imageIconAnimationState = imageIconAnimationState;
        }

        private void ShowIcon(
            float delta
        )
        {
            if (m_imageIcon.Visible is false)
            {
                m_imageIcon.Visible = true;
            }

            const float startRotation = 0f;
            const float endRotation = c_imageIconRotation;

            m_imageIconElapsed += c_imageIconSpeed * delta;
            m_imageIcon.RotationDegrees = Mathf.Lerp(
                from: startRotation,
                to: endRotation,
                weight: m_imageIconElapsed
            );
            m_imageIcon.Scale = Vector2.Zero.Lerp(
                to: Vector2.One,
                weight: m_imageIconElapsed
            );

            if (m_imageIconElapsed >= 1f)
            {
                m_imageIconAnimationState = ImageIconAnimationState.Idle;
                m_imageIcon.RotationDegrees = endRotation;
                m_imageIcon.Scale = Vector2.One;
                m_imageIconElapsed = 0f;
            }
        }

        protected void StartScrollToCenter(
            TextLetterType textLetterType
        )
        {
            Task.Run(
                function:
                async () =>
                {
                    Dictionary<int, TextLetter> textLetters;
                    switch (textLetterType)
                    {
                        case TextLetterType.Header:
                            m_isRichTextLabelHeaderScrolling = true;
                            textLetters = m_headerTextLetters;
                            break;

                        case TextLetterType.Footer:
                            m_isRichTextLabelFooterScrolling = true;
                            textLetters = m_footerTextLetters;
                            break;

                        default:
                            return;
                    }

                    for (var i = 0; i < textLetters.Count; i++)
                    {
                        var textLetter = textLetters[i];
                        textLetter.TextLetterScrollState = TextLetterScrollState.ScrollStartToCenter;
                        textLetters[i] = textLetter;
                        await Task.Delay(
                           millisecondsDelay: c_richTextLabelTitleDelayInMillisecondsScroll
                        );
                    }
                }
            );
        }

        protected void StartScrollToEnd(
            TextLetterType textLetterType
        )
        {
            Task.Run(
                function:
                async () =>
                {
                    Dictionary<int, TextLetter> textLetters;
                    switch (textLetterType)
                    {
                        case TextLetterType.Header:
                            m_isRichTextLabelHeaderScrolling = true;
                            textLetters = m_headerTextLetters;
                            break;

                        case TextLetterType.Footer:
                            m_isRichTextLabelFooterScrolling = true;
                            textLetters = m_footerTextLetters;
                            break;

                        default:
                            return;
                    }

                    for (var i = 0; i < textLetters.Count; i++)
                    {
                        var textLetter = textLetters[i];
                        textLetter.TextLetterScrollState = TextLetterScrollState.ScrollCenterToEnd;
                        textLetters[i] = textLetter;
                        await Task.Delay(
                            millisecondsDelay: c_richTextLabelTitleDelayInMillisecondsScroll
                        );
                    }
                }
            );
        }
    }
}