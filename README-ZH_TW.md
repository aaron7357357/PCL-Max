[简体中文](README.md) | [English](README-EN.md) | **繁體中文**

<div align="center">

# PCL_Max

維護者：**aaron7357357** · 基於 [PCL_Plus](https://github.com/x1B1C1/PCL_PLus) 的原始碼繼續開發

[![Stars](https://img.shields.io/github/stars/aaron7357357/PCL-Max?style=for-the-badge&logo=github&labelColor=444444&color=eac54f)](https://github.com/aaron7357357/PCL-Max/stargazers)
[![Release](https://img.shields.io/github/v/release/aaron7357357/PCL-Max?include_prereleases&label=release&logo=github&style=for-the-badge)](https://github.com/aaron7357357/PCL-Max/releases)
[![Downloads](https://img.shields.io/github/downloads/aaron7357357/PCL-Max/total?style=for-the-badge&logo=github&labelColor=444444&color=1F883D)](https://github.com/aaron7357357/PCL-Max/releases)

[原始碼倉庫](https://github.com/aaron7357357/PCL-Max) |
[下載](https://github.com/aaron7357357/PCL-Max/releases) |
[文件](docs/) |
[授權條款](#-授權條款) |
[回報問題](https://github.com/aaron7357357/PCL-Max/issues/new)

</div>

> **非 MINECRAFT 官方產品。** 未經 MOJANG 或 MICROSOFT 批准，也不與 MOJANG 或 MICROSOFT 關聯。
> NOT AN OFFICIAL MINECRAFT PRODUCT. NOT APPROVED BY OR ASSOCIATED WITH MOJANG OR MICROSOFT.

PCL_Max 是一個基於 PCL_Plus 原始碼繼續開發的 Minecraft 啟動器，目標只是讓它更好用一點：
收拾掉那些用起來彆扭的地方，把該有的功能補回來，把沒必要的入口收起來。

本專案沿用 **《PCL 分發有限許可》** 發布，詳見[授權條款](#-授權條款)。

## 📦 下載

前往 [Releases](https://github.com/aaron7357357/PCL-Max/releases) 下載最新版本。

| 架構 | 檔案 | 說明 |
|---|---|---|
| x64 | `PCL_Max_v0.3.0_x64.exe` | 正式版，單檔案、免解壓縮 |
| x64 | `PCL_Max_v0.3.0_Beta1_x64.exe` | 測試版，版本號帶 `Beta1`，僅預覽通道可見 |

- 單檔案版啟動時會把原生程式庫解壓縮到 `%TEMP%\.net\<exe 名稱>\`；若該目錄不可寫，
  請改用倉庫裡的資料夾版（雙擊其中的 `Plain Craft Launcher 2.exe`）
- **啟動器內建自動更新。** 正式版與測試版共用同一份 `main` 分支清單，靠 Release 上的
  `prerelease` 標記與「更新通道」設定區分：穩定通道只收正式版，預覽通道才收得到
  `Beta` / `pre` 版本
- **⚠️ 必須先使用 v0.3.0 及以後的版本，才能自動收到後續更新。**
  更早的版本（v0.1 / v0.2）讀取的是舊位址，已經無法自動升級

## 💻 支援平台

| 作業系統 | 支援情況 | 環境需求 |
|---|---|---|
| Windows 10 1809 (17763) 或更高 | ✅ 完整支援 | [.NET 10 桌面執行階段](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Windows 8 - Windows 10 1809 (17763) | ⚠️ 理論可執行，視情況提供支援 | [.NET 10 桌面執行階段](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Windows 7 或更低版本 | ❌ 不支援 | / |
| macOS / Linux / 其他 | ⚠️ 僅跨平台開發支援（交叉編譯） | [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) |

## 📖 文件

| 文件 | 內容 |
| --- | --- |
| [docs/README.md](docs/README.md) | 索引、專案結構、版本號在哪 |
| [主題與配色](docs/THEME.md) | 八級色階怎麼算、預設主題、自訂主題 |
| [設定與資料目錄](docs/CONFIG.md) | 設定檔位置、手動修改、移轉邏輯 |
| [建置與發布](docs/BUILD.md) | 編譯、金鑰注入、發布 |
| [開源專案清單](docs/CREDITS.md) | 程式碼來源、第三方程式庫、各自的授權條款 |
| [NOTICE](NOTICE) | 隨發行物打包的第三方元件及其授權義務 |

## 🔒 授權條款

本專案沿用 **[《PCL 分發有限許可》](LICENSE)**，完整條款以 [`LICENSE`](LICENSE) 檔案為準。
**PCL_Max 是第三方基於 PCL 獨立進行的二次創作，與 PCL 官方（龙腾猫跃）沒有隸屬關係，也不代表官方立場。**

**衍生鏈**：PCL_Max → [PCL_Plus](https://github.com/x1B1C1/PCL_PLus) → [PCL Lite](https://github.com/askbbtsk/pcl-lite) → [PCL（Plain Craft Launcher）](https://github.com/Meloong-Git/PCL)。

依該授權中「重度使用」的要求，本專案已經做到：

- **署名原作者**：「設定 → 軟體資訊」頁面的**首位**給出原作者 **龙腾猫跃（龍騰貓躍）** 的署名與
  [贊助連結](https://meloong.com/afd/a/LTCat)；緊接其後列出 **PCL_Plus** 的創始人 **x1B1C1（xL1nUwU）**
- **公開原始碼**：本倉庫即全部原始碼，建置方式見[文件](docs/README.md)
- **沿用原授權**：不另立條款，`LICENSE` 與原授權保持一致
- **名稱可辨識**：以 **PCL** 開頭並帶第三方修改後綴（`PCL_Max`），不暗示由官方提供
- **正版勸導**：保留與 `McLaunchPrecheck` 一致的正版購買勸導與贊助勸導
- **不做贊助解鎖**：沒有任何「贊助才能解鎖」的功能，介面主要色相保持藍色
- **與 Mojang / Microsoft 無關**：本專案與 Mojang Studios、Microsoft 及其關聯公司沒有任何關係

第三方元件另有授權條款，它們只涵蓋各自那部分：

- **基岩版**支援移植自 [PCL-Symbio-Edition](https://github.com/Hydrogen-Peroxide/PCL-Symbio-Edition)，
  仍受 **Apache-2.0** 約束；全文見 [`LICENSE-APACHE`](LICENSE-APACHE)。
- **陶瓦聯機（Terracotta）** 以未修改的二進位檔隨啟動器散布，受 **AGPL-3.0-or-later** 約束；
  見 [`LICENSE-AGPL-3.0`](LICENSE-AGPL-3.0)，原始碼隨倉庫提供於 [`tools/t/`](tools/t/)。
- 其餘隨包元件（`VCRUNTIME140.DLL`、Lucide 圖示等）見 [`NOTICE`](NOTICE)；
  完整的程式碼來源清單見[開源專案清單](docs/CREDITS.md)。

## ❤️ 致謝

這個專案站在三層人的肩膀上，一層都不能省：

- **龙腾猫跃（LTCat）** —— [PCL（Plain Craft Launcher）](https://github.com/Meloong-Git/PCL) 的原作者。
  沒有他打下的地基就沒有後面的一切；歡迎透過[愛發電](https://meloong.com/afd/a/LTCat)支持他。
- **x1B1C1（xL1nUwU）** —— [PCL_Plus](https://github.com/x1B1C1/PCL_PLus) 的創始人。
  本專案並不是從 PCL 直接修改的，而是基於他整理好的 PCL_Plus 原始碼繼續開發；
  PCL_Plus 本身又是從 PCL 快照版衍生而來。這一層的工程化工作省了我們非常多力氣。
- [PCL Lite](https://github.com/askbbtsk/pcl-lite) 與
  [PCL-Symbio-Edition](https://github.com/Hydrogen-Peroxide/PCL-Symbio-Edition) 的維護者 —— 上游鏈路上的貢獻者。
- **DDZmumo** —— [PCL-Living-Fork](https://github.com/DDZmumo/PCL-Living-Fork) 的維護者，
  存檔備份、還原點等功能的思路來源。
- **burningtnt** —— 陶瓦聯機作者，以及所有讓 Minecraft 社群更好用的開源專案。
- 所有提交過 Issue、PR 與回饋的玩家。

完整的程式碼來源清單見[開源專案清單](docs/CREDITS.md)，隨包散布的第三方元件見 [NOTICE](NOTICE)。
