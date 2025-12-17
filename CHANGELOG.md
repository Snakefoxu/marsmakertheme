# Changelog

Todos los cambios notables de este proyecto se documentarán en este archivo.

## [v4.2.1] - 2025-12-17

### 🔍 Auditoría y Limpieza del Proyecto

Auditoría completa del código fuente para verificar integración de todos los componentes.

#### ✅ Verificado
- **11 Servicios**: Todos integrados y activos en ViewModels
  - ThemeService, ThemeCatalogService, AnimationService, DownloadService
  - ExtractionService, InstallationService, ProjectService, SettingParser
  - SettingPreviewService, SmthemePackagerService, ThemeCreatorService
- **4 ViewModels**: MainViewModel, WizardViewModel, ThemeEditorViewModel, DownloaderViewModel
- **5 Models**: Theme, ThemeProject, Resolution, ThemeTemplate, WidgetTypes
- **5 Converters**: BoolToVisibility, InverseBoolToVisibility, InvertBool, BoolToBorderThickness, NotNullToBool
- **0 código huérfano** en el core de la aplicación

#### 🗑️ Limpieza Ejecutada
- Eliminado `.backup_wizard_20251216/` (backup obsoleto)
- Eliminado `.backup_working_20251216_0149/` (backup obsoleto)
- Eliminado `Controls/` (carpeta vacía sin uso)

#### 📝 Documentado
- Proyectos auxiliares no incluidos en .sln: SmthemeTest, VerificationApp
- ThemeExtractor incluido en .sln como herramienta CLI separada
- Carpeta `archive/` confirmada excluida en .gitignore

---

## [v4.2.0] - 2025-12-16

### 🔧 Theme Editor - Reconstrucción Completa


El Editor Visual ha sido reescrito desde cero para solucionar problemas críticos de drag & drop y actualización de colores.

#### ✨ Arreglado
- **Drag & Drop funcionando**: Los widgets ahora se pueden arrastrar correctamente en el canvas
- **SnapToGrid corregido**: Ahora aplica el snap solo al soltar el mouse, no durante el arrastre
  - Antes: El widget se "atascaba" porque cada pequeño movimiento se redondeaba de vuelta
  - Ahora: Movimiento fluido durante drag, alineación precisa al soltar
- **Colores se actualizan en tiempo real**: Añadido PropertyChanged para ForegroundBrush cuando Color cambia
- **Borde de selección visible**: BorderColor ahora notifica cambios cuando IsSelected cambia

#### 🛠️ Cambios Técnicos
- **ThemeEditorView.xaml.cs**: Reescrito completamente con código limpio y organizado
  - Secciones claramente documentadas
  - Uso de ThemeCanvas directo para mouse capture (no canvas dinámico)
- **ThemeEditorView.xaml**: Template de widget simplificado
  - Eliminado ContentControl anidado innecesario
  - Grid directo con TextBlock/Border
- **PlacedWidgetItem**: Añadidos partial methods `OnColorChanged`, `OnIsSelectedChanged`, etc.

#### 🎨 UI Improvements
- Botones con estilo macOS (pill-shaped, transiciones suaves)
- Estilos de color: Primary (cyan), Success (verde), Warning (naranja), Danger (rojo)
- TextElement.Foreground correctamente propagado en todos los estilos de botón
- Layout del Editor reorganizado para mejor uso del espacio

---

## [v4.1.0] - 2025-12-14

### 🚀 Cambio de Paradigma: Formato Unificado (.smtheme)
Hemos unificado tres ecosistemas de temas (SOEYI, TURZX, Python) en un solo formato estándar abierto, simplificando radicalmente la gestión y distribución de temas.

#### ✨ Nuevo
- **Formato `.smtheme`**: Estándar unificado basado en ZIP (sin contraseña/encriptación).
  - Incluye `manifest.json` (metadatos estandarizados), `background.png`, `preview.png` y archivos de configuración originales.
  - Diseñado para ser interoperable entre diferentes tipos de pantalla y motores.
  - **SmthemePackagerService**: Nuevo servicio C# para empaquetado, desempaquetado y validación de temas `.smtheme`.
  
- **Ingeniería Inversa de TURZX (.turtheme)**:
  - Completado análisis del formato binario propietario de TURZX (.NET BinaryFormatter).
  - Creada herramienta **ThemeExtractor** capaz de deserializar objetos, resolver dependencias de versión y extraer recursos gráficos.
  - Lograda extracción exitosa de 138/142 temas TURZX únicos.

