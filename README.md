<p align="center">
  <img src="https://raw.githubusercontent.com/Snakefoxu/marsmakertheme/main/assets/branding/github_banner.png" alt="SnakeMarsTheme - Gestor de Temas para Pantallas LCD Mars Gaming y SOEYI" height="350">
</p>

# SnakeMarsTheme - El Gestor Definitivo para Mars Gaming & SOEYI

> **La Suite Inteligente para Pantallas Mars Gaming VMAX, SOEYI y Displays IPS Turzx**
> *La única herramienta todo-en-uno para crear, convertir y personalizar temas para la pantalla LCD de tu caja de PC.*

[![Versión](https://img.shields.io/badge/versión-1.0-blue)](https://github.com/Snakefoxu/marsmakertheme/releases)
[![Plataforma](https://img.shields.io/badge/plataforma-Windows-lightgrey)]()
[![Framework](https://img.shields.io/badge/.NET-8.0-purple)]()
[![Temas en HuggingFace](https://img.shields.io/badge/HuggingFace-462_temas-orange)](https://huggingface.co/datasets/snakefoxu/soeyi-themes)

---

## 🔥 Características Principales para Personalización de PC

### 🎨 Editor Visual de Temas (WYSIWYG)
- **Interfaz Drag & Drop**: Diseña temas personalizados fácilmente para tus pantallas USB IPS genéricas de 3.5" o 5".
- **75+ Widgets en Vivo**: Monitoriza Temperatura CPU, Uso de GPU, Velocidad RAM, Red, Clima, Ventiladores y más.
- **Vista Previa en Tiempo Real**: Visualiza exactamente cómo quedará tu tema en tu dispositivo Mars Gaming o SOEYI antes de exportar.

### 📦 Formatos de Tema Soportados
- **`.smtheme`**: Estándar Abierto (ZIP sin contraseña) para compartir fácilmente.
- **`.photo`**: Soporte para formato legado SOEYI (Autodesencriptado de temas chinos).
- **Configuración JSON**: Soporte de instalación directa para el software de Mars Gaming.

### 📥 Librería de Temas en la Nube
- **462+ Temas Gratuitos**: Accede a una base de datos masiva de temas de la comunidad (1.77 GB).
- **Descarga por Lotes**: Descarga masiva en un clic desde HuggingFace.
- **Filtros Inteligentes**: Encuentra temas por resolución (Horizontal 320x240, Vertical 480x800, pantallas AIO de refrigeración líquida).

### 🎬 Herramientas de Animación Avanzadas
- **GIF a Tema**: Convierte instantáneamente GIFs genéricos en temas de hardware compatibles.
- **Video a Frames**: Extrae frames de alta calidad de MP4/AVI para una reproducción fluida.
- **Control de FPS**: Optimiza el rendimiento con soporte de reproducción de hasta 60fps.

---

## 🛠️ Instalación y Puesta en Marcha

### Requisitos del Sistema
- Windows 10 / 11
- .NET 8.0 Runtime (necesario para ejecutar la aplicación)

### Cómo Ejecutar
1. Descarga la última versión desde [Releases](https://github.com/Snakefoxu/marsmakertheme/releases).
2. Descomprime el archivo.
3. Ejecuta `SnakeMarsTheme.exe`.

```bash
# O si prefieres compilarlo tú mismo:
git clone https://github.com/Snakefoxu/marsmakertheme.git
cd marsmakertheme/src
dotnet build SnakeMarsTheme.sln
dotnet run --project SnakeMarsTheme/SnakeMarsTheme.csproj
```

---

## 📂 Estructura del Repositorio

```
SnakeMarsTheme/
├── src/
│   ├── SnakeMarsTheme/       # Aplicación Principal (WPF .NET 8)
│   │   ├── Services/         # 11 servicios de negocio
│   │   ├── ViewModels/       # 4 ViewModels (MVVM)
│   │   ├── Views/            # 3 vistas XAML
│   │   ├── Models/           # 5 modelos de datos
│   │   └── Helpers/          # Converters y utilidades
│   └── ThemeExtractor/       # CLI para extraer temas TURZX
├── resources/                 # Assets y recursos locales (excluidos del git)
├── docs/                      # Documentación técnica
├── build/                     # Scripts de compilación
└── CHANGELOG.md               # Historial de cambios v1.0
```


---

## 📚 Documentación y Guías

| Documento | Descripción |
|-----------|-------------|
| [CHANGELOG.md](CHANGELOG.md) | Registro de cambios (v4.2.1 actual) |
| [docs/investigacion/](docs/investigacion/) | Análisis técnico de formatos SOEYI/TURZX |

---

## 🔑 Información Técnica para Enthusiastas

- **Contraseña Archivos .photo**: `vmax2025` (Utilizada en temas encriptados originales)
- **Repositorio de Temas**: [snakefoxu/soeyi-themes](https://huggingface.co/datasets/snakefoxu/soeyi-themes)
- **17 Resoluciones Soportadas**:
  - **Vertical**: 360x960, 320x960, 379x960, 462x1920
  - **Horizontal**: 960x360, 960x320, 960x376, 960x480, 1920x462, 1920x480, 1600x600, 1024x600
  - **Cuadrada/AIO**: 480x480, 320x240, 240x320, 480x272


---

## 🤝 Créditos y Atribución

Desarrollado con ❤️ por **SnakeFoxu** para la comunidad de modding de PC.

*Agradecimiento especial a las comunidades de SOEYI y TURZX por la investigación e inspiración.*

---

**Licencia**: MIT - Código Abierto y Gratuito.
