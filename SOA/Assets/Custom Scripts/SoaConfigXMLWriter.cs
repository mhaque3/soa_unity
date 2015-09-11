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
    class SoaConfigXMLWriter
    {
        public static void Write(SoaConfig soaConfig, string xmlFilename)
        {
            // Create new XML document
            XmlDocument xmlDoc = new XmlDocument();

            // Create "SoaConfig" node
            XmlNode configNode = xmlDoc.CreateElement("SoaConfig");
            xmlDoc.AppendChild(configNode);

            // Populate "Network" node
            PopulateNetwork(xmlDoc, configNode, soaConfig);

            // Populate "Logger" node
            PopulateLogger(xmlDoc, configNode, soaConfig);

            // Populate "Simulation" node
            PopulateSimulation(xmlDoc, configNode, soaConfig);

            // Populate "Local" node
            PopulateLocal(xmlDoc, configNode, soaConfig);

            // Populate "Remote" node
            PopulateRemote(xmlDoc, configNode, soaConfig);

            // Write to file
            xmlDoc.Save(xmlFilename);
        }

        private static void AddAttribute(XmlDocument xmlDoc, XmlNode node, string attribute, string value)
        {
            XmlAttribute newAttribute = xmlDoc.CreateAttribute(attribute);
            newAttribute.Value = value;
            node.Attributes.Append(newAttribute);
        }

        private static void PopulateNetwork(XmlDocument xmlDoc, XmlNode configNode, SoaConfig soaConfig)
        {
            // Create "Network" node and append to configNode
            XmlNode node = xmlDoc.CreateElement("Network");
            configNode.AppendChild(node);

            // Add attributes
            AddAttribute(xmlDoc, node, "redRoom",  soaConfig.networkRedRoom);
            AddAttribute(xmlDoc, node, "blueRoom", soaConfig.networkBlueRoom);
        }

        private static void PopulateLogger(XmlDocument xmlDoc, XmlNode configNode, SoaConfig soaConfig)
        {
            // Create "Logger" node and append to configNode
            XmlNode node = xmlDoc.CreateElement("Logger");
            configNode.AppendChild(node);

            // Add attributes
            AddAttribute(xmlDoc, node, "outputFile",              soaConfig.loggerOutputFile);
            AddAttribute(xmlDoc, node, "enableLogToFile",         soaConfig.enableLogToFile);
            AddAttribute(xmlDoc, node, "enableLogEventsToFile",   soaConfig.enableLogEventsToFile);
            AddAttribute(xmlDoc, node, "enableLogToUnityConsole", soaConfig.enableLogToUnityConsole);
        }

        private static void PopulateSimulation(XmlDocument xmlDoc, XmlNode configNode, SoaConfig soaConfig)
        {
            // Create "Simulation" node and append to configNode
            XmlNode node = xmlDoc.CreateElement("Simulation");
            configNode.AppendChild(node);

            // Add attributes
            AddAttribute(xmlDoc, node, "probRedDismountWeaponized", soaConfig.probRedDismountWeaponized);
            AddAttribute(xmlDoc, node, "probRedTruckWeaponized",    soaConfig.probRedTruckWeaponized);
        }

        private static void PopulateLocal(XmlDocument xmlDoc, XmlNode configNode, SoaConfig soaConfig)
        {
            // Create "Local" node and append to configNode
            XmlNode node = xmlDoc.CreateElement("Local");
            configNode.AppendChild(node);

            // Go through each local platform and add children to "Local" node
            foreach (PlatformConfig p in soaConfig.localPlatforms)
            {
                switch(p.GetConfigType())
                {
                    case PlatformConfig.ConfigType.RED_DISMOUNT:
                        PopulateRedDismount(node, (RedDismountConfig) p);
                        break;
                    case PlatformConfig.ConfigType.RED_TRUCK:
                        PopulateRedTruck(node, (RedTruckConfig) p);
                        break;
                    case PlatformConfig.ConfigType.NEUTRAL_DISMOUNT:
                        PopulateNeutralDismount(node, (NeutralDismountConfig) p);
                        break;
                    case PlatformConfig.ConfigType.NEUTRAL_TRUCK:
                        PopulateNeutralTruck(node, (NeutralTruckConfig) p);
                        break;
                    case PlatformConfig.ConfigType.BLUE_POLICE:
                        PopulateBluePolice(node, (BluePoliceConfig) p);
                        break;
                    default:
                        #if(UNITY_STANDALONE)
                        Debug.LogError("SoaConfigXMLWriter::PopulateLocal(): Unrecognized config type " + p.GetConfigType() + " in soaConfig.localPlatforms");
                        #else
                        Console.WriteLine("SoaConfigXMLWriter::PopulateLocal(): Unrecognized config type " + p.GetConfigType() + " in soaConfig.localPlatforms");
                        #endif
                        break;
                }
            }
        }

        private static void PopulateRemote(XmlDocument xmlDoc, XmlNode configNode, SoaConfig soaConfig)
        {
            // Create "Remote" node and append to configNode
            XmlNode node = xmlDoc.CreateElement("Remote");
            configNode.AppendChild(node);

            // Go through each remote platform and add children to "Remote" node
            foreach (PlatformConfig p in soaConfig.remotePlatforms)
            {
                switch (p.GetConfigType())
                {
                    case PlatformConfig.ConfigType.HEAVY_UAV:
                        PopulateHeavyUAV(node, (HeavyUAVConfig) p);
                        break;
                    case PlatformConfig.ConfigType.SMALL_UAV:
                        PopulateSmallUAV(node, (SmallUAVConfig) p);
                        break;
                    case PlatformConfig.ConfigType.BALLOON:
                        PopulateBalloon(node, (BalloonConfig) p);
                        break;
                    default:
                        #if(UNITY_STANDALONE)
                        Debug.LogError("SoaConfigXMLWriter::PopulateRemote(): Unrecognized config type " + p.GetConfigType() + " in soaConfig.remotePlatforms");
                        #else
                        Console.WriteLine("SoaConfigXMLWriter::PopulateRemote(): Unrecognized config type " + p.GetConfigType() + " in soaConfig.remotePlatforms");
                        #endif
                        break;
                }
            }
        }

        private static void AddPlatformConfigAttributes(XmlDocument xmlDoc, XmlNode parentNode, PlatformConfig c)
        {
            // Helper function to add attributes common to all platform configs
            AddAttribute(xmlDoc, node, "x_km", c.x_km.ToString());
            AddAttribute(xmlDoc, node, "y_km", c.y_km.ToString());
            AddAttribute(xmlDoc, node, "z_km", c.z_km.ToString());
            AddAttribute(xmlDoc, node, "id", c.id.ToString());
        }

        #region Local Platforms
        private static void PopulateRedDismount(XmlDocument xmlDoc, XmlNode parentNode, RedDismountConfig c)
        {
            // Create "Red Dismount" node and append to parentNode
            XmlNode node = xmlDoc.CreateElement("Red Dismount");
            parentNode.AppendChild(node);

            // Add attributes
            addPlatformConfigAttributes(xmlDoc, node, c);
            AddAttribute(xmlDoc, node, "hasWeapon", c.hasWeapon.ToString());
            AddAttribute(xmlDoc, node, "initialWaypoint", c.initialWaypoint);
        }

        private static void PopulateRedTruck(XmlDocument xmlDoc, XmlNode parentNode, RedTruckConfig c)
        {
            // Create "Red Truck" node and append to parentNode
            XmlNode node = xmlDoc.CreateElement("Red Truck");
            parentNode.AppendChild(node);

            // Add attributes
            addPlatformConfigAttributes(xmlDoc, node, c);
            AddAttribute(xmlDoc, node, "hasWeapon", c.hasWeapon.ToString());
            AddAttribute(xmlDoc, node, "initialWaypoint", c.initialWaypoint);
        }

        private static void PopulateNeutralDismount(XmlDocument xmlDoc, XmlNode parentNode, NeutralDismountConfig c)
        {
            // Create "NeutralDismount" node and append to parentNode
            XmlNode node = xmlDoc.CreateElement("NeutralDismount");
            parentNode.AppendChild(node);

            // Add attributes
            addPlatformConfigAttributes(xmlDoc, node, c);
        }

        private static void PopulateNeutralTruck(XmlDocument xmlDoc, XmlNode parentNode, NeutralTruckConfig c)
        {
            // Create "NeutralTruck" node and append to parentNode
            XmlNode node = xmlDoc.CreateElement("NeutralTruck");
            parentNode.AppendChild(node);

            // Add attributes
            addPlatformConfigAttributes(xmlDoc, node, c);
        }

        private static void PopulateBluePolice(XmlDocument xmlDoc, XmlNode parentNode, BluePoliceConfig c)
        {
            // Create "BluePolice" node and append to parentNode
            XmlNode node = xmlDoc.CreateElement("BluePolice");
            parentNode.AppendChild(node);

            // Add attributes
            addPlatformConfigAttributes(xmlDoc, node, c);
        }
        #endregion

        #region Remote Platforms
        private static void PopulateHeavyUAV(XmlDocument xmlDoc, XmlNode parentNode, HeavyUAVConfig c)
        {
            // Create "HeavyUAV" node and append to parentNode
            XmlNode node = xmlDoc.CreateElement("HeavyUAV");
            parentNode.AppendChild(node);

            // Add attributes
            addPlatformConfigAttributes(xmlDoc, node, c);
        }

        private static void PopulateSmallUAV(XmlDocument xmlDoc, XmlNode parentNode, SmallUAVConfig c)
        {
            // Create "SmallUAV" node and append to parentNode
            XmlNode node = xmlDoc.CreateElement("SmallUAV");
            parentNode.AppendChild(node);

            // Add attributes
            addPlatformConfigAttributes(xmlDoc, node, c);
        }

        private static void PopulateBalloon(XmlDocument xmlDoc, XmlNode parentNode, BalloonConfig c)
        {
            // Create "Balloon" node and append to parentNode
            XmlNode node = xmlDoc.CreateElement("Balloon");
            parentNode.AppendChild(node);

            // Add attributes
            addPlatformConfigAttributes(xmlDoc, node, c);
        }
        #endregion
    }
}

