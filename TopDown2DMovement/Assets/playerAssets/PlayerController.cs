using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
   
    //movement
    public float moveSpeed = 1f;
    public ContactFilter2D movementFilter;
    Vector2 movementInput;
    Rigidbody2D rb;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    public float collisionOffset = 0.05f;

    Animator animator;
    
   
    //interact objects
    public LayerMask interactableObjectLayerMask;
    bool InteractObjectShown;

    //dialog
    bool inConversation;
    private bool canMove = true;
    [SerializeField] float interactDistance = 2;
    public LayerMask npcLayerMask;
    public DialogBoxController dialogBoxController;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // access rigidbody and animator components
        animator = GetComponent<Animator>();
        animator.SetFloat("faceY", -1);//start looking down
        inConversation = false;
        InteractObjectShown = false;
        dialogBoxController.InteractObjectShowing.AddListener(SetInteractShow);
        dialogBoxController.InteractObjectHidden.AddListener(SetInteractHide);
    }

    // Update is called once per frame
    void Update()
    {
        //who needs one of these
    }

    void OnMove(InputValue movementValue) //called whenever a "move" input is presed using the new unity input sytem
    {
        if (canMove)
        {
            movementInput = (movementValue.Get<Vector2>()).normalized;
            animator.SetFloat("moveX", movementInput.x);
            animator.SetFloat("moveY", movementInput.y);
            if (movementInput.x != 0 || movementInput.y != 0)
            {
                animator.SetFloat("faceX", movementInput.x);
                animator.SetFloat("faceY", movementInput.y);
            }
        }
        else
        {
            movementInput = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (movementInput != Vector2.zero) //if input check using trymove in direction of input if fail check in only x and then only y directions
        {
            movementInput = movementInput.normalized;//the player vector is normalized so that diagonals are not faster than cardinal directions
            bool success = TryMove(movementInput);
            if (!success)
            {
                if (movementInput.x != 0)
                {
                    success = TryMove(new Vector2(movementInput.x, 0));
                }
                if (!success)
                {
                    if (movementInput.y != 0)
                    {
                        success = TryMove(new Vector2(0, movementInput.y));
                    }
                }
            }
            //animator.SetBool("isMoving", success); uncomment for player to not animate when running at wall
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }
    private bool TryMove(Vector2 direction) //use rigidbody2d.cast to determine if colliders in direction of attempted move
    {
        int count = rb.Cast(direction, movementFilter, castCollisions, moveSpeed * Time.fixedDeltaTime * collisionOffset);
        if (count == 0)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            animator.SetBool("isMoving", true);
            return true;
        }
        else
        {
            //animator.SetBool("isMoving", false);
            return false;
        }
    }
    void OnInteract() //uses unity input system 
    {
        Interact();
        Debug.Log("interactcalled");
    }
   
    public void Interact()
    {
        if (inConversation)
        {
            DialogBoxController.Instance.SkipLine();
        }
        else
        {
            Debug.Log("interact object shown =" + InteractObjectShown);
            if (InteractObjectShown == true)
            {
                DialogBoxController.Instance.HideInteractObjectStart();
            }
            else
            {

                LayerMask npcLayerMask = LayerMask.GetMask("NPC");
                Collider2D[] colliderss = Physics2D.OverlapCircleAll(transform.position, interactDistance, npcLayerMask);
                if (colliderss.Length > 0)
                {
                    // Find the closest NPC
                    Collider2D closestNPC = colliderss[0];
                    float closestDistance = Vector3.Distance(transform.position, closestNPC.transform.position);

                    for (int i = 1; i < colliderss.Length; i++)
                    {
                        float distance = Vector3.Distance(transform.position, colliderss[i].transform.position);
                        if (distance < closestDistance)
                        {
                            closestNPC = colliderss[i];
                            closestDistance = distance;
                        }
                    }
                    NPC npc = closestNPC.GetComponent<NPC>();
                    DialogBoxController.Instance.StartDialog(npc.dialogTree, npc.startSection, npc.npcName);
                }
                else
                {
                    Debug.Log("noNpcHere");
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactDistance, interactableObjectLayerMask);
                    if (colliders.Length > 0)
                    {
                        // Find the closest interactable object
                        Collider2D closestInteractableObject = colliders[0];
                        float closestDistance = Vector3.Distance(transform.position, closestInteractableObject.transform.position);

                        for (int i = 1; i < colliderss.Length; i++)
                        {
                            float distance = Vector3.Distance(transform.position, colliderss[i].transform.position);
                            if (distance < closestDistance)
                            {
                                closestInteractableObject = colliderss[i];
                                closestDistance = distance;
                            }
                        }
                        InteractableObjectImageScript imageScript = closestInteractableObject.GetComponent<InteractableObjectImageScript>();

                        //interact script here

                        DialogBoxController.Instance.ShowObject(imageScript.imagePath);
                    }
                }
            }
        }

    }
    void JoinConversation()
    {
            inConversation = true;
    }
    void LeaveConversation()
    {
            inConversation = false;
    }
    private void OnEnable() //called when this object is enabled/disabled
    { 
            DialogBoxController.OnDialogStarted += JoinConversation;
            DialogBoxController.OnDialogEnded += LeaveConversation;
    }
    private void OnDisable()
    {
            DialogBoxController.OnDialogStarted -= JoinConversation;
            DialogBoxController.OnDialogEnded -= LeaveConversation;
    }
    // Call this method when you want to prevent the player from moving
    public void DisablePlayerMovement()
    {
        canMove = false;
    }

    // Call this method when you want to allow the player to move again
    public void EnablePlayerMovement()
    {
        canMove = true;
    }
    void SetInteractShow()
    {
        InteractObjectShown = true;
        Debug.Log("huh");
    }
    void SetInteractHide()
    {
        InteractObjectShown = false;
        Debug.Log("what???");//dont mind these
    }
} 
