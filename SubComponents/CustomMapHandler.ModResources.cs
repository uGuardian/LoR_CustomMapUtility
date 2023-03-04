#if DEBUG
#define PRERELEASE
#endif

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
using System.Text;
using CustomMapUtility.AssetBundleManager;
#if !NOMP3
using NAudio.Wave;
#endif
using Mod;
using System.Xml.Serialization;
using System.Threading;
using System.Threading.Tasks;
using uGuardian.Utilities.Threading;
#pragma warning disable MA0048, MA0016, MA0051

namespace CustomMapUtility {
	public partial class CustomMapHandler {
		#region RESOURCES
		public static class ModResources {
			public class CacheInit : ModInitializer {
				public const string version = "3.1.0";
				#if PRERELEASE
					#warning PRERELEASE
					public const string feature = "MP3_Optimization";
				#endif
				internal static bool initialized = false;
				internal static SynchronizationContext unitySyncContext;
				internal static ImmediateUnitySynchronizationContext masterSyncContext;
				private static void InitSyncContext() {
					if (masterSyncContext != null) {return;}
					unitySyncContext = SynchronizationContext.Current;
					ImmediateUnitySynchronizationContext.InitializeSynchronizationContext(unitySyncContext);
					masterSyncContext = new ImmediateUnitySynchronizationContext();
				}
				public override void OnInitializeMod() {
					if (initialized) {return;}
					InitSyncContext();
					#if !PRERELEASE
					Debug.Log($"CustomMapUtility Version \"{version}\"");
					#else
					Debug.Log($"CustomMapUtility Version \"{version}-PRERELEASE\" with feature \"{feature}\"");
					#endif
					CreateContainersForReferencedAssemblies(Assembly.GetExecutingAssembly());
					CheckBundle();
					masterSyncContext.ExecuteAll();
					PrintModsUsingVersion(true);
					initialized = true;
				}
			}

			static readonly HashSet<string> announcedMods = new HashSet<string>();
			static void PrintModsUsingVersion(bool initPrint = false) {
				if (!initPrint && !CacheInit.initialized) {return;}
				StringBuilder sb;
				IEnumerable<string> printKeys;
				if (!CacheInit.initialized) {
					printKeys = containerDic.Keys.AsEnumerable();
					sb = new StringBuilder("CustomMapUtility: Mods using this version: {");
				} else {
					printKeys = containerDic.Keys.Except(announcedMods);
					if (!printKeys.Any()) {return;}
					sb = new StringBuilder("CustomMapUtility: Adding mods using version "+CacheInit.version+" : {");
				}
				sb.AppendLine();
				foreach (var modId in printKeys) {
					sb.Append("	");
					sb.AppendLine(modId);
					announcedMods.Add(modId);
				}
				sb.Append("}");
				Debug.Log(sb);
			}

			public static void CreateContainersForReferencedAssemblies(params Assembly[] assemblies) =>
				CreateContainersForReferencedAssemblies(assemblies.AsEnumerable());
			public static void CreateContainersForReferencedAssemblies(IEnumerable<Assembly> assemblies) {
				ParallelQuery<Assembly> fullQuery = null;
				var step1 = AppDomain.CurrentDomain.GetAssemblies()
					.AsParallel();
				foreach (var assembly in assemblies) {
					var assemblyName = assembly.GetName();
					var assemblyNameName = assemblyName.Name;
					var assemblyToken = assemblyName.GetPublicKeyToken();
					var step2 = step1.Where(a => {
						var refAssemblies = a.GetReferencedAssemblies();
						return Array.Exists(refAssemblies, x => {
							bool tokenEqual;
							var targetToken = x.GetPublicKeyToken();
							if (assemblyToken == null) {
								tokenEqual = targetToken == null;
							} else if (targetToken == null) {
								tokenEqual = false;
							} else {
								tokenEqual = assemblyToken.SequenceEqual(targetToken);
							}
							return string.Equals(assemblyNameName, x.Name, StringComparison.Ordinal) && tokenEqual;
						});
					});
					if (fullQuery != null) {
						fullQuery.Union(step2);
					} else {
						fullQuery = step2;
					}
				}
				CreateContainersForAssemblies(fullQuery);
			}

