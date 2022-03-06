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
		#region EXTENSIONS
		/// <inheritdoc cref="CustomMapUtilityExtensions.ChangeToCustomEgoMap(BattleSceneRoot, string, Faction, Type, bool)"/>
		[Obsolete(obsoleteStringGeneric2)]
		public static void ChangeToCustomEgoMap(string mapName, Faction faction = Faction.Player, Type managerType = null, bool byAssimilationFlag = false)
			=> SingletonBehavior<BattleSceneRoot>.Instance.ChangeToCustomEgoMap(mapName, faction, managerType, byAssimilationFlag);

		/// <inheritdoc cref="CustomMapUtilityExtensions.ChangeToCustomEgoMap(BattleSceneRoot, string, Faction, Type, bool)"/>
		public static void ChangeToCustomEgoMap<T>(string mapName, Faction faction = Faction.Player, bool byAssimilationFlag = false)
			where T : MapManager, IBGM, new()
			=> SingletonBehavior<BattleSceneRoot>.Instance.ChangeToCustomEgoMap<T>(mapName, faction, byAssimilationFlag);

		/// <inheritdoc cref="CustomMapUtilityExtensions.ChangeToCustomEgoMap(BattleSceneRoot, string, Faction, Type, bool)"/>
		[Obsolete(obsoleteStringGeneric, true)]
		public static void ChangeToCustomEgoMap(string mapName, Faction faction, MapManager manager, bool byAssimilationFlag)
			=> SingletonBehavior<BattleSceneRoot>.Instance.ChangeToCustomEgoMap(mapName, faction, manager, byAssimilationFlag);

		/// <inheritdoc cref="CustomMapUtilityExtensions.AddCustomEgoMapByAssimilation(StageController, string, Faction, Type)"/>
		[Obsolete(obsoleteStringGeneric2)]
		public static void AddCustomEgoMapByAssimilation(string name, Faction faction = Faction.Player, Type managerType = null)
			=> Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(name, faction, managerType);

		/// <inheritdoc cref="CustomMapUtilityExtensions.AddCustomEgoMapByAssimilation(StageController, string, Faction, Type)"/>
		public static void AddCustomEgoMapByAssimilation<T>(string name, Faction faction = Faction.Player)
			where T : MapManager, IBGM, new()
			=> Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation<T>(name, faction);

		/// <inheritdoc cref="CustomMapUtilityExtensions.AddCustomEgoMapByAssimilation(StageController, string, Faction, Type)"/>
		[Obsolete(obsoleteStringGeneric, true)]
		public static void AddCustomEgoMapByAssimilation(string name, Faction faction, MapManager manager)
			=> Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(name, faction, manager);

		/// <inheritdoc cref="CustomMapUtilityExtensions.AddCustomEgoMapByAssimilation(StageController, string, Faction, Type)"/>
		[Obsolete(obsoleteStringGeneric2)]
		public static void ChangeToCustomEgoMapByAssimilation(string mapName, Faction faction = Faction.Player, Type managerType = null)
			=> Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(mapName, faction, managerType);

		/// <inheritdoc cref="CustomMapUtilityExtensions.AddCustomEgoMapByAssimilation(StageController, string, Faction, Type)"/>
		public static void ChangeToCustomEgoMapByAssimilation<T>(string mapName, Faction faction = Faction.Player)
			where T : MapManager, IBGM, new()
			=> Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation<T>(mapName, faction);

		/// <inheritdoc cref="CustomMapUtilityExtensions.AddCustomEgoMapByAssimilation(StageController, string, Faction, Type)"/>
		[Obsolete(obsoleteStringGeneric, true)]
		public static void ChangeToCustomEgoMapByAssimilation(string mapName, Faction faction, MapManager manager)
			=> Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(mapName, faction, manager);

		/// <summary>
		/// Removes a synchonization map.
		/// </summary>
		/// <param name="name">The name of the target map</param>
		public static void RemoveCustomEgoMapByAssimilation(string name) => Singleton<StageController>.Instance.RemoveEgoMapByAssimilation(name);
		#endregion
	}
	#region EXTENSIONS
	/// <summary>
	///  Contains helper extensions for more natural use.
	/// </summary>
	public static class CustomMapUtilityExtensions {
		/// <summary>
		/// Changes to a custom EGO map.
		/// </summary>
		/// <param name="mapName">The name of the target map</param>
		/// <param name="faction">Determines what direction the transition special effect starts from</param>
		/// <param name="manager">If not null, automatically reinitializes the map if it's been removed</param>
		/// <param name="byAssimilationFlag">Should always be false, don't change this yourself</param>
		[Obsolete(CustomMapHandler.obsoleteStringGeneric2)]
		public static void ChangeToCustomEgoMap(this BattleSceneRoot Instance,
			string mapName, Faction faction = Faction.Player, Type managerType = null, bool byAssimilationFlag = false) {
				if (String.IsNullOrWhiteSpace(mapName))
				{
					Debug.LogError("CustomMapUtility: Ego map not specified");
					return;
				}
				List<MapManager> addedMapList = Instance.GetType().GetField("_addedMapList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Instance) as List<MapManager>;
				MapChangeFilter mapChangeFilter = Instance.GetType().GetField("_mapChangeFilter", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Instance) as MapChangeFilter;
				MapManager x2 = addedMapList?.Find((MapManager x) => x.name.Contains(mapName));
				if (x2 == null && managerType == null) {
					Debug.LogError("CustomMapUtility: Ego map not initialized");
					return;
				}
				if (x2 == null)
				{
					Debug.LogWarning("CustomMapUtility: Reinitializing Ego map");
					CustomMapHandler.InitCustomMap(mapName, managerType, isEgo: true);
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

		/// <inheritdoc cref="ChangeToCustomEgoMap(BattleSceneRoot, string, Faction, Type, bool)"/>
		public static void ChangeToCustomEgoMap<T>(this BattleSceneRoot Instance, string mapName, Faction faction = Faction.Player, bool byAssimilationFlag = false) where T : MapManager, IBGM, new() {
			if (String.IsNullOrWhiteSpace(mapName))
			{
				Debug.LogError("CustomMapUtility: Ego map not specified");
				return;
			}
			List<MapManager> addedMapList = Instance.GetType().GetField("_addedMapList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Instance) as List<MapManager>;
			MapChangeFilter mapChangeFilter = Instance.GetType().GetField("_mapChangeFilter", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Instance) as MapChangeFilter;
			MapManager x2 = addedMapList?.Find((MapManager x) => x.name.Contains(mapName));
			if (x2 == null && typeof(T) == null) {
				Debug.LogError("CustomMapUtility: Ego map not initialized");
				return;
			}
			if (x2 == null)
			{
				Debug.LogWarning("CustomMapUtility: Reinitializing Ego map");
				CustomMapHandler.InitCustomMap<T>(mapName, isEgo: true);
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
		#pragma warning disable CS0618

		/// <inheritdoc cref="ChangeToCustomEgoMap(BattleSceneRoot, string, Faction, Type, bool)"/>
		public static void ChangeToCustomEgoMap(this BattleSceneRoot Instance, string mapName, Faction faction = Faction.Player, bool byAssimilationFlag = false)
			=> ChangeToCustomEgoMap(Instance, mapName, faction, (Type)null, byAssimilationFlag);
		#pragma warning restore CS0618

		[Obsolete(CustomMapHandler.obsoleteStringGeneric, true)]
		public static void ChangeToCustomEgoMap(this BattleSceneRoot Instance, string mapName, Faction faction = Faction.Player, MapManager manager = null, bool byAssimilationFlag = false) {}
		
		/// <inheritdoc cref="AddCustomEgoMapByAssimilation(StageController, string, Faction, Type)"/>
		[Obsolete(CustomMapHandler.obsoleteStringGeneric2)]
		public static void ChangeToCustomEgoMapByAssimilation(this BattleSceneRoot _, string mapName, Faction faction = Faction.Player, Type managerType = null)
			=> Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(mapName, faction, managerType);
		
		/// <inheritdoc cref="AddCustomEgoMapByAssimilation(StageController, string, Faction, Type)"/>
		public static void ChangeToCustomEgoMapByAssimilation<T>(this BattleSceneRoot _, string mapName, Faction faction = Faction.Player) where T : MapManager, IBGM, new() 
			=> Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation<T>(mapName, faction);
		
		/// <inheritdoc cref="AddCustomEgoMapByAssimilation(StageController, string, Faction, Type)"/>
		[Obsolete(CustomMapHandler.obsoleteStringGeneric, true)]
		public static void ChangeToCustomEgoMapByAssimilation(this BattleSceneRoot _, string mapName, Faction faction, MapManager manager)
			=> Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(mapName, faction, manager);
		
		/// <summary>
		/// Adds and Changes to a custom synchonization map.
		/// </summary>
		/// <param name="name">The name of the target map</param>
		/// <param name="mapName">The name of the target map</param>
		/// <param name="faction">Determines what direction the transition special effect starts from</param>
		/// <param name="managerType">If not null, automatically reinitializes the map if it's been removed</param>
		/// <param name="byAssimilationFlag">Should always be false, don't change this yourself</param>
		[Obsolete(CustomMapHandler.obsoleteStringGeneric2)]
		public static void AddCustomEgoMapByAssimilation(this StageController Instance, string name, Faction faction = Faction.Player, Type managerType = null) {
			if (Singleton<StageController>.Instance.IsTwistedArgaliaBattleEnd())
			{
				return;
			}
			var addedEgoMapOrigin = Instance.GetType().GetField("_addedEgoMap", BindingFlags.NonPublic | BindingFlags.Instance);
			List<string> addedEgoMap = addedEgoMapOrigin.GetValue(Instance) as List<string>;
			addedEgoMap.Add(name);
			addedEgoMapOrigin.SetValue(Instance, addedEgoMap);
			if (name != null && name != string.Empty)
			{
				if (faction == Faction.Player) {
					SingletonBehavior<BattleSceneRoot>.Instance.ChangeToCustomEgoMap(name, faction, managerType, byAssimilationFlag: true);
				} else {
					SingletonBehavior<BattleSceneRoot>.Instance.ChangeToSpecialMap(name, playEffect: true, scaleChange: false);
				}
			}
		}
		public static void AddCustomEgoMapByAssimilation<T>(this StageController Instance, string name, Faction faction = Faction.Player) where T : MapManager, IBGM, new() {
			if (Singleton<StageController>.Instance.IsTwistedArgaliaBattleEnd())
			{
				return;
			}
			var addedEgoMapOrigin = Instance.GetType().GetField("_addedEgoMap", BindingFlags.NonPublic | BindingFlags.Instance);
			List<string> addedEgoMap = addedEgoMapOrigin.GetValue(Instance) as List<string>;
			addedEgoMap.Add(name);
			addedEgoMapOrigin.SetValue(Instance, addedEgoMap);
			if (name != null && name != string.Empty)
			{
				if (faction == Faction.Player) {
					SingletonBehavior<BattleSceneRoot>.Instance.ChangeToCustomEgoMap<T>(name, faction, byAssimilationFlag: true);
				} else {
					SingletonBehavior<BattleSceneRoot>.Instance.ChangeToSpecialMap(name, playEffect: true, scaleChange: false);
				}
			}
		}
		/// <inheritdoc cref="AddCustomEgoMapByAssimilation(StageController, string, Faction, Type)"/>
		[Obsolete(CustomMapHandler.obsoleteStringGeneric, true)]
		public static void AddCustomEgoMapByAssimilation(this StageController Instance, string name, Faction faction = Faction.Player, MapManager manager = null) {}
		public static void RemoveCustomEgoMapByAssimilation(this StageController Instance, string name) => Instance.RemoveEgoMapByAssimilation(name);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString2)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType)
			=> CustomMapHandler.InitCustomMap(stageName, managerType);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(this BattleSceneRoot _, string stageName)
			where T : MapManager, IBGM, new()
				=> CustomMapHandler.InitCustomMap<T>(stageName);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString2)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
			bool isEgo = false)
				=> CustomMapHandler.InitCustomMap(stageName, managerType, isEgo);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(this BattleSceneRoot _, string stageName,
			bool isEgo = false) where T : MapManager, IBGM, new()
				=> CustomMapHandler.InitCustomMap<T>(stageName, isEgo);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString2)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
			bool isEgo = false, bool initBGMs = true)
				=> CustomMapHandler.InitCustomMap(stageName, managerType, isEgo, initBGMs);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(this BattleSceneRoot _, string stageName,
			bool isEgo = false, bool initBGMs = true) where T : MapManager, IBGM, new()
				=> CustomMapHandler.InitCustomMap<T>(stageName, isEgo, initBGMs);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString2)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f))
				=> CustomMapHandler.InitCustomMap(stageName, managerType, bgx, bgy, floorx, floory, underx, undery);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(this BattleSceneRoot _, string stageName,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) where T : MapManager, IBGM, new()
				=> CustomMapHandler.InitCustomMap<T>(stageName, bgx, bgy, floorx, floory, underx, undery);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString2)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
			bool isEgo = false,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f))
				=> CustomMapHandler.InitCustomMap(stageName, managerType, isEgo, bgx, bgy, floorx, floory, underx, undery);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(this BattleSceneRoot _, string stageName,
			bool isEgo = false,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) where T : MapManager, IBGM, new()
				=> CustomMapHandler.InitCustomMap<T>(stageName, isEgo, bgx, bgy, floorx, floory, underx, undery);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString2)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
			bool isEgo = false, bool initBGMs = true,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f))
				=> CustomMapHandler.InitCustomMap(stageName, managerType, isEgo, initBGMs, bgx, bgy, floorx, floory, underx, undery);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(this BattleSceneRoot _, string stageName,
			bool isEgo = false, bool initBGMs = true,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) where T : MapManager, IBGM, new()
				=> CustomMapHandler.InitCustomMap<T>(stageName, isEgo, initBGMs, bgx, bgy, floorx, floory, underx, undery);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString2)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
			Offsets offsets)
				=> CustomMapHandler.InitCustomMap(stageName, managerType, offsets);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(this BattleSceneRoot _, string stageName,
			Offsets offsets) where T : MapManager, IBGM, new()
				=> CustomMapHandler.InitCustomMap<T>(stageName, offsets);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString2)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
			Offsets offsets,
			bool isEgo = false)
				=> CustomMapHandler.InitCustomMap(stageName, managerType, offsets, isEgo);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(this BattleSceneRoot _, string stageName,
			Offsets offsets,
			bool isEgo = false) where T : MapManager, IBGM, new()
				=> CustomMapHandler.InitCustomMap<T>(stageName, offsets, isEgo);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString2)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
			Offsets offsets,
			bool isEgo = false, bool initBGMs = true)
				=> CustomMapHandler.InitCustomMap(stageName, managerType, offsets, isEgo, initBGMs);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(this BattleSceneRoot _, string stageName,
			Offsets offsets,
			bool isEgo = false, bool initBGMs = true) where T : MapManager, IBGM, new()
				=> CustomMapHandler.InitCustomMap<T>(stageName, offsets, isEgo, initBGMs);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString, true)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager)
			=> CustomMapHandler.InitCustomMap(stageName, manager.GetType());
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString, true)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
			bool isEgo = false)
				=> CustomMapHandler.InitCustomMap(stageName, manager.GetType(), isEgo);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString, true)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
			bool isEgo = false, bool initBGMs = true)
				=> CustomMapHandler.InitCustomMap(stageName, manager.GetType(), isEgo, initBGMs);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString, true)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f / 1080f),
			float underx = 0.5f, float undery = (300f / 1080f))
				=> CustomMapHandler.InitCustomMap(stageName, manager.GetType(), bgx, bgy, floorx, floory, underx, undery);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString, true)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
			bool isEgo = false,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f / 1080f),
			float underx = 0.5f, float undery = (300f / 1080f))
				=> CustomMapHandler.InitCustomMap(stageName, manager.GetType(), isEgo, bgx, bgy, floorx, floory, underx, undery);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString, true)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
			bool isEgo = false, bool initBGMs = true,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f / 1080f),
			float underx = 0.5f, float undery = (300f / 1080f))
				=> CustomMapHandler.InitCustomMap(stageName, manager.GetType(), isEgo, initBGMs, bgx, bgy, floorx, floory, underx, undery);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString, true)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
			Offsets offsets)
				=> CustomMapHandler.InitCustomMap(stageName, manager.GetType(), offsets);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString, true)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
			Offsets offsets,
			bool isEgo = false)
				=> CustomMapHandler.InitCustomMap(stageName, manager.GetType(), offsets, isEgo);
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(CustomMapHandler.obsoleteString, true)]
		public static MapManager InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
			Offsets offsets,
			bool isEgo = false, bool initBGMs = true)
				=> CustomMapHandler.InitCustomMap(stageName, manager.GetType(), offsets, isEgo, initBGMs);
		/// <summary>
		/// Gets the current playing theme.
		/// </summary>
		public static AudioClip[] GetCurrentTheme(this BattleSoundManager Instance) => GetCurrentTheme(Instance, out _);
		/// <summary>
		/// Gets the current playing theme and outputs whether it's an EnemyTheme.
		/// </summary>
		public static AudioClip[] GetCurrentTheme(this BattleSoundManager Instance, out bool isEnemy) {
			var enemyThemes = GetEnemyTheme(Instance);
			if (enemyThemes.Contains(Instance.CurrentPlayingTheme.clip)) {
				isEnemy = true;
				return enemyThemes;
			}
			Debug.LogWarning("Current theme is not in EnemyThemes, returning only the currently playing theme");
			isEnemy = false;
			return new AudioClip[]{Instance.CurrentPlayingTheme.clip};
		}
		/// <summary>
		/// Gets the current enemy theme.
		/// </summary>
		public static AudioClip[] GetEnemyTheme(this BattleSoundManager Instance) {
			var enemyThemes = Instance.SetEnemyTheme(new AudioClip[]{null});
			Instance.SetEnemyTheme(enemyThemes);
			return enemyThemes;
		}
	}
	#endregion
}