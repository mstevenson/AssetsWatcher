AssetsWatcher
=============

AssetsWatcher is a Unity Editor extension that augments AssetPostprocessor by providing fine-grained event handling for file changes of specific asset types in specific locations. Editor scripts may instantiate new Watchers to dispatch events when a desired asset type is created, deleted, modified, renamed, or moved.

To create a new watcher:

1. Add a static constructor to any class inside an Editor folder.
2. Add the InitializeOnLoad attribute to the class. This will enable the Unity Editor to call the static constructor when the project is loaded.
3. Call Watcher.Observe from the static constructor, passing in a desired base path, asset type flags, and directory recursion flag. Keep a reference to the returned Watcher instance.
4. Add listeners to the Watcher's UnityEvents:
	- onCreated
	- onDeleted
	- onModified
	- onMoved
	- onRenamed

Example implementation:
	
	[InitializeOnLoad]
	public static class AssetsWatcherExample
	{
		static AssetsWatcherExample ()
		{
			Watcher watcher = Watcher.Observe ();
			
			watcher.onCreated.AddListener (asset => {
				Debug.Log ("Created asset '" + asset.Name + "' of type " + asset.Type);
			});
		}
	}

You may specify a path to watch and asset type flags to match. You may also specify whether or not to recursively search subdirectories below the given path. The following Watcher will respond to all texture and GUI Skin changes in Assets/Graphics/GUI, but will ignore files in its subdirectories:

	Watcher watcher = Watcher.Observe ("Graphics/GUI", UnityAssetType.Texture | UnityAssetType.GUISkin, false);

Each Watcher will return details about an asset event via an AssetFileInfo object. The OnMoved and OnRenamed events provide AssetFileInfo for both the original file state and the new file state.

If you would like to disable a watcher, call watcher.Disable ().