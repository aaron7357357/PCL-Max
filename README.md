**简体中文** | [English](README-EN.md) | [繁體中文](README-ZH_TW.md)

<div align="center">

# PCL_Max

维护者：**aaron7357357** · 基于 [PCL_Plus](https://github.com/x1B1C1/PCL_PLus) 的源码继续开发

[![Stars](https://img.shields.io/github/stars/aaron7357357/PCL-Max?style=for-the-badge&logo=github&labelColor=444444&color=eac54f)](https://github.com/aaron7357357/PCL-Max/stargazers)
[![Release](https://img.shields.io/github/v/release/aaron7357357/PCL-Max?include_prereleases&label=release&logo=github&style=for-the-badge)](https://github.com/aaron7357357/PCL-Max/releases)
[![Downloads](https://img.shields.io/github/downloads/aaron7357357/PCL-Max/total?style=for-the-badge&logo=github&labelColor=444444&color=1F883D)](https://github.com/aaron7357357/PCL-Max/releases)

[源仓库](https://github.com/aaron7357357/PCL-Max) |
[下载](https://github.com/aaron7357357/PCL-Max/releases) |
[文档](docs/) |
[许可证](#-许可证) |
[提交问题](https://github.com/aaron7357357/PCL-Max/issues/new)

</div>

> **非 MINECRAFT 官方产品。** 未经 MOJANG 或 MICROSOFT 批准，也不与 MOJANG 或 MICROSOFT 关联。
> NOT AN OFFICIAL MINECRAFT PRODUCT. NOT APPROVED BY OR ASSOCIATED WITH MOJANG OR MICROSOFT.

PCL_Max 是一个基于 PCL_Plus 源码继续开发的 Minecraft 启动器，目标是做得更好用一点：
收拾掉那些用起来别扭的地方，把该有的功能补回来，把没必要的入口收起来。

本项目沿用 **《PCL 分发有限许可》** 发布，详见[许可证](#-许可证)。

## 📦 下载

前往 [Releases](https://github.com/aaron7357357/PCL-Max/releases) 下载最新版本。

| 架构 | 文件 | 说明 |
|---|---|---|
| x64 | `PCL_Max_v0.3.0_x64.exe` | 正式版，单文件、免解压 |
| x64 | `PCL_Max_v0.3.0_Beta1_x64.exe` | 测试版，版本号带 `Beta1`，仅预览通道可见 |

- 单文件版启动时会把原生库解压到 `%TEMP%\.net\<exe 名>\`；若该目录不可写，
  请改用仓库里的文件夹版（双击其中的 `Plain Craft Launcher 2.exe`）
- **启动器内置自动更新**。正式版与测试版走同一条 `main` 分支清单，靠
  Release 上的 `prerelease` 标记和「更新通道」设置区分：
  稳定通道只收正式版，预览通道才收得到 `Beta` / `pre` 版本
- **⚠️ 必须先用 v0.3.0 及以后的版本，才能自动收到后续更新。**
  更早的版本（v0.1 / v0.2）读的是旧地址，已经无法自动升级

## 💻 支持平台

| 操作系统 | 支持情况 | 环境要求 |
|---|---|---|
| Windows 10 1809 (17763) 或更高 | ✅ 完整支持 | [.NET 10 桌面运行时](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Windows 8 - Windows 10 1809 (17763) | ⚠️ 理论能跑，酌情提供支持 | [.NET 10 桌面运行时](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Windows 7 或更低版本 | ❌ 不支持 | / |
| macOS / Linux / 其他操作系统 | ⚠️ 仅跨平台开发支持（交叉编译） | [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) |

## 📖 文档

| 文档 | 讲什么 |
| --- | --- |
| [docs/README.md](docs/README.md) | 索引、项目结构、版本号在哪 |
| [主题与配色](docs/THEME.md) | 八级色阶怎么算、14 个预设、自定义主题、怎么加新主题 |
| [配置与数据目录](docs/CONFIG.md) | 配置文件位置、手动改法、迁移逻辑 |
| [构建与发布](docs/BUILD.md) | 编译、密钥注入、发版 |
| [开源项目清单](docs/CREDITS.md) | 代码来源、第三方库、各自的许可证 |
| [NOTICE](NOTICE) | 随分发物打包的第三方组件及其许可义务 |

## 🔒 许可证

本项目沿用 **[《PCL 分发有限许可》](LICENSE)**（含《PCL 存储库合理使用指南》），完整条款以 [`LICENSE`](LICENSE) 文件为准。
**PCL_Max 是第三方基于 PCL 独立进行的二次创作，与 PCL 官方（龙腾猫跃）没有隶属关系，也不代表官方立场。**

**衍生链**：PCL_Max → [PCL_Plus](https://github.com/x1B1C1/PCL_PLus) → [PCL Lite](https://github.com/askbbtsk/pcl-lite) → [PCL（Plain Craft Launcher）](https://github.com/Meloong-Git/PCL)。

按该许可中「重度使用」的要求，本项目已经做到：

- **署名原作者**：「设置 → 软件信息」页面的**首位**给出**龙腾猫跃**的署名与[赞助链接](https://meloong.com/afd/a/LTCat)；
  紧随其后列出 **PCL_Plus** 的创始人 **x1B1C1（xL1nUwU）**
- **公开源代码**：本仓库即全部源码，构建方式见[文档](docs/README.md)
- **沿用原许可**：不另立条款，`LICENSE` 与原许可保持一致
- **名称可辨识**：以 **PCL** 开头并带第三方修改后缀（`PCL_Max`），不暗示由官方提供
- **正版劝导**：保留与 `McLaunchPrecheck` 一致的正版购买劝导与赞助劝导
- **不做赞助解锁**：没有任何「赞助才能解锁」的功能，界面主要色相保持蓝色
- **与 Mojang / Microsoft 无关**：本项目与 Mojang Studios、Microsoft 及其关联公司没有任何关系，未获其批准、授权或背书

**第三方组件另有许可证**，它们只覆盖各自那部分，不改变本项目整体的许可证：

- **基岩版**（下载 + 安装 + 启动）移植自 [PCL-Symbio-Edition](https://github.com/Hydrogen-Peroxide/PCL-Symbio-Edition)，
  该部分仍受 **Apache-2.0** 约束（依据其第 4 条），全文见 [`LICENSE-APACHE`](LICENSE-APACHE)。
- **陶瓦联机（Terracotta）** 以未修改的二进制随启动器分发，受 **AGPL-3.0-or-later**（附打包与 IPC 例外）约束。
  许可全文见 [`LICENSE-AGPL-3.0`](LICENSE-AGPL-3.0)，对应源码随仓库提供于 [`tools/t/`](tools/t/)，
  界面中的版权标注在「设置 → 杂项 → 陶瓦联机」卡片里。
- 其余随包组件（`VCRUNTIME140.DLL`、Lucide 图标等）见 [`NOTICE`](NOTICE)；
  完整的代码来源与第三方库清单见[开源项目清单](docs/CREDITS.md)。

## ❤️ 致谢

这个项目站在三层人的肩膀上，一层都不能省：

- **龙腾猫跃（LTCat）** —— [PCL（Plain Craft Launcher）](https://github.com/Meloong-Git/PCL) 的原作者。
  没有他打的地基就没有后面的一切；欢迎通过[爱发电](https://meloong.com/afd/a/LTCat)支持他。
- **x1B1C1（xL1nUwU）** —— [PCL_Plus](https://github.com/x1B1C1/PCL_PLus) 的创始人。
  本项目并不是从 PCL 直接改的，而是基于他整理好的 PCL_Plus 源码继续开发；
  PCL_Plus 本身又是从 PCL 快照版衍生而来。这一层的工程化工作省了我们非常多力气。
- [PCL Lite](https://github.com/askbbtsk/pcl-lite) 与 [PCL-Symbio-Edition](https://github.com/Hydrogen-Peroxide/PCL-Symbio-Edition) 的维护者 —— 上游链路上的贡献者。
- **DDZmumo** —— [PCL-Living-Fork](https://github.com/DDZmumo/PCL-Living-Fork) 的维护者，存档备份、还原点等功能的思路来源。
- **陶瓦联机（Terracotta）** 作者 burningtnt，以及所有让 Minecraft 社区更好用的开源项目。
- 所有提交过 Issue、PR 与反馈的玩家。

完整的代码来源清单见[开源项目清单](docs/CREDITS.md)，随包分发的第三方组件见 [NOTICE](NOTICE)。
