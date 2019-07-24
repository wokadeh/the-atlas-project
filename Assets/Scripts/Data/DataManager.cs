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
    private IEarthDataFrameBuilder m_EarthDataFrameBuilder;

    private void Start() {
        m_MetaDataReader = new MetaDataManager();
        m_DataAssets = new Dictionary<string, List<EarthDataFrame>>();
    }

    public void ImportData(string _projectFilePath, IProgress<float> _progress, Action _callback) {
        StartCoroutine(ImportDataCoroutine(_projectFilePath, _progress, _callback));
    }

    public void SaveProject(string _projectFilePath, IProgress<float> _progress, Action _callback)
    {
        StartCoroutine(SaveProjectCoroutine(_projectFilePath, _progress, _callback));
    }

    public void SetCurrentAsset(EarthDataFrame _earthDataFrame) {
        m_CurrentAsset = _earthDataFrame;
        OnDataAssetChanged?.Invoke(_earthDataFrame);
    }

    public void SetCurrentVariable(string _variable) {
        m_CurrentVariable = _variable;

        SetCurrentAsset(DataAssets.First());

        // Set new data
        m_VolumeRenderer.SetData(m_CurrentAsset);
    }

    private IEnumerator SaveProjectCoroutine(string _projectFolderPath, IProgress<float> _progress, Action _callback)
    {
        // 1. Write new xml file with all previous data (bitdepth, levels, etc.)

        // Warning! Make sure always MetaData has been filled by importing/loading!!!!
        try
        {
            string filePath = _projectFolderPath + m_MetaData.DataName + ".xml";

            Debug.Log("[DataManager] - Trying to write project XML to to: " + filePath);
            
            m_MetaDataReader.Write(filePath, m_MetaData, m_DataAssets);

            Debug.Log("[DataManager] - Successfully wrote project XML to: " + filePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataManager] - Failed to save meta data: '{_projectFolderPath}' with exception:\n{e.GetType().Name} - {e.Message}!");
            _callback?.Invoke();
        }

        //// 2. write 3dTexture assets to folders of variables
        for (int i = 0; i < m_MetaData.Variables.Count; i++)
        {

            IVariable variable = m_MetaData.Variables[i];

            Debug.Log("pos 1: count variable " + variable.Name);

            // Temperature/texture3D
            string variablePath = _projectFolderPath + "\\" + variable.Name + Globals.TEXTURE3D_FOLDER_NAME;

            Debug.Log("pos 2: set path " + variablePath);

            // Only create if it does not exist, yet
            if (!Directory.Exists(variablePath))
            {
                Debug.Log("pos 3: try to create directory " + variablePath);
                Directory.CreateDirectory(variablePath);
                Debug.Log("pos 4: created directory " + variablePath);
            }
            //    yield return StartCoroutine(SaveVariableRoutine(variable, variablePath, new Progress<float>(value => {
            //        // Do overall progress report
            //        Debug.Log("pos 5: trying to save it... ");
            //        float progression = i / (float)m_MetaData.Variables.Count;
            //        _progress.Report(progression + (value / m_MetaData.Variables.Count));
            //    })));
        }
        yield return 0; 

        // 2. write 2dTexture from Histo to folders of variables

        _callback?.Invoke();
    }

    private IEnumerator ImportDataCoroutine(string _projectFilePath, IProgress<float> _progress, Action _callback) {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Read in meta data
        IMetaData metaData = null;
        BitDepth bitDepth = BitDepth.Depth8;
        try {
            metaData = m_MetaDataReader.Read(_projectFilePath);
            bitDepth = GetBitDepth(metaData);
        } catch (Exception e) {
            Debug.LogError($"[DataManager] - Failed to read meta data: '{_projectFilePath}' with exception:\n{e.GetType().Name} - {e.Message}!");
            _callback?.Invoke();
        }

        IDataLoader tiffLoader = new DataLoaderFromTIFFs(metaData.Width, metaData.Height, metaData.Levels);
        m_EarthDataFrameBuilder = new EarthDataFrameBuilder(metaData.Width, metaData.Height, metaData.Levels);

        for (int i = 0; i < metaData.Variables.Count; i++) {
            IVariable variable = metaData.Variables[i];

            // For each variable there is a list of all textures that are used
            m_DataAssets[variable.Name] = new List<EarthDataFrame>();

            string folder = Path.Combine(Path.GetDirectoryName(_projectFilePath), variable.Name.ToLower());
            if (Directory.Exists(folder)) {
                yield return StartCoroutine(ImportVariableRoutine(tiffLoader, folder, m_DataAssets[variable.Name], bitDepth, new Progress<float>(value => {
                    // Do overall progress report
                    float progression = i / (float)metaData.Variables.Count;
                    _progress.Report(progression + (value / metaData.Variables.Count));
                })));
            } else {
                Debug.LogError($"[DataManager] - Trying to load variable: '{variable.Name}' but the corresponding directory does not exist!");
                _callback?.Invoke();
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

        _callback?.Invoke();
    }

    private IEnumerator SaveVariableRoutine(IVariable _variable, string _variablePath, IProgress<float> _progress)
    {
        int i = 0;
        foreach (EarthDataFrame asset in m_DataAssets[_variable.Name])
        {
            AssetDatabase.CreateAsset(asset.DataTexture, _variablePath + "/" + asset.DataTexture.name + ".asset");

            // Report progress
            float progression = (i + 1) / (float)m_DataAssets.Count;
            _progress.Report(progression);
            i++;
            yield return null;
        }
        yield return null;
    }

    private IEnumerator ImportVariableRoutine(IDataLoader _loader, string _projectFolder, List<EarthDataFrame> _earthFrameList, BitDepth _bitDepth, IProgress<float> _progress) {
        // We assume every directory is a time stamp which contains the level tiffs
        string[] directories = Directory.GetDirectories(_projectFolder);

        for (int i = 0; i < directories.Length; i++) {
            string directory = directories[i];

            EarthDataFrame asset;
            switch (_bitDepth) {
                case BitDepth.Depth8: asset = m_EarthDataFrameBuilder.Build8Bit(_loader.Load8Bit(directory)); break;
                case BitDepth.Depth16: asset = m_EarthDataFrameBuilder.Build16Bit(_loader.Load16Bit(directory)); break;
                default: yield break;
            }
            _earthFrameList.Add(asset);

            // Report progress
            float progression = (i + 1) / (float)directories.Length;
            _progress.Report(progression);
            yield return null;
        }

        yield return null;
    }

    private BitDepth GetBitDepth(IMetaData _metaData) {
        switch (_metaData.BitDepth) {
            case 8: return BitDepth.Depth8;
            case 16: return BitDepth.Depth16;
            default: throw new Exception("Failed to determine bit depth!");
        }
    }
}
