using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace UGUIAnimationToolkit.ProgressBar.Modules
{
    [ModuleCategory("ProgressBar/Utility", Order = 100)]
    [Serializable]
    public class ProgressBarTextModule : ProgressBarAnimationModule
    {
        public enum DisplayFormat { RawValue, Percentage, Custom }

        [Header("Target")]
        [Tooltip("값을 표시할 TextMeshPro 컴포넌트를 할당하세요.")]
        [SerializeField] private TextMeshProUGUI targetText;

        [Header("Display Settings")]
        public DisplayFormat Format = DisplayFormat.Percentage;
        [Tooltip("Format이 Custom일 때 사용됩니다. {value}는 현재 값, {max}는 최대값으로 치환됩니다.")]
        public string CustomFormat = "{value} / {max}";

        public override UniTask AnimateAsync(ProgressBarAnimationContext ctx)
        {
            if (targetText == null) return UniTask.CompletedTask;
            
            var progressBar = ctx.TargetProgressBar;

            // 값의 변화를 숫자가 카운트되는 것처럼 애니메이션합니다.
            return LMotion.Create(progressBar.minValue, progressBar.value, 0.3f) // Duration은 고정하거나 옵션으로 뺄 수 있습니다.
                .Bind(v =>
                {
                    switch (Format)
                    {
                        case DisplayFormat.RawValue:
                            targetText.text = Mathf.FloorToInt(v).ToString();
                            break;
                        case DisplayFormat.Percentage:
                            var percentage = (v / progressBar.maxValue) * 100f;
                            targetText.text = $"{Mathf.FloorToInt(percentage)}%";
                            break;
                        case DisplayFormat.Custom:
                            targetText.text = CustomFormat
                                .Replace("{value}", Mathf.FloorToInt(v).ToString())
                                .Replace("{max}", Mathf.FloorToInt(progressBar.maxValue).ToString());
                            break;
                    }
                })
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}