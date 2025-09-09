using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;  // https://github.com/dbrizov/NaughtyAttributes
using UnityEngine.Timeline;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Created by Jeremy Bond for MI 231 at Michigan State University
/// Built to work with a modified version of the GMTK Platformer Toolkit
/// </summary>
[CreateAssetMenu( fileName = "GMTK_Settings_[GameName]",
    menuName = "ScriptableObjects/GMTK_Settings", order = 1 )]
public class Character_Settings_SO : ScriptableObject {
    static bool DEBUG_JUMP_LINE_CALCULATION = false;

    
    [Header( "\n_____ Movement Stats & Options _____" )]
    [SerializeField, Range( 0f, 20f )]
    [Tooltip( "Maximum movement speed (m/s)" )]
    public float maxSpeed = 10f;
    [SerializeField, Range( 0f, 100f )]
    [Tooltip( "How fast to reach max speed (m/s/s)" )]
    public float maxAcceleration = 52f;
    [SerializeField, Range( 0f, 100f )]
    [Tooltip( "How fast to stop after letting go (m/s/s)" )]
    public float maxDeceleration = 52f;
    [SerializeField, Range( 0f, 100f )]
    [Tooltip( "How fast to stop when changing direction (m/s/s)" )]
    public float maxTurnSpeed = 80f;
    [SerializeField, Range( 0f, 100f )]
    [Tooltip( "How fast to reach max speed when in mid-air (m/s/s)" )]
    public float maxAirAcceleration = 0;
    [SerializeField, Range( 0f, 100f )]
    [Tooltip( "How fast to stop in mid-air when no direction is used (m/s/s)" )]
    public float maxAirDeceleration = 0;
    [SerializeField, Range( 0f, 100f )]
    [Tooltip( "How fast to stop when changing direction when in mid-air (m/s/s)" )]
    public float maxAirTurnSpeed = 80f;
    // [SerializeField]
    // [Tooltip( "Friction to apply against movement on stick" )]
    // public float friction = 0;
    [Tooltip( "When false, the character will skip acceleration and deceleration and instantly move and stop" )]
    public bool useAcceleration = true;



    // NOTE: CGSK jummp math comes from Math for Game Programmers: Building a Better Jump
    // https://www.youtube.com/watch?v=hG9SzQxaCm8&t=9m35s & https://www.youtube.com/watch?v=hG9SzQxaCm8&t=784s
    // Th = Xh/Vx     V0 = 2H / Th     G = -2H / (Th * Th)     V0 = 2HVx / Xh     G = -2H(Vx*Vx) / (Xh*Xh) 

    [Header( "\n\n_____ Jump Settings _____" )]
    public eJumpSettingsType jumpSettingsType = eJumpSettingsType.CGSK_Height_and_Distance;
    public enum eJumpSettingsType { CGSK_Height_and_Distance, CGSK_Time_DEPRECATED, GMTK_GameMakersToolKit_DEPRECATED };

    public bool showJumpLine = true;

    [Tooltip( "Maximum jump height" )]
    [Range( 1f, 10f )]
    public float jumpHeight = 4f;

    [Label( " ––– Classic Game Starter Kit - Time-Based Jump Settings - DEPRECATED ––– " )]
    [ShowIf( "jumpSettingsType", eJumpSettingsType.CGSK_Time_DEPRECATED )]
    public CGSK_JumpSettings_Time jumpSettingsTime;

    [Label( " ––– Classic Game Starter Kit - Variable Jump Height Settings ––– " )]
    [HideIf( "jumpSettingsType", eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED )]
    public CGSK_JumpSettings_VariableHeight jumpSettingsVariableHeightCGSK;
    
    [Label( " ––– GameMakers ToolKit - Jump Settings - DEPRECATED ––– " )]
    [ShowIf("jumpSettingsType", eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED )]
    public CGSK_JumpSettings_VariableHeight jumpSettingsVariableHeightGMTK;
    [HideInInspector] internal CGSK_JumpSettings_VariableHeight jumpSettingsVariableHeight;

    [Label( " ––– Classic Game Starter Kit - Jump Distance Settings –––" )]
    [ShowIf( "jumpSettingsType", eJumpSettingsType.CGSK_Height_and_Distance )]
    public CGSK_JumpSettings_Distance jumpSettingsDistance;

    // [HideIf( "jumpSettingsType", eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED )]
    // [XnTools.ReadOnly][BoxGroup("CGSK Derived Jump Properties")]
    // public float jumpDistUp, jumpDurationUp, jumpVelUp, jumpGravUp, jumpDistDown, jumpDurationDown, jumpVelDown, jumpGravDown;


