using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OLDInventorySystem : MonoBehaviour, IDropHandler
{

    public GameObject[] inventoryScreensUI;
    public List<GameObject> slotList = new List<GameObject>();
    public List<String> ItemList = new List<String>();
    private GameObject itemToAdd;
    private GameObject slotToEquip;
    public bool isOpen;
    public bool isFull;
    public int inventoryLimit;

    public void SetIsFull(string state)
    {
        this.isFull = bool.Parse(state);
    }

    void Start()
    {
        isOpen = false;
        FillInventoryScreens();
        PopulateSlotList();
    }

    private void FillInventoryScreens()
    {
        Transform root = transform.root;
        Transform canvas = root.Find("Canvas");
        inventoryScreensUI = new GameObject[]
            {
                canvas.Find("expandable_botleft")?.gameObject
            };
    }

    private void PopulateSlotList()
    {
        foreach (GameObject inventoryScreen in inventoryScreensUI) 
        {
            
            PopulateSlotListHelper(inventoryScreen);
        }
    }

    private void PopulateSlotListHelper(GameObject inventoryScreenUI)
    {
        // Get the Transform component from the GameObject first
        Transform screenTransform = inventoryScreenUI.transform;

        // Get the first child of the screen
        Transform firstGrandchild = screenTransform.GetChild(0);

        // Now loop through its children
        foreach (Transform child in firstGrandchild)
        {
            if (child.CompareTag("slot"))
            {
                inventoryLimit++;
                slotList.Add(child.gameObject);
            }
        }
    }


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab) && !isOpen)
        {

            isOpen = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Input.GetKeyDown(KeyCode.Tab) && isOpen)
        {
            isOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (OLDDragDrop.itemBeingDragged != null)
        {
            OLDDragDrop.itemBeingDragged.transform.SetParent(transform);
            OLDDragDrop.itemBeingDragged.transform.localPosition = Vector3.zero;
        }
    }

    public void AddToInventory(string itemName)
    {
            slotToEquip = FindNextEmptySlot();
            itemToAdd = Instantiate(Resources.Load<GameObject>(itemName), slotToEquip.transform.position, slotToEquip.transform.rotation);
            itemToAdd.transform.SetParent(slotToEquip.transform);
            ItemList.Add(itemName);

        if (ItemList.Count >= inventoryLimit) 
        {
            SetIsFull("True");
        }
        
    }

    private GameObject FindNextEmptySlot()
    {
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount == 0) 
            {
                return slot;
            }
        }

        return new GameObject();
    }

    public bool CheckIfFull()
    {
        int counter = 0;
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount >0)
            {
                counter += 1;
            }
        }

        if (counter == inventoryLimit)
        {
            return true;
        }
        else return false;
    }
}