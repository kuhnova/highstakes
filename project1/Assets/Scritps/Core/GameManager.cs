using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private GameObject player;

    [SerializeField] private float reloadDelay = 2f;

    [SerializeField]private TextMeshProUGUI moneyText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        player = GameObject.FindGameObjectWithTag("Player");
        

    }

    private void LateUpdate()
    {
        if (moneyText != null && PlayerStats.Instance != null)
        {
            moneyText.text = $"$ {PlayerStats.Instance.Money}";
        }
    }   


    public void GameOver()
    {
        if (MessageUI.Instance != null)
            MessageUI.Instance.ShowMessage("Game", "Game Over");
        StartCoroutine(ReloadAfterDelay());
    }

    public void PlayAgain()
    {
        int currentMoney = PlayerStats.Instance.Money;
        PlayerStats.Instance.AddMoney(50 - currentMoney);
        int questIndex = QuestManager.Instance != null ? QuestManager.Instance.currentQuestIndex : 0;
        player.transform.position = Vector3.zero;
        HackingManager.Instance.CloseAll();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator ReloadAfterDelay()
    {
        yield return new WaitForSeconds(reloadDelay);
        player.transform.position = Vector3.zero;

        
    }
}