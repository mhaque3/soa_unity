using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using soa;

public class SoaSite : SoaActor
{
    public override void Kill(string killerName)    
    {
        // There is no way to kill a site
    }

    public override void LateUpadte()
    {
        // No positions and orientation to set
    }

    public override void updateActor()
    {
        // No position information to update
    }
}
