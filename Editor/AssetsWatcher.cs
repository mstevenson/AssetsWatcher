using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;


/// <summary>
/// Raise events for asset file changes within a specified folder.
/// </summary>
/// <remarks>
/// Based on Kevin Heeney's custom FileSystemWatcher.
/// It can be prohibitively expensive when scanning a dense file structure.
/// </remarks>
public sealed class AssetsWatcher {
	
	public delegate void FileEventHandler (AssetFileInfo asset);
	public delegate void FileMovedHandler (AssetFileInfo assetBefore, AssetFileInfo assetAfter);
	
	#region Events

	public event FileEventHandler OnCreated;
	public event FileEventHandler OnDeleted;
	public event FileEventHandler OnModified;
	public event FileMovedHandler OnRenamed;
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
	/// Path is relative to the Assets folder.
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
	
	public AssetsWatcher (UnityAssetType filter) : this("", filter)
	{
	}
	
	public AssetsWatcher () : this("", UnityAssetType.All)
	{
	}
	
	public AssetsWatcher (string path) : this(path, UnityAssetType.All)
	{
	}
	
	public AssetsWatcher (string path, UnityAssetType filter)
	{
		timer = new System.Timers.Timer ();
		timer.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs e) {
			// We need to allow ScanForChanges to wait for EditorApplication.update since
			// it needs to make use of EditorApplication properties that are only available then
			EditorApplication.update += ScanForChanges;
		};
		_filter = filter;
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
		Debug.Log(path);
		
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
	/// This runs only during EditorApplication.update, so EditorApplication properties can safely be used.
	/// </remarks>
	private void ScanForChanges ()
	{
		EditorApplication.update -= ScanForChanges;
		
		string path = System.IO.Path.Combine (Application.dataPath, Path);
		AssetFileInfo[] currentData = GetAssets (path, IncludeSubdirectories, Filter);
		CheckAssetChanges (currentData, assetFile);
		assetFile = currentData;
	}
	
	
	private AssetFileInfo[] GetAssets (string path, bool includeSubDirectories, UnityAssetType filter)
	{
		// Set up subdirectory inclusion
		SearchOption depth = includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
		
		// Set up filename filter
		string stringFilter = "*";
		if (filter != UnityAssetType.All) {
			stringFilter += AssetFileInfo.AssetExtensions[filter];
		}
		
		DirectoryInfo dir = new DirectoryInfo (path);
		
		// Grab directories
		FileSystemInfo[] fsInfo = dir.GetDirectories (stringFilter, depth);
		// If searching for more than just directories, search everything
		if (filter != UnityAssetType.Folder) {
			// Grab files, concatenate with directories
			fsInfo = dir.GetFiles (stringFilter, depth).Concat (fsInfo).ToArray ();
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
			
			// Check for file modifications
			CheckFileModification (newStaticFiles[i], oldStaticFiles[i]);
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
	
	
	private void CheckFileModification (AssetFileInfo newAsset, AssetFileInfo oldAsset)
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
		
		if (modified && OnModified != null) {
			OnModified (newAsset);
		}
	}

	
	#endregion
	
}
