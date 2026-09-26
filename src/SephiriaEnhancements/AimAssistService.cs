using UnityEngine;

namespace SephiriaEnhancements;

internal static class AimAssistService
{
	/// <summary>
	/// 复刻官方手柄分支：先按鼠标方向找角度最近的目标，再退回配置范围内最近的目标。
	/// </summary>
	internal static UnitAvatar FindGamepadStyleTarget(UnitAvatar avatar, Vector2 mouseWorldPosition)
	{
		return FindGamepadStyleTarget(avatar, mouseWorldPosition, Plugin.AimRadius);
	}

	internal static UnitAvatar FindGamepadStyleTarget(UnitAvatar avatar, Vector2 mouseWorldPosition, float maxRange)
	{
		if (avatar == null)
		{
			return null;
		}
		float sqrRadius = maxRange * maxRange;
		Vector2 direction = mouseWorldPosition - (Vector2)avatar.transform.position;
		UnitAvatar target = null;
		if (direction.sqrMagnitude > 0.0001f)
		{
			target = SearchNearestAngle(avatar, direction.normalized, sqrRadius);
		}
		if (target == null)
		{
			target = PlayerInputController.SearchTargetNearestPoint(avatar, avatar.transform.position, sqrRadius);
		}
		return target;
	}

	private static UnitAvatar SearchNearestAngle(UnitAvatar avatar, Vector2 direction, float sqrRadius)
	{
		CombatManager combatManager = CombatManager.Instance;
		RuntimeFactionManager factionManager = RuntimeFactionManager.Instance;
		if (combatManager == null || combatManager.AllCreatures == null || factionManager == null)
		{
			return null;
		}
		long hostileLayers = avatar.GetHostileFactionLayers(EDamageFromType.None);
		UnitAvatar result = null;
		float bestDot = -1f;
		foreach (UnitAvatar unit in combatManager.AllCreatures)
		{
			if (unit == null || unit == avatar || unit.canBeTarget <= 0 || unit.IsDead)
			{
				continue;
			}
			if ((hostileLayers & factionManager.FindFactionLayer(unit.faction)) == 0L)
			{
				continue;
			}
			Vector2 toUnit = (Vector2)unit.transform.position - (Vector2)avatar.transform.position;
			if (toUnit.sqrMagnitude <= 0.0001f || toUnit.sqrMagnitude > sqrRadius)
			{
				continue;
			}
			float dot = Vector2.Dot(direction, toUnit.normalized);
			if (dot > bestDot)
			{
				bestDot = dot;
				result = unit;
			}
		}
		return result;
	}
}
