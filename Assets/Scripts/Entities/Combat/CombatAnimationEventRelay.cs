using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAnimationEventRelay : MonoBehaviour
{
    private CombatBase combat;

    private void Awake()
    {
        combat = GetComponentInParent<CombatBase>();
    }

    public void AnimationHitFrame()
    {
        if (combat != null)
        {
            combat.AnimationHitFrame();
        }
    }
}
