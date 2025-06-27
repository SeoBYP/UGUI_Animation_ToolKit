using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UGUIAnimationToolkit.Text
{
    [Serializable]
    public class TextAnimationSequence
    {
        [SerializeReference] public List<TextAnimationModule> modules = new();
        
        public async UniTask PlayAsync(TextAnimationContext ctx)
        {
            ctx.MotionHandle.Complete();
            var tasks = new List<UniTask>();

            foreach (var module in modules)
            {
                if (module == null || !module.Enable) continue;
                
                async UniTask ExecuteModuleAsync()
                {
                    if (module.Delay > 0)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(module.Delay), ignoreTimeScale: true);
                    }
                    await module.AnimateAsync(ctx);
                }
                tasks.Add(ExecuteModuleAsync());
            }
            await UniTask.WhenAll(tasks);
        }
    }
}