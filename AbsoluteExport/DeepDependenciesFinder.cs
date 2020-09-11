using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace AbsoluteExport
{
    public class DeepDependenciesFinder
    {
        private readonly List<string> _guidContainingAssets;
        private readonly List<string> _allAssets;
        private readonly List<string> _results;


        public DeepDependenciesFinder(List<string> assets)
        {
            _allAssets = assets;
            _guidContainingAssets = new List<string>();
            _results = new List<string>();

            foreach (var asset in _allAssets)
            {
                var extension = Path.GetExtension(asset);
                if (extension != ".unity" && extension != ".prefab") continue;
                if (_guidContainingAssets.Contains(asset)) continue;

                _guidContainingAssets.Add(asset);
            }
        }

        public void StartSearching()
        {
            foreach (var asset in _guidContainingAssets)
            {
                var text = File.ReadAllText(asset);
                var lines = text.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("guid:"))
                    {

                        var path = AssetDatabase.GUIDToAssetPath(SelectGuid(line));
                        if (!_allAssets.Contains(path) && !_results.Contains(path))
                        {
                            _results.Add(path);
                        }
                    }
                }
            }
        }

        public IEnumerable<string> GetResults() => _results;

        private string SelectGuid(string line)
        {
            var start = line.IndexOf("guid: ", StringComparison.Ordinal) + 6;
            var result = "";
            for (var i = start; i < line.Length; i++)
            {
                if (!char.IsLetter(line[i]) && !char.IsDigit(line[i])) break;
                result += line[i];
            }

            return result;
        }

    }
}