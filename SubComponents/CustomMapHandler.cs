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
using CustomMapUtility.Texture;
#if !NOMP3
using NAudio.Wave;
#endif
using Mod;
#pragma warning disable MA0048, MA0016, MA0051

namespace CustomMapUtility {
	/// <summary>
	/// Contains all internal CustomMapUtility commands.
	/// </summary>
	public partial class CustomMapHandler {
		#pragma warning restore IDE0051,IDE0059,CS0219,IDE1006
		#region HANDLER
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
		public T InitCustomMap<T>(string stageName) where T : MapManager, ICMU, new() {
			bool initBGMs = true;
			if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets)) {
				initBGMs = mapAutoBgmCache[stageName];
				Debug.Log("CustomMapUtility: Loaded offsets from cache");
			} else {offsets = new Offsets(0.5f, 0.5f);}
			return new MapInstance<T>(this).Init(stageName, offsets, isEgo: false, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public T InitCustomMap<T>(string stageName,
			bool isEgo = false) where T : MapManager, ICMU, new() {
				bool initBGMs = true;
				if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets)) {
					initBGMs = mapAutoBgmCache[stageName];
					Debug.Log("CustomMapUtility: Loaded offsets from cache");
				}
				else {offsets = new Offsets(0.5f, 0.5f);}
				return new MapInstance<T>(this).Init(stageName, offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public T InitCustomMap<T>(string stageName,
			bool isEgo = false, bool initBGMs = true) where T : MapManager, ICMU, new() {
				if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets))
				{
					Debug.Log("CustomMapUtility: Loaded offsets from cache");
				}
				else { offsets = new Offsets(0.5f, 0.5f); }
				return new MapInstance<T>(this).Init(stageName, offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public T InitCustomMap<T>(string stageName,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) where T : MapManager, ICMU, new() {
				if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
				else {initBGMs = true;}
				var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
				return new MapInstance<T>(this).Init(stageName, offsets, isEgo: false, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public T InitCustomMap<T>(string stageName,
			bool isEgo = false,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) where T : MapManager, ICMU, new() {
				if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
				else {initBGMs = true;}
				var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
				return new MapInstance<T>(this).Init(stageName, offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public T InitCustomMap<T>(string stageName,
			bool isEgo = false, bool initBGMs = true,
			float bgx = 0.5f, float bgy = 0.5f,
			float floorx = 0.5f, float floory = (407.5f/1080f),
			float underx = 0.5f, float undery = (300f/1080f)) where T : MapManager, ICMU, new() {
				var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
				return new MapInstance<T>(this).Init(stageName, offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public T InitCustomMap<T>(string stageName,
			Offsets offsets) where T : MapManager, ICMU, new() {
				if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
				else {initBGMs = true;}
				return new MapInstance<T>(this).Init(stageName, offsets, isEgo: false, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public T InitCustomMap<T>(string stageName,
			Offsets offsets,
			bool isEgo = false) where T : MapManager, ICMU, new() {
				if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
				else {initBGMs = true;}
				return new MapInstance<T>(this).Init(stageName, offsets, isEgo, initBGMs);
		}
		/// <inheritdoc cref="InitCustomMap(string)"/>
		public T InitCustomMap<T>(string stageName,
			Offsets offsets,
			bool isEgo = false, bool initBGMs = true) where T : MapManager, ICMU, new() {
				return new MapInstance<T>(this).Init(stageName, offsets, isEgo, initBGMs);
		}

		internal readonly Dictionary<string, Offsets> mapOffsetsCache = new Dictionary<string, Offsets>(StringComparer.Ordinal);
		internal readonly Dictionary<string, bool> mapAutoBgmCache = new Dictionary<string, bool>(StringComparer.Ordinal);
	}
	public interface ICMU {
		CustomMapHandler Handler {get; set;}
	}
	public class MapInstance<T> where T : MapManager, ICMU, new() {
		readonly public CustomMapHandler handler;
		CustomMapHandler audioHandler;
		protected T manager;
		protected Offsets offsets;
		protected bool loadIsInitted;
		protected string stageName;
		protected Task imageInitTask;
		public CustomMapHandler AudioHandler {
			get => audioHandler ?? handler;
			set => audioHandler = value;
		}
		public MapInstance(CustomMapHandler handler) {
			this.handler = handler;
			var allImageNames = AllImageNames;
			newAssets = new Dictionary<string, Sprite>(allImageNames.Count, StringComparer.OrdinalIgnoreCase);
			foreach (var name in allImageNames) {
				newAssets.Add(name, null);
			}
		}
		public CustomMapHandler.ModResources.CMUContainer Container => handler.container;
		public virtual T Init(string stageName, Offsets offsets, bool isEgo, bool initBGMs) {
			// Debug.LogWarning("CustomMapUtility: StageController.InitializeMap throwing a NullReferenceException on stage start is expected, you can freely ignore it");
			List<MapManager> addedMapList = SingletonBehavior<BattleSceneRoot>.Instance._addedMapList;
			MapManager x2 = addedMapList?.Find((MapManager x) => x.name.Contains(stageName));
			if (x2 != null) {
				if (x2 is T x2t) {
					Debug.LogWarning("CustomMapUtility: A map with an overlapping name and type is already loaded");
					return x2t;
				} else {
					Debug.LogError("CustomMapUtility: A map with an overlapping name and a different type is already loaded");
					return null;
				}
			}
			this.stageName = stageName;
			this.offsets = offsets;

			var mapObject = Util.LoadPrefab("InvitationMaps/InvitationMap_Philip1", SingletonBehavior<BattleSceneRoot>.Instance.transform);

			#region InheritanceDebug
			#if InheritanceDebug
			var managerType = typeof(T);
			Type managerTypeInherit = managerType;
			string managerTypeName = managerTypeInherit.Name;
			while (managerTypeInherit != typeof(MapManager)) {
				managerTypeInherit = managerTypeInherit.BaseType;
				managerTypeName = $"{managerTypeInherit.Name}:{managerTypeName}";
			}
			Debug.Log($"CustomMapUtility: Initializing {stageName} with {managerTypeName}");
			#endif
			#endregion
			
			mapObject.name = "InvitationMap_"+stageName;
			T manager = InitManager(mapObject);
			var managerAsync = manager as IAsyncMapInit;
			if (managerAsync != null) {
				imageInitTask = ImageInit_Async();
			} else {
				throw new NotImplementedException("Synchronous Init is not currently available");
				// ImageInit(stageName, offsets, manager);
			}
			// Don't ever call SingletonBehavior<BattleSoundManager>.Instance.OnStageStart()
			if (initBGMs) {
				try {
					if (manager is IBGM managerTemp) {
						var bgms = managerTemp.GetCustomBGMs();
						if (bgms != null && bgms.Length != 0) {
							if (managerAsync == null) {
								manager.mapBgm = AudioHandler.CustomBgmParse(bgms);
							} else {
								AudioHandler.CustomBgmParseAsync(bgms);
								managerAsync.FirstLoad += (object sender, EventArgs e) =>
									manager.mapBgm = AudioHandler.GetAudioClip(bgms);
							}
						} else {
							Debug.Log("CustomMapUtility: CustomBGMs is null or empty, enabling AutoBGM");
							managerTemp.AutoBGM = true;
						}
					} else {
						Debug.LogWarning("CustomMapUtility: MapManager does not inherit from IBGM");
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
				manager._bMapInitialized = false;
				SingletonBehavior<BattleSceneRoot>.Instance.AddEgoMap(manager);
				Debug.Log("CustomMapUtility: EGO Map Added.");
				handler.mapOffsetsCache[stageName] = offsets;
				handler.mapAutoBgmCache[stageName] = initBGMs;
			}
			return manager;
		}

		protected virtual HashSet<string> AllImageNames => new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
			"Background",
			"Floor",
			"FloorUnder",
			"Scratch1",
			"Scratch2",
			"Scratch3",
		};

		[Obsolete("Synchronous Init is not currently available", true)]
		protected virtual void ImageInit(string stageName, Offsets offsets, T manager) {
			var currentStageDir = Container.GetStageDir(stageName);
		}

		protected async virtual Task ImageInit_Async() {
			var managerAsync = manager as IAsyncMapInit;
			var currentStageDir = Container.GetStageDir(stageName);
			var imageNames = AllImageNames;
			int totalCount = AllImageNames.Count;
			int curIndex = 0;
			var spriteTasks = new Dictionary<string, Task<Sprite>>(totalCount, StringComparer.OrdinalIgnoreCase);
			foreach (var file in currentStageDir.EnumerateFiles()) {
				var croppedName = file.Name;
				croppedName = croppedName.Substring(0, croppedName.IndexOf('.'));
				if (imageNames.TryGetValue(croppedName, out var imageName)) {
					spriteTasks[imageName] = SpriteLoad_Async(imageName, file);
					curIndex++;
				}
				if (curIndex >= totalCount) {break;}
			}
			managerAsync.FirstLoad += CompleteImageInit;

			foreach (var entry in spriteTasks) {
				entry.Deconstruct(out var name, out var task);
				newAssets[name] = await task;
			}

			SetTextures(manager);
			SetScratches(stageName, manager);
		}

		/*
		protected void DebugNewAssets() {
			string log = "CustomMapUtility: New Assets: {";
			foreach (DictionaryEntry img in newAssets) {
				log += Environment.NewLine + "	";
				if (img.Value != null) {
					log += img.Key + ":True";
				} else {
					log += img.Key + ":False";
				}
			}
			log += Environment.NewLine + "}";
			Debug.Log(log);
		}
		*/

		protected void CompleteImageInit(object sender, EventArgs e) {
			TextureCache.AwaitCompletion(imageInitTask);
		}

		public async Task<Sprite> SpriteLoad_Async(string name, FileInfo file) {
			Texture2D texture;
			Sprite sprite;
			try {
				texture = await TextureCache.GetFile_Async(file);
			}
			#if !DEBUG
			catch {
				return null;
			}
			#else
			catch (AggregateException ex) {
				ex.Handle((e) => {
						Debug.LogException(e);
						return true;
					}
				);
				return null;
			} catch (Exception ex) {
				Debug.LogException(ex);
				return null;
			}
			#endif
			float pixelsPerUnit;
			switch (name) {
				case "Background":
					pixelsPerUnit = 100f/1920f*texture.width;
					sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), offsets.BGOffset, pixelsPerUnit);
					break;
				case "Floor":
					pixelsPerUnit = 100f/1920f*texture.width;
					sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), offsets.FloorOffset, pixelsPerUnit);
					break;
				case "FloorUnder":
					pixelsPerUnit = 100f/1920f*texture.width;
					sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), offsets.UnderOffset, pixelsPerUnit);
					break;
				case "Scratch1":
					sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, (58.15f/170f)), (100f));
					break;
				case "Scratch2":
					sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, (24.88f/86f)), (100f));
					break;
				case "Scratch3":
					sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, (8.18f/19f)), (100f));
					break;
				default:
					sprite = null;
					break;
			}
			return sprite;
		}
		internal static TextureCache TextureCache {get {
			if (textureCache == null) {
				textureCache = SingletonBehavior<BattleScene>.Instance.gameObject.GetComponent<TextureCache>();
				if (textureCache == null) {
					textureCache = SingletonBehavior<BattleScene>.Instance.gameObject.AddComponent<TextureCache>();
				}
			}
			return textureCache;
		}}
		static TextureCache textureCache = null;
		public async Task<byte[]> LoadFile(FileInfo file) {
			byte[] result;
			using (var stream = file.OpenRead()) {
				result = new byte[stream.Length];
				await stream.ReadAsync(result, 0, (int)stream.Length).ConfigureAwait(false);
			}
			return result;
		}

		protected virtual void SetTextures(T manager) {
			foreach (var component in manager.GetComponentsInChildren<Component>(true)) {
				Sprite sprite;
				switch (component) {
					case SpriteRenderer renderer when string.Equals(renderer.name, "BG", StringComparison.Ordinal):
					{
						sprite = newAssets["Background"];
						if (sprite != null) {
							renderer.sprite = sprite;
						} else {
							renderer.gameObject.SetActive(false);
						}
						break;
					}
					case SpriteRenderer renderer when renderer.name.Contains("Floor"):
					{
						sprite = newAssets["Floor"];
						if (sprite != null) {
							renderer.sprite = sprite;
						} else {
							renderer.gameObject.SetActive(false);
						}
						break;
					}
					case SpriteRenderer renderer when renderer.name.Contains("Under"):
					{
						sprite = newAssets["FloorUnder"];
						if (sprite != null) {
							renderer.sprite = sprite;
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
		[Obsolete("Undergoing replacement")]
		protected virtual Texture2D ImageLoad(string name, string currentStagePath) {
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
		readonly protected internal Dictionary<string, Sprite> newAssets;
		protected virtual void SetScratches(string stageName, T manager) {
			var scratch1 = newAssets["Scratch1"];
			var scratch2 = newAssets["Scratch2"];
			var scratch3 = newAssets["Scratch3"];
			if (scratch3 == null && scratch2 == null && scratch1 == null) {
				manager.scratchPrefabs = Array.Empty<GameObject>();
				return;
			}
			Sprite sprite;
			#if DEBUG
			string log = "CustomMapUtility: SetScratches: {";
			#endif
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
						sprite = scratch3;
						if (sprite != null) {
							if (i == 2) {
								current = UnityEngine.Object.Instantiate(manager.scratchPrefabs[2]);
								current.name = stageName+name;
								current.GetComponent<SpriteRenderer>().sprite = sprite;
							} else {
								current = manager.scratchPrefabs[2];
							}
							break;
						}
						goto case 1;
					}
					case 1: {
						sprite = scratch2;
						if (sprite != null) {
							if (i == 1) {
								current = UnityEngine.Object.Instantiate(manager.scratchPrefabs[1]);
								current.name = stageName+name;
								current.GetComponent<SpriteRenderer>().sprite = sprite;
							} else {
								current = manager.scratchPrefabs[1];
							}
							break;
						}
						goto case 0;
					}
					case 0: {
						sprite = scratch1;
						if (sprite != null) {
							if (i == 0) {
								current = UnityEngine.Object.Instantiate(manager.scratchPrefabs[0]);
								current.name = stageName+name;
								current.GetComponent<SpriteRenderer>().sprite = sprite;
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
				#if DEBUG
				log += $"{Environment.NewLine}	Scratch{i} = {current?.name ?? "null"}";
				#endif
			}
			#if DEBUG
			log += Environment.NewLine+"}";
			Debug.Log(log);
			#endif
		}
		#endregion
		#region MANAGER
		protected virtual T InitManager(GameObject mapObject) {
			MapManager original = mapObject.GetComponent<MapManager>();
			T newManager = mapObject.AddComponent<T>();

			newManager.Handler = handler;

			newManager.isActivated = original.isActivated;
			newManager.isEnabled = original.isEnabled;
			newManager.mapSize = original.mapSize;
			newManager.sephirahType = original.sephirahType;
			newManager.borderFrame = original.borderFrame;
			newManager.backgroundRoot = original.backgroundRoot;
			newManager.sephirahColor = original.sephirahColor;
			newManager.scratchPrefabs = original.scratchPrefabs;
			newManager.wallCratersPrefabs = original.wallCratersPrefabs;
			newManager._roots = original._roots;
			newManager._obstacleRoot = original._obstacleRoot;

			UnityEngine.Object.Destroy(original);
			manager = newManager;
			return newManager;
		}
		#endregion
	}
}