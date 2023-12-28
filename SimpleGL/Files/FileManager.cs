namespace SimpleGL.Files;
public static class FileManager {

    /// <summary>
    /// The extension used for resource files.
    /// </summary>
    private const string CONFIG_FILE_EXTENSION = "cfg";
    /// <summary>
    /// The extension used for resource files.
    /// </summary>
    private const string RESOURCE_FILE_EXTENSION = "dat";
    /// <summary>
    /// The extension used for resource files.
    /// </summary>
    private const string SAVE_FILE_EXTENSION = "sf";

    /// <summary>
    /// Gets the directory where saved files are stored.
    /// </summary>
    private const string CONFIG_DIRECTORY = "Config";

    /// <summary>
    /// Gets the directory where resource files are stored.
    /// </summary>
    private const string RESOURCE_DIRECTORY = "Resources";

    /// <summary>
    /// Gets the directory where save files are stored.
    /// </summary>
    private const string SAVES_DIRECTORY = "Saves";

    public static SaveFile? LoadedSaveFile { get; set; }
    public static bool IsSaveFileLoaded => LoadedSaveFile != null;

    private static ManualResetEvent SaveFileLock { get; } = new ManualResetEvent(true);

    public static event Action<string> OnSaveFileLoaded = delegate { };
    public static event Action<string> OnSaveFileUnloading = delegate { };

    internal static void Initialize() {
        LoadedSaveFile = null;
    }

    internal static void Unload() {
        if (IsSaveFileLoaded)
            UnloadSaveFile();
    }

    public static void LoadSaveFile(string saveName) {
        if (IsSaveFileLoaded)
            UnloadSaveFile();

        SaveFileLock.WaitOne();
        SaveFileLock.Reset();

        string saveFilePath = GetSaveFilePath(saveName);
        LoadedSaveFile = new SaveFile(saveFilePath);
        LoadedSaveFile.Load();

        OnSaveFileLoaded(LoadedSaveFile!.Name);
        SaveFileLock.Set();
    }

    public static void UnloadSaveFile() {
        if (!IsSaveFileLoaded)
            return;

        SaveFileLock.WaitOne();
        SaveFileLock.Reset();

        OnSaveFileUnloading(LoadedSaveFile!.Name);

        LoadedSaveFile!.Unload();
        LoadedSaveFile = null;

        SaveFileLock.Set();
    }

    /// <summary>
    /// Retrieves the full path to a config file, and ensures the save directory exists.
    /// </summary>
    /// <param name="fileName">The name of the config file.</param>
    /// <returns>The full path to the config file.</returns>
    public static string GetConfigFilePath(string fileName) {
        // Check if the save directory exists, and create it if it doesn't
        if (!Directory.Exists(CONFIG_DIRECTORY))
            Directory.CreateDirectory(CONFIG_DIRECTORY);

        // Return the full path to the save file within the save directory
        return Path.Combine(CONFIG_DIRECTORY, $"{fileName}.{CONFIG_FILE_EXTENSION}");
    }

    /// <summary>
    /// Retrieves the full path to a resource file.
    /// </summary>
    /// <param name="paths">An array of subdirectories and/or the filename to construct the path. Useful for nested directories.</param>
    /// <returns>The full path to the resource file.</returns>
    public static string GetResourceFilePath(string fileName) {
        // Combine the execution directory of the assembly with the resource directory and provided subpaths
        return Path.Combine(RESOURCE_DIRECTORY, $"{fileName}.{RESOURCE_FILE_EXTENSION}");
    }

    /// <summary>
    /// Retrieves the full path to a save file, and ensures the save directory exists.
    /// </summary>
    /// <param name="fileName">The name of the save file.</param>
    /// <returns>The full path to the save file.</returns>
    public static string GetSaveFilePath(string fileName) {
        // Check if the save directory exists, and create it if it doesn't
        if (!Directory.Exists(SAVES_DIRECTORY))
            Directory.CreateDirectory(SAVES_DIRECTORY);

        // Return the full path to the save file within the save directory
        return Path.Combine(SAVES_DIRECTORY, $"{fileName}.{SAVE_FILE_EXTENSION}");
    }
}
