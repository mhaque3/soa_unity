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
        public float gameDurationMin;
        public float probRedDismountWeaponized;
        public float probRedTruckWeaponized;
        public float probRedTruckJammer;
        public float jammerRange;

        // Logger configuration
        public string loggerOutputFile;
        public bool enableLogToFile;
        public bool enableLogEventsToFile;
        public bool enableLogToUnityConsole;

        // Local platforms
        public List<PlatformConfig> localPlatforms;

        // Remote platforms
        public List<PlatformConfig> remotePlatforms;

        // Default beamwidth
        public Dictionary<string, float> defaultSensorBeamwidths;

        // Default modalities
        public Dictionary<string, List<PerceptionModality>> defaultSensorModalities;
        public Dictionary<string, List<PerceptionModality>> defaultClassifierModalities;

        public SoaConfig()
        {
            localPlatforms = new List<PlatformConfig>();
            remotePlatforms = new List<PlatformConfig>();
            defaultSensorBeamwidths = new Dictionary<string, float>();
            defaultSensorModalities = new Dictionary<string, List<PerceptionModality>>();
            defaultClassifierModalities = new Dictionary<string, List<PerceptionModality>>();
        }
    }

    // Generalized platform
    public abstract class PlatformConfig
    {
        public enum ConfigType {RED_DISMOUNT, RED_TRUCK, NEUTRAL_DISMOUNT, NEUTRAL_TRUCK, BLUE_POLICE, HEAVY_UAV, SMALL_UAV, BLUE_BALLOON};
        public float x_km;
        public float y_km;
        public float z_km;
        public int id;
        private bool useDefaultSensorBeamwidth;
        private float sensorBeamwidth_deg;
        private bool useDefaultSensorModalities;
        private List<PerceptionModality> sensorModalities;
        private bool useDefaultClassifierModalities;
        private List<PerceptionModality> classifierModalities;

        public PlatformConfig(float x_km, float y_km, float z_km, int id) {
            this.x_km = x_km;
            this.y_km = y_km;
            this.z_km = z_km;
            this.id = id;
            this.useDefaultSensorBeamwidth = true;
            this.sensorBeamwidth_deg = 360; // Default is isotropic
            this.useDefaultSensorModalities = true;
            this.sensorModalities = new List<PerceptionModality>();
            this.useDefaultClassifierModalities = true;
            this.classifierModalities = new List<PerceptionModality>();
        }

        public abstract ConfigType GetConfigType();

        public void SetSensorBeamwidth(float beamwidth_deg)
        {
            this.useDefaultSensorBeamwidth = false;
            this.sensorBeamwidth_deg = beamwidth_deg;
        }

        public bool GetUseDefaultSensorBeamwidth()
        {
            return useDefaultSensorBeamwidth;
        }

        public float GetSensorBeamwidth()
        {
            return sensorBeamwidth_deg;
        }

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
        public bool hasWeapon;
        public string initialWaypoint;
        public RedDismountConfig(float x_km, float y_km, float z_km, int id,
            string initialWaypoint, bool hasWeapon)
            : base(x_km, y_km, z_km, id)
        {
            this.initialWaypoint = initialWaypoint;
            this.hasWeapon = hasWeapon;
        }
        public override ConfigType GetConfigType() { return ConfigType.RED_DISMOUNT; }
    }

    // Red truck config
    public class RedTruckConfig : PlatformConfig
    {
        public bool hasWeapon;
        public bool hasJammer;
        public float jammerRange;
        public string initialWaypoint;
        public RedTruckConfig(float x_km, float y_km, float z_km, int id,
            string initialWaypoint, bool hasWeapon, bool hasJammer, float jammerRange)
            : base(x_km, y_km, z_km, id)
        {
            this.initialWaypoint = initialWaypoint;
            this.hasWeapon = hasWeapon;
            this.hasJammer = hasJammer;
            this.jammerRange = jammerRange;
        }
        public override ConfigType GetConfigType() { return ConfigType.RED_TRUCK; }
    }

    // Neutral dismount config
    public class NeutralDismountConfig : PlatformConfig
    {
        public NeutralDismountConfig(float x_km, float y_km, float z_km, int id)
            : base(x_km, y_km, z_km, id) { }
        public override ConfigType GetConfigType() { return ConfigType.NEUTRAL_DISMOUNT; }
    }
    
    // Neutral truck config
    public class NeutralTruckConfig : PlatformConfig
    {
        public NeutralTruckConfig(float x_km, float y_km, float z_km, int id)
            : base(x_km, y_km, z_km, id) { }
        public override ConfigType GetConfigType() { return ConfigType.NEUTRAL_TRUCK; }
    }

    // Blue police config
    public class BluePoliceConfig : PlatformConfig
    {
        public BluePoliceConfig(float x_km, float y_km, float z_km, int id)
            : base(x_km, y_km, z_km, id) { }
        public override ConfigType GetConfigType() { return ConfigType.BLUE_POLICE; }
    }

    // Heavy UAV config
    public class HeavyUAVConfig : PlatformConfig
    {
        public HeavyUAVConfig(float x_km, float y_km, float z_km, int id)
            : base(x_km, y_km, z_km, id) {
        }
        public override ConfigType GetConfigType() { return ConfigType.HEAVY_UAV; }
    }

    // Small UAV config
    public class SmallUAVConfig : PlatformConfig
    {
        public SmallUAVConfig(float x_km, float y_km, float z_km, int id)
            : base(x_km, y_km, z_km, id) { }
        public override ConfigType GetConfigType() { return ConfigType.SMALL_UAV; }
    }

    // Blue balloon config
    public class BlueBalloonConfig : PlatformConfig
    {
        public BlueBalloonConfig(float x_km, float y_km, float z_km, int id)
            : base(x_km, y_km, z_km, id) { }
        public override ConfigType GetConfigType() { return ConfigType.BLUE_BALLOON; }
    }
}
