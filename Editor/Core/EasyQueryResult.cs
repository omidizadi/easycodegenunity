using System;
using System.Reflection;

namespace easycodegenunity.Editor.Core
{
    public struct EasyQueryResult
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public Type Type { get; set; }
        public string FriendlyTypeName => EasyQuery.GetFriendlyTypeName(Type);
        public string Namespace { get; set; }

        //todo: don't like the fact that this will be here both for types and members, but it is convenient for now
        public MemberInfo MemberInfo { get; set; } // if the result is a member, this will be set
    }
}