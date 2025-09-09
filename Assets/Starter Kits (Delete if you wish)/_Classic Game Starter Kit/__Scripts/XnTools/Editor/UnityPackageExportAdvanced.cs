#define ENABLED
#if ENABLED
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;


namespace Prototools.UnityPackageExportAdvanced {

    public class UnityPackageExportAdvanced : EditorWindow {
        static System.Text.StringBuilder sbPaths = new System.Text.StringBuilder();
        static List<string> paths;
        bool advancedOptions = false;
        bool exportTagsAndLayers = false, exportInputManager = false, exportProjSettings = false;

        //static GUIStyle gsBold = new GUIStyle(), gsNormal = new GUIStyle();
        //static GUIStyle gsBox = new GUIStyle(), gsButtons = new GUIStyle();

        static string fileNameDefault = "[Your Name]";
        public string fileName;

        [MenuItem( "Window/UnityPackage Export Advanced", false, 992414 )]
        public static void OpenWindow() {
            string path;
            paths = new List<string>();
            if ( Selection.assetGUIDs.Length == 0 ) {
                ExportAssetsPopUp popUp = GetWindow( typeof( ExportAssetsPopUp ) ) as ExportAssetsPopUp;
                popUp.text = $"You must select the assets that you want to export.\n\nIn the Project pane, under the Assets folder, select any folders (and assets) that you want to export.";
                popUp.ShowPopup();
                return;
            }

            foreach ( string guid in Selection.assetGUIDs ) {
                path = AssetDatabase.GUIDToAssetPath( guid );
                paths.Add( path );
                string[] bits = path.Split( '/' );
                if ( UnityEngine.Windows.Directory.Exists( path ) ) {   // This is a folder
                    sbPaths.AppendLine( $"{bits[bits.Length - 1]}/" );
                } else {                                            // This is a file
                    sbPaths.AppendLine( $"{bits[bits.Length - 1]}" );
                }
            }

            Debug.Log( $"Selected Asset Paths to Export:\n{sbPaths.ToString()}" );

            // Set up GUIStyles and such
            SetGUIStyles();

            UnityPackageExportAdvanced window = GetWindow( typeof( UnityPackageExportAdvanced ) ) as UnityPackageExportAdvanced;
            window.titleContent = new GUIContent( "Export UnityPackage" );
            window.fileName = fileNameDefault;
            window.ShowUtility();
        }


        void OnGUI() {
            fileName = EditorGUILayout.TextField( "UnityPackage filename:", fileName );
            GUILayout.Space( 10 );

            GUILayout.Label( "Exporting the following…", STYLES["Bold"] );
            GUILayout.Box( sbPaths.ToString(), STYLES["Box"] );
            GUILayout.Space( 10 );


            bool altHeld = ( Event.current.modifiers == EventModifiers.Alt );
            if ( advancedOptions || altHeld ) {
                advancedOptions = EditorGUILayout.ToggleLeft( "Advanced options…", advancedOptions );
                if ( advancedOptions ) {
                    GUILayout.BeginVertical();
                    GUILayout.Label( "Include the following…", STYLES["Bold"] );
                    exportTagsAndLayers = EditorGUILayout.ToggleLeft( "Tags and Layers", exportTagsAndLayers );
                    exportInputManager = EditorGUILayout.ToggleLeft( "Input Manager", exportInputManager );
                    exportProjSettings = EditorGUILayout.ToggleLeft( "Project Settings", exportProjSettings );
                    GUILayout.EndVertical();
                }
                GUILayout.Space( 10 );
            }


            GUILayout.BeginHorizontal( STYLES["Button"] );
            if ( GUILayout.Button( "Abort" ) ) {
                Close();
            }
            GUILayout.Space( 16 );
            if ( GUILayout.Button( "OK" ) ) {
                if ( fileName == fileNameDefault ) {
                    ExportAssetsPopUp popUp = GetWindow( typeof( ExportAssetsPopUp ) ) as ExportAssetsPopUp;
                    popUp.text = $"You must select a filename other than\n\n\"{fileName}\"";
                    popUp.ShowPopup();
                } else {
                    Debug.Log( fileName );
                    ExportPackage();
                }
            }
            GUILayout.EndHorizontal();

            //this.Repaint();
        }

