using OpenTK.Mathematics;
using SimpleGL.Audio;
using SimpleGL.Graphics;
using SimpleGL.Graphics.Textures;
using SimpleGL.Util;
using SixLabors.Fonts;
using StbImageSharp;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;

namespace SimpleGL.ResourceHandling;
/// <summary>
/// Class for one set of game resources. Doesn't cache anything.
/// </summary>
internal sealed class ResourceFile : IDisposable, IEquatable<ResourceFile?> {
    public string Name => Path.GetFileNameWithoutExtension(FilePath);

    /// <summary>
    /// The file path to the asset file.
    /// </summary>
    private string FilePath { get; }

    /// <summary>
    /// A mapping of color name to colors.
    /// </summary>
    private Dictionary<string, Color4> Colors { get; }

    /// <summary>
    /// Flag indicating whether the resource file was loaded.
    /// </summary>
    private bool WasLoaded { get; set; }
    /// <summary>
    /// The zip archive containing all assets.
    /// </summary>
    private ZipArchive? FileArchive { get; set; }

    private bool disposedValue;

    /// <summary>
    /// Constructor to load a resource file from disk.
    /// </summary>
    internal ResourceFile(string filePath) {
        FilePath = filePath;
        Colors = new Dictionary<string, Color4>();

        WasLoaded = false;
    }

    internal void Load() {
        if (WasLoaded)
            throw new InvalidOperationException("Resource file was already loaded.");

        Log.WriteLine($"Loading resource file {Name}");
        MemoryStream ms = new MemoryStream();
        using FileStream fs = new FileStream(FilePath, FileMode.Open);

        fs.CopyTo(ms);
        ms.Position = 0;

        FileArchive = new ZipArchive(ms, ZipArchiveMode.Read);

        ZipArchiveEntry? colorEntry = FileArchive.GetEntry("colors.json");
        if (colorEntry == null) {
            Log.WriteLine($"Resource file {FilePath} doesn't contain colors.");
            return;
        }

        StreamReader colorStreamReader = new StreamReader(colorEntry.Open());
        Dictionary<string, int[]>? colors = JsonSerializer.Deserialize<Dictionary<string, int[]>>(colorStreamReader.ReadToEnd());
        if (colors == null) {
            Log.WriteLine($"colors.json in resource file {FilePath} has a wrong format.");
            return;
        }

        foreach (KeyValuePair<string, int[]> entry in colors) {
            Colors[entry.Key] = new Color4((byte)entry.Value[0], (byte)entry.Value[1], (byte)entry.Value[2], (byte)entry.Value[3]);
        }

        WasLoaded = true;
    }

    internal void Unload() {
        Dispose();
    }

    /// <summary>
    /// Tries to get a color to the given key.
    /// </summary>
    /// <param name="key"></param>
    /// <exception cref="InvalidOperationException">Thrown if the resource file was not loaded.</exception>
    internal Color4? GetColor(string key) {
        if (!WasLoaded)
            throw new InvalidOperationException("Resource file was not loaded.");

        if (!Colors.ContainsKey(key)) {
            return null;
        }

        return Colors[key];
    }

    /// <summary>
    /// Tries to load a font from the zip archive.
    /// </summary>
    /// <param name="key"></param>
    /// <exception cref="InvalidOperationException">Thrown if the resource file was not loaded.</exception>
    public FontFamilyData? LoadFont(string key) {
        if (!WasLoaded)
            throw new InvalidOperationException("Resource file was not loaded.");

        bool isTTF = true;

        string path = $"Fonts/{key}.ttf";
        ZipArchiveEntry? zippedFont = FileArchive!.GetEntry(path);

        if (zippedFont == null) {
            isTTF = false;
            path = $"Fonts/{key}.otf";
            zippedFont = FileArchive!.GetEntry(path);
        }

        FontFamily fontFamily;
        if (zippedFont != null) {
            Stream fontStream = zippedFont.Open();
            MemoryStream ms = new MemoryStream();
            fontStream.CopyTo(ms);
            fontStream.Dispose();
            ms.Position = 0;

            FontCollection fontCollection = new FontCollection();
            fontFamily = fontCollection.Add(ms);
            ms.Dispose();

        } else if (SystemFonts.Collection.TryGet(key, CultureInfo.InvariantCulture, out fontFamily)) {
            // Do nothing
        } else {
            Log.WriteLine($"Font {key} doesn't exist in this resource file");
            return null;
        }

        return new FontFamilyData(fontFamily, isTTF);
    }

