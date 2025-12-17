# Changelog

Todos los cambios notables de este proyecto se documentarán en este archivo.

## [v1.1.0] - 2025-12-17 (UI Upgrade)
### ✨ Nuevas Características
- **Doble Click:** Aplicación directa de GIFs como fondo desde la lista.
- **Smart Previews:** Generación automática de thumbnails para GIFs (incluido script `GeneratePreviews.ps1`).
- **UI de Rotación:** Botones rápidos (0°, 90°, 180°, 270°) en la barra inferior.
- **Rutas Inteligentes:** Diálogos "Importar/Exportar" detectan automáticamente carpetas `.photo` y `.smtheme`.
- **Footer Rediseñado:** Mayor tamaño y visibilidad para las herramientas principales.

### 🐛 Bug Fixes
- **Startup Crash:** Resuelto conflicto crítico de recursos XAML (`TextMutedBrush`).
- **Undo/Redo:** Reparada la lógica de actualización de comandos.

## [v1.0.1] - 2025-12-17 (Hotfix)
### 🐛 Bug Fixes
- **Permisos de Archivos:** Solucionado crash al intentar crear temas si la app estaba instalada en *Program Files*. Ahora los temas de usuario se guardan correctamente en *Mis Documentos/SnakeMarsTheme*.
- **Instalador:** Ahora solicita permisos de Administrador para instalar correctamente en *Program Files*.
- **Gestión de Temas:** La aplicación ahora combina temas de instalación (solo lectura) y temas de usuario (escritura).

## [v1.0.0] - 2025-12-17

### 🚀 Primera Release Pública

La primera versión estable de **SnakeMarsTheme** - Suite completa para crear y gestionar temas para pantallas LCD Mars Gaming VMAX, SOEYI y displays IPS USB.

#### 📦 Distribución (Full & Light)
- **Instalador (.exe):** Instalación profesional (Inno Setup) con todo incluido.
- **Versión Full (.zip):** Portable completa con videos y GIFs.
- **Versión Light (.zip):** Portable ligera (~150MB).
- **Offline Ready:** Todas las versiones incluyen FFmpeg.
- **Automatización:** Nuevo script `build/Publish-Release.ps1` para generar releases.

#### 🛠️ Bug Fixes (Hotfixes v1.0.0)
- **Crítico:** Solucionado crash por estilo faltante `ActionSecondaryButtonStyle` en Wizard.
- **Crítico:** Solucionado error sintaxis XAML `Margin` con llave extra.
- Solucionado detección de FFmpeg offline.

#### ✨ Características Principales

**Editor Visual de Temas**
- Interfaz drag & drop para diseño de temas
- 75+ widgets de monitorización (CPU, GPU, RAM, Disco, Ventiladores, Clima)
- Panel de propiedades en tiempo real (posición, fuente, color, tamaño)
- Zoom 20%-200% con scroll o botones
- Undo/Redo (Ctrl+Z, Ctrl+Y)
- Copiar/Pegar/Duplicar widgets
- Selección múltiple y alineación automática
- Preview de Setting.txt en tiempo real
- Validación de temas antes de exportar

**Wizard de Creación de Temas**
- Asistente guiado de 4 pasos
- Soporte para 17 resoluciones predefinidas
- Extracción de frames desde GIF (System.Drawing)
- Extracción de frames desde Video MP4/AVI/WMV (FFmpeg integrado)
- Control de FPS (5-60 frames)
- Auto-descarga de FFmpeg en primer uso

**Descargador de Temas**
- Catálogo de 462 temas gratuitos
- Descarga masiva desde HuggingFace (1.77 GB)
- Filtros por resolución y orientación
- Instalación directa a SOEYI/Mars Gaming
- Previews locales sincronizados

**Formatos Soportados**
- `.smtheme` - Formato abierto (ZIP sin contraseña)
- `.photo` - Formato SOEYI (7-Zip con password)
- `.smtproj` - Proyectos editables
- JSON/Setting.txt - Configuración Mars Gaming

#### 🏗️ Arquitectura

- **Framework**: .NET 8.0 WPF
- **Patrón**: MVVM con CommunityToolkit.Mvvm
- **Servicios**: 11 servicios de negocio integrados
- **ViewModels**: 4 (Main, Wizard, Editor, Downloader)
- **Código auditado**: 0 módulos huérfanos

#### 📦 Requisitos

- Windows 10/11
- .NET 8.0 Runtime

#### 🔑 Información Técnica

- **Password archivos .photo**: `vmax2025`
- **Repositorio HuggingFace**: [snakefoxu/soeyi-themes](https://huggingface.co/datasets/snakefoxu/soeyi-themes)

---

## Historial de Desarrollo (Interno)

> Las siguientes versiones representan el desarrollo interno previo a la release pública.

<details>
<summary>Ver historial completo de desarrollo</summary>

### [v4.2.1] - 2025-12-17 (Pre-release)
- Auditoría completa del código
- Verificación de 11 servicios integrados
- Limpieza de backups obsoletos
- Actualización de documentación

### [v4.2.0] - 2025-12-16 (Pre-release)
- Reconstrucción completa del Theme Editor
- Fix de drag & drop y actualización de colores
- Botones estilo macOS con transiciones suaves

### [v4.1.0] - 2025-12-14 (Pre-release)
- Formato unificado .smtheme
- Ingeniería inversa de TURZX (.turtheme)
- Integración con HuggingFace

### [v4.0.0] - 2025-12-14 (Pre-release)
- Reescritura completa en C# WPF
- Arquitectura MVVM
- UI moderna con tema oscuro

</details>

---

*SnakeMarsTheme v1.0 - Desarrollado por SnakeFoxu*
