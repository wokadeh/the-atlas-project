﻿public interface IDataLoader {
    byte[][] Import(string _filePath, string _fileName );

    string[] GetDataSet( string _path );
}
