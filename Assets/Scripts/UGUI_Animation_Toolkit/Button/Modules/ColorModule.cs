using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.Modules
{
    [Serializable]
    public class ColorModule : ButtonAnimationModule
    {
        public Color From = Color.white;
        public Color To = Color.gray;
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutSine;

        public override UniTask AnimateAsync(UIButtonAnimationContext ctx)
        {
            var cg = ctx.RectTransform.GetComponent<CanvasGroup>();
            if (cg == null) return UniTask.CompletedTask;
            return LMotion.Create(From, To, Duration)
                .WithEase(Ease)
                .Bind(cg, (alpha, group) => group.alpha = alpha.a)
                .ToUniTask();
        }
    }

}