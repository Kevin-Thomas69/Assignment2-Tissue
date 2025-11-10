using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/DialogueSequence")]
public class Dialogue : ScriptableObject
{
    [Header("Speaker Name (optional)")]
    public string speakerName = "NPC";

    [Header("Dialogue Lines")]
    [TextArea(3, 5)]
    public List<string> dialogueLines = new List<string>();
}