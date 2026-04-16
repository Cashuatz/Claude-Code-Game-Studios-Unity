# Editor Window UI Code Standards

<!-- Applies to: **/Editor/** paths containing EditorWindow, PropertyDrawer, Inspector code -->

This rule applies to all Unity Editor UI code using UI Toolkit (VisualElement).
Refer to `.claude/skills/editor-layout/SKILL.md` for the complete guide.

## Key Rules

### Color Palette (6 standardized colors)
- `windowBackground`: (0.22, 0.22, 0.22) — window background
- `panelBackground`: (0.235, 0.235, 0.235) — panel interior
- `panelBorderColor`: (0.17, 0.17, 0.17) — borders
- `headerBackground`: (0.235, 0.235, 0.235) — header areas
- `accentColor`: (0.36, 0.36, 0.36) — accent bars
- `subtleTextColor`: (0.78, 0.78, 0.78) — body text
- `Color.white` — important text only

### Section Structure
- Always use `CreateSectionShell(title, badge, out body)` pattern
- Header height: 40f, Foldout toggle height: 26f
- All borders: 1f (never vary)

### Spacing Rules
- Section body padding: left/right 14f, top 12f, bottom 14f
- Foldout content padding: left/right 12f, top 10f, bottom 12f
- Section margin-bottom: 10f
- Foldout margin-bottom: 8f

### Font Sizes
- Header: 12, Body: 11, Small: 10, Badge: 9

### Input Fields
- Labels: fixed width (120), flexShrink = 0
- Fields: flexGrow = 1

### Emphasis Boxes
- Left border accent (2f width) with semantic colors:
  - Gray (0.5, 0.5, 0.5) — general
  - Blue (0.4, 0.6, 1.0) — info
  - Orange (1.0, 0.6, 0.2) — warning
  - Red (1.0, 0.3, 0.3) — error

### Forbidden
- Never hard-code color values inline (use variables)
- Never vary border width (always 1f)
- Never omit padding/margins
- Never use inconsistent font sizes
