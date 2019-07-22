using System.Collections.Generic;

public interface IMetaDataManager {
    IMetaData Read(string _fileName);
    void Write(string _projectFilePath, IMetaData _metaData, Dictionary<string, List<EarthDataFrame>> _dataAssets);
}
