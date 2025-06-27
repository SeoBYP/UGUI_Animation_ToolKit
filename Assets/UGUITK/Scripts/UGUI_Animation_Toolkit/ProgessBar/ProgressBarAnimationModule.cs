using System;
using Cysharp.Threading.Tasks;

namespace UGUIAnimationToolkit.ProgressBar
{
    [Serializable]
    public abstract class ProgressBarAnimationModule
    {
        public bool Enable = true;
        public string Description;
        public float Delay = 0f;

        public abstract UniTask AnimateAsync(ProgressBarAnimationContext ctx);
    }
}