using UnityEngine;
using TMPro;

public class MessageUI : MonoBehaviour
{
    public static MessageUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI messageText;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowMessage(string name, string message)
    {
        panel.SetActive(true);
        nameText.text = name;
        messageText.text = message;

        Invoke(nameof(Hide), 2.5f);
    }

    private void Hide()
    {
        panel.SetActive(false);
    }
}