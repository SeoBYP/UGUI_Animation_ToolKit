using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAnimationToolkit.Toggle.Modules
{
    [ModuleCategory("Toggle/Graphic", Order = 10)]
    [Serializable]
    public class ToggleGraphicAlphaModule : ToggleAnimationModule
    {
        public enum TargetGraphicType
        {
            Background,
            Checkmark
        }

        [Header("Target")] 
        public TargetGraphicType Target = TargetGraphicType.Background;

        [Header("Animation Settings")] 
        [Range(0f, 1f)] public float From = 0f;
        [Range(0f, 1f)] public float To = 1f;
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutSine;

        public override UniTask AnimateAsync(ToggleAnimationContext ctx)
        {
            Graphic graphic = (Target == TargetGraphicType.Background) ? ctx.BackgroundGraphic : ctx.CheckmarkGraphic;
            if (graphic == null) return UniTask.CompletedTask;

            var fromValue = UseCurrentAsFrom ? graphic.color.a : From;

            return LMotion.Create(fromValue, To, Duration)
                .WithEase(Ease)
                .BindToColorA(graphic)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}