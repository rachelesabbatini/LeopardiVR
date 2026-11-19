using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// The goat ("pecora") of one reading path. It reacts differently each time the user
/// plays the path: the first and second time it starts a different voice line and brings in
/// the shepherd; from the third time on it sits still with a red outline and can no longer be used.
/// The number of interactions is saved in PlayerPrefs.
/// </summary>
public class NpcPecora_Uno : XRSimpleInteractable
{
    [Header("Salvataggio & Identità")]
    [SerializeField] private string idSalvataggio = "Pecora_Sessioni_Nodo"; // PlayerPrefs key
    [SerializeField] private Color coloreCompletato = Color.red;  // outline once completed
    [SerializeField] private Color coloreHover = Color.white;     // outline while pointed at

    [Header("NPC State & Animation")]
    public Animator animator;
    [SerializeField] private Transform staticTarget; // sitting position

    [Header("Interazione & Sottotitoli")]
    [SerializeField] private Outline outline;
    [SerializeField] private SubtitleTrigger subtitleTriggerPrimaVolta;   // first time
    [SerializeField] private SubtitleTrigger subtitleTriggerSecondaVolta; // second time

    [Header("Audio & Riferimenti")]
    [SerializeField] private AudioSource audioBloccoG;
    [SerializeField] private GameObject[] objectsToHide;

    [Header("Riferimento Pastore")]
    [SerializeField] private GameObject pastore1;                    // the shepherd
    [SerializeField] private XRSimpleInteractable pastoreInteractable;

    private int _interazioniEffettuate = 0;                     // interactions in previous playthroughs
    private bool _interazioneGiaFattaInQuestaSessione = false;  // already used in this playthrough

    protected override void Awake()
    {
        base.Awake();
        if (outline != null) outline.enabled = false;
    }

    void Start()
    {
        // How many times the goat was used in previous playthroughs
        _interazioniEffettuate = PlayerPrefs.GetInt(idSalvataggio, 0);

        // --- THIRD TIME OR LATER ---
        if (_interazioniEffettuate >= 2)
        {
            ConfiguraStatoRossoTerzaSessione();
        }
        else
        {
            // First and second time: the shepherd is not there yet
            if (pastore1 != null) pastore1.SetActive(false);
            if (pastoreInteractable != null) pastoreInteractable.enabled = false;
        }
    }

    private void ConfiguraStatoRossoTerzaSessione()
    {
        // 1. The goat can no longer be used
        this.enabled = false;

        // 2. Permanent red outline
        if (outline != null)
        {
            outline.enabled = true;
            outline.OutlineColor = coloreCompletato;
        }

        // 3. Sitting pose
        ForceSit();

        // 4. No shepherd in the scene
        if (pastore1 != null) pastore1.SetActive(false);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (_interazioneGiaFattaInQuestaSessione || _interazioniEffettuate >= 2) return;

        _interazioneGiaFattaInQuestaSessione = true;

        // Turn off the hover outline right away
        if (outline != null) outline.enabled = false;

        // Change the world immediately
        AttivaMondoImmediatamente();

        // Start the voice line for the first or second time
        SubtitleTrigger currentTrigger = (_interazioniEffettuate == 0) ? subtitleTriggerPrimaVolta : subtitleTriggerSecondaVolta;
        StartCoroutine(SequenzaNarrativaSessione(currentTrigger));
    }

    private void AttivaMondoImmediatamente()
    {
        if (pastore1 != null) pastore1.SetActive(true);
        if (pastoreInteractable != null) pastoreInteractable.enabled = false; // not selectable yet

        foreach (var obj in objectsToHide) if (obj != null) obj.SetActive(false);
    }

    private IEnumerator SequenzaNarrativaSessione(SubtitleTrigger trigger)
    {
        if (trigger != null && SubtitleManager.Instance != null)
            SubtitleManager.Instance.StartSubtitles(trigger.jsonFileName, trigger.audioSource);

        if (audioBloccoG != null)
        {
            audioBloccoG.Play();
            while (audioBloccoG.isPlaying)
            {
                yield return new WaitForSeconds(0.2f);
            }
        }

        // The voice line is over: the shepherd can now be selected
        if (pastoreInteractable != null) pastoreInteractable.enabled = true;

        // Count this interaction for the next playthrough
        _interazioniEffettuate++;
        PlayerPrefs.SetInt(idSalvataggio, _interazioniEffettuate);
        PlayerPrefs.Save();

        // No more interaction with the goat in this playthrough
        this.enabled = false;
    }

    // --- HOVER (first and second time only) ---
    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        // White outline only if the goat has not been used yet
        if (!_interazioneGiaFattaInQuestaSessione && _interazioniEffettuate < 2 && outline != null)
        {
            outline.enabled = true;
            outline.OutlineColor = coloreHover;
        }
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        // From the third time on the red outline stays; otherwise turn it off
        if (_interazioniEffettuate < 2 && outline != null)
        {
            outline.enabled = false;
        }
    }

    // Puts the goat in the sitting pose at its sitting position (also called by AchievementManager)
    public void ForceSit()
    {
        if (animator != null)
        {
            animator.Play("Ovis sitting (1_40)_GRT__002", 0, 0f);
            animator.Update(0f);
            animator.SetBool("IsSitting", true);
        }
        if (staticTarget != null)
        {
            transform.position = staticTarget.position;
            transform.rotation = staticTarget.rotation;
        }
    }
}
