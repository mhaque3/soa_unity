[System.Serializable]
public class PerceptionModality
{
    public string tagString;
    public float RangeP1;
    public float RangeMax;

    public PerceptionModality(string tagString, float RangeP1, float RangeMax)
    {
        this.tagString = tagString;
        this.RangeP1 = RangeP1;
        this.RangeMax = RangeMax;
    }
}
