using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
    /// <summary>
    /// Contains a set of image offsets ranging from 0 to 1 for initializing the stage
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
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
    /// <summary>
    ///  Contains all CustomMapUtility commands.
    /// </summary>
    public class CustomMapHandler {
        #pragma warning disable IDE0051,IDE0059,CS0219,IDE1006
        sealed class A_ReadMe : CustomMapUtility.A_ReadMe {
            const string _text = text;
            const string _text2 = text2;
            const string _URL = URL;
        }
        #pragma warning restore IDE0051,IDE0059,CS0219,IDE1006
        /// <summary>
        /// Initializes a custom map.
        /// Will automatically try to get previous init values for unspecified arguments.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        public static void InitCustomMap(string stageName, Type managerType) {
            bool initBGMs = true;
            if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets)) {
                initBGMs = mapAutoBgmCache[stageName];
                Debug.Log("CustomMapUtility: Loaded offsets from cache");
            } else {offsets = new Offsets(0.5f, 0.5f);}
            new CustomMapHandler().Init(stageName, managerType, offsets, isEgo: false, initBGMs);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Has flag for Ego.
        /// Will automatically try to get previous init values for unspecified arguments.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        public static void InitCustomMap(string stageName, Type managerType,
            bool isEgo = false) {
                bool initBGMs = true;
                if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets)) {
                    initBGMs = mapAutoBgmCache[stageName];
                    Debug.Log("CustomMapUtility: Loaded offsets from cache");
                }
                else {offsets = new Offsets(0.5f, 0.5f);}
                new CustomMapHandler().Init(stageName, managerType, offsets, isEgo, initBGMs);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Has flags for Ego and whether to Initialize BGM.
        /// Will automatically try to get previous init values for unspecified arguments.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        public static void InitCustomMap(string stageName, Type managerType,
            bool isEgo = false, bool initBGMs = true) {
                if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets))
                {
                    Debug.Log("CustomMapUtility: Loaded offsets from cache");
                }
                else { offsets = new Offsets(0.5f, 0.5f); }
                new CustomMapHandler().Init(stageName, managerType, offsets, isEgo, initBGMs);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Has arguments for various offsets.
        /// Will automatically try to get previous init values for unspecified arguments.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        /// <param name="bgx">Background x pivot</param>
        /// <param name="bgy">Background y pivot</param>
        /// <param name="floorx">Floor x pivot</param>
        /// <param name="floory">Floor y pivot</param>
        /// <param name="underx">FloorUnder x pivot</param>
        /// <param name="undery">FloorUnder y pivot</param>
        public static void InitCustomMap(string stageName, Type managerType,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {
                if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
                else {initBGMs = true;}
                var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
                new CustomMapHandler().Init(stageName, managerType, offsets, isEgo: false, initBGMs);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Has flag for Ego and arguments for various offsets.
        /// Will automatically try to get previous init values for unspecified arguments.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        /// <param name="bgx">Background x pivot</param>
        /// <param name="bgy">Background y pivot</param>
        /// <param name="floorx">Floor x pivot</param>
        /// <param name="floory">Floor y pivot</param>
        /// <param name="underx">FloorUnder x pivot</param>
        /// <param name="undery">FloorUnder y pivot</param>
        public static void InitCustomMap(string stageName, Type managerType,
            bool isEgo = false,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {
                if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
                else {initBGMs = true;}
                var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
                new CustomMapHandler().Init(stageName, managerType, offsets, isEgo, initBGMs);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Has flags for Ego, whether to Initialize BGM, and arguments for various offsets.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        /// <param name="bgx">Background x pivot</param>
        /// <param name="bgy">Background y pivot</param>
        /// <param name="floorx">Floor x pivot</param>
        /// <param name="floory">Floor y pivot</param>
        /// <param name="underx">FloorUnder x pivot</param>
        /// <param name="undery">FloorUnder y pivot</param>
        public static void InitCustomMap(string stageName, Type managerType,
            bool isEgo = false, bool initBGMs = true,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {
                var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
                new CustomMapHandler().Init(stageName, managerType, offsets, isEgo, initBGMs);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Will automatically try to get previous init values for isEgo and initBGMs.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        /// <param name="offsets">A user-defined Offsets struct</param>
        public static void InitCustomMap(string stageName, Type managerType,
            Offsets offsets) {
                if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
                else {initBGMs = true;}
                new CustomMapHandler().Init(stageName, managerType, offsets, isEgo: false, initBGMs);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Has flag for Ego.
        /// Will automatically try to get previous init value for initBGMs.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        /// <param name="offsets">A user-defined Offsets struct</param>
        public static void InitCustomMap(string stageName, Type managerType,
            Offsets offsets,
            bool isEgo = false) {
                if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
                else {initBGMs = true;}
                new CustomMapHandler().Init(stageName, managerType, offsets, isEgo, initBGMs);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Has flags for Ego and whether to Initialize BGM.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        /// <param name="offsets">A user-defined Offsets struct</param>
        public static void InitCustomMap(string stageName, Type managerType,
            Offsets offsets,
            bool isEgo = false, bool initBGMs = true) {
                new CustomMapHandler().Init(stageName, managerType, offsets, isEgo, initBGMs);
        }
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(string stageName, MapManager manager) {}
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(string stageName, MapManager manager,
            bool isEgo = false) {}
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(string stageName, MapManager manager,
            bool isEgo = false, bool initBGMs = true) {}
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(string stageName, MapManager manager,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {}
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(string stageName, MapManager manager,
            bool isEgo = false,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {}
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(string stageName, MapManager manager,
            bool isEgo = false, bool initBGMs = true,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {}
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(string stageName, MapManager manager,
            Offsets offsets) {}
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(string stageName, MapManager manager,
            Offsets offsets,
            bool isEgo = false, bool initBGMs = true) {}

        protected void Init(string stageName, Type managerType, Offsets offsets, bool isEgo, bool initBGMs) {
            // Debug.LogWarning("CustomMapUtility: StageController.InitializeMap throwing a NullReferenceException on stage start is expected, you can freely ignore it");
            List<MapManager> addedMapList = SingletonBehavior<BattleSceneRoot>.Instance.GetType().GetField("_addedMapList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(SingletonBehavior<BattleSceneRoot>.Instance) as List<MapManager>;
            MapManager x2 = addedMapList?.Find((MapManager x) => x.name.Contains(stageName));
            if (x2 != null && managerType.Equals(x2)) {
                Debug.LogWarning("CustomMapUtility: Map already loaded");
                return;
            }

            GameObject mapObject = Util.LoadPrefab("InvitationMaps/InvitationMap_Philip1", SingletonBehavior<BattleSceneRoot>.Instance.transform);
            mapObject.name = "InvitationMap_"+stageName;
            var manager = this.InitManager(managerType, mapObject);

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
                log += Environment.NewLine+"    ";
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
            if (initBGMs) {
                try {
                    var managerTemp = manager as IBGM;
                    var bgms = managerTemp.GetCustomBGMs();
                    if (bgms != null && bgms.Length != 0) {
                        manager.mapBgm = CustomBgmParse(bgms);
                    } else {
                        // If you get this error and your method stops, you've called SingletonBehavior<BattleSoundManager>.Instance.OnStageStart(), don't do that. 
                        Debug.Log("CustomMapUtility: CustomBGMs is null or empty, enabling AutoBGM");
                        managerTemp.AutoBGM = true;
                    }
                } catch (NullReferenceException ex) {
                    Debug.LogError("CustomMapUtility: MapManager is not inherited from Custom(Creature)MapManager");
                    Debug.LogException(ex);
                } catch (Exception ex) {
                    Debug.LogError("CustomMapUtility: Failed to get BGMs");
                    Debug.LogException(ex);
                }
            } else {
                Debug.Log($"CustomMapUtility: BGM initialization for {stageName} is disabled");
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
        }

        private static readonly Dictionary<string, Offsets> mapOffsetsCache = new Dictionary<string, Offsets>(StringComparer.Ordinal);
        private static readonly Dictionary<string, bool> mapAutoBgmCache = new Dictionary<string, bool>(StringComparer.Ordinal);

        private void SetTextures(MapManager manager, Offsets offsets) {
            foreach (var component in manager.GetComponentsInChildren<Component>()) {
                switch (component) {
                    case SpriteRenderer renderer when string.Equals(renderer.name, "BG", StringComparison.Ordinal):
                    {
                        Texture2D texture = (Texture2D)newAssets["newBG"];
                        float pixelsPerUnit = (100f/1920f*(float)texture.width);
                        renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), offsets.BGOffset, pixelsPerUnit);
                        break;
                    }
                    case SpriteRenderer renderer when renderer.name.Contains("Floor"):
                    {
                        Texture2D texture = (Texture2D)newAssets["newFloor"];
                        float pixelsPerUnit = (100f/1920f*(float)texture.width);
                        renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), offsets.FloorOffset, pixelsPerUnit);
                        break;
                    }
                    case SpriteRenderer renderer when renderer.name.Contains("Under"):
                    {
                        Texture2D texture = (Texture2D)newAssets["newUnder"];
                        if (texture != null){
                            float pixelsPerUnit = (100f/1920f*(float)texture.width);
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
                texture.LoadImage(File.ReadAllBytes(currentStagePath+"/"+name+".png"));
            } catch {
                try {
                    texture.LoadImage(File.ReadAllBytes(currentStagePath+"/"+name+".jpg"));
                } catch {return null;}
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
                    log += $"{Environment.NewLine}    Scratch{i} = {current.name}";
                } catch {
                    log += $"{Environment.NewLine}    Scratch{i} = null";
                }
            }
            log += Environment.NewLine+"}";
            Debug.Log(log);
        }

        public static class ModResources {
            public class CacheInit : ModInitializer {
                #if !NOMP3
                public const string version = "2.0.1";
                #else
                public const string version = "2.0.1-NOMP3";
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
                            stagePathsDebug += $"{Environment.NewLine}    {dir.FullName}";
                        }
                        stagePathsDebug += Environment.NewLine+"}";
                        Debug.Log(stagePathsDebug);
                    }
                    if (_bgms != null && _bgms.Count != 0) {
                        string bgmsDebug = "CustomMapUtility BgmPaths: {";
                        foreach (var path in _bgms) {
                            bgmsDebug += $"{Environment.NewLine}    {path.FullName}";
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
                Debug.Log("CustomMapUtility: Loading stage from "+path);
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

            #if RootCopy
            var originalTypeBase = original.GetType().BaseType;
            var newManagerTypeBase = newManager.GetType();
            FieldInfo rootField = null;
            FieldInfo obstacleRootField = null;
            while (rootField == null || obstacleRootField == null) {
                newManagerTypeBase = newManagerTypeBase.BaseType;
                if (newManagerTypeBase == null) {
                    Debug.LogWarning("CustomMapUtility: InitManager had a minor error", newManager);
                    goto ManagerReturn;
                }
                try {
                    rootField = newManagerTypeBase.GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance);
                    rootField = newManagerTypeBase.GetField("_obstacleRoot", BindingFlags.NonPublic | BindingFlags.Instance);
                } catch {}
            }
            rootField.SetValue(newManager, originalTypeBase.GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original));
            obstacleRootField.SetValue(newManager, originalTypeBase.GetField("_obstacleRoot", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original));
            ManagerReturn:
            #endif

            UnityEngine.Object.Destroy(original);
            return newManager;
        }
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
        sealed private class AudioCache : MonoBehaviour {
            #pragma warning disable IDE0051
            public AudioClip Parse(string path, AudioType format) {
                // if (format == AudioType.WAV) {
                //     return ParseWAV(path);
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
                //      return ParseWAV(path);
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
            private static AudioClip ParseWAV(string path) => ParseWAV(new WAV(path));
            private static AudioClip ParseWAV(WAV wav)
            {
                if (wav.NumChannels > 8) {
                    throw new NotSupportedException("Unity does not support more than 8 audio channels per file");
                }
                var audioClip = AudioClip.Create("BGM", (int)wav.SampleCount, wav.NumChannels, (int)wav.SampleRate, stream: false);
                audioClip.SetData(wav.InterleavedAudio, 0);
                Debug.Log($"Parse Result: {wav}");
                return audioClip;
            }
            public void Async(string path, AudioType format, string bgmName) {
                if (!coroutines.ContainsKey(bgmName)) {
                    coroutines.Add(bgmName, StartCoroutine(ParseAsync(path, format, bgmName)));
                }
            }
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
                coroutines.Clear();
                wwwDic.Clear();
            }
            public readonly ConcurrentDictionary<string, AudioClip> Dictionary = new ConcurrentDictionary<string, AudioClip>(StringComparer.Ordinal);
            readonly Dictionary<string, Coroutine> coroutines = new Dictionary<string, Coroutine>(StringComparer.Ordinal);
            internal readonly ConcurrentDictionary<string, (UnityWebRequestAsyncOperation, (string, AudioType))> wwwDic = new ConcurrentDictionary<string, (UnityWebRequestAsyncOperation, (string, AudioType))>(StringComparer.Ordinal);
            // private Dictionary<string, WeakReference<AudioClip>> HeldTheme {get => CustomMapHandler.HeldTheme;}
            #pragma warning restore IDE0051
        }
        // Legacy cache implementation that's a pain to change and acts as a near-zero overhead redundancy.
        private static readonly Dictionary<string, WeakReference<AudioClip>> HeldTheme = new Dictionary<string, WeakReference<AudioClip>>(StringComparer.Ordinal);
        private static AudioCache CurrentCache = null;
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
        /// Loads a sound file and outputs it as an AudioClip.
        /// </summary>
        /// <param name="bgmName">The name of the audio file (including extension).</param>
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
        /// <summary>
        /// Returns a loaded AudioClip.
        /// </summary>
        /// <param name="bgmName">The name of the audio file (including extension).</param>
        public static AudioClip GetEnemyTheme(string bgmName) => GetAudioClip(bgmName);
        /// <summary>
        /// Returns a loaded AudioClip.
        /// </summary>
        /// <param name="bgmName">The name of the audio file (including extension).</param>
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
        /// <summary>
        /// Returns an array of loaded AudioClips.
        /// </summary>
        /// <param name="bgmNames">An array of the names of the audio files (including extension).</param>
        public static AudioClip[] GetAudioClip(string[] bgmNames) {
            AudioClip[] themes = new AudioClip[bgmNames.Length];
            for (int i = 0; i < bgmNames.Length; i++) {
                themes[i] = GetAudioClip(bgmNames[i]);
            }
            return themes;
        }
        /// <summary>
        /// Returns an array of loaded AudioClips.
        /// </summary>
        /// <param name="bgmNames">An array of the names of the audio files (including extension).</param>
        public static AudioClip[] GetAudioClips(string[] bgmNames) => GetAudioClip(bgmNames);
        /// <summary>
        /// Sets the current map's mapBgm.
        /// Also sets EnemyTheme to be sure.
        /// </summary>
        /// <param name="bgmName">The name of the audio file (including extension).</param>
        /// <param name="immediate">Whether it immediately forces the music to change</param>
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
        /// If mapName is null, changes Sephirah's mapBgm instead.
        /// Also sets EnemyTheme to be sure.
        /// </summary>
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
        /// <param name="num">Which map is chosen, or -1 for the Sephirah Map</param>
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
        /// <summary>
        /// Changes to a custom EGO map.
        /// </summary>
        /// <param name="mapName">The name of the target map</param>
        /// <param name="faction">Determines what direction the transition special effect starts from</param>
        /// <param name="manager">If not null, automatically reinitializes the map if it's been removed</param>
        /// <param name="byAssimilationFlag">Should always be false, don't change this yourself</param>
        public static void ChangeToCustomEgoMap(string mapName, Faction faction = Faction.Player, Type managerType = null, bool byAssimilationFlag = false) => SingletonBehavior<BattleSceneRoot>.Instance.ChangeToCustomEgoMap(mapName, faction, managerType, byAssimilationFlag);
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void ChangeToCustomEgoMap(string mapName, Faction faction, MapManager manager, bool byAssimilationFlag) => SingletonBehavior<BattleSceneRoot>.Instance.ChangeToCustomEgoMap(mapName, faction, manager, byAssimilationFlag);
        /// <summary>
        /// Adds and Changes to a custom synchonization map.
        /// </summary>
        /// <param name="name">The name of the target map</param>
        /// <param name="faction">Determines what direction the transition special effect starts from</param>
        /// <param name="manager">If not null, automatically reinitializes the map if it's been removed</param>
        /// <param name="byAssimilationFlag">Should always be false, don't change this yourself</param>
        public static void AddCustomEgoMapByAssimilation(string name, Faction faction = Faction.Player, Type managerType = null) => Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(name, faction, managerType);
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void AddCustomEgoMapByAssimilation(string name, Faction faction, MapManager manager) => Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(name, faction, manager);
        /// <summary>
        /// Adds and Changes to a custom synchonization map.
        /// </summary>
        /// <param name="mapName">The name of the target map</param>
        /// <param name="faction">Determines what direction the transition special effect starts from</param>
        /// <param name="manager">If not null, automatically reinitializes the map if it's been removed</param>
        /// <param name="byAssimilationFlag">Should always be false, don't change this yourself</param>
        public static void ChangeToCustomEgoMapByAssimilation(string mapName, Faction faction = Faction.Player, Type managerType = null) => Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(mapName, faction, managerType);
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void ChangeToCustomEgoMapByAssimilation(string mapName, Faction faction, MapManager manager) => Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(mapName, faction, manager);
        /// <summary>
        /// Removes a synchonization map.
        /// </summary>
        /// <param name="name">The name of the target map</param>
        public static void RemoveCustomEgoMapByAssimilation(string name) => Singleton<StageController>.Instance.RemoveEgoMapByAssimilation(name);
    }
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
        public static void ChangeToCustomEgoMap(this BattleSceneRoot Instance, string mapName, Faction faction = Faction.Player, Type managerType = null, bool byAssimilationFlag = false) {
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
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void ChangeToCustomEgoMap(this BattleSceneRoot Instance, string mapName, Faction faction = Faction.Player, MapManager manager = null, bool byAssimilationFlag = false) {}
        /// <summary>
        /// Adds and Changes to a custom synchonization map.
        /// </summary>
        /// <param name="mapName">The name of the target map</param>
        /// <param name="faction">Determines what direction the transition special effect starts from</param>
        /// <param name="managerType">If not null, automatically reinitializes the map if it's been removed</param>
        /// <param name="byAssimilationFlag">Should always be false, don't change this yourself</param>
        public static void ChangeToCustomEgoMapByAssimilation(this BattleSceneRoot _, string mapName, Faction faction = Faction.Player, Type managerType = null) => Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(mapName, faction, managerType);
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void ChangeToCustomEgoMapByAssimilation(this BattleSceneRoot _, string mapName, Faction faction = Faction.Player, MapManager manager = null) => Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(mapName, faction, manager);
        /// <summary>
        /// Adds and Changes to a custom synchonization map.
        /// </summary>
        /// <param name="name">The name of the target map</param>
        /// <param name="faction">Determines what direction the transition special effect starts from</param>
        /// <param name="managerType">If not null, automatically reinitializes the map if it's been removed</param>
        /// <param name="byAssimilationFlag">Should always be false, don't change this yourself</param>
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
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void AddCustomEgoMapByAssimilation(this StageController Instance, string name, Faction faction = Faction.Player, MapManager manager = null) {}
        /// <summary>
        /// Removes a synchonization map.
        /// </summary>
        /// <param name="name">The name of the target map</param>
        public static void RemoveCustomEgoMapByAssimilation(this StageController Instance, string name) => Instance.RemoveEgoMapByAssimilation(name);
        /// <summary>
        /// Initializes a custom map.
        /// Will automatically try to get previous init values for unspecified arguments.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType) {
            CustomMapHandler.InitCustomMap(stageName, managerType);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Has flag for Ego.
        /// Will automatically try to get previous init values for unspecified arguments.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
            bool isEgo = false) {
                CustomMapHandler.InitCustomMap(stageName, managerType, isEgo);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Has flags for Ego and whether to Initialize BGM.
        /// Will automatically try to get previous init values for unspecified arguments.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
            bool isEgo = false, bool initBGMs = true) {
                CustomMapHandler.InitCustomMap(stageName, managerType, isEgo, initBGMs);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Has arguments for various offsets.
        /// Will automatically try to get previous init values for unspecified arguments.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        /// <param name="bgx">Background x pivot</param>
        /// <param name="bgy">Background y pivot</param>
        /// <param name="floorx">Floor x pivot</param>
        /// <param name="floory">Floor y pivot</param>
        /// <param name="underx">FloorUnder x pivot</param>
        /// <param name="undery">FloorUnder y pivot</param>
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {
                CustomMapHandler.InitCustomMap(stageName, managerType, bgx, bgy, floorx, floory, underx, undery);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Has flag for Ego and arguments for various offsets.
        /// Will automatically try to get previous init values for unspecified arguments.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        /// <param name="bgx">Background x pivot</param>
        /// <param name="bgy">Background y pivot</param>
        /// <param name="floorx">Floor x pivot</param>
        /// <param name="floory">Floor y pivot</param>
        /// <param name="underx">FloorUnder x pivot</param>
        /// <param name="undery">FloorUnder y pivot</param>
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
            bool isEgo = false,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {
                CustomMapHandler.InitCustomMap(stageName, managerType, isEgo, bgx, bgy, floorx, floory, underx, undery);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Has flags for Ego, whether to Initialize BGM, and arguments for various offsets.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        /// <param name="bgx">Background x pivot</param>
        /// <param name="bgy">Background y pivot</param>
        /// <param name="floorx">Floor x pivot</param>
        /// <param name="floory">Floor y pivot</param>
        /// <param name="underx">FloorUnder x pivot</param>
        /// <param name="undery">FloorUnder y pivot</param>
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
            bool isEgo = false, bool initBGMs = true,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {
                CustomMapHandler.InitCustomMap(stageName, managerType, isEgo, initBGMs, bgx, bgy, floorx, floory, underx, undery);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Will automatically try to get previous init values for isEgo and initBGMs.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        /// <param name="offsets">A user-defined Offsets struct</param>
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
            Offsets offsets) {
                CustomMapHandler.InitCustomMap(stageName, managerType, offsets);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Has flag for Ego.
        /// Will automatically try to get previous init value for initBGMs.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        /// <param name="offsets">A user-defined Offsets struct</param>
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
            Offsets offsets,
            bool isEgo = false) {
                CustomMapHandler.InitCustomMap(stageName, managerType, offsets, isEgo);
        }
        /// <summary>
        /// Initializes a custom map.
        /// Has flags for Ego and whether to Initialize BGM.
        /// </summary>
        /// <param name="stageName">The name of your stage folder</param>
        /// <param name="managerType">Your custom map manager</param>
        /// <param name="offsets">A user-defined Offsets struct</param>
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, Type managerType,
            Offsets offsets,
            bool isEgo = false, bool initBGMs = true) {
                CustomMapHandler.InitCustomMap(stageName, managerType, offsets, isEgo, initBGMs);
        }
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager)
            => CustomMapHandler.InitCustomMap(stageName, manager.GetType());
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
            bool isEgo = false)
                => CustomMapHandler.InitCustomMap(stageName, manager.GetType(), isEgo);
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
            bool isEgo = false, bool initBGMs = true)
                => CustomMapHandler.InitCustomMap(stageName, manager.GetType(), isEgo, initBGMs);
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f / 1080f),
            float underx = 0.5f, float undery = (300f / 1080f))
                => CustomMapHandler.InitCustomMap(stageName, manager.GetType(), bgx, bgy, floorx, floory, underx, undery);
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
            bool isEgo = false,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f / 1080f),
            float underx = 0.5f, float undery = (300f / 1080f))
                => CustomMapHandler.InitCustomMap(stageName, manager.GetType(), isEgo, bgx, bgy, floorx, floory, underx, undery);
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
            bool isEgo = false, bool initBGMs = true,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f / 1080f),
            float underx = 0.5f, float undery = (300f / 1080f))
                => CustomMapHandler.InitCustomMap(stageName, manager.GetType(), isEgo, initBGMs, bgx, bgy, floorx, floory, underx, undery);
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
            Offsets offsets)
                => CustomMapHandler.InitCustomMap(stageName, manager.GetType(), offsets);
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
            Offsets offsets,
            bool isEgo = false)
                => CustomMapHandler.InitCustomMap(stageName, manager.GetType(), offsets, isEgo);
        [Obsolete("Change (new MapManager) to (typeof(MapManager))", true)]
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
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
    interface IBGM {
        string[] GetCustomBGMs();
        bool AutoBGM {get; set;}
    }

    public class CustomMapManager : MapManager, IBGM {
        public override void EnableMap(bool b) {
            base.EnableMap(b);
            this.gameObject.SetActive(b);
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
            if (this.scratchPrefabs.Length == 0 || this.scratchPrefabs[lv] == null) {
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
            this.gameObject.SetActive(b);
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
            if (this.scratchPrefabs.Length == 0 || this.scratchPrefabs[lv] == null) {
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
}