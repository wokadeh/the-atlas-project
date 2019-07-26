using System;
using System.Collections.Generic;
using System.Xml;

using System.Security.AccessControl;
using System.IO;
using Microsoft.Win32.SafeHandles;

public class MetaDataManager : IMetaDataManager {
    private class MetaDataException : Exception {
        public MetaDataException(string message) : base(message) { }
    }

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
        public IList<IList<ITimestamp>> Timestamps { get; set; }
    }

    private class Variable : IVariable {
        public string Name { get; set; }
    }

    private class Timestamp : ITimestamp
    {
        public string Name { get; set; }
        public float Value { get; set; }
    }


    public void Write(string _projectFilePath, IMetaData _metaData, Dictionary<string, List<EarthDataFrame>> _dataAssets)
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

            // Add node for each variable
            foreach (Variable var in _metaData.Variables)
            {
                XmlElement varNode = document.CreateElement(Globals.VARIABLE_ELEMENT);
                varNode.SetAttribute(Globals.VARIABLE_NAME_ATTRIBUTE, var.Name);
                root.AppendChild(varNode);

                int i = 0;
                // Go through every element in the list of earthDataFrames for each variable
                foreach (EarthDataFrame earthDataFrame in _dataAssets[var.Name])
                {
                    XmlElement earthFrameDataNode = document.CreateElement(Globals.EARTH_DATA_FRAME_ELEMENT);

                    earthFrameDataNode.SetAttribute(Globals.TIMESTAMP_DATETIME_ATTRIBUTE, _metaData.Timestamps[i].ToString());

                    earthFrameDataNode.SetAttribute(Globals.EARTH_DATA_FRAME_DIM_X_ATTRIBUTE, earthDataFrame.Dimensions.x.ToString());
                    earthFrameDataNode.SetAttribute(Globals.EARTH_DATA_FRAME_DIM_Y_ATTRIBUTE, earthDataFrame.Dimensions.y.ToString());
                    earthFrameDataNode.SetAttribute(Globals.EARTH_DATA_FRAME_DIM_Z_ATTRIBUTE, earthDataFrame.Dimensions.z.ToString());


                    varNode.AppendChild(earthFrameDataNode);
                    i++;
                }
                // @TODO: Go through every node in the transfer functions for each variable
                // foreach( )
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
            throw new MetaDataException($"[MetaDataManager] - Failed to save file '{_projectFilePath}' because of '{e}' !");
        }
    }

    public IMetaData Read(string _projectFilePath)
    {
        XmlDocument document = new XmlDocument();
        document.Load(_projectFilePath);
        XmlElement root = document.DocumentElement;

        // Read in bit depth, width and height
        string dataName = root.Name;
        int bitDepth = this.ReadIntegerAttribute(root, Globals.BIT_DEPTH_ATTRIBUTE);
        int width = this.ReadIntegerAttribute(root, Globals.WIDTH_ATTRIBUTE);
        int height = this.ReadIntegerAttribute(root, Globals.HEIGHT_ATTRIBUTE);
        int levels = this.ReadIntegerAttribute(root, Globals.LEVELS_ATTRIBUTE);
        float startDateTime = this.ReadFloatAttribute(root, Globals.START_DATETIME_ATTRIBUTE);
        float endDateTime = this.ReadFloatAttribute(root, Globals.END_DATETIME_ATTRIBUTE);
        int timeInterval = this.ReadIntegerAttribute(root, Globals.TIME_INTERVAL_ATTRIBUTE);

        // Read in variables
        IList<IVariable> variables = new List<IVariable>();
        IList<IList<ITimestamp>> timestamps = new List<IList<ITimestamp>>();

        if (root.ChildNodes.Count == 0)
        {
            throw new MetaDataException($"[MetaDataManager] - Trying to read meta data with NO VARIABLES!");
        }
        foreach (XmlNode varNode in root.ChildNodes)
        {
            if (varNode.Name == Globals.VARIABLE_ELEMENT) {
                // Read name from variable node
                string varNodeName = varNode.Attributes[Globals.VARIABLE_NAME_ATTRIBUTE].Value;
                if (varNodeName != null) {
                    variables.Add(new Variable() { Name = varNodeName });

                    // Create a new list for timestamps
                    timestamps.Add( new List<ITimestamp>());
                } else
                {
                    throw new MetaDataException($"[MetaDataManager] - Failed to read '{Globals.VARIABLE_NAME_ATTRIBUTE}' attribute from variable!");
                }

                if (varNode.ChildNodes.Count == 0)
                {
                    throw new MetaDataException($"[MetaDataManager] - Trying to read variable ' {varNode.Name} ' data with NO TIMESTAMPS!");
                }
                foreach (XmlNode timestampNode in varNode.ChildNodes)
                {
                    if (varNode.Name == Globals.TIMESTAMP_LIST_ELEMENT)
                    {
                        if (timestampNode == null)
                        {
                            throw new MetaDataException($"[MetaDataManager] - Timestamp of ' {varNode.Name} ' is NULL!");
                        }

                        if (timestampNode.Attributes[Globals.TIMESTAMP_DATETIME_ATTRIBUTE] == null)
                        {
                            throw new MetaDataException($"[MetaDataManager] - Timestamp of ' {varNode.Name} ' is has NO attributes!");
                        }

                        float timestampNodeValue = float.Parse(timestampNode.Attributes[Globals.TIMESTAMP_DATETIME_ATTRIBUTE].Value);
                        if (timestampNodeValue != 0)
                        {
                            // fill last list with timestamps
                            timestamps[timestamps.Count - 1].Add(new Timestamp() { Value = timestampNodeValue });
                        }
                        else
                        {
                            throw new MetaDataException($"Failed to read '{Globals.TIMESTAMP_DATETIME_ATTRIBUTE}' attribute from timestamp!");
                        }
                    }
                }
            }
        }

        return new MetaData() {
            DataName = dataName,
            BitDepth = bitDepth,
            Width = width,
            Height = height,
            Levels = levels,
            StartDateTimeNumber = startDateTime,
            EndDateTimeNumber = endDateTime,
            TimeInterval = timeInterval,
            Timestamps = timestamps,
            Variables = variables
        };
    }

    private int ReadIntegerAttribute(XmlElement _relement, string _name) {
        int attribute;
        if (_relement.HasAttribute(_name)) {
            if (!int.TryParse(_relement.GetAttribute(_name), out attribute)) {
                throw new MetaDataException($"Failed to read '{_name}' attribute!");
            }
        } else {
            throw new MetaDataException($"Xml file has no '{_name}' attribute!");
        }
        return attribute;
    }

    private float ReadFloatAttribute(XmlElement _relement, string _name)
    {
        float attribute;
        if (_relement.HasAttribute(_name))
        {
            if (!float.TryParse(_relement.GetAttribute(_name), out attribute))
            {
                throw new MetaDataException($"Failed to read '{_name}' attribute!");
            }
        }
        else
        {
            throw new MetaDataException($"Xml file has no '{_name}' attribute!");
        }
        return attribute;
    }
}
