﻿using System.IO;

public class ProjectFilesLoader : IDataLoader
{
    private IMetaData m_MetaData;

    // For performance reasons these arrays are reused and therefore refilled with new data when calling "Load" again
    byte[][] m_Buffer;

    public ProjectFilesLoader( IMetaData _metaData )
    {
        m_MetaData = _metaData;

        // The raster and buffer for tiff loading can be reused and therefore need only to be created once
        int size = m_MetaData.Width * m_MetaData.Height;

        m_Buffer = Utils.CreateEmptyBuffer( m_MetaData.Levels, size );
    }

    public byte[][] Import( int _level, string _filePath, string _justImplemented )
    {
        Log.Info( this, "Load project files from " + _filePath );

        // Load file into byte array
        byte[] timestepBytes = File.ReadAllBytes( _filePath );

        if(_level == 0)
        {
            // all levels
            m_Buffer = Utils.CreateEmptyBuffer( m_MetaData.Levels, m_MetaData.Width * m_MetaData.Height );
            m_Buffer = Utils.ConvertBytesToBuffer( m_Buffer, timestepBytes, 0, m_MetaData.Levels, m_MetaData.Width, m_MetaData.Height );
        }
        else
        {
            // one level only
            m_Buffer = Utils.CreateEmptyBuffer( 1, m_MetaData.Width * m_MetaData.Height );
            m_Buffer = Utils.ConvertBytesToBuffer( m_Buffer, timestepBytes, _level - 1, _level, m_MetaData.Width, m_MetaData.Height );
        }

        return m_Buffer;
    }

    public string[] GetDataSet( string _path )
    {
        return Directory.GetFiles( _path );
    }
}
