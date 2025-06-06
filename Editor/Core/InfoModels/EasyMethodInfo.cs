using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Core
{
    public struct EasyMethodInfo
    {
        public string Name { get; set; }
        public SyntaxKind[] Modifiers { get; set; }
        public string ReturnType { get; set; }
        public (string, string)[] Parameters { get; set; }
        public string Body { get; set; }
    }
}