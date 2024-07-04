
namespace Overlay
{
    public sealed partial class NotifierControllerTextScrollerSocialTwitch : NotifierControllerTextScrollerSocial
    {
        protected override string HeaderText { get; set; } = "Watch Live on Twitch!";
        protected override string FooterText { get; set; } = "twitch.tv/SmoothDagger";
    }
}