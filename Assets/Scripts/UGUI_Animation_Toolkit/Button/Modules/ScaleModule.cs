using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.Modules
{
    [Serializable]
    public class ScaleModule : ButtonAnimationModule
    {
        public Vector3 From = Vector3.one;
        public Vector3 To = Vector3.one * 0.9f;
        public float Duration = 0.07f;
        public Ease Ease = Ease.OutQuad;

        public override UniTask AnimateAsync(UIButtonAnimationContext ctx)
        {
            return LMotion.Create(From, To, Duration)
                .WithEase(Ease)
                .BindToLocalScale(ctx.RectTransform)
                .ToUniTask();
        }
    }
}