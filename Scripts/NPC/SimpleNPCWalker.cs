using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Moves a character (the shepherd) to a target point with the NavMesh,
/// switching between the walk and idle animations. It starts still until StartWalking is called.
/// </summary>
public class SimpleNPCWalker : MonoBehaviour
{
    public Transform target;
    public Animator animator;

    private NavMeshAgent agent;
    private bool isMoving = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();

        // Start still
        agent.isStopped = true;
    }

    void Update()
    {
        if (isMoving)
        {
            // Stop once the target has been reached
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                StopNPC();
            }
        }
    }

    // Starts the walk (called e.g. when the cup is picked up)
    public void StartWalking()
    {
        if (target == null) return;

        isMoving = true;
        agent.isStopped = false;
        agent.SetDestination(target.position);

        // Walk animation on
        animator.SetBool("IsWalking", true);
    }

    private void StopNPC()
    {
        isMoving = false;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Back to the idle animation
        animator.SetBool("IsWalking", false);
    }
}
