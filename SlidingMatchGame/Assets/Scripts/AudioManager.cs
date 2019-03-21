using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour {
	public Sound[] fall;
	public Sound[] match;
	public Sound[] land;
	public Sound timerEnd;
	static bool soundOn = true;
	public Image soundIcon;
	public Sprite soundSpriteOn, soundSpriteOff;
	void Start(){
		foreach(Sound sound in fall){SetupSound(sound);}
		foreach(Sound sound in match){SetupSound(sound);}
		foreach(Sound sound in land){SetupSound(sound);}
		SetupSound(timerEnd);
	}

	void SetupSound(Sound sound){
		sound.source = gameObject.AddComponent<AudioSource>();
		sound.source.clip = sound.clip;
		sound.source.pitch = sound.pitch;
		sound.source.volume = sound.volume;
	}

	public void PlayFall(){
		// return;
	}
	public void PlayMatch(){
		if (!soundOn)
			return;
		int index = Mathf.FloorToInt(Random.value * (match.Length - 1));
		match[index].source.Play();
	}
	public void PlayLand(){
		if (!soundOn)
			return;
		int index = Mathf.FloorToInt(Random.value * (land.Length - 1));
		land[index].source.Play();
	}
	public void PlayTimer(){if (!soundOn)
			return;
		timerEnd.source.Play();
	}

	public void SoundFlip(){
		soundOn = !soundOn;
		if (soundOn)
			soundIcon.sprite = soundSpriteOn;
		else 
			soundIcon.sprite = soundSpriteOff;
	}
}
