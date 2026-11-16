[简体中文](README.md) | **English** | [繁體中文](README-ZH_TW.md)

<div align="center">

# PCL_Max

Maintainer: **aaron7357357** · Continues from the source of [PCL_Plus](https://github.com/x1B1C1/PCL_PLus)

[![Stars](https://img.shields.io/github/stars/aaron7357357/PCL-Max?style=for-the-badge&logo=github&labelColor=444444&color=eac54f)](https://github.com/aaron7357357/PCL-Max/stargazers)
[![Release](https://img.shields.io/github/v/release/aaron7357357/PCL-Max?include_prereleases&label=release&logo=github&style=for-the-badge)](https://github.com/aaron7357357/PCL-Max/releases)
[![Downloads](https://img.shields.io/github/downloads/aaron7357357/PCL-Max/total?style=for-the-badge&logo=github&labelColor=444444&color=1F883D)](https://github.com/aaron7357357/PCL-Max/releases)

[Source](https://github.com/aaron7357357/PCL-Max) |
[Downloads](https://github.com/aaron7357357/PCL-Max/releases) |
[Documentation](docs/) |
[License](#-license) |
[Report an issue](https://github.com/aaron7357357/PCL-Max/issues/new)

</div>

> **NOT AN OFFICIAL MINECRAFT PRODUCT.** NOT APPROVED BY OR ASSOCIATED WITH MOJANG OR MICROSOFT.
> 非 MINECRAFT 官方产品。未经 MOJANG 或 MICROSOFT 批准，也不与 MOJANG 或 MICROSOFT 关联。

PCL_Max is a Minecraft launcher that continues from the PCL_Plus source. The goal is simply to make it
nicer to use: clean up the awkward parts, restore what should be there, and tuck away what should not.

This project is released under the **[PCL Distribution Limited License](LICENSE)** — see [License](#-license).

## 📦 Download

Grab the latest build from [Releases](https://github.com/aaron7357357/PCL-Max/releases).

| Arch | File | Notes |
|---|---|---|
| x64 | `PCL_Max_v0.3.0_x64.exe` | Stable build, single file, no extraction needed |
| x64 | `PCL_Max_v0.3.0_Beta1_x64.exe` | Beta build, versioned with `Beta1`, preview channel only |

- The single-file build extracts native libraries into `%TEMP%\.net\<exe name>\` on launch.
  If that path is not writable, use the folder build instead (run `Plain Craft Launcher 2.exe` inside it)
- **Built-in auto-update.** Stable and beta builds share one `main` manifest; the `prerelease` flag on the
  Release plus your "update channel" setting decide what you get — the stable channel only receives
  stable releases, while the preview channel also receives `Beta` / `pre` builds
- **⚠️ You must be on v0.3.0 or later to receive future automatic updates.**
  Older builds (v0.1 / v0.2) point at an obsolete address and can no longer upgrade themselves

## 💻 Supported platforms

| OS | Support | Requirement |
|---|---|---|
| Windows 10 1809 (17763) or later | ✅ Fully supported | [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Windows 8 – Windows 10 1809 (17763) | ⚠️ Should run, best-effort support | [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Windows 7 or earlier | ❌ Not supported | / |
| macOS / Linux / other | ⚠️ Cross-compilation only | [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) |

## 📖 Documentation

| Document | Contents |
| --- | --- |
| [docs/README.md](docs/README.md) | Index, project layout, where the version number lives |
| [Themes](docs/THEME.md) | How the eight color tiers work, presets, custom themes |
| [Config](docs/CONFIG.md) | Config file locations, manual edits, migration |
| [Build](docs/BUILD.md) | Compiling, key injection, publishing |
| [Credits](docs/CREDITS.md) | Code origins, third-party libraries, licenses |
| [NOTICE](NOTICE) | Bundled third-party components and their obligations |

## 🔒 License

This project is released under the **[PCL Distribution Limited License](LICENSE)**; the [`LICENSE`](LICENSE)
file is authoritative.

**PCL_Max is an independent third-party derivative of PCL. It is not affiliated with the PCL author
(龙腾猫跃 / LTCat) and does not represent them.**

**Derivation chain**: PCL_Max → [PCL_Plus](https://github.com/x1B1C1/PCL_PLus) → [PCL Lite](https://github.com/askbbtsk/pcl-lite) → [PCL (Plain Craft Launcher)](https://github.com/Meloong-Git/PCL).

As the license's "heavy use" clause requires, this project:

- **Credits the original author**: the **first** entry on the "Settings → About" page is the original
  author **龙腾猫跃 (LTCat)** with their [sponsorship link](https://meloong.com/afd/a/LTCat);
  immediately after it comes **x1B1C1 (xL1nUwU)**, the founder of **PCL_Plus**
- **Publishes the source**: this repository is the complete source, build steps in the [docs](docs/README.md)
- **Keeps the original license**: no separate terms are added
- **Is clearly named**: begins with **PCL** plus a third-party suffix (`PCL_Max`), never implying official origin
- **Encourages legitimate copies**: retains the same purchase/sponsorship prompts as `McLaunchPrecheck`
- **Has no paywalled features**: nothing is unlocked by donating, and the UI hue stays blue
- **Is unrelated to Mojang / Microsoft**: no approval, authorization, or endorsement from them

Third-party components carry their own licenses, which cover only those parts:

- **Bedrock edition** support is ported from [PCL-Symbio-Edition](https://github.com/Hydrogen-Peroxide/PCL-Symbio-Edition)
  and remains under **Apache-2.0** (per its clause 4); see [`LICENSE-APACHE`](LICENSE-APACHE).
- **Terracotta** ships as an unmodified binary under **AGPL-3.0-or-later** (with packaging and IPC exceptions);
  see [`LICENSE-AGPL-3.0`](LICENSE-AGPL-3.0), source in [`tools/t/`](tools/t/).
- Other bundled components (`VCRUNTIME140.DLL`, Lucide icons, …) are listed in [`NOTICE`](NOTICE);
  the full origin list is in [Credits](docs/CREDITS.md).

## ❤️ Acknowledgements

This project stands on three layers, and none of them can be skipped:

- **龙腾猫跃 (LTCat)** — author of [PCL (Plain Craft Launcher)](https://github.com/Meloong-Git/PCL).
  Nothing here would exist without that foundation; consider [supporting them](https://meloong.com/afd/a/LTCat).
- **x1B1C1 (xL1nUwU)** — founder of [PCL_Plus](https://github.com/x1B1C1/PCL_PLus).
  This project is not a direct modification of PCL; it builds on the PCL_Plus source, which in turn
  derives from a PCL snapshot build. That engineering work saved us a great deal of effort.
- The maintainers of [PCL Lite](https://github.com/askbbtsk/pcl-lite) and
  [PCL-Symbio-Edition](https://github.com/Hydrogen-Peroxide/PCL-Symbio-Edition) — contributors along the chain.
- **DDZmumo** — maintainer of [PCL-Living-Fork](https://github.com/DDZmumo/PCL-Living-Fork), source of ideas
  for save backups and restore points.
- **burningtnt**, author of Terracotta, and everyone making the Minecraft community better through open source.
- Every player who filed an issue, PR, or feedback.

See [Credits](docs/CREDITS.md) for the full list of code origins, and [NOTICE](NOTICE) for bundled components.
