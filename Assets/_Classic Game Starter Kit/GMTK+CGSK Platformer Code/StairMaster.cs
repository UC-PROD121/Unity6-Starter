using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using XnTools;

[DefaultExecutionOrder(10)]
public class StairMaster : MonoBehaviour {
    static public StairMaster S;
    // static public bool ON_STAIRS;
    
    [Header( "Inscribed" )]
    public float stairSpeed = 5;
    [Tooltip("How close does the player need to be to the end of the stairs to hop off of them? (Default 0.01f)")]
    public float stairsEndLimit = 0.01f;
    
    [Header("Dynamic")]
    [XnTools.ReadOnly] public float dirX = 0;
    [XnTools.ReadOnly] public float dirY = 0;
    [XnTools.ReadOnly] public Stairs stairs = null;
    [XnTools.ReadOnly][SerializeField] private bool _onStairs = false;
    [XnTools.ReadOnly] public float stairsU    = -1;

    private Rigidbody2D r2D;

    void Awake() {
        S = this;
    }
    
    public void OnMovement(InputAction.CallbackContext context)
    {
        //This is called when you input a direction on a valid input type, such as arrow keys or analogue stick
        //The value will read -1 when pressing left, 0 when idle, and 1 when pressing right.
        dirX = context.ReadValue<float>();
    }
    
    public void OnVertical(InputAction.CallbackContext context)
    {
        //This is called when you input a direction on a valid input type, such as arrow keys or analogue stick
        //The value will read -1 when pressing down, 0 when idle, and 1 when pressing up.
        dirY = context.ReadValue<float>();
    }

    public static bool ON_STAIRS {
        get { return (S==null) ? false : S.onStairs; }
        set { if (S != null) S.onStairs = value; }
    }
    public bool onStairs {
        get { return _onStairs; }
        private set {
            _onStairs = value;
            
            if ( r2D == null ) r2D = GetComponent<Rigidbody2D>();
            if (r2D != null) {
                // r2D.simulated = !value;
                r2D.constraints = value ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }
    

    private void OnTriggerEnter2D( Collider2D col ) {
        Stairs st = col.transform.parent.GetComponent<Stairs>();
        if ( st != null ) {
            stairs = st;
            onStairs = false;
        }
    }
    
    private void OnTriggerExit2D( Collider2D col ) {
        Stairs st = col.transform.parent.GetComponent<Stairs>();
        if ( st == stairs ) {
            stairs = null;
            onStairs = false;
        }
    }

    void CheckInput() {
        dirY = 0;
        if ( Xnput.GetButton( Xnput.eButton.up ) ) dirY += 1;
        if ( Xnput.GetButton( Xnput.eButton.down ) ) dirY -= 1;

        dirX = 0;
        if ( Xnput.GetButton( Xnput.eButton.right ) ) dirX += 1;
        if ( Xnput.GetButton( Xnput.eButton.left ) ) dirX -= 1;
        
        if (ON_STAIRS && Xnput.GetButton(Xnput.eButton.a)) onStairs = false;
    }
    
    private void FixedUpdate() {
        CheckInput();
        
        if ( !onStairs ) {
            if ( stairs != null ) {
                // Only up and down will attach to stairs
                if ( dirY != 0 ) {
                    // Move the position of the player into Stairs coordinates.
                    Vector3 pPos = transform.position;
                    // Find position on stairs in stairsU coordinates (i.e., relative to p0 and p1)
                    Vector3 stairsDeltaVec = stairs.p1 - stairs.p0;
                    // stairs.actualLength is the length from p0 to p1
                    Vector3 stairsDirection = stairsDeltaVec.normalized;
                    float projection = Vector3.Dot(stairsDirection, pPos - stairs.p0);
                    // projectionU is the distance that pPos is along the stairs in terms of stairsU
                    float projectionU = projection / stairs.actualLength;
                    bool attachToStairs = false; // The default case when we're not near the stairs
                    if ( projectionU < stairsEndLimit && dirY > 0) {
                        // Only attach to bottom of stairs if going up
                        attachToStairs = true;
                    } else if ( projectionU > (1-stairsEndLimit) && dirY < 0 ) {
                        // Only attach to top of stairs if going down
                        attachToStairs = true;
                    } else if ( stairsEndLimit < projectionU && projectionU < (1-stairsEndLimit) ) {
                        // We must be in the middle of the stairs
                        attachToStairs = true;
                    } else {
                        // attachToStairs = false; // This line isn't necessary, but it would go here.
                    }

                    if (attachToStairs) {
                        onStairs = true;
                        stairsU = projectionU;
                    }
                }
            }
        } else {
            float dirUp = dirY;
            if ( dirUp == 0 ) {
                if ( stairs.orientation == Stairs.eOrientation.diagonalLeft ) dirUp = -dirX;
                if ( stairs.orientation == Stairs.eOrientation.diagonalRight ) dirUp = dirX;
            }

            if ( dirUp != 0 ) {
                float uPerSecond = stairSpeed / stairs.actualLength;
                stairsU += uPerSecond * dirUp * Time.fixedDeltaTime;
                if ( stairsU <= 0 ) {
                    transform.position = stairs.p0;
                    onStairs = false;
                } else if ( stairsU >= 1 ) {
                    transform.position = stairs.p1;
                    onStairs = false;
                } else {
                    transform.position = Vector3.Lerp(stairs.p0, stairs.p1, stairsU);
                }
            }
            
            
        }
    }
}
