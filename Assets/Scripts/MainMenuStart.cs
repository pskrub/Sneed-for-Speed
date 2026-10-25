using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// NEW SCRIPT
// For the main menu. Hook the Start button's On Click () up to StartGame.
// It plays an animation on the snail, waits, and then loads the game scene.
public class MainMenuStart : MonoBehaviour
{
    [Tooltip("The Animator on the menu snail.")]
    public Animator snailAnimator;

    [Tooltip("Name of the Trigger parameter in the snail's Animator that starts the animation.")]
    public string triggerName = "Start";

    [Tooltip("Exact name of the game scene (must be in File > Build Profiles > Scene List).")]
    public string gameSceneName = "Game";

    [Tooltip("Seconds to wait before the game loads. Set it to the length of the animation.")]
    public float delay = 1.5f;

    bool starting;

    public void StartGame()
    {
        if (starting) return; // ignore extra clicks while the animation plays
        starting = true;

        if (snailAnimator != null) snailAnimator.SetTrigger(triggerName);
        StartCoroutine(LoadAfterDelay());
    }

    IEnumerator LoadAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(gameSceneName);
    }
}