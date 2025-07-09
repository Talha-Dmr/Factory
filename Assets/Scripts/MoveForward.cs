using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveForwardWithDynamicSwap : MonoBehaviour
{
    public float speed = 2f;
    public Vector3 direction = Vector3.forward;

    private bool isWaiting = false;
    private bool hasStoppedPermanently = false;

    // Collider'ları tekrar tetiklememek için
    [HideInInspector]
    public HashSet<Collider> triggeredColliders = new HashSet<Collider>();

    void Update()
    {
        if (!isWaiting && !hasStoppedPermanently)
        {
            transform.Translate(direction.normalized * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isWaiting || triggeredColliders.Contains(other)) return;

        triggeredColliders.Add(other);

        if (other.CompareTag("StopPoint"))
        {
            StopPointInfo info = other.GetComponent<StopPointInfo>();
            if (info != null && info.targetPrefab != null)
            {
                StartCoroutine(SwapWholeObjectAfterDelay(3f, info.targetPrefab, other));
            }
            else
            {
                StartCoroutine(PauseBeforeContinue(3f));
            }
        }
        else if (other.CompareTag("Camera"))
        {
            StartCoroutine(PauseBeforeContinue(2f));
        }
        else if (other.CompareTag("TerminalPoint"))
        {
            hasStoppedPermanently = true;
        }
    }

    IEnumerator SwapWholeObjectAfterDelay(float delay, GameObject targetPrefab, Collider lastCollider)
    {
        isWaiting = true;
        yield return new WaitForSeconds(delay);

        GameObject newObj = Instantiate(targetPrefab);
        newObj.transform.position = transform.position + direction.normalized * 0.05f; // 5cm ileri koy
        newObj.transform.rotation = transform.rotation;
        newObj.transform.localScale = transform.localScale;

        newObj.tag = tag;
        newObj.layer = gameObject.layer;

        MoveForwardWithDynamicSwap newScript = newObj.GetComponent<MoveForwardWithDynamicSwap>();
        if (newScript != null)
        {
            newScript.speed = speed;
            newScript.direction = direction;
            newScript.isWaiting = false;

            // Eski tetiklenmiş colliderları aktar
            newScript.triggeredColliders = new HashSet<Collider>(triggeredColliders);

            // Yeni objeyle çakışmayı önle
            Collider newCol = newObj.GetComponent<Collider>();
            if (newCol != null)
            {
                Physics.IgnoreCollision(newCol, lastCollider, true);
            }
        }

        // Rigidbody ayarları
        Rigidbody oldRb = GetComponent<Rigidbody>();
        Rigidbody newRb = newObj.GetComponent<Rigidbody>();
        if (oldRb != null && newRb != null)
        {
            newRb.useGravity = oldRb.useGravity;
            newRb.isKinematic = oldRb.isKinematic;
            newRb.mass = oldRb.mass;
            newRb.linearDamping = oldRb.linearDamping;
            newRb.angularDamping = oldRb.angularDamping;

            if (!newRb.isKinematic)
            {
                newRb.linearVelocity = oldRb.linearVelocity;
                newRb.angularVelocity = oldRb.angularVelocity;
            }
        }

        Destroy(gameObject);
    }

    IEnumerator PauseBeforeContinue(float duration)
    {
        isWaiting = true;
        yield return new WaitForSeconds(duration);
        isWaiting = false;
    }
}
