using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

//Two types 
enum ExperimentType
{
    COMMS_RANGE,    //Factor: blue actor comms range
    BLUE_NUM,       //Factor: small UAV number
    RED_NUM,        //Factor: red actors (truck + dismount) number
    RED_PRED,       //Factor: red truck movement predictability
    FACTORIAL       //Factor: comms x numBlue x numRed x pred
}

namespace soa
{
    public class MCTrialGenerator
    {
        /********************* GENERATOR *********************/
        // Random seed for MC trial generation (not runtime computations)
        int generatorRandomSeed = (int)System.DateTime.Now.Ticks;

        /******************** MONTE CARLO ********************/
        const int numMCRuns = 3;            //previously 100; 2 for ONR review
        const int numTrialsPerRun = 375;    //previously 5; 375 for ONR review
        const string soaConfigOutputPath = "../../../../GeneratedFiles";
        const string soaConfigFileHeader = "MCConfig_";

        /******************** EXPERIMENT TYPE ********************/
        //private ExperimentType experimentType = ExperimentType.COMMS_RANGE;
        //private ExperimentType experimentType = ExperimentType.BLUE_NUM;
        //private ExperimentType experimentType = ExperimentType.RED_NUM;
        //private ExperimentType experimentType = ExperimentType.RED_PRED;
        private ExperimentType experimentType = ExperimentType.FACTORIAL;

        private DefaultExperiment experiment;

        /******************** ENVIRONMENT ********************/
        const string envConfigFile = "../../../../../SOA/Assets/Custom Scripts/MC Trial Generation/SoaEnvConfig.xml";

        /********************** NETWORK **********************/
        const string networkRedRoomHeader = "soa-mc-red_";
        const string networkBlueRoomHeader = "soa-mc-blue_";

        /********************** LOGGER ***********************/
        const string soaLoggerFileHeader = "MCOutput_";
        const bool logEvents = true;
        const bool logToUnityConsole = true;

        /******************** SIMULATION *********************/
        private int simulationRandomSeed = 1;
        const float gameDurationHr = 15.0f;
        const float probRedDismountHasWeapon = 0.5f;
        const float probRedTruckHasWeapon = 0.5f;
        const float probRedTruckHasJammer = 0.5f;
        //const float min_comms_range_km = 0f;
        //const float max_comms_range_km = 25f;
        //const int defaultPredRedMovement = 0;

        /*********************** FUEL ************************/
        const float defaultHeavyUAVFuelTankSize_s = 10000;
        const float defaultSmallUAVFuelTankSize_s = 40000;

        /*********************** STORAGE ************************/
        const int defaultHeavyUAVStorageSlots = 1;
        const int defaultRedDismountStorageSlots = 1;
        const int defaultRedTruckStorageSlots = 1;

        /******************** REMOTE ID **********************/
        const int remoteStartID = 100;
        private int availableRemoteID;
        
        // No balloon laydown

        /***************** SITE LAYDOWN **********************/
        private List<SiteConfig> blueBases;
        private List<SiteConfig> redBases;
        private List<SiteConfig> ngoSites;
        private List<SiteConfig> villages;

