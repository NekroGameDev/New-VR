using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG
{
    public class VRKeyboardKey : MonoBehaviour
    {
        private UnityEngine.UI.Button _button;
        private UnityEngine.UI.Text _buttonText;

        private VRKeyboard _vrKeyboard;

        public string Keycode;
        public string KeycodeShift;

        private bool _isKeyBeingPressed = false;

        [HideInInspector]
        public bool UseShiftKey = false;

        void Awake()
        {
            _button = GetComponent<UnityEngine.UI.Button>();
            _buttonText = GetComponentInChildren<UnityEngine.UI.Text>();

            if (_button != null)
            {
                _button.onClick.AddListener(OnKeyHit);
            }

            _vrKeyboard = GetComponentInParent<VRKeyboard>();
        }

        public void ToggleShift()
        {
            if (Keycode == "ENTER" || Keycode == "SWITCHMAIN") return;

            UseShiftKey = !UseShiftKey;

            if (_buttonText == null)
            {
                Debug.LogError("Button text is missing!");
                return;
            }

            _buttonText.text = UseShiftKey && !string.IsNullOrEmpty(KeycodeShift) ? KeycodeShift : Keycode;
        }

        public void OnKeyHit()
        {
            if (_isKeyBeingPressed) return;

            _isKeyBeingPressed = true;
            string key = UseShiftKey && !string.IsNullOrEmpty(KeycodeShift) ? KeycodeShift : Keycode;

            if (_vrKeyboard != null && !_vrKeyboard.IsRayInputActive)
            {
                _vrKeyboard.HandleKeyPress(key);
            }

            Debug.Log("Pressed key: " + key);
            _isKeyBeingPressed = false;
        }
    }
}
