using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateController : MonoBehaviour
{
    Animator animator;
    int IsMovingHash;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        IsMovingHash = Animator.StringToHash("IsMoving");
    }

    // Update is called once per frame
    void Update()
    {
        bool isMoving = animator.GetBool(IsMovingHash);
        //Change this to be for controller, twas for testing anim
        bool forwardPressed = Input.GetKey ("w");
        //Player Moves
        if (forwardPressed && forwardPressed)
        {
            //Set is moving to true
            animator.SetBool(IsMovingHash, true);
        }
        {
            if (isMoving && !forwardPressed)
            {
                animator.SetBool(IsMovingHash, false);
            }
        }
    }
}
