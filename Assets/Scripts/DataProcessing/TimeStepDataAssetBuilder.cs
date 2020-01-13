using System;
using UnityEngine;

public class TimeStepDataAssetBuilder : ITimeStepDataAssetBuilder
{


    private int m_Width;
    private int m_Height;
    private int m_Levels;

    private Color[] m_ColorBuffer3D;
    private Color[] m_ColorBuffer2D;

    private int[] m_HistogramValuesBuffer;
    private Color[] m_HistogramColorBuffer;

    private int m_HistogramSampleSize = 1;

    public TimeStepDataAssetBuilder( int _width, int _height, int _levels )
    {
        m_Width = _width;
        m_Height = _height;
        m_Levels = _levels;

        m_HistogramSampleSize = 2 ^ _levels;

        m_ColorBuffer3D = new Color[ m_Width * m_Height * m_Levels ];
        m_ColorBuffer2D = new Color[ m_Width * m_Height ];

        m_HistogramValuesBuffer = new int[ m_HistogramSampleSize ];
        m_HistogramColorBuffer = new Color[ m_HistogramSampleSize ];
    }

    public TimeStepDataAsset BuildTimestepDataAssetFromData( byte[][] _data )
    {
        if( _data == null )
        {
            Log.ShowDialogBox( "No image files could be found, so unfortunately the project cannot be imported! " );
            return null;
        }

        if( _data.Length == 1 )
        {
            Log.Debg( this, "Build 2D texture from 2D data!" );
            return new TimeStepDataAsset() { Dimensions = new Vector3( m_Width, m_Height, _data.Length ), DataTexture2D = this.ConvertBytesToTexture( _data[0], m_Width, m_Height ), HistogramTexture = this.BuildHistogramTextureFrom2D() };
        }
        else
        {
            Log.Debg( this, "Build 3D texture from 3D data!" );
            return new TimeStepDataAsset() { Dimensions = new Vector3( m_Width, m_Height, _data.Length ), DataTexture3D = this.ConvertBytesToTexture( _data, m_Width, m_Height, m_Levels ), HistogramTexture = this.BuildHistogramTextureFrom3D() };
        }
    }

    public TimeStepDataAsset BuildTimestepDataAssetFromTexture( Texture3D _assetTex )
    {
        int depth = _assetTex.depth;

        // We need to fill the color buffer to create the histogram
        m_ColorBuffer3D = _assetTex.GetPixels();

        return new TimeStepDataAsset() { Dimensions = new Vector3( m_Width, m_Height, depth ), DataTexture3D = _assetTex, HistogramTexture = this.BuildHistogramTextureFrom3D() };
    }

    private Texture2D BuildHistogramTextureFrom3D()
    {
        Log.Debg( this, "Start building histogram with sample size " + m_HistogramSampleSize + " and ColorBuffer3D size is " + m_ColorBuffer3D.Length );

        Texture2D histogram = new Texture2D( m_HistogramSampleSize, 1, TextureFormat.RFloat, false );

        int maxFrequency = 0;
        for ( int i = 0; i < m_ColorBuffer3D.Length; i++ )
        {
            int value = ( int ) ( m_ColorBuffer3D[ i ].r * ( m_HistogramSampleSize - 1 ) );
            m_HistogramValuesBuffer[ value ] += 1;
            maxFrequency = Mathf.Max( maxFrequency, m_HistogramValuesBuffer[ value ] );
        }

        for ( int i = 0; i < m_HistogramSampleSize; i++ )
        {
            float value = Mathf.Log10( ( float ) m_HistogramValuesBuffer[ i ] ) / Mathf.Log10( ( float ) maxFrequency );
            m_HistogramColorBuffer[ i ] = new Color( value, 0.0f, 0.0f, 1.0f );
        }

        histogram.SetPixels( m_HistogramColorBuffer );
        histogram.Apply();

        return histogram;
    }

    private Texture2D BuildHistogramTextureFrom2D()
    {
        Log.Debg( this, "Start building histogram with sample size " + m_HistogramSampleSize + " and ColorBuffer2D size is " + m_ColorBuffer2D.Length );

        Texture2D histogram = new Texture2D( m_HistogramSampleSize, 1, TextureFormat.RFloat, false );

        int maxFrequency = 0;
        for( int i = 0; i < m_ColorBuffer2D.Length; i++ )
        {
            int value = ( int ) ( m_ColorBuffer2D[ i ].r * ( m_HistogramSampleSize - 1 ) );
            m_HistogramValuesBuffer[value] += 1;
            maxFrequency = Mathf.Max( maxFrequency, m_HistogramValuesBuffer[value] );
        }

        for( int i = 0; i < m_HistogramSampleSize; i++ )
        {
            float value = Mathf.Log10( ( float ) m_HistogramValuesBuffer[ i ] ) / Mathf.Log10( ( float ) maxFrequency );
            m_HistogramColorBuffer[i] = new Color( value, 0.0f, 0.0f, 1.0f );
        }

        histogram.SetPixels( m_HistogramColorBuffer );
        histogram.Apply();

        return histogram;
    }

    public Texture3D ConvertBytesToTexture( byte[][] _buffer, int _width, int _height, int _levels )
    {
        int size2d = _width * _height;

        // Get color data from all textures
        for( int i = 0; i < _buffer.Length; i++ )
        {
            // Fill 2d color buffer with data
            for( int x = 0; x < _width; x++ )
            {
                for( int y = 0; y < _height; y++ )
                {
                    int index = y * _width + x;
                    byte value = _buffer[ i ][ index ];
                    m_ColorBuffer2D[ index ] = new Color32( value, 0, 0, 0 );
                }
            }

            m_ColorBuffer2D.CopyTo( m_ColorBuffer3D, i * size2d );
        }

        Texture3D dataTexture = new Texture3D( _width, _height, _levels, TextureFormat.R8, false );
        dataTexture.SetPixels( m_ColorBuffer3D );
        dataTexture.Apply();

        return dataTexture;
    }

    public Texture2D ConvertBytesToTexture( byte[] _buffer, int _width, int _height )
    {
        // Fill 2d color buffer with data
        for( int x = 0; x < _width; x++ )
        {
            for( int y = 0; y < _height; y++ )
            {
                int index = y * _width + x;
                byte value = _buffer[ index ];
                m_ColorBuffer2D[ index ] = new Color32( value, 0, 0, 0 );
            }
        }

        Texture2D dataTexture = new Texture2D( _width, _height, TextureFormat.R8, false );
        dataTexture.SetPixels( m_ColorBuffer3D );
        dataTexture.Apply();

        return dataTexture;
    }
}
