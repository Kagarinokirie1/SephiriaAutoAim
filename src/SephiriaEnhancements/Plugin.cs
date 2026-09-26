using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;

namespace SephiriaEnhancements;

[BepInPlugin("com.sephiriamods.enhancements", "Sephiria Enhancements", "0.2.0")]
public sealed class Plugin : BaseUnityPlugin
{
	public const string PluginGuid = "com.sephiriamods.enhancements";

	public const string PluginName = "Sephiria Enhancements";

	public const string PluginVersion = "0.2.0";

	internal static Plugin Instance;

	internal ConfigEntry<bool> Enabled;

	internal ConfigEntry<float> MaxRange;

	private Harmony harmony;

	internal static bool IsEnabled => Instance != null && Instance.Enabled != null && Instance.Enabled.Value;

	internal static float AimRadius
	{
		get
		{
			if (Instance == null || Instance.MaxRange == null)
			{
				return 15f;
			}
			return Mathf.Clamp(Instance.MaxRange.Value, 5f, 30f);
		}
	}

	private void Awake()
	{
		Instance = this;
		Enabled = base.Config.Bind("General", "Enabled", true, "键鼠模式下模拟手柄自瞄：优先选择鼠标方向上的敌人，无方向目标时选择附近敌人。");
		MaxRange = base.Config.Bind("Aim", "MaxRange", 15f, "键鼠自瞄的搜索半径（格），角度搜索和附近目标兜底都使用该范围。");
		MaxRange.Value = Mathf.Clamp(MaxRange.Value, 5f, 30f);
		harmony = new Harmony(PluginGuid);
		harmony.PatchAll(typeof(Plugin).Assembly);
		base.Logger.LogInfo("Sephiria Enhancements v0.2.0 loaded. 键鼠自瞄: " + Enabled.Value + ", 范围: " + MaxRange.Value);
	}

	private void OnDestroy()
	{
		if (harmony != null)
		{
			harmony.UnpatchSelf();
		}
		Instance = null;
	}
}