    /// <summary>
    /// Tries to load a texture from the zip archive.
    /// </summary>
    /// <param name="key"></param>
    /// <exception cref="InvalidOperationException">Thrown if the resource file was not loaded.</exception>
    public Texture2D? LoadTexture(string key) {
        if (!WasLoaded)
            throw new InvalidOperationException("Resource file was not loaded.");

        string path = $"Textures/{key}.png";
        ZipArchiveEntry? zippedTexture = FileArchive!.GetEntry(path);

        if (zippedTexture == null) {
            Log.WriteLine($"Texture {key} doesn't exist in this resource file");
            return null;
        }

        Stream textureStream = zippedTexture.Open();
        MemoryStream ms = new MemoryStream();
        textureStream.CopyTo(ms);
        textureStream.Dispose();
        ms.Position = 0;

        ImageResult image = ImageResult.FromStream(ms, ColorComponents.RedGreenBlueAlpha);
        ms.Dispose();

        Texture2D texture = GraphicsHelper.CreateTexture(key, image);

        // TODO add check if texture was loaded correctly
        /*if (texture.id == 0) {
            Log.WriteLine($"Failed to load texture {key} from {path}");
            return null;
        }*/
        return texture;
    }

    /// <summary>
    /// Tries to load a TextureAtlas from the zip archive.
    /// </summary>
    /// <param name="key"></param>
    /// <exception cref="InvalidOperationException">Thrown if the theme was not loaded.</exception>
    public TextureAtlas? LoadTextureAtlas(string key) {
        if (!WasLoaded)
            throw new InvalidOperationException("Theme was not loaded.");

        string texturePath = $"Textures/{key}.png";
        ZipArchiveEntry? zippedTexture = FileArchive!.GetEntry(texturePath);

        if (zippedTexture == null) {
            Log.WriteLine($"Texture {key} doesn't exist in this resource file");
            return null;
        }

        Stream textureStream = zippedTexture.Open();
        MemoryStream ms = new MemoryStream();
        textureStream.CopyTo(ms);
        textureStream.Dispose();
        ms.Position = 0;

        ImageResult image = ImageResult.FromStream(ms, ColorComponents.RedGreenBlueAlpha);
        ms.Dispose();


        string atlasDataPath = $"Textures/TextureAtlasData/{key}.json";
        ZipArchiveEntry? zippedAtlasData = FileArchive!.GetEntry(atlasDataPath);

        if (zippedAtlasData == null) {
            Debug.WriteLine($"TextureAtlasData {key} doesn't exist in this theme");
            return null;
        }

        using Stream atlasDataStream = zippedAtlasData.Open();
        Dictionary<string, string>? dict = JsonSerializer.Deserialize<Dictionary<string, string>>(atlasDataStream);

        if (dict == null)
            return null;

        Dictionary<string, Box2i> subTextures = new();
        foreach (KeyValuePair<string, string> item in dict) {
            string id = item.Key;
            string[] components = item.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (components.Length != 4) {
                Log.WriteLine($"TextureAtlasData {key} has an invalid format.", eLogType.Error);
                continue;
            }

            if (!int.TryParse(components[0], CultureInfo.InvariantCulture, out int x) ||
                !int.TryParse(components[1], CultureInfo.InvariantCulture, out int y) ||
                !int.TryParse(components[2], CultureInfo.InvariantCulture, out int w) ||
                !int.TryParse(components[3], CultureInfo.InvariantCulture, out int h)) {
                Log.WriteLine($"TextureAtlasData {key} has an invalid format.", eLogType.Error);
                continue;
            }

            subTextures[id] = new Box2i(x, y, x + w, y + h);
        }

        return GraphicsHelper.CreateTextureAtlas(key, image, subTextures);
    }

    /// <summary>
    /// Tries to load a sound from the zip archive.
    /// </summary>
    /// <param name="key"></param>
    /// <exception cref="InvalidOperationException">Thrown if the resource file was not loaded.</exception>
    public Sound? LoadSound(string key) {
        if (!WasLoaded)
            throw new InvalidOperationException("Resource file was not loaded.");

        string path = $"Sounds/{key}.wav";
        ZipArchiveEntry? zippedSound = FileArchive!.GetEntry(path);

        if (zippedSound == null) {
            Log.WriteLine($"Sound {key} doesn't exist in this resource file");
            return null;
        }

        Stream soundStream = zippedSound.Open();
        MemoryStream ms = new MemoryStream();
        soundStream.CopyTo(ms);
        soundStream.Dispose();
        ms.Position = 0;

        Sound sound = Sound.Create(ms);
        ms.Dispose();

        return sound;
    }

