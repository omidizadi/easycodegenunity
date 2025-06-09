using System.Collections.Generic;
using UnityEngine;

namespace com.omidizadi.EasyCodeGen
{
    [CreateAssetMenu(fileName = "EasyCodeGenSettings", menuName = "Easy Code Gen/Settings")]
    public class EasyCodeGenSettings : ScriptableObject
    {
        private readonly List<string> _includedAssemblies = new List<string>();

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
            return _includedAssemblies.Contains(assemblyName);
        }

        public void AddAssembly(string assemblyName)
        {
            if (!_includedAssemblies.Contains(assemblyName))
            {
                _includedAssemblies.Add(assemblyName);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        public void RemoveAssembly(string assemblyName)
        {
            if (_includedAssemblies.Contains(assemblyName))
            {
                _includedAssemblies.Remove(assemblyName);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        public void ClearAssemblies()
        {
            _includedAssemblies.Clear();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}