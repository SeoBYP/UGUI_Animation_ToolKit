using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace UGUIAnimationToolkit.Core
{
    [Serializable]
    public class AnimationSequence
    {
        [Tooltip("모듈 실행 방식 (동시 또는 순차)")] public ExecutionMode mode = ExecutionMode.Parallel;

        [SerializeReference] public List<UIAnimationModule> modules = new List<UIAnimationModule>();
        private readonly List<UIAnimationModule> _executeModules = new List<UIAnimationModule>();

        public async UniTask PlayAsync(UIAnimationContext ctx)
        {
            ctx.MotionHandle.Complete();
            _executeModules.Clear();

            switch (mode)
            {
                case ExecutionMode.Parallel:
                {
                    var tasks = new List<UniTask>();
                    foreach (var module in modules)
                    {
                        if (module is null || !module.Enable)
                            continue;
                        _executeModules.Add(module);
                        tasks.Add(ExecuteModuleWithDelay(module, ctx));
                    }
                    await UniTask.WhenAll(tasks);
                }
                    break;
                case ExecutionMode.Sequential:
                {
                    foreach (var module in modules)
                    {
                        if (module is null || !module.Enable)
                            continue;
                        _executeModules.Add(module);
                        await ExecuteModuleWithDelay(module, ctx);
                    }
                }
                    break;
            }
        }

        public async UniTask RevertAsync(UIAnimationContext ctx)
        {
            ctx.MotionHandle.Complete();
            var tasks = new List<UniTask>();
            foreach (var module in _executeModules)
            {
                if (module is null || !module.Enable)
                    continue;
                tasks.Add(module.RevertAsync(ctx));
            }
            await UniTask.WhenAll(tasks);
        }

        private async UniTask ExecuteModuleWithDelay(UIAnimationModule module, UIAnimationContext ctx)
        {
            if (module.Delay > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(module.Delay), ignoreTimeScale: true);
            }

            await module.AnimateAsync(ctx);
        }
    }
}