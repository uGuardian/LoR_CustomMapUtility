using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CustomMapUtility.AssetBundleManager
{
	public static class MapTemplate {
		private static AssetBundle bundle;

		public static string bundlePath;

		private static readonly WeakReference<GameObject> cache = new WeakReference<GameObject>(null);

		public static bool MapTemplateExists {get; set;}

		public static GameObject Asset {
			get {
				if (bundlePath != null) {
					return GetTemplate(bundlePath, "assets/maptemplate/maptemplate.prefab");
				} else if (MapTemplateExists) {
					Debug.LogError("CustomMapUtility: Template does not exist, reverting to fallback");
				}
				return Fallback();
			}
		}

		private static GameObject GetTemplate(string bundlePath, string internalPath)
		{
			if (cache.TryGetTarget(out GameObject template) && template != null) {
				return template;
			}
			try {
				if (bundle == null) {
					bundle = AssetBundle.LoadFromFile(bundlePath);
					template = bundle?.LoadAsset<GameObject>(internalPath);
				} else {
					template = bundle.LoadAsset<GameObject>(internalPath);
				}
			}
			catch (InvalidOperationException ex)
			{
				Debug.LogException(ex);
				Debug.LogWarning("Attempting to reload asset bundle");
				bundle = AssetBundle.LoadFromFile(bundlePath);
				template = bundle?.LoadAsset<GameObject>(internalPath);
			}
			if (template == null)
			{
				Debug.LogError("CustomMapUtility: Template does not exist, reverting to fallback");
				return Fallback();
			}
			cache.SetTarget(template);
			return template;
		}

		private static GameObject Fallback() {
			MapTemplateExists = false;
			return Resources.Load<GameObject>("Prefabs/InvitationMaps/InvitationMap_Philip1");
		}
	}
}
