using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class MetaDataReader
{
    public static MetaDataManager.MetaData LoadProjectAttributes( XmlElement _root, MetaDataManager.MetaData _inputMetaData )
    {
        _inputMetaData.DataName = _root.Name;
        _inputMetaData.BitDepth = Utils.ReadIntegerAttribute( _root, Globals.BIT_DEPTH_ATTRIBUTE );
        _inputMetaData.Width = Utils.ReadIntegerAttribute( _root, Globals.WIDTH_ATTRIBUTE );
        _inputMetaData.Height = Utils.ReadIntegerAttribute( _root, Globals.HEIGHT_ATTRIBUTE );
        _inputMetaData.Levels = Utils.ReadIntegerAttribute( _root, Globals.LEVELS_ATTRIBUTE );
        _inputMetaData.StartDateTimeNumber = Utils.ReadDoubleAttribute( _root, Globals.START_DATETIME_ATTRIBUTE );
        _inputMetaData.EndDateTimeNumber = Utils.ReadDoubleAttribute( _root, Globals.END_DATETIME_ATTRIBUTE );
        _inputMetaData.TimeInterval = Utils.ReadIntegerAttribute( _root, Globals.TIME_INTERVAL_ATTRIBUTE );

        return _inputMetaData;
    }

    public static IMetaData Load( string _projectFilePath )
    {
        return MetaDataReader.Import( _projectFilePath );
    }

    public static IMetaData Import( string _projectFilePath )
    {
        XmlDocument document = new XmlDocument();
        document.Load( _projectFilePath );

        XmlElement root = document.DocumentElement;

        MetaDataManager.MetaData outputMetaData = new MetaDataManager.MetaData();

        outputMetaData = MetaDataReader.LoadProjectAttributes( root, outputMetaData );

        // Read in variables
        IList<IVariable> variablesList = new List<IVariable>();
        IList<IList<TimeStepDataAsset>> timestampLisList = new List<IList<TimeStepDataAsset>>();

        if( root.ChildNodes.Count == 0 )
        {
            Log.ThrowValueNotFoundException( "MetaDataReader", root.Name + "is empty" );
        }
        foreach( XmlNode varNode in root.ChildNodes )
        {
            if( varNode.Name == Globals.VARIABLE_ELEMENT )
            {
                // Read name from variable node
                string varNodeName = varNode.Attributes[ Globals.VARIABLE_NAME_ATTRIBUTE ].Value;
                double varNodeMin = Utils.ReadAttribute( varNode, Globals.VARIABLE_MIN_ATTRIBUTE );
                double varNodeMax = Utils.ReadAttribute( varNode, Globals.VARIABLE_MAX_ATTRIBUTE );

                List<TimeStepDataAsset> varTimestampList = new List<TimeStepDataAsset>();

                if( varNodeName != null )
                {
                    variablesList.Add( new MetaDataManager.Variable() { Name = varNodeName, Min = varNodeMin, Max = varNodeMax } );

                    // Create a new list for timestamps
                    timestampLisList.Add( varTimestampList );
                }
                else
                {
                    Log.ThrowValueNotFoundException( "MetaDataReader", Globals.VARIABLE_NAME_ATTRIBUTE );
                }

                if( varNode.ChildNodes.Count == 0 )
                {
                    Log.ThrowValueNotFoundException( "MetaDataReader", varNode.Name );
                }

                foreach( XmlNode timestampNode in varNode.ChildNodes )
                {
                    if( timestampNode.Name == Globals.TIME_STAMP_LIST_ELEMENT )
                    {
                        TimeStepDataAsset newTimestamp = MetaDataReader.ReadTimeStamp( timestampNode );

                        // fill last list with timestamps
                        varTimestampList.Add( newTimestamp );
                    }
                }
            }
        }

        outputMetaData.Timestamps = timestampLisList;
        outputMetaData.Variables = variablesList;

        return outputMetaData;
    }

    private static TimeStepDataAsset ReadTimeStamp( XmlNode timestampNode )
    {
        TimeStepDataAsset newTimestamp = new TimeStepDataAsset();

        newTimestamp.DateTimeDouble = Utils.ReadAttribute( timestampNode, Globals.TIME_STAMP_DATETIME_ATTRIBUTE );

        Debug.Log( "loading timestamps: " + newTimestamp.DateTimeDouble );

        Vector3 dim = new Vector3();
        dim.x = Utils.ReadAttribute( timestampNode, Globals.TIME_STAMP_DATA_ASSET_DIM_X_ATTRIBUTE );
        dim.y = Utils.ReadAttribute( timestampNode, Globals.TIME_STAMP_DATA_ASSET_DIM_Y_ATTRIBUTE );
        dim.z = Utils.ReadAttribute( timestampNode, Globals.TIME_STAMP_DATA_ASSET_DIM_Z_ATTRIBUTE );

        return newTimestamp;
    }
}