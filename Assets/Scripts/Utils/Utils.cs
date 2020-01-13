/*
    Note:   This class is meant to be a home for little helper functions to clean up the 
            important classes!

*/
using BitMiracle.LibTiff.Classic;
using System.Text.RegularExpressions;
using System;
using System.Xml;
using System.Globalization;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections;

public static class Utils
{
    public enum BitDepth
    {
        Depth8,
        Depth16
    }
    public static BitDepth GetBitDepth( IMetaData _metaData )
    {
        switch( _metaData.BitDepth )
        {
            case 8: return BitDepth.Depth8;
            case 16: return BitDepth.Depth16;
            default: throw new Exception( "Failed to determine bit depth!" );
        }
    }
    public static string TryConvertDoubleToDateTimeString( double dateTimeNumber )
    {
        try
        {
            DateTime dateTime = Utils.ConvertDoubleToDateTime( dateTimeNumber );

            try
            {
                string dateTimeString = dateTime.ToString();
                return dateTimeString;
            }
            catch( Exception e )
            {
                Debug.LogWarning( "ToString of dateTime " + dateTimeNumber + " failed: " + e );
            }
        }
        catch( Exception e )
        {
            Debug.LogWarning( "Conversion of double " + dateTimeNumber + " failed: " + e );
        }

        return "NaN";
    }

    private static DateTime ConvertDoubleToDateTime( double varTime )
    {
        return DateTime.FromOADate( varTime - Globals.DATE_FIX_NUMBER );
    }

    public static float CalculateProgress( int _index, int _maximum, float _value )
    {
        float progression = _index / ( float ) _maximum;
        return progression + ( _value / _maximum );
    }

    public static float CalculateProgress( int _index, int _maximum )
    {
        return _index / ( float ) _maximum;
    }

    public static int ReadIntegerAttribute( XmlElement _relement, string _name )
    {
        int attribute;
        if( _relement.HasAttribute( _name ) )
        {
            if( !int.TryParse( _relement.GetAttribute( _name ), out attribute ) )
            {
                throw new Log.MetaDataException( $"Failed to read '{_name}' attribute!" );
            }
        }
        else
        {
            throw new Log.MetaDataException( $"Xml file has no '{_name}' attribute!" );
        }
        return attribute;
    }

    public static double ReadDoubleAttribute( XmlElement _relement, string _name )
    {
        double attribute;
        if( _relement.HasAttribute( _name ) )
        {
            attribute = Double.Parse( _relement.GetAttribute( _name ), CultureInfo.InvariantCulture );
        }
        else
        {
            throw new Log.MetaDataException( $"Xml file has no '{_name}' attribute!" );
        }
        return attribute;
    }

    public static float ReadAttribute( XmlNode _relement, string _name )
    {
        float value = 0;
        try
        {
            string valueString = _relement.Attributes[ _name ].Value;
            value = ( float.Parse( valueString, CultureInfo.InvariantCulture.NumberFormat ) );
        }
        catch( Exception )
        {
            // No problem!
        }

        return value;
    }

    public static Progress<float> CreateProgressBarProgress( Image _progressBar, TMP_Text _progressBarText )
    {
        return new Progress<float>( progress =>
         {
             _progressBar.fillAmount = progress;
             _progressBarText.text = $"{( progress * 100 ).ToString( "0" )} %";
         } );
    }

    public static IEnumerator SetupProjectCoroutine( GameObject _projectStateScreen, GameObject _projectScreen, Image _progressBar, TMP_Text _progressBarText )
    {
        _progressBar.fillAmount = 0;
        _progressBarText.text = "0 %";

        _projectScreen.SetActive( false );
        _projectStateScreen.SetActive( true );

        // We are waiting for two frames so that unity has enough time to redraw the ui
        // which apparently it needs or otherwise the positions are off...
        yield return null; // frame 0...
        yield return null; // frame 1...

    }

    public static void SetupScreenWhileProgress( GameObject _projectStateScreen, GameObject _mainSystemScreen, GameObject _bottomScreen, Button _saveProjectButton, Button _cancelButton )
    {
        _projectStateScreen.SetActive( false );
        _mainSystemScreen.SetActive( true );
        _bottomScreen.SetActive( true );

        _saveProjectButton.interactable = true;
        Singleton.GetVolumeRenderer().gameObject.SetActive( true );
        _cancelButton.interactable = true;
    }

