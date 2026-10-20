# UI Reference Research

Status: **DRAFT — requires Product Owner approval**

Prepared: 2026-10-20

Scope: Iteration 5, Academic Workspace & Library Management

## Purpose and guardrails

This research identifies interaction patterns worth testing for StudyAI Campus. It does not import another product's information architecture or visual identity. The Final PRD and Core Architecture v1.2 remain authoritative, especially for Personal Data Space ownership, canonical Knowledge Items, reusable placements, lifecycle, source provenance, and contextual AI.

Primary users are Indonesian university students working across lectures, assignments, research, reading, and writing. The first workspace must feel understandable to a new student while remaining efficient for daily, keyboard-heavy use.

## Evaluation summary

| Product | Strongest relevant lesson | What StudyAI should not copy |
|---|---|---|
| Linear | Compact navigation, personal shortcuts, saved views, keyboard-first operations | Issue-tracker vocabulary, extreme density, or status-board assumptions for academic content |
| Notion | Nested pages, breadcrumbs, collapsible sidebar, drag-and-drop hierarchy | Treating every object as an unrestricted page or weakening canonical Library and placement rules |
| Readwise Reader | Focused reading, keyboard navigation, highlights, tags, and margin notes | A reader-only IA that hides Courses, Projects, notes, and planning workflows |
| Capacities | Typed knowledge objects and reusable membership in collections | Replacing StudyAI's approved Knowledge Item and placement model with object-type jargon |
| Zotero | PDF annotations that retain page/source context and can become notes | Citation-manager complexity in the first Workspace slice |
| RemNote | Notes and study material remain connected; review obligations are visible | Making flashcards or spaced repetition the primary workspace metaphor |
| ChatGPT Study Mode | Guided, question-led assistance rather than immediate answer dumping | A permanent floating chatbot or AI that silently reads the whole library |

## Reference observations

### Linear

Observed in official documentation:

- Favorites are personal shortcuts shown in the sidebar and available through the command menu.
- Board and list layouts expose the same work through different representations, with filtering and grouping.
- Custom views save filters for repeated access.
- Team pages can surface pinned resources near the working context.

StudyAI application:

- Provide a small, personal **Pinned** section for frequently used courses, projects, folders, and notes.
- Let search and a command menu reach the same destinations as the sidebar.
- Preserve one canonical item and its placements; a pin is a shortcut, never a duplicate.
- Introduce alternate views only when the same academic data genuinely benefits from them. Folder/list view comes before board view.

