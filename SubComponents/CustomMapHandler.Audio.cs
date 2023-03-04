using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;
using Mod;
using CustomMapUtility.Audio;
using uGuardian.WAV;
#pragma warning disable MA0048, MA0016, MA0051

namespace CustomMapUtility
{
	public partial class CustomMapHandler {
		#region AUDIO
		internal AudioClip CustomBgmParse(string BGM) => CustomBgmParse(new string[] { BGM }, async: false)[0];
		internal AudioClip[] CustomBgmParse(params string[] BGMs) => CustomBgmParse(BGMs, async: false);
		internal AudioClip[] CustomBgmParse(string[] BGMs, bool async) {
			var files = container.GetStageBgmInfos(BGMs);
			if (files?.Count == 0) {
				foreach (var bgm in BGMs) {
					Debug.LogError($"CustomMapUtility:AudioHandler: {bgm} does not exist.");
				}
				return null;
			}
			var parsed = new Dictionary<string, AudioClip>(files.Count, StringComparer.OrdinalIgnoreCase);
			var index = new Dictionary<int, string>(files.Count);
			for (int i = 0; i < BGMs.Length; i++) {
				index.Add(i, BGMs[i]);
			}
			foreach (var entry in files) {
				AudioClip clip;
				var file = entry.Value;
				var fullName = file.FullName;
				var name = entry.Key;
				try {
					if (!async) {
						clip = AudioCache.GetFile(file);
						parsed[name] = clip;
					} else {
						_ = AudioCache.GetFile_Async(file);
					}
				} catch (Exception ex) {
					Debug.LogException(ex);
					parsed[name] = null;
				}
			}
			if (!async) {
				var output = new AudioClip[BGMs.Length];
				foreach (var i in index) {
					if (!parsed.TryGetValue(i.Value, out output[i.Key])) {
						Debug.LogError($"CustomMapUtility:AudioHandler: {i.Value} does not exist.");
					}
				}
				return output;
			} else {
				return null;
			}
		}
		internal void CustomBgmParseAsync(params string[] BGMs) => CustomBgmParse(BGMs, async: true);
		internal static AudioCache AudioCache {get {
			if (audioCache == null) {
				audioCache = SingletonBehavior<BattleScene>.Instance.gameObject.GetComponent<AudioCache>();
				if (audioCache == null) {
					audioCache = SingletonBehavior<BattleScene>.Instance.gameObject.AddComponent<AudioCache>();
				}
			}
			return audioCache;
		}}
		static AudioCache audioCache = null;
		// REVIEW Consider refactoring
		
