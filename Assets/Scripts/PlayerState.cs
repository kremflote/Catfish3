using StarterAssets;

public abstract class PlayerState
{
    protected FirstPersonController controller;
    protected StarterAssetsInputs input;

    public PlayerState(FirstPersonController controller)
    {
        this.controller = controller;
        this.input = controller._input;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void LateUpdate() { }
}
