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
                #if(UNITY_STANDALONE)
                Debug.LogError("SoaConfigXMLReader::Parse(): Could not load " + xmlFilename + " because " + e.Message);
                #else
                Console.WriteLine("SoaConfigXMLReader::Parse(): Could not load " + xmlFilename + " because " + e.Message);
                #endif
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
                #if(UNITY_STANDALONE)
                Debug.LogError("SoaConfigXMLReader::Parse(): Could not find \"SoaConfig\" node");
                #else
                Console.WriteLine("SoaConfigXMLReader::Parse(): Could not find \"SoaConfig\" node");
                #endif
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
                    case "Fuel":
                        // Fuel configuration
                        ParseFuel(c, soaConfig);
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
                #if(UNITY_STANDALONE)
                Debug.LogError("SoaConfigXMLReader::ParseNetwork(): Error parsing " + node.Name);
                #else
                Console.WriteLine("SoaConfigXMLReader::ParseNetwork(): Error parsing " + node.Name);
                #endif
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
                #if(UNITY_STANDALONE)
                Debug.LogError("SoaConfigXMLReader::ParseLogger(): Error parsing " + node.Name);
                #else
                Console.WriteLine("SoaConfigXMLReader::ParseLogger(): Error parsing " + node.Name);
                #endif
            }
        }

        // Simulation configuration category parsing
        private static void ParseSimulation(XmlNode node, SoaConfig soaConfig)
        {
            // Pull attributes directly from the node
            try
            {
                soaConfig.gameDurationHr = GetFloatAttribute(node,"gameDurationHr", 15.0f);
                soaConfig.probRedDismountHasWeapon = GetFloatAttribute(node, "probRedDismountHasWeapon", 0.5f);
                soaConfig.probRedTruckHasWeapon = GetFloatAttribute(node, "probRedTruckHasWeapon", 0.5f);
                soaConfig.probRedTruckHasJammer = GetFloatAttribute(node, "probRedTruckHasJammer", 0.5f);
            }
            catch (Exception)
            {
                #if(UNITY_STANDALONE)
                Debug.LogError("SoaConfigXMLReader::ParseSimulation(): Error parsing " + node.Name);
                #else
                Console.WriteLine("SoaConfigXMLReader::ParseSimulation(): Error parsing " + node.Name);
                #endif
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
                                    soaConfig.defaultCommsRanges["BlueBase"] // Lookup direclty from defaults since there is only ever one blue base
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
                                #if(UNITY_STANDALONE)
                                Debug.LogWarning("SoaConfigXMLReader::ParseSites(): Unrecognized node " + c.Name);
                                #else
                                Console.WriteLine("SoaConfigXMLReader::ParseSites(): Unrecognized node " + c.Name);
                                #endif
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    #if(UNITY_STANDALONE)
                    Debug.LogError("SoaConfigXMLReader::ParseSites(): Error parsing " + c.Name);
                    #else
                    Console.WriteLine("SoaConfigXMLReader::ParseSites(): Error parsing " + c.Name);
                    #endif
                } // End try-catch

                // Add to list of sites if valid
                if (newConfigValid)
                {
                    soaConfig.sites.Add(newConfig);
                }
            } // End foreach
        }

        // Fuel configuration category parsing
        private static void ParseFuel(XmlNode node, SoaConfig soaConfig)
        {
            // Pull attributes directly from the node
            try
            {
                soaConfig.heavyUAVFuelTankSize_s = GetFloatAttribute(node, "heavyUAVFuelTankSize_s", 10000);
                soaConfig.smallUAVFuelTankSize_s = GetFloatAttribute(node, "smallUAVFuelTankSize_s", 40000);
            }
            catch (Exception)
            {
                #if(UNITY_STANDALONE)
                Debug.LogError("SoaConfigXMLReader::ParseFuel(): Error parsing " + node.Name);
                #else
                Console.WriteLine("SoaConfigXMLReader::ParseFuel(): Error parsing " + node.Name);
                #endif
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
                            #if(UNITY_STANDALONE)
                            Debug.LogWarning("SoaConfigXMLReader::ParseModalities(): Unrecognized node " + c.Name);
                            #else
                            Console.WriteLine("SoaConfigXMLReader::ParseModalities(): Unrecognized node " + c.Name);
                            #endif
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
                                #if(UNITY_STANDALONE)
                                Debug.LogWarning("SoaConfigXMLReader::ParsePerceptionDefaults(): Unrecognized node " + c.Name);
                                #else
                                Console.WriteLine("SoaConfigXMLReader::ParsePerceptionDefaults(): Unrecognized node " + c.Name);
                                #endif
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    #if(UNITY_STANDALONE)
                    Debug.LogError("SoaConfigXMLReader::ParsePerceptionDefaults(): Error parsing " + c.Name);
                    #else
                    Console.WriteLine("SoaConfigXMLReader::ParsePerceptionDefaults(): Error parsing " + c.Name);
                    #endif
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
                        d[c.Name] = 360; // Hardcoded as isotropic, can't change
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
                                #if(UNITY_STANDALONE)
                                Debug.LogWarning("SoaConfigXMLReader::ParseCommsDefaults(): Unrecognized node " + c.Name);
                                #else
                                Console.WriteLine("SoaConfigXMLReader::ParseCommsDefaults(): Unrecognized node " + c.Name);
                                #endif
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    #if(UNITY_STANDALONE)
                    Debug.LogError("SoaConfigXMLReader::ParseCommsDefaults(): Error parsing " + c.Name);
                    #else
                    Console.WriteLine("SoaConfigXMLReader::ParseCommsDefaults(): Error parsing " + c.Name);
                    #endif
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
                                #if(UNITY_STANDALONE)
                                Debug.LogWarning("SoaConfigXMLReader::ParseJammerDefaults(): Unrecognized node " + c.Name);
                                #else
                                Console.WriteLine("SoaConfigXMLReader::ParseJammerDefaults(): Unrecognized node " + c.Name);
                                #endif
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    #if(UNITY_STANDALONE)
                    Debug.LogError("SoaConfigXMLReader::ParseJammerDefaults(): Error parsing " + c.Name);
                    #else
                    Console.WriteLine("SoaConfigXMLReader::ParseJammerDefaults(): Error parsing " + c.Name);
                    #endif
                }
            }
        }

        // Local platform category parsing
        private static void ParseLocal(XmlNode node, SoaConfig soaConfig)
        {
            // Random number generator
            System.Random rand = new System.Random();

            // Go through each child node
            PlatformConfig newConfig = new BluePoliceConfig(0.0f, 0.0f, 0.0f, -1, 0.0f); // Dummy value
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
                                    -1, // Runtime determined id field
                                    GetStringAttribute(c, "initialWaypoint", null),
                                    GetBooleanAttribute(c, "hasWeapon", rand.NextDouble() <= soaConfig.probRedDismountHasWeapon),
                                    GetFloatAttribute(c, "commsRange_km", soaConfig.defaultCommsRanges["RedDismount"])
                                );
                            }
                            break;
                        case "RedTruck":
                            {
                                newConfig = new RedTruckConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    0, // No y_km field for land unit
                                    GetFloatAttribute(c, "z_km", 0),
                                    -1, // Runtime determined id field
                                    GetStringAttribute(c, "initialWaypoint", null),
                                    GetBooleanAttribute(c, "hasWeapon", rand.NextDouble() <= soaConfig.probRedTruckHasWeapon),
                                    GetBooleanAttribute(c, "hasJammer", rand.NextDouble() <= soaConfig.probRedTruckHasJammer),
                                    GetFloatAttribute(c, "commsRange_km", soaConfig.defaultCommsRanges["RedTruck"]),
                                    GetFloatAttribute(c, "jammerRange_km", soaConfig.defaultCommsRanges["RedTruck"])
                                );
                            }
                            break;
                        case "NeutralDismount":
                            {
                                newConfig = new NeutralDismountConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    0, // No y_km field for land unit
                                    GetFloatAttribute(c, "z_km", 0),
                                    -1 // Runtime determined id field
                                );
                            }
                            break;
                        case "NeutralTruck":
                            {
                                newConfig = new NeutralTruckConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    0, // No y_km field for land unit
                                    GetFloatAttribute(c, "z_km", 0),
                                    -1 // Runtime determined id field
                                );
                            }
                            break;
                        case "BluePolice":
                            {
                                newConfig = new BluePoliceConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    0, // No y_km field for land unit
                                    GetFloatAttribute(c, "z_km", 0),
                                    -1, // Runtime determined id field
                                    GetFloatAttribute(c, "commsRange_km", soaConfig.defaultCommsRanges["BluePolice"])
                                );
                            }
                            break;
                        default:
                            newConfigValid = false;
                            if(c.Name != "#comment")
                            {
                                #if(UNITY_STANDALONE)
                                Debug.LogWarning("SoaConfigXMLReader::ParseLocal(): Unrecognized node " + c.Name);
                                #else
                                Console.WriteLine("SoaConfigXMLReader::ParseLocal(): Unrecognized node " + c.Name);
                                #endif
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    #if(UNITY_STANDALONE)
                    Debug.LogError("SoaConfigXMLReader::ParseLocal(): Error parsing " + c.Name);
                    #else
                    Console.WriteLine("SoaConfigXMLReader::ParseLocal(): Error parsing " + c.Name);
                    #endif
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
                                    GetIntAttribute(c, "id", -1),
                                    GetFloatAttribute(c, "commsRange_km", soaConfig.defaultCommsRanges["HeavyUAV"]),
                                    soaConfig.heavyUAVFuelTankSize_s
                                );
                            }
                            break;
                        case "SmallUAV":
                            {
                                newConfig = new SmallUAVConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    GetFloatAttribute(c, "y_km", 0),
                                    GetFloatAttribute(c, "z_km", 0),
                                    GetIntAttribute(c, "id", -1),
                                    GetFloatAttribute(c, "commsRange_km", soaConfig.defaultCommsRanges["SmallUAV"]),
                                    soaConfig.smallUAVFuelTankSize_s
                                );
                            }
                            break;
                        case "BlueBalloon":
                            {
                                newConfig = new BlueBalloonConfig(
                                    GetFloatAttribute(c, "x_km", 0),
                                    15, // Balloon only has one valid altitude, will get set upon instantiation
                                    GetFloatAttribute(c, "z_km", 0),
                                    GetIntAttribute(c, "id", -1)
                                );
                            }
                            break;
                        default:
                            newConfigValid = false;
                            if (c.Name != "#comment")
                            {
                                #if(UNITY_STANDALONE)
                                Debug.LogWarning("SoaConfigXMLReader::ParseRemote(): Unrecognized node " + c.Name);
                                #else
                                Console.WriteLine("SoaConfigXMLReader::ParseRemote(): Unrecognized node " + c.Name);
                                #endif
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    #if(UNITY_STANDALONE)
                    Debug.LogError("SoaConfigXMLReader::ParseRemote(): Error parsing " + c.Name);
                    #else
                    Console.WriteLine("SoaConfigXMLReader::ParseRemote(): Error parsing " + c.Name);
                    #endif
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
                                newConfig.SetSensorBeamwidth(GetFloatAttribute(g, "beamwidth_deg", 360));
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
                // Use default and give warning
                #if(UNITY_STANDALONE)
                Debug.LogWarning("SoaConfigXMLReader::getBooleanAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                #else
                Console.WriteLine("SoaConfigXMLReader::getBooleanAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                #endif
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
                // Use default and give warning
                #if(UNITY_STANDALONE)
                Debug.LogWarning("SoaConfigXMLReader::getIntAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                #else
                Console.WriteLine("SoaConfigXMLReader::getIntAttribute(): Could not find attribute " +
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
                Debug.LogWarning("SoaConfigXMLReader::getFloatAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                #else
                Console.WriteLine("SoaConfigXMLReader::getFloatAttribute(): Could not find attribute " +
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

        private static string GetStringAttribute(XmlNode node, string attribute, string defaultValue)
        {
            if (node.Attributes[attribute] == null)
            {
                // Use default and give warning
                #if(UNITY_STANDALONE)
                Debug.LogWarning("SoaConfigXMLReader::getStringAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                #else
                Console.WriteLine("SoaConfigXMLReader::getStringAttribute(): Could not find attribute " +
                    attribute + " in node " + node.Name + ", using default value of " + defaultValue);
                #endif
                return defaultValue;
            }
            else
            {
                // Return actual value
                return node.Attributes[attribute].Value;
            }
        }
        #endregion
    }
}

