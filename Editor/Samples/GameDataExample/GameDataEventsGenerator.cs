using System.Linq;
using easycodegenunity.Editor.Core;

namespace easycodegenunity.Editor.Samples.GameDataExample
{
    public class GameDataEventsGenerator : IEasyCodeGenerator
    {
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
                    .AddType(new EasyTypeInfo
                    {
                        Type = EasyType.Struct,
                        Name = queryResult.Name,
                        IsPartial = true,
                        AccessModifier = TypeAccessModifier.Public
                    });

                var members = queryResult.WithAttribute<GameDataField>();

                foreach (var memberQueryResult in members)
                {
                    builder
                        .AddEvent(new EasyEventInfo
                        {
                            Name = $"On{memberQueryResult.Name.ToPascalCase()}Changed",
                            Type = EasyEventInfo.EventType.Action,
                            ParameterTypes = new[] { memberQueryResult.FriendlyTypeName },
                            AccessModifier = MemberAccessModifier.Public
                        });
                }

                foreach (var memberQueryResult in members)
                {
                    builder
                        .AddMethod(new EasyMethodInfo
                        {
                            Name = $"Set{memberQueryResult.Name.ToPascalCase()}",
                            ReturnType = "void",
                            AccessModifier = MemberAccessModifier.Public,
                            Parameters = new[]
                            {
                                (memberQueryResult.FriendlyTypeName, "newValue")
                            },
                            Body = builder.ExtractMethodBodyFromTemplate(nameof(GameDataEventTemplate
                                .Set_GAME_DATA_FIELD_))
                        })
                        .ReplaceInMethodBody("_GAME_DATA_FIELD_", memberQueryResult.Name.ToPascalCase())
                        .ReplaceInMethodBody("_ORIGINAL_FIELD_", memberQueryResult.Name)
                        .AddMethod(new EasyMethodInfo
                        {
                            Name = $"Get{memberQueryResult.Name.ToPascalCase()}",
                            ReturnType = memberQueryResult.FriendlyTypeName,
                            AccessModifier = MemberAccessModifier.Public,
                            Body = builder.ExtractMethodBodyFromTemplate(nameof(GameDataEventTemplate
                                .Get_GAME_DATA_FIELD_))
                        })
                        .ReplaceInMethodBody("_ORIGINAL_FIELD_", memberQueryResult.Name);
                }

                builder
                    .SetDirectory("Assets/easycodegenunity/Editor/Samples/GameDataExample/Generated")
                    .SetFileName(queryResult.Name + "Events.cs")
                    .GenerateCode();
            }
        }
    }
}