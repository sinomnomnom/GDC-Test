using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] bool firstInteraction = true;
    [SerializeField] int repeatStartPosition; //deides where to start convo if you have spoken with them before
    public string npcName; //name
    public DialogTree dialogTree; //dialog asset
    [HideInInspector]
    public int startSection //changes what value is returned based on other data can be read when raycast
    {
        get
        {
            if (firstInteraction)
            {
                firstInteraction = false;
                return 0;
            } 
            else
            {
                return repeatStartPosition;
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
