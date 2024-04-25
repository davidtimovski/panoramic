# Panoramic

A productivity app that revolves around a high-level view of notes, links, and checklists.

Organized as a grid system, it allows you to add widgets in a layout that you prefer.

![Preview (Dark)](design/preview-dark.webp)

![Preview (Light)](design/preview-light.webp)

---

## Widgets

Currently supports 4 types of widgets.

### Note

Textual widget that supports simplified markup. 

### Link collection

A collection of links ordered to your preference.

### Recent links

Links that were recently clicked within the app. 

### Checklist

A collection of tasks (to-dos).

---

## Storage

I wanted to make the app easy to onboard to, but I also wanted to make it easy to off-board from. All of the data is stored in markdown files in the folder specified in Preferences, so if you decide to dip out it should be fairly easy to do so without losing your data to a proprietary format.

An example of how the markdown files are formatted can be seen in the [MarkdownSamples folder](/test/Benchmarks/MarkdownSamples).

---

## Hotkeys

### General

| Hotkey | Command |
|-|-|-|
| Ctrl+S | Filters the list of links in Link Collection and Recent Links widgets and tasks in the Checklist widgets |

### Note widget

| Hotkey | Command | Caveat |
|-|-|-|
| Ctrl+N | Opens a dialog for creating a new note | Only works when you have a single Note widget |

### Checklist widget

| Hotkey | Command | Caveat |
|-|-|-|
| Ctrl+T | Opens a dialog for creating a new task | Only works when you have a single Checklist widget |

---

## License

This project is licensed under the GNU GPLv3 License - see the [LICENSE](LICENSE) file for details.
