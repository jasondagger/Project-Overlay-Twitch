
namespace Overlay
{
    using Godot;
    using System;
    using System.Collections.Generic;

    public sealed partial class PastelInterpolator : Node
    {
        public enum ColorType : uint
        {
            Red = 0u,
            Orange,
            Yellow,
            Lime,
            Green,
            Turquoise,
            Cyan,
            Teal,
            Blue,
            Purple,
            Magenta,
            Pink,
            White,
            Rainbow
        }

        public enum RainbowColorIndexType : uint
        {
            Color0 = 0u,
            Color1,
            Color2,
            Color3,
            Color4,
            Color5,
        }

        public override void _Process(
            double delta
        )
        {
            UpdateColor(
                delta: (float)delta
            );
        }

        public Color GetColor(
            RainbowColorIndexType rainbowColorIndexType
        )
        {
            return m_rainbowColorDatas[key: rainbowColorIndexType].Current;
        }

        public string GetColorAsHex(
            RainbowColorIndexType rainbowColorIndexType
        )
        {
            return GetColor(
                rainbowColorIndexType: rainbowColorIndexType    
            ).ToHtml();
        }

        public static string GetColorAsHexByColorType(
            ColorType colorType
        )
        {
            return c_colorHexes[colorType];
        }

        public static string GetRainbowColorTag()
        {
            return c_rainbowColorTag;
        }

        public static string GetSpectrumColorTag()
        {
            return c_rainbowColorTag;
        }

        public static bool IsColorHexTheRainbowColorType(
            string hexCode    
        )
        {
            return c_colorHexes[ColorType.Rainbow].Contains(
                value: hexCode    
            );
        }

        public static string RainbowifyText(
            string text
        )
        {
            var rainbowifiedText = string.Empty;

            var colorIterator = 0;
            foreach (var character in text)
            {
                rainbowifiedText += $"{(character.Equals(obj: ' ') ? character : $"[color={c_colorHexes[key: c_rainbowColorTypes[index: colorIterator++]]}]{character}")}";
                if (colorIterator == c_rainbowColorTypes.Count)
                {
                    colorIterator = 0;
                }
            }

            return rainbowifiedText;
        }

        private enum ColorInterpolationType : uint
        {
            RedToYellow = 0u,
            YellowToGreen,
            GreenToCyan,
            CyanToBlue,
            BlueToMagenta,
            MagentaToRed
        }

        private sealed class RainbowColorIndexData
        {
            public Color Current { get; set; } = c_colorCodes[ColorType.Rainbow];
            public Color Previous { get; set; } = c_colorCodes[ColorType.Rainbow];
            public float Interpolation { get; set; } = 0f;
            public ColorInterpolationType InterpolationType { get; set; } = ColorInterpolationType.RedToYellow;

            public RainbowColorIndexData(
                Color current,
                Color previous,
                float interpolation,
                ColorInterpolationType interpolationType
            )
            {
                this.Current = current;
                this.Previous = previous;
                this.Interpolation = interpolation;
                this.InterpolationType = interpolationType;
            }
        }