- **Nube y Distribución**:
  - **Catálogo Centralizado**: Generado `catalog.json` con índice de 355 activos (271 temas + 84 videos).
  - **HuggingFace Integration**: Subidos 271 temas convertidos al formato `.smtheme` al repositorio `snakefoxu/soeyi-themes`.
  - Estructura de repositorio híbrida soportando tanto temas legacy (`themes/`) como el nuevo formato (`smtheme/`).

#### 🛠️ Mejoras Técnicas
- **Conversión Masiva**:
  - Convertidos 67 temas Python → `.smtheme`.
  - Convertidos 204 temas TURZX → `.smtheme`.
- **Limpieza de Recursos**:
  - Reorganización total de la carpeta `resources/` (~5.2 GB), eliminando duplicados y archivos temporales.
  - Consolidación de previews (479 imágenes generadas y normalizadas).

---

### ✨ Nueva Característica: Soporte de Animación por Frames

**Creación de temas animados desde GIF y Video**

Esta versión añade soporte completo para crear temas animados convirtiendo archivos GIF o Video a secuencias de frames PNG, compatibles con las limitaciones de hardware SOEYI/Mars Gaming.

#### Añadido
- 🎬 **AnimationService.cs**: Nuevo servicio para extracción de frames
  - Extracción de frames desde GIF (usando System.Drawing)
  - Extracción de frames desde Video MP4/AVI/WMV (usando Xabe.FFmpeg)
  - Límite automático de 60 frames (restricción de hardware)
  - Auto-descarga de binarios FFmpeg (~80MB) en primer uso
  
- 🧙 **Wizard mejorado** con panel de animación:
  - ComboBox para seleccionar tipo de fondo (Imagen Estática, GIF Animado, Video, Secuencia de Frames)
  - Slider de FPS (5-30) para control de densidad de frames en videos
  - Botón "⚡ Extraer Frames" con feedback en tiempo real
  - Contador de frames extraídos con información de progreso
  
- 📦 **ThemeCreatorService actualizado**:
  - Exportación de frames numerados (1.png, 2.png, ..., N.png)
  - Mensaje personalizado mostrando conteo de frames generados

#### Dependencias Nuevas
- ✅ **Xabe.FFmpeg** v5.2.6 - Wrapper .NET para FFmpeg
- ✅ **Xabe.FFmpeg.Downloader** - Auto-descarga de binarios FFmpeg
- ℹ️ FFmpeg se descarga automáticamente en `%LocalAppData%\SnakeMarsTheme\FFmpeg`

#### Flujo de Trabajo
1. Abrir Wizard → Seleccionar tipo "GIF Animado" o "Video"
2. Explorar y seleccionar archivo .gif / .mp4
3. (Solo para video) Ajustar FPS con slider
4. Click "Extraer Frames" → Esperar extracción automática
5. Añadir widgets opcionales
6. Crear tema → Genera 1.png, 2.png, ..., N.png

#### Notas Técnicas
- No requiere instalación manual de FFmpeg
- GIF funciona offline (solo System.Drawing)
- Video requiere internet en primera extracción (~80MB descarga)
- Máximo 60 frames por restricción de hardware SOEYI/Mars
- Frames se exportan como PNG sin compresión

---

