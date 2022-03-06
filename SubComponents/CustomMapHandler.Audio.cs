using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
#if !NOMP3
using NAudio.Wave;
#endif
using Mod;
#pragma warning disable MA0048, MA0016, MA0051

namespace CustomMapUtility {
	public partial class CustomMapHandler {
		#region AUDIO
		private static AudioClip CustomBgmParse(string BGM) => CustomBgmParse(new string[] { BGM }, false)[0];
		private static AudioClip[] CustomBgmParse(string[] BGMs) => CustomBgmParse(BGMs, false);
		private static AudioClip[] CustomBgmParse(string[] BGMs, bool async) {
			var files = ModResources.GetStageBgmInfos(BGMs);
			if (files?.Count == 0) {
				foreach (var bgm in BGMs) {
					Debug.LogError($"CustomMapUtility:AudioHandler: {bgm} does not exist.");
				}
				return null;
			}
			GetCurrentCache();
			var parsed = new Dictionary<string, AudioClip>(files.Count, StringComparer.OrdinalIgnoreCase);
			var index = new Dictionary<int, string>(files.Count);
			for (int i = 0; i < BGMs.Length; i++) {
				index.Add(i, BGMs[i]);
			}
			foreach (var file in files) {
				AudioClip clip;
				var name = file.Key;
				if (HeldTheme.ContainsKey(name)) {
					if (HeldTheme[name].TryGetTarget(out clip) && clip != null) {
						parsed[name] = clip;
						CurrentCache.Dictionary[name] = clip;
						Debug.Log($"CustomMapUtility:AudioHandler: EnemyTheme {name} already exists in cache");
						continue;
					} else {
						Debug.Log($"CustomMapUtility:AudioHandler: Cache for {name} was dropped from memory");
					}
				}
				clip = CurrentCache.CheckAsync(name, false);
				if (clip != null) {
					parsed[name] = clip;
					continue;
				}
				try {
					var info = files[name];
					AudioType format = AudioType.UNKNOWN;
					// Debug.Log($"CustomMapUtility:AudioHandler: Audio{i} = {name}");
					Debug.Log($"CustomMapUtility:AudioHandler: Loading {name}");
					switch (info.Extension.ToUpperInvariant()) {
						case ".WAVE":
						case ".WAV": {
							format = AudioType.WAV;
							break;
						}
						case ".MP3":
						case ".MPEG": {
							format = AudioType.MPEG;
							break;
						}
						case ".OGV":
						case ".OGA":
						case ".OGX":
						case ".OGM":
						case ".SPX":
						case ".OPUS":
						case ".OGG": {
							format = AudioType.OGGVORBIS;
							break;
						}
						case ".MP4":
						case ".3GP":
						case ".M4B":
						case ".M4P":
						case ".M4R":
						case ".M4V":
						case ".M4A":
						case ".AAC": {
							format = AudioType.ACC;
							break;
						}
						case ".AIF":
						case ".AIFC":
						case ".AIFF": {
							format = AudioType.AIFF;
							break;
						}
						case ".IT": {
							format = AudioType.IT;
							break;
						}
						case ".MOD": {
							format = AudioType.MOD;
							break;
						}
						case ".S3M": {
							format = AudioType.S3M;
							break;
						}
						case ".XM": {
							format = AudioType.XM;
							break;
						}
						case ".XMA": {
							format = AudioType.XMA;
							break;
						}
						case ".VAG": {
							format = AudioType.VAG;
							break;
						}
						default: {
							Debug.LogError("CustomMapUtility:AudioHandler: File type unknown");
							break;
						}
					}
					if (!async) {
						clip = CurrentCache.Parse(files[name].FullName, format);
						if (clip != null) {
							clip.name = name;
						}
						parsed[name] = clip;
						CurrentCache.Dictionary[name] = clip;
						HeldTheme[name] = new WeakReference<AudioClip>(clip);
					} else {
						CurrentCache.Async(files[name].FullName, format, name);
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
		private static void CustomBgmParseAsync(string BGM) => CustomBgmParse(new string[] { BGM }, true);
		private static void CustomBgmParseAsync(string[] BGMs) => CustomBgmParse(BGMs, true);
		sealed internal class AudioCache : MonoBehaviour {
			#pragma warning disable IDE0051
			#region AudioParsing
			public AudioClip Parse(string path, AudioType format) {
				// if (format == AudioType.WAV) {
				//	 return ParseWAV(path);
				// }
				try {
					using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($"file://{path}", format))
					{
						var request = www.SendWebRequest();

						while (!request.isDone){
							// Debug.Log(request.progress);
						}

						if (www.isNetworkError)
						{
							throw new InvalidOperationException(www.error);
						}
						else
						{
							var clip = DownloadHandlerAudioClip.GetContent(www);
							if (clip != null) {
								return clip;
							}
						}
					}
					#if !NOMP3
					if (format == AudioType.MPEG) {
						Debug.LogWarning("CustomMapUtility:AudioHandler: Falling back to NAudio and Custom WAV");
						WAV wav;
						using (var sourceProvider = new Mp3FileReader(path)) {
							MemoryStream stream = new MemoryStream();
							WaveFileWriter.WriteWavFileToStream(stream, sourceProvider);
							wav = new WAV(stream.ToArray());
							return ParseWAV(wav);
						}
					}
					#endif
					return null;
				} catch (UnityException) {
					Debug.LogError("CustomMapUtility:AudioHandler: Don't call LoadEnemyTheme (or derivatives) in field initializers or class constructors, Unity hates that.");
					return null;
				}
			}
			public IEnumerator ParseAsync(string path, AudioType format, string bgmName) {
				// if (format == AudioType.WAV) {
				//	  return ParseWAV(path);
				// }
				using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($"file://{path}", format))
				{
					UnityWebRequestAsyncOperation request;
					if (!CurrentCache.wwwDic.ContainsKey(bgmName)) {
						request = www.SendWebRequest();
						CurrentCache.wwwDic.TryAdd(bgmName, (request, (path, format)));
					} else {
						yield break;
					}
					yield return request;

					if (www.isNetworkError)
					{
						// HeldTheme[bgmName] = new WeakReference<AudioClip>(null);
						throw new InvalidOperationException($"{bgmName}: {www.error}");
					}
					else
					{
						var clip = DownloadHandlerAudioClip.GetContent(www);
						if (clip != null) {
							clip.name = bgmName;
							CurrentCache.Dictionary[bgmName] = clip;
							HeldTheme[bgmName] = new WeakReference<AudioClip>(clip);
						}
						#if !NOMP3
						else if (format == AudioType.MPEG) {
							Debug.LogWarning("CustomMapUtility:AudioHandler: Falling back to NAudio and Custom WAV");
							WAV wav;
							using (var sourceProvider = new Mp3FileReader(path)) {
								MemoryStream stream = new MemoryStream();
								WaveFileWriter.WriteWavFileToStream(stream, sourceProvider);
								wav = new WAV(stream.ToArray());
								if (clip != null) {
									clip.name = bgmName;
								}
								CurrentCache.Dictionary[bgmName] = clip;
								HeldTheme[bgmName] = new WeakReference<AudioClip>(ParseWAV(wav));
							}
						}
						#endif
						else {
							// HeldTheme[bgmName] = new WeakReference<AudioClip>(null);
							throw new InvalidOperationException(bgmName+": BGM Returned Null");
						}
					}
				}
			}
			#if !NOMP3
			private static AudioClip ParseWAV(string path) => ParseWAV(new WAV(path));
			private static AudioClip ParseWAV(WAV wav) {
				if (wav.NumChannels > 8) {
					throw new NotSupportedException("Unity does not support more than 8 audio channels per file");
				}
				var audioClip = AudioClip.Create("BGM", (int)wav.SampleCount, wav.NumChannels, (int)wav.SampleRate, stream: false);
				audioClip.SetData(wav.InterleavedAudio, 0);
				Debug.Log($"Parse Result: {wav}");
				return audioClip;
			}
			#endif
			public void Async(string path, AudioType format, string bgmName) {
				if (!coroutines.ContainsKey(bgmName)) {
					coroutines.Add(bgmName, StartCoroutine(ParseAsync(path, format, bgmName)));
				}
			}
			#endregion
			public AudioClip CheckAsync(string bgmName, bool errors = true)
			{
				if (coroutines.ContainsKey(bgmName)) {
					if (wwwDic.ContainsKey(bgmName)) {
						try {
							Debug.Log($"CustomMapUtility:AudioHandler: Waiting for async load of {bgmName} to finish...");
							var request = wwwDic[bgmName].Item1.webRequest;
							while (!request.isDone) {}
							var clip = DownloadHandlerAudioClip.GetContent(request);
							if (clip != null) {
								clip.name = bgmName;
							}
							CurrentCache.Dictionary[bgmName] = clip;
							HeldTheme[bgmName] = new WeakReference<AudioClip>(clip);
							StopCoroutine(coroutines[bgmName]);
							coroutines.Remove(bgmName);
							wwwDic.TryRemove(bgmName, out _);
							Debug.Log("CustomMapUtility:AudioHandler: Async load finished");
							return clip;
						} catch (Exception ex) {
							Debug.LogError("CustomMapUtility:AudioHandler: Async load error");
							Debug.LogException(ex);
							// HeldTheme[bgmName] = new WeakReference<AudioClip>(null);
							return null;
						}
					} else {
						Debug.LogWarning($"CustomMapUtility:AudioHandler: Async load never started, Reloading entry {bgmName}");
						// HeldTheme[bgmName] = new WeakReference<AudioClip>(null);
						try {
						StopCoroutine(coroutines[bgmName]);
						} catch {}
						return null;
					}
				} else {
					if (errors) {
						Debug.LogWarning($"CustomMapUtility:AudioHandler: Entry {bgmName} does not exist, Reloading entry");
					}
					return null;
				}
			}
			void OnDisable() {
				Dictionary.Clear();
				StopAllCoroutines();
				DisableLoopSources();
				coroutines.Clear();
				wwwDic.Clear();
			}
			public readonly ConcurrentDictionary<string, AudioClip> Dictionary = new ConcurrentDictionary<string, AudioClip>(StringComparer.Ordinal);
			readonly Dictionary<string, Coroutine> coroutines = new Dictionary<string, Coroutine>(StringComparer.Ordinal);
			internal readonly ConcurrentDictionary<string, (UnityWebRequestAsyncOperation, (string, AudioType))> wwwDic = new ConcurrentDictionary<string, (UnityWebRequestAsyncOperation, (string, AudioType))>(StringComparer.Ordinal);
			// private Dictionary<string, WeakReference<AudioClip>> HeldTheme {get => CustomMapHandler.HeldTheme;}

			#region LoopAudio
			//TODO Add audio queue
			//TODO Allow ally themes
			[Obsolete]
			public void PlayLoopPair(AudioClip clip, AudioClip loopClip, float overlap = 0, bool changeoverRaw = true, float changeover = 2) {
				float changeoverlap = changeoverRaw ? overlap - changeover : overlap * changeover;
				InitLoopSources();
				LateUpdate();
				currentQueue.Stop();
				currentQueue = new AudioLoopQueue();
				currentQueue.Init(clip);
				float delay = clip.length - (overlap - changeoverlap);
				currentQueue.Enqueue(loopClip, AudioLoopQueue.SwitchMode.Clip,
					changePeriodStart: delay-changeoverlap, changePeriodEnd: delay+changeoverlap);
			}
			public void ChangeCheck() {
				currentCheck = StartCoroutine(ChangeCheckCoroutine());
			}
			public IEnumerator ChangeCheckCoroutine() {
				yield return new WaitWhile(() => SingletonBehavior<BattleSoundManager>.Instance.CurrentPlayingTheme.clip == currentQueue.placeholder);
				DisableLoopSources();
				StopCrossFades();
			}
			// This is a constant that should be as close to 0 as possible without making pops so it doesn't get in the way of base game music.
			const float flipStartDelay = 0.0625f;
			IEnumerator CrossFadeCoroutine(double nextDspTime, double nextDspEndTime) {
				var active = ActiveLoopSource;
				var inactive = InactiveLoopSource;
				inactive.volume = 0;
				var interval = nextDspEndTime - nextDspTime;
				yield return new WaitWhile(() => AudioSettings.dspTime < nextDspTime);
				activeTransition = true;
				while (AudioSettings.dspTime < nextDspEndTime) {
					var volume = SingletonBehavior<BattleSoundManager>.Instance.CurrentPlayingTheme.volume;
					float current = (float)((AudioSettings.dspTime - nextDspTime) / interval);
					active.volume = Mathf.Lerp(0f, volume, current);
					inactive.volume = Mathf.Lerp(volume, 0f, current);
					yield return null;
				}
				FlipLoopSource();
				InactiveLoopSource.Stop();
				currentQueue.currentDspTime = nextDspTime;
				currentQueue.Next();
				activeTransition = false;
			}
			public void CrossFade(double nextDspTime, double nextDspEndTime)
				=> CrossFadeRoutine = StartCoroutine(CrossFadeCoroutine(nextDspTime, nextDspEndTime));
			void LateUpdate() {
				if (!activeTransition) {
					foreach (var loopSource in loopSources) {
						if (loopSource != null) {
							loopSource.volume = SingletonBehavior<BattleSoundManager>.Instance.CurrentPlayingTheme.volume;
						}
					}
				}
			}
			public void StopCrossFades() {
				foreach (var coroutine in crossFadeRoutines) {
					StopCoroutine(coroutine);
				}
			}
			public void DisableLoopSources() {
				foreach (var loopSource in loopSources) {
					if (loopSource != null) {
						loopSource.time = 0;
						loopSource.Stop();
						loopSource.clip = null;
					}
				}
			}
			AudioSource FlipLoopSource() {
				activeLoopSource ^= true;
				loopSources[activeLoopSource ? 1 : 0].name = "LoopSource_Active";
				loopSources[activeLoopSource ? 0 : 1].name = "LoopSource_Inactive";
				return loopSources[activeLoopSource ? 1 : 0];
			}
			public void InitLoopSources() {
				var currentTheme = SingletonBehavior<BattleSoundManager>.Instance.CurrentPlayingTheme;
				if (loopSources[0] == null || loopSources[1] == null) {
					loopSources[0] = Instantiate(currentTheme, currentTheme.transform);
					loopSources[1] = Instantiate(currentTheme, currentTheme.transform);
					FlipLoopSource();
				}
			}

			public AudioSource ActiveLoopSource => loopSources[activeLoopSource ? 1 : 0];
			public AudioSource InactiveLoopSource => loopSources[activeLoopSource ? 0 : 1];
			bool activeLoopSource;
			static readonly AudioSource[] loopSources = new AudioSource[2];
			public AudioSource[] LoopSources {get => loopSources;}
			static Coroutine currentCheck;
			static readonly Coroutine[] crossFadeRoutines = new Coroutine[3];
			static Coroutine CrossFadeRoutine {get => crossFadeRoutines[0]; set => crossFadeRoutines[0] = value;}
			static Coroutine FadeInRoutine {get => crossFadeRoutines[1]; set => crossFadeRoutines[1] = value;}
			static Coroutine FadeOutRoutine {get => crossFadeRoutines[2]; set => crossFadeRoutines[2] = value;}

			public static AudioLoopQueue currentQueue;
			bool activeTransition = false;
			public readonly Dictionary<object, AudioLoopQueue> queueDictionary = new Dictionary<object, AudioLoopQueue>();
			#endregion
			#pragma warning restore IDE0051
		}
		// Legacy cache implementation that's a pain to change and acts as a near-zero overhead redundancy and longer-term cache.
		private static readonly Dictionary<string, WeakReference<AudioClip>> HeldTheme = new Dictionary<string, WeakReference<AudioClip>>(StringComparer.Ordinal);
		internal static AudioCache CurrentCache = null;
		internal static AudioCache GetCurrentCache() {
			if (CurrentCache == null) {
				CurrentCache = SingletonBehavior<BattleScene>.Instance.gameObject.GetComponent<AudioCache>();
				if (CurrentCache == null) {
					CurrentCache = SingletonBehavior<BattleScene>.Instance.gameObject.AddComponent<AudioCache>();
				}
			}
			return CurrentCache;
		}
		/// <summary>
		/// Sets the current EnemyTheme.
		/// </summary>
		/// <param name="bgmName">The name of the audio file (including extension).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		public static void SetEnemyTheme(string bgmName, bool immediate = true) {
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
		/// Preloads a sound file to be used with other functions.
		/// </summary>
		/// <param name="bgmName">The name of the audio file (including extension).</param>
		public static void LoadEnemyTheme(string bgmName) {
			if (HeldTheme.ContainsKey(bgmName)) {
				if (HeldTheme[bgmName].TryGetTarget(out AudioClip _)) {
					Debug.Log($"CustomMapUtility:AudioHandler: EnemyTheme {bgmName} already exists in cache");
					return;
				}
			}
			GetCurrentCache();
			CustomBgmParseAsync(bgmName);
			Debug.Log($"CustomMapUtility:AudioHandler: Async EnemyTheme {bgmName}");
		}
		/// <summary>
		/// Loads a sound file and outputs it as an AudioClip.
		/// </summary>
		/// <param name="bgmName">The name of the audio file (including extension).</param>
		/// <param name="clip">The loaded AudioClip</param>
		public static void LoadEnemyTheme(string bgmName, out AudioClip clip) {
			clip = CustomBgmParse(bgmName);
			Debug.Log($"CustomMapUtility:AudioHandler: Loaded EnemyTheme {bgmName}");
		}
		/// <summary>
		/// Starts the last loaded AudioClip.
		/// </summary>
		[Obsolete("Please use StartEnemyTheme(bgmName) instead", true)]
		public static void StartEnemyTheme() {
			var last = HeldTheme.Last();
			StartEnemyTheme(last.Key);
		}
		/// <summary>
		/// Sets the current EnemyTheme using a loaded AudioClip.
		/// </summary>
		/// <param name="bgmName">The name of the audio file (including extension).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		public static void StartEnemyTheme(string bgmName, bool immediate = true) {
			var theme = GetAudioClip(bgmName);
			SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(new AudioClip[]{theme});
			if (immediate) {
				SingletonBehavior<BattleSoundManager>.Instance.ChangeEnemyTheme(0);
				Debug.Log($"CustomMapUtility:AudioHandler: Changed EnemyTheme to {bgmName} and enforced it");
			} else {
				Debug.Log($"CustomMapUtility:AudioHandler: Changed EnemyTheme to {bgmName}");
			}
		}
		public static AudioClip StartEnemyTheme_LoopPair(AudioClip clip, AudioClip loopClip, bool overlap = true, bool changeoverRaw = true, float changeover = 5) => StartEnemyTheme_LoopPair(clip, loopClip, overlap ? loopClip.length : 0, changeoverRaw, changeover);
		/// <param name="overlap">How far back from the end of the audio file the loop should start in seconds.</param>
		public static AudioClip StartEnemyTheme_LoopPair(AudioClip clip, AudioClip loopClip, float overlap, bool changeoverRaw = true, float changeover = 5) {
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
			GetCurrentCache();
			if ((CurrentCache.ActiveLoopSource?.clip == clip) || CurrentCache.ActiveLoopSource?.clip == loopClip) {
				return SingletonBehavior<BattleSoundManager>.Instance.CurrentPlayingTheme.clip;
			}
			CurrentCache.PlayLoopPair(clip, loopClip, overlap, changeoverRaw, changeover);
			return clip;
		}
		public class AudioLoopQueue {
			public void Init(AudioClip clip) {
				currentPlaying = clip;
				var loopSource = GetCurrentCache().ActiveLoopSource;
				loopSource.Stop();
				loopSource.time = 0;
				loopSource.clip = clip;
				currentDspTime = AudioSettings.dspTime+0.2;
				loopSource.PlayScheduled(currentDspTime);
			}
			public void Play(AudioClip clip, bool force = false) {
				var loopSource = GetCurrentCache().ActiveLoopSource;
				if (currentPlaying == null || force) {
					Init(clip);
				}
				Play();
			}
			public void Play() {
				var loopSource = GetCurrentCache().ActiveLoopSource;
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
								GetCurrentCache().CrossFade(nextDspTime, nextDspEndTime);
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
				CurrentCache.StopCrossFades();
				ResetCallback.Invoke(this);
			}
			/// <remarks>
			/// Alias for Stop
			/// </remarks>
			public void Pause() => Stop();
			public void Stop() {
				Reset();
				GetCurrentCache().DisableLoopSources();
			}
			public delegate void StartEvent(AudioLoopQueue queue);
			public event StartEvent StartCallback;
			public void Clear() => queue.Clear();

			readonly Queue<Entry> queue = new Queue<Entry>();
			public AudioClip currentPlaying;
			public Entry nextPlaying;
			public int iteration;
			public readonly AudioClip placeholder = AudioClip.Create("Placeholder", 1, 1, 1, false);
			public AudioSource ActiveLoopSource => GetCurrentCache().ActiveLoopSource;
			public AudioSource InactiveLoopSource => GetCurrentCache().InactiveLoopSource;
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
		public static AudioSource[] LoopSources {get => CurrentCache?.LoopSources;}
		public static AudioLoopQueue GetAudioLoopQueue(object key) {
			GetCurrentCache().InitLoopSources();
			if (CurrentCache.queueDictionary.ContainsKey(key)) {
				return CurrentCache.queueDictionary[key];
			} else {
				var queue = new AudioLoopQueue();
				CurrentCache.queueDictionary[key] = queue;
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
		public static AudioClip GetEnemyTheme(string bgmName) => GetAudioClip(bgmName);
		/// <param name="bgmName">The name of the audio file (including extension).</param>
		/// <returns>A loaded AudioClip</returns>
		public static AudioClip GetAudioClip(string bgmName) {
			AudioClip theme;
			if (HeldTheme.ContainsKey(bgmName)) {
				if (!HeldTheme[bgmName].TryGetTarget(out theme)) {
					// Debug.LogWarning($"CustomMapUtility:AudioHandler: Entry {bgmName} was dropped from memory. Reloading entry");
					LoadEnemyTheme(bgmName, out theme);
				}
			} else {
				if (CurrentCache != null) {
					var async = CurrentCache.CheckAsync(bgmName);
					if (async != null) {
						return async;
					}
				} else {
					Debug.LogWarning($"CustomMapUtility:AudioHandler: Entry {bgmName} does not exist, Reloading entry");
				}
				LoadEnemyTheme(bgmName, out theme);
			}
			Debug.Log($"CustomMapUtility:AudioHandler: Got EnemyTheme {bgmName}");
			return theme;
		}
		/// <param name="bgmNames">An array of the names of the audio files (including extension).</param>
		/// <returns>An array of loaded AudioClips</returns>
		public static AudioClip[] GetAudioClip(string[] bgmNames) {
			AudioClip[] themes = new AudioClip[bgmNames.Length];
			for (int i = 0; i < bgmNames.Length; i++) {
				themes[i] = GetAudioClip(bgmNames[i]);
			}
			return themes;
		}
		/// <inheritdoc cref="GetAudioClip(string[])"/>
		public static AudioClip[] GetAudioClips(string[] bgmNames) => GetAudioClip(bgmNames);
		/// <inheritdoc cref="SetMapBgm(string, bool, string)"/>
		[Obsolete("Please use SetMapBgm(string, bool, string) instead")]
		public static void SetMapBgm(string bgmName, bool immediate = true) {

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
		public static void SetMapBgm(string bgmName, bool immediate = true, string mapName = null) {
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
				List<MapManager> addedMapList = SingletonBehavior<BattleSceneRoot>.Instance.GetType().GetField("_addedMapList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(SingletonBehavior<BattleSceneRoot>.Instance) as List<MapManager>;
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
		public static void LoadMapBgm(string bgmName, bool immediate = true) => SetMapBgm(bgmName, immediate);
		/// <summary>
		/// Sets the specified map's mapBgm using a loaded AudioClip.
		/// Also sets EnemyTheme to be sure.
		/// </summary>
		/// <param name="bgmName">The name of the audio file (including extension).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		[Obsolete("Please use StartMapBgm(string, bool, string) instead")]
		public static void StartMapBgm(string bgmName, bool immediate = true) {
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
		public static void StartMapBgm(string bgmName, bool immediate = true, string mapName = null) {
			var clips = new AudioClip[]{GetAudioClip(bgmName)};
			MapManager manager;
			if (mapName == null) {
				Debug.LogWarning("CustomMapUtility:AudioHandler: Setting sephirah map's BGM");
				if (SingletonBehavior<BattleSceneRoot>.Instance.mapList != null) {
					manager = SingletonBehavior<BattleSceneRoot>.Instance.mapList.Find((MapManager x) => x.sephirahType == Singleton<StageController>.Instance.CurrentFloor);
					manager.mapBgm = clips;
				} else {return;}
			} else {
				List<MapManager> addedMapList = SingletonBehavior<BattleSceneRoot>.Instance.GetType().GetField("_addedMapList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(SingletonBehavior<BattleSceneRoot>.Instance) as List<MapManager>;
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
		public static void EnforceMap(int num = 0) {
			EnforceTheme();
			Singleton<StageController>.Instance.GetStageModel().SetCurrentMapInfo(num);
		}
		/// <summary>
		/// Informs the game that the enemy's (and by extension custom) music should be active.
		/// </summary>
		public static void EnforceTheme() {
			var emotionTotalCoinNumber = Singleton<StageController>.Instance.GetCurrentStageFloorModel().team.emotionTotalCoinNumber;
			Singleton<StageController>.Instance.GetCurrentWaveModel().team.emotionTotalBonus = emotionTotalCoinNumber + 1;
		}
		#endregion
	}
}