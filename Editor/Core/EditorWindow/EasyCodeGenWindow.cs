using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using easycodegenunity.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace com.omidizadi.EasyCodeGen
{
    public class EasyCodeGenWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private readonly Dictionary<string, bool> _assemblySelections = new();
        private readonly List<string> _availableAssemblies = new();
        private bool initialized;
        private GUIStyle sectionStyle;
        private string assemblySearchTerm = "";

        [MenuItem("Window/EasyCodeGen")]
        public static void ShowWindow()
        {
            var window = GetWindow<EasyCodeGenWindow>("Easy Code Generator");
            window.minSize = new Vector2(400, 300);
        }

        private void OnEnable()
        {
            initialized = false;
        }

        private void Initialize()
        {
            sectionStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                margin = new RectOffset(0, 0, 8, 4)
            };

            RefreshAssemblyList();

            foreach (var assemblyName in EasyCodeGenSettings.Instance.IncludedAssemblies)
            {
                if (_assemblySelections.ContainsKey(assemblyName))
                {
                    _assemblySelections[assemblyName] = true;
                }
            }

            initialized = true;
        }

        private void RefreshAssemblyList()
        {
            _availableAssemblies.Clear();
            _assemblySelections.Clear();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .OrderBy(a => a.GetName().Name);

            foreach (var assembly in assemblies)
            {
                var assemblyName = assembly.GetName().Name;
                _availableAssemblies.Add(assemblyName);
                var isIncluded = EasyCodeGenSettings.Instance.IsAssemblyIncluded(assemblyName);
                _assemblySelections[assemblyName] = isIncluded;
            }
        }

        private void OnGUI()
        {
            if (!initialized)
            {
                Initialize();
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Assembly Selection", sectionStyle);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Include All", GUILayout.Width(100)))
            {
                IncludeAllAssemblies();
            }

            if (GUILayout.Button("Exclude All", GUILayout.Width(100)))
            {
                ExcludeAllAssemblies();
            }

            if (GUILayout.Button("Refresh List", GUILayout.Width(100)))
            {
                RefreshAssemblyList();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Select the assemblies to scan for code generators (classes implementing IEasyCodeGenerator)",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            var newSearchTerm = EditorGUILayout.TextField(assemblySearchTerm);
            if (newSearchTerm != assemblySearchTerm)
            {
                assemblySearchTerm = newSearchTerm;
                scrollPosition = Vector2.zero;
            }

            if (GUILayout.Button("X", GUILayout.Width(20)) && !string.IsNullOrEmpty(assemblySearchTerm))
            {
                assemblySearchTerm = "";
                scrollPosition = Vector2.zero;
            }

            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var assemblyName in _availableAssemblies)
            {
                if (!string.IsNullOrEmpty(assemblySearchTerm) &&
                    !assemblyName.ToLowerInvariant().Contains(assemblySearchTerm.ToLowerInvariant()))
                {
                    continue;
                }

                var previous = _assemblySelections[assemblyName];
                var current = EditorGUILayout.ToggleLeft(
                    new GUIContent(assemblyName), previous);

                if (previous != current)
                {
                    _assemblySelections[assemblyName] = current;
                    if (current)
                    {
                        EasyCodeGenSettings.Instance.AddAssembly(assemblyName);
                    }
                    else
                    {
                        EasyCodeGenSettings.Instance.RemoveAssembly(assemblyName);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(10);

            if (GUILayout.Button(new GUIContent("Generate Code", "Generate code using the selected assemblies"),
                    GUILayout.Height(30)))
            {
                GenerateCode();
            }

            EditorGUILayout.EndVertical();
        }

        private void IncludeAllAssemblies()
        {
            foreach (var assemblyName in _availableAssemblies)
            {
                _assemblySelections[assemblyName] = true;
                EasyCodeGenSettings.Instance.AddAssembly(assemblyName);
            }
        }

        private void ExcludeAllAssemblies()
        {
            foreach (var assemblyName in _availableAssemblies)
            {
                _assemblySelections[assemblyName] = false;
            }

            EasyCodeGenSettings.Instance.ClearAssemblies();
        }

        private void GenerateCode()
        {
            var generatorCount = 0;

            foreach (var assemblyEntry in _assemblySelections.Where(kv => kv.Value))
            {
                var assemblyName = assemblyEntry.Key;

                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == assemblyName);

                if (assembly != null)
                {
                    var generators = assembly.GetTypes()
                        .Where(t => t.GetInterfaces().Contains(typeof(IEasyCodeGenerator)) && !t.IsAbstract)
                        .ToList();

                    if (generators.Count > 0)
                    {
                        foreach (var generatorType in generators)
                        {
                            var generator = (IEasyCodeGenerator)Activator.CreateInstance(generatorType);
                            generator.Execute();
                            generatorCount++;
                            Debug.Log($"Executed generator: {generatorType.Name}");
                        }
                    }
                }
            }

            if (generatorCount > 0)
            {
                EditorUtility.DisplayDialog("Code Generation Complete",
                    $"Successfully executed {generatorCount} code generator{(generatorCount > 1 ? "s" : "")}.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("No Generators Found",
                    "No code generators were found in the selected assemblies. Make sure you've selected assemblies containing classes that implement IEasyCodeGenerator.",
                    "OK");
            }
        }
    }
}