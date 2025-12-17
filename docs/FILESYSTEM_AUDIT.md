# Auditoría de Sistema de Archivos - SnakeMarsTheme v1.0.2

Esta auditoría describe el comportamiento FINAL implementado en la aplicación respecto a la gestión de archivos y directorios, cumpliendo con el **Protocolo Omega** de limpieza y segregación de datos.

## 1. Arquitectura de Rutas (PathService)

La aplicación utiliza un sistema híbrido **Portable/Instalable** gestionado por `PathService`. Detecta automáticamente el modo de ejecución basándose en permisos de escritura.

### Rutas Base

| Concepto | Variable | Modo Instalado (System) | Modo Portable (USB/User) |
|----------|----------|-------------------------|--------------------------|
| **AppDir** (Lectura) | `PathService.AppDir` | `C:\Program Files\SnakeMarsTheme` | `X:\SnakeMarsTheme` |
| **UserDataDir** (Escritura) | `PathService.UserDataDir` | `C:\Users\Usuario\Documents\SnakeMarsTheme` | `X:\SnakeMarsTheme` |

*Nota: En modo portable, `AppDir` y `UserDataDir` son la misma carpeta.*

## 2. Mapa de Directorios Creados

### En Carpeta de Instalación (`AppDir`)
Estos archivos son INMUTABLES y vienen con el instalador. La aplicación NUNCA escribe aquí en modo instalado.
- `/SnakeMarsTheme.exe` (Ejecutable)
- `/resources/catalog.json` (Catálogo Offline)
- `/resources/previews/` (Imágenes stock)
- `/resources/themes/` (Temas stock preinstalados)

### En Carpeta de Usuario (`UserDataDir`)
Estos directorios se crean SOLO cuando es necesario (Lazy Creation).
- `/logs/`
  - `error.log` (Registro de errores graves. Se recicla/append).
- `/resources/ThemesPhoto/`
  - Descargas crudas `.photo` (Legacy).
- `/resources/Themes_SMTHEME/`
  - Descargas crudas `.smtheme`.
- `/resources/themes/`
  - Temas instalados y extraídos listos para usar.

## 3. Flujo de Datos

1. **Descarga**:
   - `DownloadService` lee catálogo de `AppDir/resources/catalog.json`.
   - Descarga archivos a `UserDataDir/resources/ThemesPhoto` (o `_SMTHEME`).
   - NUNCA ensucia `Program Files`.

2. **Instalación (Extracción)**:
   - `ExtractionService` toma el zip desde `UserDataDir`.
   - Extrae el contenido en `UserDataDir/resources/themes/{ThemeName}`.
   - `ThemeService` carga temas combinando `AppDir` (stock) y `UserDataDir` (usuario).

3. **Logs**:
   - `App.xaml.cs` captura excepciones y escribe en `UserDataDir/logs/error.log`.

## 4. Desinstalación

- **Modo Instalado**: El desinstalador elimina `AppDir` (Program Files). La carpeta `UserDataDir` (Documents) se MANTIENE para preservar los temas y descargas del usuario (Standard Windows Behavior).
- **Modo Portable**: Al borrar la carpeta raíz, se borra todo (ya que todo está contenido ahí).

## 5. Garantía de Limpieza

- No se crean archivos en `%AppData%`, `%Temp%`, ni Registro de Windows (salvo install paths estándar).
- El usuario tiene control total sobre sus datos en la carpeta `Documents`.
