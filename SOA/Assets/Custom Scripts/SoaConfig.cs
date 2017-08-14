using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class SoaConfig
    {
        // Network configuration
        public string networkRedRoom;
        public string networkBlueRoom;

        // Simulation configuration
        public int simulationRandomSeed;
        public float gameDurationHr;
        public float probRedDismountHasWeapon;
        public float probRedTruckHasWeapon;
        public float probRedTruckHasJammer;
        public float controlUpdateRate_s;
        public int predRedMovement;

        // Logger configuration
        public string loggerOutputFile;
        public bool enableLogToFile;
        public bool enableLogEventsToFile;
        public bool enableLogToUnityConsole;

        // Local platforms
        public List<PlatformConfig> localPlatforms;

        // Remote platforms
        public List<PlatformConfig> remotePlatforms;

        // Sites
        public List<SiteConfig> sites;

        // Default beamwidth
        public Dictionary<string, float> defaultSensorBeamwidths;

        // Default modalities
        public Dictionary<string, List<PerceptionModality>> defaultSensorModalities;
        public Dictionary<string, List<PerceptionModality>> defaultClassifierModalities;

        // Default comms/jamming
        public Dictionary<string, float> defaultCommsRanges;
        public Dictionary<string, float> defaultJammerRanges;

        // Fuel default settings
        public float defaultHeavyUAVFuelTankSize_s;
        public float defaultSmallUAVFuelTankSize_s;

        // Storage default settings
        public int defaultHeavyUAVNumStorageSlots;
        public int defaultRedDismountNumStorageSlots;
        public int defaultRedTruckNumStorageSlots;

        public SoaConfig()
        {
            localPlatforms = new List<PlatformConfig>();
            remotePlatforms = new List<PlatformConfig>();
            sites = new List<SiteConfig>();
            defaultSensorBeamwidths = new Dictionary<string, float>();
            defaultSensorModalities = new Dictionary<string, List<PerceptionModality>>();
            defaultClassifierModalities = new Dictionary<string, List<PerceptionModality>>();
            defaultCommsRanges = new Dictionary<string, float>();
            defaultJammerRanges = new Dictionary<string, float>();
        }
    }

    #region Site Config
    // Generalized sites
    public abstract class SiteConfig
    {
        public enum ConfigType { BLUE_BASE, RED_BASE, NGO_SITE, VILLAGE };
        public float x_km;
        public float z_km;
        public string name;

        public SiteConfig(float x_km, float z_km, string name)
        {
            this.x_km = x_km;
            this.z_km = z_km;
            this.name = name;
        }

        public abstract ConfigType GetConfigType();
    }

    // Blue base config
    public class BlueBaseConfig : SiteConfig
    {
        public Optional<float> commsRange_km;
        public BlueBaseConfig(float x_km, float z_km, string name, Optional<float> commsRange_km)
            : base(x_km, z_km, name)
        {
            this.commsRange_km = commsRange_km;
        }
        public override ConfigType GetConfigType() { return ConfigType.BLUE_BASE; }
    }

    // Red base config
    public class RedBaseConfig : SiteConfig
    {
        public RedBaseConfig(float x_km, float z_km, string name)
            : base(x_km, z_km, name) {}
        public override ConfigType GetConfigType() { return ConfigType.RED_BASE; }
    }

    // NGO Site config
    public class NGOSiteConfig : SiteConfig
    {
        public NGOSiteConfig(float x_km, float z_km, string name)
            : base(x_km, z_km, name) {}
        public override ConfigType GetConfigType() { return ConfigType.NGO_SITE; }
    }

    // Village config
    public class VillageConfig : SiteConfig
    {
        public VillageConfig(float x_km, float z_km, string name)
            : base(x_km, z_km, name) {}
        public override ConfigType GetConfigType() { return ConfigType.VILLAGE; }
    }
    #endregion

    #region Platform Config
    // Generalized platform
    public abstract class PlatformConfig
    {
        public enum ConfigType {RED_DISMOUNT, RED_TRUCK, NEUTRAL_DISMOUNT, NEUTRAL_TRUCK, BLUE_POLICE, HEAVY_UAV, SMALL_UAV, BLUE_BALLOON};
        public float x_km;
        public float y_km;
        public float z_km;
        public Optional<int> id;
        public Optional<float> sensorBeamwidth_deg;
        private bool useDefaultSensorModalities;
        private List<PerceptionModality> sensorModalities;
        private bool useDefaultClassifierModalities;
        private List<PerceptionModality> classifierModalities;

        public PlatformConfig(float x_km, float y_km, float z_km, Optional<int> id, Optional<float> sensorBeamwidth_deg) {
            this.x_km = x_km;
            this.y_km = y_km;
            this.z_km = z_km;
            this.id = id;
            this.sensorBeamwidth_deg = sensorBeamwidth_deg;
            this.useDefaultSensorModalities = true;
            this.sensorModalities = new List<PerceptionModality>();
            this.useDefaultClassifierModalities = true;
            this.classifierModalities = new List<PerceptionModality>();
        }

        public abstract ConfigType GetConfigType();

        public void SetSensorModalities(List<PerceptionModality> sensorModalities)
        {
            this.useDefaultSensorModalities = false;
            this.sensorModalities = sensorModalities;
        }

        public List<PerceptionModality> GetSensorModalities()
        {
            return sensorModalities;
        }

        public bool GetUseDefaultSensorModalities()
        {
            return useDefaultSensorModalities;
        }

        public void SetClassifierModalities(List<PerceptionModality> classifierModalities)
        {
            this.useDefaultClassifierModalities = false;
            this.classifierModalities = classifierModalities;
        }

        public List<PerceptionModality> GetClassifierModalities()
        {
            return classifierModalities;
        }

        public bool GetUseDefaultClassifierModalities()
        {
            return useDefaultClassifierModalities;
        }     
    }

    // Red dismount config
    public class RedDismountConfig : PlatformConfig
    {
        public Optional<string> initialWaypoint;
        public Optional<bool> hasWeapon;
        public Optional<float> commsRange_km;
        public Optional<int> numStorageSlots;
        public RedDismountConfig(float x_km, float y_km, float z_km, Optional<int> id, Optional<float> sensorBeamwidth_deg,
            Optional<string> initialWaypoint, Optional<bool> hasWeapon, Optional<float> commsRange_km, Optional<int> numStorageSlots)
            : base(x_km, y_km, z_km, id, sensorBeamwidth_deg)
        {
            this.initialWaypoint = initialWaypoint;
            this.hasWeapon = hasWeapon;
            this.commsRange_km = commsRange_km;
            this.numStorageSlots = numStorageSlots;
        }
        public override ConfigType GetConfigType() { return ConfigType.RED_DISMOUNT; }
    }

    // Red truck config
    public class RedTruckConfig : PlatformConfig
    {
        public Optional<string> initialWaypoint;
        public Optional<bool> hasWeapon;
        public Optional<bool> hasJammer;
        public Optional<float> commsRange_km;
        public Optional<float> jammerRange_km;
        public Optional<int> numStorageSlots;
        public RedTruckConfig(float x_km, float y_km, float z_km, Optional<int> id, 
            Optional<float> sensorBeamwidth_deg, Optional<string> initialWaypoint, 
            Optional<bool> hasWeapon, Optional<bool> hasJammer,  
            Optional<float> commsRange_km, Optional<float> jammerRange_km,
            Optional<int> numStorageSlots)
            : base(x_km, y_km, z_km, id, sensorBeamwidth_deg)
        {
            this.initialWaypoint = initialWaypoint;
            this.hasWeapon = hasWeapon;
            this.hasJammer = hasJammer;
            this.commsRange_km = commsRange_km;
            this.jammerRange_km = jammerRange_km;
            this.numStorageSlots = numStorageSlots;
        }
        public override ConfigType GetConfigType() { return ConfigType.RED_TRUCK; }
    }

    // Neutral dismount config
    public class NeutralDismountConfig : PlatformConfig
    {
        public NeutralDismountConfig(float x_km, float y_km, float z_km, Optional<int> id, Optional<float> sensorBeamwidth_deg)
            : base(x_km, y_km, z_km, id, sensorBeamwidth_deg) { }
        public override ConfigType GetConfigType() { return ConfigType.NEUTRAL_DISMOUNT; }
    }
    
    // Neutral truck config
    public class NeutralTruckConfig : PlatformConfig
    {
        public NeutralTruckConfig(float x_km, float y_km, float z_km, Optional<int> id, Optional<float> sensorBeamwidth_deg)
            : base(x_km, y_km, z_km, id, sensorBeamwidth_deg) { }
        public override ConfigType GetConfigType() { return ConfigType.NEUTRAL_TRUCK; }
    }

    // Blue police config
    public class BluePoliceConfig : PlatformConfig
    {
        public Optional<float> commsRange_km;
        public BluePoliceConfig(float x_km, float y_km, float z_km, Optional<int> id, Optional<float> sensorBeamwidth_deg, 
            Optional<float> commsRange_km)
            : base(x_km, y_km, z_km, id, sensorBeamwidth_deg) {
                this.commsRange_km = commsRange_km;
        }
        public override ConfigType GetConfigType() { return ConfigType.BLUE_POLICE; }
    }

    // Heavy UAV config
    public class HeavyUAVConfig : PlatformConfig
    {
        public Optional<float> commsRange_km;
        public Optional<float> fuelTankSize_s;
        public Optional<int> numStorageSlots;
        public HeavyUAVConfig(float x_km, float y_km, float z_km, int id, Optional<float> sensorBeamwidth_deg,
            Optional<float> commsRange_km, Optional<float> fuelTankSize_s, Optional<int> numStorageSlots)
            : base(x_km, y_km, z_km, new Optional<int>(id), sensorBeamwidth_deg) {
                this.commsRange_km = commsRange_km;
                this.fuelTankSize_s = fuelTankSize_s;
                this.numStorageSlots = numStorageSlots;
        }
        public override ConfigType GetConfigType() { return ConfigType.HEAVY_UAV; }
    }

    // Small UAV config
    public class SmallUAVConfig : PlatformConfig
    {
        public Optional<float> commsRange_km;
        public Optional<float> fuelTankSize_s;
        public SmallUAVConfig(float x_km, float y_km, float z_km, int id, Optional<float> sensorBeamwidth_deg,
            Optional<float> commsRange_km, Optional<float> fuelTankSize_s)
            : base(x_km, y_km, z_km, new Optional<int>(id), sensorBeamwidth_deg) {
                this.commsRange_km = commsRange_km;
                this.fuelTankSize_s = fuelTankSize_s;
        }
        public override ConfigType GetConfigType() { return ConfigType.SMALL_UAV; }
    }

    // Blue balloon config
    public class BlueBalloonConfig : PlatformConfig
    {
        public List<PrimitivePair<float, float>> waypoints_km;
        public bool teleportLoop;
        public BlueBalloonConfig(int id, Optional<float> sensorBeamwidth_deg, 
            List<PrimitivePair<float, float>> waypoints_km, bool teleportLoop)
            : base(0, 0, 0, new Optional<int>(id), sensorBeamwidth_deg) 
        {
            // Copy list of coordinates
            this.waypoints_km = new List<PrimitivePair<float, float>>();
            foreach(PrimitivePair<float,float> waypoint_km in waypoints_km){
                this.waypoints_km.Add(new PrimitivePair<float,float>(waypoint_km.first, waypoint_km.second));
            }

            // Set coordinates as first waypoint if applicable
            if (this.waypoints_km.Count > 0)
            {
                this.x_km = waypoints_km[0].first;
                this.z_km = waypoints_km[0].second;
            }

            // Copy other data
            this.teleportLoop = teleportLoop;
        }
        public override ConfigType GetConfigType() { return ConfigType.BLUE_BALLOON; }
    }
#endregion
}
