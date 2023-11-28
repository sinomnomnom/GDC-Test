using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


public class InteractableObjectImageScript : MonoBehaviour
{
    public string imagePath;
  
    void Start()
    {
        initialScale = transform.localScale;
       

        
    }
    public float pulseDuration = 1f;
    public float maxScale = 1.5f;

    private Vector3 initialScale;
    private float timer;

    private void Update()
    {
        // Calculate the pulsing effect
        timer += Time.deltaTime;
        float t = Mathf.PingPong(timer / pulseDuration, 1f);
        float scale = Mathf.Lerp(1f, maxScale, t);

        // Apply the scale to the GameObject
        transform.localScale = initialScale * scale;
    }

}