		/// <summary>
		/// Sets the current EnemyTheme.
		/// </summary>
		/// <param name="bgmName">The name of the audio file (including extension).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		public void SetEnemyTheme(string bgmName, bool immediate = true) {
			LoadEnemyTheme(bgmName, out var theme);
			SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(new AudioClip[]{theme});
			if (immediate) {
				SingletonBehavior<BattleSoundManager>.Instance.ChangeEnemyTheme(0);
				Debug.Log($"CustomMapUtility:AudioHandler: Changed EnemyTheme to {bgmName} and enforced it");
			} else {
				Debug.Log($"CustomMapUtility:AudioHandler: Changed EnemyTheme to {bgmName}");
			}
		}
		/// <summary>
		/// Sets the current EnemyThemes.
		/// </summary>
		/// <param name="bgmNames">An array of audio file names (including extensions).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		public void SetEnemyTheme(string[] bgmNames, bool immediate = true) {
			LoadEnemyTheme(bgmNames, out var themes);
			SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(themes);
			if (immediate) {
				SingletonBehavior<BattleSoundManager>.Instance.ChangeEnemyTheme(0);
				Debug.Log($"CustomMapUtility:AudioHandler: Changed EnemyTheme to {bgmNames[0]} + others and enforced it");
			} else {
				Debug.Log($"CustomMapUtility:AudioHandler: Changed EnemyTheme to {bgmNames[0]} + others");
			}
		}
		/// <summary>
		/// Preloads a sound file to be used with other functions.
		/// </summary>
		/// <param name="bgmName">The name of the audio file (including extension).</param>
		public void LoadEnemyTheme(string bgmName) => CustomBgmParseAsync(bgmName);
		/// <summary>
		/// Preloads multiple sound files to be used with other functions.
		/// </summary>
		/// <param name="bgmNames">An array of audio file names (including extensions).</param>
		public void LoadEnemyTheme(string[] bgmNames) => GetAudioClip(bgmNames);
		/// <summary>
		/// Loads a sound file and outputs it as an AudioClip.
		/// </summary>
		/// <param name="bgmName">The name of the audio file (including extension).</param>
		/// <param name="clip">The loaded AudioClip</param>
		public void LoadEnemyTheme(string bgmName, out AudioClip clip) => clip = CustomBgmParse(bgmName);
		/// <summary>
		/// Loads multiple sound files and outputs it as an AudioClip array.
		/// </summary>
		/// <param name="bgmNames">An array of audio file names (including extensions).</param>
		/// <param name="clips">The loaded AudioClips</param>
		public void LoadEnemyTheme(string[] bgmNames, out AudioClip[] clips) => clips = CustomBgmParse(bgmNames);
		/// <summary>
		/// Sets the current EnemyTheme using a loaded AudioClip.
		/// </summary>
		/// <param name="bgmName">The name of the audio file (including extension).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		public void StartEnemyTheme(string bgmName, bool immediate = true) {
			var theme = GetAudioClip(bgmName);
			SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(new AudioClip[]{theme});
			if (immediate) {
				SingletonBehavior<BattleSoundManager>.Instance.ChangeEnemyTheme(0);
				Debug.Log($"CustomMapUtility:AudioHandler: Changed EnemyTheme to {bgmName} and enforced it");
			} else {
				Debug.Log($"CustomMapUtility:AudioHandler: Changed EnemyTheme to {bgmName}");
			}
		}
		/// <summary>
		/// Sets the current EnemyTheme using a loaded AudioClip array.
		/// </summary>
		/// <param name="bgmNames">An array of audio file names (including extensions).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		public void StartEnemyTheme(string[] bgmNames, bool immediate = true) {
			var themes = GetAudioClip(bgmNames);
			SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(themes);
			if (immediate) {
				SingletonBehavior<BattleSoundManager>.Instance.ChangeEnemyTheme(0);
				Debug.Log($"CustomMapUtility:AudioHandler: Changed EnemyTheme to {bgmNames[0]} + others and enforced it");
			} else {
				Debug.Log($"CustomMapUtility:AudioHandler: Changed EnemyTheme to {bgmNames[0]} + others");
			}
		}
		[Obsolete("Not ready yet. Implementation is buggy")]
		public AudioClip StartEnemyTheme_LoopPair(AudioClip clip, AudioClip loopClip, bool overlap = true, bool changeoverRaw = true, float changeover = 5) => StartEnemyTheme_LoopPair(clip, loopClip, overlap ? loopClip.length : 0, changeoverRaw, changeover);
		/// <param name="overlap">How far back from the end of the audio file the loop should start in seconds.</param>
		[Obsolete("Not ready yet. Implementation is buggy")]
		public AudioClip StartEnemyTheme_LoopPair(AudioClip clip, AudioClip loopClip, float overlap, bool changeoverRaw = true, float changeover = 5) {
			if (!changeoverRaw && (changeover < 0 || changeover > 1)) {
				Debug.LogError($"CustomMapUtility:AudioHandler: changeover isn't raw but not between 0 and 1, defaulting to 0.5f");
				changeover = 0.5f;
			} else if (changeoverRaw && changeover > overlap) {
				string error = $"CustomMapUtility:AudioHandler: changeover is raw but is greater than overlap length, defaulting to non-raw 0.5f";
				if (changeover == 5) {
					Debug.LogWarning(error);
				} else {
					Debug.LogError(error);
				}
				changeoverRaw = false;
				changeover = 0.5f;
			}
			if ((AudioCache.ActiveLoopSource?.clip == clip) || AudioCache.ActiveLoopSource?.clip == loopClip) {
				return SingletonBehavior<BattleSoundManager>.Instance.CurrentPlayingTheme.clip;
			}
			AudioCache.PlayLoopPair(clip, loopClip, overlap, changeoverRaw, changeover);
			return clip;
		}
		public class AudioLoopQueue {
			public void Init(AudioClip clip) {
				currentPlaying = clip;
				var loopSource = AudioCache.ActiveLoopSource;
				loopSource.Stop();
				loopSource.time = 0;
				loopSource.clip = clip;
				currentDspTime = AudioSettings.dspTime+0.2;
				loopSource.PlayScheduled(currentDspTime);
			}
			public void Play(AudioClip clip, bool force = false) {
				var loopSource = AudioCache.ActiveLoopSource;
				if (currentPlaying == null || force) {
					Init(clip);
				}
				Play();
			}
			public void Play() {
				var loopSource = AudioCache.ActiveLoopSource;
				if (currentPlaying != null) {
					loopSource.clip = currentPlaying;
					StartCallback.Invoke(this);
				}
			}
			public void Enqueue(Entry entry) {
				queue.Enqueue(entry);
				if (nextPlaying == null) {
					Next();
				}
			}

			public void Enqueue(AudioClip clip, SwitchMode switchMode = SwitchMode.CrossFade, float fadeOut = 0, float fadeIn = 0,
				bool lengthFormat = false, int changePeriodStart = 0, int changePeriodEnd = 0, int offset = 0) {
					throw new NotImplementedException("SamplesMode not ready yet");
					queue.Enqueue(new Entry(clip, switchMode, fadeOut, fadeIn, lengthFormat, changePeriodStart, changePeriodEnd));
					if (nextPlaying == null) {
						Next();
					}
			}

			public void Enqueue(AudioClip clip, SwitchMode switchMode = SwitchMode.CrossFade, float fadeOut = 0, float fadeIn = 0,
				bool lengthFormat = false, float changePeriodStart = 0, float changePeriodEnd = 0, float offset = 0) {
					queue.Enqueue(new Entry(clip, switchMode, fadeOut, fadeIn, lengthFormat, changePeriodStart, changePeriodEnd));
					if (nextPlaying == null) {
						Next();
					}
			}
			/// <summary>
			/// Removes the next entry without changing the song
			/// </summary>
			/// <returns>The removed entry</returns>
			public Entry CancelNext() {
				if (nextPlaying != null) {
					var temp = nextPlaying;
					nextPlaying = null;
					return temp;
				} else {
					return null;
				}
			}
			/// <summary>
			/// Removes the entry after the next without changing the song
			/// </summary>
			/// <returns>The removed entry</returns>
			public Entry Dequeue() {
				if (queue.Count > 0) {
					var next = queue.Dequeue();
					return queue.Dequeue();
				} else {
					nextPlaying = null;
					return null;
				}
			}
			public void Next() {
				if (queue.Count > 0) {
					var active = ActiveLoopSource;
					var inactive = InactiveLoopSource;
					var next = queue.Dequeue();
					inactive.clip = next.clip;
					switch (next.switchMode) {
						case SwitchMode.Clip when next.timeMode:
							nextDspEndTime = nextDspTime;
							throw new NotImplementedException();
						case SwitchMode.Clip:
							nextDspEndTime = nextDspTime;
							throw new NotImplementedException("SamplesMode not ready yet");

						case SwitchMode.CrossFade when next.timeMode:
							double catchUp = currentDspTime;
							catchUp += next.timePeriod.changePeriodStart;
							catchUp += currentPlaying.length;
							while (catchUp + next.timePeriod.changePeriodStart < AudioSettings.dspTime) {
								currentDspTime += currentPlaying.length;
								catchUp += currentPlaying.length;
							}
							nextDspTime = currentDspTime + next.timePeriod.changePeriodStart;
							nextDspEndTime = currentDspTime + next.timePeriod.changePeriodEnd;
							if (next.fadeIn == 0 && next.fadeOut == 0) {
								AudioCache.CrossFade(nextDspTime, nextDspEndTime);
							} else {
								throw new NotImplementedException();
							}
							break;
						case SwitchMode.CrossFade:
							throw new NotImplementedException("SamplesMode not ready yet");

						case SwitchMode.Overlap when next.timeMode:
							
							if (next.fadeIn == 0 && next.fadeOut == 0) {
							}
							break;
						case SwitchMode.Overlap:
							throw new NotImplementedException("SamplesMode not ready yet");
					}
					inactive.PlayScheduled(nextDspTime);
					active.SetScheduledEndTime(nextDspEndTime);
					nextPlaying = next;
				}
			}
			public Entry Peek() {
				if (queue.Count > 0) {
					return queue.Peek();
				} else {
					return null;
				}
			}

			public delegate void ResetEvent(AudioLoopQueue queue);
			public event ResetEvent ResetCallback;
			public void Reset() {
				var active = ActiveLoopSource;
				currentPos = active.time;
				currentPosSamples = active.timeSamples;
				InactiveLoopSource.Stop();
				AudioCache.StopCrossFades();
				ResetCallback.Invoke(this);
			}
			/// <remarks>
			/// Alias for Stop
			/// </remarks>
			public void Pause() => Stop();
			public void Stop() {
				Reset();
				AudioCache.DisableLoopSources();
			}
			public delegate void StartEvent(AudioLoopQueue queue);
			public event StartEvent StartCallback;
			public void Clear() => queue.Clear();

			readonly Queue<Entry> queue = new Queue<Entry>();
			public AudioClip currentPlaying;
			public Entry nextPlaying;
			public int iteration;
			public readonly AudioClip placeholder = AudioClip.Create("Placeholder", 1, 1, 1, false);
			public AudioSource ActiveLoopSource => AudioCache.ActiveLoopSource;
			public AudioSource InactiveLoopSource => AudioCache.InactiveLoopSource;
			public double currentDspTime;
			public double nextDspTime;
			public double nextDspEndTime;
			public float currentPos;
			public float currentPosSamples;

			public class Entry {
				public Entry(AudioClip clip, SwitchMode switchMode = SwitchMode.CrossFade, float fadeIn = 0, float fadeOut = 0,
					bool lengthFormat = false, int changePeriodStart = 0, int changePeriodEnd = 0, int offset = 0) {
						this.clip = clip;
						if (switchMode != SwitchMode.Clip) {
							this.fadeOut = fadeOut;
							this.fadeIn = fadeIn;
						}
						if (lengthFormat) {
							changePeriodEnd += changePeriodStart;
						}
						if (changePeriodEnd <= changePeriodStart || changePeriodEnd > clip.samples) {
							changePeriodEnd = clip.samples;
						}
						samplesPeriod = new SamplesPeriod(changePeriodStart, changePeriodEnd, offset);
						timeMode = false;
						this.switchMode = switchMode;
						throw new NotImplementedException("SamplesMode not ready yet");
				}
				public Entry(AudioClip clip, SwitchMode switchMode = SwitchMode.CrossFade, float fadeOut = 0, float fadeIn = 0,
					bool lengthFormat = false, float changePeriodStart = 0, float changePeriodEnd = 0, float offset = 0) {
						this.clip = clip;
						if (switchMode != SwitchMode.Clip) {
							this.fadeOut = fadeOut;
							this.fadeIn = fadeIn;
						}
						if (lengthFormat) {
							changePeriodEnd += changePeriodStart;
						}
						if (changePeriodEnd <= changePeriodStart || changePeriodEnd > clip.samples) {
							changePeriodEnd = clip.samples;
						}
						timePeriod = new TimePeriod(changePeriodStart, changePeriodEnd, offset);
						timeMode = true;
						this.switchMode = switchMode;
				}

				public readonly AudioClip clip;
				public readonly float fadeOut;
				public readonly float fadeIn;
				public readonly SamplesPeriod samplesPeriod;
				public readonly TimePeriod timePeriod;
				public readonly struct SamplesPeriod {
					public SamplesPeriod(int changePeriodStart, int changePeriodEnd, int offset) {
						this.changePeriodStart = changePeriodStart;
						this.changePeriodEnd = changePeriodEnd;
						this.offset = offset;
					}
					public readonly int changePeriodStart;
					public readonly int changePeriodEnd;
					public readonly int offset;
				}
				public readonly struct TimePeriod {
					public TimePeriod(float changePeriodStart, float changePeriodEnd, float offset) {
						this.changePeriodStart = changePeriodStart;
						this.changePeriodEnd = changePeriodEnd;
						this.offset = offset;
					}
					public readonly float changePeriodStart;
					public readonly float changePeriodEnd;
					public readonly float offset;
				}
				public readonly bool timeMode;
				public readonly SwitchMode switchMode;
			}
			public enum SwitchMode {
				Clip,
				CrossFade,
				Overlap,
			}
		}
		public static AudioSource[] LoopSources {get => AudioCache?.LoopSources;}
		public static AudioLoopQueue GetAudioLoopQueue(object key) {
			AudioCache.InitLoopSources();
			if (AudioCache.queueDictionary.ContainsKey(key)) {
				return AudioCache.queueDictionary[key];
			} else {
				var queue = new AudioLoopQueue();
				AudioCache.queueDictionary[key] = queue;
				return queue;
			}
		}
		public static AudioClip StartEnemyTheme_LoopPair(string clip, string loopClip, bool overlap = true, bool changeoverRaw = true, float changeover = 5) => StartEnemyTheme_LoopPair(GetAudioClip(clip), GetAudioClip(loopClip), overlap, changeoverRaw, changeover);
		/// <param name="overlap">How far back from the end of the audio file the loop should start in seconds.</param>
		public static AudioClip StartEnemyTheme_LoopPair(string clip, string loopClip, float overlap, bool changeoverRaw = true, float changeover = 5) => StartEnemyTheme_LoopPair(GetAudioClip(clip), GetAudioClip(loopClip), overlap, changeoverRaw, changeover);
		public static AudioClip ClipCut(AudioClip clip, int looplength, int loopstart, string name) {
			var newClip = AudioClip.Create(name, looplength, clip.channels, clip.frequency, false);
			float[] data = new float[looplength * clip.channels];
			clip.GetData(data, loopstart);
			newClip.SetData(data, 0);
			return newClip;
		}
		public AudioClip ClipCut(AudioClip clip, int looplength, int loopstart) => ClipCut(clip, looplength, loopstart, $"{clip.name}_loop");
		public static AudioClip[] ClipCut(AudioClip clip, int[] breakPoints, string name) {
			int entries = breakPoints.Length+1;
			AudioClip[] newClips = new AudioClip[entries];
			try {
				for (int i = 0; i < breakPoints.Length; i++) {
					newClips[i] = AudioClip.Create($"{name}_{i}", breakPoints[i], clip.channels, clip.frequency, false);
					float[] data = new float[breakPoints[i] * clip.channels];
					int loopstart = 0;
					if (i > 0) {
						loopstart = breakPoints[i-1];
					}
					clip.GetData(data, loopstart);
					newClips[i].SetData(data, 0);
				}
				newClips[entries] = AudioClip.Create($"{name}_{entries}", breakPoints[entries], clip.channels, clip.frequency, false);
				float[] data2 = new float[clip.samples * clip.channels];
				clip.GetData(data2, breakPoints[entries - 1]);
				newClips[entries].SetData(data2, 0);
			} catch (Exception ex) {
				Debug.LogException(ex);
			}
			return newClips;
		}
		public static AudioClip ClipCut(AudioClip clip, int looplength, int loopstart)
			=> ClipCut(clip, looplength, loopstart, $"{clip.name}_loop");
		public static AudioClip[] ClipCut(AudioClip clip, int[] breakPoints)
			=> ClipCut(clip, breakPoints, $"{clip.name}_loop");
		
		/// <inheritdoc cref="GetAudioClip(string)"/>
		[Obsolete("Use GetAudioClip instead")]
		public AudioClip GetEnemyTheme(string bgmName) => GetAudioClip(bgmName);
		/// <param name="bgmName">The name of the audio file (including extension).</param>
		/// <returns>A loaded AudioClip</returns>
		public AudioClip GetAudioClip(string bgmName) {
			var file = container.GetStageBgmInfo(bgmName);
			return AudioCache.GetFile(file);
		}
		/// <param name="bgmNames">An array of audio file names (including extensions).</param>
		/// <returns>An array of loaded AudioClips</returns>
		public AudioClip[] GetAudioClip(string[] bgmNames) {
			AudioClip[] themes = new AudioClip[bgmNames.Length];
			for (int i = 0; i < bgmNames.Length; i++) {
				themes[i] = GetAudioClip(bgmNames[i]);
			}
			return themes;
		}
		/// <inheritdoc cref="GetAudioClip(string[])"/>
		public AudioClip[] GetAudioClips(string[] bgmNames) => GetAudioClip(bgmNames);
		/// <inheritdoc cref="SetMapBgm(string, bool, string)"/>
		[Obsolete("Please use SetMapBgm(string, bool, string) instead")]
		public void SetMapBgm(string bgmName, bool immediate = true) {

			LoadEnemyTheme(bgmName, out var clip);
			SingletonBehavior<BattleSceneRoot>.Instance.currentMapObject.mapBgm = new AudioClip[]{clip};
			SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(SingletonBehavior<BattleSceneRoot>.Instance.currentMapObject.mapBgm);
			if (immediate) {
				SingletonBehavior<BattleSoundManager>.Instance.ChangeEnemyTheme(0);
			}
		}
		/// <summary>
		/// Sets the specified map's mapBgm.
		/// </summary>
		/// <remarks>
		/// If <paramref name="mapName"/> is null, changes Sephirah's mapBgm instead.
		/// Also sets EnemyTheme to be sure.
		/// </remarks>
		/// <param name="bgmName">The name of the audio file (including extension).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		/// <param name="mapName">The name of the target map</param>
		public void SetMapBgm(string bgmName, bool immediate = true, string mapName = null) {
			LoadEnemyTheme(bgmName, out var clip);
			var clips = new AudioClip[]{clip};
			MapManager manager;
			if (mapName == null) {
				Debug.LogWarning("CustomMapUtility:AudioHandler: Setting sephirah map's BGM");
				if (SingletonBehavior<BattleSceneRoot>.Instance.mapList != null) {
					manager = SingletonBehavior<BattleSceneRoot>.Instance.mapList.Find((MapManager x) => x.sephirahType == Singleton<StageController>.Instance.CurrentFloor);
					manager.mapBgm = clips;
				} else {return;}
			} else {
				List<MapManager> addedMapList = SingletonBehavior<BattleSceneRoot>.Instance._addedMapList;
				manager = addedMapList?.Find((MapManager x) => x.name.Contains(mapName));
				if (manager == null) {
					Debug.LogError("CustomMapUtility:AudioHandler: Map not initialized");
					return;
				}
				manager.mapBgm = clips;
			}
			SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(clips);
			if (immediate) {
				SingletonBehavior<BattleSoundManager>.Instance.ChangeEnemyTheme(0);
			}
		}
		/// <summary>
		/// Sets the specified map's mapBgm.
		/// </summary>
		/// <remarks>
		/// If <paramref name="mapName"/> is null, changes Sephirah's mapBgm instead.
		/// Also sets EnemyTheme to be sure.
		/// </remarks>
		/// <param name="bgmNames">The name of the audio file (including extension).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		/// <param name="mapName">The name of the target map</param>
		public void SetMapBgm(string[] bgmNames, bool immediate = true, string mapName = null) {
			LoadEnemyTheme(bgmNames, out var clips);
			MapManager manager;
			if (mapName == null) {
				Debug.LogWarning("CustomMapUtility:AudioHandler: Setting sephirah map's BGM");
				if (SingletonBehavior<BattleSceneRoot>.Instance.mapList != null) {
					manager = SingletonBehavior<BattleSceneRoot>.Instance.mapList.Find((MapManager x) => x.sephirahType == Singleton<StageController>.Instance.CurrentFloor);
					manager.mapBgm = clips;
				} else {return;}
			} else {
				List<MapManager> addedMapList = SingletonBehavior<BattleSceneRoot>.Instance._addedMapList;
				manager = addedMapList?.Find((MapManager x) => x.name.Contains(mapName));
				if (manager == null) {
					Debug.LogError("CustomMapUtility:AudioHandler: Map not initialized");
					return;
				}
				manager.mapBgm = clips;
			}
			SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(clips);
			if (immediate) {
				SingletonBehavior<BattleSoundManager>.Instance.ChangeEnemyTheme(0);
			}
		}
		/// <inheritdoc cref="SetMapBgm(string, bool, string)"/>
		[Obsolete("Please use SetMapBgm(string, bool, string) instead")]
		public void LoadMapBgm(string bgmName, bool immediate = true) => SetMapBgm(bgmName, immediate);
		/// <summary>
		/// Sets the specified map's mapBgm using a loaded AudioClip.
		/// Also sets EnemyTheme to be sure.
		/// </summary>
		/// <param name="bgmName">The name of the audio file (including extension).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		[Obsolete("Please use StartMapBgm(string, bool, string) instead")]
		public void StartMapBgm(string bgmName, bool immediate = true) {
			SingletonBehavior<BattleSceneRoot>.Instance.currentMapObject.mapBgm = new AudioClip[]{GetAudioClip(bgmName)};
			SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(SingletonBehavior<BattleSceneRoot>.Instance.currentMapObject.mapBgm);
			if (immediate) {
				SingletonBehavior<BattleSoundManager>.Instance.ChangeEnemyTheme(0);
			}
		}
		/// <summary>
		/// Sets the specified map's mapBgm using a loaded AudioClip.
		/// If mapName is null, changes Sephirah's mapBgm instead.
		/// Also sets EnemyTheme to be sure.
		/// </summary>
		/// <param name="bgmName">The name of the audio file (including extension).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		/// <param name="mapName">The name of the target map</param>
		public void StartMapBgm(string bgmName, bool immediate = true, string mapName = null) {
			var clips = new AudioClip[]{GetAudioClip(bgmName)};
			MapManager manager;
			if (mapName == null) {
				Debug.LogWarning("CustomMapUtility:AudioHandler: Setting sephirah map's BGM");
				if (SingletonBehavior<BattleSceneRoot>.Instance.mapList != null) {
					manager = SingletonBehavior<BattleSceneRoot>.Instance.mapList.Find((MapManager x) => x.sephirahType == Singleton<StageController>.Instance.CurrentFloor);
					manager.mapBgm = clips;
				} else {return;}
			} else {
				List<MapManager> addedMapList = SingletonBehavior<BattleSceneRoot>.Instance._addedMapList;
				manager = addedMapList?.Find((MapManager x) => x.name.Contains(mapName));
				if (manager == null) {
					Debug.LogError("CustomMapUtility:AudioHandler: Map not initialized");
					return;
				}
				manager.mapBgm = clips;
			}
			SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(clips);
			if (immediate) {
				SingletonBehavior<BattleSoundManager>.Instance.ChangeEnemyTheme(0);
			}
		}
		/// <summary>
		/// Sets the specified map's mapBgm using a loaded AudioClip array.
		/// If mapName is null, changes Sephirah's mapBgm instead.
		/// Also sets EnemyTheme to be sure.
		/// </summary>
		/// <param name="bgmNames">The name of the audio file (including extension).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		/// <param name="mapName">The name of the target map</param>
		public void StartMapBgm(string[] bgmNames, bool immediate = true, string mapName = null) {
			var clips = GetAudioClip(bgmNames);
			MapManager manager;
			if (mapName == null) {
				Debug.LogWarning("CustomMapUtility:AudioHandler: Setting sephirah map's BGM");
				if (SingletonBehavior<BattleSceneRoot>.Instance.mapList != null) {
					manager = SingletonBehavior<BattleSceneRoot>.Instance.mapList.Find((MapManager x) => x.sephirahType == Singleton<StageController>.Instance.CurrentFloor);
					manager.mapBgm = clips;
				} else {return;}
			} else {
				List<MapManager> addedMapList = SingletonBehavior<BattleSceneRoot>.Instance._addedMapList;
				manager = addedMapList?.Find((MapManager x) => x.name.Contains(mapName));
				if (manager == null) {
					Debug.LogError("CustomMapUtility:AudioHandler: Map not initialized");
					return;
				}
				manager.mapBgm = clips;
			}
			SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(clips);
			if (immediate) {
				SingletonBehavior<BattleSoundManager>.Instance.ChangeEnemyTheme(0);
			}
		}

		/// <summary>
		/// Informs the game that a given map and its music in the StageInfo XML should be active.
		/// </summary>
		/// <param name="num">Which map from the stage XML is chosen, or -1 for the Sephirah Map</param>
		public void EnforceMap(int num = 0) {
			var instance = Singleton<StageController>.Instance;
			if (num >= 0) {
				EnforceTheme();
			} else {
				UnEnforceTheme();
				instance._mapChanged = true;
			}
			instance.GetStageModel().SetCurrentMapInfo(num);
		}
		/// <summary>
		/// Informs the game that the enemy's (and by extension custom) music should be active.
		/// </summary>
		public void EnforceTheme() {
			var emotionTotalCoinNumber = Singleton<StageController>.Instance.GetCurrentStageFloorModel().team.emotionTotalCoinNumber;
			Singleton<StageController>.Instance.GetCurrentWaveModel().team.emotionTotalBonus = emotionTotalCoinNumber + 1;
		}
		/// <summary>
		/// Informs the game that the enemy's (and by extension custom) music shouldn't be active.
		/// </summary>
		/// <param name="force">Forces the command even if it appears something else has set the theme</param>
		public void UnEnforceTheme(bool force = false) {
			var emotionTotalCoinNumber = Singleton<StageController>.Instance.GetCurrentStageFloorModel().team.emotionTotalCoinNumber;
			var stageFloorModel = Singleton<StageController>.Instance.GetCurrentStageFloorModel();
			var waveModel = Singleton<StageController>.Instance.GetCurrentWaveModel();
			// An imperfect attempt to account for other things setting the theme without causing a turn of player theme.
			if (force
				|| emotionTotalCoinNumber + 1 != waveModel.team.emotionTotalBonus
				|| stageFloorModel.team.emotionLevel == stageFloorModel.team.emotionLevelMax) {
					waveModel.team.emotionTotalBonus = 0;
			}
		}
		/// <summary>
		/// Call this method before changing to your map to stop it from breaking the user's eardrums
		/// </summary>
		/// <param name="enemy">Whether this is operating on EnemyTheme or AllyTheme</param>
		public void AntiEardrumDamage(bool enemy = true) {
			var instance = SingletonBehavior<BattleSoundManager>.Instance;
			var antiEardrumDamageClip = AudioClip.Create("AntiEardrumDamage", 1, 1, 1000, stream: false);
			var antiEardrumDamageClipArray = new AudioClip[] {
				antiEardrumDamageClip,
			};
			if (enemy) {
				instance.SetEnemyTheme(antiEardrumDamageClipArray);
				instance.ChangeEnemyTheme(0);
			} else {
				var curMap = SingletonBehavior<BattleSceneRoot>.Instance.currentMapObject;
				var mapBgm = curMap.mapBgm;
				curMap.mapBgm = antiEardrumDamageClipArray;
				// instance.SetAllyTheme(antiEardrumDamageClipArray);
				instance.ChangeAllyTheme(0);
				curMap.mapBgm = mapBgm;
			}
			#if DEBUG
			Debug.Log($"CustomMapUtility: Don't break my eardrums please; Called AntiEardrumDamage({enemy})");
			#endif
		}
		public void AntiEardrumDamage_Checked(bool enemy, params AudioClip[] clips) {
			var instance = SingletonBehavior<BattleSoundManager>.Instance;
			AudioSource checkedTheme;
			if (enemy) {
				checkedTheme = instance._currentEnemyTheme;
			} else {
				checkedTheme = instance._currentAllyTheme;
			}
			var currentTheme = instance.CurrentPlayingTheme;
			if (currentTheme == checkedTheme && clips[GetCurrentEmotionIndex(clips)] == currentTheme.clip) {return;}
			AntiEardrumDamage(enemy);
		}
		public int GetCurrentEmotionIndex(params AudioClip[] clips) => GetCurrentEmotionIndex(clips.Length - 1);
		public int GetCurrentEmotionIndex(int maxIndex) {
			int emotionLevel = Singleton<StageController>.Instance.GetCurrentStageFloorModel().team.emotionLevel;
			int idx = 0;
			switch (emotionLevel)
			{
			case 0:
			case 1:
				idx = 0;
				break;
			case 2:
			case 3:
				idx = 1;
				break;
			case 4:
			case 5:
				idx = 2;
				break;
			}
			idx = Mathf.Clamp(idx, 0, maxIndex);
			return idx;
		}
		#endregion
	}
}