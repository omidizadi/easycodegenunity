using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyStructBuilder : EasyTypeBuilder
    {
        public EasyStructBuilder(SyntaxNode templateRoot) : base(templateRoot)
        {
        }

        protected override BaseTypeDeclarationSyntax CreateTypeDeclaration()
        {
            var structDeclaration = SyntaxFactory.StructDeclaration(name);

            if (HasConstructor())
            {
                structDeclaration = structDeclaration.AddMembers(CreateConstructorDeclaration());
            }

            return structDeclaration;
        }
    }
}