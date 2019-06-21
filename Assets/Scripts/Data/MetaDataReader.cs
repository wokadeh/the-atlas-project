using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;

public class MetaDataReader : IMetaDataReader {
    private class MetaDataException : Exception {
        public MetaDataException(string message) : base(message) { }
    }

    private class MetaData : IMetaData {
        public int BitDepth { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public IList<IVariable> Variables { get; set; }
    }

    private class Variable : IVariable {
        public string Name { get; set; }
    }

    private const string BIT_DEPTH_ATTRIBUTE = "bit_depth";
    private const string WIDTH_ATTRIBUTE = "width";
    private const string HEIGHT_ATTRIBUTE = "height";

    private const string VARIABLE_ELEMENT = "variable";
    private const string VARIABLE_NAME_ATTRIBUTE = "name";

    public IMetaData Read(string path) {
        XmlDocument document = new XmlDocument();
        document.Load(path);
        XmlElement root = document.DocumentElement;

        // Read in bit depth, width and height
        int bitDepth = ReadAttribute(root, BIT_DEPTH_ATTRIBUTE);
        int width = ReadAttribute(root, WIDTH_ATTRIBUTE);
        int height = ReadAttribute(root, HEIGHT_ATTRIBUTE);

        // Read in variables
        IList<IVariable> variables = new List<IVariable>();
        if (root.ChildNodes.Count == 0) {
            throw new MetaDataException($"Trying to read meta data with no variables!");
        }
        foreach (XmlNode node in root.ChildNodes) {
            if (node.Name == VARIABLE_ELEMENT) {
                // Read name from variable node
                string name = node.Attributes[VARIABLE_NAME_ATTRIBUTE].Value;
                if (name != null) {
                    variables.Add(new Variable() { Name = name });
                } else {
                    throw new MetaDataException($"Failed to read '{VARIABLE_NAME_ATTRIBUTE}' attribute from variable!");
                }
            }
        }

        return new MetaData() {
            BitDepth = bitDepth,
            Width = width,
            Height = height,

            Variables = variables
        };
    }

    private int ReadAttribute(XmlElement element, string name) {
        int attribute;
        if (element.HasAttribute(name)) {
            if (!int.TryParse(element.GetAttribute(name), out attribute)) {
                throw new MetaDataException($"Failed to read '{name}' attribute!");
            }
        } else {
            throw new MetaDataException($"Xml file has no '{name}' attribute!");
        }
        return attribute;
    }
}
