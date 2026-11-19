using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Grazing behavior for the sheep of the flock: walk to a random destination,
/// stop and eat, idle briefly, repeat. Each sheep gets a random speed and walk-cycle
/// offset, so the flock never moves in sync. The sheep start still until StartAI is called.
/// </summary>
public class AIControl : MonoBehaviour
{
    public GameObject[] goalLocation; // possible destinations on the NavMesh
    private NavMeshAgent agent;
    private Animator anim;
    [SerializeField] private AudioSource bleatAudio;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // --- Movement settings ---
        agent.speed = 2f;
        agent.acceleration = 2.5f;
        agent.angularSpeed = 120f;
        agent.stoppingDistance = 1.2f;
        agent.autoBraking = true;

        // Sheep start still
        agent.isStopped = true;

        // Random walk-cycle offset and speed, so each sheep moves differently
        anim.SetFloat("wOffset", Random.Range(0.0f, 1.0f));
        float sm = Random.Range(0.8f, 1.2f);
        anim.SetFloat("SpeedMult", sm);
        agent.speed *= sm;
    }

    // --- Setup for narrative block F (called by the clock) ---
    public void SetupForBlocF()
    {
        // Place the sheep at a random destination from the list
        if (goalLocation != null && goalLocation.Length > 0)
        {
            int startIndex = Random.Range(0, goalLocation.Length);

            // Disable the agent while moving the sheep, to avoid NavMesh errors
            agent.enabled = false;
            transform.position = goalLocation[startIndex].transform.position;
            agent.enabled = true;
        }

        // Start by eating, before walking off
        SetAnimState(eat: true);

        // Start the grazing behavior
        StartAI();
    }

    // Starts the grazing loop (called e.g. when the cup is picked up)
    public void StartAI()
    {
        if (goalLocation == null || goalLocation.Length == 0)
        {
            Debug.LogWarning($"{name} has no destinations assigned!");
            return;
        }

        if (agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
            StartCoroutine(StartWithDelay());
        }
    }

    // Small random delay, so the sheep don't all start on the same frame
    IEnumerator StartWithDelay()
    {
        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));

        if (goalLocation.Length > 0)
        {
            int i = Random.Range(0, goalLocation.Length);
            agent.SetDestination(goalLocation[i].transform.position);
            StartCoroutine(SheepRoutine());
        }
    }

    IEnumerator SheepRoutine()
    {
        while (true)
        {
            // 1. WALK to a random destination
            SetAnimState(walk: true);
            agent.isStopped = false;

            int i = Random.Range(0, goalLocation.Length);
            agent.SetDestination(goalLocation[i].transform.position);

            // Bleat with a slightly different pitch each time
            if (bleatAudio != null)
            {
                bleatAudio.pitch = Random.Range(1.1f, 1.3f);
                bleatAudio.Play();
            }

            // Wait until it arrives (checked a few times per second, not every frame)
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                yield return new WaitForSeconds(0.2f);
            }

            // 2. STOP
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            // 3. EAT
            SetAnimState(eat: true);
            yield return new WaitForSeconds(Random.Range(3f, 5f));

            // 4. SHORT IDLE
            SetAnimState(idle: true);
            yield return new WaitForSeconds(Random.Range(1f, 1.5f));
        }
    }

    // Sets the Animator state (only one is true at a time)
    void SetAnimState(bool idle = false, bool walk = false, bool eat = false)
    {
        if (anim == null) return;
        anim.SetBool("IsIdle", idle);
        anim.SetBool("IsMoving", walk);
        anim.SetBool("IsEating", eat);
    }
}
