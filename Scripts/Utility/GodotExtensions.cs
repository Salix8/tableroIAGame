#nullable enable
using System;
using System.Collections.Generic;
using Godot;

namespace Game;

public static class GodotExtensions
{
	public static SceneTree? TryGetSceneTree()
	{
		return Engine.GetMainLoop() as SceneTree;
	}
	public static T? TryGetParentOfType<T>(this Node node) where T : Node
	{
		Node parent = node.GetParent();
		if (parent is T tParent)
		{
			return tParent;
		}
		return null;
	}
	public static T InstantiateUnderAs<T>(this PackedScene scene, Node parent) where T : Node
	{
		var instance = scene.Instantiate<T>();
		parent.AddChild(instance);
		return instance;
	}
	public static IEnumerable<T> GetChildrenOfType<T>(this Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is T tChild)
				yield return tChild;
		}
	}
	public static IEnumerable<T> GetAllChildrenOfType<T>(this Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is T tChild)
				yield return tChild;

			foreach (T grandChild in child.GetAllChildrenOfType<T>())
				yield return grandChild;
		}
	}
	public static T GetRandomElement<T>(this IList<T> list)
	{
		if (list == null || list.Count == 0)
			throw new InvalidOperationException("Cannot get random element from an empty or null list.");

		int index = (int)(GD.Randi() % list.Count);
		return list[index];
	}

	public static Window? GetSceneRoot() => Engine.GetMainLoop() is SceneTree tree ? tree.Root : null;
}