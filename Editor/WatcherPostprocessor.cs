// Copyright (c) 2012-2015 Michael Stevenson <michael@mstevenson.net>
// This code is distributed under the MIT license

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetsWatcher
{
	public sealed class WatcherPostprocessor : AssetPostprocessor
	{
		static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPaths)
		{
			string[] created = importedAssets.Except (Watcher.allAssets).ToArray ();
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
			foreach (Watcher w in Watcher.allWatchers) {
				w.InvokeEventForPaths (created, w.onAssetCreated);
				w.InvokeEventForPaths (deletedAssets, w.onAssetDeleted);
				w.InvokeEventForPaths (modified, w.onAssetModified);
				w.InvokeMovedEventForPaths (renamed, w.onAssetRenamed);
				w.InvokeMovedEventForPaths (moved, w.onAssetMoved);
			}
			
			// Update asset paths cache
			Watcher.allAssets = AssetDatabase.GetAllAssetPaths ();
		}
	}
}

