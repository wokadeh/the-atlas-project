using UnityEngine;

public class DataAssetBuilder : IDataAssetBuilder {
    public DataAsset Build(Texture2D[] textures) {
        // NOTE: Here would be the data interpolation of layers, time or maybe even texture size.
        //       For now we simply convert the 2D textures to a 3D texture.

        // HACK: We assume every texture has the same size as the first
        int width = textures[0].width;
        int height = textures[0].height;
        int depth = textures.Length;

        // Create data texture
        Texture3D dataTexture = BuildDataTexture(textures, width, height, depth, out Color[] colors);
        Texture2D histogramTexture = BuildHistogramTexture(colors);

        // We have taken ownership over the textures and now that the processing is over we can destroy them
        for (int i = 0; i < textures.Length; i++) {
            Object.Destroy(textures[i]);
        }

        return new DataAsset() { Dimensions = new Vector3(width, height, depth), DataTexture = dataTexture, HistogramTexture = histogramTexture };
    }

    private Texture3D BuildDataTexture(Texture2D[] textures, int width, int height, int depth, out Color[] colors) {
        int size2d = width * height;
        int size3d = size2d * depth;

        // Get color data from all textures
        colors = new Color[size3d];
        for (int i = 0; i < textures.Length; i++) {
            Texture2D texture = textures[i];
            Color[] pixels = texture.GetPixels();
            pixels.CopyTo(colors, i * size2d);
        }

        Texture3D dataTexture = new Texture3D(width, height, depth, TextureFormat.R8, false);
        dataTexture.SetPixels(colors);
        dataTexture.Apply();

        return dataTexture;
    }

    private Texture2D BuildHistogramTexture(Color[] dataColors) {
        int samples = 256;
        int[] values = new int[samples];
        Color[] colors = new Color[samples];
        Texture2D histogram = new Texture2D(samples, 1, TextureFormat.RFloat, false);

        int maxFrequency = 0;
        for (int i = 0; i < dataColors.Length; i++) {
            int value = (int)(dataColors[i].r * (samples - 1));
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
