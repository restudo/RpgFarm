using System.Collections;
using UnityEngine;

public class Crop : MonoBehaviour
{
    private int harvestActionCount = 0;

    [Tooltip("This should be populated from child gameobject")]
    [SerializeField] private SpriteRenderer cropHarvestedSpriteRenderer = null;

    [HideInInspector] public Vector2Int cropGridPosition;


    public void ProcessToolAction(ItemDetails equippedItemDetails, bool isToolRight)
    {
        // Get grid property details
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y);
        if (gridPropertyDetails == null)
            return;

        // Get seed item details
        ItemDetails seedItemDetails = InventoryManager.Instance.GetItemDetails(gridPropertyDetails.seedItemCode);
        if (seedItemDetails == null)
            return;

        // Get crop details
        CropDetails cropDetails = GridPropertiesManager.Instance.GetCropDetails(seedItemDetails.itemCode);
        if (cropDetails == null)
            return;

        // Get animator for crop if present
        Animator animator = GetComponentInChildren<Animator>();

        // Trigger tool animation
        if (animator != null)
        {
            if (isToolRight)
            {
                animator.SetTrigger("usetoolright");
            }
            else
            {
                animator.SetTrigger("usetoolleft");
            }
        }

        // Get required harvest actions for tool
        int requiredHarvestActions = cropDetails.RequiredHarvestActionsForTool(equippedItemDetails.itemCode);
        if (requiredHarvestActions == -1)
            return; // this tool can't be used to harvest this crop


        // Increment harvest action count
        harvestActionCount += 1;

        // Check if required harvest actions made
        if (harvestActionCount >= requiredHarvestActions)
            HarvestCrop(isToolRight, cropDetails, gridPropertyDetails, animator);
    }

    private void HarvestCrop(bool isUsingToolRight, CropDetails cropDetails, GridPropertyDetails gridPropertyDetails, Animator animator)
    {
        // Is there a harvested animation
        if (cropDetails.isHarvestedAnimation && animator != null)
        {
            // If harvest sprite then add to sprite renderer
            if (cropDetails.harvestedSprite != null)
            {
                if (cropHarvestedSpriteRenderer != null)
                {
                    cropHarvestedSpriteRenderer.sprite = cropDetails.harvestedSprite;
                }
            }

            if (isUsingToolRight)
            {
                animator.SetTrigger("harvestright");
            }
            else
            {
                animator.SetTrigger("harvestleft");
            }
        }

        // Delete crop from grid properties
        gridPropertyDetails.seedItemCode = -1;
        gridPropertyDetails.growthDays = -1;
        gridPropertyDetails.daysSinceLastHarvest = -1;
        gridPropertyDetails.daysSinceWatered = -1;

        // Should the crop be hidden before the harvested animation
        if (cropDetails.hideCropBeforeHarvestedAnimation)
        {
            GetComponentInChildren<SpriteRenderer>().enabled = false;
        }

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // Is there a harvested animation - Destroy this crop game object after animation completed
        if (cropDetails.isHarvestedAnimation && animator != null)
        {
            StartCoroutine(ProcessHarvestActionsAfterAnimation(cropDetails, gridPropertyDetails, animator));
        }
        else
        {

            HarvestActions(cropDetails, gridPropertyDetails);
        }
    }

    private IEnumerator ProcessHarvestActionsAfterAnimation(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails, Animator animator)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Harvested"))
        {
            yield return null;
        }

        HarvestActions(cropDetails, gridPropertyDetails);
    }

    private void HarvestActions(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        SpawnHarvestedItems(cropDetails);

        Destroy(gameObject);
    }

    private void SpawnHarvestedItems(CropDetails cropDetails)
    {
        // Spawn the item(s) to be produced
        for (int i = 0; i < cropDetails.cropProducedItemCode.Length; i++)
        {
            int cropsToProduce;

            // Calculate how many crops to produce
            if (cropDetails.cropProducedMinQuantity[i] == cropDetails.cropProducedMaxQuantity[i] ||
                cropDetails.cropProducedMaxQuantity[i] < cropDetails.cropProducedMinQuantity[i])
            {
                cropsToProduce = cropDetails.cropProducedMinQuantity[i];
            }
            else
            {
                cropsToProduce = Random.Range(cropDetails.cropProducedMinQuantity[i], cropDetails.cropProducedMaxQuantity[i] + 1);
            }

            for (int j = 0; j < cropsToProduce; j++)
            {
                Vector3 spawnPosition;
                if (cropDetails.spawnCropProducedAtPlayerPosition)
                {
                    //  Add item to the players inventory
                    InventoryManager.Instance.AddItem(InventoryLocation.player, cropDetails.cropProducedItemCode[i]);
                }
                else
                {
                    // Random position
                    spawnPosition = new Vector3(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f), 0f);
                    SceneItemsManager.Instance.InstantiateSceneItem(cropDetails.cropProducedItemCode[i], spawnPosition);
                }
            }
        }
    }

}
