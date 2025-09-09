using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using XnTools;

public class Xnput : MonoBehaviour {
    static public  bool  DEBUG             = false;
    static public  bool  DEBUG_EVERY_FRAME = false;
    static private Xnput _S;
    static private bool  _S_IsSet = false;

    public InfoProperty info = new InfoProperty( $"Xnput Instructions",
        "To check any of the buttons, us Xnput just like the old Unity Input class." +
        " The following are available:" +
        "\n\tXnput.GetButton()" +
        "\n\tXnput.GetButtonDown()" +
        "\n\tXnput.GetButtonUp()" +
        "\n\tXnput.GetAxisRaw()",
        true, false );

    public ButtonState up, down, left, right, b, a, start, select;
    public string      buttons;
    [HideInInspector]
    public Vector2 move, moveRaw, rightStick, rightStickRaw;

    [MinMaxSlider( -1, 1 )]
    [SerializeField]
    private Vector2 moveX, moveY, rightStickX, rightStickY;

    [Header( "Settings" )]
    [Tooltip(
        "Unlike Unity's Input.GetAxis, gravity and sensitivity affect joysticks and gamepad sticks in addition to keyboard input." )]
    public bool useGravityAndSensitivity = true;
    public float sensitivity = 3;
    public float gravity     = 3;
    [Tooltip(
        "If the current value is the opposite sign from the desired value, snap will jump to 0 before easing the input." )]
    public bool snap = true;
    [Range( 0, 1 )]
    public float deadZone = 0.1f;

    //public float h, v;
    public enum eAxis { horizontal, vertical, rightStickH, rightStickV };

    public float h => move.x;
    public float v => move.y;

    static public float H => _S_IsSet ? _S.h : 0;
    static public float V => _S_IsSet ? _S.v : 0;

    public float hRaw => moveRaw.x;
    public float vRaw => moveRaw.y;

    public float rsH => rightStick.x;
    public float rsV => rightStick.y;

    public float rsHRaw => rightStickRaw.x;
    public float rsVRaw => rightStickRaw.y;


    public enum eButton {
        up, down, left,
        right, b, a,
        start, select
    };

    public Dictionary<eButton, ButtonState> buttonDict;

    // Start is called before the first frame update
    void Awake() {
        if ( _S != null ) {
            Destroy( gameObject );
            return;
        }
        _S = this;
        _S_IsSet = true;

        buttonDict = new Dictionary<eButton, ButtonState>();
        buttonDict.Add( eButton.up, up );
        buttonDict.Add( eButton.down, down );
        buttonDict.Add( eButton.left, left );
        buttonDict.Add( eButton.right, right );
        buttonDict.Add( eButton.a, a );
        buttonDict.Add( eButton.b, b );
        buttonDict.Add( eButton.start, start );
        buttonDict.Add( eButton.select, select );
    }

    // LateUpdate is called once per frame after all Updates have completed
    void LateUpdate() {
        buttons =
            $"U:{up.Char} D:{down.Char} L:{left.Char} R:{right.Char} B:{b.Char} A:{a.Char} Se:{select.Char} St:{start.Char}";
        if ( DEBUG_EVERY_FRAME ) Debug.Log( buttons );
        // Progress all ButtonStates
        foreach ( ButtonState bs in buttonDict.Values ) {
            bs.Progress();
        }
        //up.Progress();
        //down.Progress();
        //left.Progress();
        //right.Progress();
        //a.Progress();
        //b.Progress();
        //start.Progress();
        //select.Progress();

        // Manage easing on move, h, and v values
        move = AxisEasing( move, moveRaw, Time.deltaTime );
        // Expose in Inspector
        moveX[0] = move.x;
        moveX[1] = moveRaw.x;
        moveY[0] = move.y;
        moveY[1] = moveRaw.y;
        if ( DEBUG ) Debug.Log( $"move: {move}" );


        rightStick = AxisEasing( rightStick, rightStickRaw, Time.deltaTime );
        // Expose in Inspector
        rightStickX[0] = rightStick.x;
        rightStickX[1] = rightStickRaw.x;
        rightStickY[0] = rightStick.y;
        rightStickY[1] = rightStickRaw.y;

        if ( DEBUG ) Debug.Log( $"rightStick: {rightStick}" );
    }


