# EZSee — Project / Feature List (Read-only, Fast-start Image Viewer)

Goal
- Build a fast-start, lightweight image viewer optimized to launch quickly and remain strictly non-destructive: the app does not modify user files (no tags, ratings, metadata edits, renames, deletes, or any operations that persist changes to the original files).

Core principles
- Launch in <1s on typical desktop hardware
- Minimal memory footprint; lazy-load images and thumbnails
- Responsive UI with keyboard + mouse-first navigation

Default behavior
- Default startup opens a single-image view (focused on the last opened file or a user-selected default).
- Press Enter to switch from single-image view to folder view (thumbnail grid). Press Enter again to return to single-image view.

Viewing & browsing
- Single-image view (default start):
  - Fit-to-window, fill, and 100% / actual pixel modes
  - Smooth/continuous zoom (mouse wheel + gestures) and pan
  - Full-screen mode (toggle) and distraction-free viewing
  - Image info overlay (filename, dimensions, basic EXIF read-only display)
- Thumbnail grid / folder view (accessible via Enter):
  - Fast thumbnail generation with lazy loading and persistent cache
  - Configurable thumbnail density and grid size
- Slideshow mode with configurable interval and simple transitions

Supported formats
- Common raster: JPEG, PNG, GIF, BMP, TIFF, WebP, HEIF/HEIC (where platform support exists)
- RAW viewing support via optional decoder library or OS-provided codecs (deferred/optional module)
- Animated GIF and APNG playback

Navigation & UI
- Keyboard navigation: ← / → (previous/next), PgUp / PgDn, Home / End, Space (advance / start slideshow), Esc (exit full-screen or cancel)
- Enter: toggle between single-image view and folder (thumbnail) view
- Configurable shortcuts (keyboard and mouse)
- Breadcrumb path and folder tree for navigation in folder view
- Recent folders and files list (read-only history)

Mouse shortcuts (defaults — user-configurable)
- Left-click: focus (single-click does not change image); single-click in thumbnail grid opens the image in single-image view
- Double-left-click (single-image view): toggle full-screen
- Right-click: open context menu (view-only actions: Reveal in OS, Copy path, Properties,...)
- Mouse wheel: zoom in/out (when cursor is over image); in folder view, wheel scrolls thumbnail grid
- Ctrl + mouse wheel: change thumbnail grid density in folder view
- Click-drag (left button): pan when zoomed in
- Shift + click-drag: fine pan (slower)
- Ctrl + double-click: fit to window

Keyboard shortcuts (suggested defaults)
- ← / → : previous / next image
- Enter : toggle single-image view <-> folder (thumbnail) view
- Space : play/pause slideshow
- F : toggle full-screen
- Esc : exit full-screen / close dialogs
- Ctrl+O : open folder or file
- + / - or Ctrl+mouse wheel : zoom in/out

Thumbnails, caching & performance
- On-demand decode, with prefetching of next/previous x images for low-latency navigation
- Optional hardware-accelerated decode where available
- Low memory footprint; decode limits and configurable threads for thumbnail generation

Metadata & file info
- Read-only EXIF/IPTC/XMP display (camera, lens, date/time, basic tags)
- No in-app metadata editing (to prevent accidental modification)

Integration & extensibility
- Plugin system limited to read-only extensions (for example: additional thumbnail decoders, extra file format viewers, UI panels that do not perform file writes).

Safety & privacy
- Read-only by default — app never writes to user images or metadata.
- No built-in telemetry by default; opt-in only with clear privacy policy.

UI & UX
- Compact toolbar and minimalist mode for faster startup
- Theme support (light/dark) and high-DPI scaling
- Touch-friendly gestures for zoom/pan where applicable

Accessibility & localization
- Keyboard-navigable UI, screen-reader friendly labels, and multi-language support

Settings & preferences
- Portable mode option (store config next to binary)
- Session restore limited to viewer window layout and last-opened file/folder
- Configurable thumbnail cache limits, decode thread count, and default start mode (single-image view by default)


Stack: 
- .NET Core 10 WPF
- Github action for CI/CD
