using System.Collections.Generic;
using UnityEngine;

namespace com.omidizadi.EasyCodeGen
{
    [CreateAssetMenu(fileName = "EasyCodeGenSettings", menuName = "Easy Code Gen/Settings")]
    public class EasyCodeGenSettings : ScriptableObject
    {
        [SerializeField] private List<string> _includedAssemblies = new List<string>();
        
        public List<string> IncludedAssemblies => _includedAssemblies;

        private static EasyCodeGenSettings _instance;

        public static EasyCodeGenSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<EasyCodeGenSettings>("EasyCodeGenSettings");
                    
                    // Create settings asset if it doesn't exist
                    if (_instance == null)
                    {
                        _instance = CreateInstance<EasyCodeGenSettings>();
                        #if UNITY_EDITOR
                        // Create resources folder if it doesn't exist
                        if (!System.IO.Directory.Exists("Assets/Resources"))
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
