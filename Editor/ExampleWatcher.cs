using UnityEditor;
using UnityEngine;

// Super secret undocumented attribute that calls a static constructor when the Unity editor loads.
[InitializeOnLoad]
/// <summary>
/// Example AssetsWater usage.
/// </summary>
public static class ExampleWatcher
{	
	/// <summary>
	/// Static constructor called immediately when the project loads.
	/// </summary>
	static ExampleWatcher ()
	{	
		AssetsWatcher watcher = new AssetsWatcher ("", UnityAssetType.Material);
		Debug.Log ("Began watching 'Assets/" + watcher.Path + "' for asset changes");
		
		watcher.OnCreated += delegate(AssetFileInfo asset) {
			Debug.Log ("Created asset '" + asset.Name + "' of type " + asset.Type);
		};
		
		watcher.OnDeleted += delegate(AssetFileInfo asset) {
			Debug.Log ("Deleted asset '" + asset.Name + "' of type " + asset.Type);
		};
		
		watcher.OnModified += delegate(AssetFileInfo asset) {
			Debug.Log ("Changed asset '" + asset.Name + "' of type " + asset.Type);
		};
		
		watcher.OnMoved += delegate(AssetFileInfo assetBefore, AssetFileInfo assetAfter) {
			Debug.Log ("Moved asset '" + assetBefore.Name + "' to '" + assetAfter.Name + "'");
		};
		
		watcher.OnRenamed += delegate(AssetFileInfo assetBefore, AssetFileInfo assetAfter) {
			Debug.Log ("Renamed asset '" + assetBefore.Name + "' to '" + assetAfter.Name + "'");
		};
	}
	
}