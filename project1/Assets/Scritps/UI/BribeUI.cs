using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BribeUI : MonoBehaviour
{
    public static BribeUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button payButton;
    [SerializeField] private Button refuseButton;

    private NPCPatrol currentNpc;
    private int currentAmount;

    private void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);

        if (payButton != null) payButton.onClick.AddListener(OnPay);
        if (refuseButton != null) refuseButton.onClick.AddListener(OnRefuse);
    }

    public void Show(NPCPatrol npc, int amount)
    {
        if (panel == null) return;
        currentNpc = npc;
        currentAmount = amount;
        if (messageText != null)
            messageText.text = $"Pay ${amount} to avoid consequences?";
        panel.SetActive(true);
    }

    private void Close()
    {
        if (panel != null) panel.SetActive(false);
        currentNpc = null;
    }

    private void OnPay()
    {
        
        var npc = currentNpc;

        if (PlayerStats.Instance != null && PlayerStats.Instance.TrySpend(currentAmount))
        {
            
            npc?.OnBribeAccepted();

            if (MessageUI.Instance != null)
                MessageUI.Instance.ShowMessage("Bribe", $"You paid ${currentAmount}.");

            Close();
        }
        else
        {
            
            npc?.OnBribeRefused();

            if (MessageUI.Instance != null)
                MessageUI.Instance.ShowMessage("Bribe", "You don't have enough money!");

            Close();
        }
    }

    private void OnRefuse()
    {
        var npc = currentNpc;
        npc?.OnBribeRefused();
        Close();
    }
}