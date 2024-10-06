using UnityEngine;
using System;
using System.Collections;

public class NPCBehavior : MonoBehaviour
{
    public float speed = 2f;
    public Animator animator;
    private Vector3 targetPosition;
    public bool isWalking = false;
    public bool isStopped = false;
    public Action<NPCBehavior> OnReachedTarget;

    void Update()
    {
        if (isWalking && !isStopped)
        {
            MoveTowardsTarget();
        }
    }

    private void MakeFootSound()
    {
        StartCoroutine(PlayFootstepWithDelay());
    }

    private IEnumerator PlayFootstepWithDelay()
    {
        // Generate a random delay between minDelay and maxDelay
        float randomDelay = UnityEngine.Random.Range(0f, 0.1f);

        // Wait for the random delay
        yield return new WaitForSeconds(randomDelay);

        // Play the footstep sound after the delay
        Broadcaster.Broadcast("PlaySFX", "step");
    }


    public void SetTargetPosition(Vector3 position, Action<NPCBehavior> callback)
    {
        targetPosition = position;
        isWalking = true;
        isStopped = false;
        OnReachedTarget = callback;
        animator.SetBool("isWalking", true);
        animator.SetBool("isStopped", false);
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            StopWalking();
            OnReachedTarget?.Invoke(this);
        }
    }

    public void StopWalking()
    {
        isWalking = false;
        animator.SetBool("isWalking", false);
        animator.SetBool("isStopped", true);
    }

    public void StopBehind()
    {
        isWalking = false;
        isStopped = true;
        animator.SetBool("isWalking", false);
        animator.SetBool("isStopped", true);
    }

    public void ActivateHolding()
    {
        animator.SetBool("Holding", true);
    }

    public void ResumeWalking()
    {
        isStopped = false;
        isWalking = true;
        animator.SetBool("isWalking", true);
        animator.SetBool("isStopped", false);
    }
}
