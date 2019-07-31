using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

using System.Security.AccessControl;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Globalization;

public class MetaDataManager : IMetaDataManager {
  
    public class MetaData : IMetaData {
        public string DataName { get; set; }
        public int BitDepth { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Levels { get; set; }
        public float StartDateTimeNumber { get; set; }
        public float EndDateTimeNumber { get; set; }
        public int TimeInterval { get; set; }

        public IList<IVariable> Variables { get; set; }
        public IList<IList<TimeStepDataAsset>> Timestamps { get; set; }
    }

    private class Variable : IVariable {
        public string Name { get; set; }
    }

    private class Timestamp : ITimestamp
    {
        public string DateTime { get; set; }
    }


    public void Write(string _projectFilePath, IMetaData _metaData, Dictionary<string, List<TimeStepDataAsset>> _dataAssets)
    {
        try
        {
            // Create root
            XmlDocument document = new XmlDocument();
            document.CreateXmlDeclaration("1.0", "utf-8", "");

            //Create the root element and 
            //add it to the document.
            document.AppendChild(document.CreateElement(_metaData.DataName));
            XmlElement root = document.DocumentElement;

            // Set main attributes
            root.SetAttribute(Globals.BIT_DEPTH_ATTRIBUTE, _metaData.BitDepth.ToString());
            root.SetAttribute(Globals.WIDTH_ATTRIBUTE, _metaData.Width.ToString());
            root.SetAttribute(Globals.HEIGHT_ATTRIBUTE, _metaData.Height.ToString());
            root.SetAttribute(Globals.LEVELS_ATTRIBUTE, _metaData.Levels.ToString());
            root.SetAttribute(Globals.START_DATETIME_ATTRIBUTE, _metaData.StartDateTimeNumber.ToString());
            root.SetAttribute(Globals.END_DATETIME_ATTRIBUTE, _metaData.EndDateTimeNumber.ToString());
            root.SetAttribute(Globals.TIME_INTERVAL_ATTRIBUTE, _metaData.TimeInterval.ToString());

            int j = 0;
            // Add node for each variable
            foreach (Variable var in _metaData.Variables)
            {
                XmlElement varNode = document.CreateElement(Globals.VARIABLE_ELEMENT);
                varNode.SetAttribute(Globals.VARIABLE_NAME_ATTRIBUTE, var.Name);
                root.AppendChild(varNode);

                int i = 0;
                // Go through every element in the list of earthDataFrames for each variable
                foreach (TimeStepDataAsset earthDataFrame in _dataAssets[var.Name])
                {
                    XmlElement earthFrameDataNode = document.CreateElement(Globals.TIME_STAMP_DATA_ASSET_ELEMENT);

                    string currentTimeStamp = _metaData.Timestamps[j][i].DateTime.ToString().Replace(',', '.');

                    Log.Info(this, "Save timestamp to XML: " + currentTimeStamp);

                    earthFrameDataNode.SetAttribute(Globals.TIMESTAMP_DATETIME_ATTRIBUTE, currentTimeStamp);

                    earthFrameDataNode.SetAttribute(Globals.TIME_STAMP_DATA_ASSET_DIM_X_ATTRIBUTE, earthDataFrame.Dimensions.x.ToString());
                    earthFrameDataNode.SetAttribute(Globals.TIME_STAMP_DATA_ASSET_DIM_Y_ATTRIBUTE, earthDataFrame.Dimensions.y.ToString());
                    earthFrameDataNode.SetAttribute(Globals.TIME_STAMP_DATA_ASSET_DIM_Z_ATTRIBUTE, earthDataFrame.Dimensions.z.ToString());


                    varNode.AppendChild(earthFrameDataNode);
                    i++;
                }
                // @TODO: Go through every node in the transfer functions for each variable
                // foreach( )

                j++;
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            // Save the document to a file and auto-indent the output.
            XmlWriter writer = XmlWriter.Create(_projectFilePath, settings);

            document.PreserveWhitespace = true;
            document.Save(writer);
        }
        catch (Exception e)
        {
            Log.ThrowDataException(this, _projectFilePath, e);
        }
    }

    public MetaData LoadProjectAttributes(XmlElement _root, MetaData _inputMetaData)
    {
        _inputMetaData.DataName = _root.Name;
        _inputMetaData.BitDepth = Utils.ReadIntegerAttribute(_root, Globals.BIT_DEPTH_ATTRIBUTE);
        _inputMetaData.Width = Utils.ReadIntegerAttribute(_root, Globals.WIDTH_ATTRIBUTE);
        _inputMetaData.Height = Utils.ReadIntegerAttribute(_root, Globals.HEIGHT_ATTRIBUTE);
        _inputMetaData.Levels = Utils.ReadIntegerAttribute(_root, Globals.LEVELS_ATTRIBUTE);
        _inputMetaData.StartDateTimeNumber = Utils.ReadFloatAttribute(_root, Globals.START_DATETIME_ATTRIBUTE);
        _inputMetaData.EndDateTimeNumber = Utils.ReadFloatAttribute(_root, Globals.END_DATETIME_ATTRIBUTE);
        _inputMetaData.TimeInterval = Utils.ReadIntegerAttribute(_root, Globals.TIME_INTERVAL_ATTRIBUTE);

        return _inputMetaData;
    }

    public IMetaData Load(string _projectFilePath)
    {
        return Import(_projectFilePath);
    }

    public IMetaData Import(string _projectFilePath)
    {
        XmlDocument document = new XmlDocument();
        document.Load(_projectFilePath);
        XmlElement root = document.DocumentElement;

        MetaData outputMetaData = new MetaData();

        outputMetaData = this.LoadProjectAttributes(root, outputMetaData);

        // Read in variables
        IList<IVariable> variablesList = new List<IVariable>();
        IList<IList<TimeStepDataAsset>> timestampLisList = new List<IList<TimeStepDataAsset>>();

        if (root.ChildNodes.Count == 0)
        {
            Log.ThrowValueNotFoundException(this, root.Name + "is empty");
        }
        foreach (XmlNode varNode in root.ChildNodes)
        {
            if (varNode.Name == Globals.VARIABLE_ELEMENT) {
                // Read name from variable node
                string varNodeName = varNode.Attributes[Globals.VARIABLE_NAME_ATTRIBUTE].Value;

                List<TimeStepDataAsset> varTimestampList = new List<TimeStepDataAsset>();

                if (varNodeName != null) {
                    variablesList.Add(new Variable() { Name = varNodeName });

                    Log.Info(this, "Checking variable " + variablesList[0]);

                    // Create a new list for timestamps
                    timestampLisList.Add(varTimestampList);
                } else
                {
                    Log.ThrowValueNotFoundException(this, Globals.VARIABLE_NAME_ATTRIBUTE);
                }

                if (varNode.ChildNodes.Count == 0)
                {
                    Log.ThrowValueNotFoundException(this, varNode.Name);
                }
                foreach (XmlNode timestampNode in varNode.ChildNodes)
                {
                    if (timestampNode.Name == Globals.TIMESTAMP_LIST_ELEMENT)
                    {
                        TimeStepDataAsset newTimestamp = this.ReadTimeStamp(timestampNode);

                        // fill last list with timestamps
                        varTimestampList.Add(newTimestamp);
                    }
                }
            }
        }

        outputMetaData.Timestamps = timestampLisList;
        outputMetaData.Variables = variablesList;

        return outputMetaData;


        }

    private TimeStepDataAsset ReadTimeStamp(XmlNode timestampNode)
    {
        TimeStepDataAsset newTimestamp = new TimeStepDataAsset();

        newTimestamp.DateTime = this.ReadAttribute(timestampNode, Globals.TIMESTAMP_DATETIME_ATTRIBUTE);

        Vector3 dim = new Vector3();
        dim.x = this.ReadAttribute(timestampNode, Globals.TIME_STAMP_DATA_ASSET_DIM_X_ATTRIBUTE);
        dim.y = this.ReadAttribute(timestampNode, Globals.TIME_STAMP_DATA_ASSET_DIM_Y_ATTRIBUTE);
        dim.z = this.ReadAttribute(timestampNode, Globals.TIME_STAMP_DATA_ASSET_DIM_Z_ATTRIBUTE);
    
        return newTimestamp;
    }

    private float ReadAttribute(XmlNode node, string attributeName)
    {
        float value = 0;
        try
        {
            string valueString = node.Attributes[attributeName].Value;
            value = (float.Parse(valueString, CultureInfo.InvariantCulture.NumberFormat));
        }
        catch (Exception e)
        {
            // No problem!
        }

        return value;
    }
}
