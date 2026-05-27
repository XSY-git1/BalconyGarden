using System;
using System.Collections.Generic;

[Serializable]
public class PotLayoutSaveData
{
    public List<PotInstance> pots = new List<PotInstance>();

    public PotLayoutSaveData()
    {
    }

    public PotLayoutSaveData(IEnumerable<PotInstance> sourcePots)
    {
        pots = new List<PotInstance>();

        if (sourcePots == null)
        {
            return;
        }

        foreach (PotInstance pot in sourcePots)
        {
            if (pot != null)
            {
                pots.Add(pot.Clone());
            }
        }
    }
}
