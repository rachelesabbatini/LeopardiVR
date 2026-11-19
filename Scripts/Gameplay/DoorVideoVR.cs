using UnityEngine;
using DG.Tweening;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

/// <summary>
/// The door at the end of the labyrinth. Selecting it opens the door with DOTween,
/// hides the room, switches to the lunar skybox while a voice line plays,
/// then restores the world and teleports the player to the next area.
/// </summary>
public class DoorVideoVR : XRSimpleInteractable
{
    [Header("Visuals")]
    [SerializeField] private Outline outline;

    [Header("Sequence Settings")]
    [SerializeField] private Material skyBoxLunar;         // skybox shown during the voice line
    [SerializeField] private AudioSource audioSource;      // voice line
    [SerializeField] private GameObject[] objectsToHide;   // hidden when the door opens
    [SerializeField] private GameObject[] objectsToActivate; // shown at the end of the sequence
    [SerializeField] private GameObject labirinto;         // the labyrinth, removed after the teleport

    [Header("Teleport & Animation")]
    [SerializeField] private TeleportingPlayerVR teleporter;
    [SerializeField] private Vector3 targetRotation = new Vector3(0, 90f, 0);
    [SerializeField] private float rotationSpeed = 1.5f;   // duration of the door opening (seconds)
    [SerializeField] private float sequenceDelay = 0.5f;

    private Material originalSkyBox;
    private bool isCutsceneActive = false;
    private bool _isOpen = false;

    protected override void Awake()
    {
        base.Awake();
        if (outline != null) outline.enabled = false;
    }

    void Start()
    {
        // Remember the scene's own skybox, to restore it at the end.
        originalSkyBox = RenderSettings.skybox;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        // The door can be opened only once.
        if (!_isOpen && !isCutsceneActive)
        {
            StartCoroutine(ExecuteDoorSequence());
        }
    }

    private IEnumerator ExecuteDoorSequence()
    {
        isCutsceneActive = true;
        _isOpen = true;
        SetHighlight(false);

        // 1. Open the door
        transform.DOLocalRotate(targetRotation, rotationSpeed);

        yield return new WaitForSeconds(sequenceDelay);

        // 2. Hide the room
        foreach (var obj in objectsToHide)
        {
            if (obj != null) obj.SetActive(false);
            else Debug.LogWarning("DoorVideoVR: an element of objectsToHide is not assigned.");
        }

        // 3. Switch to the lunar skybox and update the ambient lighting
        RenderSettings.skybox = skyBoxLunar;
        DynamicGI.UpdateEnvironment();

        // 4. Play the voice line and wait for it to end
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
        }
        else
        {
            Debug.LogError("DoorVideoVR: AudioSource or clip missing, waiting 2 seconds instead.");
            yield return new WaitForSeconds(2f);
        }

        EndSequence();
    }

    private void EndSequence()
    {
        // Restore the original skybox
        RenderSettings.skybox = originalSkyBox;
        DynamicGI.UpdateEnvironment();

        foreach (var obj in objectsToActivate)
            if (obj != null) obj.SetActive(true);

        // Move the player to the next area
        if (teleporter != null)
            teleporter.TeleportPlayer();

        // Remove the labyrinth, now that the player has left it
        labirinto.SetActive(false);
        isCutsceneActive = false;
    }

    // Highlight while pointed at (connected to the hover events), never during the sequence.
    public void SetHighlight(bool val)
    {
        if (isCutsceneActive) val = false;
        if (outline != null) outline.enabled = val;
    }
}