    public static void SetupScreenWhileProgress( GameObject _projectStateScreen, GameObject _mainSystemScreen, GameObject _bottomScreen, Button _saveProjectButton, Button _saveProjectAsButton, Button _cancelButton )
    {
        SetupScreenWhileProgress( _projectStateScreen , _mainSystemScreen, _bottomScreen, _saveProjectButton, _cancelButton );

        _saveProjectButton.interactable = false;
        _saveProjectAsButton.interactable = true;
    }

    public static void ToggleItemsOnClick( bool _isOn, Toggle _toggle, Transform _transform )
    {
        Toggle[] toggles = _transform.GetComponentsInChildren<Toggle>();
        if( toggles.Length > 1 )
        {
            foreach( Toggle t in toggles )
            {
                if( t != _toggle )
                {
                    t.isOn = false;
                }
            }
        }
    }

    public static byte[] ConvertBufferToBytes( byte[][] _buffer )
    {
        byte[] texture3dBuffer = new byte[_buffer.Length * _buffer[0].Length];
        Log.Info( "Utils", "The buffer has a size of " + texture3dBuffer.Length.ToString() + " because _buffer.Length is " + _buffer.Length  + " and  _buffer[0].Length is " + _buffer[0].Length );

        for( int level = 0; level < _buffer.Length; level++ )
        {
            for( int size = 0; size < _buffer[0].Length; size++ )
            {
                int index = size * _buffer.Length + level;
                texture3dBuffer[index] = Utils.GetByteFromTIFF( _buffer[level][size] );
            }
        }

        return texture3dBuffer;
    }

    public static byte[][] CreateEmptyBuffer( int _levels, int _size )
    {
        byte[][] buffer = new byte[_levels][];
        // Create empty buffer
        for( int i = 0; i < _levels; i++ )
        {
            buffer[i] = new byte[_size];
        }

        return buffer;
    }

    public static byte[][] ConvertBytesToBuffer( byte[][] _buffer, int[] _raster, int _level, int _width, int _height )
    {
        // We convert the raster to bytes
        for( int x = 0; x < _width; x++ )
        {
            for( int y = 0; y < _height; y++ )
            {
                int index= y * _width + x;

                _buffer[_level][index] = Utils.GetByteFromTIFF( _raster[index] );
            }
        }

        return _buffer;
    }

    public static byte[][] ConvertBytesToBuffer( byte[][] _buffer, byte[] _raster, int _levelMin, int _levelMax, int _width, int _height )
    {
        Log.Debg( "Utils", "LEVEL_MIN is " + _levelMin + ", LEVEL_MAX is " + _levelMax + ", width is " + _width + ", height is " + _height + ", _buffer.Length is " + _buffer.Length.ToString() + " and _buffer[0].Length is " + _buffer[0].Length + " and _raster.Length is " + _raster.Length );

        int max = _levelMax;
        if( _levelMax == _levelMin )
        {
            max = _levelMax + 1;
        }
        // We convert the raster to bytes
        for( int l = _levelMin; l < max; l++ )
        {
            for( int x = 0; x < _width; x++ )
            {
                for( int y = 0; y < _height; y++ )
                {
                    int index = y * _width + x;
                    int ix = index * _levelMax + l;

                    if( _levelMin == _levelMax )
                    {
                        try
                        {
                            // We only have one level that we write into index 0. A 1D array could have been used, too
                            _buffer[0][index] = _raster[ix];
                        }
                        catch(Exception e)
                        {
                            Log.Warn( "Utils", "LevelMin is not 0. Exception in writing texture _buffer[][]! index/max is " + index + "/" + _buffer[0].Length + " and _raster[] has ix/max " + ix + "/" + _raster.Length + ".... " + e);
                            Log.ThrowException( "Utils", "Stop" );
                        }
                    }
                    else
                    {
                        try
                        {
                            // LevelMin == 0 means we show all levels so we fill the first dimension with the index of the level, start with 1
                            _buffer[l][index] = _raster[ix];
                        }
                        catch( Exception e )
                        {
                            Log.Warn( "Utils", "LevelMin is 0. Exception in writing texture _buffer[][]! index/max is " + index + "/" + _buffer[l].Length + " and _raster[] has ix/max " + ix + "/" + _raster.Length + ".... " + e );
                            Log.ThrowException( "Utils", "Stop" );
                        }
                    }
                }
            }
        }

        return _buffer;
    }

    public static byte GetByteFromTIFF( int _bytes )
    {
        // It is not important from which channel (r,g,b) we take the byte
        // as all three contain the same for a tiff with a bit depth of 8
        return ( byte )Tiff.GetR( _bytes );
    }

    public static string PadNumbers( string _input )
    {
        return Regex.Replace( _input, "[0-9]+", match => match.Value.PadLeft( 10, '0' ) );
    }
}
