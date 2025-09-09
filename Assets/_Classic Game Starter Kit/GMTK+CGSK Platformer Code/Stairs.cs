using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody2D))]
public class Stairs : MonoBehaviour {
    private SpriteRenderer    sRend;
    private CapsuleCollider2D capC;
    private Transform         childTrans;

    public enum eOrientation { vertical=0, diagonalLeft=45, diagonalRight=-45 };
    public eOrientation orientation = eOrientation.vertical;
    public int        tilesLength = 2;
    [XnTools.ReadOnly]
    public float actualLength;
    [XnTools.ReadOnly]
    public Vector3 p0, p1;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #if UNITY_EDITOR
    private void OnValidate() {
        UnityEditor.EditorApplication.delayCall += DelayedValidate;
    }

    private void DelayedValidate() {
        if ( this == null || gameObject == null || transform == null ) return;
        // Check for required child and components
        CheckChild();
        // Now that we know everything is good, proceed
        // Orient properly
        transform.localEulerAngles = new Vector3( 0, 0, (int)orientation );
        // And scale length (multiply by Sqrt(2) for diagonal orientations)
        actualLength = tilesLength * ( ( (int)orientation         % 90 == 0 ) ? 1 : 1.414f );
        Vector2 size = new Vector2( 1, actualLength + 1);
        capC.size = sRend.size = size;
        capC.offset = new Vector2( 0, (actualLength + 1)*0.5f );
        childTrans.localPosition = new Vector3( 0, -0.5f, 0 );

        p0 = transform.position;
        p1 = p0 + ( childTrans.up * actualLength );
    }

    void CheckChild() {
        // See if there is a child named "Capsule"
        if ( childTrans == null ) childTrans = transform.Find( "Capsule" );
        if ( childTrans == null ) {
            GameObject go = new GameObject( "Capsule", typeof(SpriteRenderer), typeof(CapsuleCollider2D) );
            Debug.LogError( "For the Stairs to work, they must have a child named Capsule. One has been added." );
            UnityEditor.EditorApplication.delayCall += DelayedValidate;
        }
        if ( sRend == null ) sRend = childTrans.GetComponent<SpriteRenderer>();
        if ( sRend == null ) {
            childTrans.gameObject.AddComponent<SpriteRenderer>();
            Debug.LogError( "For the Stairs to work, the child named Capsule must have a SpriteRenderer. One has been added." );
            UnityEditor.EditorApplication.delayCall += DelayedValidate;
        }
        if ( capC  == null ) capC = childTrans.GetComponent<CapsuleCollider2D>();
        if ( capC == null ) {
            childTrans.gameObject.AddComponent<CapsuleCollider2D>();
            Debug.LogError( "For the Stairs to work, the child named Capsule must have a CapsuleCollider2D. One has been added." );
            UnityEditor.EditorApplication.delayCall += DelayedValidate;
        }
    }
    #endif
}
