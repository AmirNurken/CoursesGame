using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 2f;
    public float damage = 20f;

    void Start()
    {
        Debug.Log("Bullet spawned at position: " + transform.position + ", scale: " + transform.localScale);
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Debug.Log("Bullet MeshRenderer is " + (renderer.enabled ? "enabled" : "disabled"));
        }
        else
        {
            Debug.LogError("MeshRenderer component is missing from the bullet!");
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Debug.Log($"Bullet collider: IsTrigger = {collider.isTrigger}, Layer = {LayerMask.LayerToName(gameObject.layer)}");
        }
        else
        {
            Debug.LogError("Collider component is missing from the bullet!");
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log($"Bullet Rigidbody: IsKinematic = {rb.isKinematic}, Collision Detection = {rb.collisionDetectionMode}");
        }
        else
        {
            Debug.LogError("Rigidbody component is missing from the bullet!");
        }

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Bullet triggered with: {other.gameObject.name}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}, IsTrigger: {other.isTrigger}, Active: {other.gameObject.activeSelf}, Parent: {other.transform.parent?.gameObject.name}");

        if (LayerMask.LayerToName(other.gameObject.layer) == "Enemy")
        {
            // Проверяем наличие ZombieController
            ZombieController zombie = other.GetComponentInParent<ZombieController>();
            if (zombie != null && !zombie.IsDead())
            {
                zombie.TakeDamage(damage);
                Debug.Log($"Bullet dealt {damage} damage to {other.gameObject.name} (ZombieController) via OnTriggerEnter");
            }
            else
            {
                // Проверяем наличие MagicZombieController
                MagicZombieController magicZombie = other.GetComponentInParent<MagicZombieController>();
                if (magicZombie != null)
                {
                    magicZombie.TakeDamage(damage);
                    Debug.Log($"Bullet dealt {damage} damage to {other.gameObject.name} (MagicZombieController) via OnTriggerEnter");
                }
                else if (zombie == null && magicZombie == null)
                {
                    Debug.LogWarning($"No ZombieController or MagicZombieController found on {other.gameObject.name} or its parents despite Enemy layer.");
                }
            }
        }
        else
        {
            Debug.Log($"Bullet hit non-Enemy object: {other.gameObject.name} on layer {LayerMask.LayerToName(other.gameObject.layer)}. Ignoring.");
        }

        Destroy(gameObject);
    }
}