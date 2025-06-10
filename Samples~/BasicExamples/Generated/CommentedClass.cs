using System;
using System.Collections.Generic;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generated
{
    /* This class is generated automatically.
    It contains examples of different types of comments
    that can be applied to types and members.
    Last generated: 2025-06-10 */
    public class CommentedClass
    {
        // The name of the person
        private string _name;
        /// <summary>
        /// Gets or sets the person's name
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the CommentedClass class
        /// </summary>
        /// <param name="name">The person's name</param>
        /// <param name="age">The person's age</param>
        /// <param name="isActive">Whether the person is active</param>
        public CommentedClass(string name, int age, bool isActive)
        {
            _name = name;
        }

        // Formats the name in uppercase
        public string GetFormattedName()
        {
            return _name.ToUpper();
        }

        /// <summary>
        /// Processes the string data in the provided collection
        /// </summary>
        /// <param name="items">The collection of strings to process</param>
        public void ProcessData(List<string> items)
        {
        }
    }
}