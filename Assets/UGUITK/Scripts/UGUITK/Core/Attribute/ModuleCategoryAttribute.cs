using System;

namespace UGUIAnimationToolkit
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleCategoryAttribute : Attribute
    {
        public string Path { get; }
        
        // [추가] 정렬 순서를 위한 Order 프로퍼티.
        // 기본값을 큰 수로 설정하여, Order를 지정하지 않은 항목이 뒤로 가도록 합니다.
        public int Order { get; set; } = int.MaxValue;

        public ModuleCategoryAttribute(string path)
        {
            Path = path;
        }
    }
}