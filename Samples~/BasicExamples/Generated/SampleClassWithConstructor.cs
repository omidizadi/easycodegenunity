using System;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generated
{
    public class SampleClassWithConstructor
    {
        private int age;
        private string name;
        public SampleClassWithConstructor(string name, int age)
        {
            Console.WriteLine("Constructor called!");
            this.name = name;
            this.age = age;
        }
    }
}