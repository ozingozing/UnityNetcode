using ChocoOzing;
public abstract class AimBaseState
{
    public abstract void EnterState(AimStateManager aim);
    public abstract void UpdateSatate(AimStateManager aim);
    public abstract void ExitState(AimStateManager aim);
}
