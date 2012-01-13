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
	Model,
	Text,
	Texture,
	Audio,
	Video,
	All,
	None
}


public class AssetFileInfo
{
	public string Name { get; private set; }
	public string FullName { get; private set; }
	public string DirectoryName { get; private set; }
	public DateTime CreationTime { get; private set; }
	public DateTime LastWriteTime { get; private set; }
	public string Attributes { get; private set; }
	public long Size { get; private set; }
	public UnityAssetType Type { get; private set; }
	public string Guid { get; private set; }
	
	
	private static Dictionary<UnityAssetType, string[]> _assetExtensions = new Dictionary<UnityAssetType, string[]>() {
		{ UnityAssetType.Folder, new string[] {""} },
		{ UnityAssetType.Asset, new string[] {".asset"} },
		{ UnityAssetType.Scene, new string[] {".unity"} },
		{ UnityAssetType.Material, new string[] {".mat"} },
		{ UnityAssetType.Shader, new string[] {".shader"} },
		{ UnityAssetType.Model, new string[] {".ma", ".mb", ".fbx", ".max", ".jas", ".c4d", ".blend", ".lwo", ".skp", ".3ds", ".obj", ".dxf"} },
		{ UnityAssetType.Texture, new string[] {".psd", ".jpg", ".jpeg", ".png", ".exr", ".tif", ".tiff", ".gif", ".bmp", ".tga", ".iff", ".pict"} },
		{ UnityAssetType.Audio, new string[] {".wav", ".aif", ".aiff", ".mp3", ".ogg"} },
		{ UnityAssetType.Video, new string[] {".mov", ".avi", ".asf", ".mpg", ".mpeg", ".mp4"} },
		{ UnityAssetType.Text, new string[] {".txt", ".xml"} }
	};
	
	
	
	/// <summary>
	/// Extension (with leading dot) for the given Unity asset type
	/// </summary>
	public static string[] GetExtensionsForType (UnityAssetType type)
	{
		string[] ext;
		try {
			ext = _assetExtensions[type];
		} catch {
			return new string[0];
		}
		return ext;
	}
	
	
	public static UnityAssetType GetTypeForExtension (string extension)
	{
		foreach (var kvp in _assetExtensions) {
			foreach (string s in kvp.Value) {
				if (s == extension) {
					return kvp.Key;
				}
			}
		}
		return UnityAssetType.None;
	}
	
	
	
	public AssetFileInfo (FileSystemInfo f)
	{
		this.Name = f.Name;
		this.FullName = f.FullName;
		this.DirectoryName = (f is FileInfo) ? ((FileInfo)f).DirectoryName : ((DirectoryInfo)f).Parent.FullName;
		this.CreationTime = f.CreationTimeUtc;
		this.LastWriteTime = f.LastWriteTimeUtc;
		this.Attributes = f.Attributes.ToString ();
		this.Size = (f is FileInfo) ? Size : 0;
		// Warning: Guid can only be set during EditorApplication.update callback
		this.Guid = AssetDatabase.AssetPathToGUID (AssetsRelativePath);
		this.Type = GetTypeForExtension (f.Extension);
	}
	
	/// <summary>
	/// Return the path for this asset relative to the current project's Assets folder.
	/// </summary>
	public string AssetsRelativePath
	{
		get {
			string path = FullName;
			int length = Application.dataPath.Length - 6;
			if (path.Length <= length)
				return null;
			return path.Remove (0, length);
		}
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
		return System.Object.Equals(x, y);
	}
	
	public static bool operator != (AssetFileInfo x, AssetFileInfo y)
	{
		return !System.Object.Equals (x, y);
	}

}
