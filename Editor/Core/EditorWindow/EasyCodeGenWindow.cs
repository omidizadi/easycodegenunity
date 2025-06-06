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
        private Vector2 _scrollPosition;
        private Dictionary<string, bool> _assemblySelections = new Dictionary<string, bool>();
        private List<string> _availableAssemblies = new List<string>();
        private bool _initialized = false;
        private GUIStyle _headerStyle;
        private GUIStyle _sectionStyle;
        private string _assemblySearchTerm = "";

        [MenuItem("Window/EasyCodeGen")]
        public static void ShowWindow()
        {
            var window = GetWindow<EasyCodeGenWindow>("Easy Code Generator");
            window.minSize = new Vector2(400, 300);
        }

        private void OnEnable()
        {
            _initialized = false;
        }

        private void Initialize()
        {
            // Set up styles
            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                margin = new RectOffset(0, 0, 10, 10)
            };

            _sectionStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                margin = new RectOffset(0, 0, 8, 4)
            };

            // Get all assemblies in the current domain
            RefreshAssemblyList();

            // Load selected assemblies from settings
            foreach (string assemblyName in EasyCodeGenSettings.Instance.IncludedAssemblies)
            {
                if (_assemblySelections.ContainsKey(assemblyName))
                {
                    _assemblySelections[assemblyName] = true;
                }
            }

            _initialized = true;
        }

        private void RefreshAssemblyList()
        {
            _availableAssemblies.Clear();
            _assemblySelections.Clear();

            // Get all loaded assemblies in the domain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .OrderBy(a => a.GetName().Name);

            foreach (var assembly in assemblies)
            {
                string assemblyName = assembly.GetName().Name;
                _availableAssemblies.Add(assemblyName);
                bool isIncluded = EasyCodeGenSettings.Instance.IsAssemblyIncluded(assemblyName);
                _assemblySelections[assemblyName] = isIncluded;
            }
        }

        private void OnGUI()
        {
            if (!_initialized)
            {
                Initialize();
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Assembly Selection", _sectionStyle);

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

            // Add search field for assemblies
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            string newSearchTerm = EditorGUILayout.TextField(_assemblySearchTerm);
            if (newSearchTerm != _assemblySearchTerm)
            {
                _assemblySearchTerm = newSearchTerm;
                // Reset scroll position when search changes
                _scrollPosition = Vector2.zero;
            }

            if (GUILayout.Button("X", GUILayout.Width(20)) && !string.IsNullOrEmpty(_assemblySearchTerm))
            {
                _assemblySearchTerm = "";
                // Reset scroll position when clearing search
                _scrollPosition = Vector2.zero;
            }

            EditorGUILayout.EndHorizontal();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Filter and display assembly selection based on search term
            foreach (var assemblyName in _availableAssemblies)
            {
                // Skip assemblies that don't match the search term
                if (!string.IsNullOrEmpty(_assemblySearchTerm) &&
                    !assemblyName.ToLowerInvariant().Contains(_assemblySearchTerm.ToLowerInvariant()))
                {
                    continue;
                }

                bool previous = _assemblySelections[assemblyName];
                bool current = EditorGUILayout.ToggleLeft(
                    new GUIContent(assemblyName), previous);

                // If changed
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
            int generatorCount = 0;

            // For each selected assembly
            foreach (var assemblyEntry in _assemblySelections.Where(kv => kv.Value))
            {
                string assemblyName = assemblyEntry.Key;
                try
                {
                    // Find the assembly by name
                    Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == assemblyName);

                    if (assembly != null)
                    {
                        // Find all types implementing IEasyCodeGenerator
                        var generators = assembly.GetTypes()
                            .Where(t => t.GetInterfaces().Contains(typeof(IEasyCodeGenerator)) && !t.IsAbstract)
                            .ToList();

                        if (generators.Count > 0)
                        {
                            foreach (var generatorType in generators)
                            {
                                try
                                {
                                    var generator = (IEasyCodeGenerator)Activator.CreateInstance(generatorType);
                                    generator.Execute();
                                    generatorCount++;
                                    Debug.Log($"Executed generator: {generatorType.Name}");
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Error executing generator {generatorType.Name}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing assembly {assemblyName}: {ex.Message}");
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