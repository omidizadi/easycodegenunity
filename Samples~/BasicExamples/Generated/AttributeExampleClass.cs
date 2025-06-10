using System;
using UnityEngine;
using System.Diagnostics;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generated
{
    [Serializable]
    [Obsolete("This class will be removed in future versions", false)]
    /// <summary>
    /// A class demonstrating various attributes
    /// </summary>
    public class AttributeExampleClass : MonoBehaviour
    {
        [Range(1, 1000)]
        private int _id;
        private bool _initialized = false;
        [Tooltip("Enter a description here")]
        [field: SerializeField]
        public string Description { get; set; }

        [Conditional("DEBUG")]
        [Obsolete("Use InitializeAsync instead", true)]
        public void Initialize()
        {
            _initialized = true;
            Console.WriteLine("Initialized");
        }
    }
}