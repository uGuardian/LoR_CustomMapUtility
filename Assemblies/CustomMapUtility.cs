using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using NAudio.Wave;
using Mod;
#pragma warning disable MA0048,MA0076

namespace CustomMapUtility {
    public class CustomMapHandler {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
        public readonly struct Offsets {
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
            public readonly Vector2 BGOffset;
            public readonly Vector2 FloorOffset;
            public readonly Vector2 UnderOffset;
        }
        public static void InitCustomMap(string stageName, MapManager manager) {
            bool initBGMs = true;
            if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets)) {
                initBGMs = mapAutoBgmCache[stageName];
                Debug.Log("CustomMapUtility: Loaded offsets from cache");
            } else {offsets = new Offsets(0.5f, 0.5f);}
            new CustomMapHandler().Init(stageName, manager, offsets, isEgo: false, initBGMs);
        }
        public static void InitCustomMap(string stageName, MapManager manager,
            bool isEgo = false) {
                bool initBGMs = true;
                if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets)) {
                    initBGMs = mapAutoBgmCache[stageName];
                    Debug.Log("CustomMapUtility: Loaded offsets from cache");
                }
                else {offsets = new Offsets(0.5f, 0.5f);}
                new CustomMapHandler().Init(stageName, manager, offsets, isEgo, initBGMs);
        }
        public static void InitCustomMap(string stageName, MapManager manager,
            bool isEgo = false, bool initBGMs = true) {
                if (mapOffsetsCache.TryGetValue(stageName, out Offsets offsets))
                {
                    Debug.Log("CustomMapUtility: Loaded offsets from cache");
                }
                else { offsets = new Offsets(0.5f, 0.5f); }
                new CustomMapHandler().Init(stageName, manager, offsets, isEgo, initBGMs);
        }
        public static void InitCustomMap(string stageName, MapManager manager,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {
            if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
            var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
                new CustomMapHandler().Init(stageName, manager, offsets, isEgo: false, initBGMs);
        }
        public static void InitCustomMap(string stageName, MapManager manager,
            bool isEgo = false,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {
                if (mapAutoBgmCache.TryGetValue(stageName, out bool initBGMs)) { }
                var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
                new CustomMapHandler().Init(stageName, manager, offsets, isEgo, initBGMs);
        }
        public static void InitCustomMap(string stageName, MapManager manager,
            bool isEgo = false, bool initBGMs = true,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {
                var offsets = new Offsets(bgx, bgy, floorx, floory, underx, undery);
                new CustomMapHandler().Init(stageName, manager, offsets, isEgo, initBGMs);
        }

        private void Init(string stageName, MapManager manager, Offsets offsets, bool isEgo, bool initBGMs) {
            List<MapManager> addedMapList = SingletonBehavior<BattleSceneRoot>.Instance.GetType().GetField("_addedMapList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(SingletonBehavior<BattleSceneRoot>.Instance) as List<MapManager>;
            MapManager x2 = addedMapList?.Find((MapManager x) => x.name.Contains(stageName));
            if (x2 != null && x2.Equals(manager.GetType())) {
                Debug.LogWarning("CustomMapUtility: Map already loaded, using it");
                manager = x2;
                return;
            }

            GameObject mapObject = Util.LoadPrefab("InvitationMaps/InvitationMap_Philip1", SingletonBehavior<BattleSceneRoot>.Instance.transform);
            mapObject.name = "InvitationMap_"+stageName;
            manager = this.InitManager(manager, mapObject);
            var modResources = new ModResources();

            var currentStagePath = modResources.GetStagePath(stageName);
            newAssets = new Dictionary<string, Texture2D>(StringComparer.Ordinal){
                {"newBG", ImageLoad("Background", currentStagePath)},
                {"newFloor", ImageLoad("Floor", currentStagePath)},
                {"newUnder", ImageLoad("FloorUnder", currentStagePath)},
                {"scratch1", ImageLoad("Scratch1", currentStagePath)},
                {"scratch2", ImageLoad("Scratch2", currentStagePath)},
                {"scratch3", ImageLoad("Scratch3", currentStagePath)},
            };
            string log = "CustomMapUtility: New Assets: {";
            foreach (var img in newAssets) {
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
                    var managerTemp = manager as CustomMapManager;
                    if (managerTemp.CustomBGMs != null) {
                        manager.mapBgm = CustomBgmParse(managerTemp.CustomBGMs);
                    } else {
                        // If you get this error and your method stops, you've called SingletonBehavior<BattleSoundManager>.Instance.OnStageStart(), don't do that. 
                        Debug.LogError("CustomMapUtility: CustomBGMs is null or empty, filling with current themes");
                    }
                } catch (NullReferenceException) {
                    try {
                        var managerTemp = manager as CustomCreatureMapManager;
                        if (managerTemp.CustomBGMs != null && managerTemp.CustomBGMs.Length != 0) {
                            manager.mapBgm = CustomBgmParse(managerTemp.CustomBGMs);
                        } else {
                            // If you get this error and your method stops, you've called SingletonBehavior<BattleSoundManager>.Instance.OnStageStart(), don't do that. 
                            Debug.LogError("CustomMapUtility: CustomBGMs is null or empty, filling with current themes");
                        }
                    } catch (NullReferenceException ex) {
                        Debug.LogError($"CustomMapUtility: MapManager is not inherited from Custom(Creature)MapManager{Environment.NewLine}{ex}{Environment.NewLine}");
                    }
                }
            } else {
                Debug.LogWarning("CustomMapUtility: Auto BGM initialization is off");
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
                        var texture = newAssets["newBG"];
                        float pixelsPerUnit = (100f/1920f*(float)texture.width);
                        renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), offsets.BGOffset, pixelsPerUnit);
                        break;
                    }
                    case SpriteRenderer renderer when renderer.name.Contains("Floor"):
                    {
                        var texture = newAssets["newFloor"];
                        float pixelsPerUnit = (100f/1920f*(float)texture.width);
                        renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), offsets.FloorOffset, pixelsPerUnit);
                        break;
                    }
                    case SpriteRenderer renderer when renderer.name.Contains("Under"):
                    {
                        var texture = newAssets["newUnder"];
                        if (texture != null){
                            float pixelsPerUnit = (100f/1920f*(float)texture.width);
                            renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), offsets.UnderOffset, pixelsPerUnit);
                        } else {
                            #pragma warning disable MA0003
                            renderer.gameObject.SetActive(false);
                        }
                        break;
                    }
                    default:
                    {
                        if (component.name.Contains("Effect") || component.name.Contains("Filter")) {
                            component.gameObject.SetActive(false);
                            #pragma warning restore MA0003
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
        #pragma warning disable MA0016
        public Dictionary<string, Texture2D> newAssets;
        #pragma warning restore MA0016
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
                        texture = newAssets["scratch3"];
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
                        texture = newAssets["scratch2"];
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
                        texture = newAssets["scratch1"];
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

        public class ModResources {
            public class CacheInit : ModInitializer {
                const string version = "1.0.1";
                public override void OnInitializeMod()
                {
                    var tempInstance = new ModResources();
                    if (string.Equals(Assembly.GetExecutingAssembly().GetName().Name, "ConfigAPI", StringComparison.Ordinal)) {
                        _dirInfos =
                            from modInfo in Mod.ModContentInfoLoader.LoadAllModInfos()
                            // where modInfo.activated == true
                            select modInfo.dirInfo;
                        Debug.Log($"CustomMapUtility Version \"{version}\" in Global Mode");
                    } else {
                        var curDir = new DirectoryInfo(Assembly.GetExecutingAssembly().Location+"\\..\\..");
                        Debug.Log($"CustomMapUtility Version \"{version}\" in Local Mode at {curDir.FullName}");
                        _dirInfos = new DirectoryInfo[]{curDir};
                    }
                    _stagePaths = tempInstance.GetStageRootPaths();
                    _bgms = tempInstance.GetStageBgmInfos();
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
                    this.RemoveError();
                }
                public void RemoveError() {
                    var list = new List<string>();
                    var list2 = new List<string>();
                    list.Add("NAudio");
                    list.Add("netstandard");
                    using (var enumerator = Singleton<ModContentManager>.Instance.GetErrorLogs().GetEnumerator()) {
                        while (enumerator.MoveNext()) {
                            var errorLog = enumerator.Current;
                            if (list.Exists(x => errorLog.Contains(x))) {
                                list2.Add(errorLog);
                            }
                        }
                    }
                    foreach (var item in list2) {
                        Singleton<ModContentManager>.Instance.GetErrorLogs().Remove(item);
                    }
                }
            }
            public string GetStagePath(string stageName) {
                IEnumerable<DirectoryInfo> stagePaths =
                    from info in GetStageRootPaths()
                    where string.Equals(info.Name, stageName, StringComparison.Ordinal)
                    select info;
                string path = null;
                foreach (var dir in stagePaths) {
                    if (path == null) {
                        path = dir.FullName;
                    } else {
                        throw new ArgumentException("Multiple stages share this name", nameof(stageName));
                    }
                }
                if (path == null) {
                    throw new ArgumentNullException(nameof(stageName), "Stage does not exist");
                }
                Debug.Log("CustomMapUtility: Loading stage from "+path);
                return path;
            }
            [Obsolete("Use GetStageBgmInfos() instead")]
            private List<FileInfo> GetStageBgmPaths() => GetStageBgmInfos();
            private static List<FileInfo> _bgms;
            [Obsolete("Use GetStageBgmInfos(string[]) instead")]
            public string[] GetStageBgmPaths(string[] bgmNames) {
                int debugCount = 0;
                foreach (var bgm in bgmNames) {
                    Debug.Log($"CustomMapUtility: BGM{debugCount}: {bgm}{Environment.NewLine}");
                    debugCount++;
                }
                IEnumerable<string> bgms =
                    from file in GetStageBgmPaths()
                    where bgmNames.Any(b => string.Equals(b, file.Name, StringComparison.OrdinalIgnoreCase))
                    select file.FullName;
                return bgms.ToArray();
            }
            private List<FileInfo> GetStageBgmInfos() {
                if (_bgms != null && _bgms.Count != 0) {
                    return _bgms;
                }
                List<FileInfo> bgms = new List<FileInfo>();
                foreach (DirectoryInfo dir in _dirInfos) {
                    DirectoryInfo bgmsPath = new DirectoryInfo(Path.Combine(dir.FullName, "Resource\\StageBgm"));
                    if (bgmsPath.Exists) {
                        foreach (FileInfo file in bgmsPath.GetFiles()) {
                            bgms.Add(file);
                        }
                    }
                }
                return bgms;
            }
            public FileInfo[] GetStageBgmInfos(string[] bgmNames) {
                IEnumerable<FileInfo> bgms =
                    from file in GetStageBgmInfos()
                    where bgmNames.Any(b => string.Equals(b, file.Name, StringComparison.OrdinalIgnoreCase))
                    select file;
                return bgms.ToArray();
            }
            private static IEnumerable<DirectoryInfo> _dirInfos;
            private List<DirectoryInfo> GetStageRootPaths() {
                if (_stagePaths != null && _stagePaths.Count != 0) {
                    return _stagePaths;
                }
                List<DirectoryInfo> paths = new List<DirectoryInfo>();
                foreach (DirectoryInfo dir in _dirInfos) {
                    DirectoryInfo stagePath = new DirectoryInfo(Path.Combine(dir.FullName, "Resource\\Stage"));
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
        public MapManager InitManager(MapManager manager, GameObject mapObject) {
            MapManager original = mapObject.GetComponent<MapManager>();
            MapManager newManager = mapObject.AddComponent(manager.GetType()) as MapManager;

            newManager.isActivated = original.isActivated;
            newManager.isEnabled = original.isEnabled;
            newManager.mapSize = original.mapSize;
            newManager.isCreature = original.isCreature;
            newManager.isBossPhase = original.isBossPhase;
            newManager.isEgo = original.isEgo;
            newManager.sephirahType = original.sephirahType;
            newManager.borderFrame = original.borderFrame;
            newManager.backgroundRoot = original.backgroundRoot;
            newManager.sephirahColor = original.sephirahColor;
            newManager.scratchPrefabs = original.scratchPrefabs;
            newManager.wallCratersPrefabs = original.wallCratersPrefabs;

            UnityEngine.Object.Destroy(original);
            return newManager;
        }
        public static AudioClip CustomBgmParse(string BGM) {
            return CustomMapHandler.CustomBgmParse(new string[]{BGM})[0];
        }
        public static AudioClip[] CustomBgmParse(string[] BGMs) {
            var resources = new ModResources();
            var files = resources.GetStageBgmInfos(BGMs);
            AudioClip[] output = new AudioClip[BGMs.Length];
            var handler = new AudioHandler();
            for (int i = 0; i < BGMs.Length; i++) {
                try {
                    var info = new FileInfo(BGMs[i]);
                    Debug.Log($"CustomMapUtility: BGM{i} = {BGMs[i]}");
                    if (string.Equals(info.Extension, ".mp3", StringComparison.OrdinalIgnoreCase)) {
                        output[i] = handler.Parse(files[i].FullName, AudioHandler.FileType.mp3);
                    } else if (string.Equals(info.Extension, ".wav", StringComparison.OrdinalIgnoreCase)) {
                        output[i] = handler.Parse(files[i].FullName, AudioHandler.FileType.wav);
                    } else {
                        output[i] = handler.Parse(files[i].FullName, AudioHandler.FileType.unknown);
                    }
                } catch (Exception ex) {
                    Debug.LogException(ex);
                    output[i] = null;
                }
            }
            return output;
        }
        internal class AudioHandler {
            [Obsolete("Use Parse(string, FileType) Instead")]
            internal AudioClip Convert(string path, bool mp3 = true) => Parse(path, FileType.mp3);
            internal AudioClip Parse(string path, FileType format)
            {
                Wav wav;
                if (format == FileType.mp3) {
                    using (var sourceProvider = new Mp3FileReader(path)) {
                        MemoryStream stream = new MemoryStream();
                        WaveFileWriter.WriteWavFileToStream(stream, sourceProvider);
                        wav = new Wav(stream.ToArray());
                    }
                } else if (format == FileType.wav) {
                    wav = new Wav(path);
                } else {
                    // using (var sourceProvider = new MediaFoundationReader(path)) {
                    //     MemoryStream stream = new MemoryStream();
                    //     WaveFileWriter.WriteWavFileToStream(stream, sourceProvider);
                    //     wav = new Wav(stream);
                    // }
                    throw new NotSupportedException("Only wav and mp3 are supported");
                }
                if (wav.NumChannels > 8) {
                    throw new NotSupportedException("Unity does not support more than 8 audio channels per file");
                }
                var audioClip = AudioClip.Create("BGM", (int)wav.SampleCount, wav.NumChannels, (int)wav.SampleRate, stream: false);
                audioClip.SetData(wav.InterleavedAudio, 0);
                // File.Delete(path + ".wav");
                Debug.Log($"Parse Result: {wav}");
                return audioClip;
            }
            internal enum FileType {
                unknown = -1,
                wav = 0,
                mp3 = 1,
            }
        }
        private static readonly Dictionary<string, Task> HeldTask = new Dictionary<string, Task>(StringComparer.Ordinal);
        private static readonly Dictionary<string, WeakReference<AudioClip>> HeldTheme = new Dictionary<string, WeakReference<AudioClip>>(StringComparer.Ordinal);
        public static void SetEnemyTheme(string bgmName, bool immediate = true) {
            LoadEnemyTheme(bgmName);
            #pragma warning disable MA0003
            if (!immediate) {
                StartEnemyTheme(bgmName, false);
            } else {
                StartEnemyTheme(bgmName, true);
            }
            #pragma warning restore MA0003
        }
        public static void LoadEnemyTheme(string bgmName) {
            if (HeldTask.ContainsKey(bgmName)) {
                if (HeldTheme[bgmName].TryGetTarget(out AudioClip _)) {
                    return;
                }
                HeldTheme.Remove(bgmName);
            }
            var task = Task.Run(() => {
                if (!HeldTheme.ContainsKey(bgmName)) {
                    HeldTheme[bgmName] = new WeakReference<AudioClip>(CustomBgmParse(bgmName));
                }
                if (!HeldTheme[bgmName].TryGetTarget(out AudioClip _)) {
                    HeldTheme[bgmName].SetTarget(CustomBgmParse(bgmName));
                }
            });
            Debug.Log($"CustomMapUtility:AudioHandler:Task: Holding EnemyTheme {bgmName}");
            HeldTask[bgmName] = task;
        }
        public static void LoadEnemyTheme(string bgmName, out AudioClip clip) {
            LoadEnemyTheme(bgmName);
            HeldTheme[bgmName].TryGetTarget(out AudioClip temp);
            clip = temp;
        }
        [Obsolete("Please call StartEnemyTheme(bgmName) intead")]
        public static void StartEnemyTheme() {
            var last = HeldTask.Last();
            StartEnemyTheme(last.Key);
        }
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
        public static AudioClip GetEnemyTheme(string bgmName) => GetAudioClip(bgmName);
        public static AudioClip GetAudioClip(string bgmName) {
            if (!HeldTask.ContainsKey(bgmName)) {
                if (!HeldTheme[bgmName].TryGetTarget(out AudioClip _)) {
                    Debug.LogWarning("CustomMapUtility:AudioHandler: Theme was not already loaded, preload the theme with LoadEnemyTheme(bgmName) if possible");
                    LoadEnemyTheme(bgmName);
                }
                Debug.LogWarning("CustomMapUtility:AudioHandler: Entry does not exist in held themes but existed in cache");
            } else {
                HeldTask.Remove(bgmName, out Task task);
                task.Wait();
            }
            if (!HeldTheme[bgmName].TryGetTarget(out AudioClip theme)) {
                Debug.LogWarning("CustomMapUtility:AudioHandler: Entry was dropped from memory, maybe it was held for too long? Reloading entry");
                LoadEnemyTheme(bgmName);
            }
            Debug.Log($"CustomMapUtility:AudioHandler:Task: Got EnemyTheme {bgmName}");
            return theme;
        }
        public static void SetMapBgm(string bgmName, bool immediate = true) {
            LoadEnemyTheme(bgmName);
            SingletonBehavior<BattleSceneRoot>.Instance.currentMapObject.mapBgm = new AudioClip[]{GetAudioClip(bgmName)};
            SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(SingletonBehavior<BattleSceneRoot>.Instance.currentMapObject.mapBgm);
            if (immediate) {
                SingletonBehavior<BattleSoundManager>.Instance.ChangeEnemyTheme(0);
            }
        }
        public static void LoadMapBgm(string bgmName, bool immediate = true) {
            SingletonBehavior<BattleSceneRoot>.Instance.currentMapObject.mapBgm = new AudioClip[]{GetAudioClip(bgmName)};
            SingletonBehavior<BattleSoundManager>.Instance.SetEnemyTheme(SingletonBehavior<BattleSceneRoot>.Instance.currentMapObject.mapBgm);
            if (immediate) {
                SingletonBehavior<BattleSoundManager>.Instance.ChangeEnemyTheme(0);
            }
        }

        public static void EnforceMap(int num = 0) {
            var emotionTotalCoinNumber = Singleton<StageController>.Instance.GetCurrentStageFloorModel().team.emotionTotalCoinNumber;
            Singleton<StageController>.Instance.GetCurrentWaveModel().team.emotionTotalBonus = emotionTotalCoinNumber + 1;
            Singleton<StageController>.Instance.GetStageModel().SetCurrentMapInfo(num);
        }
        public static void ChangeToCustomEgoMap(string mapName, Faction faction = Faction.Player, MapManager manager = null, bool byAssimilationFlag = false) {
            SingletonBehavior<BattleSceneRoot>.Instance.ChangeToCustomEgoMap(mapName, faction, manager, byAssimilationFlag); 
        }
        public static void AddCustomEgoMapByAssimilation(string name, Faction faction = Faction.Player, MapManager manager = null) {
            Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(name, faction, manager);
        }
        public static void ChangeToCustomEgoMapByAssimilation(string mapName, Faction faction = Faction.Player, MapManager manager = null) {
            Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(mapName, faction, manager);
        }
        public static void RemoveCustomEgoMapByAssimilation(string name) {
            Singleton<StageController>.Instance.RemoveEgoMapByAssimilation(name);
            // Singleton<StageController>.Instance.RemoveEgoMapAll();
		    // SingletonBehavior<BattleSceneRoot>.Instance.RemoveEgoMapAll();
        }
    }
    public static class CustomMapUtilityExtensions {
        public static void ChangeToCustomEgoMap(this BattleSceneRoot Instance, string mapName, Faction faction = Faction.Player, MapManager manager = null, bool byAssimilationFlag = false) {
            if (String.IsNullOrWhiteSpace(mapName))
            {
                Debug.LogError("CustomMapUtility: Ego map not specified");
                return;
            }
            List<MapManager> addedMapList = Instance.GetType().GetField("_addedMapList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Instance) as List<MapManager>;
            MapChangeFilter mapChangeFilter = Instance.GetType().GetField("_mapChangeFilter", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Instance) as MapChangeFilter;
            MapManager x2 = addedMapList?.Find((MapManager x) => x.name.Contains(mapName));
            if (x2 == null && manager == null) {
                Debug.LogError("CustomMapUtility: Ego map not initialized");
                return;
            }
            if (x2 == null)
            {
                Debug.LogWarning("CustomMapUtility: Reinitializing Ego map");
                CustomMapHandler.InitCustomMap(mapName, manager, isEgo: true);
                x2 = manager;
            }
            mapChangeFilter.StartMapChangingEffect((Direction)faction, particleOn: true);
            x2.isBossPhase = false;
            x2.isEgo = true;
            if (x2 != Instance.currentMapObject)
            {
                Instance.currentMapObject.EnableMap(false);
                Instance.currentMapObject = x2;
                if (!byAssimilationFlag) {
                    #pragma warning disable MA0003
                    Instance.currentMapObject.ActiveMap(true);
                    #pragma warning restore MA0003
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
        public static void ChangeToCustomEgoMapByAssimilation(this BattleSceneRoot _, string mapName, Faction faction = Faction.Player, MapManager manager = null) {
            Singleton<StageController>.Instance.AddCustomEgoMapByAssimilation(mapName, faction, manager);
        }
        public static void AddCustomEgoMapByAssimilation(this StageController Instance, string name, Faction faction = Faction.Player, MapManager manager = null) {
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
                    SingletonBehavior<BattleSceneRoot>.Instance.ChangeToCustomEgoMap(name, faction, manager, byAssimilationFlag: true);
                } else {
                    SingletonBehavior<BattleSceneRoot>.Instance.ChangeToSpecialMap(name, playEffect: true, scaleChange: false);
                }
            }
        }
        public static void RemoveCustomEgoMapByAssimilation(this StageController Instance, string name) {
            Instance.RemoveEgoMapByAssimilation(name);
            // Instance.RemoveEgoMapAll();
		    // SingletonBehavior<BattleSceneRoot>.Instance.RemoveEgoMapAll();
        }
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager) {
            CustomMapHandler.InitCustomMap(stageName, manager);
        }
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
            bool isEgo = false) {
                CustomMapHandler.InitCustomMap(stageName, manager, isEgo);
        }
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
            bool isEgo = false, bool initBGMs = true) {
                CustomMapHandler.InitCustomMap(stageName, manager, isEgo, initBGMs);
        }
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {
                CustomMapHandler.InitCustomMap(stageName, manager, bgx, bgy, floorx, floory, underx, undery);
        }
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
            bool isEgo = false,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {
                CustomMapHandler.InitCustomMap(stageName, manager, isEgo, bgx, bgy, floorx, floory, underx, undery);
        }
        public static void InitCustomMap(this BattleSceneRoot _, string stageName, MapManager manager,
            bool isEgo = false, bool initBGMs = true,
            float bgx = 0.5f, float bgy = 0.5f,
            float floorx = 0.5f, float floory = (407.5f/1080f),
            float underx = 0.5f, float undery = (300f/1080f)) {
                CustomMapHandler.InitCustomMap(stageName, manager, isEgo, initBGMs, bgx, bgy, floorx, floory, underx, undery);
        }
    }

    public class CustomMapManager : MapManager {
        public override void EnableMap(bool b) {
            base.EnableMap(b);
            this.gameObject.SetActive(b);
            SingletonBehavior<BattleCamManager>.Instance.BlurBackgroundCam(!b);
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
            #pragma warning disable MA0003
            SingletonBehavior<BattleCamManager>.Instance.BlurBackgroundCam(true);
            #pragma warning restore MA0003
            Debug.Log("CustomMapUtility: Cleaned up custom objects");
            base.ResetMap();
        }
        protected internal virtual string[] CustomBGMs {get;}
    }
    public class CustomCreatureMapManager : CreatureMapManager {
        public override void EnableMap(bool b) {
            base.EnableMap(b);
            this.gameObject.SetActive(b);
            SingletonBehavior<BattleCamManager>.Instance.BlurBackgroundCam(!b);
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
            #pragma warning disable MA0003
            SingletonBehavior<BattleCamManager>.Instance.BlurBackgroundCam(true);
            #pragma warning restore MA0003
            Debug.Log("CustomMapUtility: Cleaned up custom objects");
            base.ResetMap();
        }
        protected internal virtual string[] CustomBGMs {get;}
    }
}