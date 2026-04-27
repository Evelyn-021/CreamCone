using Unity.VisualScripting;
using UnityEngine;

public class AnimationManager 
{
private StatesAnimsAbstract actualState;
public void setState(StatesAnimsAbstract newState)
    {
        actualState = newState;
    }
}
