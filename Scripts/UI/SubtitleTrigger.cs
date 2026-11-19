using UnityEngine;

/// <summary>
/// Data holder for one voice line: the name of its subtitle file
/// (a JSON in a Resources folder) and the AudioSource that plays it.
/// Interactable objects pass these to SubtitleManager.
/// </summary>
public class SubtitleTrigger : MonoBehaviour
{
    [Header("Configurazione specifica")]
    public string jsonFileName;       // e.g. "poesiaBloccoG"
    public AudioSource audioSource;   // the voice line for this point
}
