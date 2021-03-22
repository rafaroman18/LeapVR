using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Interaction;
using UnityEngine;

public class DetectCollision : MonoBehaviour
{
    private PlaceObjectOnGrid placeObjectOnGrid;
    private bool collisionOnce;
    public Square squareCell;
    private InteractionBehaviour bhvr;

    void Start()
    {
        placeObjectOnGrid = FindObjectOfType<PlaceObjectOnGrid>();
        bhvr = transform.GetComponent<InteractionBehaviour>();
        collisionOnce = true;
    }

    void OnCollisionEnter(Collision targetObj)
    {
        if (collisionOnce &&
        !bhvr.isGrasped &&
        targetObj.gameObject.tag == "Cell")
        {
            squareCell = placeObjectOnGrid.PlaceOnGrid(transform);
            if (squareCell != null)
                collisionOnce = false;
        }
    }

    void OnCollisionExit(Collision targetObj)
    {
        if (targetObj.gameObject.tag == "Cell")
        {
            collisionOnce = true;
        }
    }
}
