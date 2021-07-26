using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldItemView : InterfaceItemView, IPointerClickHandler
{
    [SerializeField] private PlayerInventory _playerInventory;
    [SerializeField] private GraphicRaycaster canvasRaycaster;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // //No item held
        // if (itemImageObject.GetComponent<Image>().sprite == null && itemCountObject.GetComponent<Text>().text == "")
        // {
            List<RaycastResult> results = new List<RaycastResult>();
            canvasRaycaster.Raycast(eventData, results);

            foreach (var res in results)
            {
                if (res.gameObject.tag == "InvItemSlot")
                {
                    res.gameObject.GetComponent<InventoryViewItemSlot>().OnPointerClick(eventData);
                    break;
                }
            }
        // }
    }
}
