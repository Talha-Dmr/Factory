using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PopupText : MonoBehaviour
{
    public GameObject popupTextObject; // Aynı text objesini burada da ata

    void Start()
    {
        if (popupTextObject != null)
            popupTextObject.SetActive(false);
    }

    void Update()
    {
        // Sol click ile popup açılır
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == this.transform)
                {
                    if (popupTextObject != null)
                        popupTextObject.SetActive(true);
                }
            }
        }

        // ESC ile popup kapanır
        if (popupTextObject != null && popupTextObject.activeSelf)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                popupTextObject.SetActive(false);
        }
    }
}

