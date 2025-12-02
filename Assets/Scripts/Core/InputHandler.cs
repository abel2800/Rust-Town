using UnityEngine;

namespace NeonArena.Core
{
    /// <summary>
    /// Centralized input handling system
    /// Provides clean interface for input queries across the game
    /// </summary>
    public static class InputHandler
    {
        // Movement
        public static Vector2 GetMovementInput()
        {
            return new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );
        }

        // Look
        public static Vector2 GetLookInput()
        {
            return new Vector2(
                Input.GetAxisRaw("Mouse X"),
                Input.GetAxisRaw("Mouse Y")
            );
        }

        // Actions
        public static bool GetJumpPressed()
        {
            return Input.GetButtonDown("Jump");
        }

        public static bool GetJumpHeld()
        {
            return Input.GetButton("Jump");
        }

        public static bool GetSprintHeld()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        public static bool GetFirePressed()
        {
            return Input.GetMouseButtonDown(0);
        }

        public static bool GetFireHeld()
        {
            return Input.GetMouseButton(0);
        }

        public static bool GetPausePressed()
        {
            return Input.GetKeyDown(KeyCode.Escape);
        }

        // Utility
        public static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public static void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}

