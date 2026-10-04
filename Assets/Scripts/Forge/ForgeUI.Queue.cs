using UnityEngine;
using UnityEngine.UI;

// Pile des pièces forgées (façon Forge Master) : une vignette sur l'enclume montre la prochaine pièce et le nombre en attente.
// La toucher ouvre la fiche ; équiper/vendre passe automatiquement à la suivante.
public partial class ForgeUI
{
    Button queueBtn;
    Image queueFrame;
    RawImage queueIcon;
    Text queueCount;
    float queuePulse;

    void BuildQueueBadge(RectTransform anvil)
    {
        var rt = MakeRect("Pile de pièces", anvil, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-150, -150), new Vector2(-8, -8));
        queueFrame = rt.gameObject.AddComponent<Image>();
        queueFrame.sprite = Round(); queueFrame.type = Image.Type.Sliced;
        queueBtn = rt.gameObject.AddComponent<Button>();
        queueBtn.onClick.AddListener(Sfx.Click);
        queueBtn.onClick.AddListener(ReviewNext);
        var inner = MakeRect("Fond", rt, Vector2.zero, Vector2.one, new Vector2(5, 5), new Vector2(-5, -5)).gameObject.AddComponent<Image>();
        inner.sprite = Round(); inner.type = Image.Type.Sliced; inner.color = new Color(0.08f, 0.04f, 0.04f, 0.95f); inner.raycastTarget = false;
        queueIcon = MakeRect("Icône", rt, Vector2.zero, Vector2.one, new Vector2(10, 18), new Vector2(-10, -6)).gameObject.AddComponent<RawImage>();
        queueIcon.raycastTarget = false;
        queueCount = Label(rt, "", 30, TextAnchor.LowerRight, Color.white, Vector2.zero, Vector2.one, new Vector2(6, 4), new Vector2(-10, 0));
        queueCount.fontStyle = FontStyle.Bold;
        rt.gameObject.SetActive(false);
    }

    void RefreshQueueBadge()
    {
        if (queueBtn == null) return;
        var d = GameState.Data;
        int n = d.forged.Count;
        bool show = n > 0 && !popup.activeSelf;
        queueBtn.gameObject.SetActive(show);
        if (!show) return;
        var next = d.forged[0];
        var c = GameState.CircleColors[next.circle];
        queueFrame.color = c;
        queueIcon.texture = ItemIcons.Get(next.slot, next.circle);
        queueCount.text = "x" + n;
    }

    // Petit rebond de la vignette à chaque pièce forgée.
    void UpdateQueueBadge()
    {
        if (queueBtn == null || !queueBtn.gameObject.activeSelf) return;
        if (queuePulse > 0f) queuePulse = Mathf.Max(0f, queuePulse - Time.deltaTime * 4f);
        queueBtn.transform.localScale = Vector3.one * (1f + 0.25f * queuePulse);
    }
}
