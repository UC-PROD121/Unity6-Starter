using System.Collections;
using UnityEngine;

public class FollowTarget : MonoBehaviour {
    public GameObject targetToFollow;

    private string  _name;
    private string  _tag;
    private bool    _hasTarget;
    
    [XnTools.ReadOnly][SerializeField]
    private Vector3 _relPos;

    void Start() {
        if ( targetToFollow != null ) {
            _name = targetToFollow.name;
            _tag = targetToFollow.tag;
            _relPos = transform.position - targetToFollow.transform.position;
            _hasTarget = true;
        }
    }

    // Update is called once per frame
    void Update() {
        if ( _hasTarget && targetToFollow == null ) { // This happens when the GameObject has been destroyed
            // Try to find a GameObject with the same name or tag
            GameObject go = GameObject.Find( _name );
            if ( go == null ) {
                go = GameObject.FindWithTag( _tag );
            }
            if ( go == null ) {
                // If we still haven't found anything, give up on following for a while
                _hasTarget = false;
                StartCoroutine( CheckAgainInASecond() );
                return;
            }
            targetToFollow = go;
        }

        if ( _hasTarget ) {
            transform.position = targetToFollow.transform.position + _relPos;
        }
    }

    IEnumerator CheckAgainInASecond() {
        yield return new WaitForSeconds( 1 ); // Wait for 1 second
        _hasTarget = true; // Then set _hasTarget = true to force Update() to search for a target again.
    }
}