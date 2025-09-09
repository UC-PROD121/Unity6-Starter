using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Add the #if and #endif lines below
#if UNITY_EDITOR
using UnityEditor;

namespace XnTools {
    public class XnTransformSnap : MonoBehaviour {

        /// <summary>
        /// Snaps the GameObject.transform to the grid.
        /// </summary>
        /// <param name="command"></param>
        [MenuItem("CONTEXT/Transform/Snap to Grid Pos", false, Int32.MaxValue)]
        static void SnapToGridPos(MenuCommand command)
        {
            Transform trans = (Transform)command.context;
            trans.localPosition = trans.localPosition.SnapToGrid();
        }
    }

    
    
    static public class XnTransformSnapExtensionMethods {
        /// <summary>
        /// Snaps v3 to the default snap settings of the UnityEditor.
        /// JGB 2022-10-06
        /// </summary>
        /// <param name="v3"></param>
        /// <returns></returns>
        static public Vector3 SnapToGrid(this Vector3 v3) {
            return v3.SnapToGrid(UnityEditor.EditorSnapSettings.move);
        }
        /// <summary>
        /// Snaps v3 to the gridSpacing that was passed in.
        /// JGB 2022-10-06
        /// </summary>
        /// <param name="v3"></param>
        /// <param name="gridSpacingV3"></param>
        static public Vector3 SnapToGrid(this Vector3 v3, Vector3 gridSpacingV3) {
            return Snapping.Snap(v3, gridSpacingV3);
        }

    }
}
#endif