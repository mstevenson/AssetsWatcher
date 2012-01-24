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
/// The undocumented attribute <code>[InitializeOnLoad]</code>
/// call a static constructor when a Unity project is loaded.
/// </remarks>
[InitializeOnLoad]
public sealed class AssetsWatcher : AssetPostprocessor
{
	private static string[] allAssets;
	private static List<Watcher> watchers;
	
	
	/// <summary>
	/// Initialize the AssetsWatcher when a project is loaded.
	/// </summary>
	static AssetsWatcher ()
	{
		// Cache asset paths
		allAssets = AssetDatabase.GetAllAssetPaths ();
		watchers = new List<Watcher> ();
		
		var a = Watch ("Qwer", UnityAssetType.All, true);
		a.OnCreated += delegate(AssetFileInfo asset) {
			Debug.Log ("asdf");
		};
	}
	
	
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPaths)
	{
		string[] created = importedAssets.Except (allAssets).ToArray ();
		string[] modified = importedAssets.Except (created).ToArray ();
		string[] renamed =
			(from current in movedAssets
			join last in movedFromPaths
				on Path.GetDirectoryName (current) equals Path.GetDirectoryName (last)
			select current).ToArray ();
		string[] moved = movedAssets.Except (renamed).ToArray ();
		
		// Dispatch asset events to available watchers
		foreach (Watcher w in watchers) {
			w.Created (created);
			w.Modified (modified);
//			w.Renamed (renamed);
//			w.Moved (moved);
			w.Deleted (deletedAssets);
		}
		
		// Update asset paths cache
		allAssets = AssetDatabase.GetAllAssetPaths ();
	}
	
	
	
	public static Watcher Watch (string path)
	{
		return Watch (path, UnityAssetType.All, true);
	}
	
	public static Watcher Watch (string path, bool useSubdirectories)
	{
		return Watch (path, UnityAssetType.All, useSubdirectories);
	}
	
	public static Watcher Watch (UnityAssetType assetType)
	{
		return Watch ("", assetType, true);
	}
	
	public static Watcher Watch (string path, UnityAssetType assetType, bool useSubdirectories)
	{
		Watcher w = new Watcher (path, assetType, useSubdirectories);
		watchers.Add (w);
		return w;
	}
	
	
	public static void UnWatch (Watcher watcher)
	{
		watchers.Remove (watcher);
	}
}


public class Watcher
{
	public delegate void FileEventHandler (AssetFileInfo asset);

	public delegate void FileMovedHandler (AssetFileInfo assetBefore,AssetFileInfo assetAfter);
		
	/// <summary>
	/// Occurs when an asset is first created.
	/// </summary>
	public event FileEventHandler OnCreated;
	/// <summary>
	/// Occurs when an asset is deleted or is moved out of scope.
	/// </summary>
	public event FileEventHandler OnDeleted;
	/// <summary>
	/// Occurs when the content of an asset is modified.
	/// </summary>
	public event FileEventHandler OnModified;
	/// <summary>
	/// Occurs when an asset is renamed in-place.
	/// </summary>
	public event FileMovedHandler OnRenamed;
	/// <summary>
	/// Occurs when an asset is moved to a new location within scope.
	/// </summary>
	public event FileMovedHandler OnMoved;
		
	public readonly string basePath;
	public readonly UnityAssetType assetType;
	public readonly bool useSubdirectories;
		
	public Watcher (string path, UnityAssetType assetType, bool useSubdirectories)
	{
		this.basePath = Path.Combine ("Assets", path);
		this.assetType = assetType;
		this.useSubdirectories = useSubdirectories;
	}
		
	~Watcher ()
	{
		AssetsWatcher.UnWatch (this);
	}
		
	internal void Created (string[] paths)
	{
		InvokeEventForPaths (paths, OnCreated);
	}
	
	internal void Deleted (string[] paths)
	{
		InvokeEventForPaths (paths, OnDeleted);
	}
		
	internal void Modified (string[] paths)
	{
		InvokeEventForPaths (paths, OnDeleted);
	}
		
//	internal void Renamed (string[] paths)
//	{
//		if (OnRenamed == null)
//			return;
//		foreach (var p in paths) {
//			if (IsValidPath (p)) {
//				OnRenamed (new AssetFileInfo (p));
//			}
//		}
//	}
//		
//	internal void Moved (string[] paths)
//	{
//		if (OnMoved == null)
//			return;
//		foreach (var p in paths) {
//			if (IsValidPath (p)) {
//				OnMoved (new AssetFileInfo (p));
//			}
//		}
//	}
	
	private void InvokeEventForPaths (string[] paths, FileEventHandler e)
	{
		if (e == null)
			return;
		foreach (var p in paths) {
			if (IsValidPath (p)) {
				AssetFileInfo asset = new AssetFileInfo (p);
				// FIXME this should test a bit flag enum
				if (asset.Type == assetType) {
					e (asset);
				}
			}
		}
	}
	
	/// <summary>
	/// Determines whether the specified assetPath is valid given the current path constraints.
	/// </summary>
	private bool IsValidPath (string assetPath)
	{
		if (useSubdirectories)
			return assetPath.StartsWith (this.basePath);
		else
			return Path.GetDirectoryName (assetPath) == this.basePath;
	}
}
