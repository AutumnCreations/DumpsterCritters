using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public TMP_Text dialogueText;
    public GameObject dialogueUI;
    public Button continueButton;
    private Dialogue currentDialogue;
    private int currentLineIndex;

    private void Awake()
    {
        dialogueUI.SetActive(false);
    }
    private void Start()
    {
        continueButton.onClick.AddListener(() => ContinueDialogue());
    }

    public void StartDialogue(Dialogue dialogue)
    {
        GameStateManager.Instance.ChangeState(GameStateManager.GameState.Dialogue);
        currentDialogue = dialogue;
        currentLineIndex = 0;
        dialogueUI.SetActive(true);
        ShowLine();
    }

    public void ContinueDialogue()
    {
        if (currentLineIndex < currentDialogue.lines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowLine()
    {
        dialogueText.text = currentDialogue.lines[currentLineIndex];
        if (currentLineIndex == currentDialogue.shopLineIndex)
        {
            // Open the shop UI when the shop line index is reached
            dialogueUI.SetActive(false);
            ShopSystem shopSystem = GetComponent<ShopSystem>();
            shopSystem.OpenShop();
        }
        currentLineIndex++;
    }

    private void EndDialogue()
    {
        GameStateManager.Instance.ChangeState(GameStateManager.GameState.Playing);
        dialogueUI.SetActive(false);
    }
}
