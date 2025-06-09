using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyClassBuilder : EasyTypeBuilder
    {
        protected override BaseTypeDeclarationSyntax CreateTypeDeclaration()
        {
            var classDeclaration = SyntaxFactory.ClassDeclaration(name);

            if (HasConstructor())
            {
                classDeclaration = classDeclaration.AddMembers(CreateConstructorDeclaration());
            }

            return classDeclaration;
        }
    }
}