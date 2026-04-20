using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistrictManager : MonoBehaviour
{
    public static DistrictManager Instance;

    public int maxDistrictHealth = 100;
    public int currentDistrictHealth;

    List<Building> buildings = new List<Building>();

    void Awake()
    {
        Instance = this;
        currentDistrictHealth = maxDistrictHealth;
    }

    public void RegisterBuilding(Building b)
    {
        buildings.Add(b);
    }

    public void UnregisterBuilding(Building b)
    {
        buildings.Remove(b);
    }

    public void ReportDamage(int damage)
    {
        currentDistrictHealth -= damage;

        Debug.Log($"[DISTRICT DAMAGE] -{damage} -> {currentDistrictHealth}/{maxDistrictHealth}");

        if (currentDistrictHealth <= 0)
        {
            Debug.Log("DISTRICT DESTROYED");
            //todo: trigger fail state / game over
        }
       
    }

    public Building GetClosestBuilding(Vector3 position)
    {
        float closestDist = Mathf.Infinity;
        Building closest = null;

        foreach (Building b in buildings)
        {
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
