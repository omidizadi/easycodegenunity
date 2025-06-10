using System;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generated
{
    public class PersonFromTemplate
    {
        private string name;
        private int age;
        private string[] items;
        public PersonFromTemplate(string name, int age)
        {
            Console.WriteLine($"Creating new instance for {name}");
            this.name = name;
            this.age = age;
            this.items = new string[0];
        }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                    return "Unknown"; return  name + " (Age: " + age + ")" ; 
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("DisplayName cannot be null or empty"); name  =  value ;  Console . WriteLine ( $"Name changed to {value}" ) ; 
            }
        }

        public bool AddItem(string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                Console.WriteLine("Cannot add empty item");
                return false;
            }

            // Check if item already exists
            foreach (var existingItem in items)
            {
                if (existingItem.Equals(item, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Item '{item}' already exists");
                    return false;
                }
            }

            // Add the item to the array
            Array.Resize(ref items, items.Length + 1);
            items[items.Length - 1] = item;
            Console.WriteLine($"Added item: {item}");
            return true;
        }

        public string GetGreeting() => $"Hello, {name}!";
    }
}