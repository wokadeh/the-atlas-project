using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class DataManager : MonoBehaviour {
    

    [SerializeField] private VolumeRenderer m_VolumeRenderer;
    [SerializeField] private TransferFunctionUI m_TransferFunctionUI;

    private Dictionary<string, List<TimeStepDataAsset>> m_DataAssets;
    public IReadOnlyList<TimeStepDataAsset> DataAssets => m_DataAssets[m_CurrentVariable];

    public event Action OnNewImport;
    public event Action<TimeStepDataAsset> OnDataAssetChanged;

    public IMetaData m_MetaData { get; private set; }
    public string m_CurrentVariable { get; private set; }
    public TimeStepDataAsset m_CurrentAsset { get; private set; }
    private IMetaDataManager m_MetaDataManager;
    private IDataLoader m_DataLoder;

    private void Start()
    {
        this.Clear();
    }

    private void Clear()
    {
        m_MetaDataManager = new MetaDataManager();
        m_DataAssets = new Dictionary<string, List<TimeStepDataAsset>>();
        m_CurrentAsset = null;
        m_CurrentVariable = "";
        m_MetaData = null;
    }

    public void ImportData(string _projectFilePath, IProgress<float> _progress, Action _callback)
    {
        this.Clear();
        this.StartCoroutine(ImportDataCoroutine(_projectFilePath, _progress, _callback));
    }

    public void SaveProject(string _projectFilePath, bool _saveOnlyXml, IProgress<float> _progress, Action _callback)
    {
        this.StartCoroutine(SaveProjectCoroutine(_projectFilePath, _saveOnlyXml, _progress, _callback));
    }

    public void LoadProject(string _projectFilePath, IProgress<float> _progress, Action _callback)
    {
        this.Clear();
        this.StartCoroutine(LoadProjectCoroutine(_projectFilePath, _progress, _callback));
    }

    public void SetCurrentAsset(TimeStepDataAsset _earthDataFrame) {
        m_CurrentAsset = _earthDataFrame;
        OnDataAssetChanged?.Invoke(_earthDataFrame);
    }

    public void SetCurrentVariable(string _variable)
    {
        m_CurrentVariable = _variable;

        SetCurrentAsset(DataAssets.First());

        // Set new data
        m_VolumeRenderer.SetData(m_CurrentAsset);
    }

    private IEnumerator SaveProjectCoroutine(string _projectFolderPath, bool _saveOnlyXml, IProgress<float> _progress, Action _callback)
    {
        // 1. Write new xml file with all previous data (bitdepth, levels, etc.)
        if(m_MetaData != null)
        {
            // Warning! Make sure always MetaData has been filled by importing/loading!!!!
            string projectFolderPath = _projectFolderPath + m_MetaData.DataName;

            try
            {
                // Only create if it does not exist, yet
                if (!Directory.Exists(projectFolderPath))
                {
                    Directory.CreateDirectory(projectFolderPath);
                }
                string filePath = Path.Combine(projectFolderPath, m_MetaData.DataName + ".xml");

                m_MetaDataManager.Write(filePath, m_MetaData, m_DataAssets);

                Log.Info(this, "Successfully wrote project XML to: " + filePath);
            }
            catch (Exception e)
            {
                Log.ThrowException(this, "Failed to save meta data: " + projectFolderPath + " with exception:\n " + e.GetType().Name + "-" + e.Message);
                _callback?.Invoke();
            }

            if (!_saveOnlyXml)
            {
                // 2. write 3dTexture assets to folders of variables
                yield return this.CreateAssets(_progress);
            }
        }
        else
        {
            Log.Warn(this, "Cannot save, because no data is loaded!");
        }        

        _callback?.Invoke();
    }

    private IEnumerator CreateAssets(IProgress<float> _progress)
    {
        for (int varIndex = 0; varIndex < m_MetaData.Variables.Count; varIndex++)
        {
            IVariable variable = m_MetaData.Variables[varIndex];

            string variablePath = Path.Combine(Globals.SAVE_PROJECTS_PATH, m_MetaData.DataName, variable.Name);

            // Temperature/texture3D
            if (!Directory.Exists(variablePath))
            {
                Directory.CreateDirectory(variablePath);
            }

            yield return StartCoroutine(SaveVariableRoutine(variable, variablePath, varIndex, new Progress<float>(value =>
            {
                // Do overall progress report
                float progression = varIndex / (float)m_MetaData.Variables.Count;
                _progress.Report(progression + (value / m_MetaData.Variables.Count));
            })));
        }
    }

    private IEnumerator ImportDataCoroutine(string _projectFilePath, IProgress<float> _progress, Action _callback)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Read in meta data
        IMetaData metaData = null;
        Utils.BitDepth bitDepth = Utils.BitDepth.Depth8;
        try {
            metaData = m_MetaDataManager.Import(_projectFilePath);
            bitDepth = Utils.GetBitDepth(metaData);
        } catch (Exception e) {
            Log.ThrowException(this, "Failed to read meta data: " +_projectFilePath + " with exception:\n " + e.GetType().Name + "-" + e.Message );
            _callback?.Invoke();
        }

        IDataLoader tiffLoader = new DataLoaderFromTIFFs(metaData.Width, metaData.Height, metaData.Levels);
        ITimeStepDataAssetBuilder timeAssetBuilder = new TimeStepDataAssetBuilder(metaData.Width, metaData.Height, metaData.Levels);

        for (int i = 0; i < metaData.Variables.Count; i++)
        {
            IVariable variable = metaData.Variables[i];

            // For each variable there is a list of all textures that are used
            m_DataAssets[variable.Name] = new List<TimeStepDataAsset>();

            string folder = Path.Combine(Path.GetDirectoryName(_projectFilePath), variable.Name.ToLower());
            if (Directory.Exists(folder)) {
                yield return StartCoroutine(ImportVariableRoutine(tiffLoader, timeAssetBuilder, folder, m_DataAssets[variable.Name], bitDepth, new Progress<float>(value => {
                    // Do overall progress report
                    float progression = i / (float)metaData.Variables.Count;
                    _progress.Report(progression + (value / metaData.Variables.Count));
                })));
            } else {
                Log.ThrowValueNotFoundException(this, variable.Name);
                _callback?.Invoke();
            }
        }

        Log.Info(this, "Loading and creating assets took " + (stopwatch.ElapsedMilliseconds / 1000.0f).ToString("0.00") + "seconds.");

        m_MetaData = metaData;
        m_CurrentVariable = m_DataAssets.First().Key;
        m_CurrentAsset = DataAssets.First();

        // Set new data
        m_VolumeRenderer.SetData(m_CurrentAsset);
        m_TransferFunctionUI.Redraw();

        OnNewImport?.Invoke();

        _callback?.Invoke();
    }

    private IEnumerator LoadProjectCoroutine(string _projectFilePath, IProgress<float> _progress, Action _callback)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        m_DataAssets.Clear();

        // Read in meta data
        IMetaData metaData = null;
        Utils.BitDepth bitDepth = Utils.BitDepth.Depth8;
        
        try
        {
            metaData = m_MetaDataManager.Load(_projectFilePath);
            bitDepth = Utils.GetBitDepth(metaData);
        }
        catch (Exception e)
        {
            Log.ThrowException(this, "Failed to read meta data: " + _projectFilePath + " with exception:\n " + e.GetType().Name + "-" + e.Message);
            _callback?.Invoke();
        }

        ITimeStepDataAssetBuilder timeAssetBuilder = new TimeStepDataAssetBuilder(metaData.Width, metaData.Height, metaData.Levels);

        for (int i = 0; i < metaData.Variables.Count; i++)
        {
            IVariable variable = metaData.Variables[i];

            // For each variable there is a list of all textures that are used
            m_DataAssets[variable.Name] = new List<TimeStepDataAsset>();

            string projectAssetPath = Globals.SAVE_PROJECTS_PATH + metaData.DataName;

            string variableFolderPath = Path.Combine(projectAssetPath, variable.Name.ToLower());
            if (Directory.Exists(projectAssetPath))
            {
                yield return StartCoroutine(LoadVariableRoutine(variableFolderPath, timeAssetBuilder, m_DataAssets[variable.Name], variable, bitDepth, new Progress<float>(value => {
                    // Do overall progress report
                    float progression = i / (float)metaData.Variables.Count;
                    _progress.Report(progression + (value / metaData.Variables.Count));
                })));
            }
            else
            {
                Log.ThrowValueNotFoundException(this, variable.Name);
                _callback?.Invoke();
            }
        }

        Log.Info(this, "Loading and creating assets took " + (stopwatch.ElapsedMilliseconds / 1000.0f).ToString("0.00") + "seconds.");

        m_MetaData = metaData;
        m_CurrentVariable = m_DataAssets.First().Key;
        m_CurrentAsset = DataAssets.First();

        // Set new data
        m_VolumeRenderer.SetData(m_CurrentAsset);
        m_TransferFunctionUI.Redraw();

        OnNewImport?.Invoke();

        _callback?.Invoke();
    }

    private IEnumerator SaveVariableRoutine(IVariable _variable, string _variablePath, int varIndex, IProgress<float> _progress)
    {
        string textureAssetPath = Path.Combine(_variablePath, Globals.TEXTURE3D_FOLDER_NAME);
        // Only create if it does not exist, yet
        if (!Directory.Exists(textureAssetPath))
        {
            Directory.CreateDirectory(textureAssetPath);
        }

        int timestampIndex = 0;
        foreach (TimeStepDataAsset asset in m_DataAssets[_variable.Name])
        {
            string dateTimeString = m_MetaData.Timestamps[varIndex][timestampIndex].DateTime.ToString().Replace(',', '.');
            string assetName = Globals.TEXTURE3D_PREFEX + _variable.Name + "_" + dateTimeString;
            string assetPath = textureAssetPath + "/";

            AssetDatabase.CreateAsset(asset.DataTexture, assetPath + assetName + ".asset");

            // Report progress
            float progression = (timestampIndex + 1) / (float)m_DataAssets.Count;
            _progress.Report(progression);
            timestampIndex++;

            yield return null;
        }
        yield return null;
    }


    private IEnumerator LoadVariableRoutine(string _variableFolder, ITimeStepDataAssetBuilder _timestepDataAssetBuilder, List<TimeStepDataAsset> _timestepDataAssetList, IVariable variable, Utils.BitDepth _bitDepth, IProgress<float> _progress)
    {
        string assetPath = Path.Combine(_variableFolder, Globals.TEXTURE3D_FOLDER_NAME);
            
        Log.Info(this, "Look for assets at " + assetPath);

        string[] assets = Directory.GetFiles(assetPath);

        Log.Info(this, "Found " + assets.Length + " assets");

        //object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        //object[] assets = Resources.LoadAll(assetPath);
        Texture3D asset = null;
        int assetFileIndex = 0;
        foreach (string file in assets)
        {
            if(file.EndsWith(".asset"))
            {
                Log.Info(this, "Found " + file);

                asset = AssetDatabase.LoadAssetAtPath<Texture3D>(file);

                TimeStepDataAsset timestepAsset = _timestepDataAssetBuilder.BuildTimestepDataAssetFromTexture(asset);

                _timestepDataAssetList.Add(timestepAsset);

                assetFileIndex++;

                // Report progress
                float progression = (assetFileIndex) / (float)assets.Length;
                _progress.Report(progression);
                yield return null;
            }
        }
        Log.Info(this, "Found " + assetFileIndex + " assets");

        yield return null;
    }

    private IEnumerator ImportVariableRoutine(IDataLoader _loader, ITimeStepDataAssetBuilder _timestepDataAssetBuilder, string _projectFolder, List<TimeStepDataAsset> _timestepDataAssetList, Utils.BitDepth _bitDepth, IProgress<float> _progress) {
        // We assume every directory is a time stamp which contains the level tiffs
        string[] directories = Directory.GetDirectories(_projectFolder);

        for (int i = 0; i < directories.Length; i++) {
            string directory = directories[i];

            TimeStepDataAsset asset;
            switch (_bitDepth) {
                case Utils.BitDepth.Depth8: asset = _timestepDataAssetBuilder.BuildTimestepDataAssetFromData(_loader.Load8Bit(directory)); break;
                case Utils.BitDepth.Depth16: asset = _timestepDataAssetBuilder.Build16Bit(_loader.Load16Bit(directory)); break;
                default: yield break;
            }
            _timestepDataAssetList.Add(asset);

            // Report progress
            float progression = (i + 1) / (float)directories.Length;
            _progress.Report(progression);
            yield return null;
        }

        yield return null;
    }

    
}