    [HideIf( "jumpSettingsType", eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED )]
    [BoxGroup( "Derived Jump Properties" )]
    public CSSO_FloatUpDown jumpDist, jumpDuration, jumpVel, jumpGrav;
    [BoxGroup( "Derived Jump Properties" )] [SerializeField] [XnTools.ReadOnly]
    internal Vector2 maxJumpDistHeight, minJumpDistHeight;



    [ShowIf( "jumpSettingsType", eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED )]
    public GMTK_JumpSettings jumpSettingsGMTK;


    [Header( "Jump Options" )]
    [SerializeField]
    [Tooltip( "The fastest speed the character can fall" )]
    public float terminalFallVelocity = 26.45f;
    [SerializeField, Range( 0f, 0.3f )]
    [Tooltip( "How long should coyote time last?" )]
    public float coyoteTime = 0.15f;
    [SerializeField, Range( 0f, 0.3f )]
    [Tooltip( "How far from ground should we cache your jump?" )]
    public float jumpBuffer = 0.15f;
    [Tooltip( "Max jumps between grounding. (2 for Double Jump, 3 for Triple Jump, etc.) " )]
    [Range( 1, 10 )]
    public int jumpsBetweenGrounding = 1;


    [Header("\n\n_____ CapsuleCollider and Grounding Settings _____")]
    public CGSK_ColliderSettings colliderSettings;
    // TODO: Actually make these settings affect the CapsuleCollider2D



    [Header( "\n\n_____ Juice Settings - Squash and Stretch (currently unused) _____" )]
    [XnTools.Hidden]
    public bool squashAndStretch;
    [XnTools.Hidden, Tooltip( "Width Squeeze, Height Squeeze, Duration" )]
    public Vector3 jumpSquashSettings;
    [XnTools.Hidden, Tooltip( "Width Squeeze, Height Squeeze, Duration" )]
    public Vector3 landSquashSettings;
    [XnTools.Hidden, Tooltip( "How powerful should the effect be?" )]
    public float landSqueezeMultiplier;
    [XnTools.Hidden, Tooltip( "How powerful should the effect be?" )]
    public float jumpSqueezeMultiplier;
    [XnTools.Hidden]
    public float landDrop = 1;

    [Header("_____ Juice Settings - Tilting (currently unused) _____")]
    [XnTools.Hidden]
    public bool leanForward;
    [XnTools.Hidden, Tooltip( "How far should the character tilt?" )]
    public float maxTilt;
    [XnTools.Hidden, Tooltip( "How fast should the character tilt?" )]
    public float tiltSpeed;



    // public float timeToJumpApex;
    // [ShowIf( "jumpSettingsType", eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED )]
    // [SerializeField, Range( 0f, 5f )]
    // [Tooltip( "Gravity multiplier to apply when going up" )]
    // public float upwardMovementMultiplier = 1f;
    // [ShowIf( "jumpSettingsType", eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED )]
    // [SerializeField, Range( 1f, 10f )]
    // [Tooltip( "Gravity multiplier to apply when coming down" )]
    // public float downwardMovementMultiplier = 6.17f;
    // [ShowIf( "jumpSettingsType", eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED )]
    // [SerializeField, Range( 0, 1 )]
    // [Tooltip( "How many times can you jump in the air?" )]
    // public int maxAirJumps = 0;
    // [ShowIf( "jumpSettingsType", eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED )]
    // [Tooltip( "Should the character drop when you let go of jump?" )]
    // public bool variableJumpHeight;
    // [ShowIf( "jumpSettingsType", eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED )]
    // [SerializeField, Range( 1f, 10f )]
    // [Tooltip( "Gravity multiplier when you let go of jump" )]
    // public float jumpCutOff;




    private void OnValidate() {
        switch ( jumpSettingsType ) {
        case eJumpSettingsType.CGSK_Time_DEPRECATED:
            CalculateDerivedJumpValues_Time();
            break;

        case eJumpSettingsType.CGSK_Height_and_Distance:
            CalculateDerivedJumpValues_Distance();
            break;

        case eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED:
            CalculateDerivedJumpValues_GMTK();
            break;
        }

        CalculateJumpLine();
    }


    static private int       jumpLineResolution = 64; // NOTE: This must be a positive even number
    internal       Vector3[] jumpLinePoints;
    internal List<Vector3> minJumpLinePoints;
    // [HideIf( "jumpSettingsType", eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED )]
    internal Vector3[] jumpStartMidEndPoints, minJumpStartMidEndPoints;
    internal Vector2   minTimeApexFull;

