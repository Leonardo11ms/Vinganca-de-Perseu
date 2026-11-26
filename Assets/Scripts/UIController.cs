
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class UiController : MonoBehaviour
{
    [Header("Menus")]
    public GameObject pauseMenu;
    public GameObject deathMenu;

    [Header("Backgrounds")]
    public GameObject pauseBackground;
    public GameObject morreuBackground;

    [Header("Player")]
    public Transform player;
    public float deathHeight = -10f;
    public MonoBehaviour playerController;
    public HeartSystem playerHealth;

    private bool paused = false;
    private bool dead = false;

    private void OnEnable()
    {
        DesativarMenusNoEditor();
    }

    private void OnValidate()
    {
        DesativarMenusNoEditor();
    }

    private void DesativarMenusNoEditor()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (pauseMenu != null)
                pauseMenu.SetActive(false);

            if (deathMenu != null)
                deathMenu.SetActive(false);

            if (pauseBackground != null)
                pauseBackground.SetActive(false);

            if (morreuBackground != null)
                morreuBackground.SetActive(false);
        }
#endif
    }

    private void Start()
    {
        paused = false;
        dead = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (deathMenu != null)
            deathMenu.SetActive(false);

        if (pauseBackground != null)
            pauseBackground.SetActive(false);

        if (morreuBackground != null)
            morreuBackground.SetActive(false);

        if (player != null)
        {
            if (playerController == null)
                playerController = player.GetComponent<NewMonoBehaviourScript>();

            if (playerHealth == null)
                playerHealth = player.GetComponent<HeartSystem>();

            if (playerHealth == null)
                playerHealth = player.GetComponentInChildren<HeartSystem>();
        }

        if (playerController != null)
            playerController.enabled = true;

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!Application.isPlaying || dead)
            return;

        if (player == null)
        {
            PlayerDie();
            return;
        }

        if (player.position.y < deathHeight)
        {
            PlayerDie();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
                HidePauseMenu();
            else
                ShowPauseMenu();
        }
       

    }

    public void ShowPauseMenu()
    {
        if (dead || paused)
            return;

        paused = true;

        if (playerController != null)
            playerController.enabled = false;

        if (deathMenu != null)
            deathMenu.SetActive(false);

        if (morreuBackground != null)
            morreuBackground.SetActive(false);

        if (pauseBackground != null)
            pauseBackground.SetActive(true);

        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void HidePauseMenu()
    {
        if (dead || !paused)
            return;

        paused = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (pauseBackground != null)
            pauseBackground.SetActive(false);

        if (morreuBackground != null)
            morreuBackground.SetActive(false);

        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }

    public void BTN_Resume()
    {
        HidePauseMenu();
    }

    public void PlayerDie()
    {
        if (dead)
            return;

        dead = true;
        paused = false;

        // Esvazia imediatamente todos os corações.
        if (playerHealth != null)
            playerHealth.MorrerPorQueda();

        if (playerController != null)
            playerController.enabled = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (pauseBackground != null)
            pauseBackground.SetActive(false);

        if (morreuBackground != null)
            morreuBackground.SetActive(true);

        if (deathMenu != null)
            deathMenu.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void BTN_Retry()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void BTN_Quit()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#endif
    }
}
