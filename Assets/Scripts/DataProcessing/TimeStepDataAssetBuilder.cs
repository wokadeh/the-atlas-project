using System;
using UnityEngine;

public class TimeStepDataAssetBuilder : ITimeStepDataAssetBuilder
{


    private int m_Width;
    private int m_Height;
    private int m_Depth;

    private Color[] m_ColorBuffer3D;
    private Color[] m_ColorBuffer2D;

    private int[] m_HistogramValuesBuffer;
    private Color[] m_HistogramColorBuffer;

    private int m_HistogramSampleSize = 1;

    public TimeStepDataAssetBuilder( int width, int height, int depth )
    {
        m_Width = width;
        m_Height = height;
        m_Depth = depth;

        m_HistogramSampleSize = 2 ^ depth;

        m_ColorBuffer3D = new Color[ m_Width * m_Height * m_Depth ];
        m_ColorBuffer2D = new Color[ m_Width * m_Height ];

        m_HistogramValuesBuffer = new int[ m_HistogramSampleSize ];
        m_HistogramColorBuffer = new Color[ m_HistogramSampleSize ];
    }

    public TimeStepDataAsset BuildTimestepDataAssetFromData( byte[][] data )
    {
        int depth = data.Length;

        return new TimeStepDataAsset() { Dimensions = new Vector3( m_Width, m_Height, depth ), DataTexture = this.BuildDataTexture( data ), HistogramTexture = this.BuildHistogramTexture() };
    }

    public TimeStepDataAsset BuildTimestepDataAssetFromTexture( Texture3D _assetTex )
    {
        int depth = _assetTex.depth;

        // We need to fill the color buffer to create the histogram
        m_ColorBuffer3D = _assetTex.GetPixels();

        return new TimeStepDataAsset() { Dimensions = new Vector3( m_Width, m_Height, depth ), DataTexture = _assetTex, HistogramTexture = this.BuildHistogramTexture() };
    }

    public TimeStepDataAsset Build16Bit( short[][] data )
    {
        throw new NotImplementedException();
    }

    private Texture3D BuildDataTexture( byte[][] data )
    {
        int size2d = m_Width * m_Height;

        // Get color data from all textures
        for ( int i = 0; i < data.Length; i++ )
        {
            // Fill 2d color buffer with data
            for ( int x = 0; x < m_Width; x++ )
            {
                for ( int y = 0; y < m_Height; y++ )
                {
                    int index = y * m_Width + x;
                    byte value = data[ i ][ index ];
                    m_ColorBuffer2D[ index ] = new Color32( value, 0, 0, 0 );
                }
            }

            m_ColorBuffer2D.CopyTo( m_ColorBuffer3D, i * size2d );
        }

        Texture3D dataTexture = new Texture3D( m_Width, m_Height, m_Depth, TextureFormat.R8, false );
        dataTexture.SetPixels( m_ColorBuffer3D );
        dataTexture.Apply();

        return dataTexture;
    }

    private Texture2D BuildHistogramTexture()
    {
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
}
