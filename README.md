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

### 方式一：完整安装包（推荐）

到 [Releases](https://github.com/Kagarinokirie1/SephiriaAutoAim/releases) 下载最新版的
`SephiriaAutoAim-v*.zip`，解压到游戏根目录（与 `Sephiria.exe` 同级）即可。

压缩包已内含 BepInEx 6 运行库与自瞄插件，无需另行安装前置，解压后目录结构为：

```
Sephiria\
  winhttp.dll
  doorstop_config.ini
  .doorstop_version
  BepInEx\
    core\
    config\
    plugins\SephiriaEnhancements.dll
```

### 方式二：已有 BepInEx 环境

若你已经装好 BepInEx 6，只需取 [`dist/SephiriaEnhancements.dll`](dist/SephiriaEnhancements.dll)
放进 `<游戏根目录>/BepInEx/plugins/`，不要覆盖整个 `BepInEx` 文件夹。

### 方式三：自行编译

克隆仓库后按下方[编译](#编译)一节自行构建。

任一方式安装后启动游戏，插件自动加载；日志见 `BepInEx/LogOutput.log`。

## 配置

配置通过编辑文件完成，无需额外插件。

首次启动游戏后会自动生成配置文件：

```
<游戏根目录>\BepInEx\config\com.sephiriamods.enhancements.cfg
```

用记事本打开并修改，保存后**重启游戏**才会生效。文件内容如下：

```ini
[Aim]

## 键鼠自瞄的搜索半径（格），角度搜索和附近目标兜底都使用该范围。
# Setting type: Single
# Default value: 15
MaxRange = 15

[General]

## 键鼠模式下模拟手柄自瞄：优先选择鼠标方向上的敌人，无方向目标时选择附近敌人。
# Setting type: Boolean
# Default value: true
Enabled = true
```

| 配置项 | 默认值 | 说明 |
| --- | --- | --- |
| `General/Enabled` | `true` | 是否启用键鼠自瞄，填 `true` 开启、`false` 关闭 |
| `Aim/MaxRange` | `15` | 自瞄搜索半径（格），可填 5~30 |

两点注意：

- 只改等号右边的内容，不要改动 `[Aim]`、`[General]` 这些段落名和 `MaxRange`、`Enabled` 这些键名。
- 若不小心改坏，直接删除该 `.cfg` 文件并重启游戏，会重新生成一份默认配置。

### 关于「改完必须重启」

配置文件只在**游戏启动时读取一次**，之后插件一直使用内存中的值，不会感知磁盘上的文件变化。
因此：

- 游戏运行中修改该文件，不会立即生效，必须重启游戏才会应用新设置。
- 游戏运行中修改的内容，若之后有其他操作触发了配置保存，可能被内存中的旧值覆盖。

建议在**完全退出游戏后**再修改该文件，改完直接启动游戏。

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

发布新版本时，把该产物复制到 `dist/SephiriaEnhancements.dll` 一并提交，安装者即可直接取用。

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
