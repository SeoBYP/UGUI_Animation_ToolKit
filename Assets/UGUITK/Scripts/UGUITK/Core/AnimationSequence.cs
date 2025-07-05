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

        [NonSerialized] private List<UIAnimationModule> _executeModules;

        // 안전한 초기화
        private List<UIAnimationModule> ExecuteModules
        {
            get
            {
                if (_executeModules == null)
                    _executeModules = new List<UIAnimationModule>();
                return _executeModules;
            }
        }

        public async UniTask PlayAsync(UIAnimationContext ctx)
        {
            ctx.MotionHandle.Complete();
            ExecuteModules.Clear(); // 프로퍼티 사용

            switch (mode)
            {
                case ExecutionMode.Parallel:
                {
                    var tasks = new List<UniTask>();
                    foreach (var module in modules)
                    {
                        if (module is null || !module.Enable)
                            continue;
                        ExecuteModules.Add(module); // 프로퍼티 사용
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
                        ExecuteModules.Add(module); // 프로퍼티 사용
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
            foreach (var module in ExecuteModules) // 프로퍼티 사용
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