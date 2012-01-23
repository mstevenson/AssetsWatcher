// Copyright (c) 2012 Michael Stevenson <michael@theboxfort.com>, The Box Fort LLC
// This code is distributed under the MIT license
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

public enum UnityAssetType
{
	Folder,
	Asset,
	Scene,
	Material,
	Shader,
	Script,
	Model,
	Text,
	Texture,
	Audio,
	Video,
	Cubemap,
	Animation,
	Flare,
	GUISkin,
	PhysicMaterial,
	Font,
	Prefab,
	All, // includes folders
	None
}

public class AssetFileInfo
{
	public string Name { get; private set; }

	public string FullName { get; private set; }

	public string DirectoryName { get; private set; }

	public UnityAssetType Type { get; private set; }

	public string Guid { get; private set; }
	
	private static Dictionary<UnityAssetType, string[]> assetExtensions = new Dictionary<UnityAssetType, string[]> () {
		{ UnityAssetType.Folder, new string[] {""} },
		{ UnityAssetType.Asset, new string[] {".asset"} },
		{ UnityAssetType.Scene, new string[] {".unity"} },
		{ UnityAssetType.Material, new string[] {".mat"} },
		{ UnityAssetType.Shader, new string[] {".shader"} },
		{ UnityAssetType.Script, new string[] {".cs", ".js", ".boo"} },
		{ UnityAssetType.Model, new string[] {".ma", ".mb", ".fbx", ".max", ".jas", ".c4d", ".blend", ".lwo", ".skp", ".3ds", ".obj", ".dxf"} },
		{ UnityAssetType.Texture, new string[] {".psd", ".jpg", ".jpeg", ".png", ".exr", ".tif", ".tiff", ".gif", ".bmp", ".tga", ".iff", ".pict"} },
		{ UnityAssetType.Audio, new string[] {".wav", ".aif", ".aiff", ".mp3", ".ogg"} },
		{ UnityAssetType.Video, new string[] {".mov", ".avi", ".asf", ".mpg", ".mpeg", ".mp4"} },
		{ UnityAssetType.Text, new string[] {".txt", ".xml"} },
		{ UnityAssetType.Cubemap, new string[] {".cubemap"} },
		{ UnityAssetType.Animation, new string[] {".anim"} },
		{ UnityAssetType.GUISkin, new string[] {".guiskin"} },
		{ UnityAssetType.PhysicMaterial, new string[] {".physicMaterial"} },
		{ UnityAssetType.Flare, new string[] {".flare"} },
		{ UnityAssetType.Font, new string[] {".fontsettings"} },
		{ UnityAssetType.Prefab, new string[] {".prefab"} }
	};
	
	public AssetFileInfo (string path)
	{
		this.Name = Path.GetFileName (path);
		this.FullName = path;
		this.DirectoryName = Path.GetDirectoryName (path);
		this.Guid = AssetDatabase.AssetPathToGUID (path);
		this.Type = GetTypeForExtension (Path.GetExtension (path));
	}
	
	
	/// <summary>
	/// Return the path for this asset relative to the current project's Assets folder.
	/// </summary>
	public string AssetsRelativePath {
		get {
			string path = FullName;
			int length = Application.dataPath.Length - 6;
			if (path.Length <= length)
				return null;
			return path.Remove (0, length);
		}
	}
	
	/// <summary>
	/// Extension (with leading dot) for the given Unity asset type
	/// </summary>
	public static string[] GetExtensionsForType (UnityAssetType type)
	{
		string[] ext;
		try {
			ext = assetExtensions [type];
		} catch {
			return new string[0];
		}
		return ext;
	}
	
	public static UnityAssetType GetTypeForExtension (string extension)
	{
		if (extension == "")
			return UnityAssetType.Folder;
		foreach (var kvp in assetExtensions) {
			foreach (string s in kvp.Value) {
				if (s == extension) {
					return kvp.Key;
				}
			}
		}
		return UnityAssetType.None;
	}

	public override int GetHashCode ()
	{
		return Guid.GetHashCode ();
	}
	
	public override bool Equals (object obj)
	{
		if (obj is AssetFileInfo)
			return Guid == ((AssetFileInfo)obj).Guid;
		else
			return false;
	}
	
	public static bool operator == (AssetFileInfo x, AssetFileInfo y)
	{
		return System.Object.Equals (x, y);
	}
	
	public static bool operator != (AssetFileInfo x, AssetFileInfo y)
	{
		return !System.Object.Equals (x, y);
	}
	
}
