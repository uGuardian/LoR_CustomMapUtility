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
#if !NOMP3
using NAudio.Wave;
#endif
using Mod;
#pragma warning disable MA0048, MA0016, MA0051

namespace CustomMapUtility {
	public partial class CustomMapHandler {
		/// <summary>
		/// Changes to a custom map while insuring it's initialized.
		/// </summary>
		/// <param name="mapName">The name of the target map</param>
		/// <param name="playEffect">Determines what direction the transition special effect starts from, currently unsupported</param>
		/// <param name="scaleChange">Whether units are rescaled to the new map size</param>
		public bool ChangeToCustomMap<T>(string mapName, Faction? playEffect = Faction.Enemy, bool scaleChange = true)
			where T : MapManager, ICMU, new()
		{
			var instance = SingletonBehavior<BattleSceneRoot>.Instance;
			if (string.IsNullOrEmpty(mapName)) {
				return false;
			}
			List<MapManager> addedMapList = instance._addedMapList;
			bool exists = addedMapList?.Exists((MapManager x) => x.name.Contains(mapName)) ?? false;
			if (!exists) {
				#if DEBUG
				Debug.LogWarning($"Reinitializing {mapName} with {typeof(T)}");
				#endif
				InitCustomMap<T>(mapName);
			}
			// TODO Implement custom map change.
			// return instance.ChangeToSpecialMap(mapName, playEffect, scaleChange);
			bool changed = instance.ChangeToSpecialMap(mapName, playEffect != null, scaleChange);
			if (playEffect == null) {
				DisableMapChangingEffect(instance._mapChangeFilter);
			}
			return changed;
		}
		static void DisableMapChangingEffect(MapChangeFilter instance) {
			instance.enabled = false;
			instance.enabled = true;
			/*
			SpriteRenderer renderer = instance.GetComponent<SpriteRenderer>();
			_2dxFX_Ghost ghostEffect = instance.GetComponent<_2dxFX_Ghost>();
			Vector3 src = Vector3.zero;
			Vector3 dst = Vector3.zero;
			Vector3 cur = src;
			ParticleSystem component = instance._particleObj.GetComponent<ParticleSystem>();
			ParticleSystem.EmissionModule em = component.emission;
			renderer.enabled = false;
			instance._bParticleArrived = true;
			UnityEngine.Object.Destroy(ghostEffect);
			src = default(Vector3);
			dst = default(Vector3);
			cur = default(Vector3);
			em = default(ParticleSystem.EmissionModule);
			*/
		}
		/// <inheritdoc cref="CustomMapUtilityExtensions.AddCustomEgoMapByAssimilation(string, Faction, Type)"/>
		public void ChangeToCustomEgoMapByAssimilation<T>(string mapName, Faction faction = Faction.Player)
			where T : MapManager, ICMU, new()
			=> AddCustomEgoMapByAssimilation<T>(mapName, faction);

		/// <summary>
		/// Removes a synchonization map.
		/// </summary>
		/// <param name="name">The name of the target map</param>
		public void RemoveCustomEgoMapByAssimilation(string name) => Singleton<StageController>.Instance.RemoveEgoMapByAssimilation(name);

		/// <summary>
		/// Changes to a custom EGO map.
		/// </summary>
		/// <param name="mapName">The name of the target map</param>
		/// <param name="faction">Determines what direction the transition special effect starts from</param>
		/// <param name="manager">If not null, automatically reinitializes the map if it's been removed</param>
		/// <param name="byAssimilationFlag">Should always be false, don't change this yourself</param>
		public void ChangeToCustomEgoMap<T>(string mapName, Faction faction = Faction.Player, bool byAssimilationFlag = false) where T : MapManager, ICMU, new() {
			var Instance = SingletonBehavior<BattleSceneRoot>.Instance;
			if (String.IsNullOrEmpty(mapName))
			{
				Debug.LogError("CustomMapUtility: Ego map not specified");
				return;
			}
			List<MapManager> addedMapList = Instance._addedMapList;
			MapChangeFilter mapChangeFilter = Instance._mapChangeFilter;
			MapManager x2 = addedMapList?.Find((MapManager x) => x.name.Contains(mapName));
			if (x2 == null && typeof(T) == null) {
				Debug.LogError("CustomMapUtility: Ego map not initialized");
				return;
			}
			if (x2 == null)
			{
				Debug.LogWarning("CustomMapUtility: Reinitializing Ego map");
				InitCustomMap<T>(mapName, isEgo: true);
				x2 = addedMapList?.Find((MapManager x) => x.name.Contains(mapName));
			}
			mapChangeFilter.StartMapChangingEffect((Direction)faction, particleOn: true);
			x2.isBossPhase = false;
			x2.isEgo = true;
			if (x2 != Instance.currentMapObject)
			{
				Instance.currentMapObject.EnableMap(false);
				Instance.currentMapObject = x2;
				if (!byAssimilationFlag) {
					Instance.currentMapObject.ActiveMap(true);
					Instance.currentMapObject.InitializeMap();
				} else {
					if (!Instance.currentMapObject.IsMapInitialized) {
						Instance.currentMapObject.InitializeMap();
					}
					Instance.currentMapObject.EnableMap(true);
					Instance.currentMapObject.PlayMapChangedSound();
					SingletonBehavior<BattleCamManager>.Instance.SetVignetteColorBgCam(Instance.currentMapObject.sephirahColor, active: true);
					SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(Instance.currentMapObject.mapBgm);
				}
				return;
			}
			return;
		}
		
