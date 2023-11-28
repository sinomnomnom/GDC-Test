using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class DialogTree : ScriptableObject
{
    [System.Serializable]
    
    //stores all the data for the dialog of the npc it is attached to goes in the npc script
    public struct DialogSection
    {
        [TextArea]
        public string[] dialog;
        public bool endAfterDialog;
        public BranchPoint branchPoint;
    }
    [System.Serializable]
    public struct BranchPoint
    {
        [TextArea]
        public string question;
        public Answer[] answers;
    }
    [System.Serializable]
    public struct Answer
    {
        public string answerLabel;
        public int nextElement;
    }
    public DialogSection[] sections;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