    internal void CalculateJumpLine() {
        if ( jumpSettingsType == eJumpSettingsType.GMTK_GameMakersToolKit_DEPRECATED ) {
            jumpLinePoints = null;
            return;
        }
        maxJumpDistHeight = Vector2.zero;
        Vector3 acc = new Vector3( 0, jumpGrav.up, 0 );
        Vector3 newAcc = acc;
        Vector3 p = Vector3.zero;
        jumpLinePoints = new Vector3[jumpLineResolution];
        jumpStartMidEndPoints = new Vector3[3];
        jumpLinePoints[0] = p;
        jumpStartMidEndPoints[0] = p;
        Vector3 v = new Vector3( maxSpeed, jumpVel.up, 0 );
        int numSteps = jumpLineResolution / 2;
        // Jumping Up
        float timeStepUp = jumpDuration.up / (float) numSteps;
        int i = 1;
        for ( ; i <= jumpLineResolution / 2; i++ ) {
            SimplifiedVelocityVerletIntegration( ref p, ref v, acc, timeStepUp );
            // p.x += v.x         * timeStep;
            // v.y += jumpGrav.up * timeStep;
            // p.y += v.y         * timeStep;
            jumpLinePoints[i] = p;
        }
        jumpStartMidEndPoints[1] = p;
        maxJumpDistHeight.y = p.y;
        // Jumping Down
        acc.y = jumpGrav.down;
        float timeStepDown = jumpDuration.down / (float) ( numSteps - 1 );
        for ( ; i < jumpLineResolution; i++ ) {
            SimplifiedVelocityVerletIntegration( ref p, ref v, acc, timeStepDown );
            // p.x += v.x           * timeStep;
            // v.y += jumpGrav.down * timeStep;
            // p.y += v.y           * timeStep;
            jumpLinePoints[i] = p;
        }
        jumpStartMidEndPoints[2] = p;
        maxJumpDistHeight.x = p.x;

        // Calculate jump line if jump button is released immediately
        if ( !jumpSettingsVariableHeight.useVariableJumpHeight ) {
            minJumpLinePoints = null;
            return;
        }
        minJumpDistHeight = Vector2.zero;
        minTimeApexFull = Vector2.zero;
        v = new Vector3( maxSpeed, jumpVel.up, 0 );
        newAcc = acc = new Vector3( 0, jumpGrav.up, 0 );
        p = Vector3.zero;
        minJumpLinePoints = new List<Vector3>();
        minJumpStartMidEndPoints = new Vector3[3];
        minJumpLinePoints.Add( p );
        minJumpStartMidEndPoints[0] = p;
        // We'll use timeStepUp, the timeStep from jumpDuration.up
        i = 0;
        float time = 0;
        int minJumpPhase = 0; // 0=up and button held, 1=button released, 2=down 
        Vector3 debugAcc = new Vector3(acc.y, -1, -1);
        float minTimeStep = 0.01f;
        for ( ; time < 10; i++ ) { // time<10 or i<jumpLineResolution to keep it from getting out of hand
            time += minTimeStep;
            if ( minJumpPhase == 0 ) { // Up and button held
                if ( v.y <= 0 || time >= jumpSettingsVariableHeight.minJumpButtonHeldTime ) {
                    if ( v.y <= 0 || jumpSettingsVariableHeight.upwardVelocityZeroing ) {
                        v.y = 0;
                        newAcc.y = jumpGrav.down;
                        debugAcc.z = newAcc.y;
                        minJumpStartMidEndPoints[1] = p;
                        minJumpPhase = 2;
                        minTimeApexFull.x = time;
                    } else {
                        newAcc.y = jumpGrav.up * jumpSettingsVariableHeight.gravUpMultiplierOnRelease;
                        debugAcc.y = newAcc.y;
                        minJumpPhase = 1;
                    }
                }
            } else if ( minJumpPhase == 1 ) { // Still up, but button has been released
                if ( v.y <= 0 ) { // We're starting down
                    v.y = 0;
                    newAcc.y = jumpGrav.down;
                    debugAcc.z = newAcc.y;
                    minJumpStartMidEndPoints[1] = p;
                    minJumpDistHeight.y = p.y;
                    minJumpPhase = 2;
                    minTimeApexFull.x = time;
                }
            } else { // minJumpPhase == 2 // Moving down
                if ( p.y < 0 ) break; // This shouldn't ever happen because it should be caught after last SVVI call in minJumpPhase 2
            }
            
            VelocityVerletIntegration( ref p, ref v, ref acc, newAcc, minTimeStep );
            if ( p.y < 0 ) {
                p.y = 0; // This is a fudging of the numbers, but it should be ok. - JGB 2023-03-12
                minJumpStartMidEndPoints[2] = p;
                minJumpLinePoints.Add( p );
                minJumpDistHeight.x = p.x;
                minTimeApexFull.y = time;
                break;
            }
            minJumpLinePoints.Add( p );
        }
        if (DEBUG_JUMP_LINE_CALCULATION) Debug.LogWarning(
            $"gUMOR: {jumpSettingsVariableHeight.gravUpMultiplierOnRelease:0.##}"
            + $"\tp0acc: {debugAcc.x:#,0.##}"
            + $"\tp1acc: {debugAcc.y:#,0.##}"
            + $"\tp2acc: {debugAcc.z:#,0.##}");

    }

