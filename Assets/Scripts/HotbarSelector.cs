using UnityEngine;

public class HotbarSelector : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private SingleHighlighter singleHighlighter;


    private int selectedIndex = 0;

    private void Awake()
    {
        if (inputManager == null)
        {
            Transform managers = transform.parent;
            Transform inputManagerGO = managers.Find("InputManager");
            inputManager = inputManagerGO.GetComponent<InputManager>();

            Transform player = managers.parent;
            Transform mainCamera = player.Find("MainCamera");
            singleHighlighter = mainCamera.GetComponent<SingleHighlighter>();
        }
        inputManager.OnHotbarKeyPressed += HandleHotbarKeyPress;
    }

    private void HandleHotbarKeyPress(int index)
    {
        selectedIndex = index;
        Debug.Log($"Selected hotbar slot: {selectedIndex}");

    }

    public int GetSelectedIndex() => selectedIndex;

    private void OnDestroy()
    {
        inputManager.OnHotbarKeyPressed -= HandleHotbarKeyPress;
    }
}
