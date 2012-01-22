// Copyright (c) 2012 Michael Stevenson <michael@theboxfort.com>, The Box Fort LLC
// This code is distributed under the MIT license

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;


/// <summary>
/// Raise events for Unity asset file changes.
/// </summary>
/// <remarks>
/// Inspired by Kevin Heeney's custom FileSystemWatcher.
/// 
/// Added the undocumented attribute <code>[InitializeOnLoad]</code> to this class
/// to call the static constructor as soon as the Unity project loads.
/// </remarks>
//[InitializeOnLoad]
public class AssetsWatcher : AssetPostprocessor {
//	
//	public delegate void FileEventHandler (AssetFileInfo asset);
//	public delegate void FileMovedHandler (AssetFileInfo assetBefore, AssetFileInfo assetAfter);
//	
//	#region Events
//	
//	
//	// FIXME use generic events which are triggered based on a given asset type
//	
//	// FIXME watch specific directories, and specific file types
//	
//	
//	/// <summary>
//	/// Occurs when an asset is first created.
//	/// </summary>
//	public static event FileEventHandler OnCreated;
//	/// <summary>
//	/// Occurs when an asset is deleted or is moved out of scope.
//	/// </summary>
//	public static event FileEventHandler OnDeleted;
//	/// <summary>
//	/// Occurs when the content of an asset is modified.
//	/// </summary>
//	public static event FileEventHandler OnModified;
//	/// <summary>
//	/// Occurs when an asset is renamed in-place.
//	/// </summary>
//	public static event FileMovedHandler OnRenamed;
//	/// <summary>
//	/// Occurs when an asset is moved to a new location within scope.
//	/// </summary>
//	public static event FileMovedHandler OnMoved;
//	
//	#endregion
//	
//	
//	#region Fields
//	
//	private static string[] allAssetPaths;
//	
//	#endregion
//	
//	
//	#region Properties
//	
//	public UnityAssetType filter = UnityAssetType.All;
//	
//	#endregion
//	
//	
//	
//	#region Methods
//	
//	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
//	{
//		foreach (string a in importedAssets) {
//			// If doesn't exist in our database, it was created
//			// Otherwise it was modified
//		}
//		
//		foreach (string a in deletedAssets) {
//			if (OnDeleted != null)
//				OnDeleted (new AssetFileInfo (a));
//		}
//		
//		for (int i = 0; i < movedAssets.Length; i++) {
//			// If it is at the same path, it was renamed
//			// Otherwise it was moved
//		}
//		
//		// Update asset paths cache
//		allAssetPaths = AssetDatabase.GetAllAssetPaths ();
//	}
//	
//	
//	/// <summary>
//	/// Initialize the AssetsWatcher.
//	/// </summary>
//	static AssetsWatcher ()
//	{
//		// Cache asset paths
//		allAssetPaths = AssetDatabase.GetAllAssetPaths ();
//	}
//	
//	
//	private static AssetFileInfo[] GetAssets (UnityAssetType filter)
//	{
//		string[] paths = AssetDatabase.GetAllAssetPaths ();
//		return paths.Select (p => new AssetFileInfo (p)).ToArray ();
//	}
//	
//	#endregion
	
}
