using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
#if !NOMP3
using NAudio.Wave;
#endif
using Mod;
#pragma warning disable MA0048, MA0016, MA0051

namespace CustomMapUtility {
	#region MAPMANAGERS
	public interface IBGM : ICMU {
		string[] GetCustomBGMs();
		bool AutoBGM {get; set;}
	}
	public interface IAsyncMapInit {
		event EventHandler FirstLoad;
		bool FirstLoadCalled {get;}
	}
	public class CustomMapManager : MapManager, IBGM, IAsyncMapInit {
		public CustomMapHandler Handler {get; set;}
		public override void EnableMap(bool b) {
			base.EnableMap(b);
			SingletonBehavior<BattleCamManager>.Instance.BlurBackgroundCam(!b);
			if (b) {
				if (AutoBGM) {
					mapBgm = GetAutoBGM();
				} else if (mapBgm == null) {
					if (CustomBGMs != null) {
						mapBgm = Handler.GetAudioClip(CustomBGMs);
					}
					if (mapBgm == null) {
						Debug.LogError("CustomMapUtility: mapBgm was null when the map was enabled. Setting it to current theme.");
						mapBgm = GetAutoBGM();
					}
				}
			}
		}
		public AudioClip[] GetAutoBGM() {
			var battleSoundManager = SingletonBehavior<BattleSoundManager>.Instance;
			var bgms = battleSoundManager.GetCurrentThemes(out bool isEnemy, out bool isEnemyDefault);
			if (isEnemy && isEnemyDefault == true) {
				bgms = battleSoundManager.GetAllyThemes();
			}
			return bgms;
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
			OnFirstLoad(EventArgs.Empty);
			base.InitializeMap();
			// This makes the map not have the sephirah filter
			sephirahType = SephirahType.None;
			sephirahColor = Color.black;
		}
		string[] IBGM.GetCustomBGMs() => CustomBGMs;

		protected void OnFirstLoad(EventArgs e) {
			FirstLoad?.Invoke(this, e);
			firstLoadCalled = true;
		}

		/// <summary>
		/// Override and specify a string array with audio file names (including extensions) for the get parameter.
		/// </summary>
		/// <remarks>
		/// If you put multiple strings in it'll change between them based on emotion level. (Emotion level 0, 2, and 4 respectively).
		/// </remarks>
		protected internal virtual string[] CustomBGMs => null;
		bool IBGM.AutoBGM {get => AutoBGM; set => AutoBGM = value;}
		protected internal bool AutoBGM = false;
		public event EventHandler FirstLoad;
		bool IAsyncMapInit.FirstLoadCalled {get => firstLoadCalled;}
		private bool firstLoadCalled;
	}
	public class CustomCreatureMapManager : CreatureMapManager, IBGM, IAsyncMapInit {
		public CustomMapHandler Handler {get; set;}
		public override void EnableMap(bool b) {
			base.EnableMap(b);
			SingletonBehavior<BattleCamManager>.Instance.BlurBackgroundCam(!b);
			if (b) {
				if (AutoBGM) {
					mapBgm = GetAutoBGM();
				} else if (mapBgm == null) {
					if (CustomBGMs != null) {
						mapBgm = Handler.GetAudioClip(CustomBGMs);
					}
					if (mapBgm == null) {
						Debug.LogError("CustomMapUtility: mapBgm was null when the map was enabled. Setting it to current theme.");
						mapBgm = GetAutoBGM();
					}
				}
			}
		}
		public AudioClip[] GetAutoBGM() {
			var battleSoundManager = SingletonBehavior<BattleSoundManager>.Instance;
			var bgms = battleSoundManager.GetCurrentThemes(out bool isEnemy, out bool isEnemyDefault);
			if (isEnemy && isEnemyDefault == true) {
				bgms = battleSoundManager.GetAllyThemes();
			}
			return bgms;
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
			OnFirstLoad(EventArgs.Empty);
			base.InitializeMap();
			// This makes the map not have the sephirah filter
			sephirahType = SephirahType.None;
			sephirahColor = Color.black;
			if (AbnoText != null) {
				AbnoTextList.AddRange(AbnoText);
			}
			SingletonBehavior<CreatureDlgManagerUI>.Instance.Init(true);
		}
		string[] IBGM.GetCustomBGMs() => CustomBGMs;
		/// <inheritdoc cref="CustomMapManager.CustomBGMs"/>
		protected internal virtual string[] CustomBGMs => null;
		bool IBGM.AutoBGM {get => AutoBGM; set => AutoBGM = value;}
		protected internal bool AutoBGM = false;

