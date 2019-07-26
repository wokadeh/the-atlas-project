using System.Collections.Generic;

public interface IMetaDataManager {
    IMetaData Import(string _fileName);
    void Write(string _projectFilePath, IMetaData _metaData, Dictionary<string, List<EarthDataFrame>> _dataAssets);
}
