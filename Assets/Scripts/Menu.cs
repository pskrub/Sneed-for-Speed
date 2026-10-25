using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // NEW: for Image

public class Menu : MonoBehaviour
{
    public Animator snailAnimator;  // NEW: the menu snail's Animator (trigger "Start")
    public float startDelay = 1.5f; // NEW: seconds to wait for the start animation before loading

    public AudioSource menuAudio;   // NEW (sprint sound): an AudioSource to play it on (your menu music one works too)
    public AudioClip sprintSound;   // NEW (sprint sound): plays when the snail sprints away

    // CHANGED: the How to Play transition is now played by this script instead of an Animator,
    // so it can easily go forwards AND backwards.
    public Image transitionImage;           // NEW: the Transition object's Image
    public Sprite[] transitionFrames;       // NEW: your transition images, in order
    public float framesPerSecond = 12f;     // NEW: animation speed

    public GameObject menuButtons;          // NEW: StandardMenuButts (hidden while How to Play is open)
    public GameObject backButton;           // NEW: BackButtonHowToPlay (only shown while How to Play is open)
    public GameObject howToPlayPanel;       // NEW: optional extra How to Play image. Leave empty if the last frame is the overlay

    bool starting;                  // NEW: stops double clicks on Start
    bool busy;                      // NEW: true while the How to Play animation plays (forward or back)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // NEW: everything for How to Play hidden at the start
        if (transitionImage != null) transitionImage.enabled = false;
        if (backButton != null) backButton.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onClickStartGameButton()
    {
        // CHANGED: was SceneManager.LoadScene(1); right away.
        // Now it plays the animation first, then loads the game.
        if (starting || busy) return;
        starting = true;
        if (snailAnimator != null) snailAnimator.SetTrigger("Start");
        if (menuAudio != null && sprintSound != null) menuAudio.PlayOneShot(sprintSound); // NEW (sprint sound)
        StartCoroutine(LoadGameLater());
    }

    // NEW: waits for the animation, then loads scene 1 like before
    IEnumerator LoadGameLater()
    {
        yield return new WaitForSeconds(startDelay);
        SceneManager.LoadScene(1);
    }

    // NEW: hook the How to Play button's On Click () up to this (and nothing else)
    public void onClickHowToPlayButton()
    {
        if (starting || busy) return;
        StartCoroutine(OpenHowToPlay());
    }

    // NEW: hook the Back button's On Click () up to this (and nothing else)
    public void onClickBackButton()
    {
        if (busy) return;
        StartCoroutine(CloseHowToPlay());
    }

    // CHANGED: replaces ShowHowToPlayLater. Plays the frames forwards, then shows the overlay.
    IEnumerator OpenHowToPlay()
    {
        busy = true;
        if (menuButtons != null) menuButtons.SetActive(false);

        yield return PlayFrames(false);

        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
        if (backButton != null) backButton.SetActive(true);
        busy = false;
    }

    // CHANGED: replaces BackLater. Hides the overlay, plays the frames backwards, then shows the menu again.
    IEnumerator CloseHowToPlay()
    {
        busy = true;
        if (backButton != null) backButton.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);

        yield return PlayFrames(true);

        if (transitionImage != null) transitionImage.enabled = false;
        if (menuButtons != null) menuButtons.SetActive(true);
        busy = false;
    }

    // NEW: shows the frames one by one. reverse = true plays them from last to first.
    // After playing forwards, the last frame stays on screen.
    IEnumerator PlayFrames(bool reverse)
    {
        if (transitionImage == null || transitionFrames.Length == 0) yield break;

        transitionImage.enabled = true;
        for (int i = 0; i < transitionFrames.Length; i++)
        {
            int frame = reverse ? transitionFrames.Length - 1 - i : i;
            transitionImage.sprite = transitionFrames[frame];
            yield return new WaitForSeconds(1f / framesPerSecond);
        }
    }

    public void OnClickQuitButton()
    {
        // Debug.Log("swag");
        Application.Quit();
    }

}