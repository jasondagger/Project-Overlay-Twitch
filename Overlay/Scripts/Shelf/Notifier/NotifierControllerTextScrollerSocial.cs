using Godot;
using System.Threading.Tasks;

public abstract partial class NotifierControllerTextScrollerSocial : NotifierControllerTextScrollerBase
{
    protected override void PlayNotification()
    {
        Task.Run(
            async () =>
            {
                StartScrollToCenter(
                    TextLetterType.Header
                );

                await Task.Delay(
                    c_richTextLabelTitleDelayInMillisecondsFooter
                );

                StartScrollToCenter(
                    TextLetterType.Footer
                );
                SetImageIconState(
                    ImageIconAnimationState.Showing
                );

                if (FooterDistanceOffScreen > 0f)
                {
                    int animationDelay = Mathf.RoundToInt(
                        FooterDistanceOffScreen / c_velocityScrollControlInMilliseconds
                    );
                    int remainingScreenDuration = c_richTextLabelOnScreenDuration - animationDelay;
                    int halfDelay = Mathf.RoundToInt(
                        remainingScreenDuration / 2f
                    );

                    await Task.Delay(
                        halfDelay
                    );

                    m_moveFooter = true;

                    await Task.Delay(
                        halfDelay
                    );
                }
                else
                {
                    await Task.Delay(
                        c_richTextLabelOnScreenDuration
                    );
                }

                StartScrollToEnd(
                    TextLetterType.Header
                );

                await Task.Delay(
                    c_richTextLabelTitleDelayInMillisecondsFooter
                );

                StartScrollToEnd(
                    TextLetterType.Footer
                );
                SetImageIconState(
                    ImageIconAnimationState.Hiding
                );

                await Task.Delay(
                    c_richTextLabelTitleDelayInMillisecondsCompletion
                );

                ResetFooter();

                CompletedNotification?.Invoke();
            }
        );
    }
}