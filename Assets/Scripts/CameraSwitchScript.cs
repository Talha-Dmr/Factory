using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    private Camera cam1;
    private Camera cam2;
    private Camera cam3;

    private float timer = 0f;
    public float switchInterval = 5f; // Seconds to wait before switching
    private int currentIndex = 0;

    void Start()
    {
        // Automatically find cameras by name in the scene
        cam1 = GameObject.Find("CAMERA1").GetComponent<Camera>();
        cam2 = GameObject.Find("CAMERA2").GetComponent<Camera>();
        cam3 = GameObject.Find("CAMERA3").GetComponent<Camera>();

        ActivateCamera(0);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= switchInterval)
        {
            timer = 0f;
            currentIndex = (currentIndex + 1) % 3;
            ActivateCamera(currentIndex);
        }
    }

    void ActivateCamera(int index)
    {
        cam1.enabled = (index == 0);
        cam2.enabled = (index == 1);
        cam3.enabled = (index == 2);
    }
}
