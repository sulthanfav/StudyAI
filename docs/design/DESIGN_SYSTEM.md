# StudyAI Design System Foundation

Status: **DRAFT — requires Product Owner approval**

Prepared: 2026-10-20

Candidate visual direction: **Ruang Studi**

## System goals

The design system must make academic work clear, safe, and efficient across web and a future React Native/Expo client. It must encode product meaning—ownership, location, lifecycle, provenance, save state, and AI source scope—without coupling backend contracts to CSS, browser components, or a specific UI library.

This document defines tokens and behavior, not an installed package stack. No frontend library is approved or installed by this draft.

## Proposed component foundation

### Recommendation

Use **shadcn/ui as a source-owned component starting point**, backed by accessible primitives such as Radix where a complex behavior needs them. Adopt individual components only when required; do not import a complete theme or generate the entire catalog.

Why this fits:

- shadcn/ui provides editable component source rather than a closed styling dependency.
- Radix covers difficult focus, keyboard, and ARIA behavior for common composites.
- Source ownership lets StudyAI encode its own density, terminology, states, and visual direction.
- A semantic token layer can be shared conceptually with React Native even though web and native implementations differ.

Constraints:

- Generated code is reviewed like first-party code and tested for the supported browser/assistive-technology matrix.
- Native HTML is preferred for simple controls; a primitive is not a substitute for semantic review.
- Avoid mixing Radix and React Aria for the same interaction class without a documented reason.
- Do not adopt third-party registries without source, license, dependency, and security review.
- Pin versions through the repository's chosen package manager and lockfile when implementation is approved.

