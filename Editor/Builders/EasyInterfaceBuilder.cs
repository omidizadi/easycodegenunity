using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    /// <summary>
    /// A builder class for creating interface declarations.
    /// </summary>
    public class EasyInterfaceBuilder : EasyTypeBuilder
    {
        /// <summary>
        /// Creates the interface declaration syntax.
        /// </summary>
        /// <returns>The created interface declaration syntax.</returns>
        protected override BaseTypeDeclarationSyntax CreateTypeDeclaration()
        {
            return SyntaxFactory.InterfaceDeclaration(name);
        }
    }
}