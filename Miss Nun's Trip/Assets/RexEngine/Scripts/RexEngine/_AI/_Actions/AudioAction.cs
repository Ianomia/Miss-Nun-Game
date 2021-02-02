using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class AudioAction:RexAIAction 
	{
		public MusicType musicType;
		public AudioClip audioClip;
		public float volume = 1.0f;
		public bool willLoop;

		public enum MusicType
		{
			PlaySoundEffect,
			PlayMusic,
			PauseMusic,
			ChangeMusicTrack,
			SetMusicVolume,
			SetMusicLooping
		}

		public override void Begin()
		{
			switch(musicType)
			{
				case MusicType.PlayMusic:
					RexSoundManager.Instance.Play();
					break;
				case MusicType.PauseMusic:
					RexSoundManager.Instance.Pause();
					break;
				case MusicType.ChangeMusicTrack:
					if(audioClip)
					{
						RexSoundManager.Instance.Play(audioClip);
					}
					break;
				case MusicType.SetMusicVolume:
					RexSoundManager.Instance.SetVolume(volume);
					break;
				case MusicType.SetMusicLooping:
					RexSoundManager.Instance.SetLoop(willLoop);
					break;
				case MusicType.PlaySoundEffect:
					if(aiRoutine.slots.controller.slots.actor && audioClip)
					{
						aiRoutine.slots.controller.slots.actor.PlaySoundIfOnCamera(audioClip);
					}
					break;
				default:
					RexSoundManager.Instance.Play();
					break;
			}

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);
			musicType = (MusicType)EditorGUILayout.EnumPopup("Music Type", musicType);

			if(musicType == MusicType.ChangeMusicTrack || musicType == MusicType.PlaySoundEffect)
			{
				string prefix = (musicType == MusicType.ChangeMusicTrack) ? "Music Track" : "Sound Effect";
				audioClip = EditorGUILayout.ObjectField(prefix, audioClip,  typeof(AudioClip), true) as AudioClip;
			}
			else if(musicType == MusicType.SetMusicVolume)
			{
				volume = EditorGUILayout.FloatField("Music Volume", volume);
				volume = Mathf.Clamp01(volume);
			}
			else if(musicType == MusicType.SetMusicLooping)
			{
				willLoop = EditorGUILayout.Toggle("Will Music Loop", willLoop);
			}
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.Audio;
		}

		public override string GetName()
		{
			return "Audio";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(AudioAction))]
	public class AudioActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

