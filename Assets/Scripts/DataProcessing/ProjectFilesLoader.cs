using System.IO;

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

    public byte[][] Import( string _filePath, string _justImplemented )
    {
        Log.Info( this, "Load project files from " + _filePath );

        byte[] timestepBytes;
        
        m_Buffer = Utils.CreateEmptyBuffer( m_MetaData.Levels, m_MetaData.Width * m_MetaData.Height );

        // Load file into byte array
        timestepBytes = File.ReadAllBytes( _filePath );
        m_Buffer = Utils.ConvertBytesToBuffer( m_Buffer, timestepBytes, m_MetaData.Levels, m_MetaData.Width, m_MetaData.Height );

        return m_Buffer;
    }

    public string[] GetDataSet( string _path )
    {
        return Directory.GetFiles( _path );
    }
}
