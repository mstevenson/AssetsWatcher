// Copyright (c) 2012-2015 Michael Stevenson <michael@mstevenson.net>
// This code is distributed under the MIT license

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;

namespace AssetsWatcher
{
	public class Watcher
	{
		public class FileEvent : UnityEvent<AssetFileInfo> {}
		public class FileMoveEvent : UnityEvent<AssetFileInfo, AssetFileInfo> {} // AssetFileInfo before and after the move

		internal static string[] allAssets;
		internal static List<Watcher> allWatchers;

		/// <summary>
		/// Occurs when an asset is first created.
		/// </summary>
		public FileEvent onCreated = new FileEvent ();
		/// <summary>
		/// Occurs when an asset is deleted or is moved out of scope.
		/// </summary>
		public FileEvent onDeleted = new FileEvent ();
		/// <summary>
		/// Occurs when the content of an asset is modified.
		/// </summary>
		public FileEvent onModified = new FileEvent ();
		/// <summary>
		/// Occurs when an asset is renamed in-place.
		/// </summary>
		public FileMoveEvent onRenamed = new FileMoveEvent ();
		/// <summary>
		/// Occurs when an asset is moved to a new location within scope.
		/// </summary>
		public FileMoveEvent onMoved = new FileMoveEvent ();
		
		public readonly string basePath;
		public readonly UnityAssetType searchAssetTypes;
		public readonly bool useSubdirectories;

		/// <summary>
		/// Initialize the AssetsWatcher when a project is loaded.
		/// </summary>
		static Watcher ()
		{
			// Cache asset paths
			allAssets = AssetDatabase.GetAllAssetPaths ();
			allWatchers = new List<Watcher> ();
		}

		public Watcher (string path, UnityAssetType assetType, bool useSubdirectories)
		{
			this.basePath = Path.Combine ("Assets", path);
			this.searchAssetTypes = assetType;
			this.useSubdirectories = useSubdirectories;
		}
		
		~Watcher ()
		{
			Watcher.RemoveObserver (this);
		}
		
		
		#region Internal Event Handlers
		
		internal void Created (string[] paths)
		{
			InvokeEventForPaths (paths, onCreated);
		}
		
		internal void Deleted (string[] paths)
		{
			InvokeEventForPaths (paths, onDeleted);
		}
		
		internal void Modified (string[] paths)
		{
			InvokeEventForPaths (paths, onModified);
		}
		
		internal void Renamed (Dictionary<string, string> paths)
		{
			InvokeMovedEventForPaths (paths, onRenamed);
		}
		
		internal void Moved (Dictionary<string, string> paths)
		{
			InvokeMovedEventForPaths (paths, onMoved);
		}
		
		#endregion
		
		
		void InvokeEventForPaths (string[] paths, FileEvent e)
		{
			if (e == null)
				return;
			foreach (var p in paths) {
				if (IsValidPath (p)) {
					AssetFileInfo asset = new AssetFileInfo (p);
					if (searchAssetTypes == UnityAssetType.None || (searchAssetTypes & asset.Type) == asset.Type) {
						e.Invoke (asset);
					}
				}
			}
		}
		
		void InvokeMovedEventForPaths (Dictionary<string, string> paths, FileMoveEvent e)
		{
			if (e == null)
				return;
			foreach (var p in paths) {
				bool beforePathValid = IsValidPath (p.Value);
				bool afterPathValid = IsValidPath (p.Key);
				if (beforePathValid || afterPathValid) {
					var before = beforePathValid ? new AssetFileInfo (p.Value) : null;
					var after = afterPathValid ? new AssetFileInfo (p.Key) : null;
					e.Invoke (before, after);
				}
			}
		}
		
		/// <summary>
		/// Determines whether the specified assetPath is valid given the current path constraints.
		/// </summary>
		bool IsValidPath (string assetPath)
		{
			if (useSubdirectories)
				return assetPath.StartsWith (this.basePath);
			else
				return Path.GetDirectoryName (assetPath) == this.basePath;
		}


		#region API

		/// <summary>
		/// Watch for all asset changes in the project.
		/// </summary>
		public static Watcher Observe ()
		{
			return Observe ("", UnityAssetType.None, true);
		}
		
		/// <summary>
		/// Watch the specified path for asset changes.
		/// </summary>
		public static Watcher Observe (string path)
		{
			return Observe (path, UnityAssetType.None, false);
		}
		
		/// <summary>
		/// Watch the specified path for asset changes, optionally including subdirectories.
		/// </summary>
		public static Watcher Observe (string path, bool useSubdirectories)
		{
			return Observe (path, UnityAssetType.None, useSubdirectories);
		}
		
		/// <summary>
		/// Watch the specified path for the specified asset type.
		/// </summary>
		public static Watcher Observe (string path, UnityAssetType assetType)
		{
			Watcher w = new Watcher (path, assetType, false);
			allWatchers.Add (w);
			return w;
		}
		
		/// <summary>
		/// Watch for all asset changes in the project of the specified asset type.
		/// </summary>
		public static Watcher Observe (UnityAssetType assetType)
		{
			return Observe ("", assetType, true);
		}
		
		/// <summary>
		/// Watch the specified path for the specified asset type, optionally including subdirectories.
		/// </summary>
		public static Watcher Observe (string path, UnityAssetType assetType, bool useSubdirectories)
		{
			Watcher w = new Watcher (path, assetType, useSubdirectories);
			allWatchers.Add (w);
			return w;
		}
		
		/// <summary>
		/// Stop dispatching events for the specified watcher.
		/// </summary>
		public static void RemoveObserver (Watcher watcher)
		{
			allWatchers.Remove (watcher);
		}

		#endregion
	}
}