    public float AxisEasing( float curr, float target, float deltaTime ) {
        // If target is on opposite side of 0 from curr, snap to 0
        if ( target != 0 && curr != 0 ) {
            if ( Mathf.Sign( target ) != Mathf.Sign( curr ) ) {
                return 0;
            }
        }
        if ( useGravityAndSensitivity ) {
            float maxDelta;
            if ( Mathf.Abs( curr ) < Mathf.Abs( target ) ) {
                // target is further from 0 than curr, so use sensitivity
                maxDelta = sensitivity * deltaTime;
            } else {
                // target is closer to 0 than curr, so use gravity
                maxDelta = gravity * deltaTime;
            }
            return (Mathf.Abs( target - curr ) <= maxDelta) ? target : curr + Mathf.Sign( target - curr ) * maxDelta;
        }
        return target;
    }

    public Vector2 AxisEasing( Vector2 curr, Vector2 target, float deltaTime ) {
        curr.x = AxisEasing( curr.x, target.x, deltaTime );
        curr.y = AxisEasing( curr.y, target.y, deltaTime );
        return curr;
    }


#region Static Methods
    static public bool GetButton( eButton eB ) {
        return _S.buttonDict[eB];
    }

    static public bool GetButtonDown( eButton eB ) {
        return _S.buttonDict[eB].down;
    }

    static public bool GetButtonUp( eButton eB ) {
        return _S.buttonDict[eB].up;
    }

    static public float GetAxisRaw( eAxis axis ) {
        if ( axis == eAxis.horizontal ) return _S.hRaw;
        if ( axis == eAxis.vertical ) return _S.vRaw;
        if ( axis == eAxis.rightStickH ) return _S.rsHRaw;
        if ( axis == eAxis.rightStickV ) return _S.rsVRaw;
        return 0;
    }

    static public float GetAxis( eAxis axis ) {
        if ( axis == eAxis.horizontal ) return _S.h;
        if ( axis == eAxis.vertical ) return _S.v;
        if ( axis == eAxis.rightStickH ) return _S.rsH;
        if ( axis == eAxis.rightStickV ) return _S.rsV;
        return 0;
    }
#endregion


#region PlayerInput Functions

    private void OnMove( InputValue value ) {
        Vector2 moveOld = move;
        moveRaw = value.Get<Vector2>();
        if ( moveRaw.magnitude < deadZone ) moveRaw = Vector2.zero;
        if ( DEBUG ) Debug.Log( $"moveRaw: {moveRaw}" );
    }

    private void OnRightStick( InputValue value ) {
        Vector2 rsOld = rightStick;
        rightStickRaw = value.Get<Vector2>();
        if ( rightStickRaw.magnitude < deadZone ) rightStickRaw = Vector2.zero;
        if ( DEBUG ) Debug.Log( $"rightStickRaw: {rightStickRaw}" );
    }

    private void OnUp( InputValue value ) {
        up.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"up: {up}" );
        //float f = value.Get<float>();
        //if ( f > 0.5f ) up = true;
    }

    private void OnDown( InputValue value ) {
        down.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"down: {down}" );
    }

    private void OnLeft( InputValue value ) {
        left.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"left: {left}" );
    }

    private void OnRight( InputValue value ) {
        right.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"right: {right}" );
    }

    private void OnA( InputValue value ) {
        a.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"a: {a}" );
    }

    private void OnB( InputValue value ) {
        b.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"b: {b}" );
    }

    private void OnStart( InputValue value ) {
        start.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"start: {start}" );
    }

    private void OnSelect( InputValue value ) {
        select.Set( value.isPressed );
        if ( DEBUG ) Debug.Log( $"select: {select}" );
    }

#endregion
}

