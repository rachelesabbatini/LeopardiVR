using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using DG.Tweening;

/// <summary>
/// The cup in the Gameplay Environment. Grabbing it starts a narrative beat:
/// the voice line plays with subtitles, the flock and the shepherd start moving,
/// and when the line ends the next narrative block appears and the cup is released and hidden.
/// </summary>
public class CupPickUpVR : XRGrabInteractable
{
    [Header("NPC & Logic")]
    [SerializeField] private AIControl[] npcGroup;  // the flock
    [SerializeField] private SimpleNPCWalker npc2;  // the shepherd

    [Header("Audio")]
    [SerializeField] private AudioSource ambientAudio;
    [SerializeField] private AudioSource drinkAudio;

    [Header("Visuals")]
    [SerializeField] private Outline outline;

    [Header("Subtitles")]
    [SerializeField] private SubtitleTrigger subtitleTrigger;

    [Header("Blocco narrativo seguente")]
    [SerializeField] private GameObject blocco; // next narrative block

    [Header("Guida")]
    [SerializeField] private GuideArrow frecciaGuida;   // arrow pointing to the next interaction
    [SerializeField] private float pausaPostAudio = 0.5f;

    private bool _hasDrunk = false;

    protected override void Awake()
    {
        base.Awake();
        if (outline != null) outline.enabled = false;
    }

    // Highlight while pointed at, only until the cup has been used
    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        if (outline != null && !_hasDrunk)
        {
            outline.enabled = true;
        }
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        if (outline != null) outline.enabled = false;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        // The sequence runs only the first time the cup is grabbed
        if (!_hasDrunk)
        {
            _hasDrunk = true;
            if (outline != null) outline.enabled = false;
            StartDrinkingSequence();
        }
    }

    private void StartDrinkingSequence()
    {
        // Duration of the voice line (fallback if there is none)
        float durataTotale = 2.5f;

        if (subtitleTrigger != null && SubtitleManager.Instance != null)
        {
            if (subtitleTrigger.audioSource != null && subtitleTrigger.audioSource.clip != null)
            {
                durataTotale = subtitleTrigger.audioSource.clip.length;
            }
            SubtitleManager.Instance.StartSubtitles(subtitleTrigger.jsonFileName, subtitleTrigger.audioSource);
        }

        // Drinking sound, flock and shepherd start moving, ambient sound
        if (drinkAudio != null) { drinkAudio.volume = 0.1f; drinkAudio.Play(); }
        if (npcGroup != null) foreach (var npc in npcGroup) if (npc != null) npc.StartAI();
        if (npc2 != null) npc2.StartWalking();
        if (ambientAudio != null) ambientAudio.Play();

        float tempoDiAttesa = durataTotale + pausaPostAudio;

        // When the voice line ends:
        DOVirtual.DelayedCall(tempoDiAttesa, () => {

            // show the next narrative block and the guide arrow
            if (blocco != null) blocco.SetActive(true);

            if (frecciaGuida != null) frecciaGuida.MostraConRitardo(0f);

            // Release the cup from the user's hand before disabling it:
            // disabling an interactable that is still selected leaves the controller in an inconsistent state.
            if (isSelected)
            {
                interactionManager.SelectExit(interactorsSelecting[0], this);
            }

            // Now the cup is no longer selected and can be safely disabled
            gameObject.SetActive(false);
        });
    }
}
