using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UGUIAnimationToolkit
{
    [Serializable]
    public class AnimationSequence
    {
        public bool AutoRevert;
        public float RevertDelay;

        [SerializeReference]
        public List<ButtonAnimationModule> modules = new List<ButtonAnimationModule>();

        public async UniTask PlayAsync(UIButtonAnimationContext ctx)
        {
            // 실제 플레이 로직 (예시)
            foreach (var module in modules)
            {
                if (module != null && module.Enable)
                {
                    if (module.Delay > 0)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(module.Delay), ignoreTimeScale: true);
                    }
                    await module.AnimateAsync(ctx);
                }
            }
        }
    }
}