		public override void CreateDialog() {
			if (AbnoTextList.Count > 0) {
				if (AbnoTextColor == null) {
					_dlgEffect = SingletonBehavior<CreatureDlgManagerUI>.Instance.SetDlg(CreateDialog_Shared());
				} else {
					_dlgEffect = SingletonBehavior<CreatureDlgManagerUI>.Instance.SetDlg(CreateDialog_Shared(), AbnoTextColor.GetValueOrDefault());
				}
			} else {
				IdxIterator(_creatureDlgIdList.Count);
				if (AbnoTextColor == null) {
					base.CreateDialog();
				} else {
					base.CreateDialog(AbnoTextColor.GetValueOrDefault());
				}
			}
		}
		public override void CreateDialog(Color txtColor) {
			if (AbnoTextList.Count > 0) {
				_dlgEffect = SingletonBehavior<CreatureDlgManagerUI>.Instance.SetDlg(CreateDialog_Shared(), AbnoTextColor.GetValueOrDefault());
			} else {
				IdxIterator(AbnoTextList.Count);
				base.CreateDialog(txtColor);
			}
		}
		/// <summary>
		/// Returns the next abnormality text. 
		/// </summary>
		/// <returns>A dialog string</returns>
		protected internal virtual string CreateDialog_Shared() {
			if ((isEgo && !AbnoTextForce) || AbnoTextList.Count <= 0) {return null;}
			IdxIterator(AbnoTextList.Count);
			if (_dlgIdx >= AbnoTextList.Count) {return null;}
			if (_dlgEffect != null && _dlgEffect.gameObject != null) {
				_dlgEffect.FadeOut();
			}
			return AbnoText[_dlgIdx];
		}
		/// <summary>
		/// Chooses a list entry to pull abnormality text from.
		/// </summary>
		protected internal virtual void IdxIterator(int totalEntries) {
			if (totalEntries <= 0) {return;}
			if (!AbnoTextRandomOrder) {
				_dlgIdx %= totalEntries;
			} else {
				_dlgIdx = UnityEngine.Random.Range(0, totalEntries);
			}
		}
		/// <summary>
		/// Whether abnormality text is chosen sequentially or randomly.
		/// </summary>
		/// <remarks>
		/// You can override <c>IdxIterator(int totalEntries)</c> for finer control.
		/// </remarks>
		protected internal virtual bool AbnoTextRandomOrder => false;
		/// <summary>
		/// Override and specify a string array with abnormality text.
		/// </summary>
		/// <remarks>
		/// This is an auto-fill helper for <c>AbnoTextList</c>.
		/// </remarks>
		protected internal virtual string[] AbnoText => null;
		/// <summary>
		/// The list of strings that will be displayed as the abnormality text.
		/// </summary>
		/// <remarks>
		/// If this isn't empty, the default handler won't check XML entries anymore.
		/// </remarks>
		public readonly List<string> AbnoTextList = new List<string>(){};
		/// <summary>
		/// If not null, changes the abnormality text color.
		/// </summary>
		protected internal virtual Color? AbnoTextColor => null;
		/// <summary>
		/// Forces the abno text to appear even for EGO maps
		/// </summary>
		/// <remarks>
		/// This isn't reccomended to use by itself, it's better to override <c>CreateDialogShared()</c> for finer control
		/// </remarks>
		protected internal virtual bool AbnoTextForce => false;

		protected void OnFirstLoad(EventArgs e) {
			FirstLoad?.Invoke(this, e);
			firstLoadCalled = true;
		}
		public event EventHandler FirstLoad;
		bool IAsyncMapInit.FirstLoadCalled {get => firstLoadCalled;}
		private bool firstLoadCalled;
	}
	#endregion
}