			public static void CreateContainersForAssemblies(params Assembly[] assemblies) =>
				CreateContainersForAssemblies(assemblies.AsParallel());
			public static void CreateContainersForAssemblies(IEnumerable<Assembly> assemblies) =>
				CreateContainersForAssemblies(assemblies.AsParallel());
			public static void CreateContainersForAssemblies(ParallelQuery<Assembly> assemblies) {
				assemblies.Select(GetIdAndDirFromXml)
					.ForAll(ParseDir);
				PostContainerCreate();
			}

			public static void CreateContainersForModIDs(params string[] modIDs){
				CreateContainersForModIDs(modIDs.AsEnumerable());
			}
			public static void CreateContainersForModIDs(IEnumerable<string> modIDs) {
				foreach (var (uniqueId, dirInfo) in modIDs.Select(GetIdAndDir)) {
					if (dirInfo == null) {continue;}
					ParseDir(uniqueId, dirInfo);
				}
				PostContainerCreate();
			}
			public static void CreateContainersForModIDs(ParallelQuery<string> modIDs) {
				modIDs.Select(GetIdAndDir)
					.Where(x => x.dirInfo != null)
					.ForAll(ParseDir);
				PostContainerCreate();
			}

			static void ParseDir((string uniqueId, DirectoryInfo dirInfo) tuple) {
				var (uniqueId, dirInfo) = tuple;
				ParseDir(uniqueId, dirInfo);
			}
			static void ParseDir(string uniqueId, DirectoryInfo dirInfo) {
				var container = new CMUContainer(uniqueId, dirInfo);

				if(!containerDic.TryAdd(uniqueId, container)) {
					var oldContainer = containerDic[uniqueId];
					if (!oldContainer.ConfirmSameDirectory(container)) {
						// If this occurs it will attempt to compensate, but may error.
						AddErrorLog($"CustomMapUtility: ModID {uniqueId} exists in multiple directories");
					}
					// This shouldn't happen, but just to be sure this exists as a backup.
					oldContainer.CopyFrom(container);
				}
			}

			public static (string uniqueId, DirectoryInfo dirInfo) GetIdAndDir(string modId) =>
				(modId, dirInfo: Singleton<ModContentManager>.Instance.GetLoadedModPath(modId));
			public static (string uniqueId, DirectoryInfo dirInfo) GetIdAndDirFromXml(Assembly assembly) {
				DirectoryInfo dirInfo = new FileInfo(assembly.Location).Directory.Parent;
				var files = dirInfo.EnumerateFiles("StageModInfo.xml");
				FileInfo stageModInfo;
				while ((stageModInfo = files.FirstOrDefault()) == null) {
					dirInfo = dirInfo.Parent;
					files = dirInfo.EnumerateFiles("StageModInfo.xml");
				}
				return GetIdAndDirFromXml(stageModInfo, dirInfo);
			}
			public static (string uniqueId, DirectoryInfo dirInfo) GetIdAndDirFromXml(FileInfo stageModInfo) =>
				GetIdAndDirFromXml(stageModInfo, stageModInfo.Directory);
			public static (string uniqueId, DirectoryInfo dirInfo) GetIdAndDirFromXml(FileInfo stageModInfo, DirectoryInfo dirInfo) {
				using (var streamReader = stageModInfo.OpenRead()) {
					Workshop.NormalInvitation invInfo = (Workshop.NormalInvitation) new XmlSerializer(typeof(Workshop.NormalInvitation)).Deserialize(streamReader);
					if (string.IsNullOrEmpty(invInfo.workshopInfo.uniqueId) || string.Equals(invInfo.workshopInfo.uniqueId, "-1", StringComparison.Ordinal)) {
						invInfo.workshopInfo.uniqueId = dirInfo.Name;
					}
					string uniqueId = invInfo.workshopInfo.uniqueId;
					return (uniqueId, dirInfo);
				}
			}