//public struct RockyCharacterInputs {
    //    public float MoveAxisForward;
    //    public float MoveAxisRight;
    //    public Quaternion CameraRotation;
    //    public ButtonState Jump, Crouch, Dive, NoClip;
    //    public int RespawnAt; // The default value for this is 0 for NO TELEPORT

    //    public void ProgressButtonStates() {
    //        Jump.Progress();
    //        Crouch.Progress();
    //        Dive.Progress();
    //        NoClip.Progress();
    //    }

    //    public override string ToString() {
    //        System.Text.StringBuilder sb = new System.Text.StringBuilder();
    //        sb.Append( $"Move: [{MoveAxisForward:0.00}, {MoveAxisRight:0.00}]" );
    //        sb.Append( $"  Jump: {Jump.ToString()}" );
    //        sb.Append( $"  Crch: {Crouch.ToString()}" );
    //        sb.Append( $"  NoCl: {NoClip.ToString()}" );
    //        sb.Append( $"  Spwn: {RespawnAt}" );
    //        // sb.Append( $"  Dive: {Dive.ToString()}" );
    //        sb.Append( $"  Rot: {CameraRotation.eulerAngles}" );
    //        return sb.ToString();
    //    }
    //}



#region XnPut2 Archive - This stores some code to pull InputActions from the PlayerInput class, but I'm not using it. - JGB 2025-02-10

