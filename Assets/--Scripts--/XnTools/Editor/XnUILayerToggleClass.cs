using UnityEngine;
using UnityEditor;

namespace XnTools {
    /// <summary>
    /// Places a command in the Window menu to toggle the visibility of the UI layer in the SceneView. <para />
    /// This is a followup to: https://gamedev.stackexchange.com/a/200942/171479
    /// </summary>
    static public class XnUILayerToggleClass {
        [MenuItem("Window/UI Layer Toggle", false, 992414 )]
        static void UILayerToggle()
        {
            Tools.visibleLayers ^= 1 << LayerMask.NameToLayer("UI");
            SceneView.RepaintAll();
        }
    }
}
