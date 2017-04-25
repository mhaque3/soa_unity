
namespace soa
{
    public class CommsRangeExperiment : DefaultExperiment
    {
        private float CommsRangeMin_Km = 0f;
        private float CommsRangeMax_Km = 25f;
        private float CommsRangeStep_Km = 0f;

        public CommsRangeExperiment(MCTrialGenerator trialGenerator) : base(trialGenerator)
        {
            CommsRangeStep_Km = (CommsRangeMax_Km - CommsRangeMin_Km) / (float)trialGenerator.NumTrialsPerRun();
        }

        public override string GetTrialName()
        {
            return "Comms_" + GetTrialMin().ToString("F1") + "_" + GetTrialMax().ToString("F1"); //F1 -- 1 d.p.
        }

        public override float GetCommsRange()
        {
            //Trial Number starts at 1
            float trialMin = GetTrialMin();
            float trialMax = GetTrialMax();
            double unifRand = trialGenerator.GetRand().NextDouble();
            return (float)(trialMin + (trialMax - trialMin) * unifRand);
        }

        private float GetTrialMin()
        {
            return (trialNumber - 1) * CommsRangeStep_Km + CommsRangeMin_Km;
        }

        private float GetTrialMax()
        {
            return GetTrialMin() + CommsRangeStep_Km;
        }
    }
}
