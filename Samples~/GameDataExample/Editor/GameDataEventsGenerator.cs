using easycodegenunity.Editor.Core;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Samples.GameDataExample
{
    public class GameDataEventsGenerator : IEasyCodeGenerator
    {
        private const string GameDataFieldIdentifier = "_GAME_DATA_FIELD_";
        private const string OriginalFieldIdentifier = "_ORIGINAL_FIELD_";

        public void Execute()
        {
            foreach (var queryResult in EasyQuery.WithAttribute<GameData>())
            {
                var builder = new EasyCodeBuilder();
                builder
                    .WithTemplate<GameDataEventTemplate>()
                    .AddUsingStatement("System")
                    .AddUsingStatement("UnityEngine")
                    .AddNamespace(queryResult.Namespace)
                    .AddStruct(tb => tb
                        .WithName(queryResult.Name)
                        .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
                        .Build());

                var members = queryResult.WithMembers<GameDataField>();

                foreach (var memberQueryResult in members)
                {
                    builder
                        .AddField(fb => fb
                            .WithName($"On{memberQueryResult.Name.ToPascalCase()}Changed")
                            .WithType($"Action<{memberQueryResult.FriendlyTypeName}>")
                            .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.EventKeyword)
                            .Build());
                }

                foreach (var memberQueryResult in members)
                {
                    builder
                        .AddMethod(mb => mb
                            .WithName($"Set{memberQueryResult.Name.ToPascalCase()}")
                            .WithReturnType("void")
                            .WithModifiers(SyntaxKind.PublicKeyword)
                            .WithParameters((memberQueryResult.FriendlyTypeName, "newValue"))
                            .WithBodyFromTemplate(nameof(GameDataEventTemplate.Set_GAME_DATA_FIELD_))
                            .ReplaceInBody(GameDataFieldIdentifier, memberQueryResult.Name.ToPascalCase())
                            .ReplaceInBody(OriginalFieldIdentifier, memberQueryResult.Name)
                            .Build())
                        .AddMethod(mb => mb
                            .WithName($"Get{memberQueryResult.Name.ToPascalCase()}")
                            .WithReturnType(memberQueryResult.FriendlyTypeName)
                            .WithModifiers(SyntaxKind.PublicKeyword)
                            .WithBodyFromTemplate(nameof(GameDataEventTemplate.Get_GAME_DATA_FIELD_))
                            .ReplaceInBody(OriginalFieldIdentifier, memberQueryResult.Name)
                            .Build());
                }

                builder
                    .SetDirectory("Assets/easycodegenunity/Editor/Samples/GameDataExample/Runtime/Generated")
                    .SetFileName(queryResult.Name + "Events.cs")
                    .Generate()
                    .Save();
            }
        }
    }
}