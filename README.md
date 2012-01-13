AssetsWatcher
=============

AssetsWatcher is a Unity Editor utility that provides event handling in response to Unity asset file changes.

Create a new watcher:

1. Create a new class file in Unity's Editor folder. This class does not require a base class, you don't need to inherit from either MonoBehavior or Editor.
2. Add the attribute InitializeOnLoad to the class.
3. Create a static constructor.
4. Instantiate an AssetsWatcher within the static constructor.
5. Add delegate methods for one or more of the watcher's Events:
	- OnCreated
	- OnDeleted
	- OnModified
	- OnMoved
	- OnRenamed

Example implementation:

	[InitializeOnLoad]
	public static class ExampleWatcher
	{
		AssetsWatcher watcher = new AssetsWatcher ();
		
		watcher.OnModified += delegate(AssetFileInfo asset) {
			Debug.Log ("Changed asset '" + asset.Name + "' of type " + asset.Type);
		};
	}

An AssetWatcher that is created without parameters will scan the root of the Assets folder for all asset creation, deletion, and modification.

	AssetsWatcher watcher = new AssetsWatcher ();

If you would like more precise control, you may specify a path to watch and an asset type to match. This will watch the Assets/Graphics/GUI folder for new, removed, or changed texture files:

	AssetsWatcher watcher = new AssetsWatcher ("Graphics/GUI", UnityAssetType.Texture);

If you would like to include subdirectories, you may change the IncludeSubdirectories property after instantiating the watcher. This can be very slow if scanning deeply nested folders, so use sparingly.

	watcher.IncludeSubdirectories = true;

File change events will provide detail about the event through an AssetFileInfo object. The OnMoved and OnRenamed events produce AssetFileInfo for both the original file state and the new file state.

See the included "ExampleWatcher" for a full implementation.