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
        // Sol click ile popup açılır veya kapanır
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == this.transform)
                {
                    if (popupTextObject != null)
                    {
                        // Eğer açıksa kapat, kapalıysa aç
                        popupTextObject.SetActive(!popupTextObject.activeSelf);
                    }
                }
            }
        }
    }
}
