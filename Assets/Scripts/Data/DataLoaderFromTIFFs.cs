﻿using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using BitMiracle.LibTiff.Classic;

public class DataLoaderFromTIFFs : IDataLoader {
    private class TiffException : Exception {
        public TiffException(string message) : base(message) { }
    }

    private int m_Width;
    private int m_Height;
    private int m_Levels;

    // For performance reasons these arrays are reused and therefore refilled with new data when calling "Load" again
    private int[] m_Raster;
    private byte[][] m_Buffer;

    public DataLoaderFromTIFFs(int _width, int _height, int _bithDepth) {
        m_Width = _width;
        m_Height = _height;
        m_Levels = _bithDepth;

        // The raster and buffer for tiff loading can be reused and therefore need only to be created once
        int size = _width * _height;
        m_Raster = new int[size];
        m_Buffer = new byte[_bithDepth][];
        for (int i = 0; i < _bithDepth; i++) {
            m_Buffer[i] = new byte[size];
        }
    }

    public byte[][] Load8Bit(string path) {
        // Get all tiffs in the directory and sort them appropriately
        string[] files = Directory.GetFiles(path, "*.*").Where(p => p.EndsWith(".tif") || p.EndsWith(".tiff")).OrderBy(s => PadNumbers(s)).ToArray();

        // Check the amount of files matches the expected levels
        if (files.Length != m_Levels) {
            Debug.Log($"[DataLoaderFromTIFFs] - Trying to load a data set which does not contain {m_Levels} tiff levels!");
            return null;
        }

        // Load in all tiffs
        for (int i = 0; i < files.Length; i++) {
            string file = files[i];
            try {
                LoadTiff8Bit(file, i);
            } catch(Exception e) {
                Debug.LogError($"[DataLoaderFromTIFFs] - Failed to read tiff: '{file}' with exception:\n{e.GetType().Name} - {e.Message}!");
            }
        }

        return m_Buffer;
    }

    public short[][] Load16Bit(string _projectFolder) {
        throw new NotImplementedException();
    }

    private void LoadTiff8Bit(string path, int level) {
        using (Tiff image = Tiff.Open(path, "r")) {
            // Find the width and height of the image
            FieldValue[] value = image.GetField(TiffTag.IMAGEWIDTH);
            int width = value[0].ToInt();

            value = image.GetField(TiffTag.IMAGELENGTH);
            int height = value[0].ToInt();

            if (width != m_Width) {
                throw new TiffException($"Tiff does not have the expected width of: {m_Width}");
            }
            if (height != m_Height) {
                throw new TiffException($"Tiff does not have the expected height of: {m_Height}");
            }

            // Read the image into the raster buffer
            if (!image.ReadRGBAImage(width, height, m_Raster)) {
                throw new TiffException("Failed to read pixels from tiff!");
            }

            // We convert the raster to bytes
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    int index = y * width + x;

                    m_Buffer[level][index] = GetByteFromBytes(m_Raster[index]);
                }
            }
        }
    }

    private byte GetByteFromBytes(int _bytes) {
        // It is not important from which channel (r,g,b) we take the byte
        // as all three contain the same for a tiff with a bit depth of 8
        return (byte)Tiff.GetR(_bytes);
    }

    private static string PadNumbers(string _input) {
        return Regex.Replace(_input, "[0-9]+", match => match.Value.PadLeft(10, '0'));
    }
}
