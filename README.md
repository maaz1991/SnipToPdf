# SnipToPdf

**SnipToPdf** is a tiny native Windows utility that lets you grab a screen
rectangle with **Ctrl + Shift + S** and instantly save it as a PDF.

| Feature | Notes |
|---------|-------|
| 🖼️  Screen capture | Drag any area on any monitor, multi-monitor friendly |
| 📄  PDF output    | Names files `YYYY-MM-DD_X.pdf`, saved to your chosen folder |
| 🔍  Searchable text | Built-in OCR (Tesseract 5) embeds an invisible text layer |
| ⚙️  Settings      | Change output folder any time |
| 🖱️  Tray mode     | App stays in the system-tray; Exit via tray or **Exit** button |
| 🖼️  Open-Folder   | Confirmation dialog has *Open Folder* to reveal the new PDF |

---

## Hot-key

| Action | Keys |
|--------|------|
| Start snip | **Ctrl + Shift + S** |

---

## Building from source

### Prerequisites

* .NET 9 SDK  
* **Git** + VS Code (optional)  
* `tessdata/eng.traineddata` (already included)

```bash
git clone https://github.com/maaz1991/SnipToPdf.git
cd SnipToPdf
dotnet build -c Release

Developed & Authored by Maaz Siddiqui