    // NOTE: Simplified Velocity Verlet Integration from Math for Game Programmers: Building a Better Jump
    // https://www.youtube.com/watch?v=hG9SzQxaCm8&t=23m2s
    void SimplifiedVelocityVerletIntegration( ref Vector3 pos, ref Vector3 vel, Vector3 acc,
                                              float deltaTime ) {
        pos += vel * deltaTime +
               acc * ( 0.5f * deltaTime * deltaTime ); // pos += vel*dT + 1/2*acc*dT*dT
        vel += acc * deltaTime;
    }

    void VelocityVerletIntegration( ref Vector3 pos, ref Vector3 vel, ref Vector3 acc,
                                    Vector3 newAcc, float deltaTime ) {
        pos += vel * deltaTime +
               acc * ( 0.5f * deltaTime * deltaTime ); // pos += vel*dT + 1/2*acc*dT*dT
        vel += (acc + newAcc) * (0.5f * deltaTime);
        acc = newAcc;
    }

    public float scale( float OldMin, float OldMax, float NewMin, float NewMax, float OldValue ) {
        float OldRange = ( OldMax - OldMin );
        float NewRange = ( NewMax - NewMin );
        float NewValue = ( ( ( OldValue - OldMin ) * NewRange ) / OldRange ) + NewMin;

        return ( NewValue );
    }


    [System.Serializable]
    public class CGSK_JumpSettings_Time {
        // [Header( "Classic Game Starter Kit - Time Jump Settings" )]
        [Tooltip( "The full duration of the shortest jump possible (by tapping the button)" )]
        public float fullJumpDurationMin = 0.5f;
        [Tooltip( "The full duration of the longest jump possible (by holding the button)" )]
        public float fullJumpDurationMax = 1f;
        [Tooltip( "The fraction of the jump that is going up" )]
        [Range( 0.05f, 0.95f )]
        public float jumpApexFraction = 0.6f;
    }

    private void CalculateDerivedJumpValues_Time() {
        jumpDuration.up = jumpSettingsTime.fullJumpDurationMax * jumpSettingsTime.jumpApexFraction;
        jumpDuration.down = jumpSettingsTime.fullJumpDurationMax - jumpDuration.up;
        jumpVel.up = jumpHeight * 2 / jumpDuration.up;
        jumpVel.down =
            jumpHeight * 2 /
            jumpDuration.down; // This is the velocity when the character lands. - GB 2023-03-10
        jumpGrav.up = -2   * jumpHeight   / ( jumpDuration.up   * jumpDuration.up );
        jumpGrav.down = -2 * jumpHeight   / ( jumpDuration.down * jumpDuration.down );
        jumpDist.up = jumpDuration.up     * maxSpeed;
        jumpDist.down = jumpDuration.down * maxSpeed;
        
        jumpSettingsVariableHeight = jumpSettingsVariableHeightCGSK;
    }

    [System.Serializable]
    public class CGSK_JumpSettings_Distance {
        // [Header( "Classic Game Starter Kit - Distance Jump Settings" )]
        [Tooltip( "The horizontal distance at full run speed of the shortest jump possible (by tapping the button)" )]
        public float fullJumpDistanceMin = 0.5f;
        [Tooltip( "The horizontal distance at full run speed of the longest jump possible (by holding the button)" )]
        public float fullJumpDistanceMax = 1f;
        [Tooltip( "The fraction of the jump that is going up" )]
        [Range( 0.05f, 0.95f )]
        public float jumpApexFraction = 0.6f;
    }

