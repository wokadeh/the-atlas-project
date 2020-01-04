using TMPro;
using System;
using UnityEngine;

public class Log
{
    public class MetaDataException : Exception
    {
        public MetaDataException(string message) : base(message) { }
    }

    public static void Debg( object locationClass, string message )
    {
        //Debug.Log( "<color=magenta>DEBUG: </color> [" + locationClass.ToString() + "]:: " + message );
    }

    public static void Info(object locationClass, string message)
    {
        Debug.Log( "<color=cyan>INFO: </color>[" + locationClass.ToString() + "]:: " + message);
    }

    public static void Info( string className, string message )
    {
        Debug.Log( "<color=cyan>INFO: </color>[" + className + "]:: " + message );
    }

    public static void Warn(object locationClass, string message)
    {
        Debug.LogWarning( "<color=yellow>WARNING: [" + locationClass.ToString() + "]:: " + message + "</color>");
    }

    public static void Warn( string className, string message )
    {
        Debug.LogWarning( "<color=yellow>WARNING: [" + className + "]:: " + message + "</color>" );
    }

    public static void Error(object locationClass, string message)
    {
        Debug.LogError( "<color=red>ERROR: [" + locationClass.ToString() + "]:: " + message + "</color>" );
    }

    public static void Error( string className, string message )
    {
        Debug.LogError( "<color=red>ERROR: [" + className + "]:: " + message + "</color>" );
    }

    public static void ThrowMetaDataException(object locationClass, string message)
    {
        throw new MetaDataException("[" + locationClass.ToString() + "]:: " + message);
    }

    public static void ThrowValueNotFoundException(object locationClass, string value)
    {
        throw new MetaDataException("[" + locationClass.ToString() + "]:: " + "Value could not be found/read: " + value);
    }

    public static void ThrowDataException(object locationClass, string value, Exception exception)
    {
        throw new Exception("[" + locationClass.ToString() + "]:: " + "Failed: " + value + " because of " + exception.Message);
    }

    public static void ThrowDataException( string className, string value, Exception exception )
    {
        throw new Exception( "[" + className + "]:: " + "Failed: " + value + " because of " + exception.Message );
    }

    public static void ThrowException(object locationClass, string message)
    {
        throw new Exception("[" + locationClass.ToString() + "]:: " + message);
    }

    public static void ShowDialogBox( string message )
    {
        Singleton.GetDialogBox().SetActive( true );
        Singleton.GetDialogBox().GetComponentInChildren<TextMeshProUGUI>().text = message;
    }
}