		/// <summary>
		/// Adds and Changes to a custom synchonization map.
		/// </summary>
		/// <param name="name">The name of the target map</param>
		/// <param name="mapName">The name of the target map</param>
		/// <param name="faction">Determines what direction the transition special effect starts from</param>
		/// <param name="managerType">If not null, automatically reinitializes the map if it's been removed</param>
		/// <param name="byAssimilationFlag">Should always be false, don't change this yourself</param>
		public void AddCustomEgoMapByAssimilation<T>(string name, Faction faction = Faction.Player) where T : MapManager, ICMU, new() {
			var Instance = StageController.Instance;
			if (Singleton<StageController>.Instance.IsTwistedArgaliaBattleEnd()) {
				return;
			}
			Instance._addedEgoMap.Add(name);
			if (name != null && name != string.Empty) {
				if (faction == Faction.Player) {
					ChangeToCustomEgoMap<T>(name, faction, byAssimilationFlag: true);
				} else {
					SingletonBehavior<BattleSceneRoot>.Instance.ChangeToSpecialMap(name, playEffect: true, scaleChange: false);
				}
			}
		}
		public AudioClip[] GetCurrentThemes() => SingletonBehavior<BattleSoundManager>.Instance.GetCurrentThemes();
		public AudioClip[] GetCurrentThemes(out bool isEnemy, out bool isEnemyDefault) =>
			SingletonBehavior<BattleSoundManager>.Instance.GetCurrentThemes(out isEnemy, out isEnemyDefault);
		public AudioClip[] GetEnemyThemes() => SingletonBehavior<BattleSoundManager>.Instance.GetEnemyThemes();
	}

	public static class Extensions {
		/// <summary>
		/// Gets the current playing theme.
		/// </summary>
		public static AudioClip[] GetCurrentThemes(this BattleSoundManager Instance) => GetCurrentThemes(Instance, out _, out _);
		/// <summary>
		/// Gets the current playing theme and outputs whether it's an EnemyTheme.
		/// </summary>
		public static AudioClip[] GetCurrentThemes(this BattleSoundManager Instance, out bool isEnemy, out bool isEnemyDefault) {
			var enemyThemes = GetEnemyThemes(Instance);
			isEnemyDefault = enemyThemes.SequenceEqual(Instance.defaultEnemyThemeSound);
			if (enemyThemes.Contains(Instance.CurrentPlayingTheme.clip)) {
				isEnemy = true;
				return enemyThemes;
			}
			isEnemy = false;
			return Instance.GetAllyThemes();
		}
		/// <summary>
		/// Gets the current enemy themes.
		/// </summary>
		public static AudioClip[] GetEnemyThemes(this BattleSoundManager Instance) => CopyArray(Instance.enemyThemeSound);
		/// <summary>
		/// Gets the current ally themes.
		/// </summary>
		public static AudioClip[] GetAllyThemes(this BattleSoundManager Instance) => CopyArray(Instance.allyThemeSound);
		static T[] CopyArray<T>(T[] array) {
			var arrayLength = array.Length;
			var newArray = new T[arrayLength];
			Array.Copy(array, newArray, arrayLength);
			return newArray;
		}
		/* TODO Implement with SyncFix
		/// <summary>
		/// Changes to an already loaded map.
		/// </summary>
		/// <remarks>
		/// Identical to vanilla ChangeToSpecialMap except it allows you to determine the direction of the particle effect.
		/// </remarks>
		/// <param name="mapName">The name of the target map</param>
		/// <param name="playEffect">Determines what direction the transition special effect starts from</param>
		/// <param name="scaleChange">Whether units are rescaled to the new map size</param>
		public static bool ChangeToSpecialMap(
			this BattleSceneRoot instance, string mapName, Faction? playEffect = Faction.Enemy, bool scaleChange = true)
		{
			if (string.IsNullOrEmpty(mapName)) {
				return false;
			}
			List<MapManager> addedMapList = instance._addedMapList;
			MapManager x2 = addedMapList?.Find((MapManager x) => x.name.Contains(mapName));
			if (x2 != null && x2 != instance.currentMapObject) {
				if (playEffect != null) {
					instance._mapChangeFilter.StartMapChangingEffect((Direction)playEffect, particleOn: true);
				}
				if (instance.currentMapObject.isCreature) {
					UnityEngine.Object.Destroy(instance.currentMapObject.gameObject);
				}
				else {
					instance.currentMapObject.EnableMap(false);
				}
				instance.currentMapObject = x2;
				if (!instance.currentMapObject.IsMapInitialized)
				{
					instance.currentMapObject.InitializeMap();
				}
				instance.currentMapObject.EnableMap(true);
				instance.currentMapObject.PlayMapChangedSound();
				SingletonBehavior<BattleCamManager>.Instance.SetVignetteColorBgCam(instance.currentMapObject.sephirahColor, true);
				SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(instance.currentMapObject.mapBgm);
				if (scaleChange)
				{
					foreach (BattleUnitModel battleUnitModel in BattleObjectManager.instance.GetList())
					{
						battleUnitModel.view.ChangeScale(instance.currentMapObject.mapSize);
					}
				}
				return true;
			}
			return false;
		}
		*/
	}
}