using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    /// <summary>
    /// An abstract base class for builders that create syntax nodes.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder, used for chaining.</typeparam>
    public abstract class EasyBasicBuilder<TBuilder> where TBuilder : EasyBasicBuilder<TBuilder>
    {
        protected SyntaxNode templateRoot;

        private string comment;

        private List<AttributeListSyntax> attributeLists = new List<AttributeListSyntax>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyBasicBuilder{TBuilder}"/> class.
        /// </summary>
        protected EasyBasicBuilder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyBasicBuilder{TBuilder}"/> class with a template root.
        /// </summary>
        /// <param name="templateRoot">The template root.</param>
        protected EasyBasicBuilder(SyntaxNode templateRoot)
        {
            this.templateRoot = templateRoot;
        }

        /// <summary>
        /// Adds a single-line comment to the generated code.
        /// </summary>
        /// <param name="commentText">The comment text.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TBuilder WithSingleLineComment(string commentText)
        {
            comment = "// " + commentText;
            return (TBuilder)this;
        }

        /// <summary>
        /// Adds a multi-line comment to the generated code.
        /// </summary>
        /// <param name="commentText">The comment text.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TBuilder WithMultiLineComment(string commentText)
        {
            comment = "/* " + commentText + " */";
            return (TBuilder)this;
        }

        /// <summary>
        /// Adds a summary comment (XML documentation) to the generated code.
        /// </summary>
        /// <param name="commentText">The comment text.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TBuilder WithSummaryComment(string commentText)
        {
            comment = "/// <summary>\n/// " + commentText + "\n/// </summary>";
            return (TBuilder)this;
        }

        /// <summary>
        /// Adds a parameter comment (XML documentation) to the generated code.
        /// </summary>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="description">The description of the parameter.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TBuilder WithParamComment(string paramName, string description)
        {
            if (comment == null)
            {
                comment = "/// <param name=\"" + paramName + "\">" + description + "</param>";
            }
            else
            {
                comment += "\n/// <param name=\"" + paramName + "\">" + description + "</param>";
            }

            return (TBuilder)this;
        }

        /// <summary>
        /// Adds a returns comment (XML documentation) to the generated code.
        /// </summary>
        /// <param name="description">The description of the return value.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TBuilder WithReturnsComment(string description)
        {
            if (comment == null)
            {
                comment = "/// <returns>" + description + "</returns>";
            }
            else
            {
                comment += "\n/// <returns>" + description + "</returns>";
            }

            return (TBuilder)this;
        }

        /// <summary>
        /// Replaces text in a parameter comment (XML documentation).
        /// </summary>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="oldValue">The old text to replace.</param>
        /// <param name="newValue">The new text to replace with.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TBuilder ReplaceParamCommentText(string paramName, string oldValue, string newValue)
        {
            if (comment == null)
            {
                throw new InvalidOperationException("Comment is not set. Please set a comment first.");
            }

            // Find the parameter comment for this specific parameter
            int paramStart = comment.IndexOf("<param name=\"" + paramName + "\">");
            if (paramStart >= 0)
            {
                int paramEnd = comment.IndexOf("</param>", paramStart);
                if (paramEnd >= 0)
                {
                    string beforeParam = comment.Substring(0, paramStart);
                    string paramContent = comment.Substring(paramStart, paramEnd - paramStart + 8); // +8 for "</param>"
                    string afterParam = comment.Substring(paramEnd + 8);

                    // Replace text only in the specific parameter
                    paramContent = paramContent.Replace(oldValue, newValue);

                    // Reconstruct the comment
                    comment = beforeParam + paramContent + afterParam;
                }
            }

            return (TBuilder)this;
        }

        /// <summary>
        /// Extracts a comment from a template member and applies it to the generated code.
        /// </summary>
        /// <param name="memberName">The name of the member in the template.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TBuilder WithCommentFromTemplate(string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                throw new ArgumentNullException(nameof(memberName), "Member name cannot be null or empty.");
            }

            comment = ExtractCommentFromTemplate(memberName);
            return (TBuilder)this;
        }

        /// <summary>
        /// Replaces text in the comment.
        /// </summary>
        /// <param name="oldValue">The old text to replace.</param>
        /// <param name="newValue">The new text to replace with.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TBuilder ReplaceInComment(string oldValue, string newValue)
        {
            if (comment == null)
            {
                throw new InvalidOperationException("Comment is not set. Please set a comment first.");
            }

            if (string.IsNullOrEmpty(oldValue))
            {
                throw new ArgumentException("Old value cannot be null or empty.", nameof(oldValue));
            }

            comment = comment.Replace(oldValue, newValue);
            return (TBuilder)this;
        }

        /// <summary>
        /// Adds an attribute to the generated code.
        /// </summary>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <param name="parameters">The parameters of the attribute (optional).</param>
        /// <returns>The builder instance for chaining.</returns>
        public TBuilder WithAttribute(string attributeName, string parameters = null)
        {
            if (string.IsNullOrWhiteSpace(attributeName))
            {
                throw new ArgumentException("Attribute name cannot be null or empty.", nameof(attributeName));
            }

            var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName));

            if (!string.IsNullOrWhiteSpace(parameters))
            {
                var argumentList = SyntaxFactory.ParseAttributeArgumentList(parameters);
                attribute = attribute.WithArgumentList(argumentList);
            }

            var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute));

            attributeLists.Add(attributeList);

            return (TBuilder)this;
        }

        /// <summary>
        /// Builds the syntax node.
        /// </summary>
        /// <returns>The built syntax node.</returns>
        public MemberDeclarationSyntax Build()
        {
            var memberDeclaration = BuildDeclarationSyntax();

            if (comment != null)
            {
                var triviaList = new List<SyntaxTrivia>();

                string[] commentLines = comment.Split('\n');
                foreach (var line in commentLines)
                {
                    triviaList.Add(SyntaxFactory.Comment(line));
                    triviaList.Add(SyntaxFactory.CarriageReturnLineFeed);
                }

                memberDeclaration = memberDeclaration.WithLeadingTrivia(SyntaxFactory.TriviaList(triviaList));
            }

            if (attributeLists is { Count: > 0 })
            {
                memberDeclaration = memberDeclaration.WithAttributeLists(SyntaxFactory.List(attributeLists));
            }

            return memberDeclaration;
        }

        /// <summary>
        /// Builds the declaration syntax.
        /// </summary>
        /// <returns>The built member declaration syntax.</returns>
        protected abstract MemberDeclarationSyntax BuildDeclarationSyntax();

        /// <summary>
        /// Extracts a comment from a template member.
        /// </summary>
        /// <param name="memberName">The name of the member.</param>
        /// <returns>The extracted comment.</returns>
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

        /// <summary>
        /// Finds a named node in the template.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <returns>The found syntax node.</returns>
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