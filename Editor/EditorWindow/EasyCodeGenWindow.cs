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
        private readonly Dictionary<string, Dictionary<string, bool>> _generatorSelections = new();
        private readonly Dictionary<string, List<Type>> _assemblyGenerators = new();
        private readonly List<string> _assembliesWithGenerators = new();
        private bool initialized;
        private GUIStyle sectionStyle;
        private GUIStyle categoryStyle;
        private string searchTerm = "";
        private Dictionary<string, bool> _foldoutStates = new();

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

            categoryStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                margin = new RectOffset(0, 0, 4, 2)
            };

            RefreshGeneratorList();
            initialized = true;
        }

        private void RefreshGeneratorList()
        {
            _assemblySelections.Clear();
            _generatorSelections.Clear();
            _assemblyGenerators.Clear();
            _assembliesWithGenerators.Clear();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .OrderBy(a => a.GetName().Name);

            foreach (var assembly in assemblies)
            {
                var assemblyName = assembly.GetName().Name;

                // Find generators in this assembly
                var generators = assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Contains(typeof(IEasyCodeGenerator)) && !t.IsAbstract)
                    .ToList();

                // Only add assemblies that have generators
                if (generators.Count > 0)
                {
                    _assembliesWithGenerators.Add(assemblyName);
                    _assemblyGenerators[assemblyName] = generators;

                    // Initialize assembly selection
                    bool isAssemblyIncluded = EasyCodeGenSettings.Instance.IsAssemblyIncluded(assemblyName);
                    _assemblySelections[assemblyName] = isAssemblyIncluded;

                    // Initialize generator selections
                    _generatorSelections[assemblyName] = new Dictionary<string, bool>();

                    foreach (var generator in generators)
                    {
                        var generatorKey = $"{assemblyName}.{generator.FullName}";
                        bool isSelected = isAssemblyIncluded; // Default to the assembly's selection state
                        _generatorSelections[assemblyName][generator.FullName] = isSelected;
                    }

                    // Initialize foldout state (default to open)
                    _foldoutStates.TryAdd(assemblyName, true);
                }
            }
        }

        private void OnGUI()
        {
            if (!initialized)
            {
                Initialize();
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Code Generator Selection", sectionStyle);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All", GUILayout.Width(100)))
            {
                SelectAll();
            }

            if (GUILayout.Button("Deselect All", GUILayout.Width(100)))
            {
                DeselectAll();
            }

            if (GUILayout.Button("Refresh List", GUILayout.Width(100)))
            {
                RefreshGeneratorList();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Select code generators to execute. You can select entire assemblies or individual generators.",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            var newSearchTerm = EditorGUILayout.TextField(searchTerm);
            if (newSearchTerm != searchTerm)
            {
                searchTerm = newSearchTerm;
                scrollPosition = Vector2.zero;
            }

            if (GUILayout.Button("X", GUILayout.Width(20)) && !string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = "";
                scrollPosition = Vector2.zero;
            }

            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Display hierarchical list of assemblies and their generators
            foreach (var assemblyName in _assembliesWithGenerators)
            {
                // Skip if doesn't match search
                if (!string.IsNullOrEmpty(searchTerm) &&
                    !assemblyName.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()) &&
                    !_assemblyGenerators[assemblyName]
                        .Any(g => g.Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant())))
                {
                    continue;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();

                var content = new GUIContent(assemblyName);
                var rect = GUILayoutUtility.GetRect(content, categoryStyle);

                var foldoutRect = new Rect(rect.x, rect.y, 15, rect.height);
                bool newFoldout = EditorGUI.Foldout(foldoutRect, _foldoutStates[assemblyName], GUIContent.none);

                if (!newFoldout.Equals(_foldoutStates[assemblyName]))
                {
                    _foldoutStates[assemblyName] = newFoldout;
                }

                var toggleRect = new Rect(rect.x + 15, rect.y, rect.width - 15, rect.height);
                EditorGUI.BeginChangeCheck();
                _assemblySelections[assemblyName] = EditorGUI.ToggleLeft(toggleRect, content,
                    _assemblySelections[assemblyName], categoryStyle);

                if (EditorGUI.EndChangeCheck())
                {
                    foreach (var generator in _assemblyGenerators[assemblyName])
                    {
                        _generatorSelections[assemblyName][generator.FullName] = _assemblySelections[assemblyName];
                    }

                    if (_assemblySelections[assemblyName])
                    {
                        EasyCodeGenSettings.Instance.AddAssembly(assemblyName);
                    }
                    else
                    {
                        EasyCodeGenSettings.Instance.RemoveAssembly(assemblyName);
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (_foldoutStates[assemblyName])
                {
                    EditorGUI.indentLevel++;

                    foreach (var generator in _assemblyGenerators[assemblyName])
                    {
                        if (!string.IsNullOrEmpty(searchTerm) &&
                            !generator.Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()) &&
                            !assemblyName.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                        {
                            continue;
                        }

                        EditorGUI.BeginChangeCheck();

                        _generatorSelections[assemblyName][generator.FullName] = EditorGUILayout.ToggleLeft(
                            new GUIContent(generator.Name), _generatorSelections[assemblyName][generator.FullName]);

                        if (EditorGUI.EndChangeCheck())
                        {
                            UpdateAssemblySelectionState(assemblyName);
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(10);

            if (GUILayout.Button(new GUIContent("Generate Code", "Generate code using the selected generators"),
                    GUILayout.Height(30)))
            {
                GenerateCode();
            }

            EditorGUILayout.EndVertical();
        }

        private void UpdateAssemblySelectionState(string assemblyName)
        {
            bool allSelected = _assemblyGenerators[assemblyName]
                .All(g => _generatorSelections[assemblyName][g.FullName]);

            bool anySelected = _assemblyGenerators[assemblyName]
                .Any(g => _generatorSelections[assemblyName][g.FullName]);

            // Update assembly selection state
            _assemblySelections[assemblyName] = allSelected;

            // Update settings
            if (anySelected)
            {
                EasyCodeGenSettings.Instance.AddAssembly(assemblyName);
            }
            else
            {
                EasyCodeGenSettings.Instance.RemoveAssembly(assemblyName);
            }
        }

        private void SelectAll()
        {
            foreach (var assemblyName in _assembliesWithGenerators)
            {
                _assemblySelections[assemblyName] = true;
                EasyCodeGenSettings.Instance.AddAssembly(assemblyName);

                foreach (var generator in _assemblyGenerators[assemblyName])
                {
                    _generatorSelections[assemblyName][generator.FullName] = true;
                }
            }
        }

        private void DeselectAll()
        {
            foreach (var assemblyName in _assembliesWithGenerators)
            {
                _assemblySelections[assemblyName] = false;

                foreach (var generator in _assemblyGenerators[assemblyName])
                {
                    _generatorSelections[assemblyName][generator.FullName] = false;
                }
            }

            EasyCodeGenSettings.Instance.ClearAssemblies();
        }

        private void GenerateCode()
        {
            var executedGenerators = new List<string>();

            foreach (var assemblyName in _assembliesWithGenerators)
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == assemblyName);

                if (assembly != null)
                {
                    foreach (var generatorType in _assemblyGenerators[assemblyName])
                    {
                        if (_generatorSelections[assemblyName][generatorType.FullName])
                        {
                            var generator = (IEasyCodeGenerator)Activator.CreateInstance(generatorType);
                            generator.Execute();
                            executedGenerators.Add(generatorType.Name);
                            Debug.Log($"Executed generator: {generatorType.Name}");
                        }
                    }
                }
            }

            int generatorCount = executedGenerators.Count;
            if (generatorCount > 0)
            {
                EditorUtility.DisplayDialog("Code Generation Complete",
                    $"Successfully executed {generatorCount} code generator{(generatorCount > 1 ? "s" : "")}:\n\n" +
                    string.Join("\n", executedGenerators),
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("No Generators Executed",
                    "No code generators were selected. Please select at least one generator to execute.",
                    "OK");
            }
        }
    }
}