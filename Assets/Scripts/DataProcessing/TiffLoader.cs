using BitMiracle.LibTiff.Classic;
using System;
using System.IO;
using System.Linq;

public class TiffLoader : IDataLoader
{
    private class TiffException : Exception
    {
        public TiffException( string message ) : base( message ) { }
    }

    private IMetaData m_MetaData;

    // For performance reasons these arrays are reused and therefore refilled with new data when calling "Load" again
    private int[] m_NumberGrid;
    private byte[][] m_Buffer;

    public TiffLoader( IMetaData _metaData )
    {
        m_MetaData = _metaData;

        // The raster and buffer for tiff loading can be reused and therefore need only to be created once
        int size = m_MetaData.Width * m_MetaData.Height;
        m_NumberGrid = new int[size];

        m_Buffer = Utils.CreateEmptyBuffer( m_MetaData.Levels, size );
    }

    public byte[][] Import( string _filePath, string _fileName )
    {
        Log.Info( this, "Import TIFF files from " + _filePath );

        // Get all tiffs in the directory and sort them appropriately
        string[] files = Directory.GetFiles( _filePath, "*.*" ).Where( p => p.EndsWith( ".tif" ) || p.EndsWith( ".tiff" ) ).OrderBy( s => Utils.PadNumbers( s ) ).ToArray();

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

        this.ImportAllImages( files );

        this.SaveBytesToFile( Path.Combine( m_MetaData.DataName, _fileName ), Path.GetFileName( _filePath ) );

        return m_Buffer;
    }

    private void ImportAllImages(string[] files)
    {
        // Import in all images
        for( int level = 0; level < files.Length; level++ )
        {
            string filePath = files[ level ];
            try
            {
                this.ConvertImageToBuffer( filePath, level );
            }
            catch( Exception e )
            {
                Log.Error( this, "Failed to read tiff: " + filePath + " with exception:\n" + e.GetType().Name + " " + e.Message );
            }
        }
    }

    private void ConvertImageToBuffer( string _filePath, int _level )
    {
        using( Tiff image = Tiff.Open( _filePath, "r" ) )
        {
            // Find the width and height of the image
            FieldValue[] value = image.GetField( TiffTag.IMAGEWIDTH );
            int width = value[ 0 ].ToInt();

            value = image.GetField( TiffTag.IMAGELENGTH );
            int height = value[ 0 ].ToInt();

            if( TestImageFile( image, width, height ) )
            {
                m_Buffer = Utils.ConvertBytesToBuffer( m_Buffer, m_NumberGrid, _level, width, height );
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
        if( !_image.ReadRGBAImage( _width, _height, m_NumberGrid ) )
        {
            Log.Warn( this, "Failed to read pixels from tiff!" );
            return false;
        }
        return true;
    }

    private void SaveBytesToFile( string _varName, string _fileName )
    {
        // Create 1D buffer that can be saved
        // buffer.length == #level, buffer.length[0] == width * height
        byte[] texture3dBuffer = Utils.ConvertBufferToBytes( m_Buffer );
        
        string savePath = Path.Combine( Globals.SAVE_PROJECTS_PATH, _varName );
        if( !Directory.Exists( savePath ) )
        {
            Directory.CreateDirectory( savePath );
        }

        Log.Info( this, "Write file " + _fileName + " to path " + savePath );
        string filePath = Path.Combine(savePath, _fileName + Globals.SAVE_TIMESTEP_SUFFIX);

        Log.Info( this, "The file " + _fileName + " has a size of " + texture3dBuffer.Length.ToString() );

        if( !File.Exists( filePath ) )
        {
            // Write to Save Projects path
            File.WriteAllBytes( filePath, texture3dBuffer );
        }
        else
        {
            Log.Warn( this, "The file " + filePath + " already exists!" );
        }
    }
}
