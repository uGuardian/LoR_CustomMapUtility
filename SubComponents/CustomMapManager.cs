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
	#region MAPMANAGERS
	public interface IBGM {
		string[] GetCustomBGMs();
		bool AutoBGM {get; set;}
	}
	public class CustomMapManager : MapManager, IBGM {
		public override void EnableMap(bool b) {
			base.EnableMap(b);
			SingletonBehavior<BattleCamManager>.Instance.BlurBackgroundCam(!b);
			if (b) {
				if (AutoBGM) {
					mapBgm = SingletonBehavior<BattleSoundManager>.Instance.GetCurrentTheme(out bool isEnemy);
					if (!isEnemy) {
						Debug.LogWarning("CustomMapUtility: Use of AutoBGM on themes not included in EnemyThemes is not officially supported yet");
					}
				} else if (mapBgm == null) {
					if (CustomBGMs != null) {
						mapBgm = CustomMapHandler.GetAudioClip(CustomBGMs);
					}
					if (mapBgm == null) {
						Debug.LogError("CustomMapUtility: mapBgm was null when the map was enabled. Setting it to current theme.");
						mapBgm = SingletonBehavior<BattleSoundManager>.Instance.GetCurrentTheme(out bool isEnemy);
						if (!isEnemy) {
							Debug.LogWarning("CustomMapUtility: Use of AutoBGM on themes not included in EnemyThemes is not officially supported yet");
						}
					}
				}
			}
		}
		public override GameObject GetScratch(int lv, Transform parent)
		{
			if (scratchPrefabs.Length == 0 || scratchPrefabs[lv] == null) {
				return null;
			}
			return base.GetScratch(lv, parent);
		}
		public override void ResetMap()
		{
			foreach (var prefab in scratchPrefabs) {
				DestroyObject(prefab);
			}
			SingletonBehavior<BattleCamManager>.Instance.BlurBackgroundCam(true);
			Debug.Log("CustomMapUtility: Cleaned up custom objects");
			base.ResetMap();
		}
		public override void InitializeMap()
		{
			base.InitializeMap();
			// This makes the map not have the sephirah filter
			sephirahType = SephirahType.None;
			sephirahColor = Color.black;
		}
		string[] IBGM.GetCustomBGMs() => CustomBGMs;
		/// <summary>
		/// Override and specify a string array with audio file names (including extensions) for the get parameter.
		/// If you put multiple strings in it'll change between them based on emotion level. (Emotion level 0, 2, and 4 respectively).
		/// </summary>
		protected internal virtual string[] CustomBGMs {get;}
		bool IBGM.AutoBGM {get => AutoBGM; set => AutoBGM = value;}
		protected internal bool AutoBGM = false;
	}
	public class CustomCreatureMapManager : CreatureMapManager, IBGM {
		public override void EnableMap(bool b) {
			base.EnableMap(b);
			SingletonBehavior<BattleCamManager>.Instance.BlurBackgroundCam(!b);
			if (b) {
				if (AutoBGM) {
					mapBgm = SingletonBehavior<BattleSoundManager>.Instance.GetCurrentTheme(out bool isEnemy);
					if (!isEnemy) {
						Debug.LogWarning("CustomMapUtility: Use of AutoBGM on themes not included in EnemyThemes is not officially supported yet");
					}
				} else if (mapBgm == null) {
					if (CustomBGMs != null) {
						mapBgm = CustomMapHandler.GetAudioClip(CustomBGMs);
					}
					if (mapBgm == null) {
						Debug.LogError("CustomMapUtility: mapBgm was null when the map was enabled. Setting it to current theme.");
						mapBgm = SingletonBehavior<BattleSoundManager>.Instance.GetCurrentTheme(out bool isEnemy);
						if (!isEnemy) {
							Debug.LogWarning("CustomMapUtility: Use of AutoBGM on themes not included in EnemyThemes is not officially supported yet");
						}
					}
				}
			}
		}
		public override GameObject GetScratch(int lv, Transform parent)
		{
			if (scratchPrefabs.Length == 0 || scratchPrefabs[lv] == null) {
				return null;
			}
			return base.GetScratch(lv, parent);
		}
		public override void ResetMap()
		{
			foreach (var prefab in scratchPrefabs) {
				DestroyObject(prefab);
			}
			SingletonBehavior<BattleCamManager>.Instance.BlurBackgroundCam(true);
			Debug.Log("CustomMapUtility: Cleaned up custom objects");
			base.ResetMap();
		}
		public override void InitializeMap()
		{
			base.InitializeMap();
			// This makes the map not have the sephirah filter
			sephirahType = SephirahType.None;
			sephirahColor = Color.black;
		}
		string[] IBGM.GetCustomBGMs() => CustomBGMs;
		/// <summary>
		/// Override and specify a string array with audio file names (including extensions) for the get parameter.
		/// If you put multiple strings in it'll change between them based on emotion level. (Emotion level 0, 2, and 4 respectively).
		/// </summary>
		protected internal virtual string[] CustomBGMs {get;}
		bool IBGM.AutoBGM {get => AutoBGM; set => AutoBGM = value;}
		protected internal bool AutoBGM = false;
	}
	#endregion
}