			static void PostContainerCreate() {
				CheckBundle();
				PrintModsUsingVersion();
			}

			static void CheckBundle() {
				if (MapTemplate.MapTemplateExists) {return;}
				foreach (var container in containerDic.Values) {
					var bundle = container.resourceDir.EnumerateDirectories("AssetBundle")
						.FirstOrDefault()?
						.EnumerateFiles("cmumaptemplate.assets")
						.FirstOrDefault();
					if (bundle != null) {
						var fullName = bundle.FullName;
						MapTemplate.bundlePath = fullName;
						MapTemplate.MapTemplateExists = true;
						Debug.Log($"CustomMapUtility: Using template bundle at {fullName}");
					}
				}
			}

			internal static readonly ConcurrentDictionary<string, CMUContainer> containerDic = new ConcurrentDictionary<string, CMUContainer>();
			public class CMUContainer : IEquatable<CMUContainer>, IEquatable<string> {
				public readonly string uniqueId;
				public readonly DirectoryInfo directory;
				public readonly DirectoryInfo resourceDir;
				public readonly DirectoryInfo stageRootDir;
				public readonly Dictionary<string, DirectoryInfo> stageDic;
				public readonly DirectoryInfo audioRootDir;
				public readonly Dictionary<string, FileInfo> audioDic;

				public CMUContainer(string uniqueId, DirectoryInfo directory) {
					this.uniqueId = uniqueId;
					this.directory = directory;

					this.resourceDir = directory.EnumerateDirectories("Resource").FirstOrDefault();
					if (resourceDir == null) {return;}
					var resourceDirs = resourceDir.GetDirectories();
					this.stageRootDir = resourceDirs.FirstOrDefault(x => string.Equals(x.Name, "Stage", StringComparison.OrdinalIgnoreCase));
					this.audioRootDir = resourceDirs.FirstOrDefault(x => string.Equals(x.Name, "CustomAudio", StringComparison.OrdinalIgnoreCase));
					this.stageDic = stageRootDir?.EnumerateDirectories()
						.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase) ??
						new Dictionary<string, DirectoryInfo>(StringComparer.OrdinalIgnoreCase);
					this.audioDic = audioRootDir?.EnumerateFiles("*", SearchOption.AllDirectories)
						.ToDictionary(b => b.Name, StringComparer.OrdinalIgnoreCase) ??
						new Dictionary<string, FileInfo>(StringComparer.OrdinalIgnoreCase);
				}

				public bool Equals(CMUContainer target) {
					if (target == null) {
						return false;
					}
					return string.Equals(uniqueId, target.uniqueId, StringComparison.Ordinal);
				}
				public bool Equals(string target) {
					if (target == null) {
						return false;
					}
					return string.Equals(uniqueId, target, StringComparison.Ordinal);
				}
				public override bool Equals(object obj) {
					if (obj == null || !GetType().Equals(obj.GetType())) {
						return false;
					}
					var target = (CMUContainer)obj;
					return string.Equals(uniqueId, target.uniqueId, StringComparison.Ordinal);
				}
				public static bool operator ==(CMUContainer a, CMUContainer b) => a.Equals(b);
				public static bool operator !=(CMUContainer a, CMUContainer b) => !a.Equals(b);
				public static explicit operator string(CMUContainer a) => a.uniqueId;

				public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(uniqueId);

				public override string ToString() => uniqueId;

