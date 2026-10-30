using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneWalls : MonoBehaviour
{
    public Animator WallCollapse;
    public Transform VisualRoot;
    public Collider WallCollider;
    public float WallCollapseDurationSeconds = 3; //How long it takes for wall to collapse
    private float CurrentCollapsedDurationSeconds;
    private bool IsCollapsing;

    public Vector3 EndPositionOffset;

    private Vector3 StartPosition;

    private void Awake() 
    {
        StartPosition = VisualRoot.position;
    }

    void Update()
    {
        if(IsCollapsing)
        {
            CurrentCollapsedDurationSeconds += Time.deltaTime;

            //Move object down from current (start) position to a new specified end position offset
            var collapsedProgress = CurrentCollapsedDurationSeconds / WallCollapseDurationSeconds;
            VisualRoot.position = Vector3.Lerp(StartPosition, StartPosition + EndPositionOffset, collapsedProgress);

            //Stop wall from collapsing after it is finished.
            if(CurrentCollapsedDurationSeconds >= WallCollapseDurationSeconds)
            {
                IsCollapsing = false;
                WallCollapse.Play("Idle");
                WallCollider.enabled = false;
                //stop dust particle effects
            }
        }
    }

    public void CollapsedWall()
    {
        //Play Wall Collapse Animation
        WallCollapse.Play("WallCollapse");

        //Play dust particles effect
        
        //Allow wall to move down
        IsCollapsing = true;
    }


}
