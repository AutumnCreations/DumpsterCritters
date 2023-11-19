using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    TMP_Text dialogueText;
    [SerializeField]
    GameObject dialogueUI;
    [SerializeField]
    Button continueButton;
    [SerializeField, Range(0.01f, 2f)]
    float textSpeed = 0.05f;

    NPC currentNPC;
    Dialogue currentDialogue;
    int currentLineIndex;
    Coroutine typewriter;
    bool isTypewriterEffectRunning = false;

    public GameObject Music;
    private MusicPlayer musicPlayer;

    private void Awake()
    {
        dialogueUI.SetActive(false);
        musicPlayer = Music.GetComponent<MusicPlayer>();
    }
    private void Start()
    {
        continueButton.onClick.AddListener(() => ContinueDialogue());
        StartTutorialDialogue();
    }

    public void StartDialogue(NPC npc)
    {
        GameStateManager.Instance.ChangeState(GameStateManager.GameState.Dialogue);
        currentNPC = npc;
        currentDialogue = npc.GetDialogue();
        currentLineIndex = 0;
        dialogueUI.SetActive(true);
        ShowLine();

        ShopSystem shopSystem = GetComponent<ShopSystem>();
        shopSystem.OnShopClosed += ContinueDialogueAfterShop;
    }

    public void StartTutorialDialogue()
    {
        currentNPC = FindObjectOfType<NPC>();
        GameStateManager.Instance.ChangeState(GameStateManager.GameState.Tutorial);
        currentDialogue = currentNPC.GetDialogue(false);
        currentLineIndex = 0;
        dialogueUI.SetActive(true);
        ShowLine();
    }

    private void ShowLine()
    {
        if (currentDialogue.lines[currentLineIndex].ToLower() == "open shop")
        {
            // Open the shop UI when the shop line index is reached
            dialogueUI.SetActive(false);
            ShopSystem shopSystem = GetComponent<ShopSystem>();
            shopSystem.OpenShop(currentNPC);
        }
        else
        {
            StopAllCoroutines();
            typewriter = StartCoroutine(ShowTextWithTypewriterEffect(currentDialogue.lines[currentLineIndex], textSpeed));
        }
        currentLineIndex++;
    }

    public void ContinueDialogue()
    {
        if (isTypewriterEffectRunning)
        {
            // Set flag to stop the coroutine loop
            isTypewriterEffectRunning = false;
            // Stop the current coroutine
            StopCoroutine(typewriter);
            // Show the full text of the current line
            dialogueText.text = currentDialogue.lines[currentLineIndex - 1];
            return;
        }

        if (currentLineIndex < currentDialogue.lines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void ContinueDialogueAfterShop()
    {
        GameStateManager.Instance.ChangeState(GameStateManager.GameState.Dialogue);
        dialogueUI.SetActive(true);
        // Unsubscribe to avoid repeated calls
        ShopSystem shopSystem = GetComponent<ShopSystem>();
        shopSystem.OnShopClosed -= ContinueDialogueAfterShop;
        musicPlayer.PlayTrack(2);

        if (currentLineIndex < currentDialogue.lines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        GameStateManager.Instance.ChangeState(GameStateManager.GameState.Playing);
        dialogueUI.SetActive(false);
    }

    private IEnumerator ShowTextWithTypewriterEffect(string text, float speed)
    {
        isTypewriterEffectRunning = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(speed);

            // Check if player clicked through dialogue
            if (!isTypewriterEffectRunning)
            {
                // Display full text
                dialogueText.text = text;
                break;
            }
        }

        isTypewriterEffectRunning = false;
    }
}
