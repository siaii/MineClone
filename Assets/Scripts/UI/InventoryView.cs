using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    private bool isInventoryActive = false;
    public bool IsInventoryActive
    {
        get => isInventoryActive;
        private set => isInventoryActive = value;
    }

    [SerializeField] private GameObject inventoryViewUI;
    // Start is called before the first frame update
    void Start()
    {
        IsInventoryActive = false;
        inventoryViewUI.SetActive(isInventoryActive);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Inventory") || Input.GetButtonDown("Cancel"))
        {
            if (Input.GetButtonDown("Cancel"))
            {
                IsInventoryActive = false;
            }
            else
            {
                IsInventoryActive = !IsInventoryActive;
            }
            inventoryViewUI.SetActive(isInventoryActive);
            if (isInventoryActive)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }   
}
