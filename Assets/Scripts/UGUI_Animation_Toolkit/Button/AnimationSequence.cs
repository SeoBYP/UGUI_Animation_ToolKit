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

        [SerializeReference] public List<ButtonAnimationModule> modules = new List<ButtonAnimationModule>();

        public async UniTask PlayAsync(UIButtonAnimationContext ctx)
        {
            ctx.MotionHandle.Cancel();

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

            // [추가] AutoRevert 로직
            if (AutoRevert)
            {
                // RevertDelay가 있다면 대기
                if (RevertDelay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(RevertDelay), ignoreTimeScale: true);
                }

                // 되돌리기 애니메이션을 위한 Task 리스트
                var revertTasks = new List<UniTask>();

                // 실행되었던 모듈들에 대해서만 RevertAsync 호출
                foreach (var module in executedModules)
                {
                    revertTasks.Add(module.RevertAsync(ctx));
                }

                // 모든 되돌리기 애니메이션이 끝날 때까지 대기
                await UniTask.WhenAll(revertTasks);
            }
        }
    }
}