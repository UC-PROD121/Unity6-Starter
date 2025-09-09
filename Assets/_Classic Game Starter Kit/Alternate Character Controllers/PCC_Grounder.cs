using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCC_Grounder : MonoBehaviour {
    [Header("Inscribed")]
    public float width = .25f;
    public float     depth = .05f;
    public LayerMask groundMask;

    [Header("Dynamic")]
    public bool isGrounded = false;


    void FixedUpdate() {
        isGrounded = false;
        
        // Raycast to see if we hit the ground
        Vector2 p0 = (Vector2) transform.position - Vector2.right * width + Vector2.up * depth;
        // ((Vector2) transform.position) - (Vector2.right * width) + (Vector2.up * depth);
        RaycastHit2D hitLeft = Physics2D.Raycast( p0, Vector2.down, depth * 2, groundMask );
        if ( hitLeft.collider != null ) {
            // We hit something!
            isGrounded = true;
            Debug.DrawLine( p0, p0 + Vector2.down * (depth * 2), Color.green );
        } else {
            Debug.DrawLine( p0, p0 + Vector2.down * (depth * 2), Color.blue );
        }
        
        p0 = (Vector2) transform.position + Vector2.right * width + Vector2.up * depth;
        RaycastHit2D hitRight = Physics2D.Raycast( p0, Vector2.down, depth * 2, groundMask );
        if ( hitRight.collider != null ) {
            // We hit something!
            isGrounded = true;
            Debug.DrawLine( p0, p0 + Vector2.down * (depth * 2), Color.green );
        } else {
            Debug.DrawLine( p0, p0 + Vector2.down * (depth * 2), Color.blue );
        }
    }
    
}
