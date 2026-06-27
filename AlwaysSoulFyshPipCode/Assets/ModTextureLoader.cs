using System;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace AlwaysSoulFyshPip.AlwaysSoulFyshPipCode.Assets;

public static class ModTextureLoader
{
    private static readonly Dictionary<string, Texture2D> Cache = [];

    public static Texture2D? Load(string fileName)
    {
        if (Cache.TryGetValue(fileName, out Texture2D? texture))
        {
            return texture;
        }

        try
        {
            string? assemblyPath = typeof(ModTextureLoader).Assembly.Location;
            string? modDir = Path.GetDirectoryName(assemblyPath);
            if (modDir == null)
            {
                return null;
            }

            string imagePath = Path.Combine(modDir, "assets", fileName);
            if (!File.Exists(imagePath))
            {
                MainFile.Logger.Error($"Texture asset '{fileName}' not found at '{imagePath}'.");
                return null;
            }

            Image image = Image.LoadFromFile(imagePath);
            ImageTexture imageTexture = ImageTexture.CreateFromImage(image);
            Cache[fileName] = imageTexture;
            return imageTexture;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Failed to load texture asset '{fileName}': {ex}");
            return null;
        }
    }
}