        private static readonly Dictionary<ColorType, Color> c_colorCodes = new()
        {
            { ColorType.Red,       new(0xF898A4FF) },
            { ColorType.Orange,    new(0xFBCCADFF) },
            { ColorType.Yellow,    new(0xFDFFB6FF) },
            { ColorType.Lime,      new(0xE4FFBBFF) },
            { ColorType.Green,     new(0xCAFFBFFF) },
            { ColorType.Turquoise, new(0xB3FBDEFF) },
            { ColorType.Cyan,      new(0x9BF6FFFF) },
            { ColorType.Teal,      new(0x9EDCFFFF) },
            { ColorType.Blue,      new(0xA0C4FFFF) },
            { ColorType.Purple,    new(0xD0C5FFFF) },
            { ColorType.Magenta,   new(0xFFC6FFFF) },
            { ColorType.Pink,      new(0xFCAFDCFF) },
            { ColorType.White,     new(0xF2F2F2FF) },
            { ColorType.Rainbow,   new(0x000000FF) },
        };
        private static readonly Dictionary<ColorType, string> c_colorHexes = new()
        {
            { ColorType.Red,       c_colorCodes[ ColorType.Red       ].ToHtml() },
            { ColorType.Orange,    c_colorCodes[ ColorType.Orange    ].ToHtml() },
            { ColorType.Yellow,    c_colorCodes[ ColorType.Yellow    ].ToHtml() },
            { ColorType.Lime,      c_colorCodes[ ColorType.Lime      ].ToHtml() },
            { ColorType.Green,     c_colorCodes[ ColorType.Green     ].ToHtml() },
            { ColorType.Turquoise, c_colorCodes[ ColorType.Turquoise ].ToHtml() },
            { ColorType.Cyan,      c_colorCodes[ ColorType.Cyan      ].ToHtml() },
            { ColorType.Teal,      c_colorCodes[ ColorType.Teal      ].ToHtml() },
            { ColorType.Blue,      c_colorCodes[ ColorType.Blue      ].ToHtml() },
            { ColorType.Purple,    c_colorCodes[ ColorType.Purple    ].ToHtml() },
            { ColorType.Magenta,   c_colorCodes[ ColorType.Magenta   ].ToHtml() },
            { ColorType.Pink,      c_colorCodes[ ColorType.Pink      ].ToHtml() },
            { ColorType.White,     c_colorCodes[ ColorType.White     ].ToHtml() },
            { ColorType.Rainbow,   c_colorCodes[ ColorType.Rainbow   ].ToHtml() },
        };
        private static readonly List<ColorType> c_rainbowColorTypes = new()
        {
            ColorType.Red,
            ColorType.Orange,
            ColorType.Yellow,
            ColorType.Green,
            ColorType.Cyan,
            ColorType.Blue,
            ColorType.Purple,
        };

        private const float c_colorInterpolationRate = 0.25f;
        private const string c_rainbowColorTag = "[RAINBOW]";
        private const string c_spectrumColorTag = "[SPECTRUM]";

        private readonly Dictionary<RainbowColorIndexType, RainbowColorIndexData> m_rainbowColorDatas = new()
        {
            {
                RainbowColorIndexType.Color0,
                new(
                    current: c_colorCodes[ColorType.Red],
                    previous: c_colorCodes[ColorType.Red],
                    interpolation: 0f,
                    interpolationType: ColorInterpolationType.RedToYellow
                )
            },
            {
                RainbowColorIndexType.Color1,
                new(
                    current: c_colorCodes[ColorType.Yellow],
                    previous: c_colorCodes[ColorType.Yellow],
                    interpolation: 0f,
                    interpolationType: ColorInterpolationType.YellowToGreen
                )
            },
            {
                RainbowColorIndexType.Color2,
                new(
                    current: c_colorCodes[ColorType.Green],
                    previous: c_colorCodes[ColorType.Green],
                    interpolation: 0f,
                    interpolationType: ColorInterpolationType.GreenToCyan
                )
            },
            {
                RainbowColorIndexType.Color3,
                new(
                    current: c_colorCodes[ColorType.Cyan],
                    previous: c_colorCodes[ColorType.Cyan],
                    interpolation: 0f,
                    interpolationType: ColorInterpolationType.CyanToBlue
                )
            },
            {
                RainbowColorIndexType.Color4,
                new(
                    current: c_colorCodes[ColorType.Blue],
                    previous: c_colorCodes[ColorType.Blue],
                    interpolation: 0f,
                    interpolationType: ColorInterpolationType.BlueToMagenta
                )
            },
            {
                RainbowColorIndexType.Color5,
                new(
                    current: c_colorCodes[ColorType.Magenta],
                    previous: c_colorCodes[ColorType.Magenta],
                    interpolation: 0f,
                    interpolationType: ColorInterpolationType.MagentaToRed
                )
            }
        };

