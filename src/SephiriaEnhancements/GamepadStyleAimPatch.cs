using System;
using HarmonyLib;
using UnityEngine;

namespace SephiriaEnhancements;

/// <summary>
/// 官方键鼠分支只调用 SearchTargetNearestPoint(avatar, mouseWorld, 4f)。
/// 仅在该调用点替换为手柄分支的目标选择，避免影响其他系统对同一搜索函数的使用。
/// </summary>
[HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.SearchTargetNearestPoint))]
internal static class GamepadStyleAimPatch
{
	private static bool Prefix(UnitAvatar avatar, Vector2 mouseWorldPosition, float sqrRadius, ref UnitAvatar __result)
	{
		if (!Plugin.IsEnabled || avatar == null || Mathf.Abs(sqrRadius - 4f) > 0.0001f)
		{
			return true;
		}
		PlayerInputController controller = PlayerInputController.Instance;
		if (controller == null || controller.playerInput == null)
		{
			return true;
		}
		if (!string.Equals(controller.playerInput.currentControlScheme, PlayerInputController.KeyboardAndMouseScheme, StringComparison.Ordinal))
		{
			return true;
		}
		__result = AimAssistService.FindGamepadStyleTarget(avatar, mouseWorldPosition);
		return false;
	}
}
