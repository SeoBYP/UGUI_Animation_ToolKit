using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.Popup.Modules
{
    // [ModuleCategory("Popup/Transform")] // 필요하다면 카테고리 어트리뷰트 추가
    [Serializable]
    public class PopupScaleModule : PopupAnimationModule
    {
        [Header("Animation Settings")]
        public Vector3 From = Vector3.one * 0.8f;
        public Vector3 To = Vector3.one;
        public float Duration = 0.3f;
        public Ease Ease = Ease.OutBack;

        public override UniTask AnimateAsync(UIPopupAnimationContext ctx)
        {
            var fromValue = UseCurrentAsFrom ? ctx.PopupRectTransform.localScale : From;
            return LMotion.Create(fromValue, To, Duration)
                .WithEase(Ease)
                .BindToLocalScale(ctx.PopupRectTransform)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}