        /***************** CLASS DEFINITION ******************/
        private GridMath gridMath;
        private HashSet<PrimitivePair<int, int>> landCells;
        private HashSet<PrimitivePair<int, int>> landAndWaterCells;
        private EnvConfig envConfig;
        private Random rand;
        public MCTrialGenerator()
        {
            // Read in envConfig
            envConfig = EnvConfigXMLReader.Parse(envConfigFile);
            if (envConfig == null)
            {
                Log.debug("MCTrialGenerator::Main(): Parsed envConfig is null, exiting");
                return;
            }

            // Initialize GridMath object
            gridMath = new GridMath(envConfig.gridOrigin_x, envConfig.gridOrigin_z, envConfig.gridToWorldScale);

            // Populate hashsets of cells for easy lookup
            landCells = new HashSet<PrimitivePair<int, int>>(envConfig.landCells);
            landAndWaterCells = new HashSet<PrimitivePair<int, int>>(envConfig.landCells);
            landAndWaterCells.UnionWith(envConfig.waterCells);
            
            // Random generator for determining whether a unit has weapons or jammers etc.
            rand = new Random(generatorRandomSeed);

            // Type of experiment that will be run
            switch (experimentType) {
                case ExperimentType.COMMS_RANGE:
                    experiment = new CommsRangeExperiment(this);
                    break;
                case ExperimentType.BLUE_NUM:
                    experiment = new SmallUAVNumberExperiment(this);
                    break;
                case ExperimentType.RED_NUM:
                    experiment = new RedAgentNumberExperiment(this);
                    break;
                case ExperimentType.RED_PRED:
                    experiment = new RedMovePredExperiment(this);
                    break;
                case ExperimentType.FACTORIAL:
                    experiment = new FactorialExperiment(this);
                    break;
            }
            
            // Blue Base
            blueBases = new List<SiteConfig>();
            blueBases.Add(new BlueBaseConfig(-3.02830208f, -9.7492255f, "Blue Base", new Optional<float>()));

            // Red Base
            redBases = new List<SiteConfig>();
            redBases.Add(new RedBaseConfig(-17.7515077f, 13.7463599f, "Red Base 0"));
            redBases.Add(new RedBaseConfig(-0.439941213f, 12.7518844f, "Red Base 1"));
            redBases.Add(new RedBaseConfig(22.0787984f, 10.7493104f, "Red Base 2"));

            // NGO Site
            ngoSites = new List<SiteConfig>();
            ngoSites.Add(new NGOSiteConfig(-21.2181483f, -1.25010618f, "NGO Site 0"));
            ngoSites.Add(new NGOSiteConfig(-4.76803318f, -5.74888572f, "NGO Site 1"));
            ngoSites.Add(new NGOSiteConfig(11.6916982f, 4.76402643f, "NGO Site 2"));
            ngoSites.Add(new NGOSiteConfig(24.6807822f, -6.75057337f, "NGO Site 3"));

            // Village
            villages = new List<SiteConfig>();
            villages.Add(new VillageConfig(-16.0197901f, 5.74808437f, "Village 0"));
            villages.Add(new VillageConfig(-14.296086f, -3.22944096f, "Village 1"));
            villages.Add(new VillageConfig(-5.63349131f, 4.74559538f, "Village 2"));
            villages.Add(new VillageConfig(7.34998325f, -0.743652906f, "Village 3"));
            villages.Add(new VillageConfig(-13.4306279f, -11.7638197f, "Village 4"));
        }
        
        public int NumTrialsPerRun()
        {
            return numTrialsPerRun;
        }

        public GridMath GetGridMath()
        {
            return gridMath;
        }

        public Random GetRand()
        {
            return rand;
        }

        public HashSet<PrimitivePair<int, int>> GetLandCells()
        {
            return new HashSet<PrimitivePair<int, int>>(landCells);
        }

        public HashSet<PrimitivePair<int, int>> GetLandAndWaterCells()
        {
            return new HashSet<PrimitivePair<int, int>>(landAndWaterCells);
        }

        public List<PrimitivePair<float, float>> GetRedBaseLocations()
        {
            return extractSiteLocations(redBases);
        }

        public List<PrimitivePair<float, float>> GetBlueBaseLocations()
        {
            return extractSiteLocations(blueBases);
        }

        public List<PrimitivePair<float, float>> GetNeutralSiteLocations()
        {
            List<SiteConfig> neutralSites = new List<SiteConfig>();
            neutralSites.AddRange(ngoSites);
            neutralSites.AddRange(villages);
            return extractSiteLocations(neutralSites);
        }

        public void GenerateConfigFiles()
        {

            for (int run = 1; run <= numMCRuns; run++)
            {
                experiment.StartNewRun();
                simulationRandomSeed = rand.Next(); //trials should share the random seed

                for (int trial = 1; trial <= numTrialsPerRun; trial++)
                {
                    experiment.StartNewTrial();
                    
                    SoaConfig config = CreateTrial(trial);
                    WriteConfig(run, trial, config);
                }
            }
        }

