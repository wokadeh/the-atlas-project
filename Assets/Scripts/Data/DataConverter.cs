using UnityEngine;

public class DataConverter : IDataConverter {
    public Texture3D Convert(Texture2D[] textures) {
        // NOTE: Here would be the data interpolation of layers, time or maybe even texture size.
        //       For now we simply convert the 2D textures to a 3D texture.

        // HACK: We assume every texture has the same size as the first
        int width = textures[0].width;
        int height = textures[0].height;
        int depth = textures.Length;

        // Create the texture
        Color[] colors = GetColorsFromTextures(textures, width, height, depth);
        Texture3D data = new Texture3D(width, height, depth, TextureFormat.R8, false);
        data.SetPixels(colors);
        data.Apply();

        return data;
    }

    private Color[] GetColorsFromTextures(Texture2D[] textures, int width, int height, int depth) {
        int size2d = width * height;
        int size3d = size2d * depth;

        Color[] colors = new Color[size3d];

        for (int i = 0; i < textures.Length; i++) {
            Texture2D texture = textures[i];
            Color[] pixels = texture.GetPixels();
            pixels.CopyTo(colors, i * size2d);
        }

        return colors;
    }
}
