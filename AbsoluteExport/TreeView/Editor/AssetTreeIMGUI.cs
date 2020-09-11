using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;

/**
 * TreeNode.cs
 * Author: Luke Holland (http://lukeholland.me/)
 */

namespace TreeView {
	
	public class AssetTreeIMGUI : TreeIMGUI<AssetData>
	{

		public AssetTreeIMGUI(TreeNode<AssetData> root) : base(root)
		{
			
		}

		protected override void OnDrawTreeNode(Rect rect, TreeNode<AssetData> node, bool selected, bool focus)
		{
			//GUIContent labelContent = new GUIContent($"{node.Data.path} - {node.CheckType.ToString()} {node.Checked} ({node.c})", AssetDatabase.GetCachedIcon(node.Data.fullPath));
			var labelContent = new GUIContent(node.Data.path, AssetDatabase.GetCachedIcon(node.Data.fullPath));

			if(!node.IsLeaf){
				node.Data.isExpanded = EditorGUI.Foldout(new Rect(rect.x-12,rect.y,12,rect.height),node.Data.isExpanded,GUIContent.none);
			}

			var style = node.CheckType == Amount.Mixed ? CreateMixedToggle() : EditorStyles.toggle;
			
			EditorGUI.BeginDisabledGroup(node.ReadOnly);
			node.Checked = EditorGUI.Toggle(rect, GUIContent.none, node.Checked, style);
			EditorGUI.EndDisabledGroup();
			
			rect.x += 15f;
			EditorGUI.LabelField(rect, labelContent, EditorStyles.label);
		}

		private static GUIStyle CreateMixedToggle()
		{
			var style = new GUIStyle("ToggleMixed");
			return style;
		}
		
	}

}