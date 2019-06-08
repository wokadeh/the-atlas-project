using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataConverter : IDataConverter {
    [SerializeField] private int[] m_Size = new int[3] { 875, 656, 6 };
    [SerializeField] private List<Texture2D> m_DataTextures;
    [SerializeField] private Vector4 m_DataChannels;

    private void Start() {
        // Create the texture
        Color[] colors = GetColorsFromTextures();
        Texture3D data = new Texture3D(m_Size[0], m_Size[1], m_Size[2], TextureFormat.R8, false);
        data.SetPixels(colors);
        data.Apply();

        // Assign it to the material of the parent object
        try {
            //Material material = GetComponent<Renderer>().material;
            //material.SetTexture("_Data", data);
            //material.SetVector("_DataChannel", m_DataChannels);
        } catch {
            Debug.Log("Cannot attach the texture to the parent object");
        }
    }

    private Color[] GetColorsFromTextures() {
        int size2d = m_DataTextures[0].width * m_DataTextures[0].height;
        int size3d = size2d * m_DataTextures.Count;

        Color[] colors = new Color[size3d];

        for (int i = 0; i < m_DataTextures.Count; i++) {
            Texture2D texture = m_DataTextures[i];
            Color[] pixels = texture.GetPixels();
            pixels.CopyTo(colors, i * size2d);
        }

        return colors;
    }

    public Texture3D Convert(Texture2D[] textures) {
        return null;
    }
}
