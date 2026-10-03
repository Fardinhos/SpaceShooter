/*
 * Maurice Mattick - 3103312
 *
 * Marian Müller - 3103387
 *
 * Rezaul Hoque - 3077415
 *
 * Fardin Afzalzada - 3082980
 */

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class PSDestroy : MonoBehaviour
{
        void Start()
        {
            var ps = GetComponent<ParticleSystem>();

            Destroy(gameObject, ps.main.duration);
        }
}