using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.Modules
{
    [Serializable]
    public class RotationModule : ButtonAnimationModule
    {
        public Vector3 FromEuler = Vector3.zero;
        public Vector3 ToEuler = new Vector3(0, 0, 15);
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutBack;

        public override UniTask AnimateAsync(UIButtonAnimationContext ctx)
        {
            return LMotion.Create(
                    Quaternion.Euler(FromEuler),
                    Quaternion.Euler(ToEuler),
                    Duration)
                .WithEase(Ease)
                .BindToLocalRotation(ctx.RectTransform)
                .ToUniTask();
        }
    }
}