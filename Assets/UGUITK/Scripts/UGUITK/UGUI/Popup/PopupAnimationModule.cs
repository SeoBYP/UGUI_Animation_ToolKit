using System;
using Cysharp.Threading.Tasks;

namespace UGUIAnimationToolkit.Popup
{
    [Serializable]
    public abstract class PopupAnimationModule
    {
        public bool Enable = true;
        public string Description;
        public float Delay = 0f;
        public bool UseCurrentAsFrom = false;

        public abstract UniTask AnimateAsync(UIPopupAnimationContext ctx);
    }
}