/*
#define DEBUG_INPUT_ACTION_ENUMERATOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XnTools {
    [RequireComponent( typeof(PlayerInput) )]
    public class Xnput2 : MonoBehaviour {
        static public bool  DEBUG             = true;
        static public bool  DEBUG_EVERY_FRAME = false;
        static        Xnput2 _S;

        public InfoProperty info = new InfoProperty( $"Xnput Instructions",
            "To check any of the buttons, us Xnput just like the old Unity Input class." +
            " The following are available:" +
            "\n\tXnput.GetButton()" +
            "\n\tXnput.GetButtonDown()" +
            "\n\tXnput.GetButtonUp()" +
            "\n\tXnput.GetAxisRaw()",
            true, false );

        public List<XnButtonState> buttonStates;

        
        // public ButtonState up, down, left, right, b, a, start, select;
        public string      buttons;
        public Vector2     moveRaw;

        //public float h, v;
        public enum eAxis { horizontal, vertical };

        public float hRaw {
            get { return moveRaw.x; }
        }

        public float vRaw {
            get { return moveRaw.y; }
        }

        // public enum eButton {
        //     up, down, left,
        //     right, b, a,
        //     start, select
        // };

        public Dictionary<string, XnButtonState> buttonDict;

        public PlayerInput playerInput;

        public List<XnputActionCollection> inputMapActions;


        void OnValidate() { }
        
        protected internal void PullFromPlayerInput() {
            playerInput = GetComponent<PlayerInput>();
            InputActionAsset iaa = playerInput.actions;
            IEnumerator<InputAction> actionEnumerator = iaa.GetEnumerator();
            InputAction ia;
            buttonStates = new List<XnButtonState>();
            XnButtonState bSt;
            inputMapActions ??= new List<XnputActionCollection>();
            inputMapActions.Clear();

#if DEBUG_INPUT_ACTION_ENUMERATOR
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine( "___InputActions___" );
#endif
            while ( actionEnumerator.MoveNext() ) {
                ia = actionEnumerator.Current;
                if ( ia == null ) continue;
                
#if DEBUG_INPUT_ACTION_ENUMERATOR
                sb.AppendLine( $"{ia.actionMap.name}\t{ia.name}\t{ia.type}\t{ia.expectedControlType}\t{ia}" );
#endif
                string actionMapName = ia.actionMap.name;
                int actionMapIndex = -1;
                for (int i = 0; i < inputMapActions.Count; i++) {
                    if ( inputMapActions[i].actionMapName == actionMapName ) {
                        actionMapIndex = i;
                        break;
                    }
                }
                if ( actionMapIndex == -1 ) {
                    inputMapActions.Add( new XnputActionCollection() {actionMapName = actionMapName}  );
                    actionMapIndex = inputMapActions.Count - 1;
                }
                inputMapActions[actionMapIndex].AddInputAction( ia );
                
                
                if ( ia.type == InputActionType.Button ) {
                    bSt = new XnButtonState();
                    bSt.name = ia.name;
                    bSt.id = ia.id;
                    buttonStates.Add( bSt );
                }
                // Debug.Log( ia );
            }
#if DEBUG_INPUT_ACTION_ENUMERATOR
            Debug.Log( sb.ToString() );
#endif
            
            // Sort the list based on name
            buttonStates.Sort( ( a, b ) => string.Compare( a.name, b.name ) );
        }

        // Start is called before the first frame update
        void Awake() {
            if ( _S != null ) {
                Destroy( gameObject );
                return;
            }
            _S = this;
            playerInput ??= GetComponent<PlayerInput>(); // Only do this if plIn is null
            
            buttonDict = new Dictionary<string, XnButtonState>();
            foreach ( XnButtonState bs in buttonStates ) {
                buttonDict.Add( bs.name, bs );
            }
            
            playerInput.onActionTriggered += ActionTriggered;
            
            // buttonDict.Add( eButton.up, up );
            // buttonDict.Add( eButton.down, down );
            // buttonDict.Add( eButton.left, left );
            // buttonDict.Add( eButton.right, right );
            // buttonDict.Add( eButton.a, a );
            // buttonDict.Add( eButton.b, b );
            // buttonDict.Add( eButton.start, start );
            // buttonDict.Add( eButton.select, select );
        }

        void ActionTriggered( InputAction.CallbackContext context ) {
            InputAction action = context.action;
            string actionMapName = action.actionMap.name;
            string n = action.name;
            InputActionPhase phase = context.phase;
            if ( action.type == InputActionType.Button ) {
                XnButtonState bs;
                if ( buttonDict.TryGetValue( n, out bs ) ) {
                    switch ( phase ) {
                    case InputActionPhase.Started:
                        bs.state = XnButtonState.eInputButtonState.down;
                        break;
                    case InputActionPhase.Performed:
                        bs.state = XnButtonState.eInputButtonState.held;
                        break;
                    case InputActionPhase.Canceled:
                        bs.state = XnButtonState.eInputButtonState.up;
                        break;
                    default:
                        Debug.Log( $"Unexpected phase: {phase}" );
                        break;
                    }
                }
            }
        }

        // LateUpdate is called once per frame after all Updates have completed
        void LateUpdate() {
            // buttons = $"U:{up.Char} D:{down.Char} L:{left.Char} R:{right.Char} B:{b.Char} A:{a.Char} Se:{select.Char} St:{start.Char}";
            if ( DEBUG_EVERY_FRAME ) Debug.Log( buttons );
            // Progress all ButtonStates
            foreach ( XnButtonState bs in buttonDict.Values ) {
                bs.Progress();
            }
            //up.Progress();
            //down.Progress();
            //left.Progress();
            //right.Progress();
            //a.Progress();
            //b.Progress();
            //start.Progress();
            //select.Progress();
            // TODO: Manage easing on move, h, and v values
        }

        static public bool GetButton( string buttonName ) {
            XnButtonState bs;
            if ( _S.buttonDict.TryGetValue( buttonName, out bs ) ) {
                return bs;
            }
            Debug.LogError( $"No InputAction button named {buttonName} exists." );
            return false;
        }
        
        static public bool GetButtonDown( string buttonName ) {
            XnButtonState bs;
            if ( _S.buttonDict.TryGetValue( buttonName, out bs ) ) {
                return bs.down;
            }
            Debug.LogError( $"No InputAction button named {buttonName} exists." );
            return false;
        }
        
        static public bool GetButtonUp( string buttonName ) {
            XnButtonState bs;
            if ( _S.buttonDict.TryGetValue( buttonName, out bs ) ) {
                return bs.up;
            }
            Debug.LogError( $"No InputAction button named {buttonName} exists." );
            return false;
        }

        static public float GetAxisRaw( eAxis axis ) {
            if ( axis == eAxis.horizontal ) return _S.hRaw;
            if ( axis == eAxis.vertical ) return _S.vRaw;
            // Debug.LogError( $"Xnput does not have an axis named \"{axis}\"." );
            return 0;
        }


// #region PlayerInput Functions
//
//         private void OnMove( InputValue value ) {
//             moveRaw = value.Get<Vector2>();
//             if ( DEBUG ) Debug.Log( $"moveRaw: {moveRaw}" );
//         }
//
//         private void OnUp( InputValue value ) {
//             up.Set( value.isPressed );
//             if ( DEBUG ) Debug.Log( $"up: {up}" );
//             //float f = value.Get<float>();
//             //if ( f > 0.5f ) up = true;
//         }
//
//         private void OnDown( InputValue value ) {
//             down.Set( value.isPressed );
//             if ( DEBUG ) Debug.Log( $"down: {down}" );
//         }
//
//         private void OnLeft( InputValue value ) {
//             left.Set( value.isPressed );
//             if ( DEBUG ) Debug.Log( $"left: {left}" );
//         }
//
//         private void OnRight( InputValue value ) {
//             right.Set( value.isPressed );
//             if ( DEBUG ) Debug.Log( $"right: {right}" );
//         }
//
//         private void OnNESA( InputValue value ) {
//             a.Set( value.isPressed );
//             if ( DEBUG ) Debug.Log( $"a: {a}" );
//         }
//
//         private void OnNESB( InputValue value ) {
//             b.Set( value.isPressed );
//             if ( DEBUG ) Debug.Log( $"b: {b}" );
//         }
//
//         private void OnStart( InputValue value ) {
//             start.Set( value.isPressed );
//             if ( DEBUG ) Debug.Log( $"start: {start}" );
//         }
//
//         private void OnSelect( InputValue value ) {
//             select.Set( value.isPressed );
//             if ( DEBUG ) Debug.Log( $"select: {select}" );
//         }
//
// #endregion

    }
    
    
    
#if UNITY_EDITOR
    [CustomEditor( typeof(Xnput2) )]
    public class Xnput2Editor : Editor {
        public override void OnInspectorGUI() {
            Xnput2 xnput2 = (Xnput2) target;
            Init();
            // Start a code block to check for GUI changes
            EditorGUI.BeginChangeCheck();

            DrawDefaultInspector();
            if ( GUILayout.Button( "Pull InputActions from PlayerInput" ) ) {
                xnput2.PullFromPlayerInput();
            }

            EditorGUI.EndChangeCheck();
        }

        
        
        bool m_Initialized;

        GUIStyle LinkStyle {
            get { return m_LinkStyle; }
        }

        [SerializeField]
        GUIStyle m_LinkStyle;

        GUIStyle TitleStyle {
            get { return m_TitleStyle; }
        }

        [SerializeField]
        GUIStyle m_TitleStyle;

        GUIStyle SubTitleStyle {
            get { return m_SubTitleStyle; }
        }

        [SerializeField]
        GUIStyle m_SubTitleStyle;

        GUIStyle HeadingStyle {
            get { return m_HeadingStyle; }
        }

        [SerializeField]
        GUIStyle m_HeadingStyle;

        GUIStyle BodyStyle {
            get { return m_BodyStyle; }
        }

        [SerializeField]
        GUIStyle m_BodyStyle;

        void Init() {
            if ( m_Initialized )
                return;

            m_BodyStyle = new GUIStyle( EditorStyles.label );
            m_BodyStyle.wordWrap = true;
            m_BodyStyle.fontSize = 14;
            m_BodyStyle.richText = true;

            m_TitleStyle = new GUIStyle( m_BodyStyle );
            m_TitleStyle.fontSize = 26;
            m_TitleStyle.alignment = TextAnchor.MiddleCenter;

            m_SubTitleStyle = new GUIStyle( m_BodyStyle );
            m_SubTitleStyle.fontSize = 18;
            m_SubTitleStyle.alignment = TextAnchor.MiddleCenter;

            m_HeadingStyle = new GUIStyle( m_BodyStyle );
            m_HeadingStyle.fontSize = 18;

            m_LinkStyle = new GUIStyle( m_BodyStyle );
            m_LinkStyle.wordWrap = false;
            // Match selection color which works nicely for both light and dark skins
            m_LinkStyle.normal.textColor = new Color( 0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f );
            m_LinkStyle.stretchWidth = false;

            m_Initialized = true;
        }
        
    }
#endif
    
}
*/
#endregion