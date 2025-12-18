<p align="center">
  <img src="https://raw.githubusercontent.com/Snakefoxu/marsmakertheme/main/assets/branding/github_banner.png" alt="SnakeMarsTheme - Gestor de Temas para Pantallas LCD Mars Gaming y SOEYI" height="350">
</p>

# SnakeMarsTheme - El Gestor Definitivo para Mars Gaming & SOEYI

> **La Suite Inteligente para Pantallas Mars Gaming VMAX, SOEYI y Displays IPS Turzx**
> *La única herramienta todo-en-uno para crear, convertir y personalizar temas para la pantalla LCD de tu caja de PC.*

[![Versión](https://img.shields.io/badge/versión-4.2.1-blue)](https://github.com/Snakefoxu/marsmakertheme/releases)
[![Plataforma](https://img.shields.io/badge/plataforma-Windows-lightgrey)]()
[![Framework](https://img.shields.io/badge/.NET-8.0-purple)]()
[![Temas en HuggingFace](https://img.shields.io/badge/HuggingFace-462_temas-orange)](https://huggingface.co/datasets/snakefoxu/soeyi-themes)

---

## 📖 [VER MANUAL DE USUARIO (Instrucciones Completas)](USER_MANUAL.md)

---

## 🔥 Características Principales (v4.2.1)

### 🎨 Editor Visual de Temas (WYSIWYG)
- **Interfaz Drag & Drop**: Diseña temas personalizados fácilmente para pantallas USB IPS de 3.5" o 5".
- **32 Widgets Oficiales Mars Gaming**: Validados al 100% en hardware real
    - **CPU**: CPUTemp, CpuUsage, CpuFrequency, CpuVoltage, CpuTEC
    - **GPU**: GPUTemp, GpuUsage, GpuFrequency, GPUMemoryFrequency, GpuTEC
    - **Sistema**: CurrentTime, CurrentDate, LunarDate, ScreenBrightness, PowerMode, WeatherInfo
    - **Red**: UpNetSpeed, DownNetSpeed, WifiState, WifiName, ConnectedWifiSSID
    - **Hardware**: BatteryLevel, CapLockPressed, NumLockPressed, MemoryUsage, DiskTemp
    - **Bluetooth/Audio**: BleState, IsMute, Volume, y más
- **Control Total de Medios**: 
    - **Rotación**: Rota fondos 90°/180°/270° al instante.
    - **Formatos**: Soporte para GIF (Type 0 validado), Imagen estática, Video (.mp4).
- **Deshacer/Rehacer**: Sistema robusto para editar sin miedo.
- **Instalación Directa**: Botones para instalar en Mars Gaming/SOEYI con auto-elevación.

### 🧙‍♂️ Asistentes Inteligentes
- **Wizard de Inicio**: Crea temas base en 3 pasos (Pantalla -> Orientación -> Estilo).
- **Importador Inteligente**: Detecta automáticamente si importas un archivo `.smtheme` o `.photo` y lo coloca donde debe ir.

### 📥 Librería de Temas en la Nube
- **462+ Temas Gratuitos**: Accede a una base de datos masiva de temas de la comunidad.
- **Vistas Previas Reales**: Sistema de thumbnails optimizado (320px) para navegar rápido por el catálogo.
- **Filtros Avanzados**: Encuentra temas por resolución (Horizontal, Vertical, Cuadrada/AIO).

### 🛠️ Herramientas de Animación
- **GIF a Tema**: Convierte instantáneamente GIFs genéricos en temas de hardware compatibles.
- **Video a Frames**: Extrae frames de alta calidad de MP4/AVI para una reproducción fluida.

---

## 🛠️ Instalación y Puesta en Marcha

### Requisitos del Sistema
- Windows 10 / 11
- .NET 8.0 Runtime (necesario para ejecutar la aplicación)

### Cómo Ejecutar
1. Descarga la última versión desde [Releases](https://github.com/Snakefoxu/marsmakertheme/releases).
2. Descomprime el archivo.
3. Ejecuta `SnakeMarsTheme.exe`.

---

## 📂 Estructura del Repositorio Limpio

```
SnakeMarsTheme/
├── src/
│   ├── SnakeMarsTheme/       # Aplicación Principal (WPF .NET 8)
│   └── ThemeExtractor/       # CLI para extraer temas TURZX
├── resources/                 # Assets vitales (Temas, GIFs, Previews reducidos)
├── releases/                  # Binarios compilados (Solo última versión)
└── USER_MANUAL.md             # Guía de uso detallada
```

---

## 📚 Documentación y Guías

| Documento | Descripción |
|-----------|-------------|
| [USER_MANUAL.md](USER_MANUAL.md) | **Instrucciones de uso** paso a paso |
| [CHANGELOG.md](CHANGELOG.md) | Registro de cambios (v1.1.0 actual) |
| [docs/investigacion/](docs/investigacion/) | Análisis técnico de formatos SOEYI/TURZX |

---

## 🔑 Información Técnica para Enthusiastas

- **Contraseña Archivos .photo**: `vmax2025` (Utilizada en temas encriptados originales)
- **Repositorio de Temas**: [snakefoxu/soeyi-themes](https://huggingface.co/datasets/snakefoxu/soeyi-themes)
- **17 Resoluciones Soportadas**: Soporte nativo para casi cualquier pantalla USB del mercado asiático/occidental.

---

## 🤝 Créditos y Atribución

Desarrollado con ❤️ por **SnakeFoxu** para la comunidad de modding de PC.

*Agradecimiento especial a las comunidades de SOEYI y TURZX por la investigación e inspiración.*

---

**Licencia**: MIT - Código Abierto y Gratuito.
