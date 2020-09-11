using System.Collections;
using UnityEditor;

/**
 * TreeNode.cs
 * Author: Luke Holland (http://lukeholland.me/)
 */

namespace TreeView {

	public class AssetTree 
	{
		
		private TreeNode<AssetData> _root;

		public AssetTree()
		{
			_root = new TreeNode<AssetData>(null);
		}

		public TreeNode<AssetData> Root { get { return _root; }}

		public void Clear()
		{
			_root.Clear();
		}
		

		public void ConfigurePackages()
		{
			var packageRoot = _root[1];
			for (int i = 0; i < packageRoot.Count; i++)
			{
				if (packageRoot[i].CheckType == Amount.Mixed)
				{
					packageRoot[i].Checked = true;
				}

				SetAllReadOnly(packageRoot[i]);
				packageRoot[i].ReadOnly = false;
			}
			
		}
		

		
		
		private void SetAllReadOnly(TreeNode<AssetData> node)
		{
			node.ReadOnly = true;
			for (int i = 0; i < node.Count; i++)
			{
				SetAllReadOnly(node[i]);
			}
		}
		
		public void AddAsset(string guid)
		{
			if(string.IsNullOrEmpty(guid)) return;

			TreeNode<AssetData> node = _root;

			string assetPath = AssetDatabase.GUIDToAssetPath(guid);
			int startIndex = 0, length = assetPath.Length;
			while(startIndex<length)
			{
				int endIndex = assetPath.IndexOf('/',startIndex);
				int subLength = endIndex==-1 ? length-startIndex : endIndex-startIndex;
				string directory = assetPath.Substring(startIndex,subLength);

				AssetData pathNode = new AssetData(endIndex==-1 ? guid : null,directory,assetPath.Substring(0,endIndex==-1 ? length : endIndex),node.Level==0);

				TreeNode<AssetData> child = node.FindInChildren(pathNode);
				if(child==null) child = node.AddChild(pathNode, guid);

				node = child;
				startIndex += subLength+1;
			}
		}

	}

	public class AssetData : ITreeIMGUIData
	{

		public readonly string guid;
		public readonly string path;
		public readonly string fullPath;
		public bool isExpanded { get; set; }

		public AssetData(string guid, string path, string fullPath, bool isExpanded)
		{
			this.guid = guid;
			this.path = path;
			this.fullPath = fullPath;
			this.isExpanded = isExpanded;
		}

		public override string ToString ()
		{
			return path;
		}

		public override int GetHashCode ()
		{
			return path.GetHashCode()+10;
		}

		public override bool Equals (object obj)
		{
			AssetData node = obj as AssetData;
			return node!=null && node.path==path;
		}

		public bool Equals(AssetData node)
		{
			return node.path==path;
		}

	}

}