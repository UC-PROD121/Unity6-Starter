#define Use_Xnput

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


[RequireComponent(typeof(Rigidbody))]
public class FirstPersonController : MonoBehaviour {
    [Header( "Inscribed" )]
    public float speed = 10;
    
    [OnValueChanged("SetJumpVars")]
    public float jumpHeight = 5;
    [OnValueChanged("SetJumpVars")]
    public float jumpDist   = 10;
    [Range(0.1f,0.9f)]
    [OnValueChanged("SetJumpVars")]
    public float jumpApex = 0.5f;
    [Tooltip("Note that variable jump height only works if jumpApex > 0.5." +
             "    Otherwise, there is no difference between rising and falling gravity")]
    public bool useVariableHeightJump = true;
    
    public Transform camTrans;
    public float     yawMult     = 30;
    public float     pitchMult   = 20;
    public bool      invertPitch = true;
    public Vector2   pitchLimits = new Vector2( -60, 60 );
    
    
    [Header("Dynamic")]
    public float jumpVel;
    public float jumpGrav;
    public float jumpGravDown;
    public bool  jumpRising = false;
    
    private Rigidbody rigid;

    void Start() {
        rigid = GetComponent<Rigidbody>();
        
        // Hide and Lock the Cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        // We're not using Unity gravity because we're modifying it ourselves - JGB 2025-03-09
        rigid.useGravity = false;
    }

    // Update is called once per frame
    void Update() {
        // NOTE: This is only really necessary when you're tuning the jump values.
        //   You can remove this call once you're happy with the jump values. - JGB 2025-03-09
#if UNITY_EDITOR // Only run this in the Editor, in case you don't remove it.
        SetJumpVars();
#endif
        
        // Get the horizontal and vertical axis.
        // By default they are mapped to the arrow keys.
        // The value is in the range -1 to 1
#if Use_Xnput
        float h = Xnput.GetAxis( Xnput.eAxis.horizontal );
        float v = Xnput.GetAxis( Xnput.eAxis.vertical );
        float mX = Xnput.GetAxisRaw( Xnput.eAxis.rightStickH );
        float mY = Xnput.GetAxisRaw( Xnput.eAxis.rightStickV );
        bool jumpNow = Xnput.GetButtonDown( Xnput.eButton.a );
        bool jumpHeld = Xnput.GetButton( Xnput.eButton.a );
#else
        float h = Input.GetAxis( "Horizontal" );
        float v = Input.GetAxis( "Vertical" );
        float mX = Input.GetAxisRaw( "Mouse X" );
        float mY = Input.GetAxisRaw( "Mouse Y" );
        bool jumpNow = Input.GetKeyDown( KeyCode.Space ) || Input.GetKeyDown( KeyCode.X );
        bool jumpHeld = Input.GetKey( KeyCode.Space ) || Input.GetKey( KeyCode.X );
#endif

        // XY movement
        Vector3 vel = transform.forward * v + transform.right * h;
        if ( vel.magnitude > 1 ) vel.Normalize();
        vel *= speed;
        
        // Jump movement
        // NOTE: There is no Grounded check for this character, so you can just infinitely air jump
        vel.y = rigid.velocity.y;
        if ( jumpNow ) {
            // If jump was pressed this frame, set the vel.y and start rising jump
            jumpRising = true;
            vel.y = jumpVel;
        } else if ( !jumpHeld && useVariableHeightJump ) {
            // If the player is no longer holding the jump button, jumpRising = false
            //  This makes the variable-height jump work
            jumpRising = false;
        }
        
        // Assign back to Rigidbody
        rigid.velocity = vel;

        // Player rotation (Yaw)
        Vector3 rot = transform.eulerAngles;
        rot.y += mX * yawMult * Time.deltaTime;
        transform.eulerAngles = rot;
        
        // Camera rotation (Pitch)
        Vector3 rotCam = camTrans.eulerAngles;
        float rotX = rotCam.x + ( mY * pitchMult * Time.deltaTime * (invertPitch ? -1 : 1) );
        if ( rotX > 180 ) rotX = rotX - 360;
        rotX = Mathf.Clamp( rotX, pitchLimits.x, pitchLimits.y );
        rotCam = new Vector3( rotX, 0, 0 );
        camTrans.localEulerAngles = rotCam;
        
    }

    void FixedUpdate() {
        // Apply our own gravity, since we're adjusting it and NOT using standard Unity gravity
        Vector3 vel = rigid.velocity;
        if ( jumpRising ) {
            vel.y += jumpGrav * Time.fixedDeltaTime;
        } else {
            vel.y += jumpGravDown * Time.fixedDeltaTime;
        }
        if (vel.y < 0 ) jumpRising = false;
        rigid.velocity = vel;
    }
    
    

    void SetJumpVars() {
        float jumpDistHalf = jumpDist * jumpApex;
        jumpVel = 2 * jumpHeight * speed / jumpDistHalf;
        jumpGrav = -2 * jumpHeight * (speed * speed) / (jumpDistHalf * jumpDistHalf);

        float fallingDistHalf = jumpDist - jumpDistHalf;
        jumpGravDown = -2 * jumpHeight * (speed * speed) / (fallingDistHalf * fallingDistHalf);
    }
}