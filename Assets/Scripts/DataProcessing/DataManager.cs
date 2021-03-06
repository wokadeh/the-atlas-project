﻿// ****************************** LOCATION ********************************
//
// [UI] CartesianLevelScalePlane (Prefeb Asset) -> attached
//
// ************************************************************************

using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField] private CameraModeUI m_CameraModeUI;
    [SerializeField] private DataTypeUI m_DataTypeUI;
    [SerializeField] private TransferFunctionUI m_TransferFunctionUI;
    [SerializeField] private TimelineUI m_TimelineUI;

    private Dictionary<string, List<TimeStepDataAsset>> m_DataAssets;
    public IReadOnlyList<TimeStepDataAsset> CurrentDataAssets => m_DataAssets[CurrentVariableName];

    public event Action OnNewImport;
    public event Action<TimeStepDataAsset> OnDataAssetChanged;

    public IMetaData MetaData { get; private set; }
    public string CurrentVariableName { get; private set; }
    public double CurrentVariableMin { get; private set; }
    public double CurrentVariableMax { get; private set; }
    public TimeStepDataAsset CurrentTimeStepDataAsset { get; private set; }
    private IMetaDataManager m_MetaDataManager;

    public TimestampUI m_TimestampUI;

    private void Start()
    {
        this.Clear();

        System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo( "en-US" );
    }

    // Call to clean up when a project is closed
    private void Clear()
    {
        m_MetaDataManager = new MetaDataManager();
        m_DataAssets = new Dictionary<string, List<TimeStepDataAsset>>();
        CurrentVariableName = "";
        CurrentVariableMin = 0;
        CurrentVariableMax = 0;
        MetaData = null;

        m_TimelineUI.Show( false );
        m_CameraModeUI.Show( false );
        m_DataTypeUI.Show( false );
        Singleton.GetVolumeRenderer().Show( false );
    }

    // Start internal routines to import the data to a new project
    public void ImportData( string _projectFilePath, IProgress<float> _progress, Action _callback )
    {
        this.Clear();
        this.StartCoroutine( this.ImportDataCoroutine( _projectFilePath, _progress, _callback ) );
    }

    // Start internal routines to save the settings of an open project
    public void SaveProject( string _projectFileName, string _projectFolderPath, bool _saveOnlyXml, IProgress<float> _progress, Action _callback )
    {
        this.StartCoroutine( this.SaveProjectCoroutine( _projectFileName, _projectFolderPath, _saveOnlyXml, _progress, _callback ) );
    }

    // Start internal routines to load an existing project
    public void LoadProject( string _projectFilePath, IProgress<float> _progress, Action _callback )
    {
        this.Clear();
        this.StartCoroutine( this.LoadProjectCoroutine( _projectFilePath, _progress, _callback ) );
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

        this.SetCurrentAsset( this.CurrentDataAssets.First() );

        // Set new data
        Singleton.GetVolumeRenderer().SetData( this.CurrentTimeStepDataAsset );

        m_TimestampUI.UpdateTimestamp( m_TimestampUI.CurrentIndex );
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

                m_MetaDataManager.Write( filePath, this.MetaData, m_DataAssets );

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

            string variablePath = Path.Combine( Globals.SAVE_PROJECTS_PATH, this.MetaData.DataName, variable.Name );

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

    private IEnumerator ImportDataCoroutine( string _projectFilePath, IProgress<float> _progress, Action _callback )
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Read in meta data
        IMetaData metaData = null;
        Utils.BitDepth bitDepth = Utils.BitDepth.Depth8;
        try
        {
            metaData = m_MetaDataManager.Import( _projectFilePath );
            bitDepth = Utils.GetBitDepth( metaData );
        }
        catch( Exception e )
        {
            Log.ThrowException( this, "Failed to read meta data: " + _projectFilePath + " with exception:\n " + e.GetType().Name + "-" + e.Message );
            _callback?.Invoke();
        }

        IDataLoader tiffLoader = new DataLoaderFromTIFFs( metaData.Width, metaData.Height, metaData.Levels );
        ITimeStepDataAssetBuilder timeAssetBuilder = new TimeStepDataAssetBuilder( metaData.Width, metaData.Height, metaData.Levels );

        int numberOfVariables = metaData.Variables.Count;

        Log.Info( this, "Found " + numberOfVariables + " variables" );

        for( int i = 0; i < numberOfVariables; i++ )
        {
            IVariable variable = metaData.Variables[ i ];

            // For each variable there is a list of all textures that are used
            m_DataAssets[variable.Name] = new List<TimeStepDataAsset>();

            string folder = Path.Combine( Path.GetDirectoryName( _projectFilePath ), variable.Name.ToLower() );
            Log.Info( this, "Create folders and files for variable " + variable.Name );

            if( Directory.Exists( folder ) )
            {
                yield return this.StartCoroutine( this.ImportVariableRoutine( tiffLoader, timeAssetBuilder, folder, m_DataAssets[variable.Name], bitDepth, new Progress<float>( value =>
              {
                  // Do overall progress report
                  _progress.Report( Utils.CalculateProgress( i, numberOfVariables, value ) );
              } ) ) );
            }
            else
            {
                Log.ThrowValueNotFoundException( this, variable.Name );
                _callback?.Invoke();
            }
        }

        Log.Info( this, "Loading and creating assets took " + ( stopwatch.ElapsedMilliseconds / 1000.0f ).ToString( "0.00" ) + "seconds." );

        this.FinishLoading( metaData, stopwatch );

        _callback?.Invoke();
    }

    private IEnumerator LoadProjectCoroutine( string _projectFilePath, IProgress<float> _progress, Action _callback )
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        m_DataAssets.Clear();

        // Read in meta data
        IMetaData metaData = null;
        Utils.BitDepth bitDepth = Utils.BitDepth.Depth8;

        try
        {
            metaData = m_MetaDataManager.Load( _projectFilePath );
            bitDepth = Utils.GetBitDepth( metaData );
        }
        catch( Exception e )
        {
            Log.ThrowException( this, "Failed to read meta data: " + _projectFilePath + " with exception:\n " + e.GetType().Name + "-" + e.Message );
            _callback?.Invoke();
        }

        ITimeStepDataAssetBuilder timeAssetBuilder = new TimeStepDataAssetBuilder( metaData.Width, metaData.Height, metaData.Levels );

        bool isSuccess = true;

        for( int i = 0; i < metaData.Variables.Count; i++ )
        {
            IVariable variable = metaData.Variables[ i ];

            // For each variable there is a list of all textures that are used
            m_DataAssets[variable.Name] = new List<TimeStepDataAsset>();

            string projectAssetPath = Globals.SAVE_PROJECTS_PATH + metaData.DataName;

            string variableFolderPath = Path.Combine( projectAssetPath, variable.Name.ToLower() );
            if( Directory.Exists( projectAssetPath ) )
            {
                yield return this.StartCoroutine( this.LoadVariableRoutine( variableFolderPath, timeAssetBuilder, m_DataAssets[variable.Name], variable, bitDepth, new Progress<float>( value =>
              {
                  // Do overall progress report
                  _progress.Report( Utils.CalculateProgress( i, metaData.Variables.Count, value ) );
              } ) ) );
            }
            else
            {
                isSuccess = false;
                Log.Warn( this, "The directory " + projectAssetPath + " does not exist. The project cannot be loaded." );

                _callback?.Invoke();
            }
        }

        if( !isSuccess )
        {
            this.Clear();
            yield break;
        }
        else
        {
            this.FinishLoading( metaData, stopwatch );
        }

        _callback?.Invoke();
    }

    private void FinishLoading( IMetaData _metaData, System.Diagnostics.Stopwatch _stopwatch )
    {
        Log.Info( this, "Loading and creating assets took " + ( _stopwatch.ElapsedMilliseconds / 1000.0f ).ToString( "0.00" ) + "seconds." );

        this.MetaData = _metaData;
        this.CurrentVariableName = m_DataAssets.First().Key;
        this.CurrentVariableMin = _metaData.Variables[0].Min;
        this.CurrentVariableMax = _metaData.Variables[0].Max;
        this.CurrentTimeStepDataAsset = this.CurrentDataAssets.First();

        // Set new data
        Singleton.GetVolumeRenderer().SetData( this.CurrentTimeStepDataAsset );
        m_TransferFunctionUI.Redraw();

        OnNewImport?.Invoke();

        m_TimestampUI.UpdateTimestamp( m_TimestampUI.CurrentIndex );
    }

    private IEnumerator SaveVariableRoutine( IVariable _variable, string _variablePath, int _varIndex, IProgress<float> _progress )
    {
        string textureAssetPath = Path.Combine( _variablePath, Globals.TEXTURE3D_FOLDER_NAME );
        // Only create if it does not exist, yet
        if( !Directory.Exists( textureAssetPath ) )
        {
            Directory.CreateDirectory( textureAssetPath );
        }

        List<TimeStepDataAsset> currentVariableTimeStepList = m_DataAssets[ _variable.Name ];

        for( int i = 0; i < currentVariableTimeStepList.Count; i++ )
        {
            string dateTimeString = this.MetaData.Timestamps[ _varIndex ][ i ].DateTimeDouble.ToString().Replace( ',', '.' );
            string assetName = Globals.TEXTURE3D_PREFEX + this.MetaData.DataName + "_" + _variable.Name + "_" + dateTimeString;
            string assetPath = textureAssetPath + "/";


            string assetCompleteName = assetPath + assetName + ".asset";

            Log.Info( this, "Create asset " + assetCompleteName );

            TimeStepDataAsset asset = currentVariableTimeStepList[ i ];
            AssetDatabase.CreateAsset( asset.DataTexture, assetCompleteName );

            // Report progress
            _progress.Report( Utils.CalculateProgress( i, currentVariableTimeStepList.Count ) );

            yield return null;
        }
        yield return null;
    }


    private IEnumerator LoadVariableRoutine( string _variableFolder, ITimeStepDataAssetBuilder _timestepDataAssetBuilder, List<TimeStepDataAsset> _timestepDataAssetList, IVariable variable, Utils.BitDepth _bitDepth, IProgress<float> _progress )
    {
        string assetPath = Path.Combine( _variableFolder, Globals.TEXTURE3D_FOLDER_NAME );

        Log.Info( this, "Look for assets at " + assetPath );

        string[] assets = Directory.GetFiles( assetPath );

        Log.Info( this, "Found " + assets.Length + " assets" );

        Texture3D asset = null;
        int assetFileIndex = 0;
        foreach( string file in assets )
        {
            if( file.EndsWith( ".asset" ) )
            {
                Log.Info( this, "Found " + file );

                asset = AssetDatabase.LoadAssetAtPath<Texture3D>( file );

                TimeStepDataAsset timestepAsset = _timestepDataAssetBuilder.BuildTimestepDataAssetFromTexture( asset );

                _timestepDataAssetList.Add( timestepAsset );

                assetFileIndex++;

                // Report progress
                _progress.Report( Utils.CalculateProgress( assetFileIndex, assets.Length ) );
                yield return null;
            }
        }
        Log.Info( this, "Found " + assetFileIndex + " assets" );

        yield return null;
    }

    private IEnumerator ImportVariableRoutine( IDataLoader _loader, ITimeStepDataAssetBuilder _timestepDataAssetBuilder, string _projectFolder, List<TimeStepDataAsset> _timestepDataAssetList, Utils.BitDepth _bitDepth, IProgress<float> _progress )
    {
        // We assume every directory is a time stamp which contains the level tiffs
        string[] directories = Directory.GetDirectories( _projectFolder );

        Log.Info( this, "Start variable importing routine" );

        for( int i = 0; i < directories.Length; i++ )
        {
            string directory = directories[ i ];

            Log.Info( this, "Creating asset from image from " + directory );

            TimeStepDataAsset asset = _timestepDataAssetBuilder.BuildTimestepDataAssetFromData( _loader.ImportImageFiles( directory ) );

            _timestepDataAssetList.Add( asset );

            // Report progress
            float progression = ( i + 1 ) / ( float ) directories.Length;
            _progress.Report( progression );
            yield return null;
        }

        yield return null;
    }

    public void DisableVolumeRenderer()
    {
        Singleton.GetVolumeRenderer().Show( false );
    }

    public void EnableVolumeRenderer()
    {
        Singleton.GetVolumeRenderer().Show( true );
    }
}