Official references: [shadcn/ui introduction](https://ui.shadcn.com/docs), [official-source verification](https://ui.shadcn.com/docs/official), [Radix overview](https://www.radix-ui.com/primitives/docs/overview/introduction), [Radix accessibility](https://www.radix-ui.com/primitives/docs/overview/accessibility), [React Aria overview](https://react-spectrum.adobe.com/react-aria/getting-started.html).

## Token architecture

Use three layers:

1. **Primitive tokens**: raw palette, spacing, type sizes, radii.
2. **Semantic tokens**: `color.text.primary`, `color.surface.canvas`, `space.control.inline`.
3. **Component tokens** only when a component cannot express itself with semantic tokens.

Web CSS custom properties may map these names to kebab case. A future native theme may map the same semantic names to platform values. API payloads must never include presentation tokens.

### Semantic color preview

| Token | Light candidate | Dark candidate | Purpose |
|---|---|---|---|
| `color.surface.canvas` | `#F5F6F3` | `#101513` | Application background |
| `color.surface.primary` | `#FFFFFF` | `#171D1A` | Primary content surface |
| `color.surface.secondary` | `#EAEEEA` | `#202824` | Secondary grouping |
| `color.surface.selected` | `#DDECE7` | `#1E3A33` | Selected row/item |
| `color.text.primary` | `#17201F` | `#E9EEEA` | Main text |
| `color.text.secondary` | `#61706B` | `#A5B1AC` | Supporting text |
| `color.text.disabled` | `#8B9692` | `#74807B` | Disabled text; validate per surface |
| `color.action.primary` | `#176B5B` | `#5FC3A8` | Primary action and active context |
| `color.focus` | `#2F66D0` | `#86A9F4` | Focus ring and keyboard location |
| `color.status.info` | `#2F66D0` | `#86A9F4` | Informational state |
| `color.status.success` | `#24734D` | `#68C894` | Completed/saved state |
| `color.status.warning` | `#936516` | `#E3BA62` | Attention/recoverable state |
| `color.status.danger` | `#B6403A` | `#FF8C84` | Destructive/error state |
| `color.border.default` | `#D4DAD6` | `#34403A` | Standard boundary |
| `color.border.strong` | `#9BA7A2` | `#5A6861` | Emphasized boundary |

All foreground/background combinations require automated and manual contrast validation. Status colors never carry meaning without text or an icon.

### Typography

| Token | Size/line height | Weight | Use |
|---|---|---|---|
| `type.display` | 32/40 | 650 | Rare product-level heading |
| `type.h1` | 28/36 | 650 | Route title |
| `type.h2` | 22/30 | 650 | Section title |
| `type.h3` | 18/26 | 600 | Subsection/panel title |
| `type.body` | 16/24 | 400 | Default reading and forms |
| `type.body.compact` | 14/20 | 400 | Navigation and dense lists |
| `type.label` | 14/20 | 600 | Labels and controls |
| `type.meta` | 12/16 | 500 | Secondary metadata; never long-form text |

- UI family candidate: Source Sans 3 with robust Latin coverage.
- Reading family candidate: Literata, only for student-authored long-form reading/preview.
- Preserve browser zoom and user font settings; prefer `rem` for web sizing.
- Keep long-form measure between approximately 65 and 78 characters.

### Spacing

Use a 4 px base with named semantic applications.

| Primitive | Value | Typical use |
|---|---:|---|
| `space.0` | 0 | Reset |
| `space.1` | 4 px | Icon/text micro-gap |
| `space.2` | 8 px | Compact control gap |
| `space.3` | 12 px | Row padding |
| `space.4` | 16 px | Standard content gap |
| `space.5` | 20 px | Form groups |
| `space.6` | 24 px | Section spacing |
| `space.8` | 32 px | Major content grouping |
| `space.10` | 40 px | Page rhythm |
| `space.12` | 48 px | Major separation |

Density changes component padding, not the meaning of spacing primitives. Minimum touch target is 44 by 44 CSS pixels where touch is expected.

### Radius, border, and elevation

- `radius.control`: 4 px.
- `radius.panel`: 8 px.
- `radius.dialog`: 12 px.
- `radius.pill`: 999 px only for tags, status, or binary segmented controls.
- Default border: 1 px semantic border.
- Focus ring: 2 px outer ring with sufficient separation from the control edge.
- Shadows appear only on temporary elevation such as menus, dialogs, and dragged items.
- Nested surfaces should prefer spacing, divider, and background shift over stacked shadows.

### Breakpoints

Breakpoints describe layout capacity, not device identity:

| Name | Candidate threshold | Expected change |
|---|---:|---|
| `compact` | under 640 px | One surface at a time; bottom module navigation |
| `medium` | 640–1023 px | Drawer sidebar; simplified metadata |
| `wide` | 1024–1439 px | Persistent navigation and workspace sidebar |
| `xwide` | 1440 px and above | Optional context/evidence panel; bounded reading measure |

Components must also respond to container width where useful. Test 320 px width, 200% zoom, and landscape mobile explicitly.

## Core component inventory

### Foundation

- Button, icon button, link, separator, badge/status, avatar.
- Input, textarea, checkbox, radio group, switch, select, combobox.
- Tooltip, popover, dropdown/context menu, dialog, alert dialog, sheet/drawer.
- Toast/live region, inline alert, progress, skeleton, spinner.
- Tabs, accordion, collapsible, scroll area, resizable panel only if required.

### StudyAI navigation

- App shell and global module rail.
- Workspace sidebar and content-space switcher.
- Hierarchy tree with keyboard navigation and async children.
- Breadcrumb with overflow handling.
- Command/search palette with scope indicator.
- Pinned shortcuts and recent items.
- Mobile module bar and hierarchy picker.

### Library and document

- Item row/list, metadata cell, type marker, placement indicator.
- Folder row, tree node, move destination picker, sort/filter controls.
- Upload queue and processing state.
- Document reader shell and source-version indicator.
- Note editor shell, title field, block/content region, save state, version history entry.
- Placement list (“Also in”), lifecycle banner, restore/purge confirmation.

### AI and provenance

- Contextual AI action menu.
- Source-scope summary and source selector.
- Citation/evidence link.
- AI job status with queued, running, partial, failed, retryable, and completed states.
- Never introduce a global floating AI bubble as the default entry point.

## Keyboard interaction principles

- Tab order follows visual and task order; no positive `tabindex`.
- Visible focus is never removed. Focus returns to the logical trigger after closing overlays.
- `Ctrl/Cmd+K` opens search/command access; `/` may focus search only when not editing text.
- `Enter` activates or opens; `Space` selects/toggles where the pattern expects it.
- Tree: arrows navigate/expand/collapse, Home/End move within the tree, typeahead when feasible.
- Menus, tabs, toolbars, comboboxes, and dialogs follow established WAI-ARIA patterns.
- Shortcuts have menu/tooltips, must avoid browser/OS conflicts, and need remapping strategy before broad expansion.
- Destructive actions must not be single-key shortcuts.

## Forms and feedback

- Labels remain visible; placeholders are examples, not labels.
- Validate on submit by default, then on correction; avoid punishing validation while typing.
- Place field errors next to the field and focus an error summary for complex forms.
- Primary actions use specific verbs: **Buat folder**, **Simpan perubahan**, **Pindahkan**, **Pulihkan**.
- Disable only when the reason is obvious; otherwise allow action and explain what is missing.
- Preserve entered data after validation, network, and conflict errors.
- Destructive confirmations name the affected object and explain placement versus canonical-item impact.

## Loading, empty, error, and conflict patterns

| State | Required behavior |
|---|---|
| Initial loading | Preserve page geometry; use a restrained skeleton for predictable content |
| Background refresh | Keep current content; show quiet progress without blocking interaction |
| First-use empty | Explain the space and offer one primary next action |
| Filtered empty | State that no result matches; offer clear/reset filters |
| Processing | Show stage when known, safe navigation behavior, and retry/cancel policy |
| Partial success | Identify completed and failed items separately |
| Error | State what failed, whether work was retained, and the next safe action |
| Offline/reconnecting | Keep local draft when supported and make sync status persistent |
| Note conflict | Retain the user's draft, show server/current version, and offer review/copy/merge flow; never silently overwrite |

## Theming and native portability

- Theme selection supports system, light, and dark.
- Semantic color names must not include `light`, `dark`, or literal palette names.
- Business state remains typed data (`processing`, `trashed`, `conflict`), not CSS class names.
- Navigation and command concepts should be shared across clients; interaction components remain platform-native.
- Do not share DOM components with React Native. Share TypeScript domain/view-model contracts only when platform-neutral and approved.
- Motion, spacing, type, and color scales can be represented in a platform-neutral token package later; do not create it before both clients need it.

## Initial adoption sequence

1. Approve a visual direction and tokens.
2. Create an OpenSpec change for the first frontend implementation slice.
3. Scaffold the approved Next.js baseline and add only foundational dependencies.
4. Implement tokens, typography, focus, and app shell before feature components.
5. Add Library list/tree, folder creation, note shell, and global/scoped search in dependency order.
6. Verify keyboard, screen reader, responsive, contrast, loading/error/conflict, and tenant-safe API behavior.
7. Expand the component inventory only from real feature demand.
