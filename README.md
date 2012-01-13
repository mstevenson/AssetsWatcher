AssetsWatcher
=============

AssetsWatcher is a Unity Editor extension that provides event handling for Unity asset file changes. An AssetsWatcher instance will silently run in the Unity Editor, periodically scanning for recently created, deleted, modified, or moved files. Other editor scripts may register delegate methods to respond to these events.

To create a new watcher:

1. Create a new script in Unity's Editor folder. This script should not inherit from any base class.
3. Create a static constructor for the class.
2. Add the InitializeOnLoad attribute to the class (this calls the static constructor when the Unity project is loaded).
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
		static ExampleWatcher ()
		{
			AssetsWatcher watcher = new AssetsWatcher ();

			watcher.OnModified += delegate(AssetFileInfo asset) {
				Debug.Log ("Changed asset '" + asset.Name + "' of type " + asset.Type);
			};
		}
	}

Instantiating an AssetWatcher without arguments will watch the root of the Assets folder for all asset file creation, deletion, and modification.

	AssetsWatcher watcher = new AssetsWatcher ();

You may also specify a path to watch and an asset type to match. The following code will watch the Assets/Graphics/GUI folder for new, removed, or changed texture files:

	AssetsWatcher watcher = new AssetsWatcher ("Graphics/GUI", UnityAssetType.Texture);

If you would like to include subdirectories, you may change the IncludeSubdirectories property after instantiating the watcher. This may be very slow depending on the complexity of the directory structure:

	watcher.IncludeSubdirectories = true;

AssetsWatcher will provide details about each event via an AssetFileInfo object. The OnMoved and OnRenamed events provide AssetFileInfo for both the original file state and the new file state.

See the included "ExampleWatcher" for a more complete implementation.