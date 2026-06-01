using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityStandardAssets.CrossPlatformInput.PlatformSpecific
{
    public class StandaloneInput : VirtualInput
    {
        public override float GetAxis(string name, bool raw)
        {
            if (name == "Horizontal")
            {
                float val = 0;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) val += 1;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) val -= 1;
                return val;
            }
            if (name == "Vertical")
            {
                float val = 0;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) val += 1;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) val -= 1;
                return val;
            }
            if (name == "Mouse X") return Mouse.current.delta.x.ReadValue();
            if (name == "Mouse Y") return Mouse.current.delta.y.ReadValue();
            if (name == "Mouse ScrollWheel") return Mouse.current.scroll.y.ReadValue();
            return 0;
        }


        public override bool GetButton(string name)
        {
            if (name == "Jump") return Keyboard.current.spaceKey.isPressed;
            if (name == "Fire1") return Mouse.current.leftButton.isPressed;
            if (name == "Fire2") return Mouse.current.rightButton.isPressed;
            if (name == "Fire3") return Mouse.current.middleButton.isPressed;
            if (name == "Submit") return Keyboard.current.enterKey.isPressed;
            if (name == "Cancel") return Keyboard.current.escapeKey.isPressed;
            return false;
        }


        public override bool GetButtonDown(string name)
        {
            if (name == "Jump") return Keyboard.current.spaceKey.wasPressedThisFrame;
            if (name == "Fire1") return Mouse.current.leftButton.wasPressedThisFrame;
            if (name == "Fire2") return Mouse.current.rightButton.wasPressedThisFrame;
            if (name == "Fire3") return Mouse.current.middleButton.wasPressedThisFrame;
            if (name == "Submit") return Keyboard.current.enterKey.wasPressedThisFrame;
            if (name == "Cancel") return Keyboard.current.escapeKey.wasPressedThisFrame;
            return false;
        }


        public override bool GetButtonUp(string name)
        {
            if (name == "Jump") return Keyboard.current.spaceKey.wasReleasedThisFrame;
            if (name == "Fire1") return Mouse.current.leftButton.wasReleasedThisFrame;
            if (name == "Fire2") return Mouse.current.rightButton.wasReleasedThisFrame;
            if (name == "Fire3") return Mouse.current.middleButton.wasReleasedThisFrame;
            if (name == "Submit") return Keyboard.current.enterKey.wasReleasedThisFrame;
            if (name == "Cancel") return Keyboard.current.escapeKey.wasReleasedThisFrame;
            return false;
        }


        public override void SetButtonDown(string name)
        {
            throw new Exception(
                " This is not possible to be called for standalone input. Please check your platform and code where this is called");
        }


        public override void SetButtonUp(string name)
        {
            throw new Exception(
                " This is not possible to be called for standalone input. Please check your platform and code where this is called");
        }


        public override void SetAxisPositive(string name)
        {
            throw new Exception(
                " This is not possible to be called for standalone input. Please check your platform and code where this is called");
        }


        public override void SetAxisNegative(string name)
        {
            throw new Exception(
                " This is not possible to be called for standalone input. Please check your platform and code where this is called");
        }


        public override void SetAxisZero(string name)
        {
            throw new Exception(
                " This is not possible to be called for standalone input. Please check your platform and code where this is called");
        }


        public override void SetAxis(string name, float value)
        {
            throw new Exception(
                " This is not possible to be called for standalone input. Please check your platform and code where this is called");
        }


        public override Vector3 MousePosition()
        {
            return Mouse.current.position.ReadValue();
        }
    }
}
