using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using BitMiracle.LibTiff.Classic;

public class DataLoaderFromTIFFs : IDataLoader {
    private class TiffException : Exception {
        public TiffException(string message) : base(message) { }
    }

    private const int TEXTURE_WIDTH  = 875;
    private const int TEXTURE_HEIGHT = 656;
    private const TextureFormat TEXTURE_FORMAT = TextureFormat.RGBA32;
    private const bool TEXTURE_MIPMAPS = false;

    // TODO: We need some sort of meta data for the tiffs because we don't know what sort of data we are loading in

    public Texture2D[] Load(string path) {
        if (Directory.Exists(path) == false) {
            Debug.LogError($"[DataLoaderFromDisk] - Trying to load data from the directory: {path} that does not exist!");
            return null;
        }

        // We only search in the top directory for files with the ".tif" or ".tiff" ending
        List<string> paths = null;
        try { 
            // TODO: Paths are not sorted correctly
            paths = Directory.GetFiles(path, "*.*").Where(p => p.EndsWith(".tif") || p.EndsWith(".tiff")).OrderBy(s => s).ToList();
        } catch(Exception e) {
            Debug.LogError($"[DataLoaderFromDisk] - Failed to get files from directory: {path} with exception:\n{e.GetType().Name} - {e.Message}!");
            return null;
        }

        // Load in each file as a texture
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        int count = paths.Count();
        Texture2D[] textures = new Texture2D[count];
        for (int i = 0; i < count; i++) {
            try {
                textures[i] = LoadTiff(paths[i]);
            } catch (Exception e) {
                Debug.LogError($"[DataLoaderFromDisk] - Failed to load tiff at paht: {path} with exception:\n{e.GetType().Name} - {e.Message}!");
                return null;
            }
        }
        Debug.Log($"[DataLoaderFromDisk] - Loading of tiffs took {(stopwatch.ElapsedMilliseconds / 1000.0f).ToString("0.00")} seconds.");

        return textures;
    }

    private Texture2D LoadTiff(string path) {
        using (Tiff image = Tiff.Open(path, "r")) {
            // Find the width and height of the image
            FieldValue[] value = image.GetField(TiffTag.IMAGEWIDTH);
            int width = value[0].ToInt();

            value = image.GetField(TiffTag.IMAGELENGTH);
            int height = value[0].ToInt();
            
            // HACK: For now we are only loading tiffs with a certain hardcoded with and height
            if (width != TEXTURE_WIDTH) {
                throw new TiffException($"Tiff does not have the expected width of: {TEXTURE_WIDTH}");
            }
            if (height != TEXTURE_HEIGHT) {
                throw new TiffException($"Tiff does not have the expected height of: {TEXTURE_HEIGHT}");
            }

            // Read the image into the memory buffer
            int[] raster = new int[width * height];
            if (!image.ReadRGBAImage(width, height, raster)) {
                throw new TiffException("Failed to read pixels from tiff!");
            }

            // We convert the raster to colors
            Color32[] colors = new Color32[width * height];
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    int index = y * width + x;
                    colors[index] = GetColorFromBytes(raster[index]);
                }
            }

            // Create actual texture with special format
            Texture2D texture = new Texture2D(width, height, TEXTURE_FORMAT, TEXTURE_MIPMAPS);
            texture.SetPixels32(colors);
            texture.Apply();

            return texture;
        }
    }

    private Color32 GetColorFromBytes(int bytes) {
        byte r = (byte)Tiff.GetR(bytes);
        byte g = (byte)Tiff.GetG(bytes);
        byte b = (byte)Tiff.GetB(bytes);
        byte a = (byte)Tiff.GetA(bytes);
        return new Color32(r, g, b, a);
    }
}
