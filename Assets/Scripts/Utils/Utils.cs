﻿/*
    Note:   This class is meant to be a home for little helper functions to clean up the 
            important classes!

*/

using System;
using System.Xml;
using System.Globalization;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public static class Utils
{
    public enum BitDepth
    {
        Depth8,
        Depth16
    }

    public static BitDepth GetBitDepth(IMetaData _metaData)
    {
        switch (_metaData.BitDepth)
        {
            case 8: return BitDepth.Depth8;
            case 16: return BitDepth.Depth16;
            default: throw new Exception("Failed to determine bit depth!");
        }
    }

    public static string TryConvertDoubleToDateTime( double dateTimeNumber )
    {
        try
        {
            DateTime dateTime = Utils.ConvertDoubleToDateTime( dateTimeNumber );

            try
            {
                string dateTimeString = dateTime.ToString();
                return dateTimeString;
            }
            catch(Exception e)
            {
                Debug.LogWarning( "ToString of dateTime " + dateTimeNumber + " failed: " + e );
            }
        }
        catch(Exception e)
        {
            Debug.LogWarning( "Conversion of double " + dateTimeNumber + " failed: " + e);
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

    public static int ReadIntegerAttribute(XmlElement _relement, string _name)
    {
        int attribute;
        if (_relement.HasAttribute(_name))
        {
            if (!int.TryParse(_relement.GetAttribute(_name), out attribute))
            {
                throw new Log.MetaDataException($"Failed to read '{_name}' attribute!");
            }
        }
        else
        {
            throw new Log.MetaDataException($"Xml file has no '{_name}' attribute!");
        }
        return attribute;
    }

    public static void ReadXml()
    {
        XmlDocument document = new XmlDocument();
        document.Load( "C:\\Users\\wokad\\Documents\\Projects\\Uniy_Clouds_n_Bones\\Data\\2018-01-00061218-1-1000_sdfdsf_8bit\\2018-01-00061218-1-1000_sdfdsf_8bit___META_DATA___.xml" );
        XmlElement root = document.DocumentElement;

        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo( "en-US" );

        string startTime = root.Attributes[ "start_datetime" ].Value;
        Debug.Log( "1st::: Read startTime number: " + startTime );
        double startTimeValue = double.Parse(startTime);
        Debug.Log( "2nd::: Parse startTime number: " + startTimeValue );

        string endTime = root.Attributes[ "end_datetime" ].Value;
        Debug.Log( "1st::: Read endTime number: " + endTime );
        double endTimeValue = double.Parse( endTime, CultureInfo.InvariantCulture );
        Debug.Log( "2nd::: Parse endTime number: " + endTimeValue );
    }

    public static double ReadDoubleAttribute(XmlElement _relement, string _name)
    {
        double attribute;
        if (_relement.HasAttribute(_name))
        {
            attribute = Double.Parse( _relement.GetAttribute( _name ), CultureInfo.InvariantCulture );
            //if (!float.TryParse(_relement.GetAttribute(_name), out attribute))
            //{
            //    throw new Log.MetaDataException($"Failed to read '{_name}' attribute!");
            //}
        }
        else
        {
            throw new Log.MetaDataException($"Xml file has no '{_name}' attribute!");
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

    public static float convertTimeToFloat(string _time)
    {
        return 0;
    }

    public static string convertFloatToString(float _number)
    {
        return "";
    }

    public static Progress<float> CreateProgressBarProgress(Image _progressBar, TMP_Text _progressBarText, GameObject _screen)
    {
        return new Progress<float>(progress =>
        {
            _progressBar.fillAmount = progress;
            _progressBarText.text = $"{(progress * 100).ToString("0")} %";
        });
     }
}
 