        void ExportPackage() {
            if ( exportTagsAndLayers ) paths.Add( "ProjectSettings/TagManager.asset" );
            if ( exportInputManager ) paths.Add( "ProjectSettings/InputManager.asset" );
            if ( exportProjSettings ) paths.Add( "ProjectSettings/ProjectSettings.asset" );
            //string[] projectContent = new string[] { "Assets", "ProjectSettings/TagManager.asset", "ProjectSettings/InputManager.asset", "ProjectSettings/ProjectSettings.asset" };

            string filePath = $"../{fileName}_{System.DateTime.Now.ToDateTimeStamp()}.unitypackage";
            AssetDatabase.ExportPackage( paths.ToArray(), filePath, ExportPackageOptions.Interactive | ExportPackageOptions.Recurse );// | ExportPackageOptions.IncludeDependencies );

            Debug.Log( "UnityPackage exported to: " + filePath );

            ExportAssetsPopUp popUp = GetWindow( typeof( ExportAssetsPopUp ) ) as ExportAssetsPopUp;
            popUp.text = $"UnityPackage exported to:\n\t{filePath}\n\n(it's right next to your Project folder)";
            popUp.ShowPopup();
            Close();
        }

        //[MenuItem( "Tools/PhraseAccept2" )]
        //public static void main() {
        //    EditorWindow window = GetWindow( typeof( ExportAssetsWindow ) );
        //    window.Show();
        //}


        static Dictionary<string, GUIStyle> STYLES = new Dictionary<string, GUIStyle>();
        static void SetGUIStyles() {
            GUIStyle gs;
            RectOffset roPadding = new RectOffset( 4, 4, 4, 4 );

            // Normal
            gs = new GUIStyle();
            gs.fontStyle = FontStyle.Normal;
            gs.padding = roPadding;
            AddStyle( "Normal", gs );

            // Bold
            gs = new GUIStyle();
            gs.fontStyle = FontStyle.Bold;
            gs.padding = roPadding;
            AddStyle( "Bold", gs );

            // Box
            gs = new GUIStyle();
            gs.normal.background = Texture2D.whiteTexture;
            gs.padding = roPadding;
            gs.margin = roPadding;
            AddStyle( "Box", gs );

            // Button
            gs = new GUIStyle();
            gs.fontStyle = FontStyle.Bold;
            gs.padding = new RectOffset( 16, 16, 4, 4 );
            AddStyle( "Button", gs );
        }

        static void AddStyle( string key, GUIStyle style ) {
            if ( STYLES.ContainsKey( key ) ) {  // If it exists already, update the style
                STYLES[key] = style;
            } else {  // Otherwise, add the style
                STYLES.Add( key, style );
            }
        }
    }


    public class ExportAssetsPopUp : EditorWindow {
        public string text;
        int w = 400, h = 400;

        void Awake() {
            maxSize = new Vector2( w, h );
            minSize = maxSize / 2;
            //this.CenterOnMainWin();
            position = new Rect( ( Screen.currentResolution.width - ( w / 2 ) ) / 2, ( Screen.currentResolution.height - ( h / 2 ) ) / 2, w, h );
            //Rect mainWindowRect = EditorGUIUtility.GetMainWindowPosition();
            //Rect pos = position;
            //pos.x -= pos.width / 2;
            //pos.y -= pos.height / 2;
            //position = pos;
        }

