using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NAudio.Wave;
using Mod;

namespace CustomMapUtility {
    public class CustomMapHandler {
        public static void InitCustomMap(string stageName, MapManager manager) {
            var Instance = new CustomMapHandler();
            Instance.Init(stageName, manager);
        }
        public void Init(string stageName, MapManager manager) {
            GameObject mapObject = Util.LoadPrefab("InvitationMaps/InvitationMap_Philip1", SingletonBehavior<BattleSceneRoot>.Instance.transform);
            mapObject.name = "InvitationMap_"+stageName;
            manager = this.InitManager(manager, mapObject);

            var modResources = new ModResources();

            var currentStagePath = modResources.GetStagePath(stageName);
            var newBG = new Texture2D(1920, 1080);
            try {
                newBG.LoadImage(File.ReadAllBytes(currentStagePath+"/Background.png"));
            } catch {
                newBG.LoadImage(File.ReadAllBytes(currentStagePath+"/Background.jpg"));
            }
            var newFloor = new Texture2D(1920, 1080);
            try {
                newFloor.LoadImage(File.ReadAllBytes(currentStagePath+"/Floor.png"));
            } catch {
                newFloor.LoadImage(File.ReadAllBytes(currentStagePath+"/Floor.jpg"));
            }
            Texture2D newUnder = null;
            if (File.Exists(currentStagePath+"/FloorUnder.png")) {
                newUnder = new Texture2D(1920, 1080);
                newUnder.LoadImage(File.ReadAllBytes(currentStagePath+"/FloorUnder.png"));
            } else if (File.Exists(currentStagePath+"/FloorUnder.jpg")) {
                newUnder = new Texture2D(1920, 1080);
                newUnder.LoadImage(File.ReadAllBytes(currentStagePath+"/FloorUnder.jpg"));
            }

            foreach (var component in manager.GetComponentsInChildren<Component>()) {
                switch (component) {
                    case SpriteRenderer renderer when renderer.name == "BG":
                    {
                        float pixelsPerUnit = (100f/1920f*(float)newBG.width);
                        renderer.sprite = Sprite.Create(newBG, new Rect(0f, 0f, newBG.width, newBG.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
                        break;
                    }
                    case SpriteRenderer renderer when renderer.name.Contains("Floor"):
                    {
                        float pixelsPerUnit = (100f/1920f*(float)newFloor.width);
                        renderer.sprite = Sprite.Create(newFloor, new Rect(0f, 0f, newFloor.width, newFloor.height), new Vector2(0.5f, (407.5f/1080f)), pixelsPerUnit);
                        break;
                    }
                    case SpriteRenderer renderer when renderer.name.Contains("Under"):
                    {
                        if (newUnder != null){
                            float pixelsPerUnit = (100f/1920f*(float)newUnder.width);
                            renderer.sprite = Sprite.Create(newUnder, new Rect(0f, 0f, newUnder.width, newUnder.height), new Vector2(0.5f, (300f/1080f)), pixelsPerUnit);
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
            SingletonBehavior<BattleSceneRoot>.Instance.InitInvitationMap(manager);
        }
        public class ModResources {
            public string GetStagePath(string stageName) {
                List<DirectoryInfo> stagePaths = new List<DirectoryInfo>();
                foreach (var info in GetStageRootPaths()) {
                    stagePaths.AddRange(info.GetDirectories(stageName));
                }
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
            private List<FileInfo> GetStageBgmPaths() {
                List<FileInfo> bgms = new List<FileInfo>();
                foreach (DirectoryInfo dir in _dirInfos) {
                    DirectoryInfo bgmsPath = new DirectoryInfo(Path.Combine(dir.FullName, "Resource\\StageBgm"));
                    if (bgmsPath.Exists) {
                        foreach (FileInfo file in bgmsPath.GetFiles("*.mp3")) {
                            bgms.Add(file);
                            Debug.Log(file.Name);
                        }
                    }
                }
                return bgms;
            }
            public string[] GetStageBgmPaths(string[] bgmNames) {
                foreach (var bgm in bgmNames) {
                    Debug.Log("BGMS: "+bgm);
                }
                IEnumerable<string> bgms =
                    from file in GetStageBgmPaths()
                    where bgmNames.Any(b => b == file.Name)
                    select file.FullName;
                return bgms.ToArray();
            }
            private IEnumerable<DirectoryInfo> _dirInfos =
                from modInfo in Mod.ModContentInfoLoader.LoadAllModInfos()
                // where modInfo.activated == true
                select modInfo.dirInfo;
            private IEnumerable<DirectoryInfo> GetStageRootPaths() {
                List<DirectoryInfo> paths = new List<DirectoryInfo>();
                foreach (DirectoryInfo dir in _dirInfos) {
                    DirectoryInfo stagePath = new DirectoryInfo(Path.Combine(dir.FullName, "Resource\\Stage"));
                    if (stagePath.Exists) {
                        paths.Add(stagePath);
                    }
                }
                return paths;
            }
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
            BGMs = resources.GetStageBgmPaths(BGMs);
            AudioClip[] output = new AudioClip[BGMs.Length];
            var mp3Converter = new Mp3toAudioClip();
            for (int i = 0; i < BGMs.Length; i++) {
                output[i] = mp3Converter.Convert(BGMs[i]);
            }
            return output;
        }
        internal class Mp3toAudioClip {
            internal AudioClip Convert(string path)
            {
                var sourceProvider = new Mp3FileReader(path);
                WaveFileWriter.CreateWaveFile(path + ".wav", sourceProvider);
                var wav = new Wav(File.ReadAllBytes(path + ".wav"));
                var audioClip = AudioClip.Create("cove", wav.SampleCount, 1, wav.Frequency, false);
                audioClip.SetData(wav.LeftChannel, 0);
                File.Delete(path + ".wav");
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
        public override void InitializeMap() {
            if (CustomBGMs != null) {
                this.mapBgm = CustomMapHandler.CustomBgmParse(CustomBGMs);
            }
            base.InitializeMap();
        }
        public override void EnableMap(bool b) {
            base.EnableMap(b);
            this.gameObject.SetActive(b);
        }
        protected virtual string[] CustomBGMs {get;}
    }
    public class CustomCreatureMapManager : CreatureMapManager {
        public override void InitializeMap() {
            if (CustomBGMs != null) {
                this.mapBgm = CustomMapHandler.CustomBgmParse(CustomBGMs);
            }
            base.InitializeMap();
        }
        public override void EnableMap(bool b) {
            base.EnableMap(b);
            this.gameObject.SetActive(b);
        }
        protected virtual string[] CustomBGMs {get;}
    }
}