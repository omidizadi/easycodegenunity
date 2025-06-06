using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace easycodegenunity.Editor.Core.Builders
{
    public class EasyTypeBuilder
    {
        private string name;
        private BaseTypeDeclarationSyntax typeDeclaration;

        public EasyTypeBuilder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Type name cannot be null or empty.", nameof(name));
            }

            this.name = name;
            return this;
        }

        public EasyTypeBuilder WithType(EasyType type)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Type name cannot be null or empty.", nameof(name));
            }

            typeDeclaration = type switch
            {
                EasyType.Class => SyntaxFactory.ClassDeclaration(name),
                EasyType.Struct => SyntaxFactory.StructDeclaration(name),
                EasyType.Interface => SyntaxFactory.InterfaceDeclaration(name),
                EasyType.Enum => SyntaxFactory.EnumDeclaration(name),
                _ => throw new ArgumentException("Invalid type specified.", nameof(type))
            };
            return this;
        }

        public EasyTypeBuilder WithBaseType(string type)
        {
            if (typeDeclaration == null)
            {
                throw new InvalidOperationException("Type must be set before setting a base type.");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new InvalidOperationException(
                    "Base type cannot be null or empty. Use 'WithType' method to set the type first.");
            }

            var baseType = SyntaxFactory.ParseTypeName(type);
            typeDeclaration = typeDeclaration.WithBaseList(
                SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                    SyntaxFactory.SimpleBaseType(baseType))));

            return this;
        }

        public EasyTypeBuilder WithInterfaces(params string[] interfaces)
        {
            if (interfaces == null || interfaces.Length == 0)
            {
                throw new ArgumentException("At least one interface must be specified.", nameof(interfaces));
            }

            var interfaceList = interfaces.Select(i => SyntaxFactory.SimpleBaseType(
                SyntaxFactory.ParseTypeName(i))).ToArray();
            typeDeclaration = typeDeclaration.WithBaseList(
                SyntaxFactory.BaseList(SyntaxFactory.SeparatedList<BaseTypeSyntax>(interfaceList)));

            return this;
        }

        public EasyTypeBuilder WithModifiers(params SyntaxKind[] modifiers)
        {
            if (typeDeclaration == null)
            {
                throw new InvalidOperationException("Type must be set before setting modifiers.");
            }

            if (modifiers == null || modifiers.Length == 0)
            {
                throw new ArgumentException("Modifiers cannot be null or empty.", nameof(modifiers));
            }

            foreach (var modifier in modifiers)
            {
                typeDeclaration = typeDeclaration.AddModifiers(SyntaxFactory.Token(modifier));
            }

            return this;
        }

        public BaseTypeDeclarationSyntax Build()
        {
            if (typeDeclaration == null)
            {
                throw new InvalidOperationException("Type must be set before building.");
            }

            return typeDeclaration;
        }
    }
}