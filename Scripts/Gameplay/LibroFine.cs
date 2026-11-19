using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

/// <summary>
/// The book that closes a reading path ("libro finale"). Selecting it clears the scene,
/// plays the final recited stanzas with synchronized 3D words, then unlocks the path
/// and starts the closing sequence. StartParole and SetHighlight are connected
/// to the interactable's events in the Inspector.
/// </summary>
public class LibroFine : XRSimpleInteractable
{
    [Header("Sequence Settings")]
    [SerializeField] private GameObject parole;          // 3D words of the final reading
    [SerializeField] private WordSyncManager wordManager;
    [SerializeField] private GameObject[] objectsToHide;
    [SerializeField] private GameObject appoggio;        // support the words appear on
    [SerializeField] private GameObject moon;

    [Header("End Game Reference")]
    [SerializeField] private GameEnding gameEndingScript;

    [Header("Visuals")]
    [SerializeField] private Outline outline;
    [SerializeField] private GameObject modelloLibro;    // the book's 3D model

    [Header("Sistema Achievement")]
    [Tooltip("Index of the reading path this ending completes (0-3)")]
    public int idPercorso;

    private bool isEndingStarted = false;

    protected override void Awake()
    {
        base.Awake();

        // Initial state: hide what belongs to the final reading
        if (outline != null) outline.enabled = false;
        if (parole != null) parole.SetActive(false);
        if (appoggio != null) appoggio.SetActive(false);
    }

    // Starts the final reading (connected to Select Entered)
    public void StartParole()
    {
        if (isEndingStarted) return;

        isEndingStarted = true;
        interactionLayers = 0; // the book can no longer be selected
        SetHighlight(false);

        // Hide the progress stars during the poem
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.ChiamaNascondiHUD();
        }

        if (modelloLibro != null)
        {
            modelloLibro.SetActive(false);
        }
        moon.SetActive(true);
        // 1. Clear the scene
        foreach (var obj in objectsToHide)
        {
            if (obj != null) obj.SetActive(false);
        }

        if (appoggio != null) appoggio.SetActive(true);

        // 2. Show the words
        if (parole != null) parole.SetActive(true);

        // 3. Start the recited audio; the words follow it
        if (wordManager != null)
        {
            wordManager.IniziaSequenza();
            StartCoroutine(WaitUntilSequenceEnds());
        }
        else
        {
            Debug.LogError("LibroFine: WordSyncManager not assigned!");
            // Without the manager, go straight to the ending
            InvokeEndGame();
        }
    }

    private IEnumerator WaitUntilSequenceEnds()
    {
        // Wait for the recitation to end (checked twice per second)
        while (wordManager.audioSource.isPlaying)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // Short pause to let the last words settle
        yield return new WaitForSeconds(2.0f);

        InvokeEndGame();
    }

    private void InvokeEndGame()
    {
        // Mark the path as completed
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.SbloccaPercorso(idPercorso);
        }

        // Start the closing sequence
        if (gameEndingScript != null)
        {
            gameEndingScript.LaunchFinalSequence();
        }
        else
        {
            Debug.LogError("LibroFine: GameEnding not assigned!");
        }
    }

    // Highlight while pointed at (connected to the hover events), never once the reading has started
    public void SetHighlight(bool val)
    {
        if (isEndingStarted) val = false;
        if (outline != null) outline.enabled = val;
    }
}
