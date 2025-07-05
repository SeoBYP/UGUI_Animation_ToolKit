using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.Toggle.Modules
{
    [ModuleCategory("Toggle/Transform", Order = 21)]
    [Serializable]
    public class ToggleSlideModule : ToggleAnimationModule
    {
        [Header("Target")]
        [SerializeField] private RectTransform targetTransform;
        
        [Header("Animation Settings")]
        public Vector2 FromOffset;
        public Vector2 ToOffset;
        public float Duration = 0.1f;
        public Ease Ease = Ease.OutSine;

        public override UniTask AnimateAsync(ToggleAnimationContext ctx)
        {
            if (targetTransform == null) return UniTask.CompletedTask;

            var fromValue = UseCurrentAsFrom ? targetTransform.anchoredPosition : FromOffset;
            
            return LMotion.Create(fromValue, ToOffset, Duration)
                .WithEase(Ease)
                .BindToAnchoredPosition(targetTransform)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}