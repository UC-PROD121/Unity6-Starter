using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using NaughtyAttributes;
// Comment out these two lines in SoundAndMusic.cs
using UnityEditor;
using UnityEngine.Serialization;
using XnTools;

// To make a new eAudioTrigger, just add it to the enum declaration below. - JGB
public enum eAudioTrigger { music, jump, land, dash, pickup, speak };

public class SoundAndMusic : MonoBehaviour {
    // This is a private Singleton (see http://gameprogrammingpatterns.com - JGB 2025-08-03
    static private SoundAndMusic                            _S;
    static public  Dictionary<eAudioTrigger, AudioPlayable> PlayablesDict;
    
    [SerializeField]
    private InfoProperty info = new InfoProperty( "Using the SoundAndMusic Component",
        "1. Add any audio clips you want to the Project's \"Sounds and Music\" folder as .wav, .mp3, or .ogg files.\n" +
        "2. Open <b>Audio Playables</b> below to add them to the game.\n" +
        "3. If you wish to make any more audio triggers, you need to add them to the eAudioTrigger enum declaration at the top of the SoundAndMusic.cs script.\n" +
        "4. On each Audio Playable, check <b>Show Extra Options</b> to set volume, pitch, looping, etc.\n\t<i>Note: The options still work, even if they are not shown in the Inspector.</i>\n" +
        "5. For examples of how to call SoundAndMusic.Play() in code, search for it in the Movement and Collision scripts.\n\n");

    public List<AudioPlayable> audioPlayables;
    
        
    // This is called when the GameObject of this Component is first created
    void Awake() {
        if ( _S != null && _S != this ) {
            if ( _S.gameObject != null ) Destroy( _S.gameObject );
            // I check above to see whether it is null because sometimes _S will still exist after its
            //  gameObject has already been destroyed. - JGB
        } 
        
        _S = this;
        BuildPlayablesDict();
    }

    // This is called when the GameObject of this Component is destroyed
    private void OnDestroy() {
        if ( _S == this ) {
            foreach ( AudioPlayable ap in PlayablesDict.Values ) {
                if ( ap.source != null ) {
                    Destroy( ap.source );
                    ap.source = null;
                }
            }
            PlayablesDict = null;
            _S = null;
        }
    }

    void BuildPlayablesDict() {
        PlayablesDict = new Dictionary<eAudioTrigger, AudioPlayable>();
        foreach (AudioPlayable ap in audioPlayables)
        {
            if ( PlayablesDict.ContainsKey( ap.trigger ) ) {
                Debug.LogError( $"SoundAndMusic has more than one sfx with the name {ap.trigger}. This is not allowed." );
                continue;
            }
            if ( ap.source == null ) {
                ap.source = gameObject.AddComponent<AudioSource>();
                ap.source.resource = ap.soundClip;
                if ( ap.playOnStart ) ap.Play();
            }
            PlayablesDict.Add( ap.trigger, ap );
        }
    }


    static public void Play( eAudioTrigger trigger, float pitchMultiplier = 1f ) {
        if ( !PlayablesDict.ContainsKey( trigger ) ) {
            Debug.LogWarning( $"SoundAndMusic.Play({trigger}) called, but no AudioPlayable with that trigger is set up.");
            return;
        }
        AudioPlayable aP = PlayablesDict[trigger];
        aP.Play(pitchMultiplier);
    }

    
    [System.Serializable]
    public class AudioPlayable {
        [Hidden]
        public string name;
        [OnValueChanged("UpdateNameCallback")][AllowNesting]
        public eAudioTrigger trigger;
        public AudioResource soundClip;
        [Hidden]
        public bool hasAlreadyResetExtraOptions = false;
        [OnValueChanged("ResetExtraOptionsIfNeeded")][AllowNesting]
        public bool showExtraOptions = false;
        
        [ShowIf("showExtraOptions")][AllowNesting]
        [Range(0,100)]
        public int volume = 100;
        [ShowIf("showExtraOptions")][AllowNesting]
        [Range(0,200)]
        public int pitch = 100;
        [ShowIf("showExtraOptions")][AllowNesting]
        public bool loop        = false;
        [ShowIf("showExtraOptions")][AllowNesting]
        public bool playOnStart = false;
        
        [NonSerialized]
        public AudioSource source;

        public void ResetExtraOptionsIfNeeded() {
            if ( hasAlreadyResetExtraOptions ) return;
            volume = 100;
            pitch = 100;
            loop = false;
            playOnStart = false;
            hasAlreadyResetExtraOptions = true;
        }

        public void Play(float pitchMultiplier = 1f) {
            if ( source == null ) {
                Debug.LogWarning($"Play called on AudioPlayable {trigger}, but no source was available to play the sound.");
                return;
            }
            ResetExtraOptionsIfNeeded();
            source.pitch = pitch * pitchMultiplier * 0.01f;
            source.volume = volume * 0.01f;
            source.resource = soundClip;
            source.loop = loop;
            source.Play();
        }
        
        
        private void UpdateNameCallback() {
            name = trigger.ToString().Prettify();
        }
        
        // public static string Prettify(this string s)
        // {
        //     return s.FirstCharacterToUpper().SplitWords(' ');
        // }
    }
}


