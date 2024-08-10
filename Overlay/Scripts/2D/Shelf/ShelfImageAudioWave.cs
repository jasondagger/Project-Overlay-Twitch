
namespace Overlay
{
	using Godot;
	using System.Collections.Generic;
	using static Godot.Image;
	using NodeType = NodeDirectory.NodeType;
	using RainbowColorIndexType = PastelInterpolator.RainbowColorIndexType;

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

        private const int c_textureDepthWidth = c_textureWidth - c_borderWaveStart - 1;
        private const int c_textureDepthHeight = c_textureHeight - c_borderWaveStart - 1;

        private const int c_waveStart = c_borderWaveStart + c_borderWaveDepth;
		private const int c_waveEndHeight = c_textureHeight - c_waveStart;
		private const int c_waveEndWidth = c_textureWidth - c_waveStart;

		private const int c_waveDepthData = 48;
		private const int c_waveDataSampleCount = 64;
		private const int c_waveDataCount = c_waveDataSampleCount * 2;

        private readonly Dictionary<int, int> m_texturePixelDepthHeights = new();
        private readonly Dictionary<int, int> m_texturePixelDepthWidths = new();

        private PastelInterpolator m_pastelInterpolator = null;
		private Image m_imageMask = null;
		private Image m_imageWave = null;
		private ImageTexture m_textureMask = new();
		private ImageTexture m_textureWave = new();
		private ShaderMaterial m_material = null;

		private float[] m_waveData = new float[c_waveDataCount];

		private float CalculateGreatestWaveForCoordinateValue(
			int coordinateValue
		)
		{
			return m_waveData[coordinateValue % c_waveDataCount] * c_waveDepthData + c_waveStart;
		}

		private void CalculateSoundtrackSoundWaveData()
		{
			for (var i = 0; i < c_waveDataSampleCount; i++)
			{
				var leftWaveIndex = i;
				m_waveData[leftWaveIndex] = 0f;

				var rightWaveIndex = c_waveDataCount - 1 - i;
				m_waveData[rightWaveIndex] = 0f;
			}
		}

		private void CreateImageTextures()
		{
			m_imageMask = Create(
				width: c_textureWidth,
				height: c_textureHeight,
				useMipmaps: false,
				format: Format.Rgbaf
			);
			m_imageWave = Create(
                width: c_textureWidth,
                height: c_textureHeight,
                useMipmaps: false,
                format: Format.Rgbaf
            );
			m_textureMask.SetImage(
				image: m_imageMask
			);
			m_textureWave.SetImage(
				image: m_imageWave
			);
		}

		private void CreateImageMask()
		{
			for (var x = 0; x < c_textureWidth; x++)
			{
				for (var y = 0; y < c_textureHeight; y++)
				{
					var color = 
						x < c_borderWaveStart ||
                        x >= c_textureDepthWidth ||
                        y < c_borderWaveStart ||
                        y >= c_textureDepthHeight ?
                            Colors.Transparent : Colors.White;
					m_imageMask.SetPixel(
						x: x,
						y: y,
						color: color
                    );
				}
			}

			m_textureMask.Update(
				image: m_imageMask
			);
		}

		private void CreateImageWaveBorder()
		{
			for (var i = 0; i < c_borderWaveDepth; i++)
			{
				for (var xCoordinateNear = c_borderWaveStart; xCoordinateNear < c_borderWaveEndWidth; xCoordinateNear++)
				{
					var depthNear = i + c_borderWaveStart;
					m_imageWave.SetPixel(
						x: xCoordinateNear,
						y: depthNear,
						color: Colors.White
					);

					var xCoordinateFar = c_textureWidth - xCoordinateNear;
					var depthFar = c_textureHeight - depthNear;
					m_imageWave.SetPixel(
						x: xCoordinateFar,
						y: depthFar,
						color: Colors.White
					);
				}
				for (var yCoordinateNear = c_borderWaveStart; yCoordinateNear < c_borderWaveEndHeight; yCoordinateNear++)
				{
					var depthNear = i + c_borderWaveStart;
					m_imageWave.SetPixel(
						x: depthNear,
						y: yCoordinateNear,
						color: Colors.White
					);

                    var yCoordinateFar = c_textureHeight - yCoordinateNear;
                    var depthFar = c_textureWidth - depthNear;
					m_imageWave.SetPixel(
						x: depthFar,
						y: yCoordinateFar,
						color: Colors.White
					);
				}
			}
		}

