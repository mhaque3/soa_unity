
namespace soa
{
    public class RedMovePredExperiment : DefaultExperiment
    {
        private int PredMoveMin = -1;
        private int PredMoveMax = 5;
        private int PredMoveStep = 0;

        public RedMovePredExperiment(MCTrialGenerator trialGenerator) : base(trialGenerator)
        {
            PredMoveStep = (int)(PredMoveMax - PredMoveMin) / trialGenerator.NumTrialsPerRun();
        }

        public override void StartNewTrial()
        {
            ++trialNumber;
        }

        public override string GetTrialName()
        {
            return "PredRed_" + GetTrialMin().ToString();// + "_" + GetTrialMax().ToString();
        }

        public override int GetPredRedMovement()
        {
            //Trial Number starts at 1
            int trialMin = GetTrialMin();
            int trialMax = GetTrialMax();
            double unifRand = trialGenerator.GetRand().NextDouble();
            return (int)(trialMin + (trialMax - trialMin) * unifRand);
        }

        private int GetTrialMin()
        {
            return (trialNumber - 1) * PredMoveStep + PredMoveMin;
        }

        private int GetTrialMax()
        {
            return GetTrialMin() + PredMoveStep;
        }
    }
}


