using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Anything that can take damage must also implement this
public interface Damageable
{
    void Hurt(int damage, Transform attacker);

}

// is this script still necessary since the same thing happens in the "Health" script?

// basically anything that has this can take damage, not matter what it is, without having to constantly change the combat code
// Damageable = "Anything that can be hit"
// Health = "An animate object with HP"
// Building = "Inanimate object with HP"