		private void RetrieveResources()
		{
			m_pastelInterpolator = GetNode<PastelInterpolator>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.PastelInterpolator
                )
            );
		}

		private void SetShaderMaterial()
		{
			m_material = (ShaderMaterial)Get(
				property: "material"
			);
			m_material.SetShaderParameter(
				param: "textureMask",
				value: m_textureMask
			);
			m_material.SetShaderParameter(
				param: "textureWave",
				value: m_textureWave
			);
			m_material.SetShaderParameter(
				param: "color",
				value: m_pastelInterpolator.GetColor(
                    rainbowColorIndexType: RainbowColorIndexType.Color0	
				)
			);
		}

		private void SetStartingDepths()
		{
			for (var i = c_waveStart; i < c_waveEndHeight; i++)
			{
				m_texturePixelDepthHeights.Add(
					key: i,
					value: c_waveStart
				);
			}
			for (var i = c_waveStart; i < c_waveEndWidth; i++)
			{
				m_texturePixelDepthWidths.Add(
					key: i,
					value: c_waveStart
				);
			}
		}

		private void UpdateImageWave()
		{
			foreach (var texturePixelDepthHeight in m_texturePixelDepthHeights)
			{
				var yCoordinateNear = texturePixelDepthHeight.Key;
				var yCoordinateFar = c_textureHeight - yCoordinateNear;
				var depthPrevious = texturePixelDepthHeight.Value;
				var depthCurrent = Mathf.RoundToInt(
					s: CalculateGreatestWaveForCoordinateValue(
						coordinateValue: yCoordinateNear
					)
				);

				for (var depthNear = c_waveStart; depthNear < depthCurrent; depthNear++)
				{
					m_imageWave.SetPixel(
						x: depthNear,
						y: yCoordinateNear,
						color: Colors.White
					);

					var depthFar = c_textureWidth - depthNear;
					m_imageWave.SetPixel(
						x: depthFar,
						y: yCoordinateFar,
						color: Colors.White
					);
				}
				for (var depthNear = depthCurrent; depthNear < depthPrevious; depthNear++)
				{
					m_imageWave.SetPixel(
                        x: depthNear,
						y: yCoordinateNear,
						color: Colors.Transparent
					);

                    var depthFar = c_textureWidth - depthNear;
					m_imageWave.SetPixel(
                        x: depthFar,
						y: yCoordinateFar,
						color: Colors.Transparent
					);
				}

				m_texturePixelDepthHeights[yCoordinateNear] = depthCurrent;
			}
			foreach (var texturePixelDepthWidth in m_texturePixelDepthWidths)
			{
				var xCoordinateNear = texturePixelDepthWidth.Key;
				var xCoordinateFar = c_textureWidth - xCoordinateNear;
				var depthPrevious = texturePixelDepthWidth.Value;
				var depthCurrent = Mathf.RoundToInt(
					s: CalculateGreatestWaveForCoordinateValue(
						coordinateValue: xCoordinateNear
					)
				);

				for (var depthNear = c_waveStart; depthNear < depthCurrent; depthNear++)
				{
					m_imageWave.SetPixel(
						x: xCoordinateNear,
						y: depthNear,
						color: Colors.White
					);

					var depthFar = c_textureHeight - depthNear;
					m_imageWave.SetPixel(
                        x: xCoordinateFar,
						y: depthFar,
						color: Colors.White
					);
				}
				for (var depthNear = depthCurrent; depthNear < depthPrevious; depthNear++)
				{
					m_imageWave.SetPixel(
                        x: xCoordinateNear,
						y: depthNear,
						color: Colors.Transparent
					);

					var depthFar = c_textureHeight - depthNear;
					m_imageWave.SetPixel(
                        x: xCoordinateFar,
						y: depthFar,
						color: Colors.Transparent
					);
				}

				m_texturePixelDepthWidths[xCoordinateNear] = depthCurrent;
			}
		}

		private void UpdateShaderResources()
		{
			m_textureWave.Update(
				image: m_imageWave
			);
			m_material.SetShaderParameter(
				param: "color",
				value: m_pastelInterpolator.GetColor(
					rainbowColorIndexType: RainbowColorIndexType.Color0	
				)
			);
		}
	}
}