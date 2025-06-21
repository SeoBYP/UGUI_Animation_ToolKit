using System;
using Cysharp.Threading.Tasks;

namespace UGUIAnimationToolkit.Text
{
    [Serializable]
    public abstract class TextAnimationModule
    {
        public bool Enable = true;
        public string Description;
        public float Delay = 0f;

        public abstract UniTask AnimateAsync(TextAnimationContext ctx);
        
        public abstract UniTask RevertAsync(TextAnimationContext ctx);
    }
}