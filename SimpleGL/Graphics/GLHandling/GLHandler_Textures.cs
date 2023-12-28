using OpenTK.Graphics.OpenGL4;
using SimpleGL.Graphics.Textures;
using SimpleGL.Util;
using SimpleGL.Util.Extensions;
using StbImageSharp;

namespace SimpleGL.Graphics.GLHandling;
public static partial class GLHandler {
    internal static int SupportedTextureUnits {
        get {
            if (_SupportedTextureUnits == -1)
                _SupportedTextureUnits = GL.GetInteger(GetPName.MaxTextureImageUnits);

            return _SupportedTextureUnits;
        }
    }

    internal static void InitializeTexture(ImageResult image, out int textureId) {
        textureId = GL.GenTexture();

        GL.BindTexture(TextureTarget.Texture2D, textureId);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

        //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);


        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    internal static void DeleteTexture(Texture texture) {
        if (texture.IsDisposed)
            return;

        GL.BindTexture(TextureTarget.Texture2D, texture.TextureId);
        //ReleaseTexture(texture);

        int texID = texture.TextureId;
        ReflectionExtensions.SetProperty(texture, "TextureId", -1);
        GL.DeleteTexture(texID);
    }

    internal static void AssignTexture(Texture texture) {
        int unassignedTextureUnit = GetUnassignedTextureUnit();
        if (unassignedTextureUnit < 0) {
            Log.WriteLine("Cannot assign texture. No texture unit is available.", eLogType.Warning);
            return;
        }

        AssignTexture(texture, unassignedTextureUnit, false);
    }

    internal static void AssignTexture(Texture texture, int textureUnit, bool overrideBoundTexture = true) {
        if (textureUnit < 0 || textureUnit >= SupportedTextureUnits) {
            Log.WriteLine($"Invalid texture unit {textureUnit}. Must be between 0 and {SupportedTextureUnits} (exclusive).", eLogType.Error);
            return;
        }

        int assignedTextureUnit = AssignedTextureUnit(texture);
        if (assignedTextureUnit == textureUnit)
            return;

        Texture? currentlyAssignedTexture = AssignedTexture(textureUnit);
        if (currentlyAssignedTexture != null && !overrideBoundTexture) {
            Log.WriteLine($"Cannot assign texture {texture.TextureId} to texture unit {textureUnit}. Another texture is already assigned.", eLogType.Warning);
            return;
        }

        GL.BindTextureUnit(textureUnit, texture.TextureId);
        //GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
        //GL.BindTexture(TextureTarget.Texture2D, texture.TextureId);

        AssignedTextures[textureUnit] = texture;
        AssignedTextureUnits[texture] = textureUnit;
    }

    internal static void UnassignTexture(Texture texture) {
        int assignedTextureUnit = AssignedTextureUnit(texture);
        if (assignedTextureUnit < 0)
            return;

        AssignedTextures[assignedTextureUnit] = null;
        AssignedTextureUnits.Remove(texture);
        GL.BindTextureUnit(assignedTextureUnit, 0);
    }

    internal static void UnassignTextureUnit(int textureUnit) {
        if (textureUnit < 0 || textureUnit >= SupportedTextureUnits) {
            Log.WriteLine($"Invalid texture unit {textureUnit}. Must be between 0 and {SupportedTextureUnits} (exclusive).", eLogType.Error);
            return;
        }

        Texture currentlyAssignedTexture = AssignedTexture(textureUnit);

        if (currentlyAssignedTexture == null)
            return;

        AssignedTextures[textureUnit] = null;
        AssignedTextureUnits.Remove(currentlyAssignedTexture);
        GL.BindTextureUnit(textureUnit, 0);
    }

    internal static bool IsTextureAssigned(Texture texture) => AssignedTextureUnit(texture) >= 0;

    internal static int AssignedTextureUnit(Texture texture) {
        if (AssignedTextureUnits.TryGetValue(texture, out int textureUnit))
            return textureUnit;
        else
            return -1;
    }

    internal static Texture? AssignedTexture(int textureUnit) {
        if (textureUnit < 0 || textureUnit >= SupportedTextureUnits) {
            Log.WriteLine($"Invalid texture unit {textureUnit}. Must be between 0 and {SupportedTextureUnits} (exclusive).", eLogType.Error);
            return null;
        }

        return AssignedTextures[textureUnit];
    }

    internal static int GetUnassignedTextureUnit() {
        for (int i = 0; i < AssignedTextures.Length; i++) {
            if (AssignedTextures[i] == null)
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Updates the texture's wrap mode.
    /// </summary>
    /// <param name="texture">The texture.</param>
    internal static void UpdateTextureWrapMode(Texture texture) {
        if (texture.IsDisposed) {
            Log.WriteLine("Cannot update texture wrap mode. Texture is disposed.", eLogType.Error);
            return;
        }

        GL.BindTexture(TextureTarget.Texture2D, texture.TextureId);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GraphicUtils.ToWrapMode(texture.WrapS));
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GraphicUtils.ToWrapMode(texture.WrapT));

        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    /// <summary>
    /// Updates the texture's filter mode.
    /// </summary>
    /// <param name="texture">The texture.</param>
    internal static void UpdateTextureFilterMode(Texture texture) {
        if (texture.IsDisposed) {
            Log.WriteLine("Cannot update texture filter mode. Texture is disposed.", eLogType.Error);
            return;
        }

        GL.BindTexture(TextureTarget.Texture2D, texture.TextureId);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GraphicUtils.ToMinFilter(texture.MinFilter));
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GraphicUtils.ToMinFilter(texture.MagFilter));

        GL.BindTexture(TextureTarget.Texture2D, 0);
    }
}
