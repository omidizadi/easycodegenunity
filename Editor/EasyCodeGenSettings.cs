using System.Collections.Generic;
using UnityEngine;

namespace com.omidizadi.EasyCodeGen
{
    [CreateAssetMenu(fileName = "EasyCodeGenSettings", menuName = "Easy Code Gen/Settings")]
    public class EasyCodeGenSettings : ScriptableObject
    {
        [SerializeField] private List<string> includedAssemblies;

        private static EasyCodeGenSettings _instance;

        public static EasyCodeGenSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<EasyCodeGenSettings>("EasyCodeGenSettings");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<EasyCodeGenSettings>();
#if UNITY_EDITOR
                        if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
                        {
                            UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                        }

                        UnityEditor.AssetDatabase.CreateAsset(_instance, "Assets/Resources/EasyCodeGenSettings.asset");
                        UnityEditor.AssetDatabase.SaveAssets();
#endif
                    }
                }

                return _instance;
            }
        }

        public bool IsAssemblyIncluded(string assemblyName)
        {
            return includedAssemblies.Contains(assemblyName);
        }

        public void AddAssembly(string assemblyName)
        {
            if (!includedAssemblies.Contains(assemblyName))
            {
                includedAssemblies.Add(assemblyName);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        public void RemoveAssembly(string assemblyName)
        {
            if (includedAssemblies.Contains(assemblyName))
            {
                includedAssemblies.Remove(assemblyName);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        public void ClearAssemblies()
        {
            includedAssemblies.Clear();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}