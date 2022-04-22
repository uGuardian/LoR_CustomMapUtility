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
#pragma warning disable MA0048,MA0016,MA0051

namespace CustomMapUtility {
	#pragma warning disable IDE0051,IDE0059,CS0219
	class A_ReadMe {
		protected const string text = "If you're reading this in a decompiler, please get the source code off github";
		protected const string text2 = "It's easier to read, and less likely to have a faulty output. It'll also be up to date.";
		protected const string URL = "https://github.com/uGuardian/LoR_CustomMapUtility";
	}
	#pragma warning restore IDE0051,IDE0059,CS0219,IDE1006
	#region OFFSETS
	/// <summary>
	/// Contains a set of image offsets ranging from 0 to 1 for initializing the stage
	/// </summary>
	[StructLayout(LayoutKind.Auto)]
	public readonly struct Offsets {
		/// <summary>
		/// Contains a set of image offsets ranging from 0 to 1 for initializing the stage
		/// </summary>
		/// <param name="bgOffsetX">Background x pivot</param>
		/// <param name="bgOffsetY">Background y pivot</param>
		/// <param name="floorOffsetX">Floor x pivot</param>
		/// <param name="floorOffsetY">Floor y pivot</param>
		/// <param name="underOffsetX">FloorUnder x pivot</param>
		/// <param name="underOffsetY">FloorUnder y pivot</param>
		public Offsets(float bgOffsetX = 0.5f, float bgOffsetY = 0.5f,
			float floorOffsetX = 0.5f, float floorOffsetY = (407.5f/1080f),
			float underOffsetX = 0.5f, float underOffsetY = (300f/1080f)) {
				BGOffset = new Vector2(bgOffsetX, bgOffsetY);
				FloorOffset = new Vector2(floorOffsetX, floorOffsetY);
				UnderOffset = new Vector2(underOffsetX, underOffsetY);
				// Debug.Log(BGOffset);
				// Debug.Log(FloorOffset);
				// Debug.Log(UnderOffset);
		}
		/// <summary>
		/// A pair of values defining the Background's x and y offset
		/// </summary>
		public readonly Vector2 BGOffset;
		/// <summary>
		/// A pair of values defining the Floor's x and y offset
		/// </summary>
		public readonly Vector2 FloorOffset;
		/// <summary>
		/// A pair of values defining the FloorUnder's x and y offset
		/// </summary>
		public readonly Vector2 UnderOffset;
	}
	#endregion
	/// <summary>
	/// Contains all CustomMapUtility commands.
	/// </summary>
	public class CustomMapHandler {
		#pragma warning disable IDE0051,IDE0059,CS0219,IDE1006
		sealed class A_ReadMe : CustomMapUtility.A_ReadMe {
			const string _text = text;
			const string _text2 = text2;
			const string _URL = URL;
		}
		#pragma warning restore IDE0051,IDE0059,CS0219,IDE1006
		#region OBSOLETESTRINGS
		public const string obsoleteString = "Remove (new MapManager) and change to InitCustomMap<MapManager>";
		public const string obsoleteString2 = "Using InitCustomMap<MapManager> and removing typeof(MapManager) is new preferred";
		public const string obsoleteStringGeneric = "Remove (new MapManager) and change to <MapManager> generic method";
		public const string obsoleteStringGeneric2 = "Using <MapManager> generic method and removing typeof(MapManager) is new preferred";
		#endregion
		