        #region Perception Defaults
        private void SetSensorDefaults(SoaConfig soaConfig)
        {
            // Pointer
            List<PerceptionModality> modes;
            
            // Red Dismount
            modes = new List<PerceptionModality>();
            modes.Add(new PerceptionModality("BluePolice", 1.0f, 5.0f));
            modes.Add(new PerceptionModality("HeavyUAV",   1.0f, 5.0f));
            modes.Add(new PerceptionModality("SmallUAV",   1.0f, 4.5f));
            soaConfig.defaultSensorModalities.Add("RedDismount", modes);

            // Red Truck
            modes = new List<PerceptionModality>();
            modes.Add(new PerceptionModality("BluePolice", 1.0f, 5.0f));
            modes.Add(new PerceptionModality("HeavyUAV",   1.0f, 5.0f));
            modes.Add(new PerceptionModality("SmallUAV",   1.0f, 4.5f));
            soaConfig.defaultSensorModalities.Add("RedTruck", modes);

            // Neutral Dismount
            modes = new List<PerceptionModality>();
            soaConfig.defaultSensorModalities.Add("NeutralDismount", modes);

            // Neutral Truck
            modes = new List<PerceptionModality>();
            soaConfig.defaultSensorModalities.Add("NeutralTruck", modes);

            // Blue Police
            modes = new List<PerceptionModality>();
            modes.Add(new PerceptionModality("RedDismount",     0.5f, 0.5f));
            modes.Add(new PerceptionModality("RedTruck",        0.5f, 0.5f));
            modes.Add(new PerceptionModality("NeutralDismount", 0.5f, 0.5f));
            modes.Add(new PerceptionModality("NeutralTruck",    0.5f, 0.5f));
            soaConfig.defaultSensorModalities.Add("BluePolice", modes);

            // Heavy UAV
            modes = new List<PerceptionModality>();
            modes.Add(new PerceptionModality("RedDismount",     0.5f, 0.5f));
            modes.Add(new PerceptionModality("RedTruck",        0.5f, 0.5f));
            modes.Add(new PerceptionModality("NeutralDismount", 0.5f, 0.5f));
            modes.Add(new PerceptionModality("NeutralTruck",    0.5f, 0.5f));
            soaConfig.defaultSensorModalities.Add("HeavyUAV", modes);
            soaConfig.defaultSensorBeamwidths.Add("HeavyUAV", 360);

            // Small UAV
            modes = new List<PerceptionModality>();
            modes.Add(new PerceptionModality("RedDismount",     1.0f, 3.5f));
            modes.Add(new PerceptionModality("RedTruck",        2.0f, 3.5f));
            modes.Add(new PerceptionModality("NeutralDismount", 1.0f, 3.5f));
            modes.Add(new PerceptionModality("NeutralTruck",    2.0f, 3.5f));
            soaConfig.defaultSensorModalities.Add("SmallUAV", modes);
            soaConfig.defaultSensorBeamwidths.Add("SmallUAV", 90);

            // Blue Balloon
            modes = new List<PerceptionModality>();
            modes.Add(new PerceptionModality("RedDismount",     1e20f,1e20f));
            modes.Add(new PerceptionModality("RedTruck",        1e20f,1e20f));
            modes.Add(new PerceptionModality("NeutralDismount", 1e20f,1e20f));
            modes.Add(new PerceptionModality("NeutralTruck",    1e20f,1e20f));
            soaConfig.defaultSensorModalities.Add("BlueBalloon", modes);
            soaConfig.defaultSensorBeamwidths.Add("BlueBalloon", 4);
        }

