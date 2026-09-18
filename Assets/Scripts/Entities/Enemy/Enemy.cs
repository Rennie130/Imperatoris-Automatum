using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Enemy : MonoBehaviour 
{
    public EnemyZone AssociatedEnemyZone {get; set;}  
    public abstract Action<Enemy> OnDeath {get; set;}
}
