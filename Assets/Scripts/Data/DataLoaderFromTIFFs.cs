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

    // TODO: We need some sort of meta data for the tiffs because we don't know what sort of data we are loading in

    public Color[][][] Load(string path) {
        if (Directory.Exists(path) == false) {
            Debug.LogError($"[DataLoaderFromDisk] - Trying to load data from the directory: {path} that does not exist!");
            return null;
        }

        // We only search in the top directory for files with the ".tif" or ".tiff" ending
        string[] directories = null;
        try {
            directories = Directory.GetDirectories(path);
        } catch(Exception e) {
            Debug.LogError($"[DataLoaderFromDisk] - Failed to get files from directory: {path} with exception:\n{e.GetType().Name} - {e.Message}!");
            return null;
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        int length = directories.Length;
        Color[][][] colors = new Color[length][][];
        string[] paths = null;
        // Load the tiffs from every directory
        for (int i = 0; i < length; i++) {
            string directory = directories[i];
            paths = Directory.GetFiles(directory, "*.*").Where(p => p.EndsWith(".tif") || p.EndsWith(".tiff")).OrderBy(s => PadNumbers(s)).ToArray();
            colors[i] = LoadTiffs(paths);
        }

        Debug.Log($"[DataLoaderFromDisk] - Loading of tiffs took {(stopwatch.ElapsedMilliseconds / 1000.0f).ToString("0.00")} seconds.");

        return colors;
    }

    private Color[][] LoadTiffs(string[] paths) {
        int length = paths.Length;
        Color[][] colors = new Color[length][];
        
        // HACK: For now we are only loading tiffs with a certain hardcoded width and height

        // To reduce memory usage we create the buffer here and reuse it for each load
        // which works because every image is the same size
        int[] raster = new int[DataManager.TIFF_WIDTH * DataManager.TIFF_HEIGHT];

        for (int i = 0; i < length; i++) {
            string path = paths[i];
            try {
                colors[i] = LoadTiff(path, raster);
            } catch (Exception e) {
                Debug.LogError($"[DataLoaderFromDisk] - Failed to load tiff at paht: {path} with exception:\n{e.GetType().Name} - {e.Message}!");
                return null;
            }
        }
        return colors;
    }

    private Color[] LoadTiff(string path, int[] raster) {
        using (Tiff image = Tiff.Open(path, "r")) {
            // Find the width and height of the image
            FieldValue[] value = image.GetField(TiffTag.IMAGEWIDTH);
            int width = value[0].ToInt();

            value = image.GetField(TiffTag.IMAGELENGTH);
            int height = value[0].ToInt();

            if (width != DataManager.TIFF_WIDTH) {
                throw new TiffException($"Tiff does not have the expected width of: {DataManager.TIFF_WIDTH}");
            }
            if (height != DataManager.TIFF_HEIGHT) {
                throw new TiffException($"Tiff does not have the expected height of: {DataManager.TIFF_HEIGHT}");
            }

            // Read the image into the raster buffer
            if (!image.ReadRGBAImage(width, height, raster)) {
                throw new TiffException("Failed to read pixels from tiff!");
            }

            Color[] colors = new Color[width * height];

            // We convert the raster to colors
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    int index = y * width + x;
                    colors[index] = GetColorFromBytes(raster[index]);
                }
            }

            return colors;
        }
    }

    private Color GetColorFromBytes(int bytes) {
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
