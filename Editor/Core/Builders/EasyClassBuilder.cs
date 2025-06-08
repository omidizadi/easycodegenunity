using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyClassBuilder : EasyTypeWithConstructorBuilder
    {
        protected override BaseTypeDeclarationSyntax CreateTypeDeclaration()
        {
            var classDeclaration = SyntaxFactory.ClassDeclaration(Name);
            
            if (HasConstructor())
            {
                classDeclaration = classDeclaration.AddMembers(CreateConstructorDeclaration());
            }
            
            return classDeclaration;
        }
    }
}