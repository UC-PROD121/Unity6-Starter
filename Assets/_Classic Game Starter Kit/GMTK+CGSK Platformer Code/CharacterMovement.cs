using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using XnTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

//This script handles moving the character on the X axis, both on the ground and in the air.
[RequireComponent( typeof(CharacterGround) )]
[RequireComponent( typeof(Rigidbody2D) )]
public class CharacterMovement : MonoBehaviour {

    // [Header( "Components" )]
    private Rigidbody2D       body;
    private CharacterGround   ground;
    private CapsuleCollider2D cC2D;


    [Header( "Character Settings Scriptable Object" )]
    [NaughtyAttributes.Expandable]
    public Character_Settings_SO characterSettingsSO = null;

    //[Header("Movement Stats")]
    //[SerializeField, Range(0f, 20f)][Tooltip("Maximum movement speed")] public float maxSpeed = 10f;
    //[SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed")] public float maxAcceleration = 52f;
    //[SerializeField, Range(0f, 100f)][Tooltip("How fast to stop after letting go")] public float maxDeceleration = 52f;
    //[SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction")] public float maxTurnSpeed = 80f;
    //[SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed when in mid-air")] public float maxAirAcceleration;
    //[SerializeField, Range(0f, 100f)][Tooltip("How fast to stop in mid-air when no direction is used")] public float maxAirDeceleration;
    //[SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction when in mid-air")] public float maxAirTurnSpeed = 80f;
    //[SerializeField][Tooltip("Friction to apply against movement on stick")] private float friction;

    //[Header("Options")]
    //[Tooltip("When false, the charcter will skip acceleration and deceleration and instantly move and stop")] public bool useAcceleration;

    [Header( "Current State" )]
    [XnTools.ReadOnly] public float directionX;
    private Vector2 desiredVelocity;
    [XnTools.ReadOnly] public Vector2 velocity;
    private float   maxSpeedChange;
    private float   acceleration;
    private float   deceleration;
    private float   turnSpeed;

    [XnTools.ReadOnly] public bool onGround;
    [XnTools.ReadOnly] public bool nonZeroHorizontalInput;

    private void Awake() {
        if ( characterSettingsSO == null ) {
            Debug.LogError(
                "You must assign a Character_Settings_SO Scriptable Object to the CharacterMovement script for it to function." );
            enabled = false;
        }
        //Find the character's Rigidbody and ground detection script
        body = GetComponent<Rigidbody2D>();
        ground = GetComponent<CharacterGround>();
        cC2D = GetComponent<CapsuleCollider2D>();

        SetCapsuleCollider2DValues(cC2D, characterSettingsSO);
        SetGroundingRayValues( ground, characterSettingsSO );
    }

    // public void OnMovement(InputAction.CallbackContext context)
    // {
    //     //This is called when you input a direction on a valid input type, such as arrow keys or analogue stick
    //     //The value will read -1 when pressing left, 0 when idle, and 1 when pressing right.
    //     directionX = context.ReadValue<float>();
    // }

    private void Update() {
        // Check for Xnput button presses
        directionX = 0;
        if ( Xnput.GetButton( Xnput.eButton.left ) ) directionX -= 1;
        if ( Xnput.GetButton( Xnput.eButton.right ) ) directionX += 1;

        //Used to flip the character's sprite when she changes direction
        //Also tells us that we are currently pressing a direction button
        if ( directionX != 0 ) {
            transform.localScale = new Vector3( directionX > 0 ? 1 : -1, 1, 1 );
            nonZeroHorizontalInput = true;
        } else { nonZeroHorizontalInput = false; }

        //Calculate's the character's desired velocity - which is the direction you are facing, multiplied by the character's maximum speed
        //Friction is not used in this game
        // // Then WHY is was the friction code still on the next line?!? - JGB 2023-03-14
        desiredVelocity = new Vector2( directionX, 0f ) *
                          Mathf.Max( characterSettingsSO.maxSpeed,
                              0 ); // - characterSettingsSO.friction, 0f);

    }

