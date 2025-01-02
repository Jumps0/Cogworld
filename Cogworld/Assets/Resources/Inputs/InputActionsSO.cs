using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputActionsSO", menuName = "SO Systems/InputActionsSO")]
public class InputActionsSO : ScriptableObject
{
    [SerializeField] private PlayerInputActions inputActions;

    public PlayerInputActions InputActions
    {
        get
        {
            if (inputActions == null)
            {
                inputActions = new PlayerInputActions();
                inputActions.Enable();
            }
            return inputActions;
        }
    }
}