using UnityEngine;

public class DataAssetBuilder : IDataAssetBuilder {
    public DataAsset Build(Color[][] colors) {
        // NOTE: Here would be the data interpolation of layers, time or maybe even texture size.
        //       For now we simply convert the 2D textures to a 3D texture.

        // HACK: We assume every texture has the same size as the first
        int width = DataManager.TIFF_WIDTH;
        int height = DataManager.TIFF_HEIGHT;
        int depth = colors.Length;

        // Create data texture
        Texture3D dataTexture = BuildDataTexture(colors, width, height, depth, out Color[] data);
        Texture2D histogramTexture = BuildHistogramTexture(data);

        return new DataAsset() { Dimensions = new Vector3(width, height, depth), DataTexture = dataTexture, HistogramTexture = histogramTexture };
    }

    private Texture3D BuildDataTexture(Color[][] colors, int width, int height, int depth, out Color[] data) {
        int size2d = width * height;
        int size3d = size2d * depth;

        // Get color data from all textures
        data = new Color[size3d];
        for (int i = 0; i < colors.Length; i++) {
            Color[] pixels = colors[i];
            pixels.CopyTo(data, i * size2d);
            colors[i] = null;
        }

        Texture3D dataTexture = new Texture3D(width, height, depth, TextureFormat.R8, false);
        dataTexture.SetPixels(data);
        dataTexture.Apply();

        return dataTexture;
    }

    private Texture2D BuildHistogramTexture(Color[] data) {
        int samples = 256;
        int[] values = new int[samples];
        Color[] colors = new Color[samples];
        Texture2D histogram = new Texture2D(samples, 1, TextureFormat.RFloat, false);

        int maxFrequency = 0;
        for (int i = 0; i < data.Length; i++) {
            int value = (int)(data[i].r * (samples - 1));
            values[value] += 1;
            maxFrequency = Mathf.Max(maxFrequency, values[value]);
        }

        for (int i = 0; i < samples; i++) {
            float value = Mathf.Log10((float)values[i]) / Mathf.Log10((float)maxFrequency);
            colors[i] = new Color(value, 0.0f, 0.0f, 1.0f);
        }

        histogram.SetPixels(colors);
        histogram.Apply();

        return histogram;
    }
}
