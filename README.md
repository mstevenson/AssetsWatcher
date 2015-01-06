AssetsWatcher
=============

AssetsWatcher is a Unity Editor extension that augments AssetPostprocessor by providing fine-grained event handling for changes to specific asset types in specific locations. Editor scripts may instantiate new Watchers to invoke events when a desired asset type is created, deleted, modified, renamed, or moved.

To create a new watcher:

1. Add a static constructor to any class inside an Editor folder.
2. Add the InitializeOnLoad attribute to the class. This will enable the Unity Editor to call the static constructor when the project is loaded.
3. Call Watcher.Observe from the static constructor, passing in a desired base path, asset type flags, and directory recursion flag. Keep a reference to the returned Watcher instance.
4. Add listeners to the Watcher's UnityEvents:
	- onAssetCreated
	- onAssetDeleted
	- onAssetModified
	- onAssetMoved
	- onAssetRenamed

Example implementation:
	
	[InitializeOnLoad]
	public static class AssetsWatcherExample
	{
		static AssetsWatcherExample ()
		{
			Watcher watcher = Watcher.Observe ();
			
			watcher.onAssetCreated.AddListener (asset => {
				Debug.Log ("Created asset '" + asset.Name + "' of type " + asset.Type);
			});
		}
	}

You may specify a path to watch and asset type flags to match. You may also specify whether or not to recursively search subdirectories below the given path. The following Watcher will respond to all texture and GUI Skin changes in Assets/Graphics/GUI, but will ignore assets in its subdirectories:

	Watcher watcher = Watcher.Observe ("Graphics/GUI", UnityAssetType.Texture | UnityAssetType.GUISkin, false);

Each Watcher will return details about an asset event via an AssetFileInfo object. The onAssetMoved and onAssetRenamed events provide AssetFileInfo for both the original asset state and the new asset state.

If you would like to disable a watcher, call watcher.Disable ().