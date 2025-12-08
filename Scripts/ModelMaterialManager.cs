using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game;

[GlobalClass]
public partial class ModelMaterialManager : Node3D
{
	public enum MaterialLevel
	{
		Base,
		Extra
	}
	MeshInstance3D[] meshes;
	Dictionary<MaterialLevel, Material> overlays = new();
	Material overrideMat;

	public void Manage(Node3D scene)
	{
		meshes = scene.GetAllChildrenOfType<MeshInstance3D>().ToArray();
	}

	public void SetOverlay(Material mat, MaterialLevel level)
	{
		overlays[level] = mat;
		UpdateOverlays();
	}

	public void SetOverride(Material mat)
	{
		overrideMat = mat;
		UpdateOverrides();
	}

	void UpdateOverlays()
	{
		List<Material> mats = [];
		foreach (MaterialLevel level in Enum.GetValues<MaterialLevel>()){
			if (!overlays.TryGetValue(level, out Material overlay)) continue;
			if (overlay == null) continue;
			mats.Add(overlay);
		}

		if (mats.Count == 0){
			foreach (MeshInstance3D mesh in meshes){
				mesh.MaterialOverlay = null;
			}
			return;
		}

		foreach (Material mat in mats){
			mat.NextPass = null;
		}
		Material baseMat = mats[0];
		Material curMat = baseMat;
		for (int i = 1; i < mats.Count; i++){
			curMat.NextPass = mats[i];
			curMat = mats[i];
		}
		foreach (MeshInstance3D mesh in meshes){
			mesh.MaterialOverlay = baseMat;
		}
	}

	void UpdateOverrides()
	{
		foreach (MeshInstance3D mesh in meshes){
			mesh.MaterialOverride = overrideMat;
		}
	}
}