Sources: [Favorites](https://linear.app/docs/favorites), [Board layout](https://linear.app/docs/board-layout), [Custom views](https://linear.app/docs/custom-views), [Team pages](https://linear.app/docs/default-team-pages).

### Notion

Observed in official documentation:

- Pages can be nested and shown as expandable sidebar trees.
- Breadcrumbs preserve location context within deep hierarchies.
- The sidebar can collapse; search is globally accessible with `Ctrl/Cmd+K`.
- Subpages can be created, moved, and reached from both content and navigation.

StudyAI application:

- Use a navigable tree for folders and nested pages, with clear expand/collapse state and breadcrumbs.
- Distinguish **item identity** from **placement/location** in labels and move flows.
- Never imply that moving one placement moves or deletes the canonical item everywhere.
- Constrain drag-and-drop with server-authoritative cycle and ownership validation.

Sources: [Navigate with the sidebar](https://www.notion.com/en-gb/help/navigate-with-the-sidebar), [Create a subpage](https://www.notion.com/en-gb/help/create-a-subpage), [Keyboard shortcuts](https://www.notion.com/en-gb/help/keyboard-shortcuts).

### Readwise Reader

Observed in official documentation:

- Reader is designed for keyboard-driven reading and exposes a command palette.
- Highlights, notes, and tags stay attached to reading material.
- PDF workflows keep the document central while tools remain secondary.

StudyAI application:

- Give documents a distraction-reduced reader with a retractable tool rail.
- Keep annotations visibly anchored to a source/version; do not present detached AI claims as evidence.
- Make shortcuts discoverable in menus and tooltips instead of requiring memorization.
- Defer advanced PDF annotation implementation until source-version and storage behavior are ready.

Sources: [Reader overview](https://docs.readwise.io/reader/docs), [Reader FAQ](https://docs.readwise.io/reader/docs/faqs), [Highlights, tags, and notes](https://docs.readwise.io/reader/docs/faqs/highlights-tags-notes), [Organizing content](https://docs.readwise.io/reader/docs/organizing-content), [PDFs](https://docs.readwise.io/reader/docs/faqs/pdfs).

### Capacities

Observed in official documentation:

- Content is organized as typed objects with properties.
- Collections can group an object without duplicating it.
- Object types and collections are presented as different concepts from folders.

StudyAI application:

- Show Knowledge Item type and essential metadata consistently in list rows and details.
- Explain reuse in user language: “Tambahkan ke lokasi lain,” not “duplicate.”
- Keep folders as spatial organization and metadata as descriptive organization.
- Avoid exposing backend vocabulary such as `data_space_id` or placement IDs.

Sources: [Content types](https://docs.capacities.io/reference/content-types), [Collections](https://docs.capacities.io/reference/collections), [Object types vs folders](https://docs.capacities.io/tutorials/object-types-vs-folders).

### Zotero, RemNote, and Study Mode

Observed in official documentation:

- Zotero can move annotations into notes while retaining links back to PDF pages and citation context.
- RemNote connects notes with flashcards and makes review counts visible in navigation.
- ChatGPT Study Mode uses interactive questions and stepwise guidance.

StudyAI application:

- Preserve evidence links when material moves from reading into notes or future writing workflows.
- Allow future Study & Learn obligations to appear contextually without taking over the Workspace navigation.
- AI Tutor entry points should be explicit actions such as **Jelaskan bagian ini**, **Buat pertanyaan latihan**, or **Tanya dari sumber terpilih**.
- Before any AI request, show or summarize the authorized source scope. Never default to the entire library.

Sources: [Zotero PDF reader](https://www.zotero.org/support/pdf_reader), [Zotero notes](https://www.zotero.org/support/notes), [RemNote school organization](https://help.remnote.com/en/articles/13770762-organizing-your-school-notes), [RemNote flashcard basics](https://help.remnote.com/en/articles/8663109-flashcard-basics), [ChatGPT Study Mode](https://help.openai.com/en/articles/11780217-study-mode).

## Cross-reference findings

### Information architecture and navigation

- Persistent global destinations: Home, Workspace, Study & Learn, Research, Create Studio, Planner.
- Workspace local destinations: Library, Courses, Projects, Trash.
- A contextual tree appears only within a selected Course, Project, or folder hierarchy.
- Search is global but clearly reports its current scope; users can widen or narrow it.
- Breadcrumbs communicate placement path. An item detail can separately expose “Also in” locations.

### Typography, density, and spacing

- Operational lists benefit from 36–44 px rows and strong alignment, not card grids.
- Reading and editing need a narrower measure, approximately 65–78 characters, with more line height.
- Metadata should be visually quieter but never too small; 12 px is the absolute floor, 14 px preferred.
- Density is contextual: compact navigation, comfortable forms, spacious reading.

### States and feedback

Every data surface must specify:

- loading or skeleton;
- first-use empty state with one concrete action;
- filtered empty state with a reset action;
- processing state for upload/indexing;
- success confirmation proportional to the action;
- recoverable partial failure;
- failed state with retry or support path;
- offline/reconnecting state where applicable;
- optimistic edit conflict with retained recoverable draft.

### Responsive behavior

- Desktop: global rail + workspace sidebar + content, with an optional details/AI panel.
- Tablet: global rail collapses; workspace sidebar becomes a drawer; content remains primary.
- Mobile web: one surface at a time, bottom destination bar for core modules, drill-in navigation for hierarchy, sticky context header, and bottom sheets for secondary actions.
- Do not shrink a three-column desktop workspace into three narrow mobile columns.

### Accessibility

- Target WCAG 2.2 AA and test keyboard, screen reader, zoom, reduced motion, high contrast, and touch behavior.
- Use native semantics first and visible focus indicators throughout.
- Tree, menu, dialog, tabs, combobox, and editor toolbars require documented keyboard patterns.
- Color never carries state alone; pair it with icon, text, or shape.
- Use Bahasa Indonesia by default while allowing content in any language and future locale expansion.

## Distinctiveness principles

StudyAI should feel like a calm academic working environment, not a marketing dashboard. Its distinctive elements should come from its real model:

- course and project context is always legible;
- source/version provenance is visible where it matters;
- reusable placement is explained without duplication anxiety;
- reading, note-taking, and study actions connect without becoming one overloaded screen;
- AI appears as a scoped academic tool, not a personality competing with the student's work.

## Decisions still required

1. Product Owner approval of the preferred visual direction in [DESIGN_DIRECTIONS.md](DESIGN_DIRECTIONS.md).
2. Exact behavior when removing/restoring a folder subtree, including mixed child ownership/lifecycle states (database review finding DBR-09).
3. V1 note editor format and supported import/export surface (OPEN-17).
4. Whether global search is included in the first implementation slice or follows core Library CRUD.
5. Which annotation behaviors, if any, enter Iteration 5 rather than a later document workflow.
