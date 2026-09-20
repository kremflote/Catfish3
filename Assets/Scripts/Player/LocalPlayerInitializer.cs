using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using FishNet.Object;
using StarterAssets;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerContext))]
public class LocalPlayerInitializer : NetworkBehaviour
{
    // Enables the camera, input, UI, and managers only on the owning player's clone.
    [SerializeField] private GameObject mainCameraGO;
    [SerializeField] private GameObject playerFollowCamera;
    [SerializeField] private GameObject playerCapsuleGO;
    [SerializeField] private GameObject canvasGO;
    [SerializeField] private GameObject managersGO;

    // FishNet calls this per client; only the owning clone gets cameras, input, and UI enabled.
    public override void OnStartClient()
    {

        base.OnStartClient();

        if (!IsOwner)
            return;

        ResolveReferences();

        EnableMainCameraComponents();
        EnablePlayerFollowCameraComponents();
        EnablePlayerCapsuleComponents();
        EnableCanvasComponents();
        EnableManagerComponents();
        EnablePlayerComponents();
    }

    // Finds the child objects that start disabled on networked player prefabs.
    private void ResolveReferences()
    {
        if (mainCameraGO == null)
            mainCameraGO = GetComponentInChildren<Camera>(true)?.gameObject;

        if (playerFollowCamera == null)
            playerFollowCamera = GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>(true)?.gameObject;

        if (playerCapsuleGO == null)
            playerCapsuleGO = GetComponentInChildren<FirstPersonController>(true)?.gameObject;

        if (canvasGO == null)
            canvasGO = GetComponentInChildren<Canvas>(true)?.gameObject;

        if (managersGO == null)
        {
            InventoryVisibilityController inventoryVisibilityController = GetComponentInChildren<InventoryVisibilityController>(true);
            managersGO = inventoryVisibilityController != null ? inventoryVisibilityController.transform.parent.gameObject : null;
        }
    }

    // Enables the Cinemachine follow camera for the local player only.
    private void EnablePlayerFollowCameraComponents()
    {
        if (playerFollowCamera == null)
            return;

        var cinemachineVirtualCamera = playerFollowCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        if (cinemachineVirtualCamera != null) cinemachineVirtualCamera.enabled = true;
    }

    // Enables the real camera/audio/UI-driving components for the local player.
    private void EnableMainCameraComponents()
    {
        if (mainCameraGO == null)
            return;

        var uacd = mainCameraGO.GetComponent<UniversalAdditionalCameraData>();
        if (uacd != null) uacd.enabled = true;

        var camera = mainCameraGO.GetComponent<Camera>();
        if (camera != null) camera.enabled = true;

        var audioListener = mainCameraGO.GetComponent<AudioListener>();
        if (audioListener != null) audioListener.enabled = true;

        var cinemachineBrain = mainCameraGO.GetComponent<Cinemachine.CinemachineBrain>();
        if (cinemachineBrain != null) cinemachineBrain.enabled = true;

        var inventoryController = mainCameraGO.GetComponent<InventoryController>();
        if (inventoryController != null) inventoryController.enabled = true;

        foreach (InventoryHighlighter inventoryHighlighter in mainCameraGO.GetComponents<InventoryHighlighter>())
            inventoryHighlighter.enabled = true;
    }

    // Enables the physical player body and input receiver on the owning clone.
    private void EnablePlayerCapsuleComponents()
    {
        if (playerCapsuleGO == null)
            return;

        var playerInput = playerCapsuleGO.GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = true;

        var characterController = playerCapsuleGO.GetComponent<CharacterController>();
        if (characterController != null) characterController.enabled = true;

        var firstPersonController = playerCapsuleGO.GetComponent<FirstPersonController>();
        if (firstPersonController != null) firstPersonController.enabled = true;

        var basicRigidBodyPush = playerCapsuleGO.GetComponent<BasicRigidBodyPush>();
        if (basicRigidBodyPush != null) basicRigidBodyPush.enabled = true;

        var playerInputState = playerCapsuleGO.GetComponent<PlayerInputState>();
        if (playerInputState != null) playerInputState.enabled = true;
    }

    // Enables root-level player systems that should only run for the owner.
    private void EnablePlayerComponents()
    {
        PlayerContext context = GetComponent<PlayerContext>();
        if (context == null)
            context = gameObject.AddComponent<PlayerContext>();

        context.ResolveReferences();

        SelectionManager selectionManager = GetComponent<SelectionManager>();
        if (selectionManager != null)
            selectionManager.enabled = true;

        PlayerStateMachine stateMachine = GetComponent<PlayerStateMachine>();
        if (stateMachine != null)
            stateMachine.enabled = true;
    }

    // Enables the local UI canvas so only the owning player sees and clicks their own inventory/HUD.
    private void EnableCanvasComponents()
    {
        if (canvasGO == null)
            return;

        var canvas = canvasGO.GetComponent<Canvas>();
        if (canvas != null) canvas.enabled = true;

        var scaler = canvasGO.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler != null) scaler.enabled = true;

        var raycaster = canvasGO.GetComponent<UnityEngine.UI.GraphicRaycaster>();
        if (raycaster != null) raycaster.enabled = true;
    }

    // Enables inventory/hotbar/equipment managers on the owner so remote clones do not process local UI.
    private void EnableManagerComponents()
    {
        if (managersGO == null)
            return;

        var inventoryVisibilityController = managersGO.GetComponentInChildren<InventoryVisibilityController>(true);
        if (inventoryVisibilityController != null) inventoryVisibilityController.enabled = true;

        var hotbarController = managersGO.GetComponentInChildren<HotbarController>(true);
        if (hotbarController != null) hotbarController.enabled = true;

        var equipmentManager = managersGO.GetComponentInChildren<EquipmentManager>(true);
        if (equipmentManager != null) equipmentManager.enabled = true;
    }
}
