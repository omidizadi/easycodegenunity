using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyInterfaceBuilder : EasyTypeBuilder
    {
        protected override BaseTypeDeclarationSyntax CreateTypeDeclaration()
        {
            return SyntaxFactory.InterfaceDeclaration(name);
        }
    }
}