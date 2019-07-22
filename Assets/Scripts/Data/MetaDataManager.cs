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
        public int BitDepth { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Levels { get; set; }

        public IList<IVariable> Variables { get; set; }
    }

    private class Variable : IVariable {
        public string Name { get; set; }
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
            document.AppendChild(document.CreateElement(Globals.ROOT_NODE));
            XmlElement root = document.DocumentElement;

            // Set main attributes
            root.SetAttribute(Globals.BIT_DEPTH_ATTRIBUTE, _metaData.BitDepth.ToString());
            root.SetAttribute(Globals.WIDTH_ATTRIBUTE, _metaData.Width.ToString());
            root.SetAttribute(Globals.HEIGHT_ATTRIBUTE, _metaData.Height.ToString());
            root.SetAttribute(Globals.LEVELS_ATTRIBUTE, _metaData.Levels.ToString());

            // Add node for each variable
            foreach (Variable var in _metaData.Variables)
            {
                XmlElement varNode = document.CreateElement(Globals.VARIABLE_ELEMENT);
                varNode.SetAttribute(Globals.VARIABLE_NAME_ATTRIBUTE, var.Name);
                root.AppendChild(varNode);

                // Go through every element in the list of earthDataFrames for each variable
                foreach (EarthDataFrame earthDataFrame in _dataAssets[var.Name])
                {
                    XmlElement earthFrameDataNode = document.CreateElement(Globals.EARTH_DATA_FRAME_ELEMENT);
                    earthFrameDataNode.SetAttribute(Globals.EARTH_DATA_FRAME_DIM_X_ATTRIBUTE, earthDataFrame.Dimensions.x.ToString());
                    earthFrameDataNode.SetAttribute(Globals.EARTH_DATA_FRAME_DIM_Y_ATTRIBUTE, earthDataFrame.Dimensions.y.ToString());
                    earthFrameDataNode.SetAttribute(Globals.EARTH_DATA_FRAME_DIM_Z_ATTRIBUTE, earthDataFrame.Dimensions.z.ToString());

                    earthFrameDataNode.SetAttribute(Globals.EARTH_DATA_FRAME_DATA_TEXTURE_NAME_ATTRIBUTE, earthDataFrame.DataTexture.ToString());
                    earthFrameDataNode.SetAttribute(Globals.EARTH_DATA_FRAME_HISTO_TEXTURE_NAME_ATTRIBUTE, earthDataFrame.HistogramTexture.ToString());


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
        int bitDepth = ReadAttribute(root, Globals.BIT_DEPTH_ATTRIBUTE);
        int width = ReadAttribute(root, Globals.WIDTH_ATTRIBUTE);
        int height = ReadAttribute(root, Globals.HEIGHT_ATTRIBUTE);
        int levels = ReadAttribute(root, Globals.LEVELS_ATTRIBUTE);

        // Read in variables
        IList<IVariable> variables = new List<IVariable>();
        if (root.ChildNodes.Count == 0) {
            throw new MetaDataException($"Trying to read meta data with no variables!");
        }
        foreach (XmlNode node in root.ChildNodes) {
            if (node.Name == Globals.VARIABLE_ELEMENT) {
                // Read name from variable node
                string name = node.Attributes[Globals.VARIABLE_NAME_ATTRIBUTE].Value;
                if (name != null) {
                    variables.Add(new Variable() { Name = name });
                } else {
                    throw new MetaDataException($"Failed to read '{Globals.VARIABLE_NAME_ATTRIBUTE}' attribute from variable!");
                }
            }
        }

        return new MetaData() {
            BitDepth = bitDepth,
            Width = width,
            Height = height,
            Levels = levels,

            Variables = variables
        };
    }

    private int ReadAttribute(XmlElement _relement, string name) {
        int attribute;
        if (_relement.HasAttribute(name)) {
            if (!int.TryParse(_relement.GetAttribute(name), out attribute)) {
                throw new MetaDataException($"Failed to read '{name}' attribute!");
            }
        } else {
            throw new MetaDataException($"Xml file has no '{name}' attribute!");
        }
        return attribute;
    }
}
