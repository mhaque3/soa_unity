using System.Collections.Generic;


namespace soa
{
    public class FactorialExperiment : DefaultExperiment
    {
        private List<int> fcList = new List<int>();
        private List<int> fbList = new List<int>();
        private List<int> frList = new List<int>();
        private List<int> fpList = new List<int>();

        public FactorialExperiment(MCTrialGenerator trialGenerator) : base(trialGenerator)
        {
            //trialNumber starts at 1, so creating a dummy row
            fcList.Add(0);
            fbList.Add(0);
            frList.Add(0);
            fpList.Add(0);

            //5, 10, 15, 20, 25
            for (int fc = 5; fc < 30; fc += 5)
            {
                //4, 8, 12, 16, 20
                for (int fb = 4; fb < 24; fb += 4)
                {
                    //2, 4, 6, 8, 10
                    for (int fr = 2; fr < 12; fr += 2)
                    {
                        //0, 1, 2
                        for (int fp = 0; fp < 3; fp++)
                        {
                            fcList.Add(fc);
                            fbList.Add(fb);
                            frList.Add(fr);
                            fpList.Add(fp);
                        }
                    }
                }
            }
        }

        public override void StartNewTrial()
        {
            ++trialNumber;
            GenerateRedPositions();
            GenerateBlueSmallUAVPositions();
           
        }

        public override string GetTrialName()
        {
            return "X_"        + 
                    "Comms_"    +   fcList[trialNumber - 1].ToString()    +
                    "numBlue_"  +   fbList[trialNumber - 1].ToString()    +
                    "numRed_"   +   frList[trialNumber - 1].ToString()    +
                    "predRed_"  +   fpList[trialNumber - 1].ToString();
        }

        public override void GenerateRunPositions()
        {
            GenerateNeutralPositions();
            GenerateBluePolicePositions();
            GenerateBlueHeavyUAVPositions();
        }
        
        public override float GetCommsRange()
        {
            return fcList[trialNumber - 1];
        }

        public override int GetNumberBlueSmallUAV()
        {
            return fbList[trialNumber - 1];
        }

        public override int GetNumberRed()
        {
            return frList[trialNumber - 1];
        }

        public override int GetNumberRedDismount(int total, float fraction)
        {
            return (int)(total * fraction);
        }

        public override int GetNumberRedTruck(int total, int other)
        {
            return total - other;
        }

        public override int GetPredRedMovement()
        {
            return fpList[trialNumber - 1];
        }
    }
}


