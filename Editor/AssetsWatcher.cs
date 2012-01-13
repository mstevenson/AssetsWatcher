// Copyright (c) 2012 Michael Stevenson <michael@theboxfort.com>, The Box Fort LLC
// This code is distributed under the MIT license

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;


/// <summary>
/// Raise events for asset file changes within a specified folder.
/// </summary>
/// <remarks>
/// Inspired by Kevin Heeney's custom FileSystemWatcher.
/// </remarks>
public sealed class AssetsWatcher {
	
	public delegate void FileEventHandler (AssetFileInfo asset);
	public delegate void FileMovedHandler (AssetFileInfo assetBefore, AssetFileInfo assetAfter);
	
	#region Events
	
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
	
	#endregion
	
	
	#region Fields
	
	private UnityAssetType assetTypeFilter;
	private System.Timers.Timer timer;
	private AssetFileInfo[] assetFile;
	
	#endregion
	
	
	#region Properties
	
	private string _path;
	/// <summary>
	/// The watched path relative to the current project's Assets folder.
	/// </summary>
	public string Path {
		get { return _path; }
		set {
			_path = value;
			Init ();
		}
	}
	
	
	private int _timerInterval = 1000;
	/// <summary>
	/// The milliseconds to delay when scanning an assets directory. 1000 ms by default.
	/// </summary>
	public int TimerInterval {
		get { return _timerInterval; }
		set {
			_timerInterval = value;
			Init ();
		}
	}
	
	
	private bool _includeSubdirectories = false;
	/// <summary>
	/// Scan all subdirectories below the AssetsWatcher's main directory.
	/// </summary>
	/// <remarks>
	/// This may be prohibitively expensive when scanning a dense file structure.
	/// </remarks>
	public bool IncludeSubdirectories {
		get { return _includeSubdirectories; }
		set {
			_includeSubdirectories = value;
			Init ();
		}
	}
	
	
	private UnityAssetType _filter = UnityAssetType.All;
	public UnityAssetType Filter {
		get { return _filter; }
		set {
			_filter = value;
			Init ();
		}
	}
	
	#endregion
	
	
	#region Constructors
	
	/// <summary>
	/// Watch the root of the Assets folder for changes to a given asset type.
	/// </summary>
	public AssetsWatcher (UnityAssetType type) : this("", type) {}
	
	/// <summary>
	/// Watch the root of the Assets folder for changes to all asset types.
	/// </summary>
	public AssetsWatcher () : this("", UnityAssetType.All) {}
	
	/// <summary>
	/// Watch a given path relative to the Assets folder for changes to all asset types.
	/// </summary>
	public AssetsWatcher (string path) : this(path, UnityAssetType.All) {}
	
