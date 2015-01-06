// Copyright (c) 2012-2015 Michael Stevenson <michael@mstevenson.net>
// This code is distributed under the MIT license

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetsWatcher
{
	/// <summary>
	/// Raise events for Unity asset file changes.
	/// </summary>
	/// <remarks>
	/// The undocumented attribute <code>[InitializeOnLoad]</code>
	/// call a static constructor when a Unity project is loaded.
	/// </remarks>
	[InitializeOnLoad]
	public sealed class WatcherPostprocessor : AssetPostprocessor
	{
		static string[] allAssets;
		static List<Watcher> watchers;
		
		/// <summary>
		/// Initialize the AssetsWatcher when a project is loaded.
		/// </summary>
		static WatcherPostprocessor ()
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
}

