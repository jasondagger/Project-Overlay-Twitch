
namespace Overlay
{
	using Godot;
	using System.Collections.Generic;
	using static Godot.Image;
	using NodeType = NodeDirectory.NodeType;

	public sealed partial class ShelfImageAudioWave : ShelfImage
	{
		public override void _EnterTree()
		{
			RetrieveResources();
			CreateImageTextures();
			CreateImageMask();
			CreateImageWaveBorder();
			SetStartingDepths();
			SetShaderMaterial();
		}

		public override void _Process(
			double delta
		)
		{
			CalculateSoundtrackSoundWaveData();
			UpdateImageWave();
			UpdateShaderResources();
		}

		private const int c_borderWaveDepth = 24;
		private const int c_borderWaveStart = 32;
		private const int c_borderWaveEndHeight = c_textureHeight - c_borderWaveStart;
		private const int c_borderWaveEndWidth = c_textureWidth - c_borderWaveStart;

		private const int c_waveStart = c_borderWaveStart + c_borderWaveDepth;
		private const int c_waveEndHeight = c_textureHeight - c_waveStart;
		private const int c_waveEndWidth = c_textureWidth - c_waveStart;

		private const int c_waveDepthData = 48;
		private const int c_waveDataSampleCount = 64;
		private const int c_waveDataCount = c_waveDataSampleCount * 2;

		private PastelInterpolator m_pastelInterpolator = null;
		private Image m_imageMask = null;
		private Image m_imageWave = null;
		private ImageTexture m_textureMask = new();
		private ImageTexture m_textureWave = new();
		private ShaderMaterial m_material = null;

		private float[] m_waveData = new float[c_waveDataCount];

		private Dictionary<int, int> m_texturePixelDepthHeights = new();
		private Dictionary<int, int> m_texturePixelDepthWidths = new();

		private float CalculateGreatestWaveForCoordinateValue(
			int coordinateValue
		)
		{
			return m_waveData[coordinateValue % c_waveDataCount] * c_waveDepthData + c_waveStart;
		}

		private void CalculateSoundtrackSoundWaveData()
		{
			for (int i = 0; i < c_waveDataSampleCount; i++)
			{
				int leftWaveIndex = i;
				m_waveData[leftWaveIndex] = 0f;

				int rightWaveIndex = c_waveDataCount - 1 - i;
				m_waveData[rightWaveIndex] = 0f;
			}
		}

		private void CreateImageTextures()
		{
			m_imageMask = Create(
				c_textureWidth,
				c_textureHeight,
				false,
				Format.Rgbaf
			);
			m_imageWave = Create(
				c_textureWidth,
				c_textureHeight,
				false,
				Format.Rgbaf
			);
			m_textureMask.SetImage(
				m_imageMask
			);
			m_textureWave.SetImage(
				m_imageWave
			);
		}

		private void CreateImageMask()
		{
			const int textureDepthWidth = c_textureWidth - c_borderWaveStart - 1;
			const int textureDepthHeight = c_textureHeight - c_borderWaveStart - 1;

			for (int x = 0; x < c_textureWidth; x++)
			{
				for (int y = 0; y < c_textureHeight; y++)
				{
					m_imageMask.SetPixel(
						x,
						y,
						x < c_borderWaveStart ||
						x >= textureDepthWidth ||
						y < c_borderWaveStart ||
						y >= textureDepthHeight ?
							Colors.Transparent : Colors.White
					);
				}
			}

			m_textureMask.Update(
				m_imageMask
			);
		}

		private void CreateImageWaveBorder()
		{
			for (int i = 0; i < c_borderWaveDepth; i++)
			{
				for (int xCoordinateNear = c_borderWaveStart; xCoordinateNear < c_borderWaveEndWidth; xCoordinateNear++)
				{
					int depthNear = i + c_borderWaveStart;
					m_imageWave.SetPixel(
						xCoordinateNear,
						depthNear,
						Colors.White
					);

					int xCoordinateFar = c_textureWidth - xCoordinateNear;
					int depthFar = c_textureHeight - depthNear;
					m_imageWave.SetPixel(
						xCoordinateFar,
						depthFar,
						Colors.White
					);
				}
				for (int yCoordinateNear = c_borderWaveStart; yCoordinateNear < c_borderWaveEndHeight; yCoordinateNear++)
				{
					int depthNear = i + c_borderWaveStart;
					m_imageWave.SetPixel(
						depthNear,
						yCoordinateNear,
						Colors.White
					);

					int yCoordinateFar = c_textureHeight - yCoordinateNear;
					int depthFar = c_textureWidth - depthNear;
					m_imageWave.SetPixel(
						depthFar,
						yCoordinateFar,
						Colors.White
					);
				}
			}
		}

