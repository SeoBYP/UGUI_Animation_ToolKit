using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.Events;

namespace UGUI_Animation_Toolkit.Animators
{
    public class CanvasGroupAnimator
    {
        private readonly CanvasGroup _cg;
        private readonly float _from, _to, _duration;
        private MotionHandle _motion;
        public event UnityAction OnStart;
        public event UnityAction OnComplete;

        public CanvasGroupAnimator(CanvasGroup cg, float from, float to, float duration)
        {
            _cg = cg;
            _from = from;
            _to = to;
            _duration = duration;
        }
    }
}