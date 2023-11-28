using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Events;

public class DialogBoxController : MonoBehaviour
{
    public static DialogBoxController Instance;
    [SerializeField] TextMeshProUGUI dialogText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] GameObject dialogPanel;
    [SerializeField] GameObject answerBox;
    [SerializeField] Button[] answerObjects;
    [SerializeField] CanvasGroup dialogcanvasGroup;
    [SerializeField] CanvasGroup interactableObjectcanvasGroup;
    [SerializeField] GameObject ObjectUI;
    public static event Action OnDialogStarted;
    public static event Action OnDialogEnded;
    bool skipLineTriggered;
    public bool answerTriggered;
    int answerIndex;
    public float charactersPerSecond = 5;
    bool isTyping;
    public float targetAlpha = 1f;
    public float lerpDuration = .5f;
    public Image imageUI;
    public PlayerController playerController;
    public UnityEvent InteractObjectShowing;
    public UnityEvent InteractObjectHidden;

    public RectTransform imageRectTransform;
    // Start is called before the first frame update
    void Start()
    {
        ObjectUI.SetActive(false);
        dialogPanel.SetActive(false);
        nameText.text = "";
        dialogText.text = "";
        dialogcanvasGroup.alpha = 0;
        isTyping = true;
        PlayerController playerController = FindObjectOfType<PlayerController>();
        RectTransform imageRectTransform = imageUI.GetComponent<RectTransform>();

}

    // Update is called once per frame
    void Update()
    {
        
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    public void StartDialog(DialogTree dialogTree, int startSection, string name)
    {
        ResetBox();
        nameText.text = name + "...";
        dialogPanel.SetActive(true);
        OnDialogStarted?.Invoke();
        StopAllCoroutines();
        StartCoroutine(LerpCanvasGroupAlpha(1f,dialogcanvasGroup));
        StartCoroutine(RunDialog(dialogTree, startSection));
        PlayerController playerController = FindObjectOfType<PlayerController>();
        playerController.DisablePlayerMovement();
    }
    IEnumerator RunDialog(DialogTree dialogTree, int section)
    {
        skipLineTriggered = false;
        OnDialogStarted?.Invoke();
        for (int i = 0; i < dialogTree.sections[section].dialog.Length; i++)
        {
            StartCoroutine(TypeText(dialogTree.sections[section].dialog[i]));
            //dialogText.text = dialog[i];
            while (skipLineTriggered == false)
            {
                yield return null;
            }
            skipLineTriggered = false;
        }
        if (dialogTree.sections[section].endAfterDialog)
        {
            OnDialogEnded?.Invoke();
            playerController.EnablePlayerMovement();
            yield return StartCoroutine(LerpCanvasGroupAlpha(0f, dialogcanvasGroup));
            yield return new WaitForSeconds(lerpDuration);
            dialogPanel.SetActive(false);
            yield break;
        }
        StartCoroutine(TypeText(dialogTree.sections[section].branchPoint.question));
        ShowAnswers(dialogTree.sections[section].branchPoint);
        while (answerTriggered == false) { yield return null; }
        Debug.Log("answerrecieved!");
        answerBox.SetActive(false);
        answerTriggered = false;
        isTyping = true;
        StartCoroutine(RunDialog(dialogTree, dialogTree.sections[section].branchPoint.answers[answerIndex].nextElement));
    }

    void ResetBox()
    {
        StopAllCoroutines();
        dialogPanel.SetActive(false);
        answerBox.SetActive(false);
        skipLineTriggered = false;
        answerTriggered = false;
        playerController.EnablePlayerMovement();
    }
    void ShowAnswers(DialogTree.BranchPoint branchPoint)
    {
        answerBox.SetActive(true);
        for (int i = 0; i < 3; i++)
        {
            if (i < branchPoint.answers.Length)
            {
                answerObjects[i].GetComponentInChildren<TextMeshProUGUI>().text = branchPoint.answers[i].answerLabel;
                answerObjects[i].gameObject.SetActive(true);
            }
            else
            {
                answerObjects[i].gameObject.SetActive(false);
            }
        }
    }
    public void AnswerQuestion(int answer)
    {
        answerIndex = answer;
        answerTriggered = true;
        Debug.Log("answerrecieved?");
    }
    IEnumerator TypeText(string line)
    {
        string textBuffer = null;
        foreach (char c in line)
        {
            if (isTyping == true)
            { 
                textBuffer += c;
                dialogText.text = textBuffer;
                if (c == ',')
                {
                    yield return new WaitForSeconds(7/charactersPerSecond);
                }
                else
                {
                    if (c == '.' | c=='?' | c=='!' | c==';') { yield return new WaitForSeconds(10/ charactersPerSecond); }
                    else
                    {
                        yield return new WaitForSeconds(1 / charactersPerSecond);
                    }
                } 
            }
            else
            {
                dialogText.text = line;
                break;
            }
           
            
        }
        isTyping = false;
    }
    public void SkipLine() 
    { 
        if (isTyping == false)
        {
            skipLineTriggered = true;
            isTyping=true;
        }
        else
        {
            isTyping = false;
        }
    }
    IEnumerator LerpCanvasGroupAlpha(float targetAlpha,CanvasGroup canvasGroup)
    {
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        while (elapsedTime < lerpDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / lerpDuration);
            elapsedTime += Time.deltaTime;

            yield return null; // Wait for the next frame
        }

        // Ensure the alpha reaches the target exactly
        canvasGroup.alpha = targetAlpha;
    }
    IEnumerator LerpSize(Vector2 targetSize, float speed)
    {
        // Ensure that the RectTransform component is assigned
        if (imageRectTransform != null)
        {
            // Lerp the size towards the target size
            float elapsedTime = 0f;
            Vector2 currentSize = imageRectTransform.sizeDelta;
            while (elapsedTime < speed)
            {
                Vector2 newSize = Vector2.Lerp(currentSize, targetSize, elapsedTime / speed);
                elapsedTime += Time.deltaTime;
                imageRectTransform.sizeDelta = newSize;
                yield return null;
            }

            // Set the new size to the RectTransform

            yield break;
        }
        else
        {
            Debug.LogWarning("Image RectTransform not assigned. Please assign it in the Unity Editor.");
            yield break;
        }
    }
        public void ShowObject(string path)
   {
        // Load the image file
        Texture2D texture = LoadImageFromFile(path);
        SetImage(texture);
        ResetBox();
        nameText.text = "";
        dialogPanel.SetActive(false);
        StopAllCoroutines();
        ObjectUI.SetActive(true);
        playerController.DisablePlayerMovement();
        InteractObjectShowing.Invoke();
        StartCoroutine(LerpSize(new Vector2(800,800),.75f));
        StartCoroutine(LerpCanvasGroupAlpha(1f, interactableObjectcanvasGroup));
        
    }
    Texture2D LoadImageFromFile(string path)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }
    public void SetImage(Texture2D texture)
    {
        if (imageUI != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            imageUI.sprite = sprite;
        }
        else
        {
            Debug.LogError("Image UI reference not set in the UIManager script.");
        }
    }
    IEnumerator HideInteractObject()
    {
        StartCoroutine(LerpSize(new Vector2(400, 400),.75f));
        InteractObjectHidden.Invoke();
        StartCoroutine(LerpCanvasGroupAlpha(0f, interactableObjectcanvasGroup));
        playerController.EnablePlayerMovement();
        yield return new WaitForSeconds(lerpDuration);
        ObjectUI.SetActive(false);
       
    }
    public void HideInteractObjectStart()
    {
        StartCoroutine(HideInteractObject());
    }
}
