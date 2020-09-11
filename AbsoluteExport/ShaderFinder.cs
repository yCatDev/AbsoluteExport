using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AbsoluteExport
{
    public class ShaderFinder
    {
        private readonly HashSet<string> _foundedShaders;
        private readonly HashSet<string> _pathes;
        private readonly List<string[]> _shadersToProcess;
        private readonly string[] _assets;

        public ShaderFinder(IEnumerable<string[]> shaders)
        {
            _foundedShaders = new HashSet<string>();
            _pathes = new HashSet<string>();
            _shadersToProcess = new List<string[]>();

            _assets = AssetDatabase.GetAllAssetPaths();
            _shadersToProcess.AddRange(shaders);
        }

        public IEnumerable<string> Process()
        {
            foreach (var shader in _shadersToProcess)
            {
                var result = FindCginc(shader);
                foreach (var s in result)
                    _foundedShaders.Add(s);
            }

            FindAssets();
            return _pathes;
        }

        private IEnumerable<string> FindCginc(string[] target) =>
            (from line in target where line.Contains("#include")
                select Regex.Match(line, "(?<=\")[\\w]+(?!=\")").Value + ".cginc");

        private void FindAssets()
        {
            foreach (var asset in _assets)
            {
                foreach (var shader in _foundedShaders)
                {
                    if (asset.Contains(shader))
                    {
                        _pathes.Add(asset);
                    }
                }
            }
        }

    }
}