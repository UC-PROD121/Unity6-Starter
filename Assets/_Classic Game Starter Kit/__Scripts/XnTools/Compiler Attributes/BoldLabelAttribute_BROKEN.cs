// using UnityEngine;
// #if UNITY_EDITOR
// using UnityEditor;
// #endif
//
// namespace XnTools {
//     // Originally from https://assetstore.unity.com/packages/tools/physics/kinematic-character-controller-99131
//     /// <summary>
//     /// Causes a field to be shown grayed-out and uneditable.
//     /// However, the field *can* be edited when the Inspector is in Debug mode. 
//     /// </summary>
//     public class BoldLabelAttribute : PropertyAttribute { }
//
// #if UNITY_EDITOR
//     [CustomPropertyDrawer( typeof(BoldLabelAttribute) )]
//     public class BoldLabelPropertyDrawer : PropertyDrawer {
//         static public GUIStyle BOLD_LABEL_STYLE = null;
//         
//         public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
//             return EditorGUI.GetPropertyHeight( property, label, true );
//         }
//
//         public override void OnGUI( Rect position, SerializedProperty property,
//                                     GUIContent label ) {
//             if ( BOLD_LABEL_STYLE == null ) {
//                 BOLD_LABEL_STYLE = new GUIStyle();
//             }
//             GUIStyle storedLabelStyle = GUI.skin.label;
//             GUI.skin.label = BOLD_LABEL_STYLE;
//             EditorGUI.PropertyField( position, property, label, true );
//             GUI.skin.label = storedLabelStyle;
//         }
//     }
// #endif
// }
