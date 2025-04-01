using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private float currentMoveDirection = 0f;
    private float blendSpeed = 10f;
    private float rollDuration = 1f;
    private float damageDuration = 0.5f;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component is missing from the player.");
        }
    }

    public void SetMovement(float moveDirection)
    {
        if (animator != null)
        {
            currentMoveDirection = Mathf.Lerp(currentMoveDirection, moveDirection, blendSpeed * Time.deltaTime);
            animator.SetFloat("moveDirection", currentMoveDirection);
            Debug.Log($"Set moveDirection to {currentMoveDirection}");
        }
    }

    public void SetIdle()
    {
        if (animator != null)
        {
            currentMoveDirection = Mathf.Lerp(currentMoveDirection, 0f, blendSpeed * Time.deltaTime);
            animator.SetFloat("moveDirection", currentMoveDirection);
            Debug.Log($"Set idle, moveDirection to {currentMoveDirection}");
        }
    }

    public void SetMode(int mode)
    {
        if (animator != null)
        {
            animator.SetInteger("mode", mode);
            Debug.Log($"Set mode to {mode}");
        }
    }

    public void SetAttacking2(bool isAttacking2)
    {
        if (animator != null)
        {
            animator.SetBool("isAttacking2", isAttacking2);
            Debug.Log($"Set isAttacking2 to {isAttacking2}");
        }
    }

    public void SetStableSword(bool isStableSword)
    {
        if (animator != null)
        {
            animator.SetBool("isStableSword", isStableSword);
            Debug.Log($"Set isStableSword to {isStableSword}");
        }
    }

    public void SetDisarmed(bool isDisarmed)
    {
        if (animator != null)
        {
            animator.SetBool("isDisarmed", isDisarmed);
            Debug.Log($"Set isDisarmed to {isDisarmed}");
        }
    }

    public void SetIdleActive(bool isIdleActive)
    {
        if (animator != null)
        {
            animator.SetBool("isIdleActive", isIdleActive);
            Debug.Log($"Set isIdleActive to {isIdleActive}");
        }
    }

    public void SetRolling(bool isRolling)
    {
        if (animator != null)
        {
            Debug.Log($"Setting isRolling to {isRolling}");
            animator.SetBool("isRolling", isRolling);
            if (isRolling)
            {
                Invoke("ResetRolling", rollDuration);
            }
        }
        else
        {
            Debug.LogError("Animator is null in AnimationController!");
        }
    }

    public void SetDead(bool isDead)
    {
        if (animator != null)
        {
            Debug.Log($"Setting isDead to {isDead}");
            animator.SetBool("isDead", isDead);
        }
    }

    public void SetDamaged(bool isDamaged)
    {
        if (animator != null)
        {
            Debug.Log($"Setting isDamaged to {isDamaged}");
            animator.SetBool("isDamaged", isDamaged);
            if (isDamaged)
            {
                Invoke("ResetDamaged", damageDuration);
            }
        }
    }

    public bool IsAttacking()
    {
        if (animator == null) return false;
        bool isAttacking = animator.GetBool("isAttacking2") || animator.GetBool("isStableSword") || animator.GetBool("isDisarmed");
        Debug.Log($"IsAttacking: {isAttacking}");
        return isAttacking;
    }

    public bool IsDamaged()
    {
        if (animator == null) return false;
        bool isDamaged = animator.GetBool("isDamaged");
        Debug.Log($"IsDamaged: {isDamaged}");
        return isDamaged;
    }

    private void ResetRolling()
    {
        if (animator != null)
        {
            Debug.Log("Resetting isRolling to false");
            animator.SetBool("isRolling", false);
        }
    }

    private void ResetDamaged()
    {
        if (animator != null)
        {
            Debug.Log("Resetting isDamaged to false");
            animator.SetBool("isDamaged", false);
        }
    }
}