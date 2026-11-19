using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

/// <summary>
/// The seated shepherd ("pastore") at the end of a reading path. Selecting him hides him
/// and the scene, plays the final recited stanzas with synchronized 3D words,
/// then unlocks the path and starts the closing sequence.
/// </summary>
public class PastoreVRInteractable : XRSimpleInteractable
{
    [Header("Posizionamento")]
    public Transform seatTarget; // where the shepherd sits

    [Header("Sequence Settings")]
    [SerializeField] private GameObject parole;            // 3D words of the final reading
    [SerializeField] private WordSyncManager wordManager;
    [SerializeField] private GameObject[] objectsToHide;
    [SerializeField] private GameObject appoggio;          // support the words appear on
    [SerializeField] private GameObject moon;
    [SerializeField] private Outline outline;

    [Header("End Game Reference")]
    [SerializeField] private GameEnding gameEndingScript;

    [Header("Sistema Achievement")]
    public int idPercorso; // reading path completed by this ending (0-3)

    private Animator _anim;
    private bool _isParoleActive = false;

    protected override void Awake()
    {
        base.Awake();
        _anim = GetComponent<Animator>();

        // Seat the shepherd at his position
        if (seatTarget != null)
        {
            transform.position = seatTarget.position;
            transform.rotation = seatTarget.rotation;
        }

        if (outline != null) outline.enabled = false;
        if (parole != null) parole.SetActive(false);
        if (appoggio != null) appoggio.SetActive(false);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if (!_isParoleActive) StartParole();
    }

    // Starts the final reading
    public void StartParole()
    {
        _isParoleActive = true;
        interactionLayers = 0; // the shepherd can no longer be selected

        // Hide the progress stars during the poem
        if (AchievementManager.Instance != null) AchievementManager.Instance.ChiamaNascondiHUD();

        // Hide the shepherd's model
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) r.enabled = false;

        if (outline != null) outline.enabled = false;

        moon.SetActive(true);

        // Clear the scene and show the words
        foreach (var obj in objectsToHide)
            if (obj != null) obj.SetActive(false);

        if (appoggio != null) appoggio.SetActive(true);
        if (parole != null) parole.SetActive(true);

        // Start the recited audio; the words follow it
        if (wordManager != null)
        {
            wordManager.IniziaSequenza();
            StartCoroutine(WaitUntilSequenceEnds());
        }
    }

    private IEnumerator WaitUntilSequenceEnds()
    {
        // Wait for the recitation to end (checked twice per second)
        if (wordManager.audioSource != null)
        {
            while (wordManager.audioSource.isPlaying)
                yield return new WaitForSeconds(0.5f);
        }

        // Short pause after the last word
        yield return new WaitForSeconds(1.5f);
        InvokeEndGame();
    }

    private void InvokeEndGame()
    {
        // Mark the path as completed, then close the experience
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.SbloccaPercorso(idPercorso);
        }

        if (gameEndingScript != null)
            gameEndingScript.LaunchFinalSequence();
    }

    // Highlight while pointed at, never once the reading has started
    public void SetHighlight(bool val)
    {
        if (_isParoleActive) return;
        if (outline != null) outline.enabled = val;
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args) => SetHighlight(true);
    protected override void OnHoverExited(HoverExitEventArgs args) => SetHighlight(false);
}
