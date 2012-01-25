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
	}
	
	
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPaths)
	{
		string[] created = importedAssets.Except (allAssets).ToArray ();
		string[] modified = importedAssets.Except (created).ToArray ();
		
		Dictionary<string, string> allMoved = new Dictionary<string, string> ();
		for (int i = 0; i < movedAssets.Length; i++) {
			allMoved.Add (movedAssets [i], movedFromPaths [i]);
		}
		
		// Renamed to, renamed from
		Dictionary<string, string> renamed = 
			(from m in allMoved
			where (Path.GetDirectoryName (m.Key)) == (Path.GetDirectoryName (m.Value))
			select m).ToDictionary (p => p.Key, p => p.Value);
		
		Dictionary<string, string> moved = allMoved.Except (renamed).ToDictionary (p => p.Key, p => p.Value);
		
		// Dispatch asset events to available watchers
		foreach (Watcher w in watchers) {
			w.Created (created);
			w.Modified (modified);
			w.Renamed (renamed);
			w.Moved (moved);
			w.Deleted (deletedAssets);
		}
		
		// Update asset paths cache
		allAssets = AssetDatabase.GetAllAssetPaths ();
	}
	
	/// <summary>
	/// Watch for all asset changes in the project.
	/// </summary>
	public static Watcher Watch ()
	{
		return Watch ("", UnityAssetType.None, true);
	}
	
	/// <summary>
	/// Watch the specified path for asset changes.
	/// </summary>
	public static Watcher Watch (string path)
	{
		return Watch (path, UnityAssetType.None, false);
	}
	
	/// <summary>
	/// Watch the specified path for asset changes, optionally including subdirectories.
	/// </summary>
	public static Watcher Watch (string path, bool useSubdirectories)
	{
		return Watch (path, UnityAssetType.None, useSubdirectories);
	}
	
	/// <summary>
	/// Watch the specified path for the specified asset type.
	/// </summary>
	public static Watcher Watch (string path, UnityAssetType assetType)
	{
		Watcher w = new Watcher (path, assetType, false);
		watchers.Add (w);
		return w;
	}
	
	/// <summary>
	/// Watch for all asset changes in the project of the specified asset type.
	/// </summary>
	public static Watcher Watch (UnityAssetType assetType)
	{
		return Watch ("", assetType, true);
	}
	
	/// <summary>
	/// Watch the specified path for the specified asset type, optionally including subdirectories.
	/// </summary>
	public static Watcher Watch (string path, UnityAssetType assetType, bool useSubdirectories)
	{
		Watcher w = new Watcher (path, assetType, useSubdirectories);
		watchers.Add (w);
		return w;
	}
	
	/// <summary>
	/// Stop dispatching events for the specified watcher.
	/// </summary>
	public static void Unwatch (Watcher watcher)
	{
		watchers.Remove (watcher);
	}
}


public class Watcher
{
	public delegate void FileEventHandler (AssetFileInfo asset);
	public delegate void FileMovedHandler (AssetFileInfo assetBefore, AssetFileInfo assetAfter);
		
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
	public readonly UnityAssetType searchAssetTypes;
	public readonly bool useSubdirectories;
		
	public Watcher (string path, UnityAssetType assetType, bool useSubdirectories)
	{
		this.basePath = Path.Combine ("Assets", path);
		this.searchAssetTypes = assetType;
		this.useSubdirectories = useSubdirectories;
	}
		
	~Watcher ()
	{
		AssetsWatcher.Unwatch (this);
	}
	
	
	#region AssetsWatcher Friend Methods
	
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
		InvokeEventForPaths (paths, OnModified);
	}
		
	internal void Renamed (Dictionary<string, string> paths)
	{
		InvokeMovedEventForPaths (paths, OnRenamed);
	}
	
	internal void Moved (Dictionary<string, string> paths)
	{
		InvokeMovedEventForPaths (paths, OnMoved);
	}
	
	#endregion
	
	
	private void InvokeEventForPaths (string[] paths, FileEventHandler e)
	{
		if (e == null)
			return;
		foreach (var p in paths) {
			if (IsValidPath (p)) {
				AssetFileInfo asset = new AssetFileInfo (p);
				if (searchAssetTypes == UnityAssetType.None || (searchAssetTypes & asset.Type) == asset.Type) {
					e (asset);
				}
			}
		}
	}
	
	private void InvokeMovedEventForPaths (Dictionary<string, string> paths, FileMovedHandler e)
	{
		if (e == null)
			return;
		foreach (var p in paths) {
			if (IsValidPath (p.Key)) {
				// Path before, path after
				e (new AssetFileInfo (p.Value), new AssetFileInfo (p.Key));
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
