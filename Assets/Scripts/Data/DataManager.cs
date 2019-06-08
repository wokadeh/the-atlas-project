using UnityEngine;

public class DataManager : MonoBehaviour {
    [SerializeField] private FilterMode m_DataTextureFilterMode;

    private IDataLoader m_DataLoder;
    private IDataAssetBuilder m_DataConverter;

    private void Start() {
        m_DataLoder = new DataLoaderFromTIFFs();
        m_DataConverter = new DataAssetBuilder();
    }

    public DataAsset Load(string path) {
        Texture2D[] textures = m_DataLoder.Load(path);
        DataAsset asset = m_DataConverter.Build(textures);
        asset.DataTexture.filterMode = m_DataTextureFilterMode;
        return asset;
    }
}
