<p align="center">
  <img src="https://raw.githubusercontent.com/Snakefoxu/marsmakertheme/main/assets/branding/github_banner.png" alt="SnakeMarsTheme - Theme Manager for Mars Gaming and SOEYI LCD Screens" height="350">
</p>

# SnakeMarsTheme - The Ultimate Manager for Mars Gaming & SOEYI

🌐 **Language / Idioma:** **English** | [Español](README_ES.md)

> **The Smart Suite for Mars Gaming VMAX, SOEYI and Turzx IPS Displays**
> *The only all-in-one tool to create, convert, and customize themes for your PC case's LCD screen.*

[![Version](https://img.shields.io/badge/version-4.2.1-blue)](https://github.com/Snakefoxu/marsmakertheme/releases)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)]()
[![Framework](https://img.shields.io/badge/.NET-8.0-purple)]()
[![HuggingFace Themes](https://img.shields.io/badge/HuggingFace-462_themes-orange)](https://huggingface.co/datasets/snakefoxu/soeyi-themes)

---

## 📖 [VIEW USER MANUAL (Complete Instructions)](USER_MANUAL.md)

---

## 🔥 Main Features (v4.2.1)

### 🎨 Visual Theme Editor (WYSIWYG)
- **Drag & Drop Interface**: Easily design custom themes for 3.5" or 5" USB IPS screens.
- **32 Official Mars Gaming Widgets**: 100% validated on real hardware
    - **CPU**: CPUTemp, CpuUsage, CpuFrequency, CpuVoltage, CpuTEC
    - **GPU**: GPUTemp, GpuUsage, GpuFrequency, GPUMemoryFrequency, GpuTEC
    - **System**: CurrentTime, CurrentDate, LunarDate, ScreenBrightness, PowerMode, WeatherInfo
    - **Network**: UpNetSpeed, DownNetSpeed, WifiState, WifiName, ConnectedWifiSSID
    - **Hardware**: BatteryLevel, CapLockPressed, NumLockPressed, MemoryUsage, DiskTemp
    - **Bluetooth/Audio**: BleState, IsMute, Volume, and more
- **Full Media Control**: 
    - **Rotation**: Rotate backgrounds 90°/180°/270° instantly.
    - **Formats**: Support for GIF (Type 0 validated), Static image, Video (.mp4).
- **Undo/Redo**: Robust system for fearless editing.
- **Direct Installation**: Buttons to install on Mars Gaming/SOEYI with auto-elevation.

### 🧙‍♂️ Smart Wizards
- **Startup Wizard**: Create base themes in 3 steps (Screen -> Orientation -> Style).
- **Smart Importer**: Auto-detects if you're importing a `.smtheme` or `.photo` file and places it correctly.

### 📥 Cloud Theme Library
- **462+ Free Themes**: Access a massive community theme database.
- **Real Previews**: Optimized thumbnail system (320px) for fast catalog browsing.
- **Advanced Filters**: Find themes by resolution (Horizontal, Vertical, Square/AIO).

### 🛠️ Animation Tools
- **GIF to Theme**: Instantly convert generic GIFs into compatible hardware themes.
- **Video to Frames**: Extract high-quality frames from MP4/AVI for smooth playback.

---

## 🛠️ Installation & Setup

### System Requirements
- Windows 10 / 11
- .NET 8.0 Runtime (required to run the application)

### How to Run
1. Download the latest version from [Releases](https://github.com/Snakefoxu/marsmakertheme/releases).
2. Extract the archive.
3. Run `SnakeMarsTheme.exe`.

---

## 📂 Clean Repository Structure

```
SnakeMarsTheme/
├── src/
│   ├── SnakeMarsTheme/       # Main Application (WPF .NET 8)
│   └── ThemeExtractor/       # CLI to extract TURZX themes
├── resources/                # Vital assets (Themes, GIFs, Reduced previews)
├── releases/                 # Compiled binaries (Latest version only)
└── USER_MANUAL.md            # Detailed usage guide
```

---

## 📚 Documentation & Guides

| Document | Description |
|----------|-------------|
| [USER_MANUAL.md](USER_MANUAL.md) | **Step-by-step usage instructions** |
| [CHANGELOG.md](CHANGELOG.md) | Changelog (current v1.1.0) |
| [docs/investigacion/](docs/investigacion/) | Technical analysis of SOEYI/TURZX formats |

---

## 🔑 Technical Information for Enthusiasts

- **Password for .photo Files**: `vmax2025` (Used in original encrypted themes)
- **Theme Repository**: [snakefoxu/soeyi-themes](https://huggingface.co/datasets/snakefoxu/soeyi-themes)
- **17 Supported Resolutions**: Native support for almost any USB screen from Asian/Western markets.

---

## 🤝 Credits & Attribution

Developed with ❤️ by **SnakeFoxu** for the PC modding community.

*Special thanks to the SOEYI and TURZX communities for research and inspiration.*

---

**License**: MIT - Open Source and Free.
