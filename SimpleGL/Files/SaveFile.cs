using SimpleGL.Util;
using System.IO.Compression;

namespace SimpleGL.Files;
public sealed class SaveFile : IDisposable, IEquatable<SaveFile?> {
    public string Name => Path.GetFileNameWithoutExtension(FilePath);

    private string FilePath { get; }

    /// <summary>
    /// Flag indicating whether the resource file was loaded.
    /// </summary>
    private bool WasLoaded { get; set; }
    /// <summary>
    /// The zip archive containing all assets.
    /// </summary>
    private ZipArchive? FileArchive { get; set; }

    private bool disposedValue;

    public SaveFile(string filePath) {
        FilePath = filePath;

        WasLoaded = false;
    }

    internal void Load() {
        if (WasLoaded)
            throw new InvalidOperationException("Save file was already loaded.");
        WasLoaded = true;

        MemoryStream ms = new MemoryStream();
        FileStream fs;
        if (File.Exists(FilePath)) {
            Log.WriteLine($"Loading save file {Name}");
            fs = new FileStream(FilePath, FileMode.Open);
        } else {
            Log.WriteLine($"Creating save file {Name}");
            fs = new FileStream(FilePath, FileMode.CreateNew);
        }

        fs.CopyTo(ms);
        ms.Position = 0;
        fs.Dispose();

        FileArchive = new ZipArchive(ms, ZipArchiveMode.Read);
    }

    internal void Unload() {
        if (!WasLoaded)
            return;

        Dispose();
    }

    public bool FileExists(string fileName) {
        if (!WasLoaded)
            throw new InvalidOperationException("Save file was not loaded.");

        return FileArchive!.GetEntry(fileName) != null;
    }

    public void ReadFileStream(string fileName, Action<Stream> readAction) {
        if (!WasLoaded)
            throw new InvalidOperationException("Save file was not loaded.");

        ZipArchiveEntry? entry = FileArchive!.GetEntry(fileName);

        if (entry == null)
            throw new InvalidOperationException($"Save file entry {fileName} does not exist.");

        using (Stream stream = entry.Open()) {
            readAction(stream);
        }
    }

    public void WriteToFileStream(string fileName, Action<Stream> writeAction) {
        if (!WasLoaded)
            throw new InvalidOperationException("Save file was not loaded.");

        ZipArchiveEntry? entry = FileArchive!.GetEntry(fileName);

        if (entry == null)
            entry = FileArchive.CreateEntry(fileName);

        using (Stream stream = entry.Open()) {
            writeAction(stream);
        }
    }

    private void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // TODO: dispose managed state (managed objects)
                FileArchive?.Dispose();
                FileArchive = null;
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~SaveFile()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public override bool Equals(object? obj) {
        return Equals(obj as SaveFile);
    }

    public bool Equals(SaveFile? other) {
        return other is not null &&
               Name == other.Name;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Name);
    }

    public static bool operator ==(SaveFile? left, SaveFile? right) => EqualityComparer<SaveFile>.Default.Equals(left, right);
    public static bool operator !=(SaveFile? left, SaveFile? right) => !(left == right);
}