    private void FixedUpdate() {
        //Fixed update runs in sync with Unity's physics engine

        //Get Kit's current ground status from her ground script
        onGround = ground.GetOnGround();

        //Get the Rigidbody's current velocity
        velocity = body.velocity;

        //Calculate movement, depending on whether "Instant Movement" has been checked
        if ( characterSettingsSO.useAcceleration ) { RunWithAcceleration(); } else {
            if ( onGround ) { RunWithoutAcceleration(); } else { RunWithAcceleration(); }
        }

        //Update the Rigidbody with this new velocity
        body.velocity = velocity;
    }

    private void RunWithAcceleration() {
        //Set our acceleration, deceleration, and turn speed stats, based on whether we're on the ground on in the air

        acceleration = onGround ? characterSettingsSO.maxAcceleration : characterSettingsSO.maxAirAcceleration;
        deceleration = onGround ? characterSettingsSO.maxDeceleration : characterSettingsSO.maxAirDeceleration;
        turnSpeed = onGround ? characterSettingsSO.maxTurnSpeed : characterSettingsSO.maxAirTurnSpeed;

        if ( nonZeroHorizontalInput ) {
            //If the sign (i.e. positive or negative) of our input direction doesn't match our movement, it means we're turning around and so should use the turn speed stat.
            // if ( Mathf.Sign( directionX ) != Mathf.Sign( velocity.x ) ) { // This was a really slow way to do this. - JGB 2023-03-14
            if ( directionX * velocity.x < 0 ) { // This does the same thing without two function calls
                maxSpeedChange = turnSpeed * Time.deltaTime;
            } else {
                //If they match, it means we're simply running along and so should use the acceleration stat
                maxSpeedChange = acceleration * Time.deltaTime;
            }
        } else {
            //And if we're not pressing a direction at all, use the deceleration stat
            maxSpeedChange = deceleration * Time.deltaTime;
        }

        //Move our velocity towards the desired velocity, at the rate of the number calculated above
        velocity.x = Mathf.MoveTowards( velocity.x, desiredVelocity.x, maxSpeedChange );


    }

    private void RunWithoutAcceleration() {
        //If we're not using acceleration and deceleration, just send our desired velocity (direction * max speed) to the Rigidbody
        velocity.x = desiredVelocity.x;
    }

    internal void SetCapsuleCollider2DValues(CapsuleCollider2D cC2D, Character_Settings_SO csso) {
        // Adjust the CapsuleCollider2D based on settings in csso
        Vector2 size = Vector2.zero;
        size.x = csso.colliderSettings.width;
        size.y = csso.colliderSettings.height;
        cC2D.size = size;

        Vector2 offset = Vector2.zero;
        offset.y = csso.colliderSettings.height * 0.5f;
        cC2D.offset = offset;
    }

    internal void SetGroundingRayValues(CharacterGround cg, Character_Settings_SO csso) {
        // Adjust the CharacterGround settings based on csso
        cg.raycastOffsetHeight = new Vector3(0, csso.colliderSettings.groundRaycastDepth * 0.5f, 0);
        cg.raycastOffsetWidth = new Vector3(csso.colliderSettings.groundRaycastWidth * 0.5f, 0, 0);
        cg.groundLength = csso.colliderSettings.groundRaycastDepth;
        cg.groundLayers = csso.colliderSettings.groundLayers;
    } 
}


#if UNITY_EDITOR

[CustomEditor( typeof(CharacterMovement) )]
public class CharacterMovement_Editor : Editor {
    private const float lineThickness = 2;

    private CharacterMovement cMove;
    private CapsuleCollider2D cC2D;
    private CharacterGround cGround;
    private void OnEnable() {
        cMove = (CharacterMovement) target;
        cC2D = cMove.GetComponent<CapsuleCollider2D>();
        cGround = cMove.GetComponent<CharacterGround>();
    }


