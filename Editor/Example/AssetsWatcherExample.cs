// Copyright (c) 2015 Michael Stevenson <michael@mstevenson.net>
// This code is distributed under the MIT license

using UnityEngine;
using UnityEditor;
using System.Collections;
using AssetsWatcher;

[InitializeOnLoad]
public static class AssetsWatcherExample
{
	static AssetsWatcherExample ()
	{
		// Observe the entire assets folder for changes
		var watcher = Watcher.Observe ();
		
		watcher.onAssetCreated.AddListener (asset => {
			Debug.Log ("<color=yellow>[AssetsWatcherExample]</color> <color=cyan>Created</color> asset '" + asset.Name + "' of type " + asset.Type);
		});

		watcher.onAssetDeleted.AddListener (asset => {
			Debug.Log ("<color=yellow>[AssetsWatcherExample]</color> <color=red>Deleted</color> asset '" + asset.Name + "' of type " + asset.Type);
		});

		watcher.onAssetModified.AddListener (asset => {
			Debug.Log ("<color=yellow>[AssetsWatcherExample]</color> <color=orange>Modified</color> asset '" + asset.Name + "' of type " + asset.Type);
		});

		watcher.onAssetMoved.AddListener ((before, after) => {
			Debug.Log ("<color=yellow>[AssetsWatcherExample]</color> <color=blue>Moved</color> asset '" + before.Name + "' from '" + before.DirectoryName + "' to '" + after.DirectoryName + "'");
		});

		watcher.onAssetRenamed.AddListener ((before, after) => {
			Debug.Log ("<color=yellow>[AssetsWatcherExample]</color> <color=magenta>Renamed</color> asset from '" + before.Name + "' to '" + after.Name + "'");
		});
	}
}