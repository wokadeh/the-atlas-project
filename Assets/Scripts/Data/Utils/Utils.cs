using System;
using System.Xml;
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

    public static float ReadFloatAttribute(XmlElement _relement, string _name)
    {
        float attribute;
        if (_relement.HasAttribute(_name))
        {
            if (!float.TryParse(_relement.GetAttribute(_name), out attribute))
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
 