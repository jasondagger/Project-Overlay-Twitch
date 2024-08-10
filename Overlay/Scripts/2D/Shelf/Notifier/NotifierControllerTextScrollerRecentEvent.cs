
namespace Overlay
{
    using Godot;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public abstract partial class NotifierControllerTextScrollerRecentEvent : NotifierControllerTextScrollerBase
    {
        protected const uint c_maxNameCount = 5u;

        protected Queue<string> m_names = new();
        protected Queue<string> m_pendingNames = new();

        protected override void PlayNotification()
        {
            Task.Run(
                function:
                async () =>
                {
                    for (var i = 0; i < m_names.Count; i++)
                    {
                        FooterText = m_names.ElementAt(
                            index: i
                        );

                        m_createFooter = true;
                        while (m_createFooter) ;

                        if (
                            i.Equals(
                                obj: 0
                            ) is true
                        )
                        {
                            StartScrollToCenter(
                                textLetterType: TextLetterType.Header
                            );
                        }

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

                        if (
                            i.Equals(
                                obj: m_names.Count - 1
                            ) is true
                        )
                        {
                            StartScrollToEnd(
                                textLetterType: TextLetterType.Header
                            );
                        }

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

                    if (m_pendingNames.Count > 0u)
                    {
                        var names = new Queue<string>();
                        names.Enqueue(
                            item: m_pendingNames.Dequeue()
                        );
                        while (m_names.Count > 1u)
                        {
                            names.Enqueue(
                                item: m_names.Dequeue()
                            );
                        }
                        m_names = names;
                    }

                    CompletedNotification?.Invoke();
                }
            );
        }
    }
}