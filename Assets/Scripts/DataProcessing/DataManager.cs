// ****************************** LOCATION ********************************
//
// [Volume_Renderer] Volume_Renderer -> attached
//
// ************************************************************************

using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private Dictionary<string, List<TimeStepDataAsset>> m_DataDictionary;
    private Dictionary<string, List<TimeStepDataAsset>> m_DataLevelDictionary;
    

    public IReadOnlyList<TimeStepDataAsset> CurrentDataAssetList => m_DataDictionary[CurrentVariableName];
    public IReadOnlyList<TimeStepDataAsset> CurrentDataLevelAssetList => m_DataLevelDictionary[CurrentVariableName];

    public event Action OnNewImport;
    public event Action<TimeStepDataAsset> OnDataAssetChanged;

    public IMetaData MetaData { get; private set; }
    public string CurrentVariableName { get; private set; }
    public string CurrentProjectFilePath { get; private set; }
    public double CurrentVariableMin { get; private set; }
    public double CurrentVariableMax { get; private set; }
    public ProjectLoadUI ProjectLoader { get; private set; }
    public TimeStepDataAsset CurrentTimeStepDataAsset { get; private set; }
    public TimeStepDataAsset CurrentLevelTimeStepDataAsset { get; private set; }

    public Dictionary<string, int> LevelModeList { get; private set; }

    private IMetaDataManager m_MetaDataManager;

    private bool m_IsLevelMode = false;
    private int m_CurrentLevel = 0;

    private void Start()
    {
        this.Clear();

        System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo( "en-US" );
    }

    // Call to clean up when a project is closed
    private void Clear()
    {
        m_MetaDataManager = new MetaDataManager();
        m_DataDictionary = new Dictionary<string, List<TimeStepDataAsset>>();
        m_DataLevelDictionary = new Dictionary<string, List<TimeStepDataAsset>>();
        this.CurrentVariableName = "";
        this.CurrentVariableMin = 0;
        this.CurrentVariableMax = 0;
        this.MetaData = null;

        this.SetupLevelList();
    }

    // Start internal routines to import the data to a new project
    public void ImportData( string _projectFilePath, IProgress<float> _progress, Action _callback )
    {
        this.Clear();
        IMetaData metaData = this.ImportMetaData(_projectFilePath, _callback );
        IDataLoader tiffLoader = new TiffLoader( metaData );
        this.CurrentProjectFilePath = _projectFilePath;
        this.StartCoroutine( this.LoadImportDataCoroutine( false, LevelModeList.Count - 1, _projectFilePath, tiffLoader, metaData, _progress, _callback ) );
    }

    // Start internal routines to save the settings of an open project
    public void SaveProject( string _projectFileName, string _projectFolderPath, bool _saveOnlyXml, IProgress<float> _progress, Action _callback )
    {
        this.StartCoroutine( this.SaveProjectCoroutine( _projectFileName, _projectFolderPath, _saveOnlyXml, _progress, _callback ) );
    }

    // Start internal routines to load an existing project
    public IEnumerator LoadProject( ProjectLoadUI _projectLoader, int _level, string _projectFilePath, IProgress<float> _progress, Action _callback )
    {
        Log.Debg( this, "LOAD PROJECT" );
        this.Clear();
        this.ProjectLoader = _projectLoader;
        IMetaData metaData = this.ImportMetaData(_projectFilePath, _callback );
        IDataLoader projectFilesLoader = new ProjectFilesLoader( metaData );
        this.CurrentProjectFilePath = _projectFilePath;
        yield return this.StartCoroutine( this.LoadImportDataCoroutine( true, _level, _projectFilePath, projectFilesLoader, metaData, _progress, _callback ) );
    }

    public void ReloadProject( string _levelName )
    {
        int level = this.LevelModeList[_levelName];
        Log.Debg( this, "Current level is " + m_CurrentLevel + " and selected level is " + level );
        // Only load again if the selected level is different from the rendered one
        if( m_CurrentLevel != level )
        {
            Log.Debg( this, "Start loading level " + level );
            StartCoroutine( this.ProjectLoader.LoadProjectCoroutine( this.CurrentProjectFilePath, level ) );
            
        }
        else
        {
            Log.Debg( this, "NOT loading level " + level );
        }
    }

    // Hold on a specific timestep and display the corresponding data
    public void SetCurrentAsset( TimeStepDataAsset _timeStepDataAsset )
    {
        this.CurrentTimeStepDataAsset = _timeStepDataAsset;
        this.OnDataAssetChanged?.Invoke( _timeStepDataAsset );
    }

    // Set the selected variable
    public void SetCurrentVariable( string _variable, double _min, double _max )
    {
        Log.Info( this, "Current variable changed to " + _variable );
        this.CurrentVariableName = _variable;
        this.CurrentVariableMin = _min;
        this.CurrentVariableMax = _max;

        if( m_IsLevelMode )
        {
            this.SetCurrentAsset( this.CurrentDataLevelAssetList.First() );
            // Set new data
            Singleton.GetVolumeRenderer().SetData( this.CurrentLevelTimeStepDataAsset );
        }
        else
        {
            this.SetCurrentAsset( this.CurrentDataAssetList.First() );
            // Set new data
            Singleton.GetVolumeRenderer().SetData( this.CurrentTimeStepDataAsset );
        }
    }

    public void SetupLevelList()
    {
        if( LevelModeList == null)
        {
            LevelModeList = new Dictionary<string, int>();

            if( LevelModeList.Count == 0 )
            {
                LevelModeList = Globals.LEVEL_LIST_37();

                LevelModeList.Add( Globals.LEVEL_ALL_NAME, LevelModeList.Count );
            }
        }
    }

    private IEnumerator SaveProjectCoroutine( string _projectFileName, string _projectFolderPath, bool _saveOnlyXml, IProgress<float> _progress, Action _callback )
    {
        // 1. Write new xml file with all previous data (bitdepth, levels, etc.)
        if( this.MetaData != null )
        {
            // Warning! Make sure always MetaData has been filled by importing/loading!!!!
            string projectFileAndFolderPath = Path.Combine( _projectFolderPath, _projectFileName );

            try
            {
                // Only create if it does not exist, yet
                if( !Directory.Exists( projectFileAndFolderPath ) )
                {
                    Directory.CreateDirectory( projectFileAndFolderPath );
                }
                string filePath = Path.Combine( projectFileAndFolderPath, _projectFileName + ".xml" );

                this.MetaData = m_MetaDataManager.SetMetaDataName( _projectFileName, this.MetaData );

                m_MetaDataManager.Write( filePath, this.MetaData, m_DataDictionary );

                Log.Info( this, "Successfully wrote project XML to: " + filePath );
            }
            catch( Exception e )
            {
                Log.ThrowException( this, "Failed to save meta data: " + projectFileAndFolderPath + " with exception:\n " + e.GetType().Name + "-" + e.Message );
                _callback?.Invoke();
                yield break;
            }

            if( !_saveOnlyXml )
            {
                // 2. write 3dTexture assets to folders of variables
                yield return this.CreateAssets( _progress );
            }
        }
        else
        {
            Log.Warn( this, "Cannot save, because no data is loaded!" );
            yield break;
        }

        _callback?.Invoke();
    }
    private IEnumerator CreateAssets( IProgress<float> _progress )
    {
        for( int varIndex = 0; varIndex < this.MetaData.Variables.Count; varIndex++ )
        {
            IVariable variable = this.MetaData.Variables[ varIndex ];

            string variablePath = Path.Combine( Globals.RESOURCES, Globals.SAVE_PROJECTS_PATH, this.MetaData.DataName, variable.Name );

            Log.Warn( this, "Save assets to: " + variablePath );

            // Temperature/texture3D
            if( !Directory.Exists( variablePath ) )
            {
                Directory.CreateDirectory( variablePath );
            }

            yield return this.StartCoroutine( this.SaveVariableRoutine( variable, variablePath, varIndex, new Progress<float>( value =>
            {
                // Do overall progress report
                _progress.Report( Utils.CalculateProgress( varIndex, MetaData.Variables.Count, value ) );
            } ) ) );
        }
    }

    // TODO: not used, yet!
    private IEnumerator SaveVariableRoutine( IVariable _variable, string _variablePath, int _varIndex, IProgress<float> _progress )
    {
        string textureAssetPath = Path.Combine( _variablePath, Globals.TEXTURE3D_FOLDER_NAME );
        // Only create if it does not exist, yet
        if( !Directory.Exists( textureAssetPath ) )
        {
            Directory.CreateDirectory( textureAssetPath );
        }

        List<TimeStepDataAsset> currentVariableTimeStepList = m_DataDictionary[ _variable.Name ];

        string[] variableAssets = new string[currentVariableTimeStepList.Count];

        for( int i = 0; i < currentVariableTimeStepList.Count; i++ )
        {
            string dateTimeString = this.MetaData.Timesteps[ _varIndex ][ i ].DateTimeDouble.ToString().Replace( ',', '.' );
            string assetName = Globals.TEXTURE3D_PREFEX + this.MetaData.DataName + "_" + _variable.Name + "_" + dateTimeString;
            string assetPath = textureAssetPath + "/";


            variableAssets[i] = assetPath + assetName;

            Log.Info( this, "Create asset " + variableAssets[i] );

            TimeStepDataAsset asset = currentVariableTimeStepList[ i ];

            // Report progress
            _progress.Report( Utils.CalculateProgress( i, currentVariableTimeStepList.Count ) );

            yield return null;
        }

        yield return null;
    }

    IMetaData ImportMetaData( string _dataPath, Action _callback )
    {
        // Read in meta data
        IMetaData metaData = null;
        Utils.BitDepth bitDepth = Utils.BitDepth.Depth8;
        try
        {
            metaData = m_MetaDataManager.Import( _dataPath );
            bitDepth = Utils.GetBitDepth( metaData );
        }
        catch( Exception e )
        {
            Log.ThrowException( this, "Failed to read meta data: " + _dataPath + " with exception:\n " + e.GetType().Name + "-" + e.Message );
            _callback?.Invoke();
        }

        return metaData;
    }

    private IEnumerator LoadImportDataCoroutine( bool _isLoading, int _level, string _projectFilePath, IDataLoader _dataLoader, IMetaData _metaData, IProgress<float> _progress, Action _callback )
    {
        Log.Debg( this, "LOAD IMPORT DATA COROUTINE" );

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        m_DataDictionary.Clear();

        // Read in meta data
        ITimeStepDataAssetBuilder timeAssetBuilder = new TimeStepDataAssetBuilder( _metaData.Width, _metaData.Height, _metaData.Levels );

        Log.Debg( this, "Found " + _metaData.Variables.Count + " variables" );

        for( int i = 0; i < _metaData.Variables.Count; i++ )
        {
            IVariable variable = _metaData.Variables[ i ];

            // For each variable there is a list of all textures that are used
            m_DataDictionary[variable.Name] = new List<TimeStepDataAsset>();

            string folder = Path.Combine( Path.GetDirectoryName( _projectFilePath ), variable.Name.ToLower() );

            string[] dataSet = _dataLoader.GetDataSet( folder );

            Log.Debg( this, "Create folders and files for variable " + variable.Name );

            if( Directory.Exists( folder ) )
            {
                yield return this.StartCoroutine( this.LoadImportVariableRoutine( _level, dataSet, _dataLoader, timeAssetBuilder, variable.Name, "", m_DataDictionary[variable.Name], new Progress<float>( value =>
                {
                // Do overall progress report
                _progress.Report( Utils.CalculateProgress( i, _metaData.Variables.Count, value ) );
                } ) ) );
            }
            else
            {
                Log.ThrowValueNotFoundException( this, variable.Name );
                _callback?.Invoke();
            }
        }

        if(!_isLoading)
        {
            m_MetaDataManager.SaveProjectXMLFile( _metaData, m_DataDictionary );
        }

        m_CurrentLevel = _level;

        Log.Info( this, "Loading and creating assets took " + ( stopwatch.ElapsedMilliseconds / 1000.0f ).ToString( "0.00" ) + "seconds." );

        this.FinishLoading( _metaData, stopwatch );

        _callback?.Invoke();
    }

    private void FinishLoading( IMetaData _metaData, System.Diagnostics.Stopwatch _stopwatch )
    {
        Log.Info( this, "Loading and creating assets took " + ( _stopwatch.ElapsedMilliseconds / 1000.0f ).ToString( "0.00" ) + "seconds." );


        if( m_DataDictionary[m_DataDictionary.First().Key].Count > 0 )
        {
            this.MetaData = _metaData;
            this.CurrentVariableName = m_DataDictionary.First().Key;
            this.CurrentVariableMin = _metaData.Variables[0].Min;
            this.CurrentVariableMax = _metaData.Variables[0].Max;

            this.CurrentTimeStepDataAsset = this.CurrentDataAssetList.First();

            // Set new data
            Singleton.GetVolumeRenderer().SetData( this.CurrentTimeStepDataAsset );

        }
        else
        {
            Log.Info( this, "There is no current data asset loaded" );
        }

        OnNewImport?.Invoke();
    }

    private IEnumerator LoadImportVariableRoutine( int _level, string[] _dataSet, IDataLoader _dataLoader, ITimeStepDataAssetBuilder _timestepDataAssetBuilder, string _variableName, string _assetEnding, List<TimeStepDataAsset> _timestepDataAssetList, IProgress<float> _progress )
    {
        Log.Debg( this, "LOAD VARIABLE AT LEVEL " + _level );

        Log.Info( this, "Found " + _dataSet.Length + " data items" );

        int assetFileIndex = 0;
        foreach( string filePath in _dataSet )
        {
            Log.Info( this, "Find asset from variable path  " + filePath );

            if( filePath.EndsWith( _assetEnding ) )
            {
                Log.Info( this, "Creating asset from image from " + filePath );
                TimeStepDataAsset timestepAsset = _timestepDataAssetBuilder.BuildTimestepDataAssetFromData( _dataLoader.Import( _level, filePath, _variableName ) );

                if( timestepAsset != null )
                {
                    _timestepDataAssetList.Add( timestepAsset );
                }

                assetFileIndex++;

                // Report progress
                _progress.Report( Utils.CalculateProgress( assetFileIndex, _dataSet.Length ) );
                yield return null;
            }
        }
        Log.Info( this, "Found " + assetFileIndex + " assets" );

        yield return null;
    }
}