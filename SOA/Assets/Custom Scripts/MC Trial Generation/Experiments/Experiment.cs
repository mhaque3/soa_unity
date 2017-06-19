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

        // Default comms and jammer ranges
        protected float CommsRangeDefault_Km;
        protected float JammerRangeDefault_Km;

        // Default predictability of red actor movement
        protected int PredRedMovementDefault;

        // Default neutral numbers
        protected int NumberNeutralDismountDefault;
        protected int NumberNeutralTrucksDefault;

        // Experimental neutral numbers
        protected int NumberNeutralDismount;        //Currently, not a factor
        protected int NumberNeutralTrucks;          //Currently, not a factor

        // Default blue numbers
        protected int NumberBlueHeavyUAVDefault;
        protected int NumberBlueSmallUAVDefault;
        protected int NumberBluePoliceDefault;

        // Experimental blue numbers
        protected int NumberBlueHeavyUAV;           //Currently, not a factor
        protected int NumberBlueSmallUAV;
        protected int NumberBluePolice;             //Currently, not a factor

        // Default red 
        protected int NumberRedDefault;             //truck + dismount
        protected int NumberRedDismountDefault;
        protected int NumberRedTrucksDefault;

        // Experimental red numbers
        protected float fractionRedDismount;
        protected int NumberRed;
        protected int NumberRedDismount;
        protected int NumberRedTrucks;
        
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
            trialNumber = 1;

            // Default comms and jammer ranges
            CommsRangeDefault_Km = 10f;
            JammerRangeDefault_Km = 2f;

            // Default predictability of red actor movement
            PredRedMovementDefault = 5; 

            // Default red dismounts and trucks and the total
            NumberRedDefault = 6;
            NumberRedDismountDefault = 3;
            NumberRedTrucksDefault = 3;

            // Default blue and neutral numbers
            NumberNeutralDismountDefault = 3;
            NumberNeutralTrucksDefault = 3;
            NumberBluePoliceDefault = 1; 
            NumberBlueHeavyUAVDefault = 3;
            NumberBlueSmallUAVDefault = 3;
          
            // Positions
            GenerateRunPositions();
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

        public virtual void GenerateRunPositions()
        {
            // Generate all at the start of a new run during a comms_range experiment
            GenerateNeutralPositions();
            GenerateBluePolicePositions();
            GenerateBlueHeavyUAVPositions();
            GenerateBlueSmallUAVPositions();
            GenerateRedPositions();
        }

        /**
         * Called at the start of a new run.
         */
        public void GenerateNeutralPositions()
        {
            Platform2DLaydown neutralVehicleLaydown = new Platform2DLaydown();
            neutralVehicleLaydown.fromAnchorStdDev_km = 2;
            neutralVehicleLaydown.fromAnchorMax_km = 5;
            neutralVehicleLaydown.anchors = trialGenerator.GetNeutralSiteLocations();       //neutral sites
            neutralVehicleLaydown.allowedCells = trialGenerator.GetLandCells();             //land cells

            NeutralDismountPositions = GenerateGroundPositions(NumberNeutralDismountDefault, neutralVehicleLaydown);
            NeutralTruckPositions = GenerateGroundPositions(NumberNeutralDismountDefault, neutralVehicleLaydown);
        }

        /**
         * Called at the start of a new run.
         */
        public void GenerateBluePolicePositions()
        {
            Platform2DLaydown bluePoliceLaydown = new Platform2DLaydown();
            bluePoliceLaydown.fromAnchorStdDev_km = 2;
            bluePoliceLaydown.fromAnchorMax_km = 5;
            bluePoliceLaydown.anchors = trialGenerator.GetBlueBaseLocations();              //blue bases
            bluePoliceLaydown.allowedCells = trialGenerator.GetLandCells();                 //land cells

            BluePolicePositions = GenerateGroundPositions(NumberBluePoliceDefault, bluePoliceLaydown);
        }

        /**
         * Called at the start of a new run
         */
        public void GenerateBlueHeavyUAVPositions()
        {
            Platform3DLaydown uavLaydown = new Platform3DLaydown();
            uavLaydown.fromAnchorStdDev_km = 5;
            uavLaydown.fromAnchorMax_km = 15;
            uavLaydown.altitudeMean_km = 2.5f;
            uavLaydown.altitudeStdDev_km = 2.5f;
            uavLaydown.altitudeMin_km = 0.0f;
            uavLaydown.altitudeMax_km = 5.0f;
            uavLaydown.anchors = trialGenerator.GetBlueBaseLocations();                     //blue bases
            uavLaydown.allowedCells = trialGenerator.GetLandAndWaterCells();                //land and water cells

            NumberBlueHeavyUAV = GetNumberBlueHeavyUAV();
            HeavyUAVPositions = GenerateGroundPositions(NumberBlueHeavyUAV, uavLaydown);
        }

        /**
         * Called at the start of a new run of a comms experiment and a red size experiment
         * Called at the start of a new trial of a blue size experiment
         */
        public void GenerateBlueSmallUAVPositions()
        {
            Platform3DLaydown uavLaydown = new Platform3DLaydown();
            uavLaydown.fromAnchorStdDev_km = 5;
            uavLaydown.fromAnchorMax_km = 15;
            uavLaydown.altitudeMean_km = 2.5f;
            uavLaydown.altitudeStdDev_km = 2.5f;
            uavLaydown.altitudeMin_km = 0.0f;
            uavLaydown.altitudeMax_km = 5.0f;
            uavLaydown.anchors = trialGenerator.GetBlueBaseLocations();                     //blue bases
            uavLaydown.allowedCells = trialGenerator.GetLandAndWaterCells();                //land and water cells

            // Experimental blue small UAVs
            NumberBlueSmallUAV = GetNumberBlueSmallUAV();
            SmallUAVPositions = GenerateGroundPositions(NumberBlueSmallUAV, uavLaydown);
        }

        /**
         * Called at the start of a new run of a comms experiment and a blue size experiment
         * Called at the start of a new trial of a red size experiment
         */
        public void GenerateRedPositions()
        {
            Platform2DLaydown redVehicleLaydown = new Platform2DLaydown();
            redVehicleLaydown.fromAnchorStdDev_km = 1;
            redVehicleLaydown.fromAnchorMax_km = 2;
            redVehicleLaydown.anchors = trialGenerator.GetRedBaseLocations();               //red bases                   
            redVehicleLaydown.allowedCells = trialGenerator.GetLandCells();                 //land cells

            // Experimental red dismounts and trucks and the total
            fractionRedDismount = 0.50f;
            NumberRed = GetNumberRed();
            NumberRedDismount = GetNumberRedDismount(NumberRed, fractionRedDismount);
            NumberRedTrucks = GetNumberRedTruck(NumberRed, NumberRedDismount);
            
            RedDismountPositions = GenerateGroundPositions(NumberRedDismount, redVehicleLaydown);    //redVehicleLaydown
            RedTruckPositions = GenerateGroundPositions(NumberRedTrucks, redVehicleLaydown);         //redVehicleLaydown
        }

        /**
         * Returns the default number of Heavy UAVs
         * for the current trial.
         */
        public virtual int GetPredRedMovement()
        {
            return PredRedMovementDefault;
        }

        /**
         * Returns the default number of Heavy UAVs
         * for the current trial.
         */
        public virtual int GetNumberBlueHeavyUAV()
        {
            return NumberBlueHeavyUAVDefault;
        }

        /**
         * Returns the default number of Small UAVs
         * for the current trial.
         */
        public virtual int GetNumberBlueSmallUAV()
        {
            return NumberBlueSmallUAVDefault;
        }

        /**
         * Returns the default number of Red Agents (dismount + trucks)
         * for the current trial.
         */
        public virtual int GetNumberRed()
        {
            return NumberRedDefault;
        }

        /**
         * Returns the deafult number of Red Dismounts
         * for the current trial.
         */
        public virtual int GetNumberRedDismount(int total, float fraction)
        {
            return NumberRedDismountDefault;
        }

        /**
         * Returns the default number of Red Trucks (dismount + trucks)
         * for the current trial.
         */
        public virtual int GetNumberRedTruck(int total, int other)
        {
            return NumberRedTrucksDefault;
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
            return RedDismountPositions;
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

        public List<PrimitiveTriple<float, float, float>> GenerateGroundPositions(int numPositions, Platform2DLaydown laydown)
        {
            laydown.numMean = numPositions;
            laydown.numStdDev = 0;
            laydown.numMin = 0;
            laydown.numMax = numPositions;
            
            return laydown.Generate(trialGenerator.GetGridMath(), trialGenerator.GetRand());
        }
    }
}
