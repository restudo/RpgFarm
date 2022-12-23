using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShop : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameDescription;
    [SerializeField] private TextMeshProUGUI itemCostValue;

    [SerializeField] private Transform container;
    [SerializeField] private Transform shopItemTemplate;
    [SerializeField] private SO_ItemList itemList = null;

    void Start()
    {
        // shopItemTemplate.gameObject.SetActive(false);
        int positionIndex = 0;

        // Get all items in the so_itemdetails
        foreach (ItemDetails itemDetails in itemList.itemDetails)
        {
            if (itemDetails.itemCost > -1)
            {
                // CreateItemButton(itemDetails.itemSprite, itemDetails.itemDescription, itemDetails.itemCost, positionIndex);
                positionIndex++;
            }
        }
    }

    public void SetItemPosition(Vector2 pos)
    {
        GetComponent<RectTransform>().anchoredPosition += pos;
    }

    private void CreateItemButton(Sprite itemSprite, string itemName, int itemCost, int positionIndex)
    {
        // Transform shopItemTranform = Instantiate(shopItemTemplate, container);
        // RectTransform shopItemRectTranform = shopItemTranform.GetComponent<RectTransform>();

        // float shopItemHeight = 30f;
        // shopItemRectTranform.anchoredPosition = new Vector2(0, -shopItemHeight * positionIndex);

        // shopItemTranform.Find("ItemNameText").GetComponent<TextMeshProUGUI>().SetText(itemName);
        // shopItemTranform.Find("GoldText").GetComponent<TextMeshProUGUI>().SetText(itemCost.ToString());

        // shopItemTranform.Find("ItemIcon").GetComponent<Image>().sprite = itemSprite;

        itemImage.sprite = itemSprite;
        itemNameDescription.text = itemName;
        itemCostValue.text = itemCost.ToString();
    }
}
