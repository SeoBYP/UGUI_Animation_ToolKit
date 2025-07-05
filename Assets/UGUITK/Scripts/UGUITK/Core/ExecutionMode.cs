using System;

namespace UGUIAnimationToolkit.Core
{
    [Serializable]
    public enum ExecutionMode
    {
        Parallel, // 모든 모듈이 동시에 실행
        Sequential // 모든 모듈이 순서대로 실행
    }
}