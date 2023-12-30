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
				(float)elapsed
			);
		}

		if (m_isRichTextLabelTitleScrolling)
		{
			AnimateTitleScroll(
				(float)elapsed
			);
		}
		else if (m_isRichTextLabelTitleWaving)
		{
			AnimateTitleWave(
				(float)elapsed
			);
		}

		AnimateIcon(
			(float)elapsed
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
		public RichTextLabel richTextLabel = null;
		public NameRichTextLabelFontSizeState state = NameRichTextLabelFontSizeState.Idle;
		public float max = 0f;
		public float min = 0f;

		public NameLetter(
			RichTextLabel richTextLabel,
			NameRichTextLabelFontSizeState state,
			float max,
			float min
		)
		{
			this.richTextLabel = richTextLabel;
			this.state = state;
			this.max = max;
			this.min = min;
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

	private const string c_nodePathRelativeRichTextLabelSampler = "RichTextLabelSampler";
	private const string c_nodePathRelativeRichTextLabelSamplerParentNodeTarget = "TitleViewportContainer/TitleViewport/Title";
	private const string c_nodePathRelativeTitleImageIcon = "ImageViewportContainer/ImageViewport/Icon";

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
	private const uint c_richTextLabelTitleWaveCount = 2u;
	private const float c_richTextLabelTitleWaveVelocity = 5f;
	private const float c_richTextLabelTitleWaveHeight = 1f;

	private const float c_imageIconSpeed = 2f;
	private const float c_imageIconRotation = -2160f;

	private const int c_taskAwaitDelayTimeInMilliseconds = 20;

	private Dictionary<int, NameLetter> m_nameLetters = new();
	private Dictionary<int, TitleLetter> m_titleLetters = new();
	private RichTextLabelSampler m_richTextLabelSampler = null;
	private List<NameplateTitle> m_nameplateTitles = new();
	private int m_currentTitleIndex = 0;

	private TextureRect m_imageIconTitle = null;
	private ImageIconAnimationState m_imageIconAnimationState = ImageIconAnimationState.Idle;
	private float m_imageIconElapsed = 0f;

	private bool m_isRichTextLabelNameAnimating = false;
	private bool m_isRichTextLabelTitleAddTitle = false;
	private bool m_isRichTextLabelTitleScrolling = false;
	private bool m_isRichTextLabelTitleWaving = false;

	private bool m_isRunning = true;

	private void RetrieveResources()
	{
		m_richTextLabelSampler = GetNode<RichTextLabelSampler>(
			c_nodePathRelativeRichTextLabelSampler
		);

		var richTextLabelSamplerParentNodeTarget = GetNode(
			c_nodePathRelativeRichTextLabelSamplerParentNodeTarget
		);
		m_richTextLabelSampler.LoadRichTextLabelsAndAttachToParentNode(
			RichTextLabelType.NameplateTitle,
			richTextLabelSamplerParentNodeTarget
		);

		m_imageIconTitle = GetNode<TextureRect>(
			c_nodePathRelativeTitleImageIcon
		);

		var nodeNameMask = GetNode(
			"NameViewportContainer/NameViewport"
		);
		var nodeLetters = nodeNameMask.GetChildren();
		for (int i = 0; i < nodeLetters.Count; i++)
		{
			m_nameLetters.Add(
				i,
				new NameLetter(
					nodeLetters[i] as RichTextLabel,
					NameRichTextLabelFontSizeState.Idle,
					c_richTextLabelNameScaleMin,
					c_richTextLabelNameScaleMax
				)
			);
		}
	}

	private void AnimateName()
	{
		Task.Run(
			async () =>
			{
				Random random = new();
				while (m_isRunning)
				{
					await Task.Delay(
						random.Next(
							c_richTextLabelNameAnimationDelayTimeInMillisecondsMin,
							c_richTextLabelNameAnimationDelayTimeInMillisecondsMax
						)
					);

					m_isRichTextLabelNameAnimating = true;
					for(int i = 0; i < m_nameLetters.Count; i++)
					{
						NameLetter nameLetter = m_nameLetters[i];
						nameLetter.state = NameRichTextLabelFontSizeState.IncreaseFontSize;
						m_nameLetters[i] = nameLetter;
						await Task.Delay(
							c_richTextLabelNameDelayTimeInMilliseconds
						);
					}

					while (m_isRichTextLabelNameAnimating)
					{
						await Task.Delay(
							c_taskAwaitDelayTimeInMilliseconds
						);
					}
				}
			}
		);
	}

	private void AnimateTitles()
	{
		Task.Run(
			async () =>
			{
				m_nameplateTitles.Add(
					new NameplateTitle(
						"Pirate Philanthropist",
						GD.Load<CompressedTexture2D>(
							"res://Overlay/Textures/Icons/Icon_PiratePhilanthropist.png"
						)
					)
				);
				m_nameplateTitles.Add(
					new NameplateTitle(
						"Corporate Spy",
						GD.Load<CompressedTexture2D>(
							"res://Overlay/Textures/Icons/Icon_CorporateSpy.png"
						)
					)
				);
				m_nameplateTitles.Add(
					new NameplateTitle(
						"One-Trick Stitches",
						GD.Load<CompressedTexture2D>(
							"res://Overlay/Textures/Icons/Icon_OneTrickStitches.png"
						)
					)
				);

				Random random = new();
				while (m_isRunning)
				{
					m_isRichTextLabelTitleAddTitle = true;
					while (m_isRichTextLabelTitleAddTitle);
					
					m_imageIconAnimationState = ImageIconAnimationState.Showing;
					m_isRichTextLabelTitleScrolling = true;
					for (int i = 0; i < m_titleLetters.Count; i++)
					{
						TitleLetter titleLetter = m_titleLetters[i];
						titleLetter.scrollState = TitleRichTextLabelScrollState.ScrollStartToCenter;
						m_titleLetters[i] = titleLetter;
						await Task.Delay(
							c_richTextLabelTitleDelayInMillisecondsScroll
						);
					}

					while (m_isRichTextLabelTitleScrolling);

					for (uint i = 0u; i < c_richTextLabelTitleWaveCount; i++)
					{
						await Task.Delay(
							random.Next(
								c_richTextLabelTitleDelayInMillisecondsMin,
								c_richTextLabelTitleDelayInMillisecondsMax
							)
						);

						m_isRichTextLabelTitleWaving = true;
						for (int j = 0; j < m_titleLetters.Count; j++)
						{
							TitleLetter titleLetter = m_titleLetters[j];
							titleLetter.waveState = TitleRichTextLabelWaveState.CenterToTop;
							m_titleLetters[j] = titleLetter;
							await Task.Delay(
								c_richTextLabelTitleDelayInMillisecondsWave
							);
						}

						while (m_isRichTextLabelTitleWaving);
					}

					await Task.Delay(
						random.Next(
							c_richTextLabelTitleDelayInMillisecondsMin,
							c_richTextLabelTitleDelayInMillisecondsMax
						)
					);

					m_imageIconAnimationState = ImageIconAnimationState.Hiding;
					m_isRichTextLabelTitleScrolling = true;
					for (int i = 0; i < m_titleLetters.Count; i++)
					{
						TitleLetter titleLetter = m_titleLetters[i];
						titleLetter.scrollState = TitleRichTextLabelScrollState.ScrollCenterToEnd;
						m_titleLetters[i] = titleLetter;
						await Task.Delay(
							c_richTextLabelTitleDelayInMillisecondsScroll
						);
					}

					while (m_isRichTextLabelTitleScrolling);

					RemoveRichTextLabelTitle();
					await Task.Delay(
						c_richTextLabelTitleDelayInMillisecondsWaveNext
					);

					if (++m_currentTitleIndex == m_nameplateTitles.Count)
					{
						m_currentTitleIndex = 0;
					}
				}
			}
		);
	}

	private void AnimateNameScale(
		float elapsed    
	)
	{
		for (int i = 0; i < m_nameLetters.Count; i++)
		{
			NameLetter nameLetter = m_nameLetters[i];
			switch (nameLetter.state)
			{
				case NameRichTextLabelFontSizeState.IncreaseFontSize:
					IncreaseNameLetterFontSize(
						i,
						elapsed
					);
					break;

				case NameRichTextLabelFontSizeState.DecreaseFontSize:
					DecreaseNameLetterFontSize(
						i,
						elapsed
					);
					break;

				case NameRichTextLabelFontSizeState.Idle:
				default:
					break;
			}
		}

		if (
			m_nameLetters.All(
				x => x.Value.state == NameRichTextLabelFontSizeState.Idle
			)
		)
		{
			m_isRichTextLabelNameAnimating = false;
		}
	}

	private void AnimateTitleScroll(
		float elapsed    
	)
	{
		for (int i = 0; i < m_titleLetters.Count; i++)
		{
			TitleLetter titleLetter = m_titleLetters[i];
			switch (titleLetter.scrollState)
			{
				case TitleRichTextLabelScrollState.ScrollCenterToEnd:
					ScrollTitleLetterCenterToEnd(
						i,
						elapsed
					);
					break;

				case TitleRichTextLabelScrollState.ScrollStartToCenter:
					ScrollTitleLetterStartToCenter(
						i,
						elapsed
					);
					break;

				case TitleRichTextLabelScrollState.Idle:
				default:
					break;
			}
		}

		if (
			m_titleLetters.All(
				x => x.Value.scrollState == TitleRichTextLabelScrollState.Idle
			)
		)
		{
			m_isRichTextLabelTitleScrolling = false;
		}
	}

	private void AnimateTitleWave(
		float elapsed
	)
	{
		for (int i = 0; i < m_titleLetters.Count; i++)
		{
			TitleLetter titleLetter = m_titleLetters[i];
			switch (titleLetter.waveState)
			{
				case TitleRichTextLabelWaveState.CenterToTop:
					WaveTitleLetterToTop(
						i,
						elapsed
					);
					break;

				case TitleRichTextLabelWaveState.TopToBottom:
					WaveTitleLetterToBottom(
						i,
						elapsed
					);
					break;

				case TitleRichTextLabelWaveState.BottomToCenter:
					WaveTitleLetterToCenter(
						i,
						elapsed
					);
					break;

				case TitleRichTextLabelWaveState.Center:
				default:
					break;
			}
		}

		if (
			m_titleLetters.All(
				x => x.Value.waveState == TitleRichTextLabelWaveState.Center
			)
		)
		{
			m_isRichTextLabelTitleWaving = false;
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

	private void AddRichTextLabelTitle()
	{
		string title = m_nameplateTitles[m_currentTitleIndex].textTitle;
		float positionX = 0f;
		for (int i = 0; i < title.Length; i++)
		{
			char letter = title[i];
			RichTextLabel richTextLabel = m_richTextLabelSampler.DequeueRichTextLabel(
				letter
			);
			richTextLabel.SetPosition(
					new Vector2(
					positionX,
					0f
				)
			);

			m_titleLetters.Add(
				i,
				new TitleLetter(
					letter,
					richTextLabel,
					TitleRichTextLabelScrollState.Idle,
					positionX - c_richTextLabelTitleScrollDistance,
					positionX - c_richTextLabelTitleScrollDistance * 2u,
					positionX,
					TitleRichTextLabelWaveState.Center,
					0,
					c_richTextLabelTitleWaveHeight,
					-c_richTextLabelTitleWaveHeight
				)
			);

			positionX += richTextLabel.GetContentWidth();
		}

		m_isRichTextLabelTitleAddTitle = false;
	}

	private void RemoveRichTextLabelTitle()
	{
		foreach (var titleLetter in m_titleLetters.Values)
		{
			m_richTextLabelSampler.RequeueRichTextLabel(
				titleLetter.letter
			);
		}

		m_titleLetters.Clear();
	}

	private void IncreaseNameLetterFontSize(
		int index,
		float delta
	)
	{
		NameLetter nameLetter = m_nameLetters[index];
		Vector2 scale = nameLetter.richTextLabel.Scale;

		scale.X += c_richTextLabelNameScaleVelocity * delta;
		if (scale.X >= c_richTextLabelNameScaleMax)
		{
			scale.X = c_richTextLabelNameScaleMax;
			nameLetter.state = NameRichTextLabelFontSizeState.DecreaseFontSize;
		}
		scale.Y = scale.X;

		nameLetter.richTextLabel.Scale = scale;
		m_nameLetters[index] = nameLetter;
	}

	private void DecreaseNameLetterFontSize(
		int index,
		float delta
	)
	{
		NameLetter nameLetter = m_nameLetters[index];
		Vector2 scale = nameLetter.richTextLabel.Scale;

		scale.X -= c_richTextLabelNameScaleVelocity * delta;
		if (scale.X <= c_richTextLabelNameScaleMin)
		{
			scale.X = c_richTextLabelNameScaleMin;
			nameLetter.state = NameRichTextLabelFontSizeState.Idle;
		}
		scale.Y = scale.X;

		nameLetter.richTextLabel.Scale = scale;
		m_nameLetters[index] = nameLetter;
	}

	private void WaveTitleLetterToTop(
		int index,
		float delta
	)
	{
		TitleLetter textLetter = m_titleLetters[index];
		Vector2 position = textLetter.richTextLabel.Position;

		position.Y += c_richTextLabelTitleWaveVelocity * delta;
		if (position.Y >= textLetter.waveMax)
		{
			position.Y = textLetter.waveMax;
			textLetter.waveState = TitleRichTextLabelWaveState.TopToBottom;
		}

		textLetter.richTextLabel.Position = position;
		m_titleLetters[index] = textLetter;
	}

	private void WaveTitleLetterToBottom(
		int index,
		float delta
	)
	{
		TitleLetter textLetter = m_titleLetters[index];
		Vector2 position = textLetter.richTextLabel.Position;

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
		TitleLetter textLetter = m_titleLetters[index];
		Vector2 position = textLetter.richTextLabel.Position;

		position.Y += c_richTextLabelTitleWaveVelocity * delta;
		if (position.Y >= textLetter.waveCenter)
		{
			position.Y = textLetter.waveCenter;
			textLetter.waveState = TitleRichTextLabelWaveState.Center;
		}

		textLetter.richTextLabel.Position = position;
		m_titleLetters[index] = textLetter;
	}

	private void ScrollTitleLetterCenterToEnd(
		int index,
		float delta
	)
	{
		TitleLetter textLetter = m_titleLetters[index];
		Vector2 position = textLetter.richTextLabel.Position;

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
		TitleLetter textLetter = m_titleLetters[index];
		if (!textLetter.richTextLabel.Visible)
		{
			textLetter.richTextLabel.Visible = true;
		}

		Vector2 position = textLetter.richTextLabel.Position;

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
		if (!m_imageIconTitle.Visible)
		{
			m_imageIconTitle.Texture = m_nameplateTitles[m_currentTitleIndex].textureIcon;
			m_imageIconTitle.Visible = true;
		}

		const float startRotation = 0f;
		const float endRotation = c_imageIconRotation;

		m_imageIconElapsed += c_imageIconSpeed * delta;
		m_imageIconTitle.RotationDegrees = Mathf.Lerp(
			startRotation,
			endRotation,
			m_imageIconElapsed
		);
		m_imageIconTitle.Scale = Vector2.Zero.Lerp(
			Vector2.One,
			m_imageIconElapsed
		);

		if (m_imageIconElapsed >= 1f)
		{
			m_imageIconAnimationState = ImageIconAnimationState.Idle;
			m_imageIconTitle.RotationDegrees = endRotation;
			m_imageIconTitle.Scale = Vector2.One;
			m_imageIconElapsed = 0f;
		}
	}

	private void HideIcon(
		float delta
	)
	{
		const float startRotation = c_imageIconRotation;
		const float endRotation = 0f;

		m_imageIconElapsed += c_imageIconSpeed * delta;
		m_imageIconTitle.RotationDegrees = Mathf.Lerp(
			startRotation,
			endRotation,
			m_imageIconElapsed
		);
		m_imageIconTitle.Scale = Vector2.One.Lerp(
			Vector2.Zero,
			m_imageIconElapsed
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
