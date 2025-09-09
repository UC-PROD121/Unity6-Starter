using System.Collections;
using TMPro;
using UnityEngine;
using NaughtyAttributes;

public class PlayerTextTrigger : MonoBehaviour {
    public InfoProperty info = new InfoProperty( "Using the PlayerTextTrigger Component",
         "1. The <b>Circle Collider 2D</b> on this GameObject controls when this will appear.\n" +
         "2. You can swap out the <b>Sprite</b> on the <b>Sprite Renderer</b> to have different images (or no image).\n" +
         "3. The <b>Text (TMP)</b> grandchild of this GameObject controls the text that is displayed.\n\n");

    public bool makeSpeechSound = false;
    
    private Canvas _canvas;
    void Awake() {
        _canvas = GetComponentInChildren<Canvas>();
        _canvas.enabled = false;
    }
    
    
    // OnTriggerEnter2D is called by the Physics engine when the Player enters this Trigger Collider.
    // It is only called by the Player layer due to settings on the Circle Collider 2D
    void OnTriggerEnter2D( Collider2D coll ) {
        if ( coll.CompareTag( "Player" ) ) {
            _canvas.enabled = true;
            if ( makeSpeechSound ) StartCoroutine( RandomSpeaking() );
        }
    }
    
    // OnTriggerExit2D is called by the Physics engine when the Player leaves this Trigger Collider.
    // It is only called by the Player layer due to settings on the Circle Collider 2D
    void OnTriggerExit2D( Collider2D coll ) {
        if ( coll.CompareTag( "Player" ) ) {
            _canvas.enabled = false;
        }
    }

    [BoxGroup("Speaking Sounds")][ShowIf("makeSpeechSound")]
    public float speakDelay = 0.2f, speakDelayVariance = 0.5f;
    [BoxGroup("Speaking Sounds")][ShowIf("makeSpeechSound")][MinMaxSlider(0.1f,10f)]
    public Vector2 speakPitchMinMax = new Vector2( 0.5f, 2f );
    [BoxGroup("Speaking Sounds")][ShowIf("makeSpeechSound")]
    public int charsPerSpeakTone = 10;
    [BoxGroup("Speaking Sounds")][ShowIf("makeSpeechSound")]
    public eAudioTrigger speakAudioTrigger = eAudioTrigger.speak;

    float GetNumWithPercentVariance( float n, float percentVariance ) {
        float v = n * percentVariance;
        return n - v + Random.Range( 0, v );
    }
    
    IEnumerator RandomSpeaking() {
        if ( !makeSpeechSound ) yield break;
        // Figure out how long to speak
        TMP_Text tMP = GetComponentInChildren<TMP_Text>();
        if ( tMP == null ) yield break;
        int speakTime = tMP.text.Length / charsPerSpeakTone;
        for (int i = 0; i < speakTime; i++) {
            float pitchMult = speakPitchMinMax[0] + Random.Range( 0, speakPitchMinMax[1] - speakPitchMinMax[0] );
            SoundAndMusic.Play( speakAudioTrigger, pitchMult );
            yield return new WaitForSeconds( GetNumWithPercentVariance( speakDelay, speakDelayVariance ) );
        }
    }
}
