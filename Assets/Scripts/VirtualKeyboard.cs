using UnityEngine;
using TMPro; // Importante para usar TMP_InputField

public class VirtualKeyboard : MonoBehaviour
{
    public TMP_InputField inputField;

    public void OnButtonPressed(string character)
    {
        if (inputField == null) return;

        inputField.text += character;
        inputField.caretPosition = inputField.text.Length; // mueve el cursor al final
    }

    public void OnBackspacePressed()
    {
        if (inputField == null) return;
        if (inputField.text.Length > 0)
        {
            inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
            inputField.caretPosition = inputField.text.Length;
        }
    }
    public void SetInputField(TMP_InputField tmp)
    {
        inputField=tmp;
    }
}
