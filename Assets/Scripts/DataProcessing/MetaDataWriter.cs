using System;
using System.Collections.Generic;
using System.Xml;

public class MetaDataWriter
{
    public static void Write( string _projectFilePath, IMetaData _metaData, Dictionary<string, List<TimeStepDataAsset>> _dataAssets )
    {
        //try
        //{
        // Create root
        XmlDocument document = new XmlDocument();
        document.CreateXmlDeclaration( "1.0", "utf-8", "" );

        //Create the root element and 
        //add it to the document.
        document.AppendChild( document.CreateElement( _metaData.DataName ) );
        XmlElement root = document.DocumentElement;

        // Set main attributes
        root.SetAttribute( Globals.BIT_DEPTH_ATTRIBUTE, _metaData.BitDepth.ToString() );
        root.SetAttribute( Globals.WIDTH_ATTRIBUTE, _metaData.Width.ToString() );
        root.SetAttribute( Globals.HEIGHT_ATTRIBUTE, _metaData.Height.ToString() );
        root.SetAttribute( Globals.LEVELS_ATTRIBUTE, _metaData.Levels.ToString() );
        root.SetAttribute( Globals.START_DATETIME_ATTRIBUTE, _metaData.StartDateTimeNumber.ToString() );
        root.SetAttribute( Globals.END_DATETIME_ATTRIBUTE, _metaData.EndDateTimeNumber.ToString() );
        root.SetAttribute( Globals.TIME_INTERVAL_ATTRIBUTE, _metaData.TimeInterval.ToString() );

        int j = 0;
        // Add node for each variable
        foreach( MetaDataManager.Variable var in _metaData.Variables )
        {
            root.AppendChild( MetaDataWriter.WriteVariable( document, var, _metaData, _dataAssets, j ) );

            j++;
        }

        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;

        Log.Info( "MetaDataWriter", "Save XML at " + _projectFilePath );
        // Save the document to a file and auto-indent the output.
        XmlWriter writer = XmlWriter.Create( _projectFilePath, settings );

        document.PreserveWhitespace = true;

        
        document.Save( writer );
        //}
        //catch( Exception e )
        //{
        //    Log.ThrowDataException( "MetaDataWriter", _projectFilePath, e );
        //}
    }

    private static XmlElement WriteVariable( XmlDocument _document, MetaDataManager.Variable _var, IMetaData _metaData, Dictionary<string, List<TimeStepDataAsset>> _dataAssets, int _j )
    {
        XmlElement varNode = _document.CreateElement( Globals.VARIABLE_ELEMENT );
        varNode.SetAttribute( Globals.VARIABLE_NAME_ATTRIBUTE, _var.Name );
        varNode.SetAttribute( Globals.VARIABLE_MIN_ATTRIBUTE, _var.Min.ToString() );
        varNode.SetAttribute( Globals.VARIABLE_MAX_ATTRIBUTE, _var.Max.ToString() );

        int i = 0;
        // Go through every element in the list of earthDataFrames for each variable
        foreach( TimeStepDataAsset timeStepAsset in _dataAssets[ _var.Name ] )
        {
            varNode.AppendChild( MetaDataWriter.WriteVariableTimeStep( _document, _metaData, _dataAssets, _j, i, timeStepAsset ) );
            i++;
        }

        return varNode;
    }
    private static XmlElement WriteVariableTimeStep( XmlDocument _document, IMetaData _metaData, Dictionary<string, List<TimeStepDataAsset>> _dataAssets, int _j, int _i, TimeStepDataAsset _timeStepAsset )
    {
        XmlElement timeStepNode = _document.CreateElement( Globals.TIME_STAMP_DATA_ASSET_ELEMENT );

        string currentTimeStamp = _metaData.Timesteps[ _j ][ _i ].DateTimeDouble.ToString();

        Log.Info( "MetaDataWriter", "Save timestamp to XML: " + currentTimeStamp );

        timeStepNode.SetAttribute( Globals.TIME_STAMP_DATETIME_ATTRIBUTE, currentTimeStamp );

        timeStepNode.SetAttribute( Globals.TIME_STAMP_DATA_ASSET_DIM_X_ATTRIBUTE, _timeStepAsset.Dimensions.x.ToString() );
        timeStepNode.SetAttribute( Globals.TIME_STAMP_DATA_ASSET_DIM_Y_ATTRIBUTE, _timeStepAsset.Dimensions.y.ToString() );
        timeStepNode.SetAttribute( Globals.TIME_STAMP_DATA_ASSET_DIM_Z_ATTRIBUTE, _timeStepAsset.Dimensions.z.ToString() );

        return timeStepNode;
    }
}