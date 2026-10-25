using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// NEW SCRIPT
// The nerd who tells snail facts. Put this on the "Nerd" object (with his Image and the
// speech bubble as children). Click him or the bubble to get a new fact.
// He says a new fact every time he appears (main menu, game over).
// While the text types out, he switches between his two frames so he looks like he's talking.
public class NerdFacts : MonoBehaviour, IPointerClickHandler
{
    [Header("Nerd")]
    public Image nerdImage;
    public Sprite idleFrame;   // frame 1: mouth closed
    public Sprite talkFrame;   // frame 2: mouth open

    [Header("Speech")]
    public TMP_Text factText;
    public float lettersPerSecond = 40f;
    [Tooltip("How many times per second he switches frames while talking.")]
    public float mouthSpeed = 8f;

    [TextArea(2, 4)]
    // CHANGED: your own facts. <b>...</b> = bold, <i>...</i> = italic (TextMeshPro rich text)
    public string[] facts =
    {
        "Snails have thousands of small teeth on their radula for cutting up food.",
        "Almost all snails are \"right handed\", meaning their shell curves the same way as your right hand.",
        "Most snails are hermaphrodites! They possess both male and female reproductive organs.",
        "There are species of snails found in almost all habitats! From deep ocean trenches to barren deserts. Even in Antarctica!",
        "There are over <b>40,000</b> species of snails!",
        "The smallest known snail is the sea snail <b><i>Ammonicera minortalis</i></b>, measuring just about 0.012 inches.",
        "The top speed of a common garden snail is about <b>0.03</b> miles per hour.",
        "The scientific name for a snail, <b><i>Gastropoda</i></b>, means \"Stomach foot\".",
        "The Scaly-foot Gastropod, or the <b><i>Volcano Snail</i></b>, lives 8,200 feet below sea level, and has an iron-plated shell layered with pyrite.",
        "The largest species of snail in the world is the giant African land snail. It can measure up to <b>15 inches</b> and weigh up to <b>2.2 lbs</b>!",
        "When snails hibernate, they seal themselves inside their shells with a mucus plug. They can stay there for <b>months</b> or even <b>years</b>!",
        "Snails are nocturnal, and usually move about at night.",
        "A typical snail sleep cycle lasts for <b>2-4 days</b>. Then they stay awake for a little over a day, before going back to sleep.",
        "Snails are not insects but mollusks, like slugs, clams and squid.",
        "Snails have no ears and are completely deaf.",
        "Snails can be herbivores, carnivores and omnivores. Some even eat decaying matter and other snails!",
        "Take me to snurch!",
    };

    int lastIndex = -1;
    Coroutine talking;

    void OnEnable()
    {
        ShowNewFact(); // a new fact every time he appears
    }

    // Clicking the nerd (or anything under him, like the speech bubble) gives a new fact.
    // Needs Raycast Target ticked on the Image you click.
    public void OnPointerClick(PointerEventData eventData)
    {
        ShowNewFact();
    }

    // Also public, so a Button's On Click () could call it if you prefer.
    public void ShowNewFact()
    {
        if (facts.Length == 0 || factText == null) return;

        // Pick a random fact, but not the same one twice in a row.
        int index = Random.Range(0, facts.Length);
        if (facts.Length > 1)
        {
            while (index == lastIndex) index = Random.Range(0, facts.Length);
        }
        lastIndex = index;

        if (talking != null) StopCoroutine(talking);
        talking = StartCoroutine(Talk(facts[index]));
    }

    // Types the fact out letter by letter while switching between the two frames.
    // Uses unscaled time, so it also works while the game is paused.
    IEnumerator Talk(string fact)
    {
        factText.text = fact;
        factText.maxVisibleCharacters = 0;

        // CHANGED: count only the letters you can see, so the <b> and <i> tags don't add extra typing time
        factText.ForceMeshUpdate();
        int totalLetters = factText.textInfo.characterCount;

        float lettersShown = 0f;
        float mouthTimer = 0f;
        bool mouthOpen = false;

        while (factText.maxVisibleCharacters < totalLetters) // CHANGED: was fact.Length
        {
            lettersShown += lettersPerSecond * Time.unscaledDeltaTime;
            factText.maxVisibleCharacters = Mathf.Min(totalLetters, (int)lettersShown); // CHANGED: was fact.Length

            mouthTimer += Time.unscaledDeltaTime;
            if (mouthTimer >= 1f / mouthSpeed)
            {
                mouthTimer = 0f;
                mouthOpen = !mouthOpen;
                if (nerdImage != null) nerdImage.sprite = mouthOpen ? talkFrame : idleFrame;
            }

            yield return null;
        }

        factText.maxVisibleCharacters = 99999; // show everything
        if (nerdImage != null) nerdImage.sprite = idleFrame;
        talking = null;
    }
}