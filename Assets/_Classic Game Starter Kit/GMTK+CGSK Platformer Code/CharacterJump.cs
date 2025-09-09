using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using XnTools;

#if UNITY_EDITOR
using UnityEditor;
#endif

//This script handles moving the character on the Y axis, for jumping and gravity
[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(CharacterGround))]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterJump : MonoBehaviour
{
    // [Header("Components")]
    [InfoBox("If you want to see a prediction of the jump when this GameObject is selected, check showJumpLine in the Character_Settings_SO that you're using.", EInfoBoxType.Normal)]
    [XnTools.Hidden] public Rigidbody2D rigid;
    [XnTools.Hidden] public Vector2 velocity;

    private CharacterMovement movement;
    private CharacterGround   ground;
    [Header( "Platformer Character Settings" )]
    private Character_Settings_SO csso = null;
    

    // [Header("Calculations")]
    // public float jumpSpeed;
    // public float gravMultiplier;

    [Header( "Current State" )]
    public int jumpsRemaining = 1;
    private bool desiredJump;
    private float jumpBufferCounter;
    private float coyoteTimeCounter = 0;
    private bool pressingJump;
    public bool onGround;
    private bool currentlyJumping;
    
    private float scaleP2DGravityTo1;
    public enum eJumpPhase { none, up, released, down };
    [SerializeField]
    private eJumpPhase _jumpPhase = eJumpPhase.none;
    private float jumpTimeStart; // the Time.time at which the jump began
    [SerializeField]
    private eJumpPhase _gravityType = eJumpPhase.up;
    public float gravityAcc = 0;

    public eJumpPhase gravityType {
        get { return _gravityType; }
        set {
            _gravityType = value;
            if ( value == eJumpPhase.up ) gravityAcc = csso.jumpGrav.up;
            else if ( value == eJumpPhase.released ) gravityAcc = csso.jumpGrav.up * csso.jumpSettingsVariableHeight.gravUpMultiplierOnRelease;
            else if ( value == eJumpPhase.down ) gravityAcc = csso.jumpGrav.down;
            rigid.gravityScale = scaleP2DGravityTo1 * gravityAcc;
        }
    }

    
    void Awake()
    {
        //Find the character's Rigidbody and ground detection and juice scripts
        movement = GetComponent<CharacterMovement>();
        rigid = GetComponent<Rigidbody2D>();
        ground = GetComponent<CharacterGround>();
        // scaleP2DGravityTo1 = 1f;
        csso = movement.characterSettingsSO;
        // This multiplier nullifies the Physics2D.gravity setting so that calculations can be done based around meters/sec^2 
        scaleP2DGravityTo1 = 1 / Physics2D.gravity.y;

        gravityType = eJumpPhase.up;
    }
    

    // private void SetPhysics() {
    //     //Determine the character's gravity scale, using the stats provided. Multiply it by a gravMultiplier, used later
    //     Vector2 newGravity = new Vector2( 0, ( -2 * csso.jumpHeight ) / ( csso.jumpDuration.up * csso.jumpDuration.up ) );
    //     rigid.gravityScale = ( newGravity.y / Physics2D.gravity.y ) * gravMultiplier;
    //     //TODO: Fix this to work with the new Gravity stuff.
    // }

    void Update() {
        if ( Xnput.GetButtonDown( Xnput.eButton.a ) ) {
            desiredJump = true;
        }
        pressingJump = Xnput.GetButton( Xnput.eButton.a );
    }



    private void FixedUpdate() {
        // Handling grounding this frame
        bool prevOnGround = onGround;
        onGround = ground.GetOnGround();
        if ( onGround != prevOnGround && onGround) { // Just landed this frame
            jumpsRemaining = csso.jumpsBetweenGrounding;
        }
        
        //If we're not on the ground and we're not currently jumping, that means we've stepped off the edge of a platform.
        //So, start the coyote time counter...
        if (!currentlyJumping && !onGround)
        {
            coyoteTimeCounter += Time.deltaTime;
        } else {
            //Reset it when we touch the ground, or jump
            coyoteTimeCounter = 0;
        }

        // SetPhysics();

        //Jump buffer allows us to queue up a jump, which will play when we next hit the ground
        if ( csso.jumpBuffer > 0 ) {
            //Instead of immediately turning off "desireJump", start counting up...
            //All the while, the DoAJump function will repeatedly be fired off
            if ( desiredJump ) {
                jumpBufferCounter += Time.deltaTime;

                if ( jumpBufferCounter > csso.jumpBuffer ) {
                    //If time exceeds the jump buffer, turn off "desireJump"
                    desiredJump = false;
                    jumpBufferCounter = 0;
                }
            }
        }



        //Get velocity from Kit's Rigidbody 
        velocity = rigid.velocity;

        //Keep trying to do a jump, for as long as desiredJump is true
        if (desiredJump)
        {
            DoAJump();

            //Skip gravity calculations this frame, so currentlyJumping doesn't turn off
            //This makes sure you can't do the coyote time double jump bug
            rigid.velocity = velocity; // Assign velocity to rigid because return will be called next line.
            return;
        }

        CalculateGravity();
        rigid.velocity = velocity;
    }

    private void CalculateGravity()
    {
        // I completely rewrote this script - JGB 2023-03-14
        
        // If on the ground in any way, (even on a moving platform), reset gravity and jumping
        //  however, number of jumps is set in FixedUpdate
        if ( onGround ) {
            gravityType = eJumpPhase.up;
            currentlyJumping = false;
            return; // Nothing more to see here
        }

        // In *all* of these cases, the character is NOT grounded
        switch ( _jumpPhase ) {
        case eJumpPhase.none: // Don't need to do anything.
            break;
        
        case eJumpPhase.up: // Check to see if jump button was released or apex was reached
            // If the velocity transitions to downward, meaning we have reached the apex of this jump...
            if ( velocity.y < 0 ) {
                _jumpPhase = eJumpPhase.down;
                gravityType = eJumpPhase.down;
                velocity.y = 0; // Zeroing the y velocity leads to more consistent down phases
            } else 
            // If we're using variable jump height...
            if ( csso.jumpSettingsVariableHeight.useVariableJumpHeight ) {
                // If the jump button was released AND the min jump button held time has passed, then move on to the released phase
                if ( !pressingJump &&
                     (( Time.time - jumpTimeStart ) > csso.jumpSettingsVariableHeight.minJumpButtonHeldTime )) {
                    // If velocity should be zeroed when the jump button is released (like in Metroid for NES)... 
                    if ( csso.jumpSettingsVariableHeight.upwardVelocityZeroing ) {
                        velocity.y = 0; // zero the y velocity
                        _jumpPhase = eJumpPhase.down; // move to the down phase
                        gravityType = eJumpPhase.down;
                    } else {
                        _jumpPhase = eJumpPhase.released;
                        gravityType = eJumpPhase.released;
                    }
                }
            }
            break;
        
        case eJumpPhase.released: // The button was released, so we need to check for the apex
            if ( velocity.y < 0 ) {
                _jumpPhase = eJumpPhase.down;
                gravityType = eJumpPhase.down;
                velocity.y = 0; // Zeroing the y velocity leads to more consistent down phases
            }
            break;
        
        case eJumpPhase.down: // Look for landing
            if ( onGround ) {
                _jumpPhase = eJumpPhase.none;
                gravityType = eJumpPhase.up;
            }
            break;
        }
        
        // Check for terminal fall velocity being reached
        velocity.y = Mathf.Clamp( velocity.y, -csso.terminalFallVelocity, 100 );
        // rigid.velocity is set in FixedUpdate()
    }

    
    private void DoAJump() {
        if ( jumpsRemaining <= 0 ) return; // If we don't have any jumps, don't jump!
        
        //Create the jump, provided we are on the ground, in coyote time, or have a double jump available
        if (onGround || (coyoteTimeCounter > 0.03f && coyoteTimeCounter < csso.coyoteTime ) || jumpsRemaining > 0) {
            StairMaster.ON_STAIRS = false;
            
            desiredJump = false;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            jumpsRemaining--; // Reduce number of remaining jumps

            _jumpPhase = eJumpPhase.up;
            gravityType = eJumpPhase.up;

            velocity.y = csso.jumpVel.up;
            currentlyJumping = true;
            
            jumpTimeStart = Time.time;
        }

        if ( csso.jumpBuffer == 0) // TODO: Do we need this anymore? - JGB 2023-03-14
        {
            //If we don't have a jump buffer, then turn off desiredJump immediately after hitting jumping
            desiredJump = false;
        }
    }

    public void BounceUp(float bounceAmount) // TODO: Do we need this anymore? - JGB 2023-03-14
    {
        //Used by the springy pad
        rigid.AddForce(Vector2.up * bounceAmount, ForceMode2D.Impulse);
    }

/*

timeToApexStat = scale(1, 10, 0.2f, 2.5f, numberFromPlatformerToolkit)


  public float scale(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue)
    {

        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

        return (NewValue);
    }

*/
#if UNITY_EDITOR
    
    [CustomEditor( typeof(CharacterJump) )]
    public class CharacterJump_Editor : Editor {
        private const float dashSize = 4;
        
        private CharacterMovement cMove;
        private CharacterJump     cJump;
    
        private void OnEnable() {
            cJump = (CharacterJump) target;
            cMove = cJump.GetComponent<CharacterMovement>();
        }
    
    
        private void OnSceneGUI() {
            if ( cMove                     == null ) return;
            if ( cMove.characterSettingsSO == null ) return;
            Character_Settings_SO csso = cMove.characterSettingsSO;
            if ( !csso.showJumpLine ) return;
            if ( csso.jumpSettingsType == Character_Settings_SO.eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED ) return;
            if (csso.jumpLinePoints == null) cMove.characterSettingsSO.CalculateJumpLine();

            GUIStyle labelStyle = new GUIStyle( EditorStyles.foldoutHeader );
            labelStyle.imagePosition = ImagePosition.TextOnly; // NOTE: This didn't seem to do anything.
            labelStyle.richText = true;
            
            Handles.matrix = Matrix4x4.Translate(cJump.transform.position);
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(4, csso.jumpLinePoints);
            Vector3 tVec;
            Vector3[] jSME = csso.jumpStartMidEndPoints;
            if ( jSME != null && jSME.Length == 3 ) {
                Vector3 offset = Vector3.up * 0.2f;
                Handles.DrawDottedLine( jSME[0] + offset, jSME[2] + offset, dashSize );
                tVec = ( jSME[0] + jSME[2] ) / 2f + offset * 4 + Vector3.left * 0.4f;
                Handles.Label( tVec, $"<b>Dist: {csso.maxJumpDistHeight.x:0.##}</b>", labelStyle );
                tVec = jSME[1];
                tVec.y = 0;
                Handles.DrawDottedLine( tVec, jSME[1], dashSize );
                tVec = ( tVec + jSME[1] ) / 2f + Vector3.left * 0.4f;
                Handles.Label( tVec, $"<b>Height: {csso.maxJumpDistHeight.y:0.##}</b>", labelStyle );
            }

            if ( csso.jumpSettingsVariableHeight.useVariableJumpHeight ) {
                Handles.color = Color.magenta;
                Handles.DrawAAPolyLine( 8, csso.minJumpLinePoints.ToArray() );
                if ( csso.minJumpStartMidEndPoints        != null &&
                     csso.minJumpStartMidEndPoints.Length == 3 ) {
                    tVec = csso.minJumpStartMidEndPoints[0] + Vector3.down * 0.25f;
                    Handles.Label( tVec, $"<b>Min: Ht: {csso.minJumpDistHeight.y:0.##}   Dst: {csso.minJumpDistHeight.x:0.##}" +
                                         $"   tApex: {csso.minTimeApexFull.x:0.##}   tFull: {csso.minTimeApexFull.y:0.##}</b>", labelStyle );
                }
            }

        }
    }    
    
#endif



}