        private void SetClassifierDefaults(SoaConfig soaConfig)
        {
            // Pointer
            List<PerceptionModality> modes;

            // Red Dismount
            modes = new List<PerceptionModality>();
            modes.Add(new PerceptionModality("BluePolice",      5.0f, 5.0f));
            modes.Add(new PerceptionModality("HeavyUAV",        5.0f, 5.0f));
            modes.Add(new PerceptionModality("SmallUAV",        4.5f, 4.5f));
            soaConfig.defaultClassifierModalities.Add("RedDismount", modes);

            // Red Truck
            modes = new List<PerceptionModality>();
            modes.Add(new PerceptionModality("BluePolice",      5.0f, 5.0f));
            modes.Add(new PerceptionModality("HeavyUAV",        5.0f, 5.0f));
            modes.Add(new PerceptionModality("SmallUAV",        4.5f, 4.5f));
            soaConfig.defaultClassifierModalities.Add("RedTruck", modes);

            // Neutral Dismount
            modes = new List<PerceptionModality>();
            soaConfig.defaultClassifierModalities.Add("NeutralDismount", modes);

            // Neutral Truck
            modes = new List<PerceptionModality>();
            soaConfig.defaultClassifierModalities.Add("NeutralTruck", modes);

            // Blue Police
            modes = new List<PerceptionModality>();
            modes.Add(new PerceptionModality("RedDismount",     0.5f, 0.5f));
            modes.Add(new PerceptionModality("RedTruck",        0.5f, 0.5f));
            modes.Add(new PerceptionModality("NeutralDismount", 0.5f, 0.5f));
            modes.Add(new PerceptionModality("NeutralTruck",    0.5f, 0.5f));
            soaConfig.defaultClassifierModalities.Add("BluePolice", modes);

            // Heavy UAV
            modes = new List<PerceptionModality>();
            modes.Add(new PerceptionModality("RedDismount",     0.5f, 0.5f));
            modes.Add(new PerceptionModality("RedTruck",        0.5f, 0.5f));
            modes.Add(new PerceptionModality("NeutralDismount", 0.5f, 0.5f));
            modes.Add(new PerceptionModality("NeutralTruck",    0.5f, 0.5f));
            soaConfig.defaultClassifierModalities.Add("HeavyUAV", modes);

            // Small UAV
            modes = new List<PerceptionModality>();
            modes.Add(new PerceptionModality("RedDismount",     0.5f, 5.0f));
            modes.Add(new PerceptionModality("RedTruck",        0.5f, 7.0f));
            modes.Add(new PerceptionModality("NeutralDismount", 0.5f, 5.0f));
            modes.Add(new PerceptionModality("NeutralTruck",    0.5f, 7.0f));
            soaConfig.defaultClassifierModalities.Add("SmallUAV", modes);

            // Blue Balloon
            modes = new List<PerceptionModality>();
            soaConfig.defaultClassifierModalities.Add("BlueBalloon", modes);
        }
        #endregion

        #region Comms Defaults
        //public void SetCommsDefaults(SoaConfig soaConfig, float min, float max, int trial)
        public void SetCommsDefaults(SoaConfig soaConfig)
        {
            float randDist = experiment.GetCommsRange();
            soaConfig.defaultCommsRanges["BlueBase"] = randDist;
            soaConfig.defaultCommsRanges["RedDismount"] = 5.0f;
            soaConfig.defaultCommsRanges["RedTruck"] = 5.0f;
            soaConfig.defaultCommsRanges["BluePolice"] = randDist;
            soaConfig.defaultCommsRanges["HeavyUAV"] = randDist;
            soaConfig.defaultCommsRanges["SmallUAV"] = randDist;
        }

        public void SetJammerDefaults(SoaConfig soaConfig)
        {
            soaConfig.defaultJammerRanges["RedTruck"] = experiment.GetJammerRange();
        }
        #endregion
        
        #region Site Locations
        public void SetSiteLocations(SoaConfig soaConfig)
        {
            // Add all to soaConfig
            soaConfig.sites.AddRange(blueBases);
            soaConfig.sites.AddRange(redBases);
            soaConfig.sites.AddRange(ngoSites);
            soaConfig.sites.AddRange(villages);
        }

        #endregion

        #region MC Trial Generation

