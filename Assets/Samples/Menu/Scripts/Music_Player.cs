using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Music_Player : MonoBehaviour {
	[Tooltip("_audioSource defines the Audio Source component in this scene.")]
	AudioSource _audioSource;
	[Tooltip("_audioTracks defines the audio clips to be played continuously through out the scene.")]
	public AudioClip[] _audioTracks;
	[Space(20)]
	[HeaderAttribute("Music Player Options")]
	[Tooltip("_playTracks acts as the Play/Stop function of the Music Player")]
	public bool _playTracks;
	[Tooltip("Skips to the next available _audioTracks clip.")]
	public bool _nextTrack;
	[Tooltip("Skips to the previous _audioTracks clip")]
	public bool _prevTrack;
	[Tooltip("Loops the current _audioTracks clip.")]
	public bool _loopTrack;
	[Space(20)]
	[HeaderAttribute("Debugging/ReadOnly")]
	[Tooltip("_playTracks is a ReadOnly variable that displays the current _audioTracks clip that is playing")]
	public int _playingTrack;
	[Tooltip("_isMute returns the status of muting function in the Sound_Controller.")]
	public bool _isMute = false;
	
	void Awake () {		
		_audioSource = GetComponent<AudioSource>();
		if (_audioTracks != null && _audioTracks.Length > 0 && _audioTracks[0] != null)
		{
			_audioSource.clip = _audioTracks[0];
		}
		_audioSource.loop = true;
		_playingTrack = 0;

		int musicPref = PlayerPrefs.GetInt("MusicOn", 1);
		_isMute = (musicPref == 0);
		_audioSource.mute = _isMute;

		if (!_audioSource.isPlaying)
		{
			_audioSource.Play();
		}
	}
	
	void Update () {
		int musicPref = PlayerPrefs.GetInt("MusicOn", 1);
		_isMute = (musicPref == 0);
		_audioSource.mute = _isMute;

		if(!_playTracks)
		{
			if (_audioSource.isPlaying) _audioSource.Pause();
		}
		else
		{
			if (!_audioSource.isPlaying) _audioSource.Play();
		}

		if(_nextTrack){ NextTrack(); }
		if(_prevTrack){ PreviousTrack(); }
	}
	
	public void StartPlayer(){
		if(!_loopTrack) { //If the Audio Source is not set to loop the clip we will play the next clip.
			NextTrack();
		} else { //If Audio Source is set to loop we will play the same track again.
			_audioSource.Play(); 
		}
	}
	
	public void NextTrack(){
		_nextTrack = false;
		_audioSource.Stop();
			int _newCount = _playingTrack+1; //Find the next track.
			
				if(_newCount > _audioTracks.Length-1) { //Loop to beginning of _audioTracks. Prevents Array Index out of range errors.
					_audioSource.clip = _audioTracks[0]; _playingTrack = 0;
				} else {
					_audioSource.clip = _audioTracks[_newCount];_playingTrack = _newCount;
				} 
				_audioSource.Play();
				Debug.Log("Called NextTrack: _next="+_newCount+" : _playing="+_playingTrack +" : _name= "+_audioTracks[_playingTrack].name);
		
	}
	
	public void PreviousTrack(){
		_prevTrack = false;
		_audioSource.Stop();
			int _newCount = _playingTrack-1; //Find the previous track
			
				if(_newCount < 0) { //Loops to end of _audioTracks. Prevents Array Index out of range errors.
					_audioSource.clip = _audioTracks[_audioTracks.Length-1]; 
					_playingTrack = _audioTracks.Length-1;
				} else {
					_audioSource.clip = _audioTracks[_newCount];
					_playingTrack = _newCount;
				}
				_audioSource.Play();
				Debug.Log("Called PreviousTrack: _next="+_newCount+" : _playing="+_playingTrack+" : _name= "+_audioTracks[_playingTrack].name);
	}

}
