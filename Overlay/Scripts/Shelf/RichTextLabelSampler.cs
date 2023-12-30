using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public sealed partial class RichTextLabelSampler : Control
{
    public enum RichTextLabelType : uint
    {
        NameplateName = 0u,
        NameplateTitle,
        NotifierFooter,
        NotifierHeader
    }

    public void LoadRichTextLabelsAndAttachToParentNode(
        RichTextLabelType richTextLabelType,
        Node parent
    )
    {
        string richTextLabelTypeFolder = Enum.GetName(
            richTextLabelType
        );
        string[] paths = new[]{
            $"Resources\\Letters\\{richTextLabelTypeFolder}\\LowerCase",
            $"Resources\\Letters\\{richTextLabelTypeFolder}\\Numeric",
            $"Resources\\Letters\\{richTextLabelTypeFolder}\\Special",
            $"Resources\\Letters\\{richTextLabelTypeFolder}\\UpperCase"
        };

        foreach (var path in paths)
        {
            LoadRichTextLabelsAtPathAndAttachToParent(
                path,
                parent
            );
        }
    }

    public RichTextLabel DequeueRichTextLabel(
        char letter
    )
    {
        RichTextLabel textLetter = m_richTextLabelCache[letter].Dequeue();
        m_richTextLabelsInUse[letter].Enqueue(
            textLetter
        );

        return textLetter;
    }

    public void RequeueRichTextLabel(
        char letter
    )
    {
        RichTextLabel textLetter = m_richTextLabelsInUse[letter].Dequeue();
        m_richTextLabelCache[letter].Enqueue(
            textLetter
        );
    }

    private readonly Dictionary<string, char> c_specialNames = new(){
        { "ForwardSlash", '/' },
        { "Period",       '.' },
        { "Space",        ' ' }
    };
    private const uint c_maxNameLength = 24u;

    private Dictionary<char, Queue<RichTextLabel>> m_richTextLabelCache = new();
    private Dictionary<char, Queue<RichTextLabel>> m_richTextLabelsInUse = new();

    private void LoadRichTextLabelsAtPathAndAttachToParent(
        string path,
        Node parent
    )
    {
        string[] files = Directory.GetFiles(
            path
        );
        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(
                file
            );
            char index = c_specialNames.ContainsKey(
                fileName
            ) ? c_specialNames[fileName] : fileName[0];

            m_richTextLabelCache.Add(
                index,
                new()
            );
            m_richTextLabelsInUse.Add(
                index,
                new()
            );

            var sceneObject = GD.Load<PackedScene>(
                file
            );
            for (uint i = 0u; i < c_maxNameLength; i++)
            {
                var richTextLabel = sceneObject.Instantiate<RichTextLabel>();
                richTextLabel.Visible = false;

                parent.AddChild(
                    richTextLabel
                );
                richTextLabel.SetPosition(
                    Vector2.Zero
                );

                m_richTextLabelCache[index].Enqueue(
                    richTextLabel
                );
            }
        }
    }
}