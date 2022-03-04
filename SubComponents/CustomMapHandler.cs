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

namespace CustomMapUtility
{
	/// <summary>
	/// Contains all CustomMapUtility commands.
	/// </summary>
	public partial class CustomMapHandler {
		#pragma warning restore IDE0051,IDE0059,CS0219,IDE1006
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
	}
}