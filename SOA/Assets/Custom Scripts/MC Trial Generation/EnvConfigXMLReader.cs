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
    class EnvConfigXMLReader
    {
        public static EnvConfig Parse(string xmlFilename)
        {
            // Initialize EnvConfig object to hold results
            EnvConfig envConfig = new EnvConfig();

            // Read in the XML document
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlFilename);
            }
            catch (System.IO.FileNotFoundException)
            {
                // Handle error if not found
                #if(UNITY_STANDALONE)
                Debug.LogError("EnvConfigXMLReader::Parse(): Could not load " + xmlFilename);
                #else
                Console.WriteLine("EnvConfigXMLReader::Parse(): Could not load " + xmlFilename);
                #endif
                return null;
            }

            // Find the "EnvConfig" category
            XmlNode configNode = null;
            foreach (XmlNode c in xmlDoc.ChildNodes)
            {
                if (c.Name == "EnvConfig")
                {
                    configNode = c;
                    break;
                }
            }

            // Check if we actually found it
            if (configNode == null)
            {
                // Handle error if not found
                #if(UNITY_STANDALONE)
                Debug.LogError("EnvConfigXMLReader::Parse(): Could not find \"EnvConfig\" node");
                #else
                Console.WriteLine("EnvConfigXMLReader::Parse(): Could not find \"EnvConfig\" node");
                #endif
                return null;
            }

            // Parse child nodes
            foreach (XmlNode c in configNode.ChildNodes)
            {
                // Parse differently depending on which category we are in
                switch (c.Name)
                {
                    case "GridMath":
                        // Grid math cells
                        ParseGridMath(c, envConfig);
                        break;
                    case "Road":
                        // Road cells
                        ParseCells(c, envConfig.roadCells);
                        break;
                    case "Mountain":
                        // Mountain cells
                        ParseCells(c, envConfig.mountainCells);
                        break;
                    case "Water":
                        // Water cells
                        ParseCells(c, envConfig.waterCells);
                        break;
                    case "Land":
                        // Land cells
                        ParseCells(c, envConfig.landCells);
                        break;
                    default:
                        // Unrecognized
                        #if(UNITY_STANDALONE)
                        Debug.LogError("EnvConfigXMLReader::Parse(): Unrecognized node type " + c.Name);
                        #else
                        Console.WriteLine("EnvConfigXMLReader::Parse(): Unrecognized node type " + c.Name);
                        #endif
                        break;
                }
            }

            // Return result
            return envConfig;
        }

        private static void ParseGridMath(XmlNode node, EnvConfig envConfig)
        {
            envConfig.gridOrigin_x = GetFloatAttribute(node, "gridOrigin_x", 0.0f);
            envConfig.gridOrigin_z = GetFloatAttribute(node, "gridOrigin_z", 0.0f);
            envConfig.gridToWorldScale = GetFloatAttribute(node, "gridToWorldScale", 0.0f);
        }

        private static void ParseCells(XmlNode parentNode, List<PrimitivePair<int, int>> cells)
        {
            // Go through each child node
            foreach (XmlNode c in parentNode.ChildNodes)
            {
                switch (c.Name)
                {
                    case "Cell":
                        // Cell, extract coordinate and add to cell list
                        cells.Add(
                            new PrimitivePair<int, int>(
                                GetIntAttribute(c, "u", 0),
                                GetIntAttribute(c, "v", 0)));
                        break;
                    default:
                        // Unrecognized
                        #if(UNITY_STANDALONE)
                        Debug.LogError("EnvConfigXMLReader::ParseCells(): Unrecognized node type " + c.Name);
                        #else
                        Console.WriteLine("EnvConfigXMLReader::ParseCells(): Unrecognized node type " + c.Name);
                        #endif
                        break;
                }
            }
        }

        private static int GetIntAttribute(XmlNode node, string attribute, int defaultValue)
        {
            if (node.Attributes[attribute] == null)
            {
                // Use default and give warning
                #if(UNITY_STANDALONE)
                Debug.LogWarning("EnvConfigXMLReader::getIntAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                #else
                Console.WriteLine("EnvConfigXMLReader::getIntAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                #endif
                return defaultValue;
            }
            else
            {
                // Return actual value
                return Convert.ToInt32(node.Attributes[attribute].Value);
            }
        }

        private static float GetFloatAttribute(XmlNode node, string attribute, float defaultValue)
        {
            if (node.Attributes[attribute] == null)
            {
                // Use default and give warning
                #if(UNITY_STANDALONE)
                Debug.LogWarning("EnvConfigXMLReader::getFloatAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                #else
                Console.WriteLine("EnvConfigXMLReader::getFloatAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                #endif
                return defaultValue;
            }
            else
            {
                // Return actual value
                return Convert.ToSingle(node.Attributes[attribute].Value);
            }
        }
    }
}

