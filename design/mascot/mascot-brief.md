# PCL_Max 吉祥物设定稿

> 用途：交给画师或 AI 绘图工具，产出壁纸、图标、加载动画、错误页插图。
> 设计基调对齐现有视觉：主色 `#3991E3`，浅色主题，干净的蓝白配色。

---

## 一、基本信息

| 项 | 内容 |
|---|---|
| **名字** | **麦可**（Max 的音译，也贴近 Minecraft 的「麦」）<br>英文名 **Mako**，日文写法 マク |
| **定位** | 启动器的看板娘 / 助手，负责在启动、更新、报错时出现 |
| **年龄感** | 16–17 岁 |
| **性格** | 干脆、可靠、话不多；遇到出错会叉腰皱眉，但不会慌张 |
| **口头禅** | 「交给我。」／「这个我能修。」 |

---

## 二、外观设定

### 主色板

| 用途 | 色值 | 说明 |
|---|---|---|
| 主色（头发） | `#3991E3` → `#1964C2` 渐变 | 与启动器主色一致 |
| 强调色（发梢/瞳孔高光） | `#7CC0FF` | 浅蓝，用在反光和发丝末端 |
| 服装主色 | 白 `#F5F9FF` | 干净的白色，不抢眼 |
| 服装辅色 | 深蓝 `#1B3A5C` | 领结、裙边、袖口 |
| 皮肤 | `#FBE9E0` | 偏暖，避免惨白 |
| 眼睛 | 靛蓝 `#2D5C9E` + 浅蓝高光 | |

### 发型

- 及腰长发，直发为主，发梢略带内卷
- **关键识别点：左侧一绺挑染的浅蓝色**（`#7CC0FF`），从额角垂到肩
- 刘海自然分缝，露出额头
- 不建议双马尾（与常见启动器看板娘雷同，辨识度低）

### 服装

- 白衬衫 + 深蓝短马甲，马甲左胸有一枚 **M 字徽章**（即现有 logo 的 M）
- 深蓝百褶裙，及膝
- 过膝黑袜 + 深蓝乐福鞋
- 领口系一条松开的深蓝细领带，显得不太拘谨
- **不建议**：女仆装、水手服——太常见，和「工具型启动器」气质不搭

### 随身物（可选，用于强化身份）

- 手里常拿一台**老式掌机**或平板，屏幕上是启动器的界面
- 或者肩头趴一只**方块造型的小猫**（像素风，像 Minecraft 的猫），作为副吉祥物的记忆点

---

## 三、姿态与场景

| 场景 | 用途 | 构图建议 |
|---|---|---|
| **默认壁纸** | 启动器背景 | 3/4 侧身站立，视线看向右下（避开左侧导航栏），右手轻抬；背景为大面积淡蓝渐变 + 漂浮的方块粒子 |
| **启动中** | 加载动画 | Q 版，双手抱膝悬浮，闭眼 |
| **更新完成** | 提示 | Q 版，单手比 OK，头顶有感叹号气泡 |
| **出错** | 错误页 | Q 版，叉腰皱眉，头顶有问号气泡 |
| **图标版** | 圆形头像 | 只露头部，眼神直视前方 |

### 壁纸构图要求

- **左 1/3 留空**：启动器左侧是导航栏，人物主体必须偏右
- **背景不要过于复杂**：现有 UI 是浅色卡片，背景太花会看不清文字
- 分辨率至少 2560×1440，另出一份 1:1 方形裁切版

---

## 四、可直接使用的 AI 绘图提示词

### 默认壁纸（竖构图）

```
anime girl, solo, long straight blue hair with a light-blue highlighted streak on the left,
indigo eyes, white shirt with dark navy vest, M-shaped emblem on left chest,
dark navy pleated skirt, black thigh-high socks, navy loafers, loose navy necktie,
soft smile, confident expression, standing, looking to the right, three-quarter view,
large empty space on the left side of the frame,
light blue gradient background, floating cube particles, soft lighting, clean lineart,
cel shading, high quality, official art, PCL_Max mascot, 2560x1440,
--ar 16:9 --style anime --nologo
```

### Q 版表情（加载中 / 出错）

```
chibi, super deformed, same character, blue hair with light-blue streak,
floating with knees up, closed eyes, sleeping, soft expression,
simple light blue background, centered composition, sticker style,
thick outlines, clean flat colors, --ar 1:1 --nologo
```

### 圆形头像

```
headshot only, anime girl, blue hair with light-blue highlighted streak,
indigo eyes looking directly at viewer, slight smile,
centered, circular composition, flat light blue background,
clean shading, icon style, --ar 1:1 --nologo
```

---

## 五、交付清单

- [ ] 壁纸 2560×1440（16:9）+ 方形裁切版
- [ ] 圆形头像 512×512（用于官网、关于页）
- [ ] Q 版贴纸 × 3（加载中 / 更新完成 / 出错）
- [ ] 透明背景立绘 PNG（用于任意合成）
- [ ] 设计源文件（PSD / CLIP）

---

## 六、授权注意

若是请画师绘制，请在委托时写明：

- 用途为**开源启动器的免费配图**，非商业
- 需获得**署名权与修改权**（后续可能要改配色或加配件）
- 允许在仓库与官网展示

若使用 AI 绘图，注意所选工具的服务条款是否允许此类用途。
