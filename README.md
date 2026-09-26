# Sephiria Auto Aim（赛菲莉娅 键鼠自瞄）

为《赛菲莉娅》(Sephiria, Steam AppID 2436940, Unity 6 / Mirror) 制作的 BepInEx 6
游戏内插件：让**键鼠玩家**也能使用游戏自带的自动瞄准。

游戏原本只给手柄做了自动瞄准（右摇杆指方向），键鼠玩家必须把鼠标精确放到敌人身上。
本插件把"鼠标指针相对玩家的方向"当作手柄右摇杆的推杆方向，复用游戏原生的目标搜索与
瞄准写回逻辑，因此手感、目标合法性判定都与官方手柄自瞄保持一致。

## 功能

- **键鼠自动瞄准**：鼠标指一个方向，自动选中该方向上最近的敌人。
- **附近兜底**：方向附近没有敌人时，自动选择搜索半径内最近的敌人。
- **范围可调**：搜索半径默认 15 格，可在 5~30 格之间调整。
- **不干扰手柄**：仅在 `Keyboard&Mouse` 输入方案生效，手柄照旧走游戏原逻辑。
- **不改写玩家选项**：只在内存中临时覆盖 `KeyboardAimSupport`，不会保存或覆盖你的游戏设置。
- **尊重游戏规则**：只选择可被选中、未死亡、敌对阵营的目标，与其他系统使用同一套判定。

## 安装

前置条件：游戏已安装 BepInEx 6（Mono x64）。

1. 编译或下载得到 `SephiriaEnhancements.dll`。
2. 放到 `<游戏根目录>/BepInEx/plugins/`。
3. 启动游戏，插件自动加载；日志见 `BepInEx/LogOutput.log`。

安装后在游戏内「选项」界面打开「MOD 设置」即可调整，或直接编辑
`BepInEx/config/com.sephiriamods.enhancements.cfg`。

## 配置

| 配置项 | 默认值 | 说明 |
| --- | --- | --- |
| `General/Enabled` | `true` | 是否启用键鼠自瞄 |
| `Aim/MaxRange` | `15` | 自瞄搜索半径（格），范围 5~30 |

> `Aim/MaxRange` 同时作用于"方向搜索"和"附近目标兜底"两个阶段。

## 编译

需要 .NET SDK 8 或更新版本。

本插件引用游戏程序集与 BepInEx 6 程序集，因此需要指向你的游戏根目录：

```powershell
dotnet build src/SephiriaEnhancements/SephiriaEnhancements.csproj -c Release -p:GameDir="D:\steam\steamapps\common\Sephiria"
```

也可以用环境变量 `SEPHIRIA_GAME_DIR` 指定游戏根目录：

```powershell
$env:SEPHIRIA_GAME_DIR = "D:\steam\steamapps\common\Sephiria"
dotnet build src/SephiriaEnhancements/SephiriaEnhancements.csproj -c Release
```

编译产物在 `src/SephiriaEnhancements/bin/Release/net472/SephiriaEnhancements.dll`。

## 工作原理

反编译游戏程序集后可确认，官方自动瞄准集中在 `PlayerInputController.Update()`：

- 键鼠分支只调用 `SearchTargetNearestPoint(avatar, 鼠标世界坐标, 4f)`，
  而且要先满足 `KeyboardAimSupport == 1`，所以默认情况下键鼠几乎等同于"鼠标必须指到敌人"。
- 手柄分支先调用 `SearchTargetNearestAngle(avatar, 摇杆方向)` 按角度选一个目标，
  没有角度目标时再调用 `SearchTargetNearestPoint(avatar, 玩家坐标, 100f)` 兜底。
- 选中后写回 `avatar.autoAimedTarget`，并把 `avatar.NetworkaimObject` 指向该目标，
  后续攻击、技能施放（`integratedActionController.Cast(..., autoAimedTarget)`）都沿用这个目标。

本插件的做法：

1. 用 Harmony Prefix 拦截 `SaveData.GetInt`，只在读取 `KeyboardAimSupport` 且实例是
   游戏选项对象时返回 `1`，从而启用键鼠自瞄分支；不调用 `SetInt`、不写文件。
2. 用 Harmony Prefix 拦截键鼠分支里特征为 `sqrRadius == 4f` 的
   `SearchTargetNearestPoint` 调用，替换成复刻自手柄分支的目标选择：
   - 按鼠标方向做角度搜索（自定义实现，以便使用可配置半径）；
   - 没有结果时回退到原生 `SearchTargetNearestPoint`，半径同样取自配置。

因为只替换"选哪个目标"，瞄准姿势、网络同步、攻击判定全部仍由游戏原生代码处理。

## 兼容性

- 游戏版本：0.10.x（Unity 6 / Mono）
- 框架：BepInEx 6.0.0（Mono x64）
- 仅客户端行为，不修改服务端逻辑；联机时选目标完全在本地完成

## 许可

MIT License，见 [LICENSE](LICENSE)。
