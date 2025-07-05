// 파일 경로: Assets/UGUIAnimationToolkit/Text/TextAnimationContext.cs

using UGUIAnimationToolkit.Core;

namespace UGUIAnimationToolkit.Text
{
    public class TextAnimationContext : UIAnimationContext
    {
        public UGUIText TargetText { get; }
        
        // 시작 텍스트와 목표 텍스트를 저장할 프로퍼티 추가
        public string StartText { get; }
        public string TargetTextValue { get; }

        public TextAnimationContext(UGUIText owner, string startText, string targetText) : base(owner)
        {
            TargetText = owner;
            StartText = startText;
            TargetTextValue = targetText;
        }
    }
}