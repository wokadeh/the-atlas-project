public interface IDataLoader {
    byte[][] Import( int _level, string _filePath, string _fileName );

    string[] GetDataSet( string _path );
}
