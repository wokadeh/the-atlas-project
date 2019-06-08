using UnityEngine;

public class DataManager : MonoBehaviour {
    private IDataLoader m_DataLoder;
    private IDataConverter m_DataConverter;

    private void Start() {
        m_DataLoder = new DataLoaderFromTIFFs();
        m_DataConverter = new DataConverter();
    }

    public void Load(string path) {
        Texture2D[] textures = m_DataLoder.Load(path);
        Texture3D texture = m_DataConverter.Convert(textures);
    }
}
