using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        string[] paths = null;
        try { 
            paths = Directory.GetFiles(path, "*.*").Where(p => p.EndsWith(".tif") || p.EndsWith(".tiff")).OrderBy(s => PadNumbers(s)).ToArray();
        } catch(Exception e) {
            Debug.LogError($"[DataLoaderFromDisk] - Failed to get files from directory: {path} with exception:\n{e.GetType().Name} - {e.Message}!");
            return null;
        }

        // Load in each file as a texture
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Texture2D[] textures = LoadTiffs(paths);
        Debug.Log($"[DataLoaderFromDisk] - Loading of tiffs took {(stopwatch.ElapsedMilliseconds / 1000.0f).ToString("0.00")} seconds.");

        return textures;
    }

    private Texture2D[] LoadTiffs(string[] paths) {
        int length = paths.Length;
        Texture2D[] textures = new Texture2D[length];
        
        // HACK: For now we are only loading tiffs with a certain hardcoded width and height
        // To reduce memory usage we create the buffers here and reuse them for each load
        // which works because every image is the same size
        int[] raster = new int[TEXTURE_WIDTH * TEXTURE_HEIGHT];
        Color32[] colors = new Color32[TEXTURE_WIDTH * TEXTURE_HEIGHT];

        for (int i = 0; i < length; i++) {
            string path = paths[i];
            try {
                textures[i] = LoadTiff(path, raster, colors);
            } catch (Exception e) {
                Debug.LogError($"[DataLoaderFromDisk] - Failed to load tiff at paht: {path} with exception:\n{e.GetType().Name} - {e.Message}!");
                return null;
            }
        }
        return textures;
    }

    private Texture2D LoadTiff(string path, int[] raster, Color32[] colors) {
        using (Tiff image = Tiff.Open(path, "r")) {
            // Find the width and height of the image
            FieldValue[] value = image.GetField(TiffTag.IMAGEWIDTH);
            int width = value[0].ToInt();

            value = image.GetField(TiffTag.IMAGELENGTH);
            int height = value[0].ToInt();
            
            if (width != TEXTURE_WIDTH) {
                throw new TiffException($"Tiff does not have the expected width of: {TEXTURE_WIDTH}");
            }
            if (height != TEXTURE_HEIGHT) {
                throw new TiffException($"Tiff does not have the expected height of: {TEXTURE_HEIGHT}");
            }

            // Read the image into the raster buffer
            if (!image.ReadRGBAImage(width, height, raster)) {
                throw new TiffException("Failed to read pixels from tiff!");
            }

            // We convert the raster to colors
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    int index = y * width + x;
                    colors[index] = GetColorFromBytes(raster[index]);
                }
            }

            // Create actual texture with special format
            Texture2D texture = new Texture2D(width, height, TEXTURE_FORMAT, TEXTURE_MIPMAPS);
            texture.SetPixels32(colors);
            // We do not need to Apply() the textures because we don't need to load them onto the gpu

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

    private static string PadNumbers(string input) {
        return Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(10, '0'));
    }
}
