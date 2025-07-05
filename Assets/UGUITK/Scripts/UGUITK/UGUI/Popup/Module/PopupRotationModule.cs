using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.Popup.Modules
{

    [Serializable]
    public class PopupRotationModule : PopupAnimationModule
    {
        [Header("Animation Settings")]
        public Vector3 From = new Vector3(0, 0, 15f);
        public Vector3 To = Vector3.zero;
        public float Duration = 0.4f;
        public Ease Ease = Ease.OutBack;

        public override UniTask AnimateAsync(UIPopupAnimationContext ctx)
        {
            var target = ctx.PopupRectTransform;
            if (target == null) return UniTask.CompletedTask;
            
            var fromValue = UseCurrentAsFrom ? target.localRotation : Quaternion.Euler(From);
            
            return LMotion.Create(fromValue, Quaternion.Euler(To), Duration)
                .WithEase(Ease)
                .BindToLocalRotation(target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}