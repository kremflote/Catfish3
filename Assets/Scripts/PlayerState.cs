using StarterAssets;

public abstract class PlayerState
{
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void LateUpdate() { }
}
