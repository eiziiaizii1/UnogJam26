using UnityEngine;
using TMPro;
using PrimeTween;

public class DialogueController : MonoBehaviour
{
    [Header("UI Referansları")]
    [Tooltip("Siyah panelin üzerinde bulunması gereken Canvas Group bileşeni")]
    public CanvasGroup panelCanvasGroup;
    public TMP_Text dialogueText;

    [Header("Ayarlar")]
    public float fadeDuration = 0.5f; // Panelin belirme/kaybolma hızı
    public float typeSpeed = 0.05f;   // Harf başına geçen süre (Daktilo hızı)

    [TextArea(3, 5)]
    public string defaultStoryText = "Buraya hikaye metni gelecek...";

    private bool isOpen = false;
    private Tween typingTween;

    void Start()
    {
        // Oyun başında paneli tamamen görünmez ve etkileşimsiz yap
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
        dialogueText.maxVisibleCharacters = 0;
    }

    

    /// <summary>
    /// Paneli açar ve metni yazmaya başlar. Modüler kullanım için dışarıdan metin alabilir.
    /// </summary>
    public void OpenDialogue(string textToDisplay)
    {
        if (isOpen) return;
        isOpen = true;

        panelCanvasGroup.interactable = true;
        panelCanvasGroup.blocksRaycasts = true;

        // Metni ata ve görünür harf sayısını sıfırla
        dialogueText.text = textToDisplay;
        dialogueText.maxVisibleCharacters = 0;

        // Olası çakışmaları önlemek için paneldeki eski animasyonları durdur
        Tween.StopAll(panelCanvasGroup);

        // Paneli Alpha 0'dan 1'e getir, bittiğinde daktilo efektini başlat
        Tween.Alpha(panelCanvasGroup, 1f, fadeDuration).OnComplete(() => StartTypewriter(textToDisplay));
    }

    private void StartTypewriter(string text)
    {
        int textLength = text.Length;
        float totalTypeTime = textLength * typeSpeed; // Toplam yazma süresini hesapla

        // Eski yazı animasyonu varsa durdur
        typingTween.Stop();

        // PrimeTween ile maxVisibleCharacters değerini 0'dan metin uzunluğuna kadar anime et
        // Daktilo efektinde hızın sabit olması için Ease.Linear kullanmak çok önemlidir!
        typingTween = Tween.Custom(0, textLength, totalTypeTime, onValueChange: val =>
        {
            dialogueText.maxVisibleCharacters = Mathf.RoundToInt(val);
        }, ease: Ease.Linear);
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

        // Paneli yavaşça karart
        Tween.Alpha(panelCanvasGroup, 0f, fadeDuration);
    }
}