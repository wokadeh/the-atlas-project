﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour {
    private enum BitDepth {
        Depth8,
        Depth16
    }

    [SerializeField] private VolumeRenderer m_VolumeRenderer;
    [SerializeField] private TransferFunctionUI m_TransferFunctionUI;

    private Dictionary<string, List<DataAsset>> m_DataAssets;
    public IReadOnlyList<DataAsset> DataAssets => m_DataAssets[CurrentVariable];

    public event Action OnNewImport;
    public event Action<DataAsset> OnDataAssetChanged;

    public IMetaData MetaData { get; private set; }
    public string CurrentVariable { get; private set; }
    public DataAsset CurrentAsset { get; private set; }

    private IMetaDataReader m_MetaDataReader;

    private void Start() {
        m_MetaDataReader = new MetaDataReader();
        m_DataAssets = new Dictionary<string, List<DataAsset>>();
    }

    public void Load(string file, IProgress<float> progress, Action callback) {
        StartCoroutine(LoadCoroutine(file, progress, callback));
    }

    public void SetCurrentAsset(DataAsset asset) {
        CurrentAsset = asset;
        OnDataAssetChanged?.Invoke(asset);
    }

    public void SetCurrentVariable(string variable) {
        CurrentVariable = variable;

        SetCurrentAsset(DataAssets.First());

        // Set new data
        m_VolumeRenderer.SetData(CurrentAsset);
    }

    private IEnumerator LoadCoroutine(string file, IProgress<float> progress, Action callback) {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Read in meta data
        IMetaData metaData = null;
        BitDepth bitDepth = BitDepth.Depth8;
        try {
            metaData = m_MetaDataReader.Read(file);
            bitDepth = GetBitDepth(metaData);
        } catch (Exception e) {
            Debug.LogError($"[DataManager] - Failed to read meta data: '{file}' with exception:\n{e.GetType().Name} - {e.Message}!");
            callback?.Invoke();
        }

        IDataLoader loader = new DataLoaderFromTIFFs(metaData.Width, metaData.Height, metaData.Levels);
        IDataAssetBuilder builder = new DataAssetBuilder(metaData.Width, metaData.Height, metaData.Levels);

        for (int i = 0; i < metaData.Variables.Count; i++) {
            IVariable variable = metaData.Variables[i];

            m_DataAssets[variable.Name] = new List<DataAsset>();

            string folder = Path.Combine(Path.GetDirectoryName(file), variable.Name.ToLower());
            if (Directory.Exists(folder)) {
                yield return StartCoroutine(LoadVariable(loader, builder, folder, m_DataAssets[variable.Name], bitDepth, new Progress<float>(value => {
                    // Do overall progress report
                    float progression = i / (float)metaData.Variables.Count;
                    progress.Report(progression + (value / metaData.Variables.Count));
                })));
            } else {
                Debug.LogError($"[DataManager] - Trying to load variable: '{variable.Name}' but the corresponding directory does not exist!");
                callback?.Invoke();
            }
        }

        Debug.Log($"[DataManager] - Loading and creating assets took {(stopwatch.ElapsedMilliseconds / 1000.0f).ToString("0.00")} seconds.");

        MetaData = metaData;
        CurrentVariable = m_DataAssets.First().Key;
        CurrentAsset = DataAssets.First();

        // Set new data
        m_VolumeRenderer.SetData(CurrentAsset);
        m_TransferFunctionUI.Redraw();

        OnNewImport?.Invoke();

        callback?.Invoke();
    }

    private IEnumerator LoadVariable(IDataLoader loader, IDataAssetBuilder builder, string folder, List<DataAsset> assets, BitDepth bitDepth, IProgress<float> progress) {
        // We assume every directory is a time stamp which contains the level tiffs
        // We also assume that they are correctly ordered
        string[] directories = Directory.GetDirectories(folder);

        for (int i = 0; i < directories.Length; i++) {
            string directory = directories[i];

            DataAsset asset;
            switch (bitDepth) {
                case BitDepth.Depth8: asset = builder.Build8Bit(loader.Load8Bit(directory)); break;
                case BitDepth.Depth16: asset = builder.Build16Bit(loader.Load16Bit(directory)); break;
                default: yield break;
            }
            assets.Add(asset);

            // Report progress
            float progression = (i + 1) / (float)directories.Length;
            progress.Report(progression);
            yield return null;
        }

        yield return null;
    }

    private BitDepth GetBitDepth(IMetaData metaData) {
        switch (metaData.BitDepth) {
            case 8: return BitDepth.Depth8;
            case 16: return BitDepth.Depth16;
            default: throw new Exception("Failed to determine bit depth!");
        }
    }
}
