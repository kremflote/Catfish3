# Catfish3
Version 3 of my Catfish prototype

## Player Architecture Notes

- `LocalPlayerInitializer` enables camera, input, UI, and managers only for the owning player clone.
- `PlayerContext` is the shared lookup point for player references such as input, state machine, inventory toggle, controller, and camera.
- `FirstPersonController` owns body movement and camera rotation, while `PlayerStateMachine` owns gameplay modes.
- Player states decide input mode: gameplay, inventory overlay, or world interaction.
- Interactables receive `PlayerContext` instead of several separate player references.
