using System;
using HarmonyLib;

namespace SephiriaEnhancements;

/// <summary>
/// 游戏原本只有把 Options/KeyboardAimSupport 设为 1 后才会在键鼠模式执行自瞄搜索。
/// 这里只在读取该选项时返回启用值，不改写并保存玩家的选项文件。
/// </summary>
[HarmonyPatch(typeof(SaveData), nameof(SaveData.GetInt))]
internal static class KeyboardAimSupportPatch
{
	private static bool Prefix(SaveData __instance, string key, int fallback, ref int __result)
	{
		if (!Plugin.IsEnabled || !string.Equals(key, "KeyboardAimSupport", StringComparison.Ordinal))
		{
			return true;
		}
		OptionsBinding binding = OptionsBinding.Instance;
		if (binding == null || binding.Options == null || __instance != binding.Options)
		{
			return true;
		}
		__result = 1;
		return false;
	}
}
