
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HackingSequence : MonoBehaviour
{
    [SerializeField] private List<int> correctSequence = new List<int> { 1, 2, 3 };
    private List<int> playerSequence = new List<int>();


    [SerializeField] private List<Image> buttonImages;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failedSprite;

    [SerializeField] private float wrongDisplayDuration = 1.5f;
    [SerializeField] private float successDisplayDuration = 1.5f;

    [Header("Player input display")]
    [SerializeField] private TMP_Text inputDisplayText; 
    [SerializeField] private bool displayOneBased = true;


    private bool canInput = true;

    public void PressButton(int id)
    {
        if (!canInput) return;

        playerSequence.Add(id);
        AppendToInputDisplay(id);

        if (playerSequence.Count == correctSequence.Count)
        {
            
            StartCoroutine(CheckSequence());
        }
    }

    private IEnumerator CheckSequence()
    {
        canInput = false;

        for (int i = 0; i < correctSequence.Count; i++)
        {
            if (playerSequence[i] != correctSequence[i])
            {

                SetAllButtonsSprite(failedSprite);

                yield return new WaitForSeconds(wrongDisplayDuration);

                SetAllButtonsSprite(offSprite);
                playerSequence.Clear();
                ClearInputDisplay();
                canInput = true;
                yield break;

            }
        }

        SetAllButtonsSprite(successSprite);

        Debug.Log("Hack successful");
        playerSequence.Clear();

        yield return new WaitForSeconds(successDisplayDuration);

        ClearInputDisplay();

        if (HackingManager.Instance != null)
            HackingManager.Instance.NotifyHackSuccess();
    }

    private void SetAllButtonsSprite(Sprite s)
    {
        if (buttonImages == null) return;

        for (int i = 0; i < buttonImages.Count; i++)
        {
            if (buttonImages[i] != null)
                buttonImages[i].sprite = s;
        }
    }


    private void AppendToInputDisplay(int id)
    {
        if (inputDisplayText == null) return;

        int valueToShow = displayOneBased ? id : id - 1;
        inputDisplayText.text += valueToShow.ToString();
    }

    private void ClearInputDisplay()
    {
        if (inputDisplayText == null) return;
        inputDisplayText.text = string.Empty;
    }

    
    public void SetCorrectSequence(List<int> sequence)
    {
        if (sequence == null || sequence.Count == 0) return;
        correctSequence = new List<int>(sequence);
        playerSequence.Clear();
        ClearInputDisplay();
    }
}