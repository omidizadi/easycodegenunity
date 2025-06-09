using System;
using System.Reflection;

namespace easycodegenunity.Editor.Core
{
    public struct EasyQueryResult
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public Type Type { get; set; }
        public string FriendlyTypeName => Type.GetFriendlyTypeName();
        public string Namespace { get; set; }
    }
}