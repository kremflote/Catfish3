using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InventoryToggleManager inventoryToggleManager;
    [SerializeField] private InventoryController inventoryController;

    // Sentraliserte noe inputs her som ikke har � gj�re med movement
    // b�r refaktoreres til � bruke unity sitt input system smartere


    public event Action OnTabPressed;
    public event Action OnQPressed;
    public event Action OnTPressed;
    public event Action OnUPressed;
    public event Action<int> OnHotbarKeyPressed;
    public event Action<string> OnMouseClick;

    private void Awake()
    {
        InitializeInventoryComponents();
    }

    void Update()
    {
        if (inventoryToggleManager == null || inventoryController == null)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
            {
                bool isOpen = inventoryToggleManager.GetIsOpen();
                if (isOpen && !inventoryController.SelectedItemIsNull())
                {
                    bool success = inventoryController.CancelPickupItem();
                    if (!success)
                    {
                        return;
                    }
                    else
                    {
                        Debug.Log("Player dropped item successfully");
                        inventoryController.UpdateHotbar();
                        OnTabPressed?.Invoke();
                        return;
                    }
                }
                if (isOpen && inventoryController.SelectedItemIsNull())
                {

                    inventoryController.UpdateHotbar();
                    OnTabPressed?.Invoke();
                    return;
            }
                if (!isOpen)
                {
                    inventoryController.UpdateInventoryFromHotbar();
                    OnTabPressed?.Invoke();
                    return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q)) OnQPressed?.Invoke();
        if (Input.GetKeyDown(KeyCode.T)) OnTPressed?.Invoke();
        if (Input.GetKeyDown(KeyCode.U)) OnUPressed?.Invoke();

        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                OnHotbarKeyPressed?.Invoke(i);
                break;
            }
        }

        if (Input.GetMouseButtonDown(0)) OnMouseClick?.Invoke("Left Click");
        if (Input.GetMouseButtonDown(1)) OnMouseClick?.Invoke("Right Click");
    }

    private void InitializeInventoryComponents()
    {
        if (inventoryController == null)
            inventoryController = transform.root.GetComponentInChildren<InventoryController>(true);

        if (inventoryToggleManager == null)
            inventoryToggleManager = transform.root.GetComponentInChildren<InventoryToggleManager>(true);

        if (inventoryController == null)
            Debug.LogWarning("InventoryController reference is missing.", this);

        if (inventoryToggleManager == null)
            Debug.LogWarning("InventoryToggleManager reference is missing.", this);
    }
    public String GetMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            return String.Format("Left Click");
        }
        else if (Input.GetMouseButtonDown(1))
        {
            return String.Format("Right Click");
        }
        else if (Input.GetMouseButtonDown(2))
        {
            return String.Format("Middle Click");
        }
        else
        {
            return String.Format("None");
        }
    }
}