        private SoaConfig CreateTrial(int trial)
        {
            // Initialize remote platform ID assignment
            availableRemoteID = remoteStartID;

            // Create a new SoaConfig object
            SoaConfig soaConfig = new SoaConfig();

            // Populate network
            soaConfig.networkRedRoom = "localhost:11311";// networkRedRoomHeader + trial.ToString(toStringFormat);
            soaConfig.networkBlueRoom = "localhost:11411";//networkBlueRoomHeader + trial.ToString(toStringFormat);

            // Logger configuration
            soaConfig.loggerOutputFile = "ResultsLog.xml";//soaLoggerFileHeader + trial.ToString(toStringFormat) + ".xml";
            soaConfig.enableLogToFile = true;
            soaConfig.enableLogEventsToFile = logEvents;
            soaConfig.enableLogToUnityConsole = logToUnityConsole;

            // Simulation configuration
            soaConfig.simulationRandomSeed = simulationRandomSeed;
            soaConfig.gameDurationHr = gameDurationHr;
            soaConfig.probRedDismountHasWeapon = probRedDismountHasWeapon;
            soaConfig.probRedTruckHasWeapon = probRedTruckHasWeapon;
            soaConfig.probRedTruckHasJammer = probRedTruckHasJammer;
            soaConfig.controlUpdateRate_s = 0.1f;
            //soaConfig.predRedMovement = ?;

            // Populate fuel defaults
            soaConfig.defaultSmallUAVFuelTankSize_s = defaultSmallUAVFuelTankSize_s;
            soaConfig.defaultHeavyUAVFuelTankSize_s = defaultHeavyUAVFuelTankSize_s;

            // Populate storage defaults
            soaConfig.defaultHeavyUAVNumStorageSlots = defaultHeavyUAVStorageSlots;
            soaConfig.defaultRedDismountNumStorageSlots = defaultRedDismountStorageSlots;
            soaConfig.defaultRedTruckNumStorageSlots = defaultRedTruckStorageSlots;

            // Set sensor and classifier defaults
            SetSensorDefaults(soaConfig);
            SetClassifierDefaults(soaConfig);

            // Set comms and jammer defaults
            //SetCommsDefaults(soaConfig, min_comms_range_km, max_comms_range_km, trial);
            SetCommsDefaults(soaConfig);
            SetJammerDefaults(soaConfig);

            // Set predictability or red actor movement
            soaConfig.predRedMovement = experiment.GetPredRedMovement();

            // Initialize and set site locations
            SetSiteLocations(soaConfig);

            List<PrimitiveTriple<float, float, float>> randomizedPositions;

            // Local units: Red Dismount
            randomizedPositions = experiment.GetRedDismountPositions();
            for (int i = 0; i < randomizedPositions.Count; i++)
            {
                soaConfig.localPlatforms.Add(new RedDismountConfig(
                    randomizedPositions[i].first, // x
                    randomizedPositions[i].second, // y
                    randomizedPositions[i].third,  // z
                    new Optional<int>(), // id
                    new Optional<float>(), // sensorBeamwidth_deg
                    new Optional<string>(), // initialWaypoint
                    new Optional<bool>(), // hasWeapon
                    new Optional<float>(), // commsRange_km
                    new Optional<int>() // numStorageSlots
                    ));
            }

            // Local units: Red Truck
            randomizedPositions = experiment.GetRedTruckPositions();
            for (int i = 0; i < randomizedPositions.Count; i++)
            {
                soaConfig.localPlatforms.Add(new RedTruckConfig(
                    randomizedPositions[i].first, // x
                    randomizedPositions[i].second, // y
                    randomizedPositions[i].third,  // z
                    new Optional<int>(), // id
                    new Optional<float>(), // sensorBeamwidth_deg
                    new Optional<string>(), // initialWaypoint
                    new Optional<bool>(), // hasWeapon
                    new Optional<bool>(), // hasJammer
                    new Optional<float>(), // commsRange_km
                    new Optional<float>(), // jammerRange_km
                    new Optional<int>() // numStorageSlots
                    ));
            }

            // Local units: Neutral Dismount
            randomizedPositions = experiment.GetNeutralDismountPositions();
            for (int i = 0; i < randomizedPositions.Count; i++)
            {
                soaConfig.localPlatforms.Add(new NeutralDismountConfig(
                    randomizedPositions[i].first, // x
                    randomizedPositions[i].second, // y
                    randomizedPositions[i].third,  // z
                    new Optional<int>(), // id
                    new Optional<float>() // sensorBeamwidth_deg
                    ));
            }

            // Local units: Neutral Truck
            randomizedPositions = experiment.GetNeutralTruckPositions();
            for (int i = 0; i < randomizedPositions.Count; i++)
            {
                soaConfig.localPlatforms.Add(new NeutralTruckConfig(
                    randomizedPositions[i].first, // x
                    randomizedPositions[i].second, // y
                    randomizedPositions[i].third,  // z
                    new Optional<int>(), // id
                    new Optional<float>() // sensorBeamwidth_deg
                    ));
            }

            // Local units: Blue Police
            randomizedPositions = experiment.GetBluePolicePositions();
            for (int i = 0; i < randomizedPositions.Count; i++)
            {
                soaConfig.localPlatforms.Add(new BluePoliceConfig(
                    randomizedPositions[i].first, // x
                    randomizedPositions[i].second, // y
                    randomizedPositions[i].third,  // z
                    new Optional<int>(), // id
                    new Optional<float>(), // sensorBeamwidth_deg
                    new Optional<float>() // commsRange_km
                    ));
            }

            // Remote units: Heavy UAV
            randomizedPositions = experiment.GetHeavyUAVPositions();
            for (int i = 0; i < randomizedPositions.Count; i++)
            {
                soaConfig.remotePlatforms.Add(new HeavyUAVConfig(
                    randomizedPositions[i].first, // x
                    randomizedPositions[i].second, // y
                    randomizedPositions[i].third,  // z
                    availableRemoteID++, // id
                    new Optional<float>(), // sensorBeamwidth_deg
                    new Optional<float>(), // commsRange_km
                    new Optional<float>(), // fuelTankSize_s
                    new Optional<int>() // numStorageSlots
                    ));
            }

            // Remote units: Small UAV
            randomizedPositions = experiment.GetSmallUAVPositions();
            for (int i = 0; i < randomizedPositions.Count; i++)
            {
                soaConfig.remotePlatforms.Add(new SmallUAVConfig(
                    randomizedPositions[i].first, // x
                    randomizedPositions[i].second, // y
                    randomizedPositions[i].third,  // z
                    availableRemoteID++, // id
                    new Optional<float>(), // sensorBeamwidth_deg
                    new Optional<float>(), // commsRange_km
                    new Optional<float>() // fuelTankSize_s
                    ));
            }

            // Remote units: Blue Balloon
            List<PrimitivePair<float, float>> balloonWaypoints = new List<PrimitivePair<float, float>>();
            balloonWaypoints.Add(new PrimitivePair<float, float>(-28, 0));
            balloonWaypoints.Add(new PrimitivePair<float, float>(29, 0));
            soaConfig.remotePlatforms.Add(new BlueBalloonConfig(
                availableRemoteID++, // id
                new Optional<float>(), // sensorBeamwidth_deg
                balloonWaypoints,
                true
                ));

            return soaConfig;
        }

