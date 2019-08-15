using System.Collections.Generic;

public interface IMetaDataManager
{
    IMetaData SetMetaDataName( string _projectName, IMetaData _metaData );
    IMetaData Import( string _fileName );
    IMetaData Load( string _fileName );
    void Write( string _projectFilePath, IMetaData _metaData, Dictionary<string, List<TimeStepDataAsset>> _dataAssets );
}
