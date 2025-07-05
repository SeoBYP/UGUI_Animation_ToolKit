using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace UGUIAnimationToolkit.Core
{
    [Serializable]
    public class UIAnimationEvent
    {
        [Tooltip("호출할 때 사용할 이벤트의 고유 이름입니다.")]
        public string eventName = "";

        [Tooltip("이 이벤트가 완료된 후 호출될 유니티 이벤트입니다.")]
        public UnityEvent onCompleted;

        [SerializeField]
        public AnimationSequence sequence;

        // 생성자에서 초기화
        public UIAnimationEvent()
        {
            onCompleted = new UnityEvent();
            sequence = new AnimationSequence();
        }
    }
}
