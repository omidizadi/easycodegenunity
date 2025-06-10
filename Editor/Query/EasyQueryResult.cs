using System;

namespace easycodegenunity.Editor.Core
{
    /// <summary>
    /// Represents a query result with information about a type or member.
    /// </summary>
    public struct EasyQueryResult
    {
        /// <summary>
        /// Gets or sets the name of the type or member.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the full name of the type or member.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the Type object representing the type.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets the friendly name of the type.
        /// </summary>
        public string FriendlyTypeName => Type.GetFriendlyTypeName();

        /// <summary>
        /// Gets or sets the namespace of the type.
        /// </summary>
        public string Namespace { get; set; }
    }
}
