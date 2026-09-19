using UnityEngine;

public class HotbarSelector : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private SingleHighlighter singleHighlighter;


    private int selectedIndex = 0;

    private void Awake()
    {
        if (inputManager == null)
            inputManager = transform.root.GetComponentInChildren<InputManager>(true);

        if (singleHighlighter == null)
            singleHighlighter = transform.root.GetComponentInChildren<SingleHighlighter>(true);

        if (inputManager != null)
            inputManager.OnHotbarKeyPressed += HandleHotbarKeyPress;
        else
            Debug.LogWarning("InputManager reference is missing.", this);
    }

    private void HandleHotbarKeyPress(int index)
    {
        selectedIndex = index;
        Debug.Log($"Selected hotbar slot: {selectedIndex}");

    }

    public int GetSelectedIndex() => selectedIndex;

    private void OnDestroy()
    {
        if (inputManager != null)
            inputManager.OnHotbarKeyPressed -= HandleHotbarKeyPress;
    }
}
