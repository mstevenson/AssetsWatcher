// Copyright (c) 2012-2015 Michael Stevenson <michael@mstevenson.net>
// This code is distributed under the MIT license

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace AssetsWatcher
{
	[System.Flags]
	public enum UnityAssetType
	{
		None = 0,
		Folder = 1,
		Asset = 1 << 1,
		Scene = 1 << 2,
		Material = 1 << 3,
		Shader = 1 << 4,
		Script = 1 << 5,
		Model = 1 << 6,
		Text = 1 << 7,
		Texture = 1 << 8,
		Audio = 1 << 9,
		Video = 1 << 10,
		Cubemap = 1 << 11,
		Animation = 1 << 12,
		LensFlare = 1 << 13,
		GUISkin = 1 << 14,
		PhysicMaterial = 1 << 15,
		Font = 1 << 16,
		Prefab = 1 << 17,
		RenderTexture = 1 << 18,
		ComputeShader = 1 << 19,
		AnimatorController = 1 << 20,
		AnimatorOverrideController = 1 << 21,
		AvatarMask = 1 << 22,
		Physics2DMaterial = 1 << 23,
	}
	
	public class AssetFileInfo
	{
		public string Name { get; private set; }
		
		public string FullName { get; private set; }
		
		public string DirectoryName { get; private set; }
		
		public UnityAssetType Type { get; private set; }
		
		public string Guid { get; private set; }
		
		static Dictionary<UnityAssetType, string[]> assetExtensions = new Dictionary<UnityAssetType, string[]> () {
			{ UnityAssetType.Folder, new string[] {""} },
			{ UnityAssetType.Asset, new string[] {".asset"} },
			{ UnityAssetType.Scene, new string[] {".unity"} },
			{ UnityAssetType.Material, new string[] {".mat"} },
			{ UnityAssetType.Shader, new string[] {".shader"} },
			{ UnityAssetType.Script, new string[] {".cs", ".js", ".boo"} },
			{ UnityAssetType.Model, new string[] {".ma", ".mb", ".fbx", ".dae", ".lxo", ".max", ".jas", ".c4d", ".blend", ".lwo", ".skp", ".3ds", ".obj", ".dxf"} },
			{ UnityAssetType.Texture, new string[] {".psd", ".jpg", ".jpeg", ".png", ".exr", ".tif", ".tiff", ".gif", ".bmp", ".tga", ".iff", ".pict"} },
			{ UnityAssetType.Audio, new string[] {".wav", ".aif", ".aiff", ".mp3", ".ogg", ".oga", ".mod", ".it", ".s3m", ".xm"} },
			{ UnityAssetType.Video, new string[] {".mov", ".avi", ".asf", ".mpg", ".mpeg", ".mp4", ".ogv"} },
			{ UnityAssetType.Text, new string[] {".txt", ".xml"} },
			{ UnityAssetType.Cubemap, new string[] {".cubemap"} },
			{ UnityAssetType.Animation, new string[] {".anim"} },
			{ UnityAssetType.GUISkin, new string[] {".guiskin"} },
			{ UnityAssetType.PhysicMaterial, new string[] {".physicMaterial"} },
			{ UnityAssetType.LensFlare, new string[] {".flare"} },
			{ UnityAssetType.Font, new string[] {".fontsettings"} },
			{ UnityAssetType.Prefab, new string[] {".prefab"} },
			{ UnityAssetType.RenderTexture, new string[] {".renderTexture"} },
			{ UnityAssetType.ComputeShader, new string[] {".compute"} },
			{ UnityAssetType.AnimatorController, new string[] {".controller"} },
			{ UnityAssetType.AnimatorOverrideController, new string[] {".overrideController"} },
			{ UnityAssetType.AvatarMask, new string[] {".mask"} },
			{ UnityAssetType.Physics2DMaterial, new string[] {".physicsMaterial2D"} },
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
}
