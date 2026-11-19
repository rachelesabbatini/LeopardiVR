using UnityEngine;

/// <summary>
/// Trigger volume along the walkway of the Information Landscape.
/// When the user walks in, the stanza ("versi") appears; when they walk out,
/// the stanza and its paraphrase are hidden and the recited audio stops.
/// </summary>
public class VersiTrigger : MonoBehaviour
{
    [Header("Oggetti UI")]
    public GameObject versi;     // the stanza object (with its VersiVRInteraction)
    public GameObject parafrasi; // the stanza's paraphrase panel

    [Header("Audio")]
    public AudioSource audioDeiVersi; // recited audio of the stanza

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            versi.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            versi.SetActive(false);

            // Leaving the area also closes the paraphrase and stops the voice.
            if (parafrasi != null)
            {
                parafrasi.SetActive(false);
            }
            if (audioDeiVersi != null)
            {
                audioDeiVersi.Stop();
            }
        }
    }
}
