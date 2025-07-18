using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float fastSpeed = 30f;
    public float mouseSensitivity = 2f;

    float yaw;
    float pitch;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void Update()
    {
        // SAĞ MOUSE BASILIYSA DÖNÜŞ AKTİF
        if (Mouse.current.rightButton.isPressed)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Vector2 mouse = Mouse.current.delta.ReadValue() * mouseSensitivity;
            yaw += mouse.x;
            pitch -= mouse.y;
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // WASD + Q/E HAREKET
        Vector3 move = Vector3.zero;
        if (Keyboard.current.wKey.isPressed) move += transform.forward;
        if (Keyboard.current.sKey.isPressed) move -= transform.forward;
        if (Keyboard.current.aKey.isPressed) move -= transform.right;
        if (Keyboard.current.dKey.isPressed) move += transform.right;
        if (Keyboard.current.qKey.isPressed) move -= transform.up;
        if (Keyboard.current.eKey.isPressed) move += transform.up;

        float speed = Keyboard.current.leftShiftKey.isPressed ? fastSpeed : moveSpeed;
        transform.position += move.normalized * speed * Time.deltaTime;
    }
}
