using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using FishNet.Object;
using StarterAssets;
using System;

public class PlayerComponentsEnabler : NetworkBehaviour
{
    // for at multiplayer skal fungere er det veldig viktig at n�r player prefabs spawner inn, at kun den playeren skal styre f�r sine components enabled
    // mao hvis flere f�r feks input component enabled, vil en player kunne styre alle andre players
    // derfor b�r components i player prefabs v�re disabled, og bli manuelt enabled i dette scriptet.


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

        ResolveReferences();

        EnableMainCameraComponents();
        EnablePlayerFollowCameraComponents();
        EnablePlayerCapsuleComponents();
        EnableCanvasComponents();
        EnableManagerComponents();
        EnablePlayerComponents();
    }

    private void ResolveReferences()
    {
        if (mainCameraGO == null)
            mainCameraGO = GetComponentInChildren<Camera>(true)?.gameObject;

        if (PlayerFollowCamera == null)
            PlayerFollowCamera = GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>(true)?.gameObject;

        if (playerCapsuleGO == null)
            playerCapsuleGO = GetComponentInChildren<FirstPersonController>(true)?.gameObject;

        if (canvasGO == null)
            canvasGO = GetComponentInChildren<Canvas>(true)?.gameObject;

        if (managersGO == null)
        {
            InputManager inputManager = GetComponentInChildren<InputManager>(true);
            managersGO = inputManager != null ? inputManager.transform.parent.gameObject : null;
        }
    }
    private void EnablePlayerFollowCameraComponents()
    {
        if (PlayerFollowCamera == null)
            return;

        var cinemachineVirtualCamera = PlayerFollowCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        if (cinemachineVirtualCamera != null) cinemachineVirtualCamera.enabled = true;
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

        var playerInputState = playerCapsuleGO.GetComponent<PlayerInputState>();
        if (playerInputState != null) playerInputState.enabled = true;
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

        var inventoryToggleManager = managersGO.GetComponentInChildren<InventoryToggleManager>(true);
        if (inventoryToggleManager != null) inventoryToggleManager.enabled = true;

        var inputManager = managersGO.GetComponentInChildren<InputManager>(true);
        if (inputManager != null) inputManager.enabled = true;

        var hotbarController = managersGO.GetComponentInChildren<HotbarController>(true);
        if (hotbarController != null) hotbarController.enabled = true;

        var equipmentManager = managersGO.GetComponentInChildren<EquipmentManager>(true);
        if (equipmentManager != null) equipmentManager.enabled = true;
    }
}
