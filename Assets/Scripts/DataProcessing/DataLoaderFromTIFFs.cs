using BitMiracle.LibTiff.Classic;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class DataLoaderFromTIFFs : IDataLoader
{
    private class TiffException : Exception
    {
        public TiffException( string message ) : base( message ) { }
    }

    private IMetaData m_MetaData;

    // For performance reasons these arrays are reused and therefore refilled with new data when calling "Load" again
    private int[] m_Raster;
    private byte[][] m_Buffer;

    public DataLoaderFromTIFFs( IMetaData _metaData )
    {
        m_MetaData = _metaData;

        // The raster and buffer for tiff loading can be reused and therefore need only to be created once
        int size = m_MetaData.Width * m_MetaData.Height;
        m_Raster = new int[size];
        m_Buffer = new byte[m_MetaData.Levels][];

        this.CreateEmptyBuffer( m_MetaData.Levels, size );
    }

    private void CreateEmptyBuffer( int _levels, int _size )
    {
        // Create empty buffer
        for( int i = 0; i < _levels; i++ )
        {
            m_Buffer[i] = new byte[_size];
        }
    }

    public byte[][] ImportImageFiles( string _path, string _varName )
    {
        Log.Info( this, "Import TIFF files from " + _path );
        
        // Get all tiffs in the directory and sort them appropriately
        string[] files = Directory.GetFiles( _path, "*.*" ).Where( p => p.EndsWith( ".tif" ) || p.EndsWith( ".tiff" ) ).OrderBy( s => PadNumbers( s ) ).ToArray();

        Log.Info( this, "Found " + files.Length + " TIFF files." );

        if( files.Length == 0 )
        {
            Log.Warn( this, "Could not find any TIFF files!!" );
        }

        // Check the amount of files matches the expected levels
        if( files.Length != m_MetaData.Levels )
        {
            Log.Warn( this, "The amount of " + files.Length + " does not match the number of given " + m_MetaData.BitDepth + " levels from meta file" );
            return null;
        }

        // Import in all images
        for( int i = 0; i < files.Length; i++ )
        {
            string file = files[ i ];
            try
            {
                this.ConvertImageToBuffer( file, i );
            }
            catch( Exception e )
            {
                Log.Error( this, "Failed to read tiff: " + file + " with exception:\n" + e.GetType().Name + " " + e.Message );
            }
        }

        this.SaveBytesToFile( Path.Combine( m_MetaData.DataName, _varName ), Path.GetFileName( _path ) );

        return m_Buffer;
    }

    private void ConvertImageToBuffer( string _path, int _level )
    {
        using( Tiff image = Tiff.Open( _path, "r" ) )
        {
            // Find the width and height of the image
            FieldValue[] value = image.GetField( TiffTag.IMAGEWIDTH );
            int width = value[ 0 ].ToInt();

            value = image.GetField( TiffTag.IMAGELENGTH );
            int height = value[ 0 ].ToInt();

            if( TestImageFile( image, width, height ) )
            {
                this.ReadImageFile( _level, width, height );
            }
        }
    }

    private bool TestImageFile( Tiff _image, int _width, int _height )
    {
        if( _width != m_MetaData.Width )
        {
            Log.Warn( this, "Tiff does not have the expected width of: " + m_MetaData.Width );
            return false;
        }
        if( _height != m_MetaData.Height )
        {
            Log.Warn( this, "Tiff does not have the expected height of " + m_MetaData.Height );
        }

        // Read the image into the raster buffer
        if( !_image.ReadRGBAImage( _width, _height, m_Raster ) )
        {
            Log.Warn( this, "Failed to read pixels from tiff!" );
            return false;
        }
        return true;
    }

    private void ReadImageFile( int _level, int _width, int _height )
    {
        // We convert the raster to bytes
        for( int x = 0; x < _width; x++ )
        {
            for( int y = 0; y < _height; y++ )
            {
                int index= y * _width + x;

                //m_Buffer[index + _level * _width * _height] = this.GetByteFromBytes( m_Raster[index] );
                m_Buffer[_level][index] = this.GetByteFromBytes( m_Raster[index] );
            }
        }
    }

    private void SaveBytesToFile( string _varName, string _fileName )
    {
        // Create 1D buffer that can be saved
        // buffer.length == #level, buffer.length[0] == width * height
        byte[] texture3dBuffer = new byte[m_Buffer.Length * m_Buffer[0].Length];

        for( int x = 0; x < m_Buffer.Length; x++ )
        {
            for( int y = 0; y < m_Buffer[0].Length; y++ )
            {
                int index = y * m_Buffer.Length + x;

                //m_Buffer[index + _level * _width * _height] = this.GetByteFromBytes( m_Raster[index] );
                texture3dBuffer[index] = this.GetByteFromBytes( m_Buffer[x][y] );
            }
        }
        string savePath = Path.Combine( Globals.SAVE_PROJECTS_PATH, _varName );
        if( !Directory.Exists( savePath ) )
        {
            Directory.CreateDirectory( savePath );
        }

        Log.Info( this, "Write file " + _fileName + " to path " + savePath );
        string filePath = Path.Combine(savePath, _fileName + Globals.SAVE_ASSET_SUFFIX);

        if( !File.Exists(filePath))
        {
            // Write to Save Projects path
            File.WriteAllBytes( filePath, texture3dBuffer );
        }
        else
        {
            Log.Warn( this, "The file " + filePath + " already exists!" );
        }
        
    }

    private byte GetByteFromBytes( int _bytes )
    {
        // It is not important from which channel (r,g,b) we take the byte
        // as all three contain the same for a tiff with a bit depth of 8
        return ( byte )Tiff.GetR( _bytes );
    }

    private static string PadNumbers( string _input )
    {
        return Regex.Replace( _input, "[0-9]+", match => match.Value.PadLeft( 10, '0' ) );
    }
}
