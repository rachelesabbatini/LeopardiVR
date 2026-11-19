using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Progress HUD: one star per reading path and a short notification when a path is completed.
/// It listens to AchievementManager's events instead of checking progress every frame,
/// and unsubscribes when its scene is unloaded (the manager survives scene loads).
/// </summary>
public class AchievementUI : MonoBehaviour
{
    [Header("Riferimenti Visivi")]
    [Tooltip("Main CanvasGroup of the achievement panel (stars and text)")]
    [SerializeField] private CanvasGroup pannelloPrincipale;

    [Tooltip("CanvasGroup of the notification text only")]
    [SerializeField] private CanvasGroup gruppoTesto;

    [SerializeField] private TMP_Text testoNotifica;
    [SerializeField] private Image[] iconeStelle; // one star per path

    [Header("Impostazioni Animazione")]
    [SerializeField] private float durataFade = 0.5f;
    [SerializeField] private float tempoASchermo = 3.0f; // seconds the notification stays on screen
    [SerializeField] private Color coloreStellaSbloccata = Color.yellow;
    [SerializeField] private Color coloreStellaBloccata = Color.gray;

    private void Start()
    {
        // At startup: notification text hidden, stars visible
        if (gruppoTesto != null) gruppoTesto.alpha = 0f;
        if (pannelloPrincipale != null) pannelloPrincipale.alpha = 1f;

        AggiornaColoriStelleIniziali();

        // Listen to the manager's events
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnPercorsoSbloccato += MostraNotifica;
            AchievementManager.Instance.OnNascondiHUD += NascondiTuttoTemporaneamente;
        }
    }

    private void OnDestroy()
    {
        // The manager survives scene loads, so this UI must unsubscribe when its scene is unloaded;
        // otherwise the manager would keep calling methods on a destroyed object.
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnPercorsoSbloccato -= MostraNotifica;
            AchievementManager.Instance.OnNascondiHUD -= NascondiTuttoTemporaneamente;
        }
    }

    // Colors each star according to whether its path has been completed
    private void AggiornaColoriStelleIniziali()
    {
        if (AchievementManager.Instance == null) return;
        for (int i = 0; i < iconeStelle.Length; i++)
        {
            iconeStelle[i].color = AchievementManager.Instance.IsPercorsoSbloccato(i) ? coloreStellaSbloccata : coloreStellaBloccata;
        }
    }

    // Called when the shepherd starts speaking: the stars fade out so they don't distract from the voice
    private void NascondiTuttoTemporaneamente()
    {
        if (pannelloPrincipale != null)
        {
            DOTween.Kill(pannelloPrincipale);
            pannelloPrincipale.DOFade(0f, durataFade);
        }
    }

    // Called when a path is completed (at the end of the final reading)
    private void MostraNotifica(int idPercorso)
    {
        int totali = AchievementManager.Instance.GetPercorsiCompletatiCount();
        testoNotifica.text = $"Percorso sbloccato {totali} di {AchievementManager.TOTAL_PATHS}";

        AggiornaColoriStelleIniziali();

        Sequence sequenza = DOTween.Sequence();

        // 1. Fade the stars panel back in
        if (pannelloPrincipale != null)
            sequenza.Append(pannelloPrincipale.DOFade(1f, durataFade));

        // 2. Fade the text in at the same time
        if (gruppoTesto != null)
            sequenza.Join(gruppoTesto.DOFade(1f, durataFade));

        // 3. "Pop" effect on the star that was just unlocked
        if (idPercorso >= 0 && idPercorso < iconeStelle.Length)
        {
            DOTween.Kill(iconeStelle[idPercorso].transform);
            iconeStelle[idPercorso].transform.localScale = Vector3.zero;
            sequenza.Join(iconeStelle[idPercorso].transform.DOScale(Vector3.one, durataFade).SetEase(Ease.OutBack));
        }

        // 4. Wait, then fade out the text and the stars together
        if (gruppoTesto != null)
        {
            sequenza.AppendInterval(tempoASchermo);

            sequenza.Append(gruppoTesto.DOFade(0f, durataFade));

            if (pannelloPrincipale != null)
            {
                sequenza.Join(pannelloPrincipale.DOFade(0f, durataFade));
            }
        }
    }
}
