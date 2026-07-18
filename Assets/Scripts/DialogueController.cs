using UnityEngine;
using TMPro;
using PrimeTween;
using Game.Runtime.Input;

public class DialogueController : MonoBehaviour
{
    [Header("UI Referansları")]
    [Tooltip("Siyah panelin üzerinde bulunması gereken Canvas Group bileşeni")]
    public CanvasGroup panelCanvasGroup;
    public TMP_Text dialogueText;

    [Header("Ayarlar")]
    public float fadeDuration = 0.3f; // Panelin belirme/kaybolma hızı (daha hızlı tepki için 0.3f yapıldı)
    public float typeSpeed = 0.03f;   // Harf başına geçen süre (Daktilo hızı biraz hızlandırıldı)

    [TextArea(3, 5)]
    public string defaultStoryText = "Buraya hikaye metni gelecek...";

    [Tooltip("Sırayla gösterilecek diyalog metinleri. Boşsa defaultStoryText kullanılır.")]
    public string[] dialogueLines;

    public bool IsOpen => isOpen;

    private bool isOpen = false;
    private Tween typingTween;
    private int currentLineIndex = 0;
    private string[] activeLines;
    private InputReader playerInput;
    private bool skipFrameInput = false;

    void Start()
    {
        // Oyun başında paneli tamamen görünmez ve etkileşimsiz yap
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.transform.localScale = Vector3.zero;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
        dialogueText.maxVisibleCharacters = 0;
    }

    void Update()
    {
        if (!isOpen)
        {
            // Eğer kapalıysa ve E tuşuna basılırsa diyaloğu başlat
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (dialogueLines != null && dialogueLines.Length > 0)
                {
                    OpenDialogue(dialogueLines);
                }
                else
                {
                    OpenDialogue(new string[] { defaultStoryText });
                }
            }
            return;
        }

        // Açık olduğunda, ilk karedeki E basışını atla (çakışmaları önlemek için)
        if (skipFrameInput)
        {
            skipFrameInput = false;
            return;
        }

        // Açıkken E tuşuna basılırsa
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (typingTween.isAlive)
            {
                // Yazı yazılıyorsa yazıyı hemen tamamla
                CompleteTypewriter();
            }
            else
            {
                // Yazı bittiyse sonraki satıra geç veya kapat
                DisplayNextLine();
            }
        }
    }

    /// <summary>
    /// Paneli açar ve tek bir metni yazar.
    /// </summary>
    public void OpenDialogue(string textToDisplay)
    {
        OpenDialogue(new string[] { textToDisplay });
    }

    /// <summary>
    /// Paneli açar ve sıralı metin dizisini yazmaya başlar.
    /// </summary>
    public void OpenDialogue(string[] lines)
    {
        if (isOpen) return;
        isOpen = true;
        skipFrameInput = true;

        activeLines = lines;
        currentLineIndex = 0;

        panelCanvasGroup.interactable = true;
        panelCanvasGroup.blocksRaycasts = true;

        // Oyuncu girdisini durdur
        FindAndTogglePlayerInput(false);

        // Olası çakışmaları önlemek için paneldeki eski animasyonları durdur
        Tween.StopAll(panelCanvasGroup);
        Tween.StopAll(panelCanvasGroup.transform);

        // Panel görünürlüğünü Alpha ile, ölçeğini Scale ile tatlıca büyüt (pop-in)
        panelCanvasGroup.transform.localScale = Vector3.zero;
        Tween.Alpha(panelCanvasGroup, 1f, fadeDuration);
        Tween.Scale(panelCanvasGroup.transform, Vector3.one, fadeDuration, ease: Ease.OutBack)
            .OnComplete(() => DisplayLine(activeLines[currentLineIndex]));
    }

    private void DisplayLine(string textToDisplay)
    {
        dialogueText.text = textToDisplay;
        dialogueText.maxVisibleCharacters = 0;
        StartTypewriter(textToDisplay);
    }

    private void DisplayNextLine()
    {
        currentLineIndex++;
        if (currentLineIndex < activeLines.Length)
        {
            DisplayLine(activeLines[currentLineIndex]);
        }
        else
        {
            CloseDialogue();
        }
    }

    private void StartTypewriter(string text)
    {
        int textLength = text.Length;
        float totalTypeTime = textLength * typeSpeed;

        // Eski yazı animasyonu varsa durdur
        typingTween.Stop();

        // PrimeTween ile maxVisibleCharacters değerini 0'dan metin uzunluğuna kadar anime et
        typingTween = Tween.Custom(0, textLength, totalTypeTime, onValueChange: val =>
        {
            dialogueText.maxVisibleCharacters = Mathf.RoundToInt(val);
        }, ease: Ease.Linear);
    }

    private void CompleteTypewriter()
    {
        if (typingTween.isAlive)
        {
            typingTween.Stop();
            dialogueText.maxVisibleCharacters = dialogueText.text.Length;
        }
    }

    /// <summary>
    /// Paneli kapatır ve animasyonları sıfırlar.
    /// </summary>
    public void CloseDialogue()
    {
        if (!isOpen) return;
        isOpen = false;

        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;

        // Yazı yazma işlemi devam ediyorsa durdur
        typingTween.Stop();
        Tween.StopAll(panelCanvasGroup);
        Tween.StopAll(panelCanvasGroup.transform);

        // Paneli yavaşça karart ve küçülterek yok et (pop-out)
        Tween.Alpha(panelCanvasGroup, 0f, fadeDuration);
        Tween.Scale(panelCanvasGroup.transform, Vector3.zero, fadeDuration, ease: Ease.InBack);

        // Oyuncu girdisini geri aktif et
        FindAndTogglePlayerInput(true);
    }

    private void FindAndTogglePlayerInput(bool enable)
    {
        if (playerInput == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerInput = player.GetComponent<InputReader>();
            }
        }

        if (playerInput != null)
        {
            playerInput.enabled = enable;
            
            // Eğer oyuncunun InputReader bileşeni kapatılıyorsa, mevcut girdi değerlerini sıfırladığından emin ol
            if (!enable)
            {
                // InputReader deaktif olunca motor ve shooter değerlerini sıfırlayacaktır
                // Ama ekstra güvenlik önlemi olarak burası tetiklendi.
            }
        }
    }
}