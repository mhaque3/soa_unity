using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace soa
{
    public class SoaEventLogger
    {
        // Timekeeping
        private static System.DateTime epoch = new System.DateTime(1970, 1, 1);
 
        // XML structure
        private XmlDocument xmlDoc;
        private XmlNode simulationNode;
        private XmlNode errorsNode;
        private XmlNode resultNode;
        private XmlNode eventsNode;

        // Options
        private string outputFile;
        private bool logToFile, logEventsToFile, logToConsole;

        // Counters
        private int countSupplyDelivery;
        private int countRedTruckCaptured;
        private int countRedDismountCaptured;
        private int countCasualtyDelivered;
        private int countHeavyUAVLost;
        private int countSmallUAVLost;
        private int countCivilianInRedCustody;

        public SoaEventLogger(string outputFile, string configFile, bool logToFile, bool logEventsToFile, bool logToConsole)
        {
            // Save whether the logger is enabled
            this.logToFile = logToFile;
            this.logEventsToFile = logEventsToFile;
            this.logToConsole = logToConsole;

            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            // Reset counters
            countSupplyDelivery = 0;
            countRedTruckCaptured = 0;
            countRedDismountCaptured = 0;
            countCasualtyDelivered = 0;
            countHeavyUAVLost = 0;
            countSmallUAVLost = 0;
            countCivilianInRedCustody = 0;

            // Only take action if logger is enabled
            if (logToFile)
            {
                // Save log output filename
                this.outputFile = outputFile;

                // Create a XML document to log data with
                xmlDoc = new XmlDocument();
                
                // Create "Log" node
                XmlNode logNode = xmlDoc.CreateElement("Log");
                xmlDoc.AppendChild(logNode);

                // Create "Simulation" node
                simulationNode = xmlDoc.CreateElement("Simulation");
                logNode.AppendChild(simulationNode);

                // Populate with attributes decribing the simulation
                AddAttribute(simulationNode, "configFile", configFile);
                AddAttribute(simulationNode, "startDateTime", now.ToString("MM\\/dd\\/yyyy hh\\:mm\\:ss.ffffff"));
                AddAttribute(simulationNode, "startTimeStamp", timeStamp);

                // Create "Errors" node
                errorsNode = xmlDoc.CreateElement("Errors");
                logNode.AppendChild(errorsNode);

                // Create "Results" node
                resultNode = xmlDoc.CreateElement("Results");
                logNode.AppendChild(resultNode);

                // Create "Events" node
                if (logEventsToFile)
                {
                    eventsNode = xmlDoc.CreateElement("Events");
                    logNode.AppendChild(eventsNode);
                }
            }
            if (logToConsole)
            {
                Log.debug("SIMULATION (" + timeStamp + "): Loading config file " + configFile);
                Log.debug("SIMULATION (" + timeStamp + "): Start date/time " + now.ToString("MM\\/dd\\/yyyy hh\\:mm\\:ss.ffffff"));
            }
        }
 
        /******************************************* HELPER FUNCTIONS *****************************************/
        private string CreateTimestamp(DateTime now)
        {
            return ((UInt64)(now.ToUniversalTime() - epoch).Ticks / 10000).ToString();
        }
     
        private void AddAttribute(XmlNode node, string attribute, string value)
        {
            XmlAttribute newAttribute = xmlDoc.CreateAttribute(attribute);
            newAttribute.Value = value;
            node.Attributes.Append(newAttribute);
        }

        /********************************************** TERMINATION ********************************************/
        public void TerminateLogging()
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);
            
            if (logToFile)
            {
                // Log current time
                AddAttribute(simulationNode, "stopDateTime", now.ToString("MM\\/dd\\/yyyy hh\\:mm\\:ss.ffffff"));
                AddAttribute(simulationNode, "stopTimeStamp", timeStamp);

                // Log counts
                AddAttribute(resultNode, "supplyDelivery", countSupplyDelivery.ToString());
                AddAttribute(resultNode, "redTruckCaptured", countRedTruckCaptured.ToString());
                AddAttribute(resultNode, "redDismountCaptured", countRedDismountCaptured.ToString());
                AddAttribute(resultNode, "casualtyDelivered", countCasualtyDelivered.ToString());
                AddAttribute(resultNode, "heavyUAVLost", countHeavyUAVLost.ToString());
                AddAttribute(resultNode, "smallUAVLost", countSmallUAVLost.ToString());
                AddAttribute(resultNode, "civilianInRedCustody", countCivilianInRedCustody.ToString());

                // Write contents to file
                xmlDoc.Save(outputFile);
            }
            if (logToConsole)
            {
                // Output current time
                Log.debug("SIMULATION (" + timeStamp + "): Terminate date/time " + now.ToString("MM\\/dd\\/yyyy hh\\:mm\\:ss.ffffff"));

                // Output counts
                Log.debug("SIMULATION (" + timeStamp + "): Supplies delivered: " + countSupplyDelivery.ToString());
                Log.debug("SIMULATION (" + timeStamp + "): Red trucks captured: " + countRedTruckCaptured.ToString());
                Log.debug("SIMULATION (" + timeStamp + "): Red dismounts captured: " + countRedDismountCaptured.ToString());
                Log.debug("SIMULATION (" + timeStamp + "): Casualties delivered: " + countCasualtyDelivered.ToString());
                Log.debug("SIMULATION (" + timeStamp + "): Heavy UAVs lost: " + countHeavyUAVLost.ToString());
                Log.debug("SIMULATION (" + timeStamp + "): Small UAVs lost: " + countSmallUAVLost.ToString());
                Log.debug("SIMULATION (" + timeStamp + "): Civilians in red custody: " + countCivilianInRedCustody.ToString());
            }
        }

        /******************************************* PUBLIC LOGGING ********************************************/
        public void LogSupplyDelivered(string deliverer, string destination)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            // Increment counter
            countSupplyDelivery++;

            // Log to file
            if (logToFile && logEventsToFile)
            {
                // Create a "SupplyDelivery" node
                XmlNode node = xmlDoc.CreateElement("SupplyDelivery");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "deliverer", deliverer);
                AddAttribute(node, "destination", destination);

                // Add to"Events" node
                eventsNode.AppendChild(node);
            }
            if (logToConsole)
            {
                Log.debug("EVENT (" + timeStamp + "): Supply delivered by " + deliverer + " to " + destination);
            }
        }

        public void LogRedTruckCaptured(string capturer, string captured)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            // Increment counter
            countRedTruckCaptured++;

            // Log to file
            if (logToFile && logEventsToFile)
            {
                // Create a "RedTruckCaptured" node
                XmlNode node = xmlDoc.CreateElement("RedTruckCaptured");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "capturer", capturer);
                AddAttribute(node, "captured", captured);

                // Add to"Events" node
                eventsNode.AppendChild(node);
            }
            if (logToConsole)
            {
                Log.debug("EVENT (" + timeStamp + "): Red truck " + captured + " captured by " + capturer);
            }
        }

        public void LogRedDismountCaptured(string capturer, string captured)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            // Increment counter
            countRedDismountCaptured++;

            // Log to file
            if (logToFile && logEventsToFile)
            {
                // Create a "RedDismountCaptured" node
                XmlNode node = xmlDoc.CreateElement("RedDismountCaptured");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "capturer", capturer);
                AddAttribute(node, "captured", captured);

                // Add to"Events" node
                eventsNode.AppendChild(node);
            }
            if (logToConsole)
            {
                Log.debug("EVENT (" + timeStamp + "): Red dismount " + captured + " captured by " + capturer);
            }
        }

        public void LogCasualtyDelivery(string deliverer, string destination)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            // Increment counter
            countCasualtyDelivered++;

            // Log to file
            if (logToFile && logEventsToFile)
            {
                // Create a "CasualtyDelivery" node
                XmlNode node = xmlDoc.CreateElement("CasualtyDelivery");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "deliverer", deliverer);
                AddAttribute(node, "destination", destination);

                // Add to"Events" node
                eventsNode.AppendChild(node);
            }
            if (logToConsole)
            {
                Log.debug("EVENT (" + timeStamp + "): Casualty delivered by " + deliverer + " to " + destination);
            }
        }

        public void LogHeavyUAVLost(string victim, string killer)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            // Increment counter
            countHeavyUAVLost++;

            // Log to file
            if (logToFile && logEventsToFile)
            {
                // Create a "HeavyUAVLost" node
                XmlNode node = xmlDoc.CreateElement("HeavyUAVLost");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "victim", victim);
                AddAttribute(node, "killer", killer);

                // Add to"Events" node
                eventsNode.AppendChild(node);
            }
            if (logToConsole)
            {
                Log.debug("EVENT (" + timeStamp + "): Heavy UAV " + victim + " killed by " + killer);
            }
        }

        public void LogSmallUAVLost(string victim, string killer)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            // Increment counter
            countSmallUAVLost++;

            // Log to file
            if (logToFile && logEventsToFile)
            {
                // Create a "SmallUAVLost" node
                XmlNode node = xmlDoc.CreateElement("SmallUAVLost");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "victim", victim);
                AddAttribute(node, "killer", killer);

                // Add to"Events" node
                eventsNode.AppendChild(node);
            }
            if (logToConsole)
            {
                Log.debug("EVENT (" + timeStamp + "): Small UAV " + victim + " killed by " + killer);
            }
        }

        public void LogCivilianInRedCustody(string capturer, string destination)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            // Increment counter
            countCivilianInRedCustody++;

            // Log to file
            if (logToFile && logEventsToFile)
            {
                // Create a "CivilianInRedCustody" node
                XmlNode node = xmlDoc.CreateElement("CivilianInRedCustody");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "capturer", capturer);
                AddAttribute(node, "destination", destination);

                // Add to"Events" node
                eventsNode.AppendChild(node);
            }
            if (logToConsole)
            {
                Log.debug("EVENT (" + timeStamp + "): Civilian transported by " + capturer + " to custody of " + destination);
            }
        }

        public void LogError(string message)
        {
            // Compute timestamp
            DateTime now = DateTime.Now;
            String timeStamp = CreateTimestamp(now);

            // Log to file
            if (logToFile)
            {
                // Create an "Error" node
                XmlNode node = xmlDoc.CreateElement("Error");

                // Populate attributes
                AddAttribute(node, "timeStamp", timeStamp);
                AddAttribute(node, "message", message);

                // Add to "Errors" node
                errorsNode.AppendChild(node);
            }

            // Also output to unity debug console
            Log.error(message);
        }
    }
}

