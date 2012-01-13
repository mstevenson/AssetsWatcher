using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
/// <summary>
/// Example AssetsWater usage.
/// </summary>
/// <remarks>
/// Add the undocumented attribute <code>[InitializeOnLoad]</code> to this class
/// to call the static constructor as soon as the Unity editor loads.
/// </remarks>
public static class ExampleWatcher
{	
	/// <summary>
	/// Static constructor called immediately when the project loads.
	/// </summary>
	static ExampleWatcher ()
	{	
		// Watch for materials
		UnityAssetType typeToWatch = UnityAssetType.All;
		
		// Watch the Assets/ root for changes
		AssetsWatcher watcher = new AssetsWatcher ("", typeToWatch);
		
		Debug.Log ("Began watching 'Assets/" + watcher.Path + "' for changes to assets of type " + typeToWatch);
		
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