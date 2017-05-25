using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

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
            catch (Exception e)
            {
                // Handle error if not found
                Debug.LogError("SoaConfigXMLReader::Parse(): Could not load " + xmlFilename + " because " + e.Message);
                return null;
            }

            // Find the "SoaConfig" category
            XmlNode configNode = null;
            foreach (XmlNode c in xmlDoc.ChildNodes)
            {
                if (c.Name == "SoaConfig")
                {
                    configNode = c;
                    break;
                }
            }

            // Check if we actually found it
            if (configNode == null)
            {
                // Handle error if not found
                Debug.LogError("SoaConfigXMLReader::Parse(): Could not find \"SoaConfig\" node");
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
                    case "Logger":
                        // Logger configuration
                        ParseLogger(c, soaConfig);
                        break;
                    case "Simulation":
                        // Simulation configuration
                        ParseSimulation(c, soaConfig);
                        break;
                    case "FuelDefaults":
                        // Fuel default configuration
                        ParseFuelDefaults(c, soaConfig);
                        break;
                    case "StorageDefaults":
                        // Storage default configuration
                        ParseStorageDefaults(c, soaConfig);
                        break;
                    case "SensorDefaults":
                        // Sensor default configuration
                        ParseBeamwidthDefaults(c, soaConfig.defaultSensorBeamwidths);
                        ParsePerceptionDefaults(c, soaConfig.defaultSensorModalities);
                        break;
                    case "ClassifierDefaults":
                        // Classifier default configuration
                        ParsePerceptionDefaults(c, soaConfig.defaultClassifierModalities);
                        break;
                    case "CommsDefaults":
                        // Comms default configuration
                        ParseCommsDefaults(c, soaConfig.defaultCommsRanges);
                        break;
                    case "JammerDefaults":
                        // Jammer default configuration
                        ParseJammerDefaults(c, soaConfig.defaultJammerRanges);
                        break;
                }
            }

            // Then look at sites, local, and remote platforms whose defaults may
            // use information from previously parsed categories
            // (Note: This step should be done last)
            foreach (XmlNode c in configNode.ChildNodes)
            {
                // Parse differently depending on which category we are in
                switch (c.Name)
                {
                    case "Sites":
                        // Sites configuration
                        ParseSites(c, soaConfig);
                        break;
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

        #region Category Parsing Functions
        /********************************************************************************************
         *                               CATEGORY PARSING FUNCTIONS                                 *
         ********************************************************************************************/

        // Network configuration category parsing
        private static void ParseNetwork(XmlNode node, SoaConfig soaConfig)
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

        // Logger configuration category parsing
        private static void ParseLogger(XmlNode node, SoaConfig soaConfig)
        {
            // Pull attributes directly from the node
            try
            {
                soaConfig.loggerOutputFile = GetStringAttribute(node, "outputFile", "SoaSimOutput.xml");
                soaConfig.enableLogToFile = GetBooleanAttribute(node, "enableLogToFile", true);
                soaConfig.enableLogEventsToFile = GetBooleanAttribute(node, "enableLogEventsToFile", true);
                soaConfig.enableLogToUnityConsole = GetBooleanAttribute(node, "enableLogToUnityConsole", true);
            }
            catch (Exception)
            {
                Debug.LogError("SoaConfigXMLReader::ParseLogger(): Error parsing " + node.Name);
            }
        }

        // Simulation configuration category parsing
        private static void ParseSimulation(XmlNode node, SoaConfig soaConfig)
        {
            // Pull attributes directly from the node
            try
            {
                soaConfig.simulationRandomSeed = GetIntAttribute(node, "simulationRandomSeed", 0);
                soaConfig.gameDurationHr = GetFloatAttribute(node,"gameDurationHr", 15.0f);
                soaConfig.probRedDismountHasWeapon = GetFloatAttribute(node, "probRedDismountHasWeapon", 0.5f);
                soaConfig.probRedTruckHasWeapon = GetFloatAttribute(node, "probRedTruckHasWeapon", 0.5f);
                soaConfig.probRedTruckHasJammer = GetFloatAttribute(node, "probRedTruckHasJammer", 0.5f);
                soaConfig.controlUpdateRate_s = GetFloatAttribute(node, "controlUpdateRate_s", 0.1f);
                soaConfig.predRedMovement = GetIntAttribute(node, "predRedMovement", 0);
            }
            catch (Exception)
            {
                Debug.LogError("SoaConfigXMLReader::ParseSimulation(): Error parsing " + node.Name);
            }
        }

        // Site configuration category parsing
        private static void ParseSites(XmlNode node, SoaConfig soaConfig)
        {
            SiteConfig newConfig = null; // Dummy value
            // Go through each child node
            bool newConfigValid;
            foreach (XmlNode c in node.ChildNodes)
            {
                newConfigValid = true;

                try
                {
                    switch (c.Name)
                    {
                        case "BlueBase":
                            {
                                newConfig = new BlueBaseConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    GetFloatAttribute(c, "z_km", 0),
                                    GetStringAttribute(c, "name", null),
                                    GetOptionalFloatAttribute(c, "commsRange_km")    
                                );
                            }
                            break;
                        case "RedBase":
                            {
                                newConfig = new RedBaseConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    GetFloatAttribute(c, "z_km", 0),
                                    GetStringAttribute(c, "name", null)
                                );
                            }
                            break;
                        case "NGOSite":
                            {
                                newConfig = new NGOSiteConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    GetFloatAttribute(c, "z_km", 0),
                                    GetStringAttribute(c, "name", null)
                                );
                            }
                            break;
                        case "Village":
                            {
                                newConfig = new VillageConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    GetFloatAttribute(c, "z_km", 0),
                                    GetStringAttribute(c, "name", null)
                                );
                            }
                            break;
                        default:
                            newConfigValid = false;
                            if (c.Name != "#comment")
                            {
                                Debug.LogWarning("SoaConfigXMLReader::ParseSites(): Unrecognized node " + c.Name);
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    Debug.LogError("SoaConfigXMLReader::ParseSites(): Error parsing " + c.Name);
                } // End try-catch

                // Add to list of sites if valid
                if (newConfigValid)
                {
                    soaConfig.sites.Add(newConfig);
                }
            } // End foreach
        }

        // Fuel default configuration category parsing
        private static void ParseFuelDefaults(XmlNode node, SoaConfig soaConfig)
        {
            // Pull attributes directly from the node
            try
            {
                soaConfig.defaultHeavyUAVFuelTankSize_s = GetFloatAttribute(node, "heavyUAVFuelTankSize_s", 10000);
                soaConfig.defaultSmallUAVFuelTankSize_s = GetFloatAttribute(node, "smallUAVFuelTankSize_s", 40000);
            }
            catch (Exception)
            {
                Debug.LogError("SoaConfigXMLReader::ParseFuelDefaults(): Error parsing " + node.Name);
            }
        }

        // Storage default configuration category parsing
        private static void ParseStorageDefaults(XmlNode node, SoaConfig soaConfig)
        {
            // Pull attributes directly from the node
            try
            {
                soaConfig.defaultHeavyUAVNumStorageSlots = GetIntAttribute(node, "heavyUAVNumStorageSlots", 1);
                soaConfig.defaultRedDismountNumStorageSlots = GetIntAttribute(node, "redDismountNumStorageSlots", 1);
                soaConfig.defaultRedTruckNumStorageSlots = GetIntAttribute(node, "redTruckNumStorageSlots", 1);
            }
            catch (Exception)
            {
                Debug.LogError("SoaConfigXMLReader::ParseStorageDefaults(): Error parsing " + node.Name);
            }
        }

        private static List<PerceptionModality> ParseModalities(XmlNode parentNode)
        {
            // List for storing parsed modes
            List<PerceptionModality> modes = new List<PerceptionModality>();

            // Go through each child node
            foreach (XmlNode c in parentNode.ChildNodes)
            {
                switch (c.Name)
                {
                    case "Mode":
                        {
                            modes.Add(new PerceptionModality(
                                GetStringAttribute(c, "tag", null),
                                GetFloatAttribute(c, "RangeP1_km", 0.0f),
                                GetFloatAttribute(c, "RangeMax_km", 0.0f)
                                ));
                            break;
                        }
                    default:
                        if (c.Name != "#comment")
                        {
                            Debug.LogWarning("SoaConfigXMLReader::ParseModalities(): Unrecognized node " + c.Name);
                        }
                        break;
                }
            }

            // Return mode list
            return modes;
        }

        private static void ParsePerceptionDefaults(XmlNode node, Dictionary<string, List<PerceptionModality>> d)
        {
            // Go through each child node
            foreach (XmlNode c in node.ChildNodes)
            {
                try
                {
                    switch (c.Name)
                    {
                        case "RedDismount":
                        case "RedTruck":
                        case "NeutralDismount":
                        case "NeutralTruck":
                        case "BluePolice":
                        case "HeavyUAV":
                        case "SmallUAV":
                        case "BlueBalloon":
                            d[c.Name] = ParseModalities(c);
                            break;
                        default:
                            if (c.Name != "#comment")
                            {
                                Debug.LogWarning("SoaConfigXMLReader::ParsePerceptionDefaults(): Unrecognized node " + c.Name);
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    Debug.LogError("SoaConfigXMLReader::ParsePerceptionDefaults(): Error parsing " + c.Name);
                }
            }
        }

        // Only heavy UAV, small UAV, and blue balloons have default beamwidths
        private static void ParseBeamwidthDefaults(XmlNode node, Dictionary<string, float> d)
        {
            // Go through each child node
            foreach (XmlNode c in node.ChildNodes)
            {
                switch (c.Name)
                {
                    case "RedDismount":
                    case "RedTruck":
                    case "NeutralDismount":
                    case "NeutralTruck":
                    case "BluePolice":
                        d[c.Name] = 360;
                        break;
                    case "HeavyUAV":
                    case "SmallUAV":
                    case "BlueBalloon":
                        d[c.Name] = GetFloatAttribute(c, "beamwidth_deg", 360); // Get user defined default
                        break;
                }
            }
        }

        // Parse comms defaults
        private static void ParseCommsDefaults(XmlNode node, Dictionary<string, float> d)
        {
            // Go through each child node
            foreach (XmlNode c in node.ChildNodes)
            {
                try
                {
                    switch (c.Name)
                    {
                        case "Node":
                            String s = GetStringAttribute(c, "tag", null);
                            if (s != null)
                            {
                                d[s] = GetFloatAttribute(c, "commsRange_km", 0.0f); // Get user defined default
                            }
                            break;
                        default:
                            if (c.Name != "#comment")
                            {
                                Debug.LogWarning("SoaConfigXMLReader::ParseCommsDefaults(): Unrecognized node " + c.Name);
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    Debug.LogError("SoaConfigXMLReader::ParseCommsDefaults(): Error parsing " + c.Name);
                }
            }
        }

        // Parse jammer defaults
        private static void ParseJammerDefaults(XmlNode node, Dictionary<string, float> d)
        {
            // Go through each child node
            foreach (XmlNode c in node.ChildNodes)
            {
                try
                {
                    switch (c.Name)
                    {
                        case "Node":
                            String s = GetStringAttribute(c, "tag", null);
                            if (s != null)
                            {
                                d[s] = GetFloatAttribute(c, "jammerRange_km", 0.0f); // Get user defined default
                            }
                            break;
                        default:
                            if (c.Name != "#comment")
                            {
                                Debug.LogWarning("SoaConfigXMLReader::ParseJammerDefaults(): Unrecognized node " + c.Name);
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    Debug.LogError("SoaConfigXMLReader::ParseJammerDefaults(): Error parsing " + c.Name);
                }
            }
        }

        // Local platform category parsing
        private static void ParseLocal(XmlNode node, SoaConfig soaConfig)
        {
            // Go through each child node
            PlatformConfig newConfig = new BluePoliceConfig(0.0f, 0.0f, 0.0f, new Optional<int>(), new Optional<float>(), new Optional<float>()); // Dummy value
            bool newConfigValid;
            foreach (XmlNode c in node.ChildNodes)
			{
                newConfigValid = true;

				try
				{
					switch (c.Name)
					{
                        case "RedDismount":
                            {
                                newConfig = new RedDismountConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    0, // No y_km field for land unit
                                    GetFloatAttribute(c, "z_km", 0),
                                    GetOptionalIntAttribute(c, "id"),
                                    GetOptionalFloatAttribute(c, "sensorBeamwidth_deg"),
                                    GetOptionalStringAttribute(c, "initialWaypoint"),
                                    GetOptionalBooleanAttribute(c, "hasWeapon"),
                                    GetOptionalFloatAttribute(c, "commsRange_km"),
                                    GetOptionalIntAttribute(c, "numStorageSlots")
                                );
                            }
                            break;
                        case "RedTruck":
                            {
                                newConfig = new RedTruckConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    0, // No y_km field for land unit
                                    GetFloatAttribute(c, "z_km", 0),
                                    GetOptionalIntAttribute(c, "id"),
                                    GetOptionalFloatAttribute(c, "sensorBeamwidth_deg"),
                                    GetOptionalStringAttribute(c, "initialWaypoint"),
                                    GetOptionalBooleanAttribute(c, "hasWeapon"),
                                    GetOptionalBooleanAttribute(c, "hasJammer"),
                                    GetOptionalFloatAttribute(c, "commsRange_km"),
                                    GetOptionalFloatAttribute(c, "jammerRange_km"),
                                    GetOptionalIntAttribute(c, "numStorageSlots")
                                );
                            }
                            break;
                        case "NeutralDismount":
                            {
                                newConfig = new NeutralDismountConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    0, // No y_km field for land unit
                                    GetFloatAttribute(c, "z_km", 0),
                                    GetOptionalIntAttribute(c, "id"),
                                    GetOptionalFloatAttribute(c, "sensorBeamwidth_deg")
                                );
                            }
                            break;
                        case "NeutralTruck":
                            {
                                newConfig = new NeutralTruckConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    0, // No y_km field for land unit
                                    GetFloatAttribute(c, "z_km", 0),
                                    GetOptionalIntAttribute(c, "id"),
                                    GetOptionalFloatAttribute(c, "sensorBeamwidth_deg")
                                );
                            }
                            break;
                        case "BluePolice":
                            {
                                newConfig = new BluePoliceConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    0, // No y_km field for land unit
                                    GetFloatAttribute(c, "z_km", 0),
                                    GetOptionalIntAttribute(c, "id"),
                                    GetOptionalFloatAttribute(c, "sensorBeamwidth_deg"),
                                    GetOptionalFloatAttribute(c, "commsRange_km")
                                );
                            }
                            break;
                        default:
                            newConfigValid = false;
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
                } // End try-catch

                // Handle and add the new config if it is valid
                if (newConfigValid)
                {
                    // Parse any sensor/classifier override modalities specified
                    foreach (XmlNode g in c.ChildNodes)
                    {
                        switch (g.Name)
                        {
                            case "Sensor":
                                newConfig.SetSensorModalities(ParseModalities(g));
                                break;
                            case "Classifier":
                                newConfig.SetClassifierModalities(ParseModalities(g));
                                break;
                        }
                    }

                    // Add to list of local platforms
                    soaConfig.localPlatforms.Add(newConfig);
                } // End if new config valid
            } // End foreach
        }

        // Remote platform category parsing
        private static void ParseRemote(XmlNode node, SoaConfig soaConfig)
        {
            PlatformConfig newConfig = null; // Dummy value
            bool newConfigValid;

            // Go through each child node
            foreach (XmlNode c in node.ChildNodes)
            {
                newConfigValid = true;

                try
                {
                    switch (c.Name)
                    {
                        case "HeavyUAV":
                            {
                                newConfig = new HeavyUAVConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    GetFloatAttribute(c, "y_km", 0),
                                    GetFloatAttribute(c, "z_km", 0),
                                    GetIntAttribute(c, "id", -1), // Default -1 means runtime determined id field
                                    GetOptionalFloatAttribute(c, "sensorBeamwidth_deg"),
                                    GetOptionalFloatAttribute(c, "commsRange_km"),
                                    GetOptionalFloatAttribute(c, "fuelTankSize_s"),
                                    GetOptionalIntAttribute(c, "numStorageSlots")
                                );
                            }
                            break;
                        case "SmallUAV":
                            {
                                newConfig = new SmallUAVConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    GetFloatAttribute(c, "y_km", 0),
                                    GetFloatAttribute(c, "z_km", 0),
                                    GetIntAttribute(c, "id", -1), // Default -1 means runtime determined id field
                                    GetOptionalFloatAttribute(c, "sensorBeamwidth_deg"),
                                    GetOptionalFloatAttribute(c, "commsRange_km"),
                                    GetOptionalFloatAttribute(c, "fuelTankSize_s")
                                );
                            }
                            break;
                        case "BlueBalloon":
                            {
                                // Get list of waypoints
                                List<PrimitivePair<float, float>> waypoints_km = new List<PrimitivePair<float, float>>();
                                foreach(XmlNode g in c.ChildNodes){
                                    switch (g.Name)
                                    {
                                        case "Waypoint":
                                            waypoints_km.Add(new PrimitivePair<float, float>(GetFloatAttribute(g, "x_km", 0), GetFloatAttribute(g, "z_km", 0)));
                                            break;                                            
                                    }
                                }

                                // Create a Blue Balloon config
                                newConfig = new BlueBalloonConfig(
                                    GetIntAttribute(c, "id", -1), // Default -1 means runtime determined id field
                                    GetOptionalFloatAttribute(c, "sensorBeamwidth_deg"),
                                    waypoints_km,
                                    GetBooleanAttribute(c, "teleportLoop", true)
                                );
                            }
                            break;
                        default:
                            newConfigValid = false;
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
                } // End try-catch

                // Handle and add the new config if it is valid
                if (newConfigValid)
                {
                    // Parse any sensor/classifier override modalities specified
                    foreach (XmlNode g in c.ChildNodes)
                    {
                        switch (g.Name)
                        {
                            case "Sensor":
                                newConfig.SetSensorModalities(ParseModalities(g));
                                break;
                            case "Classifier":
                                newConfig.SetClassifierModalities(ParseModalities(g));
                                break;
                        }
                    }

                    // Add to list of remote platforms
                    soaConfig.remotePlatforms.Add(newConfig);
                } // End if new config valid
            } // End foreach
        }
        #endregion

        #region Attribute Reading Helper Functions
        /********************************************************************************************
         *                            ATTRIBUTE READING HELPER FUNCTIONS                            *
         ********************************************************************************************/
        private static bool GetBooleanAttribute(XmlNode node, string attribute, bool defaultValue)
        {
            if (node.Attributes[attribute] == null)
            {
                // Use default and give error
                Debug.LogError("SoaConfigXMLReader::getBooleanAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                return defaultValue;
            }
            else
            {
                // Return actual value
                return Convert.ToBoolean(node.Attributes[attribute].Value);
            }
        }

        private static int GetIntAttribute(XmlNode node, string attribute, int defaultValue)
        {
            if (node.Attributes[attribute] == null)
            {
                // Use default and give error
                Debug.LogError("SoaConfigXMLReader::getIntAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
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
                // Use default and give error
                Debug.LogError("SoaConfigXMLReader::getFloatAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                return defaultValue;
            }
            else
            {
                // Return actual value
                return Convert.ToSingle(node.Attributes[attribute].Value);
            }
        }

        private static string GetStringAttribute(XmlNode node, string attribute, string defaultValue)
        {
            if (node.Attributes[attribute] == null)
            {
                // Use default and give error
                Debug.LogError("SoaConfigXMLReader::getStringAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                return defaultValue;
            }
            else
            {
                // Return actual value
                return node.Attributes[attribute].Value;
            }
        }
        #endregion

        #region Optional Attribute Reading Methods
        private static Optional<bool> GetOptionalBooleanAttribute(XmlNode node, string attribute)
        {
            if (node.Attributes[attribute] == null)
            {
                // Not found, set as empty
                return new Optional<bool>();
            }
            else
            {
                // Return actual value
                return new Optional<bool>(Convert.ToBoolean(node.Attributes[attribute].Value));
            }
        }

        private static Optional<int> GetOptionalIntAttribute(XmlNode node, string attribute)
        {
            if (node.Attributes[attribute] == null)
            {
                // Not found, set as empty
                return new Optional<int>();
            }
            else
            {
                // Return actual value
                return new Optional<int>(Convert.ToInt32(node.Attributes[attribute].Value));
            }
        }

        private static Optional<float> GetOptionalFloatAttribute(XmlNode node, string attribute)
        {
            if (node.Attributes[attribute] == null)
            {
                // Not found, set as empty
                return new Optional<float>();
            }
            else
            {
                // Return actual value
                return new Optional<float>(Convert.ToSingle(node.Attributes[attribute].Value));
            }
        }

        private static Optional<string> GetOptionalStringAttribute(XmlNode node, string attribute)
        {
            if (node.Attributes[attribute] == null)
            {
                // Not found, set as empty
                return new Optional<string>();
            }
            else
            {
                // Return actual value
                return new Optional<string>(node.Attributes[attribute].Value);
            }
        }
        #endregion
    }
}

