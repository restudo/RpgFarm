using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MouseItemData : MonoBehaviour
{
    public Image itemSprite;
    public TextMeshProUGUI itemCount;
    public InventorySlot assignedInventorySlot;

    void Awake()
    {
        itemSprite.color = Color.clear;
        itemCount.text = "";
    }

    public void UpdateMouseSlot(InventorySlot invSlot)
    {
        assignedInventorySlot.AssignItem(invSlot);
        itemSprite.sprite = invSlot.ItemData.icon;
        itemCount.text = invSlot.StackSize.ToString();
        itemSprite.color = Color.white;
    }

    void Update()
    {
        if (assignedInventorySlot.ItemData != null)
        {
            transform.position = Input.mousePosition;

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
            {
                ClearSlot();
            }
        }
    }

    public void ClearSlot()
    {
        assignedInventorySlot.ClearSlot();
        itemCount.text = "";
        itemSprite.color = Color.clear;
        itemSprite.sprite = null;
    }

    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
