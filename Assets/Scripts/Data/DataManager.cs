using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour {
    public const int TIFF_WIDTH = 875;
    public const int TIFF_HEIGHT = 656;

    [SerializeField] private FilterMode m_DataTextureFilterMode;

    private IDataLoader m_DataLoder;
    private IDataAssetBuilder m_DataAssetBuilder;

    private List<DataAsset> m_DataAssets;
    public IReadOnlyList<DataAsset> DataAssets => m_DataAssets;

    public DataAsset CurrentAsset { get; private set; }

    private void Start() {
        m_DataLoder = new DataLoaderFromTIFFs();
        m_DataAssetBuilder = new DataAssetBuilder();
        m_DataAssets = new List<DataAsset>();
    }

    public DataAsset Load(string path) {
        Color[][][] colors = m_DataLoder.Load(path);

        // Try end clear out all the temporary memory that has been piling up until now
        GC.Collect();
        GC.WaitForPendingFinalizers();

        m_DataAssets.Clear();
        for (int i = 0; i < colors.Length; i++) {
            DataAsset asset = m_DataAssetBuilder.Build(colors[i]);
            asset.DataTexture.filterMode = m_DataTextureFilterMode;
            m_DataAssets.Add(asset);
        }

        // Try end clear out all the temporary memory that has been piling up until now
        GC.Collect();
        GC.WaitForPendingFinalizers();

        CurrentAsset = m_DataAssets[0];
        return CurrentAsset;
    }

    public void SetCurrentAsset(DataAsset asset) {
        CurrentAsset = asset;
    }
}
