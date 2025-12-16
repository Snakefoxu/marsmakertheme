# 🎯 Guía Definitiva: Crear Temas para Mars Gaming VMAX

## ✅ Descubrimientos del 2025-12-12

### Tres Métodos para Crear Temas

| Método | Type | Animación | Widgets | Setting.txt |
|--------|------|-----------|---------|-------------|
| **DIY Style** | 0 | GIF directo | DisplayTexts | ❌ No |
| **Earth Style** | 1 | Frames PNG | Overlay Mars | ❌ No |
| **Setting.txt** | 1 | Frames + Imágenes | Custom en Setting | ✅ Sí |

---

## 🔥 MÉTODO RECOMENDADO: Setting.txt

Este método permite **control total** sobre frames + widgets.

### Archivos necesarios:
```
Programme/MiTema/
├── Setting.txt      # 👈 CLAVE - posiciones de todo
├── back.png         # Fondo
├── demo.png         # Preview
├── 1.png, 2.png...  # Frames o imágenes decorativas
```

### Ejemplo Setting.txt:
```
name:MiTema
width:360
height:960
back.png:x@0,y@0,z@0
1.png:x@100,y@400,z@1
Text:x@20,y@50,z@2,FontSize@28,FontFamily@#Impact,Foreground@#00ffff,data@CpuUsage,unit@%
Text:x@20,y@100,z@2,FontSize@28,FontFamily@#Impact,Foreground@#ff00ff,data@GPUT,unit@°C
```

### Tipos de datos disponibles:
- `CpuUsage`, `CpuFrequency`, `CPUT`
- `GpuUsage`, `GpuFrequency`, `GPUT`
- `MemoryUseInt`, `MemoryFrequency`
- `Fan1`, `Fan2`, `Fan3`, `Fan4`
- `CurrentTimeShut`

📚 Ver documentación completa: [FORMATO_SETTING_TXT.md](./FORMATO_SETTING_TXT.md)

---

## 🔵 Tipo 0: Estilo DIY (GIF + Widgets)

**Uso**: Temas rápidos con GIF animado y widgets simples.

```json
{
  "Type": 0,
  "DisplayTexts": [{"TextType": "CPUTemp", "Left": 20, "Top": 60, ...}],
  "DisplayImages": [{"Image": "C:\\...\\animacion.gif"}]
}
```

---

## 🟢 Tipo 1: Estilo Earth (Solo Animación)

**Uso**: Temas con animación pero widgets de Mars Gaming.

- JSON vacío (`DisplayTexts: []`)
- Frames pequeños (326x326) para zona animada
- Widgets son overlay hardcoded de Mars Gaming

---

## 📋 Checklist para Crear Tema

### Con Setting.txt (Recomendado):
1. [ ] Crear carpeta en `Programme/MiTema/`
2. [ ] Crear `Setting.txt` con posiciones
3. [ ] Agregar `back.png` y `demo.png`
4. [ ] Agregar imágenes/frames
5. [ ] Crear JSON básico (`Type: 1`) en `ThemeScheme/`
6. [ ] Copiar JSON a subcarpeta de dispositivo

### Con GIF (Rápido):
1. [ ] GIF de 360x960
2. [ ] JSON con `Type: 0`
3. [ ] Widgets en `DisplayTexts`

---

## ⚠️ Errores Comunes

| Error | Causa | Solución |
|-------|-------|----------|
| Setting.txt not found | Type:1 con DisplayTexts pero sin Setting.txt | Crear Setting.txt o usar Type:0 |
| No widgets | JSON vacío sin Setting.txt | Crear Setting.txt |
| Widgets mal posicionados | Coordenadas incorrectas | Ajustar x@ y y@ |

---

© 2025 SnakeFoxu - SnakeMarsTheme

