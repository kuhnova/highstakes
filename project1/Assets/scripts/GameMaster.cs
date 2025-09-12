using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{


    [System.Serializable]
    public class PanelEntry
    {
        public string tag;
        public GameObject panel;
    }

    public List<PanelEntry> panels; // assign in inspector
    private Dictionary<string, GameObject> panelDict;

    // Start is called before the first frame update
    void Start()
    {
        // Build dictionary for quick lookup
        panelDict = new Dictionary<string, GameObject>();
        foreach (var entry in panels)
        {
            if (!panelDict.ContainsKey(entry.tag))
                panelDict.Add(entry.tag, entry.panel);
        }

        // Find all buttons with tags
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (Button btn in allButtons)
        {
            string t = btn.tag;
            if (panelDict.ContainsKey(t))
            {
                btn.onClick.AddListener(() => OpenPanel(t));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }

    public void OpenPanel(string tag)
    {

        // Show the one that matches
        if (panelDict.ContainsKey(tag))
            panelDict[tag].SetActive(true);
    }

    public void ClosePanel(string tag)
    {

        // Show the one that matches
        if (panelDict.ContainsKey(tag))
            panelDict[tag].SetActive(false);
    }


}
