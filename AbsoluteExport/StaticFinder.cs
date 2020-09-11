using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AbsoluteExport
{
    public class StaticFinder
    {
        private readonly List<string[]> _scripts;
        private readonly HashSet<string> _paths;
        private readonly  List<string> _markers = new List<string>()
        {
            "Shader.Find", "Resources", "AssetDatabase", "EditorUtility"    
        };

        private readonly StringBuilder _log;
        private bool _isStaticFoundedIn;

        public StaticFinder(IEnumerable<string> scriptPaths)
        {
            _scripts = new List<string[]>();
            _paths = new HashSet<string>();
            _log = new StringBuilder();

            foreach (var path in scriptPaths)
                _paths.Add(path);
            foreach (var path in _paths)
                _scripts.Add(Utils.LoadAssetAsText(path));
        }

        public void Process()
        {
            var l = _paths.ToList();
            for (var i = 0; i < _scripts.Count; i++)
            {
                var script = _scripts[i];
                FindStatic(script, l[i]);
            }
            if (_isStaticFoundedIn)
                Debug.LogWarning(_log.ToString());
        }

        private void FindStatic (string[] target, string filepath)
        {
            for (var i = 0; i < target.Length; i++)
            {
                var line = target[i];
                if (!Contains(line)) continue;
                if (!_isStaticFoundedIn)
                {
                    _isStaticFoundedIn = true;
                    _log.AppendLine(
                        "[AbsoluteExport] Possible static asset invocation found. Please check next scripts to prevent package corruption");
                }
                _log.AppendLine($"- Found in {Path.GetFileName(filepath)} on line {i}");
            }
        }

        public bool IsFoundedSomething() => _isStaticFoundedIn;
        
        private bool Contains(string line)
            => _markers.Any(line.Contains);

    }
}