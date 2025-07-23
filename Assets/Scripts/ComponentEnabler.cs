using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using FishNet.Object;
using StarterAssets;
using System;

public class PlayerComponentsEnabler : NetworkBehaviour
{
    // for at multiplayer skal fungere er det veldig viktig at når player prefabs spawner inn, at kun den playeren skal styre får sine components enabled
    // mao hvis flere får feks input component enabled, vil en player kunne styre alle andre players
    // derfor bør components i player prefabs være disabled, og bli manuelt enabled i dette scriptet.


    [SerializeField] private GameObject mainCameraGO;
    [SerializeField] private GameObject PlayerFollowCamera;
    [SerializeField] private GameObject playerCapsuleGO;
    [SerializeField] private GameObject canvasGO;
    [SerializeField] private GameObject managersGO;

    public override void OnStartClient()
    {

        base.OnStartClient();

        if (!IsOwner)
            return;

        if (mainCameraGO == null)
            mainCameraGO = transform.Find("MainCamera")?.gameObject;

        if (PlayerFollowCamera == null)
            PlayerFollowCamera = transform.Find("PlayerFollowCamera")?.gameObject;

        if (playerCapsuleGO == null)
            playerCapsuleGO = transform.Find("playerCapsule")?.gameObject;

        if (canvasGO == null)
            canvasGO = transform.Find("Canvas")?.gameObject;

        if (managersGO == null)
            managersGO = transform.Find("Managers")?.gameObject;

        EnableMainCameraComponents();
        EnablePlayerFollowCameraComponents();
        EnablePlayerCapsuleComponents();
        EnableCanvasComponents();
        EnableManagerComponents();
        EnablePlayerComponents();
    }

    private void EnablePlayerFollowCameraComponents()
    {
        var cinemachineVirtualCamera = PlayerFollowCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        cinemachineVirtualCamera.enabled = true;
    }

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

        var singleHighlighter = mainCameraGO.GetComponent<SingleHighlighter>();
        if (singleHighlighter != null) singleHighlighter.enabled = true;

        var multipleHighlighter = mainCameraGO.GetComponent<MultipleHighlighter>();
        if (multipleHighlighter != null) multipleHighlighter.enabled = true;
    }

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

        var starterAssetsInputs = playerCapsuleGO.GetComponent<StarterAssetsInputs>();
        if (starterAssetsInputs != null) starterAssetsInputs.enabled = true;
    }

    private void EnablePlayerComponents()
    {
        SelectionManager selectionManager = GetComponent<SelectionManager>();
        if (selectionManager != null)
            selectionManager.enabled = true;

        PlayerStateMachine stateMachine = GetComponent<PlayerStateMachine>();
        if (stateMachine != null)
            stateMachine.enabled = true;
    }

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

    private void EnableManagerComponents()
    {
        if (managersGO == null)
            return;

        var inventoryToggleManager = managersGO.transform.Find("InventoryToggleManager");
        if (inventoryToggleManager != null)
        {
            var itm = inventoryToggleManager.GetComponent<InventoryToggleManager>();
            if (itm != null) itm.enabled = true;
        }

        var inputManager = managersGO.transform.Find("InputManager");
        if (inputManager != null)
        {
            var im = inputManager.GetComponent<InputManager>();
            if (im != null) im.enabled = true;
        }

        var hotbarManager = managersGO.transform.Find("HotbarManager");
        if (hotbarManager != null)
        {
            var hotbarController = hotbarManager.GetComponent<HotbarController>();
            if (hotbarController != null) hotbarController.enabled = true;

            var equipmentManager = hotbarManager.GetComponent<EquipmentManager>();
            if (equipmentManager != null) equipmentManager.enabled = true;
        }
    }
}