    private void CalculateDerivedJumpValues_Distance() {
        jumpDist.up = jumpSettingsDistance.fullJumpDistanceMax *
                      jumpSettingsDistance.jumpApexFraction;
        jumpDist.down = jumpSettingsDistance.fullJumpDistanceMax - jumpDist.up;
        // Th = Xh / Vh
        jumpDuration.up = jumpDist.up     / maxSpeed;
        jumpDuration.down = jumpDist.down / maxSpeed;
        // Vy = 2hVh / Xh
        jumpVel.up = 2   * jumpHeight * maxSpeed / jumpDist.up;
        jumpVel.down = 2 * jumpHeight * maxSpeed / jumpDist.down;
        // G = -2h(Vx*Vx) / (Xh*Xh)
        jumpGrav.up = -2 * jumpHeight * ( maxSpeed * maxSpeed ) / ( jumpDist.up * jumpDist.up );
        jumpGrav.down = -2 * jumpHeight * ( maxSpeed * maxSpeed ) /
                        ( jumpDist.down * jumpDist.down );
        
        jumpSettingsVariableHeight = jumpSettingsVariableHeightCGSK;
    }

    [System.Serializable]
    public class CGSK_JumpSettings_VariableHeight {
        // [Header( "Classic Game Starter Kit - Variable Jump Height" )]

        [Tooltip("Should the character jump differently based on how long the jump button is held?")]
        public bool useVariableJumpHeight = true;
        [Tooltip( "Should upward velocity be set to 0 when the jump button is released? (Like in Metroid for NES)" )]
        public bool upwardVelocityZeroing = false;
        [Tooltip( "The minimum amount of time that the jump button will be forced to be held" +
            " Set this to 0.1f if you want to ensure that the player can't release the button before 0.1 seconds have passed." +
            " 0.05f is the default value because 100ms is a typical shortest time for a button to be held." )]
        [Range( 0.05f, 2f )]
        public float minJumpButtonHeldTime = 0.05f; // 100ms is a typical shortest time for a button to be held.;
        [Tooltip( "The multiplier applied to jumpGrav.up to slow upward velocity faster after the jump button has been released." +
                  "\nIf this is set to 1 and upwardVelocityZeroing=false, then it is the same as useVariableJumpHeight=false." +
                  "\nIf this were extremely high, it would similar to upwardVelocityZeroing=true." )]
        [Range(1,20)]
        public float gravUpMultiplierOnRelease = 1;
    }

    // NOTE: CGSK jummp math comes from Math for Game Programmers: Building a Better Jump
    // https://www.youtube.com/watch?v=hG9SzQxaCm8&t=9m35s & https://www.youtube.com/watch?v=hG9SzQxaCm8&t=784s
    // Th = Xh/Vx     V0 = 2H / Th     G = -2H / (Th * Th)     V0 = 2HVx / Xh     G = -2H(Vx*Vx) / (Xh*Xh) 
    
    [System.Serializable]
    public class GMTK_JumpSettings {
        // [Header( "Jump Settings - GameMakers ToolKit" )]
        [SerializeField, Range( 1f, 10f )]
        [Tooltip( "This number is converted from the rather meaningless [1..10] to a time to jump apex of [0.2sec..1.25sec]" )]
        public float jumpDuration = 5;
        [SerializeField, Range( 1f, 10f )]
        [Tooltip( "Gravity multiplier to apply when coming down" )]
        public float downGravity = 6.17f;
        public bool doubleJump = false;
        [Tooltip( "Should the character drop when you let go of jump?" )]
        public bool variableJumpHeight;
        [SerializeField, Range( 1f, 10f )]
        [ShowIf("useVariableJumpHeight")]
        [Tooltip( "Gravity multiplier when you let go of jump and character is still moving up" )]
        public float jumpCutOff;
    }

