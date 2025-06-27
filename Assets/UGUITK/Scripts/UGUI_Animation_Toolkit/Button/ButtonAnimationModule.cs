using System;
using Cysharp.Threading.Tasks;
using UGUI_Animation_Toolkit.Button;
using UnityEngine;

namespace UGUIAnimationToolkit
{
    // 이벤트 타입 정의

    [Serializable]
    public abstract class ButtonAnimationModule
    {
        public bool Enable = true;
        
        [Tooltip("모듈에 대한 설명")]
        public string Description;
        
        [Tooltip("애니메이션 시작 전 대기 시간(초)")]
        public float Delay = 0f;
        
        public abstract UniTask AnimateAsync(UIButtonAnimationContext ctx);
        /// <summary>
        /// 되돌리기 애니메이션을 실행합니다.
        /// </summary>
        public abstract UniTask RevertAsync(UIButtonAnimationContext ctx);
    }
}