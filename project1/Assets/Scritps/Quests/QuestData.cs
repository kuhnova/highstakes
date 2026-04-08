using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questID;
    public string questTitle;
    [TextArea]
    public string description;

    public int questOrder;

    public bool requiresAcquisition = false;
    public string acquiringNPCName = "Lily";
}