using System;
using System.Linq;
using System.Reflection;
using easycodegenunity.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace com.omidizadi.EasyCodeGen
{
    public static class EasyCodeGen
    {
        [MenuItem("Tools/Easy Code Generator")]
        public static void GenerateCode()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var typesWithAttribute = assembly.GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(IEasyCodeGenerator)) && !t.IsAbstract)
                .ToList();

            if (typesWithAttribute.Count == 0)
            {
                Debug.LogWarning("No code generators found. Please create a class that implements IEasyCodeGenerator.");
                return;
            }

            foreach (var type in typesWithAttribute)
            {
                var generator = (IEasyCodeGenerator)Activator.CreateInstance(type);
                generator.Execute();
            }
        }
    }
}