        private void WriteConfig(int run, int trial, SoaConfig config)
        {
            int numTrialDigits = numTrialsPerRun.ToString().Length;
            string trialNumberFormat = "D" + numTrialDigits.ToString();

            Directory.CreateDirectory(soaConfigOutputPath);

            string runFolder = Path.Combine(soaConfigOutputPath, "Run_" + run.ToString(trialNumberFormat));
            Directory.CreateDirectory(runFolder);

            string trialFolder = Path.Combine(runFolder, experiment.GetTrialName());
            Directory.CreateDirectory(trialFolder);

            string configName = Path.Combine(trialFolder, "SoaSimConfig.xml");
            Log.debug("Generating File: " + configName);

            Log.debug("***");
            Log.debug(generatorRandomSeed.ToString());
            Log.debug("***");

            SoaConfigXMLWriter.Write(config, configName);
        }

        #endregion

        #region Helper Functions
        /****************** MAIN FUNCTION ********************/
        private List<PrimitivePair<float,float>> extractSiteLocations(List<SiteConfig> sites)
        {
            List<PrimitivePair<float,float>> locations = new List<PrimitivePair<float, float>>();
            foreach (SiteConfig site in sites){
                locations.Add(new PrimitivePair<float, float>(site.x_km, site.z_km));
            }
            return locations;
        }
        #endregion

        #region Main Function
        /****************** MAIN FUNCTION ********************/
        public static void Main()
        {
            MCTrialGenerator mcTrialGenerator = new MCTrialGenerator();

            mcTrialGenerator.GenerateConfigFiles();
            
        }
        #endregion
    }
}
