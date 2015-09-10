using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace soa
{
    class SoaConfigXMLReader
    {
        public static SoaConfig Parse(string xmlFilename)
        {
            // Initialize SoaConfig object to hold results
            SoaConfig soaConfig = new SoaConfig();

            // Read in the XML document
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlFilename);
            }
            catch (System.IO.FileNotFoundException)
            {
                // Handle error if not found
                Debug.LogError("SoaConfigXMLReader::Parse(): Could not load " + xmlFilename);
                return null;
            }

            // Find the "Config" category
            XmlNode configNode = null;
            foreach (XmlNode c in xmlDoc.ChildNodes)
            {
                if (c.Name == "Config")
                {
                    configNode = c;
                    break;
                }
            }

            // Check if we actually found it
            if (configNode == null)
            {
                // Insert error message here
                return null;
            }

            // Parse network and simulation setup first
            foreach (XmlNode c in configNode.ChildNodes)
            {
                // Parse differently depending on which category we are in
                switch (c.Name)
                {
                    case "Network":
                        // Network configuration
                        ParseNetwork(c, soaConfig);
                        break;
                    case "Simulation":
                        // Simulation configuration
                        ParseSimulation(c, soaConfig);
                        break;
                    case "Logger":
                        // Logger configuration
                        ParseLogger(c, soaConfig);
                        break;
                }
            }

            // Then look at local and remote platforms whose defaults may
            // use information from network or simulation categories
            foreach (XmlNode c in configNode.ChildNodes)
            {
                // Parse differently depending on which category we are in
                switch (c.Name)
                {
                    case "Local":
                        // Local platforms
                        ParseLocal(c, soaConfig);
                        break;
                    case "Remote":
                        // Remote platforms
                        ParseRemote(c, soaConfig);
                        break;
                }
            }

            // Return result
            return soaConfig;
        }

        /********************************************************************************************
         *                               CATEGORY PARSING FUNCTIONS                                 *
         ********************************************************************************************/

        // Network configuration category parsing
        static void ParseNetwork(XmlNode node, SoaConfig soaConfig)
        {
            // Pull attributes directly from the node
            try
            {
                soaConfig.networkRedRoom = GetStringAttribute(node, "redRoom", "soa-default-red");
                soaConfig.networkBlueRoom = GetStringAttribute(node, "blueRoom", "soa-default-blue");
            }
            catch (Exception)
            {
                Debug.LogError("SoaConfigXMLReader::ParseNetwork(): Error parsing " + node.Name);
            }            
        }

        // Simulation configuration category parsing
        static void ParseSimulation(XmlNode node, SoaConfig soaConfig)
        {
            // Pull attributes directly from the node
            try
            {
                soaConfig.probRedDismountWeaponized = GetFloatAttribute(node, "probRedDismountWeaponized", 1.0f);
                soaConfig.probRedTruckWeaponized = GetFloatAttribute(node, "probRedTruckWeaponized", 1.0f);
            }
            catch (Exception)
            {
                Debug.LogError("SoaConfigXMLReader::ParseSimulation(): Error parsing " + node.Name);
            }
        }

        static void ParseLogger(XmlNode node, SoaConfig soaConfig)
        {
            // Pull attributes directly from the node
            try
            {
                soaConfig.loggerOutputFile = GetStringAttribute(node, "outputFile", "SoaSimOutput.xml");
                soaConfig.enableLogToFile = GetBooleanAttribute(node, "enableLogToFile", false);
                soaConfig.enableLogToUnityConsole = GetBooleanAttribute(node, "enableLogToUnityConsole", true);
            }
            catch (Exception)
            {
                Debug.LogError("SoaConfigXMLReader::ParseSimulation(): Error parsing " + node.Name);
            }
        }

        // Local platform category parsing
        static void ParseLocal(XmlNode node, SoaConfig soaConfig)
        {
            // Go through each child node
            foreach (XmlNode c in node.ChildNodes)
			{
				try
				{
					switch (c.Name)
					{
                        case "RedDismount":
                            {
                                soaConfig.localPlatforms.Add(
                                    new RedDismountConfig(
                                        GetFloatAttribute(c, "x_km", 0),
                                        GetFloatAttribute(c, "y_km", 0),
                                        GetFloatAttribute(c, "z_km", 0),
                                        GetIntAttribute(c, "id", -1),
                                        GetStringAttribute(c, "initialWaypoint", null),
                                        GetBooleanAttribute(c, "hasWeapon", UnityEngine.Random.value <= soaConfig.probRedDismountWeaponized)
                                    )
                                );
                            }
                            break;
                        case "RedTruck":
                            {
                                soaConfig.localPlatforms.Add(
                                    new RedTruckConfig(
                                        GetFloatAttribute(c, "x_km", 0),
                                        GetFloatAttribute(c, "y_km", 0),
                                        GetFloatAttribute(c, "z_km", 0),
                                        GetIntAttribute(c, "id", -1),
                                        GetStringAttribute(c, "initialWaypoint", null),
                                        GetBooleanAttribute(c, "hasWeapon", UnityEngine.Random.value <= soaConfig.probRedTruckWeaponized)
                                    )
                                );
                            }
                            break;
                        case "NeutralDismount":
                            {
                                soaConfig.localPlatforms.Add(
                                    new NeutralDismountConfig(
                                        GetFloatAttribute(c, "x_km", 0),
                                        GetFloatAttribute(c, "y_km", 0),
                                        GetFloatAttribute(c, "z_km", 0),
                                        GetIntAttribute(c, "id", -1)
                                    )
                                );
                            }
                            break;
                        case "NeutralTruck":
                            {
                                soaConfig.localPlatforms.Add(
                                    new NeutralTruckConfig(
                                        GetFloatAttribute(c, "x_km", 0),
                                        GetFloatAttribute(c, "y_km", 0),
                                        GetFloatAttribute(c, "z_km", 0),
                                        GetIntAttribute(c, "id", -1)
                                    )
                                );
                            }
                            break;
                        case "BluePolice":
                            {
                                soaConfig.localPlatforms.Add(
                                    new BluePoliceConfig(
                                        GetFloatAttribute(c, "x_km", 0),
                                        GetFloatAttribute(c, "y_km", 0),
                                        GetFloatAttribute(c, "z_km", 0),
                                        GetIntAttribute(c, "id", -1)
                                    )
                                );
                            }
                            break;
                        default:
                            if(c.Name != "#comment")
                            {
                                Debug.LogWarning("SoaConfigXMLReader::ParseLocal(): Unrecognized node " + c.Name);
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    Debug.LogError("SoaConfigXMLReader::ParseLocal(): Error parsing " + c.Name);
                }
            }
        }

        // Remote platform category parsing
        static void ParseRemote(XmlNode node, SoaConfig soaConfig)
        {
            // Go through each child node
            foreach (XmlNode c in node.ChildNodes)
            {
                try
                {
                    switch (c.Name)
                    {
                        case "HeavyUAV":
                            {
                                soaConfig.remotePlatforms.Add(
                                    new HeavyUAVConfig(
                                        GetFloatAttribute(c, "x_km", 0),
                                        GetFloatAttribute(c, "y_km", 0),
                                        GetFloatAttribute(c, "z_km", 0),
                                        GetIntAttribute(c, "id", -1)
                                    )
                                );
                            }
                            break;
                        case "SmallUAV":
                            {
                                soaConfig.remotePlatforms.Add(
                                    new SmallUAVConfig(
                                        GetFloatAttribute(c, "x_km", 0),
                                        GetFloatAttribute(c, "y_km", 0),
                                        GetFloatAttribute(c, "z_km", 0),
                                        GetIntAttribute(c, "id", -1)
                                    )
                                );
                            }
                            break;
                        case "Balloon":
                            {
                                soaConfig.remotePlatforms.Add(
                                    new BalloonConfig(
                                        GetFloatAttribute(c, "x_km", 0),
                                        GetFloatAttribute(c, "y_km", 0),
                                        GetFloatAttribute(c, "z_km", 0),
                                        GetIntAttribute(c, "id", -1)
                                    )
                                );
                            }
                            break;
                        default:
                            if (c.Name != "#comment")
                            {
                                Debug.LogWarning("SoaConfigXMLReader::ParseRemote(): Unrecognized node " + c.Name);
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    Debug.LogError("SoaConfigXMLReader::ParseRemote(): Error parsing " + c.Name);
                }
            }
        }

        /********************************************************************************************
         *                            ATTRIBUTE READING HELPER FUNCTIONS                            *
         ********************************************************************************************/
        static bool GetBooleanAttribute(XmlNode node, string attribute, bool defaultValue)
        {
            if (node.Attributes[attribute] == null)
            {
                // Use default and give warning
                Debug.LogWarning("SoaConfigXMLReader::getBooleanAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                return defaultValue;
            }
            else
            {
                // Return actual value
                return Convert.ToBoolean(node.Attributes[attribute].Value);
            }
        }

        static int GetIntAttribute(XmlNode node, string attribute, int defaultValue)
        {
            if (node.Attributes[attribute] == null)
            {
                // Use default and give warning
                Debug.LogWarning("SoaConfigXMLReader::getIntAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                return defaultValue;
            }
            else
            {
                // Return actual value
                return Convert.ToInt32(node.Attributes[attribute].Value);
            }
        }

        static float GetFloatAttribute(XmlNode node, string attribute, float defaultValue)
        {
            if (node.Attributes[attribute] == null)
            {
                // Use default and give warning
                Debug.LogWarning("SoaConfigXMLReader::getFloatAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                return defaultValue;
            }
            else
            {
                // Return actual value
                return (float) Convert.ToDouble(node.Attributes[attribute].Value);
            }
        }

        static string GetStringAttribute(XmlNode node, string attribute, string defaultValue)
        {
            if (node.Attributes[attribute] == null)
            {
                // Use default and give warning
                Debug.LogWarning("SoaConfigXMLReader::getStringAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                return defaultValue;
            }
            else
            {
                // Return actual value
                return node.Attributes[attribute].Value;
            }
        }

    }
}

