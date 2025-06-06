using easycodegenunity.Editor.Core;

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
                    .AddType(StructDefinition(queryResult));

                var members = queryResult.WithMembers<GameDataField>();

                foreach (var memberQueryResult in members)
                {
                    builder.AddEvent(EventDefinition(memberQueryResult));
                }

                foreach (var memberQueryResult in members)
                {
                    builder
                        .AddMethod(SetMethodDefinition(memberQueryResult, builder))
                        .ReplaceInMethodBody(GameDataFieldIdentifier, memberQueryResult.Name.ToPascalCase())
                        .ReplaceInMethodBody(OriginalFieldIdentifier, memberQueryResult.Name)
                        .AddMethod(GetMethodDefinition(memberQueryResult, builder))
                        .ReplaceInMethodBody(OriginalFieldIdentifier, memberQueryResult.Name);
                }

                builder
                    .SetDirectory("Assets/easycodegenunity/Editor/Samples/GameDataExample/Generated")
                    .SetFileName(queryResult.Name + "Events.cs")
                    .GenerateCode();
            }
        }

        private EasyMethodInfo GetMethodDefinition(EasyQueryResult memberQueryResult, EasyCodeBuilder builder)
        {
            return new EasyMethodInfo
            {
                Name = $"Get{memberQueryResult.Name.ToPascalCase()}",
                ReturnType = memberQueryResult.FriendlyTypeName,
                AccessModifier = MemberAccessModifier.Public,
                Body = builder.ExtractMethodBodyFromTemplate(nameof(GameDataEventTemplate.Get_GAME_DATA_FIELD_))
            };
        }

        private EasyMethodInfo SetMethodDefinition(EasyQueryResult memberQueryResult,
            EasyCodeBuilder builder)
        {
            return new EasyMethodInfo
            {
                Name = $"Set{memberQueryResult.Name.ToPascalCase()}",
                ReturnType = "void",
                AccessModifier = MemberAccessModifier.Public,
                Parameters = new[]
                {
                    (memberQueryResult.FriendlyTypeName, "newValue")
                },
                Body = builder.ExtractMethodBodyFromTemplate(nameof(GameDataEventTemplate.Set_GAME_DATA_FIELD_))
            };
        }

        private EasyEventInfo EventDefinition(EasyQueryResult memberQueryResult)
        {
            return new EasyEventInfo
            {
                Name = $"On{memberQueryResult.Name.ToPascalCase()}Changed",
                Type = EasyEventInfo.EventType.Action,
                ParameterTypes = new[] { memberQueryResult.FriendlyTypeName },
                AccessModifier = MemberAccessModifier.Public
            };
        }

        private EasyTypeInfo StructDefinition(EasyQueryResult queryResult)
        {
            return new EasyTypeInfo
            {
                Type = EasyType.Struct,
                Name = queryResult.Name,
                IsPartial = true,
                AccessModifier = TypeAccessModifier.Public
            };
        }
    }
}