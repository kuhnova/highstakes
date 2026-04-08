using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PatternPuzzle : MonoBehaviour
{
    [SerializeField] private List<Image> cells;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite failedSprite;

    [Header("Timing")]
    [SerializeField] private float displayTime = 0.5f; 
    [SerializeField] private float betweenCellsDelay = 0.2f;
    [SerializeField] private float wrongDisplayDuration = 1.5f; 
    [SerializeField] private float successDisplayDuration = 1.5f; 

    private List<int> pattern = new List<int>();
    private List<int> playerInput = new List<int>();
    private bool canInput = false;


    public void DelayedStart(int number)
    {
        GeneratePattern(number);
        StartCoroutine(ShowPattern());
    }

    private void GeneratePattern(int number)
    {
        pattern.Clear();

        for (int i = 0; i < number; i++)
        {
            pattern.Add(Random.Range(0, cells.Count));
        }
    }

    private IEnumerator ShowPattern()
    {
        canInput = false;

        for(int i = 0; i < cells.Count; i++)
        {
            cells[i].sprite = offSprite;
        }

        foreach (int index in pattern)
        {
            cells[index].sprite = activeSprite;
            yield return new WaitForSeconds(displayTime);
            cells[index].sprite = offSprite;
            yield return new WaitForSeconds(betweenCellsDelay);
        }

        canInput = true;
    }

    public void OnCellClicked(int index)
    {
        if (!canInput) return;

        playerInput.Add(index);

        if (playerInput.Count == pattern.Count)
        {
            
            StartCoroutine(CheckPatternCoroutine());
        }
    }

    private IEnumerator CheckPatternCoroutine()
    {
        canInput = false;

        
        for (int i = 0; i < pattern.Count; i++)
        {
            if (pattern[i] != playerInput[i])
            {
                Debug.Log("Pattern failed");

               
                foreach (Image img in cells)
                {
                    img.sprite = failedSprite;
                }

                yield return new WaitForSeconds(wrongDisplayDuration);

                
                foreach (Image img in cells)
                {
                    img.sprite = offSprite;
                }

                playerInput.Clear();

               
                StartCoroutine(ShowPattern());
                yield break;
            }
        }

       
        foreach (Image img in cells)
        {
            img.sprite = activeSprite;
        }

        Debug.Log("Pattern success");
        playerInput.Clear();

        
        yield return new WaitForSeconds(successDisplayDuration);    

        if (HackingManager.Instance != null)
            HackingManager.Instance.NotifyHackSuccess();
    }
}