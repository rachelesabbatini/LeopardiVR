using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// A stanza ("versi") in the Information Landscape.
/// Pointing at it highlights it; selecting it plays the recited stanza
/// and shows or hides its paraphrase panel.
/// </summary>
public class VersiVRInteraction : XRSimpleInteractable
{
    [Header("UI References")]
    public GameObject parafrasi; // paraphrase panel shown next to the stanza
    public Outline outline;      // highlight shown while the stanza is pointed at
    public AudioSource strofa;   // recited audio of this stanza

    protected override void Awake()
    {
        base.Awake(); // required by XRSimpleInteractable

        // Initial state: no highlight and no paraphrase.
        if (outline != null) outline.enabled = false;
        if (parafrasi != null) parafrasi.SetActive(false);
    }

    // Called by the XR Interaction Toolkit when the user presses Select (trigger/grip) on the stanza.
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        ToggleCanvas();
    }

    private void ToggleCanvas()
    {
        if (parafrasi != null)
        {
            // Show the paraphrase if it is hidden, hide it if it is visible.
            bool isActive = parafrasi.activeSelf;
            parafrasi.SetActive(!isActive);
        }
        if (strofa != null)
        {
            // Play the recited stanza from the beginning.
            strofa.Play();
        }
    }

    // Turns the highlight on or off.
    public void SetHighlight(bool val)
    {
        if (outline != null) outline.enabled = val;
    }

    // Highlight while the controller's ray points at the stanza.
    protected override void OnHoverEntered(HoverEnterEventArgs args) => SetHighlight(true);
    protected override void OnHoverExited(HoverExitEventArgs args) => SetHighlight(false);
}
