using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG
{
    public class VRKeyboard : MonoBehaviour
    {
        public UnityEngine.UI.InputField AttachedInputField;
        public bool UseShift = false;

        [Header("Sound FX")]
        public AudioClip KeyPressSound;

        public GameObject ObjectA;
        public GameObject ObjectB;

        private bool isKeyBeingPressed = false;

        public List<VRKeyboardKey> KeyboardKeys;
        private List<List<VRKeyboardKey>> keyboardLayout;

        private int activeRowIndex = 0;
        private int activeKeyIndex = 0;

        public bool IsRayInputActive { get; set; } = false;

        private Vector2 stickInput = Vector2.zero;
        private const float InputCooldown = 0.2f;

        private static readonly Color DefaultColor = new Color(71f / 255f, 71f / 255f, 71f / 255f);
        private static readonly Color HighlightColor = Color.yellow;

        void Start()
        {
            LoadKeyboardLayout("Main Keyboard");
        }

        public void ChangeKeyboard(string keyboard)
        {
            LoadKeyboardLayout(keyboard);
        }

        void Update()
        {
            HandleStickInput();
            HandleKeyPressInput();
        }

        private void LoadKeyboardLayout(string keyboardName)
        {
            KeyboardKeys = new List<VRKeyboardKey>();
            keyboardLayout = new List<List<VRKeyboardKey>>();

            Transform keyboardTransform = transform.Find(keyboardName);
            if (keyboardTransform == null)
            {
                Debug.LogError($"{keyboardName} not found! Check your hierarchy.");
                return;
            }

            for (int i = 1; i <= 4; i++)
            {
                Transform row = keyboardTransform.Find($"Row {i}");
                if (row == null) continue;

                var rowKeys = new List<VRKeyboardKey>();
                foreach (Transform button in row)
                {
                    if (button.TryGetComponent(out VRKeyboardKey keyboardKey))
                    {
                        rowKeys.Add(keyboardKey);
                        KeyboardKeys.Add(keyboardKey);
                    }
                }

                if (rowKeys.Count > 0)
                {
                    keyboardLayout.Add(rowKeys);
                }
            }

            HighlightKey();
        }

        private void HandleStickInput()
        {
            stickInput.x = Input.GetAxis("Horizontal");
            stickInput.y = Input.GetAxis("Vertical");

            if (Mathf.Abs(stickInput.x) > 0.2f || Mathf.Abs(stickInput.y) > 0.2f)
            {
                if (!isKeyBeingPressed)
                {
                    ProcessStickInput();
                    isKeyBeingPressed = true;
                    StartCoroutine(ResetKeyPressCooldown());
                }
            }
        }

        public void AttachToInputField(UnityEngine.UI.InputField inputField)
        {
            AttachedInputField = inputField;
        }

        private void ProcessStickInput()
        {
            if (stickInput.x > 0.5f) MoveHighlight(1, 0);
            else if (stickInput.x < -0.5f) MoveHighlight(-1, 0);
            else if (stickInput.y > 0.5f) MoveHighlight(0, -1);
            else if (stickInput.y < -0.5f) MoveHighlight(0, 1);
        }

        private void HandleKeyPressInput()
        {
             OVRInput.Update();
            if (OVRInput.GetDown(OVRInput.RawButton.A))
            {
                PressKey();
            }
        }

        public void PressKey()
        {
            if (keyboardLayout == null || keyboardLayout.Count == 0) return;

            var activeKey = keyboardLayout[activeRowIndex][activeKeyIndex];
            activeKey?.OnKeyHit();
            PlayClickSound();
        }

        public virtual void PlayClickSound()
        {
            if (KeyPressSound != null)
            {
                VRUtils.Instance.PlaySpatialClipAt(KeyPressSound, transform.position, 1f, 0.5f);
            }
        }

        public void HandleKeyPress(string key)
        {
            if (AttachedInputField == null) return;

            switch (key)
            {
                case "SPACE":
                    {
                        int caretPosSpace = AttachedInputField.caretPosition;
                        string currentTexts = AttachedInputField.text;

                        AttachedInputField.text = currentTexts.Insert(caretPosSpace, " ");

                        // Сдвигаем курсор
                        AttachedInputField.caretPosition = caretPosSpace + 1;
                        break;
                    }
                case "BACKSPACE":
                    if (AttachedInputField.text.Length > 0)
                    {
                        int caretPosBackspace = AttachedInputField.caretPosition;
                        if (caretPosBackspace == 0)
                        {
                            PlayClickSound(); // Still play the click sound
                            return;
                        }

                        AttachedInputField.text = AttachedInputField.text.Substring(0, AttachedInputField.text.Length - 1);
                    }
                    break;
                case "ENTER":
                    AttachedInputField.text = string.Empty;
                    break;
                case "SHIFT":
                    ToggleShift();
                    break;
                case "SWITCHMAIN":
                    ToggleObjects(false);
                    LoadKeyboardLayout("Secondary Keyboard");
                    break;
                case "SWITCHSECOND":
                    ToggleObjects(true);
                    LoadKeyboardLayout("Main Keyboard");
                    break;
                case "<":
                    MoveCaret(-1); // Сдвиг влево
                    break;
                case ">":
                    MoveCaret(1); // Сдвиг вправо
                    break;
                default:
                    int caretPos = AttachedInputField.caretPosition; // Текущая позиция курсора
                    string currentText =AttachedInputField.text; // Текущий текст в поле

                    // Вставляем символ в позицию курсора
                    string newText = currentText.Insert(caretPos, key);
                    AttachedInputField.text = newText;

                    // Обновляем позицию курсора
                    AttachedInputField.caretPosition = caretPos + key.Length;
                    AttachedInputField.selectionAnchorPosition = caretPos + key.Length; // Снимаем выделение
                    break;
            }
        }

        public void MoveCaret(int direction)
        {
            if (AttachedInputField == null) return;

            // Текущая позиция курсора
            int caretPosition = AttachedInputField.caretPosition;

            // Рассчитаем новую позицию
            int newPosition = Mathf.Clamp(caretPosition + direction, 0, AttachedInputField.text.Length);

            // Установим новую позицию
            AttachedInputField.caretPosition = newPosition;
            AttachedInputField.selectionAnchorPosition = newPosition; 
        }

        private void ToggleShift()
        {
            UseShift = !UseShift;
            foreach (var key in KeyboardKeys)
            {
                key.ToggleShift();
            }
        }

        public void ToggleObjects(bool showFirstObject)
        {
            ObjectA.SetActive(showFirstObject);
            ObjectB.SetActive(!showFirstObject);
        }

        public void MoveHighlight(int xOffset, int yOffset)
        {
            // Сохраняем текущий индекс клавиши
            int previousKeyIndex = activeKeyIndex;

            // Обновляем индексы строк и клавиш
            activeRowIndex += yOffset;
            activeRowIndex = Mathf.Clamp(activeRowIndex, 0, keyboardLayout.Count - 1);

            // Получаем новую строку
            var newRow = keyboardLayout[activeRowIndex];

            if (newRow == null || newRow.Count == 0)
            {
                activeRowIndex -= yOffset; // Возвращаемся к предыдущей строке
                return;
            }

            // Сохраняем активный индекс в пределах новой строки
            activeKeyIndex = Mathf.Clamp(previousKeyIndex, 0, newRow.Count - 1);

            // Для горизонтального перемещения
            activeKeyIndex += xOffset;
            activeKeyIndex = Mathf.Clamp(activeKeyIndex, 0, newRow.Count - 1);

            HighlightKey();
        }

        private void HighlightKey()
        {
            foreach (var row in keyboardLayout)
            {
                foreach (var key in row)
                {
                    SetKeyHighlight(key, DefaultColor);
                }
            }

            SetKeyHighlight(keyboardLayout[activeRowIndex][activeKeyIndex], HighlightColor);
        }

        private void SetKeyHighlight(VRKeyboardKey key, Color color)
        {
            if (key?.GetComponent<UnityEngine.UI.Button>() is { } button)
            {
                var colors = button.colors;
                colors.normalColor = color;
                button.colors = colors;
            }
        }

        private IEnumerator ResetKeyPressCooldown()
        {
            yield return new WaitForSeconds(InputCooldown);
            isKeyBeingPressed = false;
        }
    }
}
