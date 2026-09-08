using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetable
{
    Transform GetTargetPoint();
    
    bool IsAlive { get; }
}
