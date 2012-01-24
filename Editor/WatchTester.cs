using UnityEngine;
using UnityEditor;
using System.Collections;

[InitializeOnLoad]
public class WatchTester : Editor {

	static WatchTester ()
	{
		Watcher wat = AssetsWatcher.Watch (UnityAssetType.Cubemap | UnityAssetType.Flare);
		
		wat.OnCreated += delegate(AssetFileInfo asset) {
			Debug.Log ("asdf");
		};
	}
}
