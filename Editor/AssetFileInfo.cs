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
	All
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
	
	private static Dictionary<string, UnityAssetType> _assetTypes = new Dictionary<string, UnityAssetType>() {
		{ "", UnityAssetType.Folder },
		{ ".asset", UnityAssetType.Asset },
		{ ".unity", UnityAssetType.Scene },
		{ ".mat", UnityAssetType.Material },
		{ ".shader", UnityAssetType.Shader }
	};
	public static Dictionary<string, UnityAssetType> AssetTypes {
		get { return _assetTypes; }
	}
	
	private static Dictionary<UnityAssetType, string> _assetExtensions = new Dictionary<UnityAssetType, string>() {
		{ UnityAssetType.Folder, "" },
		{ UnityAssetType.Asset, ".asset" },
		{ UnityAssetType.Scene, ".unity" },
		{ UnityAssetType.Material, ".mat" },
		{ UnityAssetType.Shader, ".shader" }
	};
	public static Dictionary<UnityAssetType, string> AssetExtensions {
		get { return _assetExtensions; }
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
		if (AssetTypes.ContainsKey (f.Extension))
			Type = AssetTypes[f.Extension];
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