        void OnGUI() {
            GUIStyle style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.wordWrap = true;
            style.alignment = TextAnchor.UpperCenter;
            style.padding = new RectOffset( 4, 4, 4, 4 );
            GUIStyle gsButton = new GUIStyle( GUI.skin.button );
            //gsButton.alignment = TextAnchor.LowerCenter;
            gsButton.margin = new RectOffset( w / 4, w / 4, 4, 4 );
            //gsButton.fixedWidth = w / 4;

            GUILayout.Space( 10 );
            GUILayout.Label( text, style );
            GUILayout.Space( 10 );
            if ( GUILayout.Button( "OK", gsButton ) ) this.Close();
        }
    }

    
    //public static class EditorWindowExtensions {

    //    // From https://answers.unity.com/questions/960413/editor-window-how-to-center-a-window.html
    //    public static void CenterOnMainWin( this EditorWindow window ) {
    //        Rect main = EditorGUIUtility.GetMainWindowPosition();
    //        Rect pos = window.position;
    //        float centerWidth = ( main.width - pos.width ) * 0.5f;
    //        float centerHeight = ( main.height - pos.height ) * 0.5f;
    //        pos.x = main.x + centerWidth;
    //        pos.y = main.y + centerHeight;
    //        window.position = pos;
    //    }

    //    public static System.Type[] GetAllDerivedTypes( this System.AppDomain aAppDomain, System.Type aType ) {
    //        var result = new List<System.Type>();
    //        var assemblies = aAppDomain.GetAssemblies();
    //        foreach ( var assembly in assemblies ) {
    //            var types = assembly.GetTypes();
    //            foreach ( var type in types ) {
    //                if ( type.IsSubclassOf( aType ) )
    //                    result.Add( type );
    //            }
    //        }
    //        return result.ToArray();
    //    }

    //    public static Rect GetEditorMainWindowPos() {
    //        var containerWinType = System.AppDomain.CurrentDomain.GetAllDerivedTypes( typeof( ScriptableObject ) ).Where( t => t.Name == "ContainerWindow" ).FirstOrDefault();
    //        if ( containerWinType == null )
    //            throw new System.MissingMemberException( "Can't find internal type ContainerWindow. Maybe something has changed inside Unity" );
    //        var showModeField = containerWinType.GetField( "m_ShowMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
    //        var positionProperty = containerWinType.GetProperty( "position", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance );
    //        if ( showModeField == null || positionProperty == null )
    //            throw new System.MissingFieldException( "Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity" );
    //        var windows = Resources.FindObjectsOfTypeAll( containerWinType );
    //        foreach ( var win in windows ) {
    //            var showmode = (int) showModeField.GetValue( win );
    //            if ( showmode == 4 ) // main window
    //            {
    //                var pos = (Rect) positionProperty.GetValue( win, null );
    //                return pos;
    //            }
    //        }
    //        throw new System.NotSupportedException( "Can't find internal main window. Maybe something has changed inside Unity" );
    //    }

    //    //public static void CenterOn$$anonymous$$ainWin( this UnityEditor.EditorWindow aWin ) {
    //    //    var main = EditorGUIUtility.Get$$anonymous$$ainWindowPosition();
    //    //    var pos = aWin.position;
    //    //    float w = ( main.width - pos.width ) * 0.5f;
    //    //    float h = ( main.height - pos.height ) * 0.5f;
    //    //    pos.x = main.x + w;
    //    //    pos.y = main.y + h;
    //    //    aWin.position = pos;
    //    //}
    //}

    public static class DateTimeExtension {
        static public string ToTimeStamp( this System.DateTime time ) {
            string str = $"{time.Hour:00}{time.Minute:00}";
            return str;
        }

        static public string ToDateStamp( this System.DateTime time ) {
            string str = $"{time.Year:0000}-{time.Month:00}-{time.Day:00}";
            return str;
        }

        static public string ToDateTimeStamp( this System.DateTime time ) {
            return $"{time.ToDateStamp()}_{time.ToTimeStamp()}";
        }
    }
}
#endif