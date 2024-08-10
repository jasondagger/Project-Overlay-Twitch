
namespace Overlay
{
    using ColorType = PastelInterpolator.ColorType;

    public static class TwitchChatColorCodes
    {
        public static readonly string Error = PastelInterpolator.GetColorAsHexByColorType(
            colorType: ColorType.Red
        );
        public static readonly string Link = PastelInterpolator.GetColorAsHexByColorType(
            colorType: ColorType.Cyan
        );
        public static readonly string Normal = PastelInterpolator.GetColorAsHexByColorType(
            colorType: ColorType.White
        );
        public static readonly string SubError = PastelInterpolator.GetColorAsHexByColorType(
            colorType: ColorType.Pink
        );
        public static readonly string Success = PastelInterpolator.GetColorAsHexByColorType(
            colorType: ColorType.Green
        );
        public static readonly string User = PastelInterpolator.GetColorAsHexByColorType(
            colorType: ColorType.Cyan
        );

        public static string ConvertToErrorMessage(
            string message    
        )
        {
            return $"[color={Error}]{message}[/color]";
        }

        public static string ConvertToLinkMessage(
            string message
        )
        {
            return $"[color={Link}]{message}[/color]";
        }

        public static string ConvertToNormalMessage(
            string message    
        )
        {
            return $"[color={Normal}]{message}[/color]";
        }

        public static string ConvertToSubErrorMessage(
            string message    
        )
        {
            return $"[color={SubError}]{message}[/color]";
        }

        public static string ConvertToSuccessMessage(
            string message    
        )
        {
            return $"[color={Success}]{message}[/color]";
        }

        public static string ConvertToUserMessage(
            string message
        )
        {
            return $"[color={User}]{message}[/color]";
        }
    }
}