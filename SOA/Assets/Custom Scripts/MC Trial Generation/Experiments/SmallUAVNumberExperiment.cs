
namespace soa
{
    public class SmallUAVNumberExperiment : DefaultExperiment
    {
        private int BlueSmallNumberMin = 0;
        private int BlueSmallNumberMax = 20;
        private int BlueSmallNumberStep = 0;

        public SmallUAVNumberExperiment(MCTrialGenerator trialGenerator) : base(trialGenerator)
        {
            BlueSmallNumberStep = (int)(BlueSmallNumberMax - BlueSmallNumberMin) / trialGenerator.NumTrialsPerRun();
        }

        public override void StartNewTrial()
        {
            ++trialNumber;
            GenerateBlueSmallUAVPositions(); //this experiment's primary factor is small UAVs number, so vary starting position every trial
        }

        public override string GetTrialName()
        {
            return "BlueSmall_" + GetTrialMin().ToString() + "_" + GetTrialMax().ToString();
        }

        public override void GenerateRunPositions()
        {
            GenerateNeutralPositions();
            GenerateBluePolicePositions();
            GenerateBlueHeavyUAVPositions();
            //Blue Small UAV positions are generated at the start of each trial
            GenerateRedPositions();
        }

        public override int GetNumberBlueSmallUAV()
        {
            //Trial Number starts at 1
            int trialMin = GetTrialMin();
            int trialMax = GetTrialMax();
            double unifRand = trialGenerator.GetRand().NextDouble();
            return (int)(trialMin + (trialMax - trialMin) * unifRand);
        }

        private int GetTrialMin()
        {
            return (trialNumber - 1) * BlueSmallNumberStep + BlueSmallNumberMin;
        }

        private int GetTrialMax()
        {
            return GetTrialMin() + BlueSmallNumberStep;
        }
    }
}


