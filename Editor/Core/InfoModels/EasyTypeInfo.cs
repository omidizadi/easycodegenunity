using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Core
{
    public struct EasyTypeInfo
    {
        public string Name { set; get; }
        public SyntaxKind[] Modifiers { get; set; }
        public EasyType Type { get; set; }
        public string BaseType { get; set; }
        public string[] Interfaces { get; set; }
    }
}