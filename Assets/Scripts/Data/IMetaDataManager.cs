using System.Collections.Generic;

public interface IMetaDataManager {
    IMetaData Read(string _fileName);
    IMetaData Write(string _projectFilePath, MetaDataManager.MetaData _metaData, Dictionary<string, List<EarthDataFrame>> _dataAssets);
}
