using System.Collections.Generic;

namespace soa
{
    /*
     * Experiments are responsible for keeping everything 
     * within a run the same except for the variable
     * under test. The variable under test is changed
     * randomly from trial to trial to explore the full
     * domain of that variable.
     */
    public class DefaultExperiment
    {
        protected MCTrialGenerator trialGenerator;
        
        protected int trialNumber = 0;

        protected float CommsRangeDefault_Km = 10f;
        protected float JammerRangeDefault_Km = 2f;

        protected int NumberRedDismountDefault = 3;
        protected int NumberRedTrucksDefault = 3;
        protected int NumberNeutralDismountDefault = 3;
        protected int NumberNeutralTrucksDefault = 3;
        protected int NumberBluePoliceDefault = 1;
        protected int NumberHeavyUAVDefault = 3;
        protected int NumberSmallUAVDefault = 3;

        protected List<PrimitiveTriple<float, float, float>> RedDismountPositions;
        protected List<PrimitiveTriple<float, float, float>> RedTruckPositions;
        protected List<PrimitiveTriple<float, float, float>> NeutralDismountPositions;
        protected List<PrimitiveTriple<float, float, float>> NeutralTruckPositions;
        protected List<PrimitiveTriple<float, float, float>> BluePolicePositions;
        protected List<PrimitiveTriple<float, float, float>> HeavyUAVPositions;
        protected List<PrimitiveTriple<float, float, float>> SmallUAVPositions;

        public DefaultExperiment(MCTrialGenerator trialGenerator)
        {
            this.trialGenerator = trialGenerator;
        }

        /**
         * This method is called at the start of each
         * new Run. It is responsible for initializing
         * variables that remain the same from trial
         * to trial
         */
        public void StartNewRun()
        {
            List<PrimitivePair<float, float>> redBases = trialGenerator.GetRedBaseLocations();
            List<PrimitivePair<float, float>> blueBases = trialGenerator.GetBlueBaseLocations();
            List<PrimitivePair<float, float>> neutralSites = trialGenerator.GetNeutralSiteLocations();
            HashSet<PrimitivePair<int, int>> landCells = trialGenerator.GetLandCells();
            HashSet<PrimitivePair<int, int>> landAndWaterCells = trialGenerator.GetLandAndWaterCells();

            Platform2DLaydown redVehicleLaydown = new Platform2DLaydown();
            redVehicleLaydown.fromAnchorStdDev_km = 1;
            redVehicleLaydown.fromAnchorMax_km = 2;
            redVehicleLaydown.anchors = redBases;
            redVehicleLaydown.allowedCells = landCells;

            Platform2DLaydown neutralVehicleLaydown = new Platform2DLaydown();
            neutralVehicleLaydown.fromAnchorStdDev_km = 2;
            neutralVehicleLaydown.fromAnchorMax_km = 5;
            neutralVehicleLaydown.anchors = neutralSites;
            neutralVehicleLaydown.allowedCells = landCells;

            Platform2DLaydown bluePoliceLaydown = new Platform2DLaydown();
            bluePoliceLaydown.fromAnchorStdDev_km = 2;
            bluePoliceLaydown.fromAnchorMax_km = 5;
            bluePoliceLaydown.anchors = blueBases;
            bluePoliceLaydown.allowedCells = landCells;

            Platform3DLaydown uavLaydown = new Platform3DLaydown();
            uavLaydown.fromAnchorStdDev_km = 5;
            uavLaydown.fromAnchorMax_km = 15;
            uavLaydown.altitudeMean_km = 2.5f;
            uavLaydown.altitudeStdDev_km = 2.5f;
            uavLaydown.altitudeMin_km = 0.0f;
            uavLaydown.altitudeMax_km = 5.0f;
            uavLaydown.anchors = blueBases;
            uavLaydown.allowedCells = landAndWaterCells;

            RedDismountPositions = GenerateGroundPositions(NumberRedDismountDefault, redVehicleLaydown);
            RedTruckPositions = GenerateGroundPositions(NumberRedTrucksDefault, redVehicleLaydown);
            NeutralDismountPositions = GenerateGroundPositions(NumberNeutralDismountDefault, neutralVehicleLaydown);
            NeutralTruckPositions = GenerateGroundPositions(NumberNeutralDismountDefault, neutralVehicleLaydown);
            BluePolicePositions = GenerateGroundPositions(NumberBluePoliceDefault, bluePoliceLaydown);
            HeavyUAVPositions = GenerateGroundPositions(NumberHeavyUAVDefault, uavLaydown);
            SmallUAVPositions = GenerateGroundPositions(NumberSmallUAVDefault, uavLaydown);

            trialNumber = 1;
        }

        /**
         * Called at the start of a new trial. This
         * method should be overriden in subclasses
         * to do an initialization necessary between
         * trials.
         */
        public virtual void StartNewTrial()
        {
            ++trialNumber;
        }

        /**
         * Generates a name for the folder that
         * the trial config folder lives in. The
         * name should be relevant to the variable
         * under test.
         */ 
        public virtual string GetTrialName()
        {
            int numTrialDigits = trialGenerator.NumTrialsPerRun().ToString().Length;
            string trialNumberFormat = "D" + numTrialDigits.ToString();
            return "Trial_" + trialNumber.ToString(trialNumberFormat);
        }

        /**
         * Returns the communication range in Km for all vehicles
         * for the current trial.
         */ 
        public virtual float GetCommsRange()
        {
            return CommsRangeDefault_Km;
        }

        /**
         * Returns the jamming range in Km for all vehicles
         * for the current trial.
         */
        public virtual float GetJammerRange()
        {
            return JammerRangeDefault_Km;
        }

        /**
         * Returns the red dismounts' positions
         * for the current trial.
         */
        public virtual List<PrimitiveTriple<float, float, float>> GetRedDismountPositions()
        {
            return RedTruckPositions;
        }

        /**
         * Returns the red trucks' positions
         * for the current trial.
         */
        public virtual List<PrimitiveTriple<float, float, float>> GetRedTruckPositions()
        {
            return RedTruckPositions;
        }

        /**
         * Returns the neutral dismounts' positions
         * for the current trial.
         */
        public virtual List<PrimitiveTriple<float, float, float>> GetNeutralDismountPositions()
        {
            return NeutralDismountPositions;
        }

        /**
         * Returns the neutral trucks' positions
         * for the current trial.
         */
        public virtual List<PrimitiveTriple<float, float, float>> GetNeutralTruckPositions()
        {
            return NeutralTruckPositions;
        }

        /**
         * Returns the police's position
         * for the current trial.
         */
        public virtual List<PrimitiveTriple<float, float, float>> GetBluePolicePositions()
        {
            return BluePolicePositions;
        }

        /**
         * Returns the heavy UAVs' positions
         * for the current trial.
         */
        public virtual List<PrimitiveTriple<float, float, float>> GetHeavyUAVPositions()
        {
            return HeavyUAVPositions;
        }

        /**
         * Returns the small UAVs' positions
         * for the current trial.
         */
        public virtual List<PrimitiveTriple<float, float, float>> GetSmallUAVPositions()
        {
            return SmallUAVPositions;
        }

        private List<PrimitiveTriple<float, float, float>> GenerateGroundPositions(int numPositions, Platform2DLaydown laydown)
        {
            laydown.numMean = numPositions;
            laydown.numStdDev = 0;
            laydown.numMin = 0;
            laydown.numMax = numPositions;
            
            return laydown.Generate(trialGenerator.GetGridMath(), trialGenerator.GetRand());
        }
    }
}
