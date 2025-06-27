using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.Popup.Modules
{
    // [ModuleCategory("Popup/Graphic")]
    [Serializable]
    public class PopupFadeModule : PopupAnimationModule
    {
        [Header("Animation Settings")]
        [Range(0f, 1f)] public float From = 0f;
        [Range(0f, 1f)] public float To = 1f;
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutCubic;

        public override UniTask AnimateAsync(UIPopupAnimationContext ctx)
        {
            var fromValue = UseCurrentAsFrom ? ctx.PopupCanvasGroup.alpha : From;
            return LMotion.Create(fromValue, To, Duration)
                .WithEase(Ease)
                .BindToAlpha(ctx.PopupCanvasGroup)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}