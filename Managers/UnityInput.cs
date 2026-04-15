using UnityEngine;
using UnityEngine.InputSystem;

namespace Quantum.Managers
{
    internal static class UnityInput
    {
        // Keyboard (KeyCode)
        internal static bool GetKey(Key key) => Keyboard.current[key].isPressed;
        internal static bool GetKeyDown(Key key) => Keyboard.current[key].wasPressedThisFrame;
        internal static bool GetKeyUp(Key key) => Keyboard.current[key].wasReleasedThisFrame;

        // Keyboard (string key names)
        internal static bool GetKey(string keyName) => Input.GetKey(keyName);
        internal static bool GetKeyDown(string keyName) => Input.GetKeyDown(keyName);
        internal static bool GetKeyUp(string keyName) => Input.GetKeyUp(keyName);

        // Mouse
        internal static Vector3 mousePosition => Input.mousePosition;

        internal static bool GetMouseButton(int button) => Input.GetMouseButton(button);

        internal static bool GetMouseButtonDown(int button) => Input.GetMouseButtonDown(button);

        internal static bool GetMouseButtonUp(int button) => Input.GetMouseButtonUp(button);

        internal static Vector2 MouseScrollDelta => Input.mouseScrollDelta;
    }
}
