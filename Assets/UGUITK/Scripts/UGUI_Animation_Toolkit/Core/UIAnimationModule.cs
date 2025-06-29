using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UGUIAnimationToolkit.Core
{
    [Serializable]
    public abstract class UIAnimationModule
    {
        public bool Enable = true;
        [Multiline] public string Description;
        public float Delay = 0f;

        public abstract UniTask AnimateAsync(UIAnimationContext ctx);
        public abstract UniTask RevertAsync(UIAnimationContext ctx);
    }
}
