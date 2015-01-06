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
		var watcher = Watcher.Observe ();
		
		watcher.onCreated.AddListener (asset => {
			Debug.Log ("<color=yellow>[AssetsWatcherExample]</color> Created asset '" + asset.Name + "' of type " + asset.Type);
		});
	}
}