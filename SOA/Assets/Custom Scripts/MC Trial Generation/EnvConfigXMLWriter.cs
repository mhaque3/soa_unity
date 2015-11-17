#if(UNITY_STANDALONE)
using UnityEngine;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace soa
{
    class EnvConfigXMLWriter
    {
        public static void Write(EnvConfig envConfig, string xmlFilename)
        {
            // Create new XML document
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode node;

            // Create "EnvConfig" node
            XmlNode configNode = xmlDoc.CreateElement("EnvConfig");
            xmlDoc.AppendChild(configNode);

            // Populate "GridMath" node
            node = xmlDoc.CreateElement("GridMath");
            configNode.AppendChild(node);
            AddAttribute(xmlDoc, node, "gridOrigin_x", envConfig.gridOrigin_x.ToString());
            AddAttribute(xmlDoc, node, "gridOrigin_z", envConfig.gridOrigin_z.ToString());
            AddAttribute(xmlDoc, node, "gridToWorldScale", envConfig.gridToWorldScale.ToString());

            // Populate "Road" node
            node = xmlDoc.CreateElement("Road");
            configNode.AppendChild(node);
            AddChildCells(xmlDoc, node, envConfig.roadCells);

            // Populate "Mountain" node
            node = xmlDoc.CreateElement("Mountain");
            configNode.AppendChild(node);
            AddChildCells(xmlDoc, node, envConfig.mountainCells);

            // Populate "Water" node
            node = xmlDoc.CreateElement("Water");
            configNode.AppendChild(node);
            AddChildCells(xmlDoc, node, envConfig.waterCells);

            // Populate "Land" node
            node = xmlDoc.CreateElement("Land");
            configNode.AppendChild(node);
            AddChildCells(xmlDoc, node, envConfig.landCells);

            // Write to file
            xmlDoc.Save(xmlFilename);
        }

        private static void AddChildCells(XmlDocument xmlDoc, XmlNode parentNode, List<PrimitivePair<int, int>> cells)
        {
            // Go through each cell in list
            XmlNode childNode;
            foreach (PrimitivePair<int, int> cell in cells)
            {
                // Add as a child cell
                childNode = xmlDoc.CreateElement("Cell");
                parentNode.AppendChild(childNode);

                // Add coordinates as attribute
                AddAttribute(xmlDoc, childNode, "u", cell.first.ToString());
                AddAttribute(xmlDoc, childNode, "v", cell.second.ToString());
            }
        }

        private static void AddAttribute(XmlDocument xmlDoc, XmlNode node, string attribute, string value)
        {
            XmlAttribute newAttribute = xmlDoc.CreateAttribute(attribute);
            newAttribute.Value = value;
            node.Attributes.Append(newAttribute);
        }
    }
}

