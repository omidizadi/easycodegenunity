using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyStructBuilder : EasyTypeWithConstructorBuilder
    {
        protected override BaseTypeDeclarationSyntax CreateTypeDeclaration()
        {
            var structDeclaration = SyntaxFactory.StructDeclaration(Name);
            
            if (HasConstructor())
            {
                structDeclaration = structDeclaration.AddMembers(CreateConstructorDeclaration());
            }
            
            return structDeclaration;
        }
    }
}