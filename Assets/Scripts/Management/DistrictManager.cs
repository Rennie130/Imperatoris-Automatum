using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistrictManager : MonoBehaviour
{
    public static DistrictManager Instance;

    List<Building> buildings = new List<Building>();

    public int maxDistrictHealth {get; private set; }
    public int currentDistrictHealth {get; private set; }

    bool isDestroyed = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterBuilding(Building b)
    {
        buildings.Add(b);
        RecalculateDistrictHealth();
    }

    public void UnregisterBuilding(Building b)
    {
        buildings.Remove(b);
        RecalculateDistrictHealth();
    }

    public void RecalculateDistrictHealth()
    {
        buildings.RemoveAll(b => b == null);

        maxDistrictHealth = 0;
        currentDistrictHealth = 0;

        foreach (var b in buildings)
        {
            maxDistrictHealth += b.maxHealth;
            currentDistrictHealth += b.CurrentHealth;
        }

        if (maxDistrictHealth > 0)
        {
            Debug.Log($"[DISTRICT] {(float)currentDistrictHealth / maxDistrictHealth}");
        }
        else
        {
            Debug.Log("[DISTRICT] No buildings remaining");
        }
        

        if (!isDestroyed && currentDistrictHealth <= 0)
        {
            isDestroyed = true;

            Debug.Log("DISTRICT DESTROYED");

            //put the fail state here
            //GameManager.Instance.GameOver();
        }
    }

    public Building GetClosestBuilding(Vector3 position)
    {
        float closestDist = Mathf.Infinity;
        Building closest = null;

        foreach (Building b in buildings)
        {
            if (b == null || !b.IsAlive) continue;
 
            float dist = Vector3.Distance(position, b.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = b;
            }
        }
        
        return closest;
    }

}
