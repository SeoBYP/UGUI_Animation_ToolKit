using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.Toggle.Modules
{
    [ModuleCategory("Toggle/Transform", Order = 20)]
    [Serializable]
    public class ToggleScaleModule : ToggleAnimationModule
    {
        [Header("Target")]
        [SerializeField] private RectTransform targetTransform;
        
        [Header("Animation Settings")]
        public Vector3 From = Vector3.zero;
        public Vector3 To = Vector3.one;
        public float Duration = 0.1f;
        public Ease Ease = Ease.OutSine;

        public override UniTask AnimateAsync(ToggleAnimationContext ctx)
        {
            if (targetTransform == null) return UniTask.CompletedTask;

            var fromValue = UseCurrentAsFrom ? targetTransform.localScale : From;

            return LMotion.Create(fromValue, To, Duration)
                .WithEase(Ease)
                .BindToLocalScale(targetTransform)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}