## [4.0.7] - 2025-12-14
### Añadido
- **Preview y Validación de Setting.txt**: Panel de vista previa en tiempo real en ThemeEditorView
  - Panel expandible mostrando el código Setting.txt generado
  - Auto-actualización cuando se añaden, eliminan o modifican widgets
  - Fuente monospace (Consolas) para legibilidad del código
  - Botón para copiar al portapapeles para acceso rápido
  - Sistema de validación con 9 verificaciones diferentes:
    - Dimensiones del tema (width/height válidos)
    - Existencia del archivo de fondo
    - Límites de posición de widgets (X/Y dentro del canvas)
    - Disponibilidad de fuentes (conocidas vs. desconocidas)
    - Rango de tamaño de fuente (6px - 200px)
    - Validación de formato de color (#RGB, #RRGGBB, #AARRGGBB)
    - Verificación de asignación de DataType
  - Mensajes de validación con niveles de severidad (Éxito, Info, Advertencia, Error)
  - Retroalimentación en tiempo real previene errores de exportación

### Modificado
- **ThemeEditorViewModel**: Añadidas propiedades de preview y validación
  - SettingPreviewText - String observable para contenido del preview
  - ValidationErrors - Colección observable de mensajes de validación
  - IsPreviewExpanded - Toggle para visibilidad del panel
  - Método UpdatePreview() llamado automáticamente al cambiar widgets
  - CopySettingToClipboardCommand para exportar texto del preview

- **ThemeEditorView**: Modificado diseño del panel derecho
  - Cambiado de grid de 2 filas a 3 filas
  - Añadido control Expander para panel de preview
  - Panel de preview posicionado entre título y propiedades
  - Lista de mensajes de validación con coloreado dinámico
  - TextBox de 150px de altura con scroll horizontal/vertical

### Archivos Nuevos
- **SettingPreviewService.cs**: Servicio para generar y validar temas
  - GenerateSettingPreview(ThemeProject) - Convierte proyecto a formato Setting.txt
  - ValidateSetting(ThemeProject) - Devuelve lista de mensajes de validación
  - Soporta todos los tipos de widgets (Text, BorderLine, DefaultLine, GridLine)
  - Formato adecuado con encabezados, comentarios y sintaxis de parámetros

## [4.0.6] - 2025-12-14
### Añadido
- **Rediseño Completo de DownloaderView**: Paridad completa de características con PowerShell v2.4
  - TabControl con 2 pestañas: "Descargar Temas" y "Descarga Masiva"
  - Layout de 3 columnas: Lista de temas, Preview+Acciones, Info Catálogo
  - Filtro desplegable de resolución con más de 15 opciones
  - Lista de temas con formato `[resolución] nombre (ID: X)` en fuente Consolas
  - Panel de preview de imagen del tema
  - Panel de información del catálogo (total temas, resoluciones, tamaño, formato)
  - Botón de descarga oficial SOEYI (verde)
  - Botón de descarga espejo HuggingFace (naranja)
  - Botones Instalar en SOEYI / Mars Gaming
  - Botones Reiniciar SOEYI / Mars Gaming
  - Botón Extraer y Editar para edición de temas
  - Pestaña Descarga Masiva desde HuggingFace (1.68 GB)
  - Función de descarga por IDs específicos
  - Barra de progreso y estado de descarga masiva

### Modificado
- **DownloaderViewModel**: Añadidas nuevas propiedades y comandos
  - ThemePreviewImage, HasPreview
  - SelectedThemeId, SelectedThemeResolution
  - CatalogTotalThemes, CatalogResolutions, CatalogTotalSize
  - ThemeIdsToDownload, BulkDownloadProgress, BulkDownloadStatus
  - DownloadFromSOEYICommand, DownloadAllThemesCommand
  - DownloadByIdsCommand, ExtractAndEditCommand

- **Modelo RemoteTheme**: Añadida propiedad `Id`

- **DownloadService**: Añadido método `DownloadFileAsync` para URLs arbitrarias

## [4.0.1] - 2025-12-14
### Mejorado
- **Rediseño Completo de WizardView**: Asistente mejorado de creación de temas en 4 pasos
  - Indicadores de paso ahora se muestran como botones estilizados con resaltado de estado activo
  - Paso 1 (Configuración): Layout limpio de formulario con secciones para nombre, resolución y tipo de tema
  - Paso 2 (Fondo): Layout de dos columnas con opciones a la izquierda y preview a la derecha
  - Paso 3 (Widgets): Selector de widgets mejorado con secciones apropiadas y editor de propiedades
  - Paso 4 (Resumen): Muestra detalles de configuración y lista de archivos a generar
  - Barra de navegación con botones estilizados y contenido basado en pasos

- **Mejoras en ThemeEditorView**: 
  - Añadido nombre de tema editable en el pie
  - Añadidas dimensiones ancho/alto editables
  - Visualización de conteo de widgets
  - Biblioteca de fuentes expandida: 21 fuentes organizadas por categoría (Popular, Monospace, Decorativa, Display, Divertida)
  - Biblioteca de colores expandida: 25 colores organizados por categoría (Básicos, Cyan/Azul, Verde, Rojo/Rosa, Amarillo/Naranja, Púrpura)

- **Pestaña de Configuración**: Implementación completa con tres secciones
  - Rutas: Salida de temas, ruta de 7-Zip, URL de HuggingFace
  - Herramientas: Verificación de instalación de 7-Zip, SOEYI, Mars Gaming
  - Estadísticas: Conteo de temas, tamaño total, conteo extraído

- **Consistencia de UI**: Todas las vistas ahora usan márgenes consistentes de 15px, radio de esquina de 8px y encabezados de sección apropiados

## [4.0.0] - 2025-12-14
### Añadido
- **Aplicación C# WPF**: Reescritura completa de la aplicación en C# con WPF
  - UI moderna con tema oscuro y 5 pestañas
  - Arquitectura MVVM con CommunityToolkit.Mvvm
  - Dirigida a .NET 8
  
- **Editor Visual de Temas** (`ThemeEditorView.xaml`):
  - Colocación de widgets con arrastrar y soltar en canvas
  - Preview en tiempo real con controles de zoom (20%-200%)
  - Panel de propiedades de widgets (posición, fuente, color, tamaño)
  - Soporte para todos los tipos de widgets: Text, BorderLine, DefaultLine, GridLine
  - Selector de imagen de fondo
  - Guardado de temas (genera back.png, demo.png, Setting.txt, JSON)

- **Asistente de Temas** (proceso de 4 pasos):
  - Paso 1: Configuración del tema (nombre, resolución)
  - Paso 2: Selección de fondo (PNG/GIF)
  - Paso 3: Editor de widgets con categorías
  - Paso 4: Resumen y exportación

- **Descargador de Temas**:
  - Integración con HuggingFace para descargas de temas
  - Soporte de extracción con 7-Zip
  - Instalación directa en SOEYI/Mars Gaming

- **17 Presets de Resolución**: Añadido soporte completo de resolución incluyendo:
  - Vertical: 360x960, 320x960, 379x960, 462x1920
  - Horizontal: 960x360, 960x320, 960x376, 960x480, 1920x462, 1920x480
  - Cuadrada/AIO: 480x480, 320x240, 240x320, 480x272
  - Opción de resolución personalizada

### Modificado
- **Estructura del Proyecto**: Migrado de PowerShell a C# WPF
- **UI**: Tema oscuro moderno con colores de acento

## [3.0.0] - 2025-12-14
### Modificado
- **Reorganización Masiva**: Migrada estructura del proyecto al estándar v3.
  - Creado `src/Modules` para componentes principales (Parser, Editor, Animation).
  - Creado `src/Scripts` para herramientas auxiliares.
  - Movidos scripts legacy a `archive/`.
  - Creado lanzador raíz `Run.ps1`.
  - Renombrado `Unified.ps1` a `src/App.ps1`.

## [2.5.1] - 2025-12-14
### Corregido
- **Parser de Setting.txt**: Corregida carga automática de `back.png` como fondo (z=-100 implícito)
- **Búsqueda de Imágenes**: El parser ahora busca en la carpeta `source/` automáticamente

### Modificado
- **Reorganización de Documentación**: Reestructurada carpeta `docs/` para mejor navegación:
  - `docs/guias/` - Tutoriales y guías de inicio
  - `docs/referencia/` - Especificaciones técnicas (Setting.txt, widgets, temas)
  - `docs/investigacion/` - Ingeniería inversa y análisis
  - `docs/archivo/` - Documentos históricos (nada eliminado, solo organizado)
- **Limpieza Raíz**: Movidos AI_HANDOVER_BRIEFING.md, INTEGRATION_PLAN.md, TODO.md a archivo
- **Control de Versiones**: Establecido changelog cronológico para mejor seguimiento

## [2.5.0] - 2025-12-13
### Añadido
- **Parser de Setting.txt**: Intérprete completo para archivos de configuración de temas SOEYI/Mars Gaming.
  - Parsea todos los tipos de elementos: Text, BorderLine, DefaultLine, GridLine
  - Soporta posicionamiento de imágenes PNG y GIF
  - Extrae todos los 28+ parámetros (x, y, z, FontSize, FontFamily, Fill, MaxNum, etc.)
  - Preview visual con búsqueda automática de ruta de imagen
  - Exporta análisis a archivos de texto
- **Documentación Extendida**: Actualizado `docs/FORMATO_SETTING_TXT.md` con:
  - Todos los 60+ tipos de datos descubiertos (CPU, GPU, Memoria, Ventiladores, Fecha/Hora, Clima, etc.)
  - Todos los tipos de unidad (%, °, MHz, RPM, W, V, G, M)
  - Todas las 30+ familias de fuentes con conteos de uso
  - Barras de progreso segmentadas GridLine
  - Parámetro MaxNum para rangos de temperatura

### Modificado
- **Editor Visual**: Mejorado con 71 widgets (eran 49), cubriendo todas las convenciones de nombres de Mars Gaming y SOEYI.
- **Categorías**: Añadidas categorías Ventiladores, Clima, Etiquetas al selector de widgets.
- **Sistema de Preview**: Mejorada búsqueda de imagen de fondo para verificar múltiples rutas (carpeta JSON, source, Programme).

## [2.4.1] - 2025-12-13
### Añadido
- **Integración del Editor Visual**: Re-integrado editor visual v2 con funcionalidad de Arrastrar y Soltar y canvas en tiempo real.
- **Descarga Masiva Mejorada**: Reescrito descargador para obtener lista de archivos directamente de la API de HuggingFace, corrigiendo problemas de codificación.
- **Análisis de Temas**: Análisis comprensivo de 191 temas identificando 49 tipos de widgets, 39 fuentes y estructuras de animación.
- **Traducción al Inglés**: Auto-traducidos 10 temas con nombres en chino al inglés.
- **Verificación de Datos**: Verificados todos los 191 IDs de temas contra el sitio web oficial de SOEYI.

### Modificado
- **App Unificada**: Deshabilitados módulos v2 en EXE compilado para prevenir popups; versión script retiene todas las características.
- **Estructura del Proyecto**: Limpiados archivos temporales y organizados temas extraídos en `resources/extracted_themes`.

## [2.2.0] - 2025-12-13
### Corregido
- **Detección de ruta ultra-robusta**: Implementada cadena de estrategia de 6 fallbacks para corregir definitivamente el error "Path is null" en EXE compilado.
  - Estrategia 1: PSScriptRoot (para scripts .ps1)
  - Estrategia 2: MyInvocation.MyCommand.Path
  - Estrategia 3: Process.MainModule.FileName (para EXE)
  - Estrategia 4: Assembly.GetExecutingAssembly().Location
  - Estrategia 5: Environment.CurrentDirectory
  - Estrategia 6: Get-Location + fallback hardcodeado
- Añadida detección de ToolsPath null-safe con bloques try-catch.
- Búsqueda de carpeta de recursos ahora verifica múltiples ubicaciones.

## [2.1.3] - 2025-12-13
### Corregido
- Corregido error crítico "Path is null" en inicio añadiendo estrategias de detección de ruta de fallback.
- Eliminados popups molestos de debug (números 0-16) causados por adiciones de lista no suprimidas.
- Eliminado popup "Cancel" al salir de la aplicación.
- Deshabilitada salida de consola de debug en modo GUI.

## [2.1.2] - 2025-12-13
### Corregido
- Limpiadas funciones duplicadas en el código fuente.
- Añadida verificación robusta de null en `Get-ThemeThumbnail` para prevenir crashes al seleccionar temas con rutas faltantes.

## [2.1.1] - 2025-12-13
### Corregido
- Hotfix para error de sintaxis en lógica de detección automática de ruta.

## [2.1.0] - 2025-12-13

### Añadido
- **Aplicación Unificada**: `SnakeMarsTheme_Unified.exe` combina todas las herramientas en una sola interfaz.
- **Ejecutable Compilado**: Archivo .exe independiente para distribución más fácil sin necesitar conocimientos de PowerShell.
- **Migración del Editor**: Integrado el editor de temas visual WPF directamente en la app unificada.
- **Integración con HuggingFace**: Nueva pestaña "Descarga Masiva" para descargar el dataset completo de temas (1.68GB) o por ID.
- **Instalación Mejorada**:
  - Auto-detección de rutas de instalación de Mars Gaming y SOEYI.
  - Botones "Reiniciar App" para aplicar temas inmediatamente sin reiniciar PC.
  - Extracción automática con 7-Zip de archivos `.photo` encriptados.

### Modificado
- **Renovación de UI**: Tema oscuro moderno con 3 pestañas principales: Editor Local, Navegador Web, Descargador Masivo.
- **Sistema de Preview**: Ahora soporta miniaturas estáticas para todos los temas y previews animadas para temas WPF.
- **Detección de Ruta**: Mejorada lógica para soportar ejecución tanto como script (`src/`) como ejecutable (raíz).

### Corregido
- Corregidos errores de referencia null al hacer click en botones dinámicos en el editor.
- Corregida funcionalidad "Reiniciar" para esperar a que el proceso se cierre completamente antes de reiniciar.
- Resueltos problemas de solapamiento de UI en el dropdown de resolución.

## [2.0.0] - 2025-12-12
### Añadido
- Soporte inicial para temas Mars Gaming VMAX.
- Descubrimiento de contraseña de encriptación de temas.
- Script básico de instalador de temas.

## [1.0.0] - 2025-10
- Lanzamiento inicial para pantallas SOEYI.
