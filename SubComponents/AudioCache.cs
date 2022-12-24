using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using CustomMapUtility.Caching;
using System.IO;
using System.Threading.Tasks;
using uGuardian.WAV;
using uGuardian.Utilities;
#if !NOMP3
using NAudio.Wave;
#endif

namespace CustomMapUtility.Audio {
	internal sealed class AudioCache : FileCache<AudioClip> {
		protected override async Task<AudioClip> GetFile_Async_Internal(FileInfo file) {
			var fullName = file.FullName;
			var format = GetTypeOfFile(file);
			var name = file.Name;

			AudioClip clip = CheckCache(file);
			if (clip != null) {
				return clip;
			}

			using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($"file://{fullName}", format)) {
				var source = www.SendWebRequest().GetTaskCompletionSource();
				tasksInternal.Add(fullName, source);
				await source.Task;
				tasksInternal.Remove(fullName);

				if (www.isNetworkError) {
					throw new InvalidOperationException($"{name}: {www.error}");
				}
				clip = DownloadHandlerAudioClip.GetContent(www);
				clip.name = file.Name;
				if (clip != null) {
					return clip;
				}
				#if !NOMP3
				else if (format == AudioType.MPEG) {
					Debug.LogWarning("CustomMapUtility:AudioHandler: Falling back to NAudio and Custom WAV");
					clip = NAudio_Handler.Parse(fullName);
					return clip;
				}
				#endif
				else {
					throw new InvalidOperationException(name+": BGM Returned Null");
				}
			}
		}
		protected override AudioClip GetFile_Internal(FileInfo file, out UnityWebRequestAsyncOperation operation) {
			var fullName = file.FullName;
			var format = GetTypeOfFile(file);
			var name = file.Name;
			operation = null;

			AudioClip clip = CheckCache(file);
			if (clip != null) {
				return clip;
			}

			using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($"file://{fullName}", format)) {
				operation = www.SendWebRequest();
				operation.SpinWait();

				if (www.isNetworkError) {
					throw new InvalidOperationException($"{name}: {www.error}");
				}
				clip = DownloadHandlerAudioClip.GetContent(www);
				clip.name = file.Name;
				if (clip != null) {
					return clip;
				}
				else {
					throw new InvalidOperationException(name+": Image Returned Null");
				}
			}
		}
		public static AudioType GetTypeOfFile (FileInfo file) {
			switch (file.Extension.ToUpperInvariant()) {
				case ".WAVE":
				case ".WAV":
					return AudioType.WAV;
				case ".MP3":
				case ".MPEG":
					return AudioType.MPEG;
				case ".OGV":
				case ".OGA":
				case ".OGX":
				case ".OGM":
				case ".SPX":
				case ".OPUS":
				case ".OGG":
					return AudioType.OGGVORBIS;
				case ".MP4":
				case ".3GP":
				case ".M4B":
				case ".M4P":
				case ".M4R":
				case ".M4V":
				case ".M4A":
				case ".AAC":
					return AudioType.ACC;
				case ".AIF":
				case ".AIFC":
				case ".AIFF":
					return AudioType.AIFF;
				case ".IT":
					return AudioType.IT;
				case ".MOD":
					return AudioType.MOD;
				case ".S3M":
					return AudioType.S3M;
				case ".XM":
					return AudioType.XM;
				case ".XMA":
					return AudioType.XMA;
				case ".VAG":
					return AudioType.VAG;
				default:
					Debug.LogWarning("CustomMapUtility:AudioHandler: File type unknown");
					return AudioType.UNKNOWN;
			}
		}
		#if !NOMP3
		private static class NAudio_Handler {
			public static AudioClip Parse(string path) {
				WAV wav;
				using (var sourceProvider = new Mp3FileReader(path)) {
					MemoryStream stream = new MemoryStream();
					WaveFileWriter.WriteWavFileToStream(stream, sourceProvider);
					wav = new WAV(stream.ToArray());
					return ParseWAV(wav);
				}
			}
			public static AudioClip ParseWAV(string path) => ParseWAV(new WAV(path));
			public static AudioClip ParseWAV(WAV wav) {
				if (wav.NumChannels > 8) {
					throw new NotSupportedException("Unity does not support more than 8 audio channels per file");
				}
				var audioClip = AudioClip.Create("BGM", (int)wav.SampleCount, wav.NumChannels, (int)wav.SampleRate, stream: false);
				audioClip.SetData(wav.InterleavedAudio, 0);
				Debug.Log($"Parse Result: {wav}");
				return audioClip;
			}
		}
		#endif
		#region LoopAudio
		//TODO Add audio queue
		//TODO Allow ally themes
		public void PlayLoopPair(AudioClip clip, AudioClip loopClip, float overlap = 0, bool changeoverRaw = true, float changeover = 2) {
			float changeoverlap = changeoverRaw ? overlap - changeover : overlap * changeover;
			SingletonBehavior<BattleSoundManager>.Instance.ChangeCurrentTheme(false);
			var currentPlayingTheme = SingletonBehavior<BattleSoundManager>.Instance.CurrentPlayingTheme;
			currentPlayingTheme.Stop();
			if (loopSource == null) {
				loopSource = Instantiate(currentPlayingTheme, currentPlayingTheme.transform.parent);
				loopSource.name = "LoopSource";
			} else {
				DisableLoopSource();
				if (currentLoop != null) {
					StopCoroutine(currentLoop);
				}
				if (currentCheck != null) {
					StopCoroutine(currentCheck);
				}
			}
			currentPlayingTheme.clip = clip;
			loopSource.clip = loopClip;
			float delay = clip.length - (overlap - changeoverlap);
			double dspDelay = AudioSettings.dspTime+0.2;
			currentPlayingTheme.PlayScheduled(dspDelay);
			currentPlayingTheme.time = 0;
			double switchOver = dspDelay+delay;
			currentPlayingTheme.SetScheduledEndTime(switchOver);
			loopSource.PlayScheduled(switchOver);
			loopSource.time = changeoverlap;
			SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(new AudioClip[]{clip});
			float remainTime = loopClip.length - changeoverlap;
			float flipStart = (float)(switchOver + remainTime + flipStartDelay);
			currentCheck = StartCoroutine(ChangeCheck(clip, loopClip));
			currentLoop = StartCoroutine(LoopCheck(loopClip, delay, flipStart));
			if (overlap != 0 && (changeoverlap < flipStartDelay)) {
				Debug.LogWarning("CustomMapUtility:AudioHandler:LoopClip: Changeover value is probably too high");
			}
			/*
			Debug.LogWarning($"clip.length: {clip.length}");
			Debug.LogWarning($"loopClip.length: {loopClip.length}");
			Debug.LogWarning($"overlap: {overlap}");
			Debug.LogWarning($"changeover: {changeover}");
			Debug.LogWarning($"changeoverlap: {changeoverlap}");
			Debug.LogWarning($"delay: {delay}");
			Debug.LogWarning($"dspDelay: {dspDelay}");
			Debug.LogWarning($"switchOver: {switchOver}");
			Debug.LogWarning($"remainTime: {remainTime}");
			Debug.LogWarning($"flipStart: {flipStart}");
			*/
		}

		public IEnumerator ChangeCheck(AudioClip clip, AudioClip loopClip) {
			yield return new WaitWhile(() => SingletonBehavior<BattleSoundManager>.Instance.CurrentPlayingTheme.clip == clip || SingletonBehavior<BattleSoundManager>.Instance.CurrentPlayingTheme.clip == loopClip);
			DisableLoopSource();
			StopCoroutine(currentLoop);
		}
		public IEnumerator LoopCheck(AudioClip loopClip, float delay, double flipStart) {
			var currentPlayingTheme = SingletonBehavior<BattleSoundManager>.Instance.CurrentPlayingTheme;
			if (delay > 0) {
				yield return new WaitWhile(() => currentPlayingTheme.isPlaying);
			}
			while (AudioSettings.dspTime > flipStart) {
				flipStart += loopClip.length;
			}
			currentPlayingTheme.clip = loopClip;
			SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(new AudioClip[]{loopClip});
			currentPlayingTheme.PlayScheduled(flipStart);
			loopSource.SetScheduledEndTime(flipStart);
			currentPlayingTheme.time = flipStartDelay;
			if (delay > 0) {
				yield return new WaitWhile(() => loopSource.isPlaying);
			}
			StopCoroutine(currentCheck);
		}
		// This is a constant that should be as close to 0 as possible without making pops so it doesn't get in the way of base game music.
		const float flipStartDelay = 0.0625f;
		void LateUpdate() {
			if (LoopSource != null) {
				LoopSource.volume = SingletonBehavior<BattleSoundManager>.Instance.CurrentPlayingTheme.volume;
			}
		}
		private void DisableLoopSource() {
			if (loopSource == null) {return;}
			loopSource.time = 0;
			loopSource.Stop();
			loopSource.clip = null;
		}
		static AudioSource loopSource;
		public AudioSource LoopSource {get => loopSource;}
		static Coroutine currentLoop;
		static Coroutine currentCheck;
		#endregion
	}
}