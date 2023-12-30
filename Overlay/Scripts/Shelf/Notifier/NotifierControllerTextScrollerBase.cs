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
            Vector2 position = m_controlHeader.Position;
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
            Vector2 position = m_controlFooter.Position;
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
                (float)elapsed
            );
        }
        if (m_isRichTextLabelHeaderScrolling)
        {
            AnimateHeaderScroll(
                (float)elapsed
            );
        }

        AnimateIcon(
            (float)elapsed
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
        public RichTextLabel richTextLabel;
        public TextLetterScrollState state;
        public float center;
        public float start;
        public float end;

        public TextLetter(
            RichTextLabel richTextLabel,
            TextLetterScrollState state,
            float center,
            float start,
            float end
        )
        {
            this.richTextLabel = richTextLabel;
            this.state = state;
            this.center = center;
            this.start = start;
            this.end = end;
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
        for (int i = 0; i < FooterText.Length; i++)
        {
            m_richTextLabelSamplerFooter.RequeueRichTextLabel(
                FooterText[i]
            );
        }
        m_footerTextLetters.Clear();
    }

    protected void DestroyHeaderTextLetters()
    {
        for (int i = 0; i < HeaderText.Length; i++)
        {
            m_richTextLabelSamplerHeader.RequeueRichTextLabel(
                HeaderText[i]
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
        for (int i = 0; i < m_footerTextLetters.Count; i++)
        {
            TextLetter textLetter = m_footerTextLetters[i];
            switch (textLetter.state)
            {
                case TextLetterScrollState.ScrollCenterToEnd:
                    ScrollCenterToEnd(
                        TextLetterType.Footer,
                        i,
                        (float)elapsed
                    );
                    break;
                case TextLetterScrollState.ScrollStartToCenter:
                    ScrollStartToCenter(
                        TextLetterType.Footer,
                        i,
                        (float)elapsed
                    );
                    break;

                case TextLetterScrollState.Idle:
                default:
                    break;
            }
        }

        if (
            m_footerTextLetters.All(
                x => x.Value.state == TextLetterScrollState.Idle
            )
        )
        {
            m_isRichTextLabelFooterScrolling = false;
        }
    }

    private void AnimateHeaderScroll(
        float elapsed    
    )
    {
        for (int i = 0; i < m_headerTextLetters.Count; i++)
        {
            TextLetter textLetter = m_headerTextLetters[i];
            switch (textLetter.state)
            {
                case TextLetterScrollState.ScrollCenterToEnd:
                    ScrollCenterToEnd(
                        TextLetterType.Header,
                        i,
                        elapsed
                    );
                    break;
                case TextLetterScrollState.ScrollStartToCenter:
                    ScrollStartToCenter(
                        TextLetterType.Header,
                        i,
                        elapsed
                    );
                    break;

                case TextLetterScrollState.Idle:
                default:
                    break;
            }
        }

        if (
            m_headerTextLetters.All(
                x => x.Value.state == TextLetterScrollState.Idle
            )
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
                    elapsed
                );
                break;

            case ImageIconAnimationState.Hiding:
                HideIcon(
                    elapsed
                );
                break;

            case ImageIconAnimationState.Idle:
            default:
                break;
        }
    }

    private void CreateFooterTextLetters()
    {
        float positionX = 0f;
        for (int i = 0; i < FooterText.Length; i++)
        {
            char letter = FooterText[i];
            RichTextLabel richTextLabel = m_richTextLabelSamplerFooter.DequeueRichTextLabel(
                letter
            );
            richTextLabel.Position = new Vector2(
                positionX,
                0f
            );

            m_footerTextLetters.Add(
                i,
                new TextLetter(
                    richTextLabel,
                    TextLetterScrollState.Idle,
                    richTextLabel.Position.X,
                    richTextLabel.Position.X + c_footerScrollDistance,
                    richTextLabel.Position.X - c_footerScrollDistance - positionX
                )
            );

            Vector2 position = m_footerTextLetters[i].richTextLabel.Position;
            position.X = m_footerTextLetters[i].start;
            m_footerTextLetters[i].richTextLabel.Position = position;

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
            for (int i = 0; i < HeaderText.Length; i++)
            {
                m_richTextLabelSamplerHeader.RequeueRichTextLabel(
                    HeaderText[i]
                );
            }
            m_headerTextLetters.Clear();
        }

        float positionX = 0f;
        for (int i = 0; i < HeaderText.Length; i++)
        {
            char letter = HeaderText[i];
            RichTextLabel richTextLabel = m_richTextLabelSamplerHeader.DequeueRichTextLabel(
                letter
            );
            richTextLabel.Position = new Vector2(
                positionX,
                0f
            );

            m_headerTextLetters.Add(
                i,
                new TextLetter(
                    richTextLabel,
                    TextLetterScrollState.Idle,
                    richTextLabel.Position.X,
                    richTextLabel.Position.X + c_headerScrollDistance,
                    richTextLabel.Position.X - c_headerScrollDistance - positionX
                )
            );

            Vector2 position = m_headerTextLetters[i].richTextLabel.Position;
            position.X = m_headerTextLetters[i].start;
            m_headerTextLetters[i].richTextLabel.Position = position;

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
            startRotation,
            endRotation,
            m_imageIconElapsed
        );
        m_imageIcon.Scale = Vector2.One.Lerp(
            Vector2.Zero,
            m_imageIconElapsed
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
            TextLetter textLetter = footerTextLetter.Value;
            if (textLetter.state != TextLetterScrollState.Idle)
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
            c_nodePathRelativeTitleImageIcon
        );

        // header letters
        m_controlHeader = GetNode<Control>(
            "HeaderViewportContainer/HeaderViewport/Header"
        );
        m_richTextLabelSamplerHeader = GetNode<RichTextLabelSampler>(
            "RichTextLabelSamplerHeader"
        );
        m_richTextLabelSamplerHeader.LoadRichTextLabelsAndAttachToParentNode(
            RichTextLabelType.NotifierHeader,
            m_controlHeader
        );
        m_initialPositionHeader = m_controlHeader.Position;

        // footer letters
        m_controlFooter = GetNode<Control>(
            "FooterViewportContainer/FooterViewport/Footer"
        );
        m_richTextLabelSamplerFooter = GetNode<RichTextLabelSampler>(
            "RichTextLabelSamplerFooter"
        );
        m_richTextLabelSamplerFooter.LoadRichTextLabelsAndAttachToParentNode(
            RichTextLabelType.NotifierFooter,
            m_controlFooter
        );
        m_initialPositionFooter = m_controlFooter.Position;
    }

    private void ScrollCenterToEnd(
        TextLetterType type,
        int index,
        float elapsed
    )
    {
        Dictionary<int, TextLetter> textLetters;
        switch (type)
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

        TextLetter textLetter = textLetters[index];
        Vector2 position = textLetter.richTextLabel.Position;

        position.X -= c_velocityScrollLetter * elapsed;
        if (position.X <= textLetter.end)
        {
            position.X = textLetter.start;
            textLetter.state = TextLetterScrollState.Idle;
            textLetter.richTextLabel.Visible = false;
        }

        textLetter.richTextLabel.Position = position;
        textLetters[index] = textLetter;
    }

    private void ScrollStartToCenter(
        TextLetterType type,
        int index,
        float elapsed
    )
    {
        Dictionary<int, TextLetter> textLetters;
        switch (type)
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

        TextLetter textLetter = textLetters[index];
        if (!textLetter.richTextLabel.Visible)
        {
            textLetter.richTextLabel.Visible = true;
        }

        Vector2 position = textLetter.richTextLabel.Position;

        position.X -= c_velocityScrollLetter * elapsed;
        if (position.X <= textLetter.center)
        {
            position.X = textLetter.center;
            textLetter.state = TextLetterScrollState.Idle;
        }

        textLetter.richTextLabel.Position = position;
        textLetters[index] = textLetter;
    }

    protected void SetImageIconState(
        ImageIconAnimationState state
    )
    {
        m_imageIconAnimationState = state;
    }

    private void ShowIcon(
        float delta
    )
    {
        if (!m_imageIcon.Visible)
        {
            m_imageIcon.Visible = true;
        }

        const float startRotation = 0f;
        const float endRotation = c_imageIconRotation;

        m_imageIconElapsed += c_imageIconSpeed * delta;
        m_imageIcon.RotationDegrees = Mathf.Lerp(
            startRotation,
            endRotation,
            m_imageIconElapsed
        );
        m_imageIcon.Scale = Vector2.Zero.Lerp(
            Vector2.One,
            m_imageIconElapsed
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
        TextLetterType type
    )
    {
        Task.Run(
            async () =>
            {
                Dictionary<int, TextLetter> textLetters;
                switch (type)
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

                for (int i = 0; i < textLetters.Count; i++)
                {
                    TextLetter textLetter = textLetters[i];
                    textLetter.state = TextLetterScrollState.ScrollStartToCenter;
                    textLetters[i] = textLetter;
                    await Task.Delay(
                        c_richTextLabelTitleDelayInMillisecondsScroll
                    );
                }
            }
        );
    }

    protected void StartScrollToEnd(
        TextLetterType type
    )
    {
        Task.Run(
            async () =>
            {
                Dictionary<int, TextLetter> textLetters;
                switch (type)
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

                for (int i = 0; i < textLetters.Count; i++)
                {
                    TextLetter textLetter = textLetters[i];
                    textLetter.state = TextLetterScrollState.ScrollCenterToEnd;
                    textLetters[i] = textLetter;
                    await Task.Delay(
                        c_richTextLabelTitleDelayInMillisecondsScroll
                    );
                }
            }
        );
    }
}