using Godot;
using System;
using System.Collections.Generic;

public sealed partial class EffectManager : Node
{
//    public override void _EnterTree()
//    {
//        RetrieveEffects();
//    }

//    public enum Effect3DType : uint
//    {
//        Rain = 0u,
//        Snow
//    }

//    public readonly Dictionary<Effect3DType, Action> Effect3DPlayed = new()
//    {
//        { Effect3DType.Rain, null },
//        { Effect3DType.Snow, null },
//    };

//    public readonly Dictionary<Effect3DType, Action> Effect3DStopped = new()
//    {
//        { Effect3DType.Rain, null },
//        { Effect3DType.Snow, null },
//    };

//    public void PlayEffect3D(
//        Effect3DType effectType
//    )
//    {
//        var currentEffect = m_effects[m_currentEffectType];
//        if (
//            currentEffect.IsPlaying() &&
//            currentEffect.IsLooping()
//        )
//        {
//            currentEffect.Stop();
//        }

//        m_currentEffectType = effectType;
//        m_effects[m_currentEffectType].Play();
//    }

//    private readonly Dictionary<Effect3DType, Effect> m_effects = new()
//    {
//        { Effect3DType.Rain, null },
//        { Effect3DType.Snow, null },
//    };

//    private Effect3DType m_currentEffectType = Effect3DType.Rain;

//    private void RetrieveEffects()
//    {
//#if DEBUG
//        GD.Print(
//            $"{nameof(EffectManager)}.{nameof(RetrieveEffects)}() - Retrieving effects."
//        );
//#endif

//        var effectTypes = Enum.GetValues<Effect3DType>();
//        foreach (var effectType in effectTypes)
//        {
//            var effect = GetNode<Effect>(
//                $"{effectType}"
//            );

//            m_effects[effectType] = effect;

//#if DEBUG
//            GD.Print(
//                $"{nameof(EffectManager)}.{nameof(RetrieveEffects)}() - Retrieved effect {effectType}."
//            );
//#endif
//        }
//    }
}