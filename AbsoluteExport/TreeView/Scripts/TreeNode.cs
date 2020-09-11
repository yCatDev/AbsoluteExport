using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * TreeNode.cs
 * Author: Luke Holland (http://lukeholland.me/)
 */

namespace TreeView {

	public enum Amount
	{
		Balanced,
		Mixed
	}
	
	public class TreeNode<T>
	{

		public delegate bool TraversalDataDelegate(T data);
		public delegate bool TraversalNodeDelegate(TreeNode<T> node);

		private readonly T _data;
		private readonly TreeNode<T> _parent;
		private readonly int _level;
		private readonly List<TreeNode<T>> _children;
		private readonly string _guid;
		private bool _checked;
		private Amount _checkType = Amount.Balanced;
		internal int c = 0;

		public TreeNode(T data)
		{
			_data = data;
			_children = new List<TreeNode<T>>();
			_level = 0;
		}

		public TreeNode(T data, TreeNode<T> parent, string guid) : this(data)
		{
			_parent = parent;
			_level = _parent!=null ? _parent.Level+1 : 0;
			_guid = guid;
		}

		public int Level => _level;
		public int Count => _children.Count;
		public bool IsRoot => _parent==null;
		public bool IsLeaf => _children.Count==0;
		public T Data => _data;
		public Amount CheckType
		{
			get { return _checkType; }
			set {_checkType = value; }
		}

		public bool Checked
		{
			get { return _checked; }
			set
			{
				if (_checked != value)
				{
					SetCheckStatusToChildren(value);
					//UpdateRoot();
				}

				_checked = value;
			}
		}
		
		public bool ReadOnly { get; set; }

		/*private void UpdateRoot()
		{
			var root = _parent;
			while (root._parent != null)
			{
				root = root._parent;
			}
			
			UpdateBranch(root);
		}

		internal void UpdateBranch(TreeNode<T> root)
		{
			root.c = 0;
			var mixed = false;
			for (int i = 0; i < root.Count; i++)
			{
				if (!root[i].IsLeaf) UpdateBranch(root[i]);
				if (root[i]._checked || (root[i].CheckType==Amount.Mixed&& !root[i]._checked)) root.c++;
				if (root[i]._checkType == Amount.Mixed) mixed = true;
			}

			root._checkType =
				(root.c == 0 || root.c == root.Count) /*&& (!mixed || root.IsLeaf)#1# ? Amount.Balanced : Amount.Mixed;
			//root._checked = c == root.Count;
			root._checked = root.c == root.Count;
		}*/

		public void Update()
		{
			UpdateBranch(this);
		}

		private static void UpdateBranch(TreeNode<T> node)
		{
			node.c = 0;
			bool hasMixed = false;
			for (int i = 0; i < node.Count; i++)
			{
				if (!node[i].IsLeaf) UpdateBranch(node[i]);
				
					if (node[i].CheckType == Amount.Mixed && !node[i].Checked)
					{
						node.c++;
						hasMixed = true;
					}
					else if (node[i].Checked)
					node.c++;

			}

			if (!hasMixed && node.c == 0 || node.c == node.Count)
				node.CheckType = Amount.Balanced;
			else node.CheckType = Amount.Mixed;

			node._checked = node.c == node.Count;
		}

		public string AssetGuid => _guid;

		private void SetCheckStatusToChildren(bool status)
		{
			foreach (var child in _children)
			{
				child._checked = status;
				child.CheckType = Amount.Balanced;
				child.SetCheckStatusToChildren(status);
			}
			
		}

		/*private void CheckParentStatus()
		{
			var count = _parent._children.Count(child => child.Checked);

			_parent._checked = count == _parent._children.Count;
			_parent._checkType =
				count == 0 ? Amount.None : (count == _parent._children.Count ? Amount.All : Amount.Mixed);
		}*/
		
		public TreeNode<T> Parent { get { return _parent; }}

		public TreeNode<T> this[int key]
		{
			get { return _children[key]; }
		}

		public void Clear()
		{
			_children.Clear();
		}

		public TreeNode<T> AddChild(T value, string guid)
		{
			TreeNode<T> node = new TreeNode<T>(value,this, guid);
			_children.Add(node);

			return node;
		}

		public void SetCheckState(bool setState)
		{
			_checked = setState;
		}
		
		public bool HasChild(T data)
		{
			return FindInChildren(data)!=null;
		}

		public TreeNode<T> FindInChildren(T data)
		{
			int i = 0, l = Count;
			for(; i<l; ++i){
				TreeNode<T> child = _children[i];
				if(child.Data.Equals(data)) return child;
			}

			return null;
		}

		public bool RemoveChild(TreeNode<T> node)
		{
			return _children.Remove(node);
		}

		public void Traverse(TraversalDataDelegate handler)
		{
			if(handler(_data)){
				int i = 0, l = Count;
				for(; i<l; ++i) _children[i].Traverse(handler);
			}
		}

		public void Traverse(TraversalNodeDelegate handler)
		{
			if(handler(this)){
				int i = 0, l = Count;
				for(; i<l; ++i) _children[i].Traverse(handler);
			}
		}

	}

}