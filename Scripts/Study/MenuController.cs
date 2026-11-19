using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu in Leopardi's study, opened from the book.
/// "Studia" loads the Information Landscape; "Gioca" loads the Gameplay Environment,
/// or, if some paths are already completed, asks whether to continue or start a new game.
/// Progress is cleared on quit and at every launch, so each test participant starts fresh.
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("Pannelli")]
    [SerializeField] private GameObject pannelloPrincipale;  // main menu panel
    [SerializeField] private GameObject pannelloSceltaGioca; // "continue or new game" panel

    [Header("Bottoni Specifici")]
    [SerializeField] private GameObject bottoneContinua;

    [Header("Configurazione")]
    [SerializeField] private string nomeScenaGioco = "MoonScene"; // Gameplay Environment scene

    private void Start()
    {
        pannelloPrincipale.SetActive(true);
        pannelloSceltaGioca.SetActive(false);
    }

    // "Gioca" (Play) button
    public void PremutoGioca()
    {
        int stelle = 0;
        if (AchievementManager.Instance != null)
            stelle = AchievementManager.Instance.GetPercorsiCompletatiCount();

        if (stelle == 0)
        {
            // No path completed yet: go straight to the game.
            CaricaScenaGioco();
        }
        else
        {
            // At least one path completed: offer "continue" or "new game".
            pannelloPrincipale.SetActive(false);
            pannelloSceltaGioca.SetActive(true);

            if (bottoneContinua != null) bottoneContinua.SetActive(true);
        }
    }

    // "New game" button
    public void PremutoNuovaPartita()
    {
        // Clear progress both in memory and on disk.
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.ResetDati();
        }

        // Reload the menu scene so the tutorial appears again.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // "Continue" button
    public void PremutoContinua()
    {
        CaricaScenaGioco();
    }

    // "Studia" (Learn) button: loads the Information Landscape.
    public void PremutoStudia()
    {
        SceneManager.LoadScene("BasicScene");
    }

    public void TornaAlMenuPrincipale()
    {
        pannelloPrincipale.SetActive(true);
        pannelloSceltaGioca.SetActive(false);
    }

    private void CaricaScenaGioco()
    {
        SceneManager.LoadScene(nomeScenaGioco);
    }

    // "Quit" button
    public void EsciDalGioco()
    {
        // The session operator closes the app: clear everything for the next participant.
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.ResetDati();
        }
        else
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    // Kiosk mode for user testing and public demos:
    // progress is cleared every time the app launches, before any scene loads.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void PuliziaAutomaticaAlLancio()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
