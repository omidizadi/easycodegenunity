using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PlasticGui;

namespace easycodegenunity.Editor.Core.Builders
{
    public abstract class EasyBasicBuilder
    {
        //todo: move comment buildings here

        protected SyntaxNode templateRoot;

        private string comment;

        private AttributeListSyntax attributes;

        protected EasyBasicBuilder()
        {
        }

        protected EasyBasicBuilder(SyntaxNode templateRoot)
        {
            this.templateRoot = templateRoot ??
                                throw new ArgumentNullException(nameof(templateRoot), "Template root cannot be null.");
        }

        public EasyBasicBuilder WithComment(string comment)
        {
            this.comment = comment;
            return this;
        }

        public EasyBasicBuilder WithCommentFromTemplate(string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                throw new ArgumentNullException(nameof(memberName), "Member name cannot be null or empty.");
            }

            comment = ExtractCommentFromTemplate(memberName);
            return this;
        }

        public EasyBasicBuilder WithAttribute<T>(params object[] args) where T : Attribute
        {
            if (args == null || args.Length == 0)
            {
                throw new ArgumentException("At least one argument must be provided for the attribute.", nameof(args));
            }

            var attributeName = typeof(T).Name;
            var attributeArgumentList = SyntaxFactory.AttributeArgumentList(
                SyntaxFactory.SeparatedList(args.Select(arg =>
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(arg.ToString()))))));

            var attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName(attributeName), attributeArgumentList);
            attributes = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute));
            return this;
        }

        public MemberDeclarationSyntax Build()
        {
            var memberDeclaration = BuildDeclarationSyntax();
            memberDeclaration
                .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Comment(comment)))
                .WithAttributeLists(attributes != null
                    ? SyntaxFactory.SingletonList(attributes)
                    : SyntaxFactory.List<AttributeListSyntax>());

            return memberDeclaration;
        }

        protected abstract MemberDeclarationSyntax BuildDeclarationSyntax();

        private string ExtractCommentFromTemplate(string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                throw new ArgumentNullException(nameof(memberName), "Name cannot be null or empty.");
            }

            if (templateRoot == null)
            {
                throw new InvalidOperationException("Template root is not set. Please set a template first.");
            }

            SyntaxNode namedNode = FindNamedNode(memberName);

            if (namedNode == null)
            {
                return string.Empty;
            }

            var leadingTrivia = namedNode.GetLeadingTrivia();

            var commentBuilder = new System.Text.StringBuilder();
            bool hasComment = false;

            foreach (var trivia in leadingTrivia)
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                {
                    string text = trivia.ToString().TrimStart('/').Trim();
                    commentBuilder.AppendLine(text);
                    hasComment = true;
                }
                else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    string text = trivia.ToString();
                    text = text.Substring(2, text.Length - 4).Trim();
                    commentBuilder.AppendLine(text);
                    hasComment = true;
                }
                else if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                {
                    string text = trivia.ToString().TrimStart('/').Trim();
                    commentBuilder.AppendLine(text);
                    hasComment = true;
                }
            }

            return hasComment ? commentBuilder.ToString().TrimEnd() : string.Empty;
        }

        private SyntaxNode FindNamedNode(string name)
        {
            var typeDeclaration = templateRoot.DescendantNodes().OfType<BaseTypeDeclarationSyntax>()
                .FirstOrDefault(t => t.Identifier.Text == name);
            if (typeDeclaration != null)
                return typeDeclaration;

            var methodDeclaration = templateRoot.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.Text == name);
            if (methodDeclaration != null)
                return methodDeclaration;

            var propertyDeclaration = templateRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.Text == name);
            if (propertyDeclaration != null)
                return propertyDeclaration;

            var fieldDeclaration = templateRoot.DescendantNodes().OfType<FieldDeclarationSyntax>()
                .FirstOrDefault(f => f.Declaration.Variables.Any(v => v.Identifier.Text == name));
            if (fieldDeclaration != null)
                return fieldDeclaration;

            var enumMemberDeclaration = templateRoot.DescendantNodes().OfType<EnumMemberDeclarationSyntax>()
                .FirstOrDefault(e => e.Identifier.Text == name);
            if (enumMemberDeclaration != null)
                return enumMemberDeclaration;

            return null; // No matching node found
        }
    }
}