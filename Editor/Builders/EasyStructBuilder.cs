using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    /// <summary>
    /// A builder class for creating struct declarations.
    /// </summary>
    public class EasyStructBuilder : EasyTypeBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EasyStructBuilder"/> class.
        /// </summary>
        /// <param name="templateRoot">The template root.</param>
        public EasyStructBuilder(SyntaxNode templateRoot = null) : base(templateRoot)
        {
        }

        /// <summary>
        /// Creates the struct declaration syntax.
        /// </summary>
        /// <returns>The created struct declaration syntax.</returns>
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