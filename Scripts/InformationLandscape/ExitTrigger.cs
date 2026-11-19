using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Trigger at the end of the Information Landscape walkway.
/// When the user reaches it, the music and the view fade out with DOTween,
/// then the hub scene (Leopardi's study) is loaded.
/// </summary>
public class ExitTrigger : MonoBehaviour
{
    [Header("Impostazioni Dissolvenza")]
    [Tooltip("CanvasGroup of the black fade panel")]
    [SerializeField] private CanvasGroup pannelloNero;

    [Tooltip("AudioSource of the background music")]
    [SerializeField] private AudioSource musicaDiSottofondo;

    [SerializeField] private float durataFade = 1.5f;

    private bool inUscita = false; // prevents starting the exit twice

    private void OnTriggerEnter(Collider other)
    {
        // Start the exit only for the player, and only once.
        if (other.CompareTag("Player") && !inUscita)
        {
            inUscita = true;
            StartCoroutine(EseguiDissolvenzaECarica());
        }
    }

    private IEnumerator EseguiDissolvenzaECarica()
    {
        // 1. Fade the music out
        if (musicaDiSottofondo != null)
        {
            musicaDiSottofondo.DOFade(0f, durataFade);
        }

        // 2. Fade the view to black
        if (pannelloNero != null)
        {
            pannelloNero.alpha = 0f;
            pannelloNero.gameObject.SetActive(true);
            pannelloNero.DOFade(1f, durataFade);

            // 3. Wait for the fade to finish, plus a small margin
            yield return new WaitForSeconds(durataFade + 0.2f);
        }
        else
        {
            Debug.LogWarning("ExitTrigger: no black panel assigned, loading the scene right away.");
            // Without the panel, still wait for the music fade to finish.
            if (musicaDiSottofondo != null) yield return new WaitForSeconds(durataFade);
        }

        // 4. Back to the hub scene (Leopardi's study)
        SceneManager.LoadScene("StudioLeo");
    }
}
