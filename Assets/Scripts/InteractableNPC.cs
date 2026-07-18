using UnityEngine;
using PrimeTween;

[RequireComponent(typeof(Collider2D))]
public class InteractableNPC : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Sürekli hareket edecek olan ünlem işareti objesi")]
    public Transform exclamationMark;
    [Tooltip("Oyuncu yaklaşınca belirecek olan E tuşu objesi")]
    public Transform interactPrompt;
    public DialogueController dialogueController;

    [Header("Ayarlar")]
    [TextArea(3, 5)]
    [Tooltip("Bu NPC ile konuşulacak metin")]
    public string npcDialogue = "Merhaba yabancı!";
    public float hoverDistance = 0.3f;
    public float hoverDuration = 1f;

    private bool isPlayerInRange = false;
    private bool isDialogueOpen = false;

    void Start()
    {
        // Oyun başladığında "E" tuşu görselini görünmez yap (Scale = 0)
        interactPrompt.localScale = Vector3.zero;

        // Ünlem işareti için PrimeTween ile sonsuz yukarı-aşağı (yoyo) animasyonu başlat
        if (exclamationMark != null)
        {
            Tween.LocalPositionY(exclamationMark, exclamationMark.localPosition.y + hoverDistance, hoverDuration, ease: Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);
        }
    }

    void Update()
    {
        // Oyuncu menzil içindeyse ve E tuşuna basarsa
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (isDialogueOpen)
            {
                dialogueController.CloseDialogue();
                isDialogueOpen = false;
            }
            else
            {
                dialogueController.OpenDialogue(npcDialogue);
                isDialogueOpen = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Oyuncu yanımıza geldiyse
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // Ünlem işaretini küçülterek yok et, E tuşunu tatlı bir efektle büyüt
            Tween.Scale(exclamationMark, Vector3.zero, 0.2f);
            Tween.Scale(interactPrompt, Vector3.one, 0.3f, ease: Ease.OutBack);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Oyuncu uzaklaştıysa
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // E tuşunu küçült, ünlem işaretini geri getir
            Tween.Scale(interactPrompt, Vector3.zero, 0.2f);
            Tween.Scale(exclamationMark, Vector3.one, 0.3f, ease: Ease.OutBack);

            // Eğer oyuncu diyalog açıkken uzaklaşırsa, diyaloğu zorla kapat
            if (isDialogueOpen)
            {
                dialogueController.CloseDialogue();
                isDialogueOpen = false;
            }
        }
    }
}