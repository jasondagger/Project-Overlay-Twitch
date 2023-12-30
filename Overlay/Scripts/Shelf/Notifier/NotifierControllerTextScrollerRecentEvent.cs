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
            async () =>
            {
                for (int i = 0; i < m_names.Count; i++)
                {
                    FooterText = m_names.ElementAt(
                        i
                    );

                    m_createFooter = true;
                    while (m_createFooter);

                    if (i == 0u)
                    {
                        StartScrollToCenter(
                            TextLetterType.Header
                        );
                    }

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

                    if (i == m_names.Count - 1u)
                    {
                        StartScrollToEnd(
                            TextLetterType.Header
                        );
                    }

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

                    DestroyFooterTextLetters();
                    ResetFooter();
                }

                if (m_pendingNames.Count > 0u)
                {
                    Queue<string> names = new();
                    names.Enqueue(
                        m_pendingNames.Dequeue()
                    );
                    while (m_names.Count > 1u)
                    {
                        names.Enqueue(
                            m_names.Dequeue()
                        );
                    }
                    m_names = names;
                }

                CompletedNotification?.Invoke();
            }
        );
    }
}