using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UGUIAnimationToolkit
{
    [Serializable]
    public class ButtonAnimationSequence
    {
        [SerializeReference] public List<ButtonAnimationModule> modules = new List<ButtonAnimationModule>();

        public async UniTask PlayAsync(UIButtonAnimationContext ctx)
        {
            ctx.MotionHandle.Complete();

            var forwardTasks = new List<UniTask>();
            var executedModules = new List<ButtonAnimationModule>(); // [추가] 실제로 실행된 모듈만 기록

            foreach (var module in modules)
            {
                if (module == null || !module.Enable) continue;

                executedModules.Add(module); // 실행 목록에 추가

                async UniTask ExecuteModuleAsync()
                {
                    if (module.Delay > 0)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(module.Delay), ignoreTimeScale: true);
                    }

                    await module.AnimateAsync(ctx);
                }

                forwardTasks.Add(ExecuteModuleAsync());
            }

            await UniTask.WhenAll(forwardTasks);
        }

        public async UniTask RevertAsync(UIButtonAnimationContext ctx)
        {
            ctx.MotionHandle.Complete();

            var forwardTasks = new List<UniTask>();
            var executedModules = new List<ButtonAnimationModule>(); // [추가] 실제로 실행된 모듈만 기록

            foreach (var module in modules)
            {
                if (module == null || !module.Enable) continue;

                executedModules.Add(module); // 실행 목록에 추가

                async UniTask ExecuteModuleAsync()
                {
                    if (module.Delay > 0)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(module.Delay), ignoreTimeScale: true);
                    }

                    await module.RevertAsync(ctx);
                }

                forwardTasks.Add(ExecuteModuleAsync());
            }

            await UniTask.WhenAll(forwardTasks);
        }
    }
}