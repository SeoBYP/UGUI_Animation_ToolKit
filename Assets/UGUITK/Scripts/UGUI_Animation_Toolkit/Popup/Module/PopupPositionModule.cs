using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.Popup.Modules
{

    [Serializable]
    public class PopupPositionModule : PopupAnimationModule
    {
        [Header("Animation Settings")]
        public Vector2 From;
        public Vector2 To;
        public float Duration = 0.3f;
        public Ease Ease = Ease.OutCubic;

        public override UniTask AnimateAsync(UIPopupAnimationContext ctx)
        {
            var target = ctx.PopupRectTransform;
            if (target == null) return UniTask.CompletedTask;
            
            var fromValue = UseCurrentAsFrom ? target.anchoredPosition : From;
            
            return LMotion.Create(fromValue, To, Duration)
                .WithEase(Ease)
                .BindToAnchoredPosition(target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}