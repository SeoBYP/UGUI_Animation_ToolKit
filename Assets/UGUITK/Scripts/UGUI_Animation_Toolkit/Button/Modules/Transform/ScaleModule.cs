using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.Modules
{
    [Serializable]
    [ModuleCategory("Transform",Order = 3)] 
    public class ScaleModule : ButtonAnimationModule
    {
        [Header("Animation Settings")] public RectTransform Target;
        public Vector3 From = Vector3.zero;
        public Vector3 To = Vector3.one;
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutQuad;

        public override UniTask AnimateAsync(UIButtonAnimationContext ctx)
        {
            return LMotion.Create(From, To, Duration)
                .WithEase(Ease)
                .BindToLocalScale(Target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(UIButtonAnimationContext ctx)
        {
            return LMotion.Create(To, From, Duration)
                .WithEase(Ease)
                .BindToLocalScale(Target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}