
namespace Overlay
{
    using Godot;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract partial class NotifierControllerTextScrollerEvent : NotifierControllerTextScrollerBase
    {
        protected Queue<string> m_pendingNames = new();

        protected override void PlayNotification()
        {
            Task.Run(
                function:
                async () =>
                {
                    while (m_pendingNames.Count > 0u)
                    {
                        FooterText = m_pendingNames.Dequeue();

                        m_createFooter = true;
                        while (m_createFooter) ;

                        StartScrollToCenter(
                            textLetterType: TextLetterType.Header
                        );

                        await Task.Delay(
                            millisecondsDelay: c_richTextLabelTitleDelayInMillisecondsFooter
                        );

                        StartScrollToCenter(
                            textLetterType: TextLetterType.Footer
                        );
                        SetImageIconState(
                            imageIconAnimationState: ImageIconAnimationState.Showing
                        );

                        if (FooterDistanceOffScreen > 0f)
                        {
                            var animationDelay = Mathf.RoundToInt(
                                s: FooterDistanceOffScreen / c_velocityScrollControlInMilliseconds
                            );
                            var remainingScreenDuration = c_richTextLabelOnScreenDuration - animationDelay;
                            var halfDelay = Mathf.RoundToInt(
                                s: remainingScreenDuration / 2f
                            );

                            await Task.Delay(
                                millisecondsDelay: halfDelay
                            );

                            m_moveFooter = true;

                            await Task.Delay(
                                millisecondsDelay: halfDelay
                            );
                        }
                        else
                        {
                            await Task.Delay(
                                millisecondsDelay: c_richTextLabelOnScreenDuration
                            );
                        }

                        StartScrollToEnd(
                            textLetterType: TextLetterType.Header
                        );

                        await Task.Delay(
                            millisecondsDelay: c_richTextLabelTitleDelayInMillisecondsFooter
                        );

                        StartScrollToEnd(
                            textLetterType: TextLetterType.Footer
                        );
                        SetImageIconState(
                            imageIconAnimationState: ImageIconAnimationState.Hiding
                        );

                        await Task.Delay(
                            millisecondsDelay: c_richTextLabelTitleDelayInMillisecondsCompletion
                        );

                        DestroyFooterTextLetters();
                        ResetFooter();
                    }

                    CompletedNotification?.Invoke();
                }
            );
        }
    }
}