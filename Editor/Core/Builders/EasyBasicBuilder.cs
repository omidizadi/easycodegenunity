using System;
using Microsoft.CodeAnalysis;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyBasicBuilder
    {
        //todo: move comment buildings here

        protected SyntaxNode templateRoot;

        protected EasyBasicBuilder()
        {
        }

        protected EasyBasicBuilder(SyntaxNode templateRoot)
        {
            this.templateRoot = templateRoot ??
                                throw new ArgumentNullException(nameof(templateRoot), "Template root cannot be null.");
        }
    }
}