    /// <summary>
    /// Tries to load a music from the zip archive.
    /// </summary>
    /// <param name="key"></param>
    /// <exception cref="InvalidOperationException">Thrown if the resource file was not loaded.</exception>
    public Music? LoadMusic(string key) {
        if (!WasLoaded)
            throw new InvalidOperationException("Resource file was not loaded.");

        string path = $"Music/{key}.mp3";
        ZipArchiveEntry? zippedSound = FileArchive!.GetEntry(path);

        if (zippedSound == null) {
            Log.WriteLine($"Music {key} doesn't exist in this resource file");
            return null;
        }

        Stream musicStream = zippedSound.Open();
        MemoryStream ms = new MemoryStream();
        musicStream.CopyTo(ms);
        musicStream.Dispose();
        ms.Position = 0;

        Music music = Music.Create(ms);
        ms.Dispose();

        return music;
    }

    /// <summary>
    /// Tries to get a text to the given key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown if the resource file was not loaded.</exception>
    internal IReadOnlyDictionary<string, string>? LoadText(string key) {
        if (!WasLoaded)
            throw new InvalidOperationException("Resource file was not loaded.");

        string path = $"Texts/{key}.json";
        ZipArchiveEntry? zippedText = FileArchive!.GetEntry(path);

        if (zippedText == null) {
            Log.WriteLine($"Text {key} doesn't exist in this resource file");
            return null;
        }

        using Stream textStream = zippedText.Open();
        Dictionary<string, string>? dict = JsonSerializer.Deserialize<Dictionary<string, string>>(textStream);

        return dict;
    }

    /// <summary>
    /// Tries to get a shader to the given key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown if the resource file was not loaded.</exception>
    internal Shader? LoadShader(string key) {
        if (!WasLoaded)
            throw new InvalidOperationException("Resource file was not loaded.");

        string vsPath = $"Shaders/{key}.vert";
        ZipArchiveEntry? zippedVShader = FileArchive!.GetEntry(vsPath);

        if (zippedVShader == null) {
            Log.WriteLine($"Vertex shader {key} doesn't exist in this resource file");
            return null;
        }

        using Stream vsStream = zippedVShader.Open();
        StreamReader vsReader = new StreamReader(vsStream);
        string vertexShaderSource = vsReader.ReadToEnd();

        string fsPath = $"Shaders/{key}.frag";
        ZipArchiveEntry? zippedFShader = FileArchive!.GetEntry(fsPath);

        if (zippedFShader == null) {
            Log.WriteLine($"Fragment shader {key} doesn't exist in this resource file");
            return null;
        }

        using Stream fsStream = zippedFShader.Open();
        StreamReader fsReader = new StreamReader(fsStream);
        string fragmentShaderSource = fsReader.ReadToEnd();


        return GraphicsHelper.CreateShader(key, vertexShaderSource, fragmentShaderSource);
    }

    /// <summary>
    /// Tries to load a NPatchTexture from the zip archive.
    /// </summary>
    /// <param name="key"></param>
    /// <exception cref="InvalidOperationException">Thrown if the resource file was not loaded.</exception>
    public NPatchTexture? LoadNPatchTexture(string key) {
        if (!WasLoaded)
            throw new InvalidOperationException("Resource file was not loaded.");

        Texture2D? texture = LoadTexture(key);

        if (texture == null)
            return null;

        string path = $"Textures/NPatchData/{key}.json";
        ZipArchiveEntry? zippedText = FileArchive!.GetEntry(path);

        if (zippedText == null) {
            Log.WriteLine($"NPatchData {key} doesn't exist in this resource file");
            return null;
        }

        using Stream textStream = zippedText.Open();
        Dictionary<string, int>? dict = JsonSerializer.Deserialize<Dictionary<string, int>>(textStream);

        if (dict == null)
            return null;

        return new NPatchTexture(texture, dict["left"], dict["right"], dict["top"], dict["bottom"]);
    }

    private void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                FileArchive?.Dispose();
                FileArchive = null;
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ResourceFile()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public override bool Equals(object? obj) => Equals(obj as ResourceFile);
    public bool Equals(ResourceFile? other) => other is not null && Name == other.Name;
    public override int GetHashCode() => HashCode.Combine(Name);

    public static bool operator ==(ResourceFile? left, ResourceFile? right) => EqualityComparer<ResourceFile>.Default.Equals(left, right);
    public static bool operator !=(ResourceFile? left, ResourceFile? right) => !(left == right);
}
