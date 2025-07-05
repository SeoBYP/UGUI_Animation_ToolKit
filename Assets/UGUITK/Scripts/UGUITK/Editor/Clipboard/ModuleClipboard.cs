#if UNITY_EDITOR
using System;

namespace UGUIAnimationToolkit.Editor
{
    // 버튼과 텍스트 에디터가 클립보드를 공유하기 위한 정적 클래스
    internal static class ModuleClipboard
    {
        internal static string ClipboardJson;
        internal static Type ClipboardType;
    }
}
#endif