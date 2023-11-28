using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npcmovement : MonoBehaviour
{
    //basically just the player movement but without the player *gasp*
    Vector2 movementInput;
    public Animator animator;
    public Rigidbody2D rb;
    float moveSpeed = 5f;
    int collisionOffset = 3;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    public ContactFilter2D movementFilter;
    public Transform player;

    // Start is called before the first frame update

    void Start()
    {
        // Find the player GameObject using a tag (make sure your player has the specified tag)
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
        }
        StartCoroutine(FollowPlayer());//i put this in start just to test but it could be useful with some logic to start it

    }

    // Update is called once per frame
    void Update()
    {
        //so useless omg why did they even make this???
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
                        if (!success )
                        {
                            movementInput = new Vector2(((int)Random.value) ,(int)Random.value);
                        }
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


    IEnumerator FollowPlayer()//this thing just makes the npc look for the player and then run untill a distance 
    {
        if (player != null)
        {
            while ((Vector2.Distance(transform.position, player.position)) > 3)
            {
                // Calculate the direction from the NPC to the player
                int layerMask = LayerMask.GetMask("NPC");

                Vector3 directionToPlayer = ((player.position-new Vector3(0,.75f,0)) - (transform.position - new Vector3(0, .75f, 0)));

                // Normalize the direction to get a unit vector
                Vector3 normalizedDirection = directionToPlayer.normalized;
                Debug.DrawRay(new Vector2(transform.position.x, transform.position.y) - new Vector2(0, 0.75f), directionToPlayer,Color.red);//the -.75 is bc the collider of the player is that distance from the pivot which i think is what unity uses for the center
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x,transform.position.y)-new Vector2(0,0.75f),directionToPlayer,Mathf.Infinity,~layerMask);
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    movementInput = new Vector2(normalizedDirection.x,normalizedDirection.y);
                    Debug.Log("move toplayer");
                    animator.SetFloat("moveX", movementInput.x);
                    animator.SetFloat("moveY", movementInput.y);
                    if (movementInput.x != 0 || movementInput.y != 0)
                    {
                        animator.SetFloat("faceX", movementInput.x);
                        animator.SetFloat("faceY", movementInput.y);
                    }
                    yield return new WaitForSeconds(.1f);
                }
                else
                {
                    movementInput = Vector2.zero;
                    Debug.Log("no player hit");
                    yield return null;

                }
            }

            movementInput = Vector2.zero;
            yield break;
        }
    }
}

