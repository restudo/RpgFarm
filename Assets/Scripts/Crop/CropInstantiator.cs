﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to a crop prefab to set the values in the grid property dictionary
/// </summary>
public class CropInstantiator : MonoBehaviour
{
    private Grid grid;
    [ItemCodeDescription]
    [SerializeField] private int seedItemCode = 0;
    [SerializeField] private int growthDays = 0;
    [SerializeField] private int daysSinceDug = -1;
    [SerializeField] private int daysSinceWatered = -1;


    private void OnDisable()
    {
        EventHandler.InstantiateCropPrefabsEvent -= InstantiateCropPrefabs;
    }

    private void OnEnable()
    {
        EventHandler.InstantiateCropPrefabsEvent += InstantiateCropPrefabs;
    }

    private void InstantiateCropPrefabs()
    {
        // Get grid gameobject
        grid = GameObject.FindObjectOfType<Grid>();

        // Get grid position for crop
        Vector3Int cropPosition = new Vector3Int(Random.Range(-37, 37), Random.Range(-26, 0), 0);

        // Set Crop Grid Properties
        SetCropGridProperties(cropPosition);

        // Destroy this gameobject
        Destroy(gameObject);
    }


    private void SetCropGridProperties(Vector3Int cropPosition)
    {
        if (seedItemCode > 0)
        {
            GridPropertyDetails gridPropertyDetails;

            gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cropPosition.x, cropPosition.y);

            if (gridPropertyDetails == null)
            {
                gridPropertyDetails = new GridPropertyDetails();
            }

            while (gridPropertyDetails.daysSinceDug > -1)
            {
                cropPosition = new Vector3Int(Random.Range(-37, 37), Random.Range(-26, 0), 0);
                gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cropPosition.x, cropPosition.y);
            }

            while (gridPropertyDetails.canPlaceFurniture)
            {
                cropPosition = new Vector3Int(Random.Range(-37, 37), Random.Range(-26, 0), 0);
                gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cropPosition.x, cropPosition.y);
            }

            gridPropertyDetails.daysSinceDug = daysSinceDug;
            gridPropertyDetails.daysSinceWatered = daysSinceWatered;
            gridPropertyDetails.seedItemCode = seedItemCode;
            gridPropertyDetails.growthDays = growthDays;

            GridPropertiesManager.Instance.SetGridPropertyDetails(cropPosition.x, cropPosition.y, gridPropertyDetails);

        }
    }

}
