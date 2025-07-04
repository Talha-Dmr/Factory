using UnityEngine;
using System.Collections;

public class MoveForwardWithTriggerPause : MonoBehaviour
{
    public float speed = 2f;
    public Vector3 direction = Vector3.forward;

    private bool isWaiting = false;
    private bool hasStoppedPermanently = false;

    void Update()
    {
        if (!isWaiting && !hasStoppedPermanently)
        {
            transform.Translate(direction.normalized * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("TRIGGER ENTER: " + other.name + " / Tag: " + other.tag);

        if (other.CompareTag("StopPoint"))
        {
            Debug.Log("StopPoint ? 3 saniye duruyor...");
            StartCoroutine(PauseBeforeContinue(3f));
        }
        else if (other.CompareTag("Camera"))
        {
            Debug.Log("Camera ? 2 saniye duruyor...");
            StartCoroutine(PauseBeforeContinue(2f));
        }
        else if (other.CompareTag("TerminalPoint"))
        {
            Debug.Log("TerminalPoint ? kalýcý durdu.");
            hasStoppedPermanently = true;
        }
    }

    IEnumerator PauseBeforeContinue(float duration)
    {
        isWaiting = true;
        yield return new WaitForSeconds(duration);
        isWaiting = false;
    }
}
