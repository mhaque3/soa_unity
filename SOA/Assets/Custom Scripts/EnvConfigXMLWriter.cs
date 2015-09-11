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

            // Populate "RedBase" node
            node = xmlDoc.CreateElement("RedBase");
            configNode.AppendChild(node);
            AddChildCells(xmlDoc, node, envConfig.redBaseCells);

            // Populate "BlueBase" node
            node = xmlDoc.CreateElement("BlueBase");
            configNode.AppendChild(node);
            AddChildCells(xmlDoc, node, envConfig.blueBaseCells);
            
            // Populate "Village" node
            node = xmlDoc.CreateElement("Village");
            configNode.AppendChild(node);
            AddChildCells(xmlDoc, node, envConfig.villageCells);

            // Populate "NGOSite" node
            node = xmlDoc.CreateElement("NGOSite");
            configNode.AppendChild(node);
            AddChildCells(xmlDoc, node, envConfig.ngoSiteCells);

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

        private static void AddChildCells(XmlDocument xmlDoc, XmlNode node, List<PrimitivePair<int, int>> cells)
        {
            // Go through each cell in list
            XmlNode node;
            foreach (PrimitivePair<int, int> cell in cells)
            {
                // Add as a child cell
                node = xmlDoc.CreateElement("Cell");
                node.AppendChild(node);

                // Add coordinates as attribute
                AddAttribute(xmlDoc, node, "u", cell.first.ToString());
                AddAttribute(xmlDoc, node, "v", cell.second.ToString());
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

