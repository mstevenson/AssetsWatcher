AssetsWatcher
=============

AssetsWatcher is a Unity Editor extension that augments AssetPostprocessor by providing fine-grained event handling for file changes of specific asset types in specific locations. Editor scripts may instantiate new Watchers to dispatch events when a desired asset type is created, deleted, modified, renamed, or moved.

To create a new watcher:

1. Add a static constructor to any Editor script.
2. Add the InitializeOnLoad attribute to the script (this calls the static constructor when the Unity project is loaded).
3. Call AssetsWatcher.Watch within the static constructor, passing in a desired base path and asset type. Store the returned Watcher object.
4. Add delegate methods to the Watcher's events:
	- OnCreated
	- OnDeleted
	- OnModified
	- OnMoved
	- OnRenamed

Example implementation:
	
	[InitializeOnLoad]
	public static class ExampleEditor : Editor
	{
		static ExampleEditor ()
		{
			Watcher allAssets = AssetsWatcher.Watch (UnityAssetType.All);
			
			allAssets.OnCreated += delegate(AssetFileInfo asset) {
				Debug.Log ("Created asset '" + asset.Name + "' of type " + asset.Type);
			};
		}
	}

You may specify which path to watch, which asset type to match, and whether or not to search subdirectories. The following Watcher will respond to any changes to texture files in Assets/Graphics/GUI or any of its subdirectories:

	Watcher watcher = AssetsWatcher.Watch ("Graphics/GUI", UnityAssetType.Texture, true);

Each Watcher will provide details about asset event via an AssetFileInfo object. The OnMoved and OnRenamed events provide AssetFileInfo for both the original file state and the new file state.

If you would like to disable a watcher, call AssetsWatcher.UnWatch (watcher)