    void CalculateDerivedJumpValues_GMTK() {
        // Jump Duration up is set by the [1..10] value from the GMTK app 
        jumpDuration.up = scale( 1, 10, 0.2f, 1.25f, jumpSettingsGMTK.jumpDuration );
        // These are the only derived values where the initial gravity is based on Physics2D.gravity - JGB 2023-03-12
        jumpGrav.up = Physics2D.gravity.y;
        // downGravity is a multiplier on the up gravity
        jumpGrav.down = jumpGrav.up * jumpSettingsGMTK.downGravity;
        // Calculate jumpDuration.down from G = -2H / (Th * Th), which solves for Th to Th = √(-2H / G)
        jumpDuration.down = Mathf.Sqrt( -2 * jumpHeight / jumpGrav.down );
        // Calculate jumpVel from V = 2H / Th
        jumpVel.up = 2   * jumpHeight / jumpDuration.up;
        jumpVel.down = 2 * jumpHeight / jumpDuration.down;
        // Calculate jumpDist from Th = Xh/Vx which is Vx = Xh/Th
        jumpDist.up = maxSpeed   / jumpDuration.up;
        jumpDist.down = maxSpeed / jumpDuration.down;

        // Set double jump
        jumpsBetweenGrounding = jumpSettingsGMTK.doubleJump ? 2 : 1;

        jumpSettingsVariableHeightGMTK.useVariableJumpHeight = jumpSettingsGMTK.variableJumpHeight;
        jumpSettingsVariableHeightGMTK.gravUpMultiplierOnRelease = jumpSettingsGMTK.jumpCutOff;
        jumpSettingsVariableHeightGMTK.upwardVelocityZeroing = false;
        jumpSettingsVariableHeightGMTK.minJumpButtonHeldTime = 0.05f; // 100ms is a typical shortest time for a button to be held.
        
        jumpSettingsVariableHeight = jumpSettingsVariableHeightGMTK;
    }


    [System.Serializable]
    public class CGSK_ColliderSettings {
        public InfoProperty info = new InfoProperty("Capsule Collider Settings Info",
            "CapsuleCollider2D size and offset are set automatically from height and width here." +
            "\nAlso set grounding raycasts info here.",
            false, false);
        public float height = 2;
        public float width              = 1f;
        [Tooltip("CharacterGround raycasts downward center, left, and right. The distance between left and right is groundRaycastWidth.")]
        public float groundRaycastWidth = 0.5f;
        [Tooltip("When raycasting downward to check grounding, the raycast begins at groundRaycastDepth above the ground and extends groundRaycastDepth into the ground.")]
        public float groundRaycastDepth = 0.1f;
        [Tooltip("Which layers are read as the ground")] public LayerMask groundLayers;
    }

}

[System.Serializable]
public class CSSO_FloatUpDown {
    public float up, down;
}

#if UNITY_EDITOR
[CustomPropertyDrawer( typeof( CSSO_FloatUpDown ) )]
public class CSSO_FloatUpDown_Drawer : PropertyDrawer {
    static public GUIStyle styleLabelGray = null, styleLabelGrayBold = null; 
    // SerializedProperty m_stat;

    // Draw the property inside the given rect
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
        // Init the SerializedProperty fields
        //if ( m_show == null ) m_show = property.FindPropertyRelative( "show" );
        //if ( m_recNum == null ) m_recNum = property.FindPropertyRelative( "recNum" );
        //if ( m_playerName == null ) m_playerName = property.FindPropertyRelative( "playerName" );
        //if ( m_dateTime == null ) m_dateTime = property.FindPropertyRelative( "dateTime" );

        CSSO_FloatUpDown fud = fieldInfo.GetValue( property.serializedObject.targetObject ) as CSSO_FloatUpDown;

        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty( position, label, property );

        // Draw label
        //position = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), GUIContent.none );// label );
        if ( styleLabelGray == null) {
            styleLabelGray = new GUIStyle( EditorStyles.label );
            styleLabelGray.richText = true;
        }

        string colorString = "#606060ff";

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 1;

        EditorGUI.LabelField(position, $"<b><color={colorString}>{property.displayName}</color></b>", styleLabelGray );
        EditorGUI.indentLevel = 8;
        EditorGUI.LabelField( position, $"<color={colorString}>up: {fud.up:0.0###}</color>", styleLabelGray );
        EditorGUI.indentLevel = 14;
        EditorGUI.LabelField( position, $"<color={colorString}>down: {fud.down:0.0###}</color>", styleLabelGray );

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}

// This was a bad idea because the CSSO doesn't know who the character is. Moved to CharacterJump
// [CustomEditor( typeof(Character_Settings_SO) )]
// public class CSSO_Editor : Editor {
//     private Character_Settings_SO csso;
//
//     private void OnEnable() {
//         csso = (Character_Settings_SO) target;
//     }
//
//
//     private void OnSceneGUI() {
//         if ( csso == null || csso.jumpLinePoints == null ) return;
//         
//         // Handles.matrix = Matrix4x4.Translate(); // Not needed because jump will be shown at origin.
//         Handles.color = Color.green;
//         Handles.DrawAAPolyLine(4, csso.jumpLinePoints);
//     }
// }


#endif
