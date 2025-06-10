using System;
using easycodegenunity.Editor.Core;

namespace easycodegenunity.Editor.Samples.BasicExamples.Templates
{
    /// <summary>
    /// A template class demonstrating how to create templates for method bodies, property accessors, and constructors.
    /// This class is not meant to be used directly but as a template for code generation.
    /// </summary>
    internal class BasicTemplate
    {
        // Placeholder values that will be replaced during code generation
        private string _NAME_PLACEHOLDER_;
        private int _AGE_PLACEHOLDER_;
        private string[] _ITEMS_PLACEHOLDER_;
        
        // Example constructor template with placeholders
        public BasicTemplate(string name, int age)
        {
            Console.WriteLine($"Creating new instance for {name}");
            this._NAME_PLACEHOLDER_ = name;
            this._AGE_PLACEHOLDER_ = age;
            this._ITEMS_PLACEHOLDER_ = new string[0];
        }
        
        // Example property with custom getter and setter
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_NAME_PLACEHOLDER_))
                    return "Unknown";
                    
                return _NAME_PLACEHOLDER_ + " (Age: " + _AGE_PLACEHOLDER_ + ")";
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("DisplayName cannot be null or empty");
                    
                _NAME_PLACEHOLDER_ = value;
                Console.WriteLine($"Name changed to {value}");
            }
        }
        
        // Example method with complex logic
        public bool AddItem(string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                Console.WriteLine("Cannot add empty item");
                return false;
            }
            
            // Check if item already exists
            foreach (var existingItem in _ITEMS_PLACEHOLDER_)
            {
                if (existingItem.Equals(item, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Item '{item}' already exists");
                    return false;
                }
            }
            
            // Add the item to the array
            Array.Resize(ref _ITEMS_PLACEHOLDER_, _ITEMS_PLACEHOLDER_.Length + 1);
            _ITEMS_PLACEHOLDER_[_ITEMS_PLACEHOLDER_.Length - 1] = item;
            Console.WriteLine($"Added item: {item}");
            
            return true;
        }
        
        // Simple expression-bodied method for demonstration
        public string GetGreeting() => $"Hello, {_NAME_PLACEHOLDER_}!";
    }
}
