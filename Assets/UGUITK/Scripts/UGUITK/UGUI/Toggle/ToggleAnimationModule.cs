using System;
using Cysharp.Threading.Tasks;

namespace UGUIAnimationToolkit.Toggle
{
    [Serializable]
    public abstract class ToggleAnimationModule
    {
        public bool Enable = true;
        public string Description;
        public float Delay = 0f;
        public bool UseCurrentAsFrom = false;

        public abstract UniTask AnimateAsync(ToggleAnimationContext ctx);
    }
}