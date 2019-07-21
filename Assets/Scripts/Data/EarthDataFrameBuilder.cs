using System;
using UnityEngine;

public class EarthDataFrameBuilder : IEarthDataFrameBuilder {
    private const int HISTOGRAM_SAMPLES = 256;

    private int m_Width;
    private int m_Height;
    private int m_Depth;

    private Color[] m_3DBuffer;
    private Color[] m_2DBuffer;

    private int[] m_HistogramValuesBuffer;
    private Color[] m_HistogramColorBuffer;

    public EarthDataFrameBuilder(int width, int height, int depth) {
        m_Width = width;
        m_Height = height;
        m_Depth = depth;

        m_3DBuffer = new Color[m_Width * m_Height * m_Depth];
        m_2DBuffer = new Color[m_Width * m_Height];

        m_HistogramValuesBuffer = new int[HISTOGRAM_SAMPLES];
        m_HistogramColorBuffer = new Color[HISTOGRAM_SAMPLES];
    }

    public EarthDataFrame Build8Bit(byte[][] data) {
        int depth = data.Length;

        // Create data texture
        Texture3D dataTexture = BuildDataTexture(data);
        Texture2D histogramTexture = BuildHistogramTexture();

        return new EarthDataFrame() { Dimensions = new Vector3(m_Width, m_Height, depth), DataTexture = dataTexture, HistogramTexture = histogramTexture };
    }

    public EarthDataFrame Build16Bit(short[][] data) {
        throw new NotImplementedException();
    }

    private Texture3D BuildDataTexture(byte[][] data) {
        int size2d = m_Width * m_Height;
        Debug.Log(m_3DBuffer.Length + " m_3DBuffer length, " + " Width: " + m_Width + " Height: " + m_Height + " Size2d: " + size2d);
        // Get color data from all textures
        for (int i = 0; i < data.Length; i++) {
            // Fill 2d color buffer with data
            for (int x = 0; x < m_Width; x++) {
                for (int y = 0; y < m_Height; y++) {
                    int index = y * m_Width + x;
                    byte value = data[i][index];
                    m_2DBuffer[index] = new Color32(value, 0, 0, 0);
                }
            }
            
            m_2DBuffer.CopyTo(m_3DBuffer, i * size2d);
        }

        Texture3D dataTexture = new Texture3D(m_Width, m_Height, m_Depth, TextureFormat.R8, false);
        dataTexture.SetPixels(m_3DBuffer);
        dataTexture.Apply();

        return dataTexture;
    }

    private Texture2D BuildHistogramTexture() {
        Texture2D histogram = new Texture2D(HISTOGRAM_SAMPLES, 1, TextureFormat.RFloat, false);

        int maxFrequency = 0;
        for (int i = 0; i < m_3DBuffer.Length; i++) {
            int value = (int)(m_3DBuffer[i].r * (HISTOGRAM_SAMPLES - 1));
            m_HistogramValuesBuffer[value] += 1;
            maxFrequency = Mathf.Max(maxFrequency, m_HistogramValuesBuffer[value]);
        }

        for (int i = 0; i < HISTOGRAM_SAMPLES; i++) {
            float value = Mathf.Log10((float)m_HistogramValuesBuffer[i]) / Mathf.Log10((float)maxFrequency);
            m_HistogramColorBuffer[i] = new Color(value, 0.0f, 0.0f, 1.0f);
        }

        histogram.SetPixels(m_HistogramColorBuffer);
        histogram.Apply();

        return histogram;
    }
}