				[Obsolete("Use GetStageDir instead")]
				public string GetStagePath(string stageName) {
					if (!stageDic.TryGetValue(stageName, out var dir) && dir == null) {
						throw new ArgumentNullException(nameof(stageName), "Stage does not exist");
					}
					var path = dir.FullName;
					Debug.Log($"CustomMapUtility: StagePath: {path}");
					return path;
				}
				public DirectoryInfo GetStageDir(string stageName) {
					if (!stageDic.TryGetValue(stageName, out var dir) && dir == null) {
						throw new ArgumentNullException(nameof(stageName), "Stage does not exist");
					}
					Debug.Log($"CustomMapUtility: StagePath: {dir.FullName}");
					return dir;
				}
				public FileInfo GetStageBgmInfo(string bgmName) {
					if (audioDic.TryGetValue(bgmName, out var file)) {
						return file;
					} else {
						return null;
					}
				}
				public Dictionary<string, FileInfo> GetStageBgmInfos(IEnumerable<string> bgmNames) {
					var bgms = new Dictionary<string, FileInfo>(StringComparer.OrdinalIgnoreCase);
					foreach (var name in bgmNames) {
						if (audioDic.TryGetValue(name, out var file)) {
							bgms.Add(name, file);
						}
					}
					return bgms;
				}
				public Dictionary<string, FileInfo> GetStageBgmInfos(params string[] bgmNames) =>
					GetStageBgmInfos((IEnumerable<string>)bgmNames);

				readonly object copyLock = new object();
				public void CopyFrom(CMUContainer other) {
					// Added a try catch because apparently it's possible for the below code to fail.
					// This isn't thread safe if the "other" container is being modified.
					lock (copyLock) {
						var exceptionList = new List<Exception>();
						string key;
						foreach (var entry in other.stageDic) {
							try {
								key = entry.Key;
								DirectoryInfo value = entry.Value;
								if (!stageDic.TryAdd(key, value)) {
									if (!string.Equals(stageDic[key].FullName, value.FullName, StringComparison.Ordinal)) {
										AddErrorLog($"CustomMapUtility: Conflict for stage {key} occured during container copy");
									}
								}
							} catch (Exception ex) {
								exceptionList.Add(ex);
							}
						}
						foreach (var entry in other.audioDic) {
							try {
								key = entry.Key;
								FileInfo value = entry.Value;
								if (!audioDic.TryAdd(key, value)) {
									if (!string.Equals(audioDic[key].FullName, value.FullName, StringComparison.Ordinal)) {
										AddErrorLog($"CustomMapUtility: Conflict for audio file {key} occured during container copy");
									}
								}
							} catch (Exception ex) {
								exceptionList.Add(ex);
							}
						}
						if (exceptionList.Count > 0) {
							AddErrorLog(new AggregateException(
								$"CustomMapUtility: CopyFrom for container {this} encountered one or more errors.",
								exceptionList
							));
						}
					}
				}
				public bool ConfirmSameDirectory(CMUContainer other) => string.Equals(directory.FullName, other.directory.FullName);
			}
			#region AdvancedLogging
			static void AddErrorLog(string msg) => CacheInit.masterSyncContext.Send(AddErrorLog_Internal_String, msg);
			static void AddErrorLog(string msg, Exception e) => CacheInit.masterSyncContext.Send(AddErrorLog_Internal_Tuple, (msg, e));
			static void AddErrorLog(Exception e) => CacheInit.masterSyncContext.Send(AddErrorLog_Internal_Exception, e);
			static void AddWarningLog(string msg) => CacheInit.masterSyncContext.Send(AddWarningLog_Internal_String, msg);
			static void AddErrorLog_Internal_String(object msg) => Singleton<ModContentManager>.Instance.AddErrorLog((string)msg);
			static void AddErrorLog_Internal_Tuple(object tuple) {
				(string msg, Exception e) = ((string msg, Exception e))tuple;
				Singleton<ModContentManager>.Instance.AddErrorLog(msg, e);
			}
			static void AddErrorLog_Internal_Exception(object e) => Singleton<ModContentManager>.Instance.AddErrorLog((Exception)e);
			static void AddWarningLog_Internal_String(object msg) => Singleton<ModContentManager>.Instance.AddWarningLog((string)msg);
			#endregion
		}
		#endregion
	}
}