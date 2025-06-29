using LitMotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIAnimationToolkit.Core
{
    public class UIAnimationContext
    {
        public Component Owner { get; }
        public CompositeMotionHandle MotionHandle { get; } = new();
        public PointerEventData PointerEventData { get; internal set; }
        
        public UIAnimationContext(Component owner) 
        {
            Owner = owner;
        }
    }
}
