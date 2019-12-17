using System.Collections.Generic;
using System.IO;

public class MetaDataManager : IMetaDataManager
{
    public class MetaData : IMetaData
    {
        public string DataName { get; set; }
        public int BitDepth { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Levels { get; set; }
        public double StartDateTimeNumber { get; set; }
        public double EndDateTimeNumber { get; set; }
        public int TimeInterval { get; set; }

        public IList<IVariable> Variables { get; set; }
        public IList<IList<TimeStepDataAsset>> Timesteps { get; set; }
    }

    public class Variable : IVariable
    {
        public string Name { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
    }

    public class Timestamp : ITimestamp
    {
        public string DateTime { get; set; }
    }

    public IMetaData SetMetaDataName( string _projectName, IMetaData _metaData )
    {
        MetaData newMetaData = new MetaData();

        newMetaData.DataName = _projectName;

        newMetaData.BitDepth = _metaData.BitDepth;
        newMetaData.Width = _metaData.Width;
        newMetaData.Height = _metaData.Height;
        newMetaData.Levels = _metaData.Levels;
        newMetaData.StartDateTimeNumber = _metaData.StartDateTimeNumber;
        newMetaData.EndDateTimeNumber = _metaData.EndDateTimeNumber;
        newMetaData.TimeInterval = _metaData.TimeInterval;

        newMetaData.Variables = _metaData.Variables;
        newMetaData.Timesteps = _metaData.Timesteps;

        return newMetaData;
    }

    public void Write( string _projectFilePath, IMetaData _metaData, Dictionary<string, List<TimeStepDataAsset>> _dataAssets )
    {
        MetaDataWriter.Write( _projectFilePath, _metaData, _dataAssets );
    }

    public IMetaData Load( string _projectFilePath )
    {
        return MetaDataReader.Import( _projectFilePath );
    }

    public IMetaData Import( string _projectFilePath )
    {
        return MetaDataReader.Import( _projectFilePath );
    }

    public void SaveProjectXMLFile( IMetaData _metaData, Dictionary<string, List<TimeStepDataAsset>> _dataDictionary )
    {
        string directoryPath = Path.Combine( Globals.SAVE_PROJECTS_PATH, _metaData.DataName );

        if( !Directory.Exists( directoryPath ) )
        {
            Directory.CreateDirectory( directoryPath );
        }

        string filePath = Path.Combine( directoryPath, _metaData.DataName + ".xml" );

        Log.Info( this, "Try to write new imported project XML file to path " + filePath );
        this.Write( filePath, _metaData, _dataDictionary );

        Log.Info( this, "Successfully wrote project XML to: " + filePath );
    }
}
