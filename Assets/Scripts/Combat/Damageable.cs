using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Anything that can take damage must also implement this
public interface Damageable
{
    void Hurt(int damage, Transform attacker);

}

// is this script still necessary since the same thing happens in the "Health" script?