		#region HANDLER
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString2)]
		public static MapManager InitCustomMap(string stageName, Type managerType) {
			bool initBGMs = true;
			if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets)) {
				initBGMs = mapAutoBgmCache[stageName];
				Debug.Log("CustomMapUtility: Loaded offsets from cache");
			} else {offsets = new Offsets(0.5f, 0.5f);}
			return new CustomMapHandler().Init(stageName, managerType, offsets, isEgo: false, initBGMs);
		}
		/// <summary>
		/// Initializes a custom map.
		/// </summary>
		/// <remarks>
		/// Will automatically try to get previous init values for unspecified arguments.
		/// </remarks>
		/// <param name="stageName">The name of your stage folder</param>
		/// <param name="managerType">Your custom map manager</param>
		/// <param name="isEgo">Whether your map is an EGO map or an Invitation map</param>
		/// <param name="initBGMs">Whether the map automatically loads and starts the BGM</param>
		/// <param name="bgx">Background x pivot</param>
		/// <param name="bgy">Background y pivot</param>
		/// <param name="floorx">Floor x pivot</param>
		/// <param name="floory">Floor y pivot</param>
		/// <param name="underx">FloorUnder x pivot</param>
		/// <param name="undery">FloorUnder y pivot</param>
		/// <param name="managerType">Your custom map manager</param>
		/// <param name="offsets">A user-defined Offsets struct</param>
		public static MapManager InitCustomMap<T>(string stageName) where T : MapManager, IBGM, new() {
			bool initBGMs = true;
			if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets)) {
				initBGMs = mapAutoBgmCache[stageName];
				Debug.Log("CustomMapUtility: Loaded offsets from cache");
			} else {offsets = new Offsets(0.5f, 0.5f);}
			return new CustomMapHandler().Init(stageName, typeof(T), offsets, isEgo: false, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString2)]
		public static MapManager InitCustomMap(string stageName, Type managerType,
			bool isEgo = false) {
				bool initBGMs = true;
				if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets)) {
					initBGMs = mapAutoBgmCache[stageName];
					Debug.Log("CustomMapUtility: Loaded offsets from cache");
				}
				else {offsets = new Offsets(0.5f, 0.5f);}
				return new CustomMapHandler().Init(stageName, managerType, offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(string stageName,
			bool isEgo = false) where T : MapManager, IBGM, new() {
				bool initBGMs = true;
				if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets)) {
					initBGMs = mapAutoBgmCache[stageName];
					Debug.Log("CustomMapUtility: Loaded offsets from cache");
				}
				else {offsets = new Offsets(0.5f, 0.5f);}
				return new CustomMapHandler().Init(stageName, typeof(T), offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString2)]
		public static MapManager InitCustomMap(string stageName, Type managerType,
			bool isEgo = false, bool initBGMs = true) {
				if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets))
				{
					Debug.Log("CustomMapUtility: Loaded offsets from cache");
				}
				else { offsets = new Offsets(0.5f, 0.5f); }
				return new CustomMapHandler().Init(stageName, managerType, offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(string stageName,
			bool isEgo = false, bool initBGMs = true) where T : MapManager, IBGM, new() {
				if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets))
				{
					Debug.Log("CustomMapUtility: Loaded offsets from cache");
				}
				else { offsets = new Offsets(0.5f, 0.5f); }
				return new CustomMapHandler().Init(stageName, typeof(T), offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString2)]
		public static MapManager InitCustomMap(string stageName, Type managerType,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) {
				if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
				else {initBGMs = true;}
				var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
				return new CustomMapHandler().Init(stageName, managerType, offsets, isEgo: false, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(string stageName,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) where T : MapManager, IBGM, new() {
				if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
				else {initBGMs = true;}
				var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
				return new CustomMapHandler().Init(stageName, typeof(T), offsets, isEgo: false, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString2)]
		public static MapManager InitCustomMap(string stageName, Type managerType,
			bool isEgo = false,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) {
				if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
				else {initBGMs = true;}
				var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
				return new CustomMapHandler().Init(stageName, managerType, offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(string stageName,
			bool isEgo = false,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) where T : MapManager, IBGM, new() {
				if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
				else {initBGMs = true;}
				var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
				return new CustomMapHandler().Init(stageName, typeof(T), offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString2)]
		public static MapManager InitCustomMap(string stageName, Type managerType,
			bool isEgo = false, bool initBGMs = true,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) {
				var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
				return new CustomMapHandler().Init(stageName, managerType, offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(string stageName,
			bool isEgo = false, bool initBGMs = true,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) where T : MapManager, IBGM, new() {
				var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
				return new CustomMapHandler().Init(stageName, typeof(T), offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString2)]
		public static MapManager InitCustomMap(string stageName, Type managerType,
			Offsets offsets) {
				if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
				else {initBGMs = true;}
				return new CustomMapHandler().Init(stageName, managerType, offsets, isEgo: false, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(string stageName,
			Offsets offsets) where T : MapManager, IBGM, new() {
				if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
				else {initBGMs = true;}
				return new CustomMapHandler().Init(stageName, typeof(T), offsets, isEgo: false, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString2)]
		public static MapManager InitCustomMap(string stageName, Type managerType,
			Offsets offsets,
			bool isEgo = false) {
				if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
				else {initBGMs = true;}
				return new CustomMapHandler().Init(stageName, managerType, offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(string stageName,
			Offsets offsets,
			bool isEgo = false) where T : MapManager, IBGM, new() {
				if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
				else {initBGMs = true;}
				return new CustomMapHandler().Init(stageName, typeof(T), offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString2)]
		public static MapManager InitCustomMap(string stageName, Type managerType,
			Offsets offsets,
			bool isEgo = false, bool initBGMs = true) {
				return new CustomMapHandler().Init(stageName, managerType, offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public static MapManager InitCustomMap<T>(string stageName,
			Offsets offsets,
			bool isEgo = false, bool initBGMs = true) where T : MapManager, IBGM, new() {
				return new CustomMapHandler().Init(stageName, typeof(T), offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString, true)]
		public static void InitCustomMap(string stageName, MapManager manager) {}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString, true)]
		public static void InitCustomMap(string stageName, MapManager manager,
			bool isEgo = false) {}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString, true)]
		public static void InitCustomMap(string stageName, MapManager manager,
			bool isEgo = false, bool initBGMs = true) {}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString, true)]
		public static void InitCustomMap(string stageName, MapManager manager,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) {}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString, true)]
		public static void InitCustomMap(string stageName, MapManager manager,
			bool isEgo = false,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) {}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString, true)]
		public static void InitCustomMap(string stageName, MapManager manager,
			bool isEgo = false, bool initBGMs = true,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) {}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString, true)]
		public static void InitCustomMap(string stageName, MapManager manager,
			Offsets offsets) {}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		[Obsolete(obsoleteString, true)]
		public static void InitCustomMap(string stageName, MapManager manager,
			Offsets offsets,
			bool isEgo = false, bool initBGMs = true) {}

		protected MapManager Init(string stageName, Type managerType, Offsets offsets, bool isEgo, bool initBGMs) {
			// Debug.LogWarning("CustomMapUtility: StageController.InitializeMap throwing a NullReferenceException on stage start is expected, you can freely ignore it");
			List<MapManager> addedMapList = SingletonBehavior<BattleSceneRoot>.Instance.GetType().GetField("_addedMapList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(SingletonBehavior<BattleSceneRoot>.Instance) as List<MapManager>;
			MapManager x2 = addedMapList?.Find((MapManager x) => x.name.Contains(stageName));
			if (x2 != null && managerType.Equals(x2)) {
				Debug.LogWarning("CustomMapUtility: Map already loaded");
				return x2;
			}

			GameObject mapObject = Util.LoadPrefab("InvitationMaps/InvitationMap_Philip1", SingletonBehavior<BattleSceneRoot>.Instance.transform);

			#region InheritanceDebug
			Type managerTypeInherit = managerType;
			string managerTypeName = managerTypeInherit.Name;
			while (managerTypeInherit != typeof(MapManager)) {
				managerTypeInherit = managerTypeInherit.BaseType;
				managerTypeName = $"{managerTypeInherit.Name}:{managerTypeName}";
			}
			Debug.Log($"CustomMapUtility: Initializing {stageName} with {managerTypeName}");
			#endregion
			
			mapObject.name = "InvitationMap_"+stageName;
			var manager = InitManager(managerType, mapObject);

			var currentStagePath = ModResources.GetStagePath(stageName);
			newAssets = new ListDictionary(StringComparer.Ordinal){
				{"newBG", ImageLoad("Background", currentStagePath)},
				{"newFloor", ImageLoad("Floor", currentStagePath)},
				{"newUnder", ImageLoad("FloorUnder", currentStagePath)},
				{"scratch1", ImageLoad("Scratch1", currentStagePath)},
				{"scratch2", ImageLoad("Scratch2", currentStagePath)},
				{"scratch3", ImageLoad("Scratch3", currentStagePath)},
			};
			string log = "CustomMapUtility: New Assets: {";
			foreach (DictionaryEntry img in newAssets) {
				log += Environment.NewLine+"	";
				if (img.Value != null) {
					log += img.Key+":True";
				} else {
					log += img.Key+":False";
				}
			}
			log += Environment.NewLine+"}";
			Debug.Log(log);
			SetTextures(manager, offsets);
			SetScratches(stageName, manager);
			// Don't ever call SingletonBehavior<BattleSoundManager>.Instance.OnStageStart()
			if (initBGMs) {
				try {
					if (manager is IBGM managerTemp) {
						var bgms = managerTemp.GetCustomBGMs();
						if (bgms != null && bgms.Length != 0) {
							manager.mapBgm = CustomBgmParse(bgms);
						} else {
							Debug.Log("CustomMapUtility: CustomBGMs is null or empty, enabling AutoBGM");
							managerTemp.AutoBGM = true;
						}
					} else {
						Debug.LogError("CustomMapUtility: MapManager is not inherited from Custom(Creature)MapManager");
					}
				} catch (Exception ex) {
					Debug.LogError("CustomMapUtility: Failed to get BGMs");
					Debug.LogException(ex);
				}
			} else {
				Debug.Log($"CustomMapUtility: BGM initialization is disabled");
			}
			if (!isEgo) {
				SingletonBehavior<BattleSceneRoot>.Instance.InitInvitationMap(manager);
				Debug.Log("CustomMapUtility: Map Initialized.");
			} else {
				manager.GetType().GetField("_bMapInitialized", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, value: false);
				SingletonBehavior<BattleSceneRoot>.Instance.AddEgoMap(manager);
				Debug.Log("CustomMapUtility: EGO Map Added.");
				mapOffsetsCache[stageName] = offsets;
				mapAutoBgmCache[stageName] = initBGMs;
			}
			return manager;
		}

		private static readonly Dictionary<string, Offsets> mapOffsetsCache = new Dictionary<string, Offsets>(StringComparer.Ordinal);
		private static readonly Dictionary<string, bool> mapAutoBgmCache = new Dictionary<string, bool>(StringComparer.Ordinal);

		private void SetTextures(MapManager manager, Offsets offsets) {
			foreach (var component in manager.GetComponentsInChildren<Component>()) {
				switch (component) {
					case SpriteRenderer renderer when string.Equals(renderer.name, "BG", StringComparison.Ordinal):
					{
						Texture2D texture = (Texture2D)newAssets["newBG"];
						if (texture != null) {
							float pixelsPerUnit = 100f/1920f*texture.width;
							renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), offsets.BGOffset, pixelsPerUnit);
						} else {
							renderer.gameObject.SetActive(false);
						}
						break;
					}
					case SpriteRenderer renderer when renderer.name.Contains("Floor"):
					{
						Texture2D texture = (Texture2D)newAssets["newFloor"];
						if (texture != null) {
							float pixelsPerUnit = 100f/1920f*texture.width;
							renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), offsets.FloorOffset, pixelsPerUnit);
						} else {
							renderer.gameObject.SetActive(false);
						}
						break;
					}
					case SpriteRenderer renderer when renderer.name.Contains("Under"):
					{
						Texture2D texture = (Texture2D)newAssets["newUnder"];
						if (texture != null) {
							float pixelsPerUnit = 100f/1920f*texture.width;
							renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), offsets.UnderOffset, pixelsPerUnit);
						} else {
							renderer.gameObject.SetActive(false);
						}
						break;
					}
					default:
					{
						if (component.name.Contains("Effect") || component.name.Contains("Filter")) {
							component.gameObject.SetActive(false);
						}
						break;
					}
				}
			}
		}
		private Texture2D ImageLoad(string name, string currentStagePath) {
			Texture2D texture = new Texture2D(2, 2);
			try {
				var path = $"{currentStagePath}/{name}";
				var pngPath = $"{path}.png";
				var jpgPath = $"{path}.jpg";
				var jpegPath = $"{path}.jpeg";
				if (File.Exists(pngPath)) {
					texture.LoadImage(File.ReadAllBytes(pngPath));
				} else if (File.Exists(jpgPath)) {
					texture.LoadImage(File.ReadAllBytes(jpgPath));
				} else if (File.Exists(jpegPath)) {
					texture.LoadImage(File.ReadAllBytes(jpegPath));
				} else {
					return null;
				}
			} catch {
				return null;
			}
			return texture;
		}
		private ListDictionary newAssets;
		private void SetScratches(string stageName, MapManager manager) {
			if (newAssets["scratch3"] == null && newAssets["scratch2"] == null && newAssets["scratch1"] == null) {
				manager.scratchPrefabs = Array.Empty<GameObject>();
				return;
			}
			Texture2D texture;
			string log = "CustomMapUtility: SetScratches: {";
			int i;
			string name = "";
			for (i = 0; i < 3; i++) {
				switch (i) {
					case 2:
						name = "_Scratch_3";
						break;
					case 1:
						name = "_Scratch_2";
						break;
					case 0:
						name = "_Scratch_1";
						break;
				}
				GameObject current;
				switch (i) {
					case 2: {
						texture = (Texture2D)newAssets["scratch3"];
						if (texture != null) {
							if (i == 2) {
								current = UnityEngine.Object.Instantiate(manager.scratchPrefabs[2]);
								current.name = stageName+name;
								current.GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, (58.15f/170f)), (100f));
							} else {
								current = manager.scratchPrefabs[2];
							}
							break;
						}
						goto case 1;
					}
					case 1: {
						texture = (Texture2D)newAssets["scratch2"];
						if (texture != null) {
							if (i == 1) {
								current = UnityEngine.Object.Instantiate(manager.scratchPrefabs[1]);
								current.name = stageName+name;
								current.GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, (24.88f/86f)), (100f));
							} else {
								current = manager.scratchPrefabs[1];
							}
							break;
						}
						goto case 0;
					}
					case 0: {
						texture = (Texture2D)newAssets["scratch1"];
						if (texture != null) {
							if (i == 0) {
								current = UnityEngine.Object.Instantiate(manager.scratchPrefabs[0]);
								current.name = stageName+name;
								current.GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, (8.18f/19f)), (100f));
							} else {
								current = manager.scratchPrefabs[0];
							}
							break;
						}
						goto default;
					}
					default: {
						current = null;
						break;
					}
				}
				manager.scratchPrefabs[i] = current;
				try {
					log += $"{Environment.NewLine}	Scratch{i} = {current.name}";
				} catch {
					log += $"{Environment.NewLine}	Scratch{i} = null";
				}
			}
			log += Environment.NewLine+"}";
			Debug.Log(log);
		}
		#endregion

		#region RESOURCES
		public static class ModResources {
			public class CacheInit : ModInitializer {
				#if !NOMP3
				public const string version = "2.5.0";
				#else
				public const string version = "2.5.0-NOMP3";
				#endif
				public override void OnInitializeMod()
				{
					var assembly = Assembly.GetExecutingAssembly();
					if (!string.Equals(assembly.GetName().Name, "ConfigAPI", StringComparison.Ordinal)) {
						var curDir = new DirectoryInfo(assembly.Location + "\\..\\..");
						Debug.Log($"CustomMapUtility Version \"{version}\" in Local Mode at {curDir.FullName}");
						_dirInfos = new DirectoryInfo[] { curDir };
					} else {
						_dirInfos =
							from modInfo in ModContentInfoLoader.LoadAllModInfos()
								// where modInfo.activated == true
							select modInfo.dirInfo;
						Debug.Log($"CustomMapUtility Version \"{version}\" in Global Mode");
					}
					_stagePaths = GetStageRootPaths();
					_bgms = GetStageBgmInfos();
					if (_stagePaths != null && _stagePaths.Count != 0) {
						string stagePathsDebug = "CustomMapUtility StageRootPaths: {";
						foreach (var dir in _stagePaths) {
							stagePathsDebug += $"{Environment.NewLine}	{dir.FullName}";
						}
						stagePathsDebug += Environment.NewLine+"}";
						Debug.Log(stagePathsDebug);
					}
					if (_bgms != null && _bgms.Count != 0) {
						string bgmsDebug = "CustomMapUtility BgmPaths: {";
						foreach (var path in _bgms) {
							bgmsDebug += $"{Environment.NewLine}	{path.FullName}";
						}
						bgmsDebug += Environment.NewLine+"}";
						Debug.Log(bgmsDebug);
					}
					#if !NOMP3
					Singleton<ModContentManager>.Instance.GetErrorLogs().RemoveAll(x => x.Contains("NAudio"));
					#endif
				}
			}
			public static string GetStagePath(string stageName) {
				IEnumerable<DirectoryInfo> stagePaths =
					from info in GetStageRootPaths()
					where string.Equals(info.Name, stageName, StringComparison.Ordinal)
					select info;
				string path = null;
				foreach (var dir in stagePaths) {
					if (path == null) {
						path = dir.FullName;
					} else {
						Debug.LogError($"Multiple stages share the name \"{stageName}\"");
					}
				}
				if (path == null) {
					throw new ArgumentNullException(nameof(stageName), "Stage does not exist");
				}
				Debug.Log("CustomMapUtility: StagePath: "+path);
				return path;
			}
			[Obsolete("Use GetStageBgmInfos() instead")]
			public static string[] GetStageBgmPaths() {
				string[] bgms = new string[_bgms.Count];
				int i = 0;
				foreach (var bgm in _bgms) {
					bgms[i] = bgm.FullName;
					i++;
				}
				return bgms;
			}
			private static List<FileInfo> _bgms;
			[Obsolete("Use GetStageBgmInfos(string[]) instead")]
			public static string[] GetStageBgmPaths(string[] bgmNames) {
				int debugCount = 0;
				foreach (var bgm in bgmNames) {
					Debug.Log($"CustomMapUtility: BGM{debugCount}: {bgm}{Environment.NewLine}");
					debugCount++;
				}
				IEnumerable<string> bgms =
					from file in GetStageBgmInfos()
					where bgmNames.Any(b => string.Equals(b, file.Name, StringComparison.OrdinalIgnoreCase))
					select file.FullName;
				return bgms.ToArray();
			}
			public static List<FileInfo> GetStageBgmInfos() {
				if (_bgms != null && _bgms.Count != 0) {
					return _bgms;
				}
				List<FileInfo> bgms = new List<FileInfo>();
				foreach (DirectoryInfo dir in _dirInfos) {
					DirectoryInfo bgmsPath = new DirectoryInfo(Path.Combine(dir.FullName, "Resource/CustomAudio"));
					if (bgmsPath.Exists) {
						bgms.AddRange(bgmsPath.GetFiles());
					}
					bgmsPath = new DirectoryInfo(Path.Combine(dir.FullName, "Resource/StageBgm"));
					if (bgmsPath.Exists) {
						Debug.LogWarning("CustomMapUtility: StageBgm folder is now obsolete, please use CustomAudio folder instead.");
						Singleton<ModContentManager>.Instance.GetErrorLogs().Add($"<color=yellow>(assembly: {Assembly.GetExecutingAssembly().GetName().Name}) CustomMapUtility: StageBgm folder is now obselete, please use CustomAudio folder instead.</color>");
						bgms.AddRange(bgmsPath.GetFiles());
					}
				}
				return bgms;
			}
			public static Dictionary<string, FileInfo> GetStageBgmInfos(string[] bgmNames) {
				IEnumerable<FileInfo> bgms =
					from file in GetStageBgmInfos()
					where bgmNames.Any(b => string.Equals(b, file.Name, StringComparison.OrdinalIgnoreCase))
					select file;
				return bgms.ToDictionary(b => b.Name, StringComparer.OrdinalIgnoreCase);
			}
			/// <summary>
			/// A debug function that resets the stage bgm path cache
			/// </summary>
			[Obsolete("This is a debug function and should not be used in final production")]
			public static void ResetStageBgmInfos() {
				_bgms = null;
			}
			private static IEnumerable<DirectoryInfo> _dirInfos;
			public static List<DirectoryInfo> GetStageRootPaths() {
				if (_stagePaths != null && _stagePaths.Count != 0) {
					return _stagePaths;
				}
				List<DirectoryInfo> paths = new List<DirectoryInfo>();
				foreach (DirectoryInfo dir in _dirInfos) {
					DirectoryInfo stagePath = new DirectoryInfo(Path.Combine(dir.FullName, "Resource/Stage"));
					if (stagePath.Exists) {
						paths.Add(stagePath);
					}
				}
				List<DirectoryInfo> stagePaths = new List<DirectoryInfo>();
				foreach (var path in paths) {
					stagePaths.AddRange(path.GetDirectories());
				}
				return stagePaths;
			}
			private static List<DirectoryInfo> _stagePaths;
		}
		#endregion
		
		#region MANAGER
		private MapManager InitManager(Type managerType, GameObject mapObject) {
			MapManager original = mapObject.GetComponent<MapManager>();
			MapManager newManager = mapObject.AddComponent(managerType) as MapManager;

			newManager.isActivated = original.isActivated;
			newManager.isEnabled = original.isEnabled;
			newManager.mapSize = original.mapSize;
			newManager.sephirahType = original.sephirahType;
			newManager.borderFrame = original.borderFrame;
			newManager.backgroundRoot = original.backgroundRoot;
			newManager.sephirahColor = original.sephirahColor;
			newManager.scratchPrefabs = original.scratchPrefabs;
			newManager.wallCratersPrefabs = original.wallCratersPrefabs;

			try {
			rootField.SetValue(newManager, rootField.GetValue(original));
			// obstacleRootField.SetValue(newManager, obstacleRootField.GetValue(original));
			} catch {
				Debug.LogWarning("CustomMapUtility: InitManager had a minor error", newManager);
			}

			UnityEngine.Object.Destroy(original);
			return newManager;
		}
		private static readonly FieldInfo rootField = typeof(MapManager).GetField("_roots", BindingFlags.NonPublic | BindingFlags.Instance);
		// private static readonly FieldInfo obstacleRootField = typeof(MapManager).GetField("_obstacleRoot", BindingFlags.NonPublic | BindingFlags.Instance);
		#endregion
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
			if (CurrentCache == null) {
				CurrentCache = SingletonBehavior<BattleScene>.Instance.gameObject.GetComponent<AudioCache>();
				if (CurrentCache == null) {
					CurrentCache = SingletonBehavior<BattleScene>.Instance.gameObject.AddComponent<AudioCache>();
				}
			}
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
				DisableLoopSource();
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

			#pragma warning restore IDE0051
		}
		// Legacy cache implementation that's a pain to change and acts as a near-zero overhead redundancy and longer-term cache.
		private static readonly Dictionary<string, WeakReference<AudioClip>> HeldTheme = new Dictionary<string, WeakReference<AudioClip>>(StringComparer.Ordinal);
		internal static AudioCache CurrentCache = null;
		// REVIEW Consider refactoring
		
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
		/// Sets the current EnemyThemes.
		/// </summary>
		/// <param name="bgmNames">An array of audio file names (including extensions).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		public static void SetEnemyTheme(string[] bgmNames, bool immediate = true) {
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
		public static void LoadEnemyTheme(string bgmName) {
			if (HeldTheme.ContainsKey(bgmName)) {
				if (HeldTheme[bgmName].TryGetTarget(out AudioClip _)) {
					Debug.Log($"CustomMapUtility:AudioHandler: EnemyTheme {bgmName} already exists in cache");
					return;
				}
			}
			if (CurrentCache == null) {
				CurrentCache = SingletonBehavior<BattleScene>.Instance.gameObject.GetComponent<AudioCache>();
				if (CurrentCache == null) {
					CurrentCache = SingletonBehavior<BattleScene>.Instance.gameObject.AddComponent<AudioCache>();
				}
			}
			CustomBgmParseAsync(bgmName);
			Debug.Log($"CustomMapUtility:AudioHandler: Async EnemyTheme {bgmName}");
		}
		/// <summary>
		/// Preloads multiple sound files to be used with other functions.
		/// </summary>
		/// <param name="bgmNames">An array of audio file names (including extensions).</param>
		public static void LoadEnemyTheme(string[] bgmNames) => GetAudioClip(bgmNames);
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
		/// Loads multiple sound files and outputs it as an AudioClip array.
		/// </summary>
		/// <param name="bgmNames">An array of audio file names (including extensions).</param>
		/// <param name="clip">The loaded AudioClip</param>
		public static void LoadEnemyTheme(string[] bgmNames, out AudioClip[] clips) {
			clips = CustomBgmParse(bgmNames);
			Debug.Log($"CustomMapUtility:AudioHandler: Loaded EnemyTheme {bgmNames[0]} + others");
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
		/// <summary>
		/// Sets the current EnemyTheme using a loaded AudioClip array.
		/// </summary>
		/// <param name="bgmNames">An array of audio file names (including extensions).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		public static void StartEnemyTheme(string[] bgmNames, bool immediate = true) {
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
		public static AudioClip StartEnemyTheme_LoopPair(AudioClip clip, AudioClip loopClip, bool overlap = true, bool changeoverRaw = true, float changeover = 5) => StartEnemyTheme_LoopPair(clip, loopClip, overlap ? loopClip.length : 0, changeoverRaw, changeover);
		/// <param name="overlap">How far back from the end of the audio file the loop should start in seconds.</param>
		[Obsolete("Not ready yet. Implementation is buggy")]
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
			if (CurrentCache == null) {
				CurrentCache = SingletonBehavior<BattleScene>.Instance.gameObject.GetComponent<AudioCache>();
				if (CurrentCache == null) {
					CurrentCache = SingletonBehavior<BattleScene>.Instance.gameObject.AddComponent<AudioCache>();
				}
			}
			if (((CurrentCache.LoopSource?.isPlaying ?? false) && SingletonBehavior<BattleSoundManager>.Instance.CurrentPlayingTheme.clip == clip) || SingletonBehavior<BattleSoundManager>.Instance.CurrentPlayingTheme.clip == loopClip) {
				return SingletonBehavior<BattleSoundManager>.Instance.CurrentPlayingTheme.clip;
			}
			CurrentCache.PlayLoopPair(clip, loopClip, overlap, changeoverRaw, changeover);
			return clip;
		}
		[Obsolete("Not ready yet. Implementation is buggy")]
		public static AudioSource LoopSource {get => CurrentCache?.LoopSource;}
		[Obsolete("Not ready yet. Implementation is buggy")]
		public static AudioClip StartEnemyTheme_LoopPair(string clip, string loopClip, bool overlap = true, bool changeoverRaw = true, float changeover = 5) => StartEnemyTheme_LoopPair(GetAudioClip(clip), GetAudioClip(loopClip), overlap, changeoverRaw, changeover);
		/// <param name="overlap">How far back from the end of the audio file the loop should start in seconds.</param>
		[Obsolete("Not ready yet. Implementation is buggy")]
		public static AudioClip StartEnemyTheme_LoopPair(string clip, string loopClip, float overlap, bool changeoverRaw = true, float changeover = 5) => StartEnemyTheme_LoopPair(GetAudioClip(clip), GetAudioClip(loopClip), overlap, changeoverRaw, changeover);
		public static AudioClip ClipCut(AudioClip clip, int looplength, int loopstart, string name) {
			var newClip = AudioClip.Create(name, looplength, clip.channels, clip.frequency, false);
			float[] data = new float[looplength * clip.channels];
			clip.GetData(data, loopstart);
			newClip.SetData(data, 0);
			return newClip;
		}
		public static AudioClip ClipCut(AudioClip clip, int looplength, int loopstart) => ClipCut(clip, looplength, loopstart, $"{clip.name}_loop");
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
		/// <param name="bgmNames">An array of audio file names (including extensions).</param>
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
		public static void SetMapBgm(string[] bgmNames, bool immediate = true, string mapName = null) {
			LoadEnemyTheme(bgmNames, out var clips);
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
		/// Sets the specified map's mapBgm using a loaded AudioClip array.
		/// If mapName is null, changes Sephirah's mapBgm instead.
		/// Also sets EnemyTheme to be sure.
		/// </summary>
		/// <param name="bgmNames">The name of the audio file (including extension).</param>
		/// <param name="immediate">Whether it immediately forces the music to change</param>
		/// <param name="mapName">The name of the target map</param>
		public static void StartMapBgm(string[] bgmNames, bool immediate = true, string mapName = null) {
			var clips = GetAudioClip(bgmNames);
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
			if (num >= 0) {
				EnforceTheme();
			} else {
				UnEnforceTheme();
			}
			Singleton<StageController>.Instance.GetStageModel().SetCurrentMapInfo(num);
		}
		/// <summary>
		/// Informs the game that the enemy's (and by extension custom) music should be active.
		/// </summary>
		public static void EnforceTheme() {
			var emotionTotalCoinNumber = Singleton<StageController>.Instance.GetCurrentStageFloorModel().team.emotionTotalCoinNumber;
			Singleton<StageController>.Instance.GetCurrentWaveModel().team.emotionTotalBonus = emotionTotalCoinNumber + 1;
		}
		public static void UnEnforceTheme(bool force = false) {
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
		#endregion
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
		/// </summary>
		/// <remarks>
		/// If you put multiple strings in it'll change between them based on emotion level. (Emotion level 0, 2, and 4 respectively).
		/// </remarks>
		protected internal virtual string[] CustomBGMs => null;
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
	}
	#endregion

	#region WAV
	#if !NOMP3
    [StructLayout(LayoutKind.Sequential, Pack=1)]
	public readonly struct WAV {
		private static float BytesToFloat(byte firstByte, byte secondByte)
		{
			var num = (short)(secondByte << 8 | firstByte);
			return num / 32768f;
		}
		private static Stream FileToStream(string file) {
			return File.OpenRead(file);
		}
		private static Stream BytesToStream(byte[] bytes) {
			return new MemoryStream(bytes);
		}
		private readonly BinaryReader BinReader;
		
		[StructLayout(LayoutKind.Sequential, Pack=1)]
		public readonly struct RIFF {
			public RIFF(BinaryReader BinReader) {
				ChunkID = BinReader.ReadUInt32();
				if (ChunkID != 1179011410u) {
					Debug.LogWarning("Starting Header isn't RIFF, audio file is probably corrupt");
					Debug.LogWarning("Should be ("+1179011410u+") but it's ("+ChunkID+")");
					try {
						ushort check;
						retry:
						do {
							check = BinReader.ReadUInt16();
						} while (check != 18770u);
						if (BinReader.ReadUInt16() != 17990u) {
							goto retry;
						}
						BinReader.BaseStream.Seek(-8, SeekOrigin.Current);
						ChunkID = BinReader.ReadUInt32();
					} catch (EndOfStreamException) {
						Debug.LogError("Invalid audio file");
					}
				}
				ChunkSize = BinReader.ReadUInt32();
				Format = BinReader.ReadUInt32();
			}
			public readonly uint ChunkID; // Offset: 0
			public readonly uint ChunkSize; // Offset: 4
			public readonly uint Format; // Offset: 8
		}
		[StructLayout(LayoutKind.Sequential, Pack=1)]
		public readonly struct Fmt {
			public Fmt(BinaryReader BinReader) {
				ushort check;
				
				retry:
				do {
					check = BinReader.ReadUInt16();
				} while (check != 28006u);
				if (BinReader.ReadUInt16() != 8308) {
					goto retry;
				}
				
				BinReader.BaseStream.Seek(-4, SeekOrigin.Current);
				Subchunk1ID = BinReader.ReadUInt32();
				Subchunk1Size = BinReader.ReadUInt32();
				AudioFormat = BinReader.ReadUInt16();
				NumChannels = BinReader.ReadUInt16();
				SampleRate = BinReader.ReadUInt32();
				ByteRate = BinReader.ReadUInt32();
				BlockAlign = BinReader.ReadUInt16();
				BitsPerSample = BinReader.ReadUInt16();
				if (AudioFormat != 0x1u) {
					ExtraParamSize = BinReader.ReadUInt16(); 
					ExtraParams = BinReader.ReadBytes((int)ExtraParamSize);
				} else {
					ExtraParamSize = null;
					ExtraParams = new byte[1];
				}
			}
			public readonly uint Subchunk1ID; // Offset: 12
			public readonly uint Subchunk1Size; // Offset: 16
			public readonly ushort AudioFormat; // Offset: 20
			public enum Tag : ushort {
				Microsoft_PCM = 0x1 ,
				Microsoft_ADPCM = 0x2 ,
				Microsoft_IEEE_float = 0x3 ,
				Compaq_VSELP = 0x4 ,
				IBM_CVSD = 0x5 ,
				Microsoft_a_Law = 0x6 ,
				Microsoft_u_Law = 0x7 ,
				Microsoft_DTS = 0x8 ,
				DRM = 0x9 ,
				WMA_9_Speech = 0xa ,
				Microsoft_Windows_Media_RT_Voice = 0xb   ,
				OKI_ADPCM = 0x10,
				Intel_IMA_DVI_ADPCM = 0x11,
				Videologic_Mediaspace_ADPCM = 0x12,
				Sierra_ADPCM = 0x13,
				Antex_G_723_ADPCM = 0x14,
				DSP_Solutions_DIGISTD = 0x15,
				DSP_Solutions_DIGIFIX = 0x16,
				Dialoic_OKI_ADPCM = 0x17,
				Media_Vision_ADPCM = 0x18,
				HP_CU = 0x19,
				HP_Dynamic_Voice = 0x1a,
				Yamaha_ADPCM = 0x20,
				SONARC_Speech_Compression = 0x21,
				DSP_Group_True_Speech = 0x22,
				Echo_Speech_Corp_ = 0x23,
				Virtual_Music_Audiofile_AF36 = 0x24,
				Audio_Processing_Tech_ = 0x25,
				Virtual_Music_Audiofile_AF10 = 0x26,
				Aculab_Prosody_1612 = 0x27,
				Merging_Tech_LRC = 0x28,
				Dolby_AC2 = 0x30,
				Microsoft_GSM610 = 0x31,
				MSN_Audio = 0x32,
				Antex_ADPCME = 0x33,
				Control_Resources_VQLPC = 0x34,
				DSP_Solutions_DIGIREAL = 0x35,
				DSP_Solutions_DIGIADPCM = 0x36,
				Control_Resources_CR10 = 0x37,
				Natural_MicroSystems_VBX_ADPCM = 0x38,
				Crystal_Semiconductor_IMA_ADPCM = 0x39,
				Echo_Speech_ECHOSC3 = 0x3a,
				Rockwell_ADPCM = 0x3b,
				Rockwell_DIGITALK = 0x3c,
				Xebec_Multimedia = 0x3d,
				Antex_G_721_ADPCM = 0x40,
				Antex_G_728_CELP = 0x41,
				Microsoft_MSG723 = 0x42,
				IBM_AVC_ADPCM = 0x43,
				ITU_T_G_726 = 0x45,
				Microsoft_MPEG = 0x50,
				RT23_or_PAC = 0x51,
				InSoft_RT24 = 0x52,
				InSoft_PAC = 0x53,
				MP3 = 0x55,
				Cirrus = 0x59,
				Cirrus_Logic = 0x60,
				ESS_Tech_PCM = 0x61,
				Voxware_Inc = 0x62,
				Canopus_ATRAC = 0x63,
				APICOM_G_726_ADPCM = 0x64,
				APICOM_G_722_ADPCM = 0x65,
				Microsoft_DSAT = 0x66,
				Microsoft_DSAT_DISPLAY = 0x67,
				Voxware_Byte_Aligned = 0x69,
				Voxware_AC8 = 0x70,
				Voxware_AC10 = 0x71,
				Voxware_AC16 = 0x72,
				Voxware_AC20 = 0x73,
				Voxware_MetaVoice = 0x74,
				Voxware_MetaSound = 0x75,
				Voxware_RT29HW = 0x76,
				Voxware_VR12 = 0x77,
				Voxware_VR18 = 0x78,
				Voxware_TQ40 = 0x79,
				Voxware_SC3 = 0x7a,
				Voxware_SC3_1 = 0x7b,
				Soundsoft = 0x80,
				Voxware_TQ60 = 0x81,
				Microsoft_MSRT24 = 0x82,
				ATandT_G_729A = 0x83,
				Motion_Pixels_MVI_MV12 = 0x84,
				DataFusion_G_726 = 0x85,
				DataFusion_GSM610 = 0x86,
				Iterated_Systems_Audio = 0x88,
				Onlive = 0x89,
				Multitude_Inc_FT_SX20 = 0x8a,
				Infocom_ITS_A_S_G_721_ADPCM = 0x8b,
				Convedia_G729 = 0x8c,
				Not_specified_congruency_Inc = 0x8d,
				Siemens_SBC24 = 0x91,
				Sonic_Foundry_Dolby_AC3_APDIF = 0x92,
				MediaSonic_G_723 = 0x93,
				Aculab_Prosody_8kbps = 0x94,
				ZyXEL_ADPCM = 0x97,
				Philips_LPCBB = 0x98,
				Studer_Professional_Audio_Packed = 0x99,
				Malden_PhonyTalk = 0xa0,
				Racal_Recorder_GSM = 0xa1,
				Racal_Recorder_G720_a = 0xa2,
				Racal_G723_1 = 0xa3,
				Racal_Tetra_ACELP = 0xa4,
				NEC_AAC_NEC_Corporation = 0xb0,
				AAC = 0xff,
				Rhetorex_ADPCM = 0x100,
				IBM_u_Law = 0x101,
				IBM_a_Law = 0x102,
				IBM_ADPCM = 0x103,
				Vivo_G_723 = 0x111,
				Vivo_Siren = 0x112,
				Philips_Speech_Processing_CELP = 0x120,
				Philips_Speech_Processing_GRUNDIG = 0x121,
				Digital_G_723 = 0x123,
				Sanyo_LD_ADPCM = 0x125,
				Sipro_Lab_ACEPLNET = 0x130,
				Sipro_Lab_ACELP4800 = 0x131,
				Sipro_Lab_ACELP8V3 = 0x132,
				Sipro_Lab_G_729 = 0x133,
				Sipro_Lab_G_729A = 0x134,
				Sipro_Lab_Kelvin = 0x135,
				VoiceAge_AMR = 0x136,
				Dictaphone_G_726_ADPCM = 0x140,
				Qualcomm_PureVoice = 0x150,
				Qualcomm_HalfRate = 0x151,
				Ring_Zero_Systems_TUBGSM = 0x155,
				Microsoft_Audio1 = 0x160,
				Windows_Media_Audio_V2_V7_V8_V9_DivX_audio_WMA_Alex_AC3_Audio = 0x161,
				Windows_Media_Audio_Professional_V9 = 0x162,
				Windows_Media_Audio_Lossless_V9 = 0x163,
				WMA_Pro_over_S_PDIF = 0x164,
				UNISYS_NAP_ADPCM = 0x170,
				UNISYS_NAP_ULAW = 0x171,
				UNISYS_NAP_ALAW = 0x172,
				UNISYS_NAP_16K = 0x173,
				MM_SYCOM_ACM_SYC008_SyCom_Technologies = 0x174,
				MM_SYCOM_ACM_SYC701_G726L_SyCom_Technologies = 0x175,
				MM_SYCOM_ACM_SYC701_CELP54_SyCom_Technologies = 0x176,
				MM_SYCOM_ACM_SYC701_CELP68_SyCom_Technologies = 0x177,
				Knowledge_Adventure_ADPCM = 0x178,
				Fraunhofer_IIS_MPEG2AAC = 0x180,
				Digital_Theater_Systems_DTS_DS = 0x190,
				Creative_Labs_ADPCM = 0x200,
				Creative_Labs_FASTSPEECH8 = 0x202,
				Creative_Labs_FASTSPEECH10 = 0x203,
				UHER_ADPCM = 0x210,
				Ulead_DV_ACM = 0x215,
				Ulead_DV_ACM_1 = 0x216,
				Quarterdeck_Corp_ = 0x220,
				I_Link_VC = 0x230,
				Aureal_Semiconductor_Raw_Sport = 0x240,
				ESST_AC3 = 0x241,
				Interactive_Products_HSX = 0x250,
				Interactive_Products_RPELP = 0x251,
				Consistent_CS2 = 0x260,
				Sony_SCX = 0x270,
				Sony_SCY = 0x271,
				Sony_ATRAC3 = 0x272,
				Sony_SPC = 0x273,
				TELUM_Telum_Inc = 0x280,
				TELUMIA_Telum_Inc = 0x281,
				Norcom_Voice_Systems_ADPCM = 0x285,
				Fujitsu_FM_TOWNS_SND = 0x300,
				Fujitsu_1 = 0x301,
				Fujitsu_2 = 0x302,
				Fujitsu_3 = 0x303,
				Fujitsu_4 = 0x304,
				Fujitsu_5 = 0x305,
				Fujitsu_6 = 0x306,
				Fujitsu_7 = 0x307,
				Fujitsu_8 = 0x308,
				Micronas_Semiconductors_Inc_Development = 0x350,
				Micronas_Semiconductors_Inc_CELP833 = 0x351,
				Brooktree_Digital = 0x400,
				Intel_Music_Coder_IMC = 0x401,
				Ligos_Indeo_Audio = 0x402,
				QDesign_Music = 0x450,
				On2_VP7_On2_Technologies = 0x500,
				On2_VP6_On2_Technologies = 0x501,
				ATandT_VME_VMPCM = 0x680,
				ATandT_TCP = 0x681,
				YMPEG_Alpha_dummy_for_MPEG_2_compressor = 0x700,
				ClearJump_LiteWave_lossless = 0x8ae,
				Olivetti_GSM = 0x1000,
				Olivetti_ADPCM = 0x1001,
				Olivetti_CELP = 0x1002,
				Olivetti_SBC = 0x1003,
				Olivetti_OPR = 0x1004,
				Lernout_and_Hauspie = 0x1100,
				Lernout_and_Hauspie_CELP_codec = 0x1101,
				Lernout_and_Hauspie_SBC_codec = 0x1102,
				Lernout_and_Hauspie_SBC_codec_1 = 0x1103,
				Lernout_and_Hauspie_SBC_codec_2 = 0x1104,
				Norris_Comm_Inc = 0x1400,
				ISIAudio = 0x1401,
				ATandT_Soundspace_Music_Compression = 0x1500,
				VoxWare_RT24_speech_codec = 0x181c,
				Lucent_elemedia_AX24000P_Music_codec = 0x181e,
				Sonic_Foundry_LOSSLESS = 0x1971,
				Innings_Telecom_Inc_ADPCM = 0x1979,
				Lucent_SX8300P_speech_codec = 0x1c07,
				Lucent_SX5363S_G_723_compliant_codec = 0x1c0c,
				CUseeMe_DigiTalk_ex_Rocwell = 0x1f03,
				NCT_Soft_ALF2CD_ACM = 0x1fc4,
				FAST_Multimedia_DVM = 0x2000,
				Dolby_DTS_Digital_Theater_System = 0x2001,
				RealAudio_1_2_14_4 = 0x2002,
				RealAudio_1_2_28_8 = 0x2003,
				RealAudio_G2_8_Cook_low_bitrate = 0x2004,
				RealAudio_3_4_5_Music_DNET = 0x2005,
				RealAudio_10_AAC_RAAC = 0x2006,
				RealAudio_10_AAC_RACP = 0x2007,
				Reserved_range_to_0x2600_Microsoft = 0x2500,
				makeAVIS_ffvfw_fake_AVI_sound_from_AviSynth_scripts = 0x3313,
				Divio_MPEG_4_AAC_audio = 0x4143,
				Nokia_adaptive_multirate = 0x4201,
				Divio_G726_Divio_Inc = 0x4243,
				LEAD_Speech = 0x434c,
				LEAD_Vorbis = 0x564c,
				WavPack_Audio = 0x5756,
				Ogg_Vorbis_mode_1 = 0x674f,
				Ogg_Vorbis_mode_2 = 0x6750,
				Ogg_Vorbis_mode_3 = 0x6751,
				Ogg_Vorbis_mode_1_1 = 0x676f,
				Ogg_Vorbis_mode_2_1 = 0x6770,
				Ogg_Vorbis_mode_3_1 = 0x6771,
				COM_NBX_3Com_Corporation = 0x7000,
				FAAD_AAC = 0x706d,
				GSM_AMR_CBR_no_SID = 0x7a21,
				GSM_AMR_VBR_including_SID = 0x7a22,
				Comverse_Infosys_Ltd_G723_1 = 0xa100,
				Comverse_Infosys_Ltd_AVQSBC = 0xa101,
				Comverse_Infosys_Ltd_OLDSBC = 0xa102,
				Symbol_Technologies_G729A = 0xa103,
				VoiceAge_AMR_WB_VoiceAge_Corporation = 0xa104,
				Ingenient_Technologies_Inc_G726 = 0xa105,
				ISO_MPEG_4_advanced_audio_Coding = 0xa106,
				Encore_Software_Ltd_G726 = 0xa107,
				Speex_ACM_Codec_xiph_org = 0xa109,
				DebugMode_SonicFoundry_Vegas_FrameServer_ACM_Codec = 0xdfac,
				Unknown = 0xe708,
				Free_Lossless_Audio_Codec_FLAC = 0xf1ac,
				Extensible = 0xfffe,
				Development = 0xffff,
			}
			public readonly ushort NumChannels; // Offset: 22
			public readonly uint SampleRate; // Offset: 24
			public readonly uint ByteRate; // Offset: 28
			public readonly ushort BlockAlign; // Offset: 32
			public readonly ushort BitsPerSample; // Offset: 34
			public readonly ushort? ExtraParamSize; // Offset: 36?
			public readonly byte[] ExtraParams; // Offset: ?
		}
		[StructLayout(LayoutKind.Sequential, Pack=1)]
		public readonly struct Data {
			public Data(BinaryReader BinReader) {
				ushort check;
				retry:
				do {
					check = BinReader.ReadUInt16();
				} while (check != 24932u);
				if (BinReader.ReadUInt16() != 24948u) {
					goto retry;
				}
				BinReader.BaseStream.Seek(-4, SeekOrigin.Current);
				Subchunk2ID = BinReader.ReadUInt32();
				Subchunk2Size = BinReader.ReadUInt32();
				data = BinReader.ReadBytes((int)Subchunk2Size);
			}
			public readonly uint Subchunk2ID; // Offset: 36
			public readonly uint Subchunk2Size; // Offset: 40
			public readonly byte[] data; // Offset: 44
		}
		public WAV(string filename) : this(FileToStream(filename)) {}
		public WAV(byte[] wav) : this(BytesToStream(wav)) {}
		public WAV(Stream wav) {
			BinReader = new BinaryReader(wav);
			riffBlock = new RIFF(BinReader);
			fmtBlock = new Fmt(BinReader);
			dataBlock = new Data(BinReader);
			
			var bytesPerSample = fmtBlock.BitsPerSample/8u;

			LeftChannel = new float[dataBlock.Subchunk2Size/bytesPerSample/fmtBlock.NumChannels];
			RightChannel = new float[dataBlock.Subchunk2Size/bytesPerSample/fmtBlock.NumChannels];
			InterleavedAudio = new float[dataBlock.Subchunk2Size/bytesPerSample];
			var channel = 0;
			int[] iteration = new int[3];
			var total = dataBlock.Subchunk2Size;
			if (bytesPerSample != 2) {
				throw new NotSupportedException("Wav reader currently only supports 16 bit PCM");
			}
			for (var i = 0; i < total; i += (int)bytesPerSample) {
				float audioData = BytesToFloat(dataBlock.data[i], dataBlock.data[i+1]);
				if (channel == 0) {
					LeftChannel[iteration[0]] = audioData;
					iteration[0]++;
				} else if (channel == 1) {
					LeftChannel[iteration[1]] = audioData;
					iteration[1]++;
				}
				InterleavedAudio[iteration[2]] = audioData;
				iteration[2]++;
				channel++;
				if (channel >= fmtBlock.NumChannels) {
					channel = 0;
				}
			}
			BinReader.Dispose();
		}
		public readonly RIFF riffBlock;
		public readonly Fmt fmtBlock;
		public readonly Data dataBlock;

		public readonly float[] LeftChannel;
		public readonly float[] RightChannel;
		public readonly float[] InterleavedAudio;

		public ushort NumChannels {get {return this.fmtBlock.NumChannels;}}
		public uint FileSize {get {return this.riffBlock.ChunkSize + 8u;}}
		public uint FileSize_Audio {get {return this.dataBlock.Subchunk2Size;}}
		public uint SampleRate {get {return this.fmtBlock.SampleRate;}}
		public uint BytesPerSample {get {return this.fmtBlock.BitsPerSample/8u;}}
		public uint SampleCount {get {return FileSize_Audio/BytesPerSample/NumChannels;}}
		public Fmt.Tag AudioFormat {get {return (Fmt.Tag)this.fmtBlock.AudioFormat;}}

		public override string ToString()
		{
			#pragma warning disable MA0011
			return string.Format("[WAV: RIFFtag={4}, NumChannels={0}, FileSize={1}, SampleRate={2}, SampleCount={3}]", new object[]
			{
				this.NumChannels,
				this.FileSize,
				this.SampleRate,
				this.SampleCount,
				this.AudioFormat,
			});
			#pragma warning restore MA0011
		}
	}
	#endif
	#endregion
}