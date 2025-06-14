using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UGUI_Animation_Toolkit.Button.Extensions
{
    using Button = UnityEngine.UI.Button;

    public static class ButtonExtensions
    {
        public static void BindDebounce(this Button button, Func<UniTask> action, float delay = 0.5f)
        {
            bool isWaiting = false;
            button.onClick.AddListener(async () =>
            {
                if (isWaiting) return;
                isWaiting = true;
                await action();
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
                isWaiting = false;
            });
        }

        public static void BindThrottle(this Button button, Func<UniTask> action, float interval = 0.5f)
        {
            float lastTime = -999f;
            button.onClick.AddListener(async () =>
            {
                if (Time.unscaledTime - lastTime < interval) return;
                lastTime = Time.unscaledTime;
                await action();
            });
        }
    }
}