    private void OnSceneGUI() {
        if ( cMove                     == null ) return;
        if ( cMove.characterSettingsSO == null ) return;
        if (cC2D == null) return;
        if (cGround == null) return;

        Character_Settings_SO csso = cMove.characterSettingsSO;

        // Adjust the CapsuleCollider2D based on settings in csso
        cMove.SetCapsuleCollider2DValues( cC2D, csso );

        // Adjust the CharacterGround settings based on csso
        cMove.SetGroundingRayValues(cGround, csso);

        // Show the ground raycasts
        Handles.matrix = Matrix4x4.Translate( cMove.transform.position );
        Handles.color = Color.green;
        // Draw bottom circle of collider (This is drawn because the collider outline doesn't show up well
        Handles.DrawWireDisc(
            new Vector3( 0, csso.colliderSettings.width * 0.5f, 0 ),
            Vector3.back,
            csso.colliderSettings.width * 0.5f,
            lineThickness );
        // Draw bottom circle of collider (This is drawn because the collider outline doesn't show up well
        Handles.DrawWireDisc(
            new Vector3( 0, csso.colliderSettings.height - csso.colliderSettings.width * 0.5f, 0 ),
            Vector3.back,
            csso.colliderSettings.width * 0.5f,
            lineThickness );
        // Ground raycasts are drawn by the CharacterGround script

        // if ( !csso.showJumpLine ) return;
        // if ( csso.jumpSettingsType == Character_Settings_SO.eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED ) return;
        // if ( csso.jumpLinePoints   == null ) cMove.characterSettingsSO.CalculateJumpLine();
        //
        // GUIStyle labelStyle = new GUIStyle( EditorStyles.foldoutHeader );
        // labelStyle.imagePosition = ImagePosition.TextOnly; // NOTE: This didn't seem to do anything.
        // labelStyle.richText = true;
        //
        // Handles.matrix = Matrix4x4.Translate( cJump.transform.position );
        // Handles.color = Color.green;
        // Handles.DrawAAPolyLine( 4, csso.jumpLinePoints );
        // Vector3 tVec;
        // Vector3[] jSME = csso.jumpStartMidEndPoints;
        // if ( jSME != null && jSME.Length == 3 ) {
        //     Vector3 offset = Vector3.up * 0.2f;
        //     Handles.DrawDottedLine( jSME[0] + offset, jSME[2] + offset, dashSize );
        //     tVec = ( jSME[0] + jSME[2] ) / 2f + offset * 4 + Vector3.left * 0.4f;
        //     Handles.Label( tVec, $"<b>Dist: {csso.maxJumpDistHeight.x:0.##}</b>", labelStyle );
        //     tVec = jSME[1];
        //     tVec.y = 0;
        //     Handles.DrawDottedLine( tVec, jSME[1], dashSize );
        //     tVec = ( tVec + jSME[1] ) / 2f + Vector3.left * 0.4f;
        //     Handles.Label( tVec, $"<b>Height: {csso.maxJumpDistHeight.y:0.##}</b>", labelStyle );
        // }
        //
        // if ( csso.jumpSettingsVariableHeight.useVariableJumpHeight ) {
        //     Handles.color = Color.magenta;
        //     Handles.DrawAAPolyLine( 8, csso.minJumpLinePoints.ToArray() );
        //     if ( csso.minJumpStartMidEndPoints        != null &&
        //          csso.minJumpStartMidEndPoints.Length == 3 ) {
        //         tVec = csso.minJumpStartMidEndPoints[0] + Vector3.down * 0.25f;
        //         Handles.Label( tVec,
        //             $"<b>Min: Ht: {csso.minJumpDistHeight.y:0.##}   Dst: {csso.minJumpDistHeight.x:0.##}" +
        //             $"   tApex: {csso.minTimeApexFull.x:0.##}   tFull: {csso.minTimeApexFull.y:0.##}</b>",
        //             labelStyle );
        //     }
        // }

    }
}

#endif
    
