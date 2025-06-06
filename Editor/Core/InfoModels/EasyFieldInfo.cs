using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Core
{
    public struct EasyFieldInfo
    {
        public string Name { get; set; }
        public SyntaxKind[] Modifiers { get; set; }
        public string Type { get; set; }
        public string InitialValue { get; set; }
    }
}