# PCL_Max 吉祥物 · AI 绘图提示词

> 参考方向：深海系 Q 版立绘（鲸鱼娘那种）。主题换成「方块 / 启动器」，
> 保持相同的配色逻辑与构图习惯，但外形元素完全区分开。

---

## 角色设定

| 项 | 内容 |
|---|---|
| **名字** | **麦可**（Max 音译）/ 英文 Mako |
| **主题** | 方块 · 启动器 · 蓝色系 |
| **年龄感** | 14–16 岁，Q 版比例 |
| **性格** | 半睁眼、话少、干脆。不卖萌，不撒娇 |
| **识别点** | ① 左侧一绺浅蓝挑染 ② 发梢末端变成立方体像素 ③ 胸前 M 徽章 |

---

## 配色（与启动器主色一致）

```
主色 #3991E3 → #1964C2   头发渐变
亮色 #7CC0FF             挑染、发梢、高光
白色 #F5F9FF             衬衫
深蓝 #1B3A5C             马甲、裙子、领带
皮肤 #FBE9E0             偏暖
眼睛 #2D5C9E + #7CC0FF 高光
```

---

## 提示词 1：Q 版全身立绘（主视觉）

参考图那种质感的直接用法：

```
chibi, full body, super deformed, 1girl, solo,
dark blue to light blue gradient long hair, half-lidded eyes, indifferent expression,
one light blue highlighted streak on the left side of hair,
hair tips turning into small floating cubes, pixel-like particles at hair ends,
white shirt, dark navy vest with M-shaped emblem on chest, dark navy pleated skirt,
black thigh-high socks, navy loafers,
small cube-shaped hair ornament,
standing, arms slightly behind back, looking at viewer,
soft blue gradient background, floating cubes, clean lineart, cel shading,
soft lighting, high quality, official art style, character design sheet,
--ar 2:3 --style anime --nologo
```

---

## 提示词 2：壁纸版（横构图，左侧留白）

启动器背景用，**人物必须在右侧**：

```
chibi anime girl, full body, dark blue gradient long hair with light blue streak,
half-lidded eyes, calm confident expression,
white shirt, navy vest with M emblem, navy pleated skirt,
standing, three-quarter view, looking to the right,
large empty space on the left third of the frame,
light blue gradient background, softly blurred, floating cubes and particles,
soft ambient lighting, clean cel shading, high resolution,
--ar 16:9 --nologo
```

> 出图后如果人物还是居中，补一句 `subject on the right side, empty space on left`

---

## 提示词 3：表情包四连（贴纸）

一次生成一张，改最后一行的表情描述即可：

```
chibi, head and shoulders only, dark blue gradient hair with light blue streak,
[表情见下表], white shirt collar, navy vest,
simple flat light blue background, centered composition,
thick clean outlines, flat colors, sticker style,
--ar 1:1 --nologo
```

| 表情 | 替换 `[表情]` 为 |
|---|---|
| 加载中 | `closed eyes, sleeping, floating ZZZ` |
| 更新完成 | `small smile, one hand making OK sign` |
| 出错 | `frowning, hands on hips, annoyed` |
| 默认 | `half-lidded eyes, indifferent, looking at viewer` |

---

## 提示词 4：圆形头像

官网与关于页用：

```
chibi anime girl, headshot only, centered in circle,
dark blue gradient hair with light blue highlighted streak,
half-lidded indigo eyes looking directly at viewer, slight smile,
white shirt collar, navy vest,
flat light blue background, clean shading, icon style,
--ar 1:1 --nologo
```

---

## 生成时的注意事项

**避免**：
- 双马尾（和一堆启动器看板娘撞脸）
- 女仆装（气质不符，工具型应用不该卖萌）
- 过于饱和的荧光色（会和使用浅色 UI 打架）
- 复杂背景（壁纸版背景必须简洁，否则卡片上的字看不清）

**负面提示词**（如果工具支持）：
```
twin tails, maid outfit, excessive blush, sparkles, busy background,
text, watermark, lowres, bad anatomy, extra limbs
```

**分辨率**：
- 壁纸 2560×1440 或更高，另出方形裁切版
- 头像 512×512 以上
- 立绘透明背景 PNG

---

## 拿到图之后

放到仓库这两个位置：

```
design/mascot/            原图与源文件
Plain Craft Launcher 2/Images/Mascot/    打包用的成品
```

壁纸走「设置 → 个性化 → 背景内容」，不要塞进程序内置背景——
内置背景需要用户主动换，塞进去反而会覆盖已有设置。

Q 版贴纸可以用在：加载动画、更新完成提示、错误提示的插图位。
