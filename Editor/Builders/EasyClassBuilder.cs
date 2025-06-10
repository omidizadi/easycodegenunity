using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    /// <summary>
    /// A builder class for creating class declarations.
    /// </summary>
    public class EasyClassBuilder : EasyTypeBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EasyClassBuilder"/> class.
        /// </summary>
        /// <param name="templateRoot">The template root.</param>
        public EasyClassBuilder(SyntaxNode templateRoot = null) : base(templateRoot)
        {
        }

        /// <summary>
        /// Creates the class declaration syntax.
        /// </summary>
        /// <returns>The created class declaration syntax.</returns>
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