        private void UpdateColor(
            float delta
        )
        {
            var rainbowColorIndexTypes = Enum.GetValues<RainbowColorIndexType>();
            foreach (var rainbowColorIndexType in rainbowColorIndexTypes)
            {
                var rainbowColorIndexData = m_rainbowColorDatas[rainbowColorIndexType];

                var currentColor = rainbowColorIndexData.Current;
                var previousColor = rainbowColorIndexData.Previous;
                var colorInterpolation = rainbowColorIndexData.Interpolation;
                var colorInterpolationType = rainbowColorIndexData.InterpolationType;

                colorInterpolation += c_colorInterpolationRate * delta;
                switch (colorInterpolationType)
                {
                    case ColorInterpolationType.RedToYellow:
                        currentColor = previousColor.Lerp(
                            to: c_colorCodes[ColorType.Yellow],
                            weight: colorInterpolation
                        );

                        if (colorInterpolation >= 1f)
                        {
                            colorInterpolation = 0f;
                            currentColor = c_colorCodes[ColorType.Yellow];
                            previousColor = c_colorCodes[ColorType.Yellow];
                            colorInterpolationType = ColorInterpolationType.YellowToGreen;
                        }
                        break;

                    case ColorInterpolationType.YellowToGreen:
                        currentColor = previousColor.Lerp(
                            to: c_colorCodes[ColorType.Green],
                            weight: colorInterpolation
                        );

                        if (colorInterpolation >= 1f)
                        {
                            colorInterpolation = 0f;
                            currentColor = c_colorCodes[ColorType.Green];
                            previousColor = c_colorCodes[ColorType.Green];
                            colorInterpolationType = ColorInterpolationType.GreenToCyan;
                        }
                        break;

                    case ColorInterpolationType.GreenToCyan:
                        currentColor = previousColor.Lerp(
                            to: c_colorCodes[ColorType.Cyan],
                            weight: colorInterpolation
                        );

                        if (colorInterpolation >= 1f)
                        {
                            colorInterpolation = 0f;
                            currentColor = c_colorCodes[ColorType.Cyan];
                            previousColor = c_colorCodes[ColorType.Cyan];
                            colorInterpolationType = ColorInterpolationType.CyanToBlue;
                        }
                        break;

                    case ColorInterpolationType.CyanToBlue:
                        currentColor = previousColor.Lerp(
                            to: c_colorCodes[ColorType.Blue],
                            weight: colorInterpolation
                        );

                        if (colorInterpolation >= 1f)
                        {
                            colorInterpolation = 0f;
                            currentColor = c_colorCodes[ColorType.Blue];
                            previousColor = c_colorCodes[ColorType.Blue];
                            colorInterpolationType = ColorInterpolationType.BlueToMagenta;
                        }
                        break;

                    case ColorInterpolationType.BlueToMagenta:
                        currentColor = previousColor.Lerp(
                            to: c_colorCodes[ColorType.Magenta],
                            weight: colorInterpolation
                        );

                        if (colorInterpolation >= 1f)
                        {
                            colorInterpolation = 0f;
                            currentColor = c_colorCodes[ColorType.Magenta];
                            previousColor = c_colorCodes[ColorType.Magenta];
                            colorInterpolationType = ColorInterpolationType.MagentaToRed;
                        }
                        break;

                    case ColorInterpolationType.MagentaToRed:
                        currentColor = previousColor.Lerp(
                            to: c_colorCodes[ColorType.Red],
                            weight: colorInterpolation
                        );

                        if (colorInterpolation >= 1f)
                        {
                            colorInterpolation = 0f;
                            currentColor = c_colorCodes[ColorType.Red];
                            previousColor = c_colorCodes[ColorType.Red];
                            colorInterpolationType = ColorInterpolationType.RedToYellow;
                        }
                        break;

                    default:
                        break;
                }

                rainbowColorIndexData.Current = currentColor;
                rainbowColorIndexData.Previous = previousColor;
                rainbowColorIndexData.Interpolation = colorInterpolation;
                rainbowColorIndexData.InterpolationType = colorInterpolationType;
            }
        }
    }
}