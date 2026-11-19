using UnityEngine;
using System.Collections;

/// <summary>
/// Tracks which of the four reading paths have been completed and saves them in PlayerPrefs.
/// It is a singleton that survives scene loads (DontDestroyOnLoad), and it notifies the
/// progress HUD through C# events instead of being checked every frame.
/// It also runs small "after this voice line ends" sequences for objects in the scenes.
/// </summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }
    public const int TOTAL_PATHS = 4;
    private bool[] percorsiSbloccati = new bool[TOTAL_PATHS]; // completed paths

    // Events the UI listens to
    public event System.Action<int> OnPercorsoSbloccato; // a path was completed (passes its index)
    public event System.Action OnNascondiHUD;            // the HUD should hide

    void Awake()
    {
        // Singleton: keep the first instance, destroy duplicates when the hub scene reloads.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CaricaSalvataggi();
        }
        else Destroy(gameObject);
    }

    // Loads saved progress from PlayerPrefs.
    private void CaricaSalvataggi()
    {
        for (int i = 0; i < TOTAL_PATHS; i++)
            percorsiSbloccati[i] = PlayerPrefs.GetInt("Percorso_" + i, 0) == 1;
    }

    // Called e.g. by the shepherd to hide the stars while he speaks.
    public void ChiamaNascondiHUD()
    {
        OnNascondiHUD?.Invoke();
    }

    // Marks a path as completed, saves it and notifies the UI (only the first time).
    public void SbloccaPercorso(int idPercorso)
    {
        if (idPercorso < 0 || idPercorso >= TOTAL_PATHS) return;

        if (!percorsiSbloccati[idPercorso])
        {
            percorsiSbloccati[idPercorso] = true;
            PlayerPrefs.SetInt("Percorso_" + idPercorso, 1);
            PlayerPrefs.Save();

            OnPercorsoSbloccato?.Invoke(idPercorso);
        }
    }

    public int GetPercorsiCompletatiCount()
    {
        int count = 0;
        foreach (bool sbloccato in percorsiSbloccati)
            if (sbloccato) count++;
        return count;
    }

    public bool IsPercorsoSbloccato(int idPercorso)
    {
        if (idPercorso < 0 || idPercorso >= TOTAL_PATHS) return false;
        return percorsiSbloccati[idPercorso];
    }

    // --- Sequences that wait for a voice line to finish ---
    // They run here because this object is never disabled, so the coroutine keeps going
    // even if the object that started it is turned off right after the interaction.

    // Activates an object when the audio ends.
    public void AttivaDopoAudio(AudioSource audio, GameObject oggetto)
    {
        StartCoroutine(AttivaOggettoCoroutine(audio, oggetto));
    }

    private IEnumerator AttivaOggettoCoroutine(AudioSource audio, GameObject oggetto)
    {
        if (audio != null)
            while (audio.isPlaying)
                yield return new WaitForSeconds(0.5f);

        if (oggetto != null) oggetto.SetActive(true);
    }

    // Enables a script when the audio ends.
    public void AbilitaDopoAudio(AudioSource audio, MonoBehaviour script)
    {
        StartCoroutine(AbilitaScriptCoroutine(audio, script));
    }

    private IEnumerator AbilitaScriptCoroutine(AudioSource audio, MonoBehaviour script)
    {
        if (audio != null)
            while (audio.isPlaying)
                yield return new WaitForSeconds(0.5f);

        if (script != null) script.enabled = true;
    }

    // When the audio ends: shows the goat (optional), activates the next narrative blocks
    // and hides the branch of the path that was just played.
    public void AttivaBlocchiDopoAudio(AudioSource audio, GameObject[] blocchi, GameObject ramo, NpcPecora_Uno capra = null)
    {
        StartCoroutine(AttivaBlocchiCoroutine(audio, blocchi, ramo, capra));
    }

    private IEnumerator AttivaBlocchiCoroutine(AudioSource audio, GameObject[] blocchi, GameObject ramo, NpcPecora_Uno capra)
    {
        if (audio != null)
            while (audio.isPlaying)
                yield return new WaitForSeconds(0.5f);

        // Show the goat and make it sit
        if (capra != null)
        {
            capra.gameObject.SetActive(true);
            capra.enabled = true;
            yield return null; // wait one frame so the goat's Start runs first
            capra.ForceSit();
        }

        // Activate the next narrative blocks
        foreach (GameObject blocco in blocchi)
            if (blocco != null) blocco.SetActive(true);

        // Hide the current branch
        if (ramo != null) ramo.SetActive(false);
    }

    // Clears progress both in memory and on disk.
    public void ResetDati()
    {
        // 1. Clear the in-memory array
        for (int i = 0; i < TOTAL_PATHS; i++)
        {
            percorsiSbloccati[i] = false;
        }

        // 2. Clear PlayerPrefs (this also resets the tutorial flag)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
