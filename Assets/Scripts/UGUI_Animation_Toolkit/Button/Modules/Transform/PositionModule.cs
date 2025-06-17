using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.Modules
{
// 위치 애니메이션 모듈
    [Serializable]
    [ModuleCategory("Transform",Order = 3)] 
    public class PositionModule : ButtonAnimationModule
    {
        [Header("Animation Settings")] public RectTransform Target;
        public Vector2 From = Vector2.zero;
        public Vector2 To = Vector2.up * 5f;
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutBounce;

        public override UniTask AnimateAsync(UIButtonAnimationContext ctx)
        {
            return LMotion.Create(From, To, Duration)
                .WithEase(Ease)
                .BindToAnchoredPosition(Target)
                .ToUniTask();
        }

        public override UniTask RevertAsync(UIButtonAnimationContext ctx)
        {
            return LMotion.Create(To, From, Duration)
                .WithEase(Ease)
                .BindToAnchoredPosition(Target)
                .ToUniTask();
        }
    }
}