	/// <summary>
	/// Watch a given path relative to the Assets folder for changes to a given asset type.
	/// </summary>
	public AssetsWatcher (string path, UnityAssetType type)
	{
		// FIXME make path work on both Mac and Win
		
		timer = new System.Timers.Timer ();
		timer.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs e) {
			// We need to allow ScanForChanges to wait for EditorApplication.update since
			// it needs to make use of EditorApplication properties that are only available then
			EditorApplication.update += ScanForChanges;
		};
		_filter = type;
		_path = path == null ? "" : path;
		Init ();
	}

	#endregion
	
	
	#region Methods
	
	
	/// <summary>
	/// Initialize the AssetsWatcher.
	/// </summary>
	/// <remarks>
	/// \warning Init needs to run during EditorApplication.update otherwise EditorApplication
	/// properties will not be accessible.
	/// </remarks>
	private void Init ()
	{
		string path = System.IO.Path.Combine (Application.dataPath, Path);
		
		if (!Directory.Exists (path)) {
			Debug.LogError ("AssetsWatcher can't find folder " + Path);
			return;
		}
		
		// Grab initial filesystem state and start our timer to check for updates
		assetFile = GetAssets (path, IncludeSubdirectories, Filter);
		timer.Interval = TimerInterval;
		timer.Enabled = true;
	}
	
	
	/// <summary>
	/// Look for file and folder changes at regular intervals.
	/// </summary>
	/// <remarks>
	/// This runs only during the EditorApplication.update callback, so EditorApplication properties can safely be used.
	/// </remarks>
	private void ScanForChanges ()
	{
		EditorApplication.update -= ScanForChanges;
		
		string path = System.IO.Path.Combine (Application.dataPath, Path);
		try {
			AssetFileInfo[] currentData = GetAssets (path, IncludeSubdirectories, Filter);
			CheckAssetChanges (currentData, assetFile);
			assetFile = currentData;
		} catch (System.Exception ex) {
			// AssetsWatcher lost sight of the folder
			assetFile = null;
		}
	}
	
	
	private AssetFileInfo[] GetAssets (string path, bool includeSubDirectories, UnityAssetType typeFilter)
	{
		// Set up subdirectory inclusion
		SearchOption depth = includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
	
		// Set up filename filter
		string stringFilter = "*";
		if (typeFilter != UnityAssetType.All) {
			stringFilter = "";
			string[] extensions = AssetFileInfo.GetExtensionsForType (typeFilter);
			for (int i = 0; i < extensions.Length; i++) {
				stringFilter += @"*";
				stringFilter += extensions [i];
				if (i < extensions.Length - 1) {
					stringFilter += "|";
				}
			}
		}
		
		DirectoryInfo dir = new DirectoryInfo (path);
		if (!dir.Exists) {
			throw new System.IO.DirectoryNotFoundException ();
		}
		
		// Grab folders
		FileSystemInfo[] fsInfo = dir.GetDirectories (stringFilter, depth);
		
		if (typeFilter == UnityAssetType.All) {
			// Search all folders and files
			fsInfo = fsInfo.Concat (dir.GetFiles ("*.*", depth)).ToArray ();
		} else if (typeFilter != UnityAssetType.Folder) {
			// Search only files, no folders
			string[] extensions = AssetFileInfo.GetExtensionsForType (typeFilter);
			if (typeFilter == UnityAssetType.All) {
				// Find all Unity asset files
				fsInfo = dir.GetFiles ("*.*", depth);
			} else {
				// Find Unity asset file of a specific type
				fsInfo = dir.GetFiles ("*.*", depth).Where (f => extensions.Contains (f.Extension.ToLower ())).ToArray ();
			}
		}
		
		// Construct AssetFileInfo object for each FileSystemInfo object
		AssetFileInfo[] data = fsInfo.Select (f => new AssetFileInfo (f)).ToArray ();
		
		// Don't return anything that's not in Unity's asset database
		return data.Where (f => (f.Guid != null && f.Guid != "")).ToArray ();
	}
	
	
	
	/// <summary>
	/// Locate and manage filesystem changes.
	/// </summary>
	private void CheckAssetChanges (AssetFileInfo[] newAsset, AssetFileInfo[] oldAsset)
	{
		List<AssetFileInfo> newStaticFiles = newAsset.Intersect (oldAsset).OrderBy (f => f.Guid).ToList ();
		List<AssetFileInfo> oldStaticFiles = oldAsset.Intersect (newAsset).OrderBy (f => f.Guid).ToList ();
		
		List<AssetFileInfo> createdFiles = newAsset.Except (oldAsset).ToList ();
		List<AssetFileInfo> deletedFiles = oldAsset.Except (newAsset).ToList ();
		
		
		// Check for changes in existing files
		for (int i = 0; i < oldStaticFiles.Count; i++) {
			// Moved or renamed file
			if (oldStaticFiles[i].FullName != newStaticFiles[i].FullName) {
				// Renamed file
				if (oldStaticFiles[i].Name != newStaticFiles[i].Name) {
					if (OnRenamed != null)
						OnRenamed (oldStaticFiles[i], newStaticFiles[i]);
				}
				// Moved file
				if (System.IO.Path.GetDirectoryName (oldStaticFiles[i].FullName) != System.IO.Path.GetDirectoryName (newStaticFiles[i].FullName)) {
					if (OnMoved != null)
						OnMoved (oldStaticFiles[i], newStaticFiles[i]);
				}
			}
			
			// Modified file
			if (CheckFileModification (newStaticFiles[i], oldStaticFiles[i])) {
				OnModified (newStaticFiles[i]);
			}
		}
		
		// Created file
		foreach (AssetFileInfo f in createdFiles) {
			if (OnCreated != null)
				OnCreated (f);
		}
		
		// Deleted file
		foreach (AssetFileInfo f in deletedFiles) {
			if (OnDeleted != null)
				OnDeleted (f);
		}
	}
	
	
	private bool CheckFileModification (AssetFileInfo newAsset, AssetFileInfo oldAsset)
	{
		bool modified = false;
		
		if (newAsset.LastWriteTime != oldAsset.LastWriteTime)
			modified = true;
		else if (newAsset.Attributes != oldAsset.Attributes)
			modified = true;
		else if (newAsset.CreationTime != oldAsset.CreationTime)
			modified = true;
		else if (newAsset.Size != oldAsset.Size)
			modified = true;
		
		return modified && OnModified != null;
	}

	
	#endregion
	
}
