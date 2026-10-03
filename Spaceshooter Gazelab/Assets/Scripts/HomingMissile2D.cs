/*
 * Maurice Mattick - 3103312
 *
 * Marian Müller - 3103387
 *
 * Rezaul Hoque - 3077415
 *
 * Fardin Afzalzada - 3082980
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;
 
 
using UnityEngine;
 
public class HomingMissile2D : MonoBehaviour

{

    public string targetTag = "Enemy";

    public float moveSpeed = 10f;

    public float rotateSpeed = 540f;

    public float lifeTime = 5f;
 
    private Rigidbody2D rb;
 
    void Start()

    {

        rb = GetComponent<Rigidbody2D>();

        if (rb == null)

            Debug.LogError("HomingMissile2D: Rigidbody2D missing!");
 
        Destroy(gameObject, lifeTime);

    }


    void FixedUpdate()
    {
        if (rb == null) return;

        rb.velocity = transform.up * moveSpeed;
    }

    //alte Methode
    Transform FindClosestTarget()

    {

        GameObject[] objs = GameObject.FindGameObjectsWithTag(targetTag);

        if (objs.Length == 0) return null;
 
        Transform best = null;

        float bestDist = Mathf.Infinity;

        Vector3 pos = transform.position;
 
        foreach (GameObject go in objs)

        {

            float d = (go.transform.position - pos).sqrMagnitude;

            if (d < bestDist)

            {

                bestDist = d;

                best = go.transform;

            }

        }
 
        return best;

    }

}