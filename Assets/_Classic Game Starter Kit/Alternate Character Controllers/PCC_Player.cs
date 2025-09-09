using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using XnTools;

[RequireComponent(typeof(PCC_Grounder))]
[RequireComponent(typeof(Rigidbody2D))]
public class PCC_Player : MonoBehaviour {
    [Header("Inscribed")]
    public float speed   = 10;
    [OnValueChanged("SetJumpVars")]
    public float jumpHeight = 5;
    [OnValueChanged("SetJumpVars")]
    public float jumpDist   = 10;
    [Range(0.1f,0.9f)]
    [OnValueChanged("SetJumpVars")]
    public float jumpApex = 0.5f;
    public bool drawJumpPreview = false;
    
    [Header("Dynamic")]
    public float jumpVel;
    public float jumpGrav;
    public float jumpGravDown;

    private float baseGrav; 

    protected Rigidbody2D r2d;
    protected PCC_Grounder    grounder;
    
    // Start is called before the first frame update
    void Start() {
        r2d = GetComponent<Rigidbody2D>();
        grounder = GetComponent<PCC_Grounder>();
        baseGrav = Physics2D.gravity.y;
    }

    void SetJumpVars() {
        float jumpDistHalf = jumpDist * jumpApex;
        jumpVel = 2 * jumpHeight * speed / jumpDistHalf;
        jumpGrav = -2 * jumpHeight * (speed * speed) / (jumpDistHalf * jumpDistHalf);

        float fallingDistHalf = jumpDist - jumpDistHalf;
        jumpGravDown = -2 * jumpHeight * (speed * speed) / (fallingDistHalf * fallingDistHalf);
    }

    private bool jumpRising = false;
    
    // Update is called once per frame
    void Update() {
        SetJumpVars();
        float h = Xnput.GetAxis( Xnput.eAxis.horizontal );
        float v = Xnput.V;

        // Horizontal movement
        Vector2 vel = r2d.velocity;
        vel.x = h * speed;
        
        
        // Basic Jumping
        if ( grounder.isGrounded && Xnput.GetButtonDown( Xnput.eButton.a ) ) {
            r2d.gravityScale = jumpGrav / baseGrav;
            vel.y = jumpVel;
            jumpRising = true;
        }
        
        
        r2d.velocity = vel;
    }

    private void FixedUpdate() {
        if ( jumpRising ) {
            if ( r2d.velocity.y <= 0 || !Xnput.GetButton(Xnput.eButton.a) ) {
                jumpRising = false;
                r2d.gravityScale = jumpGravDown / baseGrav;
            }
        }
    }

    private void OnDrawGizmos() {
        if ( !drawJumpPreview ) return;
        Vector3 p0, p1;
        p0 = transform.position;
        int iterations = 100;
        float dT = (jumpDist / speed) / iterations;
        float vY = jumpVel;

        for (int i = 0; i < iterations; i++) {
            p1 = p0;
            p1.x += speed * dT;
            p1.y += vY * dT;
            
            // Update jumpVel
            if ( vY > 0 ) {
                vY += dT * jumpGrav;
            } else {
                vY += dT * jumpGravDown;
            }
            
            Debug.DrawLine(p0, p1, Color.magenta);
            p0 = p1;
        }
    }
}