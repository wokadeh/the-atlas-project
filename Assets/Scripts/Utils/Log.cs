using System.Collections;
using System;
using UnityEngine;

public class Log
{
    public class MetaDataException : Exception
    {
        public MetaDataException(string message) : base(message) { }
    }

    public static void Info(object locationClass, string message)
    {
        Debug.Log("[" + locationClass.ToString() + "]:: " + message);
    }

    public static void Warn(object locationClass, string message)
    {
        Debug.LogWarning("[" + locationClass.ToString() + "]:: " + message);
    }

    public static void Error(object locationClass, string message)
    {
        Debug.LogError("[" + locationClass.ToString() + "]:: " + message);
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

    public static void ThrowException(object locationClass, string message)
    {
        throw new Exception("[" + locationClass.ToString() + "]:: " + message);
    }
}
