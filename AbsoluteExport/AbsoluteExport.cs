using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TreeView;
using UnityEditor;
using UnityEngine;


namespace AbsoluteExport
{
    public class AbsoluteExport : EditorWindow
    {

        private string _packageName;
        private string _mainFilePath;
        private static UnityEngine.Object _mainFileObject;
        
        private Vector2 _scrollPos;

        private List<string> _assets;
        
        private static AbsoluteExport _instance;
        private static Texture _logoTexture; 
        
        private AssetTree _assetTree;
        private AssetTreeIMGUI _assetTreeGUI;

        [MenuItem("Assets/Absolute Export/Export complete package")]
        private static void ShowWindow()
        {
            _mainFileObject = Selection.activeObject;
            if (_mainFileObject == null)
            {
                Debug.LogError("[AbsoluteExport] No asset selected, returning...");
                return;
            }
            var file = AssetDatabase.GetAssetPath(_mainFileObject);

            _instance = GetWindow<AbsoluteExport>();
            _instance.titleContent = new GUIContent("AbsoluteExport");
            _instance.minSize = new Vector2(600, 700);
            _logoTexture = AssetDatabase.LoadAssetAtPath<Texture>(GetSelfPath()+"/Data/logo.png");
            _instance.Init(file);
        }

        private void Init(string path)
        {
            
            _assetTree = new AssetTree();
            _assetTreeGUI = new AssetTreeIMGUI(_assetTree.Root);
            
            
            EditorUtility.DisplayProgressBar("AbsoluteExport is exporting", "Reading AssetDatabase", 100);
            
            var tmp = AssetDatabase.FindAssets("");
            foreach (var asset in tmp)
            {
                _assetTree.AddAsset(asset);
            }

            EditorUtility.DisplayProgressBar("AbsoluteExport is exporting", "Collecting assets", 100);
            //Step one: getting hidden Project Settings and other stuff

            //_commonAssets = Directory.GetFiles("ProjectSettings", "*.asset").ToList();

            _mainFilePath = path;
            _packageName = Path.GetFileNameWithoutExtension(_mainFilePath);

            //Step two: getting assets and dependencies

            _assets = new List<string> {};
            _assets.AddRange(CollectDependencies());
            EditorUtility.ClearProgressBar();

            //Step tree: process cginc (due to SHADERCOMPILE bug)

            EditorUtility.DisplayProgressBar("AbsoluteExport is exporting", "Searching for used cginc files", 100);
            var shaders = Utils.GetShaders(_assets);
            var shadersCode = shaders.Select(Utils.LoadAssetAsText).ToList();

            var finder = new ShaderFinder(shadersCode);
            _assets.AddRange(finder.Process());
            EditorUtility.ClearProgressBar();
            
            var staticFinder = new StaticFinder(Utils.GetScripts(_assets));
            staticFinder.Process();
            
            ProcessAssetTreeBranch(_assetTree.Root);
            _assetTree.ConfigurePackages();
            
        }


        private void ProcessAssetTreeBranch(TreeNode<AssetData> root)
        {
            for (int i = 0; i < root.Count; i++)
            {
                if (!root[i].IsLeaf) ProcessAssetTreeBranch(root[i]);
                if (_assets.Contains(root[i].Data.fullPath))
                    root[i].Checked = true;
            }
        }
        
        
        private void OnGUI()
        {
            EditorGUI.DrawTextureTransparent(new Rect((position.width/2)-(_logoTexture.width/2), 0, _logoTexture.width, _logoTexture.height), _logoTexture, ScaleMode.ScaleToFit);
            EditorGUILayout.Space(_logoTexture.height);
            EditorGUILayout.Separator();
            _packageName = EditorGUILayout.TextField("Package file name:", _packageName);
            EditorGUILayout.Separator();
            

            EditorGUILayout.LabelField("Package assets", EditorStyles.boldLabel);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            
            _assetTreeGUI.DrawTreeLayout();

            EditorGUILayout.EndScrollView();
            

            EditorGUILayout.Separator();
            

            if (GUILayout.Button("Export", GUILayout.Height(30)))
            {
                Export();
            }
            EditorGUILayout.LabelField("yCatDev - Version 1.1", EditorStyles.miniBoldLabel);
        }

        private IEnumerable<string> CollectDependencies()
        {
            var result = new List<string>();

            try
            {
                if (!Utils.IsFile(_mainFilePath))
                {
                    var truePath = _mainFilePath.Replace("Assets", Application.dataPath);
                    result.AddRange(Directory.EnumerateFiles(truePath, "*.*", SearchOption.AllDirectories)
                        .Select(Utils.GetAssetLocalPath));
                }
                else result.AddRange(AssetDatabase.GetDependencies(_mainFilePath, true));

                EditorUtility.DisplayProgressBar("AbsoluteExport is exporting", "Additional deep dependencies search",
                    100);
                var deepSearch = new DeepDependenciesFinder(result);
                deepSearch.StartSearching();
                result.AddRange(deepSearch.GetResults());
                EditorUtility.ClearProgressBar();
            }
            catch (Exception ex)
            {
                Debug.LogError("[AbsoluteExport][IO] "+ex);
            }

            return result;
        }

        private void Export()
        {
            EditorUtility.DisplayProgressBar("AbsoluteExport", $"AbsoluteExport is exporting your '{_packageName}' package", 100);
            var projectContent = new List<string>();
            _assetTree.Root.Traverse((TreeNode<AssetData> node) =>
            {
                if (node.Data == null) return true;
                if (node.Checked)
                    projectContent.Add(node.Data.fullPath);
                return node.Count > 0;
            });
            
            if (projectContent.Count == 0)
                Debug.LogWarning("[AbsoluteExport] No project settings was selected");
            

            var path = EditorUtility.SaveFilePanel("Save package", Application.dataPath, _packageName,
                "unitypackage");

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[AbsoluteExport] Exporting canceled by user");
                EditorUtility.ClearProgressBar();
                return;
            }
            
            
            AssetDatabase.ExportPackage(projectContent.ToArray(), path,
                ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeLibraryAssets);
            
            EditorUtility.ClearProgressBar();
        }
        

        private static string GetSelfPath() =>
            Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(_instance)));
    }
}