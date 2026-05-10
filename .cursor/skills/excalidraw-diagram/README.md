# Excalidraw Diagram Skill

A Cursor skill that generates Excalidraw `.excalidraw` JSON files following a consistent design methodology — diagrams that **argue visually**, not just display information.

Source: https://github.com/coleam00/excalidraw-diagram-skill

## Usage

Ask Cursor to create a diagram, e.g.:

> "Create an Excalidraw class diagram for this project."

Open the generated `.excalidraw` file in Cursor with the Excalidraw VS Code extension installed.

## Customize Colors

Edit `references/color-palette.md` to change the brand palette. Everything else in the skill is universal design methodology.

## Structure

```
excalidraw-diagram/
  SKILL.md                        # Design methodology + workflow
  README.md                       # This file
  references/
    color-palette.md              # Brand colors (edit to customize)
    element-templates.md          # JSON templates for each element type
```
