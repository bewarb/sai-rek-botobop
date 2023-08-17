using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBehavior : MonoBehaviour
{
    public float pickupLifetime = 60f;
    public float duration = 60f;
    public string pickupType = "";
    public AudioClip pickupSFX;
    public Sprite sprite;

    void Start()
    {
        Destroy(gameObject, pickupLifetime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioSource.PlayClipAtPoint(pickupSFX, transform.position);

            other.GetComponent<ThirdPersonMovementController>().PowerUp(pickupType, duration, sprite);

            Destroy(gameObject);
        }
    }   
}
