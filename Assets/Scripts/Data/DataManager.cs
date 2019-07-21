using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class DataManager : MonoBehaviour {
    private enum BitDepth {
        Depth8,
        Depth16
    }

    [SerializeField] private VolumeRenderer m_VolumeRenderer;
    [SerializeField] private TransferFunctionUI m_TransferFunctionUI;

    private Dictionary<string, List<EarthDataFrame>> m_DataAssets;
    public IReadOnlyList<EarthDataFrame> DataAssets => m_DataAssets[m_CurrentVariable];

    public event Action OnNewImport;
    public event Action<EarthDataFrame> OnDataAssetChanged;

    public IMetaData m_MetaData { get; private set; }
    public string m_CurrentVariable { get; private set; }
    public EarthDataFrame m_CurrentAsset { get; private set; }

    private IMetaDataManager m_MetaDataReader;

    private IDataLoader m_DataLoder;
    private IDataAssetBuilder m_EarthDataFrameBuilder;

    private void Start() {
        m_MetaDataReader = new MetaDataManager();
        m_DataAssets = new Dictionary<string, List<EarthDataFrame>>();
    }

    public void ImportData(string file, IProgress<float> progress, Action callback) {
        StartCoroutine(ImportDataCoroutine(file, progress, callback));
    }

    public void SaveProject(string file, IProgress<float> progress, Action callback)
    {
        StartCoroutine(SaveProjectCoroutine(file, progress, callback));
    }

    public void SetCurrentAsset(EarthDataFrame asset) {
        m_CurrentAsset = asset;
        OnDataAssetChanged?.Invoke(asset);
    }

    public void SetCurrentVariable(string variable) {
        m_CurrentVariable = variable;

        SetCurrentAsset(DataAssets.First());

        // Set new data
        m_VolumeRenderer.SetData(m_CurrentAsset);
    }

    private IEnumerator SaveProjectCoroutine(string projectFilePath, IProgress<float> progress, Action callback)
    {

        // Load Data
        IMetaData metaData = m_MetaData;
        // 1. write new xml file with all previous data (bitdepth, levels, etc.)
        // 2. write location of 3dTexture assets


        // Read in meta data
        //IMetaData metaData = null;
        //BitDepth bitDepth = BitDepth.Depth8;
        //try
        //{
        //    metaData = m_MetaDataReader.Read(projectFilePath);
        //    bitDepth = GetBitDepth(metaData);
        //}
        //catch (Exception e)
        //{
        //    Debug.LogError($"[DataManager] - Failed to read meta data: '{projectFilePath}' with exception:\n{e.GetType().Name} - {e.Message}!");
        //    callback?.Invoke();
        //}

        for (int i = 0; i < m_MetaData.Variables.Count; i++)
        {
            IVariable variable = m_MetaData.Variables[i];

            string variablePath = "/unity/" + variable.Name;

            if (!Directory.Exists(variablePath))
            {
                Directory.CreateDirectory(variablePath);

                yield return StartCoroutine(SaveVariableRoutine(variable, variablePath, m_MetaData.BitDepth, new Progress<float>(value => {
                    // Do overall progress report
                    float progression = i / (float)m_MetaData.Variables.Count;
                    progress.Report(progression + (value / m_MetaData.Variables.Count));
                })));
            }
            else
            {
                Debug.LogError($"[DataManager] - The folder '{variable.Name}' already exists. Abort saving!");
                callback?.Invoke();
            }
        }

        callback?.Invoke();
    }

    private IEnumerator ImportDataCoroutine(string file, IProgress<float> progress, Action callback) {
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

        // TODO: Load level count from meta data
        IDataLoader loader = new DataLoaderFromTIFFs(metaData.Width, metaData.Height, metaData.BitDepth);
        m_EarthDataFrameBuilder = new EarthDataFrameBuilder(metaData.Width, metaData.Height, metaData.BitDepth);

        for (int i = 0; i < metaData.Variables.Count; i++) {
            IVariable variable = metaData.Variables[i];

            // For each variable there is a list of all textures that are used
            m_DataAssets[variable.Name] = new List<EarthDataFrame>();

            string folder = Path.Combine(Path.GetDirectoryName(file), variable.Name.ToLower());
            if (Directory.Exists(folder)) {
                yield return StartCoroutine(ImportVariableRoutine(loader, folder, m_DataAssets[variable.Name], bitDepth, new Progress<float>(value => {
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

        m_MetaData = metaData;
        m_CurrentVariable = m_DataAssets.First().Key;
        m_CurrentAsset = DataAssets.First();

        // Set new data
        m_VolumeRenderer.SetData(m_CurrentAsset);
        m_TransferFunctionUI.Redraw();

        OnNewImport?.Invoke();

        callback?.Invoke();
    }

    private IEnumerator SaveVariableRoutine(IVariable variable, string variablePath, int bitDepth, IProgress<float> progress)
    {
        int i = 0;
        foreach (EarthDataFrame asset in m_DataAssets[variable.Name])
        {
            AssetDatabase.CreateAsset(asset.DataTexture, variablePath + "/" + asset.DataTexture.name + ".asset");

            // Report progress
            float progression = (i + 1) / (float)m_DataAssets.Count;
            progress.Report(progression);
            i++;
            yield return null;
        }
        yield return null;
    }

    private IEnumerator ImportVariableRoutine(IDataLoader loader, string folder, List<EarthDataFrame> assets, BitDepth bitDepth, IProgress<float> progress) {
        // We assume every directory is a time stamp which contains the level tiffs
        string[] directories = Directory.GetDirectories(folder);

        for (int i = 0; i < directories.Length; i++) {
            string directory = directories[i];

            EarthDataFrame asset;
            switch (bitDepth) {
                case BitDepth.Depth8: asset = m_EarthDataFrameBuilder.Build8Bit(loader.Load8Bit(directory)); break;
                case BitDepth.Depth16: asset = m_EarthDataFrameBuilder.Build16Bit(loader.Load16Bit(directory)); break;
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
