using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Anything that can take damage must also implement this
public interface Damageable
{
    void Hurt(int damage, Transform attacker);

}