		private void RetrieveResources()
		{
			m_pastelInterpolator = GetNode<PastelInterpolator>(
				NodeDirectory.NodePaths[NodeType.PastelInterpolator]
			);
		}

		private void SetShaderMaterial()
		{
			m_material = (ShaderMaterial)Get(
				"material"
			);
			m_material.SetShaderParameter(
				"textureMask",
				m_textureMask
			);
			m_material.SetShaderParameter(
				"textureWave",
				m_textureWave
			);
			m_material.SetShaderParameter(
				"color",
				m_pastelInterpolator.GetColor()
			);
		}

		private void SetStartingDepths()
		{
			for (int i = c_waveStart; i < c_waveEndHeight; i++)
			{
				m_texturePixelDepthHeights.Add(
					i,
					c_waveStart
				);
			}
			for (int i = c_waveStart; i < c_waveEndWidth; i++)
			{
				m_texturePixelDepthWidths.Add(
					i,
					c_waveStart
				);
			}
		}

		private void UpdateImageWave()
		{
			foreach (var texturePixelDepthHeight in m_texturePixelDepthHeights)
			{
				int yCoordinateNear = texturePixelDepthHeight.Key;
				int yCoordinateFar = c_textureHeight - yCoordinateNear;
				int depthPrevious = texturePixelDepthHeight.Value;
				int depthCurrent = Mathf.RoundToInt(
					CalculateGreatestWaveForCoordinateValue(
						yCoordinateNear
					)
				);

				for (int depthNear = c_waveStart; depthNear < depthCurrent; depthNear++)
				{
					m_imageWave.SetPixel(
						depthNear,
						yCoordinateNear,
						Colors.White
					);

					int depthFar = c_textureWidth - depthNear;
					m_imageWave.SetPixel(
						depthFar,
						yCoordinateFar,
						Colors.White
					);
				}
				for (int depthNear = depthCurrent; depthNear < depthPrevious; depthNear++)
				{
					m_imageWave.SetPixel(
						depthNear,
						yCoordinateNear,
						Colors.Transparent
					);

					int depthFar = c_textureWidth - depthNear;
					m_imageWave.SetPixel(
						depthFar,
						yCoordinateFar,
						Colors.Transparent
					);
				}

				m_texturePixelDepthHeights[yCoordinateNear] = depthCurrent;
			}
			foreach (var texturePixelDepthWidth in m_texturePixelDepthWidths)
			{
				int xCoordinateNear = texturePixelDepthWidth.Key;
				int xCoordinateFar = c_textureWidth - xCoordinateNear;
				int depthPrevious = texturePixelDepthWidth.Value;
				int depthCurrent = Mathf.RoundToInt(
					CalculateGreatestWaveForCoordinateValue(
						xCoordinateNear
					)
				);

				for (int depthNear = c_waveStart; depthNear < depthCurrent; depthNear++)
				{
					m_imageWave.SetPixel(
						xCoordinateNear,
						depthNear,
						Colors.White
					);

					int depthFar = c_textureHeight - depthNear;
					m_imageWave.SetPixel(
						xCoordinateFar,
						depthFar,
						Colors.White
					);
				}
				for (int depthNear = depthCurrent; depthNear < depthPrevious; depthNear++)
				{
					m_imageWave.SetPixel(
						xCoordinateNear,
						depthNear,
						Colors.Transparent
					);

					int depthFar = c_textureHeight - depthNear;
					m_imageWave.SetPixel(
						xCoordinateFar,
						depthFar,
						Colors.Transparent
					);
				}

				m_texturePixelDepthWidths[xCoordinateNear] = depthCurrent;
			}
		}

		private void UpdateShaderResources()
		{
			m_textureWave.Update(
				m_imageWave
			);
			m_material.SetShaderParameter(
				"color",
				m_pastelInterpolator.GetColor()
			);
		}
	}
}