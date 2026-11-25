using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ShiftPrototype
{
    public static class ShiftInput
    {
        public static Vector2 Move
        {
            get
            {
                var k = Keyboard.current;
                if (k == null) return Vector2.zero;
                return new Vector2(
                    (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0),
                    (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0));
            }
        }
        public static bool JumpDown => Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        public static bool JumpUp => Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame;
        public static bool JumpHeld => Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        public static bool DashDown => Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame;
        public static bool SwapDown => Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        public static bool ResetDown => Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
    }
}
