using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Core
{
    public struct EasyPropertyInfo
    {
        public string Name { get; set; }
        public SyntaxKind[] Modifiers { get; set; }
        public string Type { get; set; }
        public string InitialValue { get; set; }
        public string Getter { get; set; }
        public string Setter { get; set; }
    }
}