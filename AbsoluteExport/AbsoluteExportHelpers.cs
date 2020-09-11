using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AbsoluteExport
{
   
      public static class Utils
      {
         public static bool IsFile(string path) => File.Exists(path);
         public static string GetAssetLocalPath(string path) =>
            path.Substring(path.LastIndexOf("/Assets/", StringComparison.Ordinal)).Remove(0, 1);
         
         public static string[] LoadAssetAsText(string assetpath)
         {
            var truePath = assetpath.Replace("Assets", Application.dataPath);
            var sr = new StreamReader(truePath);
            return sr.ReadToEnd().Split('\n');
         }
        
         public static IEnumerable<string> GetShaders(IEnumerable<string> assets) =>
            (from asset in assets where asset.Contains(".shader") select asset);
         
         public static IEnumerable<string> GetScripts(IEnumerable<string> assets) =>
            (from asset in assets where asset.Contains(".cs") select asset);
      }
      
}