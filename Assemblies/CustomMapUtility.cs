using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using NAudio.Wave;
using Mod;

namespace CustomMapUtility {
    public class CustomMapHandler {
        public static void InitCustomMap(string stageName, MapManager manager, bool initBGMs = true) {
            var Instance = new CustomMapHandler();
            Instance.Init(stageName, manager, initBGMs);
        }
        public void Init(string stageName, MapManager manager, bool initBGMs = true) {
            GameObject mapObject = Util.LoadPrefab("InvitationMaps/InvitationMap_Philip1", SingletonBehavior<BattleSceneRoot>.Instance.transform);
            mapObject.name = "InvitationMap_"+stageName;
            manager = this.InitManager(manager, mapObject);

            var modResources = new ModResources();

            var currentStagePath = modResources.GetStagePath(stageName);
            newAssets = new Dictionary<string, Texture2D>(){
                {"newBG", ImageLoad("Background", currentStagePath)},
                {"newFloor", ImageLoad("Floor", currentStagePath)},
                {"newUnder", ImageLoad("FloorUnder", currentStagePath)},
                {"scratch1", ImageLoad("Scratch1", currentStagePath)},
                {"scratch2", ImageLoad("Scratch2", currentStagePath)},
                {"scratch3", ImageLoad("Scratch3", currentStagePath)}
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
            SetTextures(manager);
            SetScratches(stageName, manager);
            if (initBGMs) {
                try {
                    var managerTemp = manager as CustomMapManager;
                    if (managerTemp.CustomBGMs != null) {
                        manager.mapBgm = CustomBgmParse(managerTemp.CustomBGMs);
                    } else {
                        Debug.LogWarning("CustomMapUtility: CustomBGMs is null");
                    }
                } catch (NullReferenceException) {
                    try {
                        var managerTemp = manager as CustomCreatureMapManager;
                        if (managerTemp.CustomBGMs != null) {
                            manager.mapBgm = CustomBgmParse(managerTemp.CustomBGMs);
                        } else {
                            Debug.LogWarning("CustomMapUtility: CustomBGMs is null");
                        }
                    } catch (NullReferenceException ex) {
                        Debug.LogError("CustomMapUtility: MapManager is not inherited from Custom(Creature)MapManager"+Environment.NewLine+ex+Environment.NewLine);
                    }
                }
            } else {
                Debug.Log("CustomMapUtility: Auto BGM initialization is off.");
            }
            SingletonBehavior<BattleSceneRoot>.Instance.InitInvitationMap(manager);
        }
        private void SetTextures(MapManager manager) {
            foreach (var component in manager.GetComponentsInChildren<Component>()) {
                switch (component) {
                    case SpriteRenderer renderer when renderer.name == "BG":
                    {
                        var texture = newAssets["newBG"];
                        float pixelsPerUnit = (100f/1920f*(float)texture.width);
                        renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
                        Debug.LogWarning(renderer.gameObject.transform.localScale);
                        break;
                    }
                    case SpriteRenderer renderer when renderer.name.Contains("Floor"):
                    {
                        var texture = newAssets["newFloor"];
                        float pixelsPerUnit = (100f/1920f*(float)texture.width);
                        renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, (407.5f/1080f)), pixelsPerUnit);
                        Debug.LogWarning(renderer.gameObject.transform.localScale);
                        break;
                    }
                    case SpriteRenderer renderer when renderer.name.Contains("Under"):
                    {
                        var texture = newAssets["newUnder"];
                        if (texture != null){
                            float pixelsPerUnit = (100f/1920f*(float)texture.width);
                            renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, (300f/1080f)), pixelsPerUnit);
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
        public Dictionary<string, Texture2D> newAssets = new Dictionary<string, Texture2D>();
        private void SetScratches(string stageName, MapManager manager) {
            if (newAssets["scratch3"] == null && newAssets["scratch2"] == null && newAssets["scratch1"] == null) {
                manager.scratchPrefabs = new GameObject[0];
                return;
            }
            Texture2D texture;
            string log = "CustomMapUtility: SetScratches: {";
            int i;
            string name = "";
            for (i = 0; i < 3; i++) {
                var current = manager.scratchPrefabs[i];
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
                        } else {goto case 1;}
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
                        } else {goto case 0;}
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
                        } else {goto default;}
                    }
                    default: {
                        current = null;
                        break;
                    }
                }
                manager.scratchPrefabs[i] = current;
                try {
                    log += Environment.NewLine+"    Scratch"+i+" = "+current.name;
                } catch {
                    log += Environment.NewLine+"    Scratch"+i+" = null";
                }
            }
            log += Environment.NewLine+"}";
            Debug.Log(log);
        }
        public class ModResources {
            public class CacheInit : ModInitializer {
                public override void OnInitializeMod()
                {
                    var tempInstance = new ModResources();
                    if (Assembly.GetExecutingAssembly().GetName().Name == "ConfigAPI") {
                        _dirInfos =
                            from modInfo in Mod.ModContentInfoLoader.LoadAllModInfos()
                            // where modInfo.activated == true
                            select modInfo.dirInfo;
                        Debug.Log("CustomMapUtility in Global Mode");
                    } else {
                        var curDir = new DirectoryInfo(Assembly.GetExecutingAssembly().Location+"\\..\\..");
                        Debug.Log("CustomMapUtility in Local Mode at "+curDir.FullName);
                        _dirInfos = new DirectoryInfo[]{curDir};
                    }
                    _stagePaths = tempInstance.GetStageRootPaths();
                    _bgms = tempInstance.GetStageBgmInfos();
                    if (_stagePaths != null && _stagePaths.Count != 0) {
                        string stagePathsDebug = "CustomMapUtility StageRootPaths: {";
                        foreach (var dir in _stagePaths) {
                            stagePathsDebug += Environment.NewLine+"    "+dir.FullName;
                        }
                        stagePathsDebug += Environment.NewLine+"}";
                        Debug.Log(stagePathsDebug);
                    }
                    if (_bgms != null && _bgms.Count != 0) {
                        string bgmsDebug = "CustomMapUtility BgmPaths: {";
                        foreach (var path in _bgms) {
                            bgmsDebug += Environment.NewLine+"    "+path.FullName;
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
                    where info.Name == stageName
                    select info;
                string path = null;
                foreach (var dir in stagePaths) {
                    if (path == null) {
                        path = dir.FullName;
                    } else {
                        throw new System.ArgumentException("Multiple stages share this name");
                    }
                }
                if (path == null) {
                    throw new System.NullReferenceException("Stage does not exist");
                }
                Debug.Log("CustomMapUtility: Loading stage from "+path);
                return path;
            }
            [Obsolete]
            private List<FileInfo> GetStageBgmPaths() {
                return GetStageBgmInfos();
            }
            private static List<FileInfo> _bgms;
            [Obsolete]
            public string[] GetStageBgmPaths(string[] bgmNames) {
                int debugCount = 0;
                foreach (var bgm in bgmNames) {
                    Debug.Log("CustomMapUtility: BGM"+debugCount+": "+bgm+Environment.NewLine);
                    debugCount++;
                }
                IEnumerable<string> bgms =
                    from file in GetStageBgmPaths()
                    where bgmNames.Any(b => b == file.Name)
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
                    where bgmNames.Any(b => b == file.Name)
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
        public static AudioClip[] CustomBgmParse(string[] BGMs) {
            var resources = new ModResources();
            var files = resources.GetStageBgmInfos(BGMs);
            AudioClip[] output = new AudioClip[BGMs.Length];
            var handler = new AudioHandler();
            for (int i = 0; i < BGMs.Length; i++) {
                try {
                    var info = new FileInfo(BGMs[i]);
                    Debug.Log("CustomMapUtility: BGM"+i+" = "+BGMs[i]);
                    if (info.Extension == ".mp3") {
                        output[i] = handler.Parse(files[i].FullName);
                    } else if (info.Extension == ".wav") {
                        output[i] = handler.Parse(files[i].FullName, false);
                    } else {
                        Debug.LogError(info.Name+" is not an mp3 or wav, filling with null");
                        output[i] = null;
                    }
                } catch (Exception ex) {
                    Debug.LogException(ex);
                    output[i] = null;
                }
            }
            return output;
        }
        internal class AudioHandler {
            [Obsolete]
            internal AudioClip Convert(string path, bool mp3 = true) {
                return Parse(path, mp3);
            }
            internal AudioClip Parse(string path, bool mp3 = true)
            {
                Wav wav;
                if (mp3) {
                    var sourceProvider = new Mp3FileReader(path);
                    MemoryStream stream = new MemoryStream();
                    WaveFileWriter.WriteWavFileToStream(stream, sourceProvider);
                    // WaveFileWriter.CreateWaveFile(path + ".wav", sourceProvider);
                    wav = new Wav(stream.ToArray());
                } else {
                    wav = new Wav(path);
                }
                if (wav.NumChannels > 8) {
                    Debug.LogError("Unity does not support more than 8 audio channels per file");
                    return null;
                }
                var audioClip = AudioClip.Create("BGM", (int)wav.SampleCount, wav.NumChannels, (int)wav.SampleRate, false);
                audioClip.SetData(wav.InterleavedAudio, 0);
                // File.Delete(path + ".wav");
                Debug.Log("Parse Result: "+wav);
                return audioClip;
            }
        }
        public static void EnforceMap(int num = 0) {
            var emotionTotalCoinNumber = Singleton<StageController>.Instance.GetCurrentStageFloorModel().team.emotionTotalCoinNumber;
            Singleton<StageController>.Instance.GetCurrentWaveModel().team.emotionTotalBonus = emotionTotalCoinNumber + 1;
            Singleton<StageController>.Instance.GetStageModel().SetCurrentMapInfo(num);
        }
    }

    public class CustomMapManager : MapManager {
        public override void EnableMap(bool b) {
            base.EnableMap(b);
            this.gameObject.SetActive(b);
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
            base.ResetMap();
        }
        protected internal virtual string[] CustomBGMs {get;}
    }
    public class CustomCreatureMapManager : CreatureMapManager {
        public override void EnableMap(bool b) {
            base.EnableMap(b);
            this.gameObject.SetActive(b);
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
            Debug.Log("CustomMapUtility: Cleaned up custom objects");
            base.ResetMap();
        }
        protected internal virtual string[] CustomBGMs {get;}
    }
}