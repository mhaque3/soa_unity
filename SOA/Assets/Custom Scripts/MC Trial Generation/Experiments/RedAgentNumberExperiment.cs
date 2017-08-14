
namespace soa
{
    public class RedAgentNumberExperiment : DefaultExperiment
    {
        private int RedActorNumberMin  =  0;
        private int RedActorNumberMax  = 12;
        private int RedActorNumberStep =  0;

        public RedAgentNumberExperiment(MCTrialGenerator trialGenerator) : base(trialGenerator)
        {
            RedActorNumberStep = (int)(RedActorNumberMax - RedActorNumberMin) / trialGenerator.NumTrialsPerRun();
        }

        public override void StartNewTrial()
        {
            ++trialNumber;
            GenerateRedPositions(); //this experiment's primary factor is red actor number, so vary starting position every trial
        }

        public override string GetTrialName()
        {
            return "RedAgents_" + GetTrialMin().ToString() + "_" + GetTrialMax().ToString();
        }

        public override void GenerateRunPositions()
        {
            GenerateNeutralPositions();
            GenerateBluePolicePositions();
            GenerateBlueHeavyUAVPositions();
            GenerateBlueSmallUAVPositions();
            //Red actor positions are generated at the start of each trial
        }

        public override int GetNumberRed()
        {
            //Trial Number starts at 1
            int trialMin = GetTrialMin();
            int trialMax = GetTrialMax();
            double unifRand = trialGenerator.GetRand().NextDouble();
            return (int)(trialMin + (trialMax - trialMin) * unifRand);
        }

        public override int GetNumberRedDismount(int total, float fraction)
        {
            return (int)(total * fraction);
        }

        public override int GetNumberRedTruck(int total, int other)
        {
            return total - other;
        }

        private int GetTrialMin()
        {
            return (trialNumber - 1) * RedActorNumberStep + RedActorNumberMin;
        }

        private int GetTrialMax()
        {
            return GetTrialMin() + RedActorNumberStep;
        }
    }
}


