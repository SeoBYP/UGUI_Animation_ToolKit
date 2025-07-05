using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UGUIAnimationToolkit.Core;
using UnityEngine;

namespace UGUIAnimationToolkit.Modules
{
    [ModuleCategory("TextMeshProUGUI",Order = 2)] 
    public class TextNumberCountModule : UIAnimationModule
    {
        public enum DisplayFormat
        {
            RawValue,
            Percentage,
            Custom
        }

        [Header("Target")] [Tooltip("값을 표시할 TextMeshPro 컴포넌트를 할당하세요.")] [SerializeField]
        private TextMeshProUGUI targetText;

        public float From = 0;
        public float To = 100;
        public float Duration = 0.25f;
        public Ease Ease = Ease.OutSine;

        [Header("Display Settings")] public DisplayFormat Format = DisplayFormat.Percentage;

        [Tooltip("Format이 Custom일 때 사용됩니다. {value}는 현재 값, {max}는 최대값으로 치환됩니다.")]
        public string CustomFormat = "{value} / {max}";


        public override UniTask AnimateAsync(UIAnimationContext ctx)
        {
            // 값의 변화를 숫자가 카운트되는 것처럼 애니메이션합니다.
            return LMotion.Create(From, To, Duration) // Duration은 고정하거나 옵션으로 뺄 수 있습니다.
                .WithEase(Ease)
                .Bind(v =>
                {
                    switch (Format)
                    {
                        case DisplayFormat.RawValue:
                            targetText.text = Mathf.FloorToInt(v).ToString();
                            break;
                        case DisplayFormat.Percentage:
                            var percentage = (v / To) * 100f;
                            targetText.text = $"{Mathf.FloorToInt(percentage)}%";
                            break;
                        case DisplayFormat.Custom:
                            targetText.text = CustomFormat
                                .Replace("{value}", Mathf.FloorToInt(v).ToString())
                                .Replace("{max}", Mathf.FloorToInt(To).ToString());
                            break;
                    }
                })
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(UIAnimationContext ctx)
        {
            return UniTask.CompletedTask;
        }
    }
}