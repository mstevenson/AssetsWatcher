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
		Watcher watcher = WatcherPostprocessor.Watch ();
		
		watcher.OnCreated += delegate(AssetFileInfo asset) {
			Debug.Log ("Created asset '" + asset.Name + "' of type " + asset.Type);
		};
	}
}