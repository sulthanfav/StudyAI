# StudyAI Design Directions

Status: **DRAFT — requires Product Owner approval**

Prepared: 2026-10-20

## Shared product principles

All three directions preserve the same interaction model:

- Workspace is a place to organize and work, not a generic analytics dashboard.
- Lists, trees, and document surfaces carry most information; cards are reserved for true grouped summaries.
- AI is contextual, source-scoped, and opened by a deliberate action.
- Desktop favors efficient scanning; mobile favors a clear drill-in flow.
- Light and dark themes use semantic tokens, not separate component designs.
- Motion explains state change and never becomes ambient decoration.

## Direction A — Ruang Studi (recommended)

### Philosophy

A calm, precise digital study room: contemporary Indonesian campus life without nostalgic school motifs. The memorable element is a narrow “study spine” that combines current course/project context, breadcrumb, save state, and source scope. Everything else stays quiet.

### Product rationale

Students regularly switch between organizing, reading, and writing. Ruang Studi changes density by activity while keeping the current academic context stable. It gives StudyAI an identity beyond a generic SaaS dashboard without making the editor ornamental.

### Palette

| Role | Light | Dark |
|---|---|---|
| Canvas | `#F5F6F3` | `#101513` |
| Surface | `#FFFFFF` | `#171D1A` |
| Ink | `#17201F` | `#E9EEEA` |
| Muted ink | `#61706B` | `#A5B1AC` |
| Academic green | `#176B5B` | `#5FC3A8` |
| Focus blue | `#2F66D0` | `#86A9F4` |
| Critical | `#B6403A` | `#FF8C84` |

Contrast must be measured against final pairings; these values are direction tokens, not an untested accessibility claim.

### Typography

- UI, navigation, and data: **Source Sans 3**, variable where practical.
- Reading and student-authored long-form preview: **Literata**, opt-in by content surface only.
- Use sentence case. Tabular numerals for counts and timestamps. No decorative monospace labels.

### Layout and density

- Desktop: 64 px global rail, 248 px workspace sidebar, fluid content, optional 320 px contextual panel.
- Top study spine is 48 px and shows location, sync state, and contextual actions.
- Navigation/list rows are compact (40 px); forms are comfortable; editor measure is 720–780 px.
- Sidebar uses hierarchy and separators rather than containers around every section.

### Shape, icons, and motion

- Radius scale: 4 px controls, 8 px popovers/panels, 12 px only for large modal surfaces.
- Thin borders and tonal layers; shadows only for temporary elevation.
- Lucide icons at consistent 1.75–2 px stroke, with labels for unfamiliar actions.
- Motion: 120 ms press/focus, 180 ms menu/panel, 240 ms route/context change; transform/opacity only where appropriate; reduced-motion path required.

### Responsive behavior

- Tablet turns the workspace sidebar into a drawer while preserving the study spine.
- Mobile uses a bottom module bar, a location header, and full-screen hierarchy picker.
- Editor actions collapse into a bottom sheet; save/conflict state remains visible.

### Strengths and risks

- Strength: balanced for organization, reading, and future modules.
- Strength: academic character comes from context and typography, not decoration.
- Risk: two type families require performance discipline and a clear rule limiting serif use.

## Direction B — Campus Grid

### Philosophy

A crisp academic operations console inspired by timetables, lab notebooks, and structured course administration. It emphasizes speed, alignment, and predictable keyboard workflows.

### Product rationale

This direction fits users managing many courses, documents, and deadlines. It makes metadata and batch operations exceptionally scannable, but reading surfaces must deliberately soften its operational density.

### Palette

| Role | Light | Dark |
|---|---|---|
| Canvas | `#F4F7FA` | `#0E141B` |
| Surface | `#FFFFFF` | `#151E28` |
| Ink | `#152233` | `#EAF0F7` |
| Muted ink | `#637286` | `#9DABBA` |
| Campus blue | `#2457A7` | `#78A8F5` |
| Marker yellow | `#B57B00` | `#F0C35C` |
| Critical | `#B42335` | `#FF8794` |

### Typography

- **IBM Plex Sans** across application surfaces.
- **IBM Plex Mono** only for identifiers, citation keys, and technical evidence—not routine metadata.
- Strong numeric alignment and a compact type scale.

### Layout and density

- Desktop: 232 px sidebar plus command/search header; list-first content with resizable metadata columns.
- Topbar carries global search, current scope, quick create, and account menu.
- 36–40 px list rows; 8 px base spacing; larger reading mode opens as a focused route.
- Dividers and alignment create structure; cards are rare.

### Shape, icons, and motion

- 3–6 px radii, crisp 1 px borders, almost no shadow.
- Lucide icons with a slightly smaller optical size; text labels remain primary.
- Fast 100–180 ms transitions; no entrance choreography.

### Responsive behavior

- Tablet swaps table columns for prioritized row metadata.
- Mobile becomes a stacked list with filter/sort bottom sheets and explicit selection mode.
- Keyboard affordances become touch actions without hiding essential operations behind hover.

### Strengths and risks

- Strength: fastest direction for power users and large libraries.
- Strength: straightforward to implement consistently.
- Risk: can feel administrative or intimidating to first-semester students.
- Risk: reading and reflective study require a deliberately calmer sub-layout.

## Direction C — Evidence Atelier

### Philosophy

A research-and-writing studio where sources, annotations, and emerging ideas are visibly related. It is more editorial and expressive, using asymmetry selectively around documents and evidence.

### Product rationale

This direction strongly anticipates Academic Research, Writing, and citation-rich AI workflows. It makes provenance emotionally and visually important, but may be premature for a Library-first iteration.

### Palette

| Role | Light | Dark |
|---|---|---|
| Canvas | `#F7F5F0` | `#171411` |
| Surface | `#FFFEFA` | `#201C18` |
| Ink | `#251F1A` | `#F1ECE5` |
| Muted ink | `#756B61` | `#B8ACA0` |
| Aubergine | `#68415F` | `#C893B9` |
| Evidence ochre | `#9B681D` | `#E0B263` |
| Link blue | `#315E91` | `#8DB7E8` |

### Typography

- UI and controls: **Manrope**.
- Reading, notes, and citations: **Source Serif 4**.
- Wider hierarchy contrast, but headings remain sentence case and left-aligned.

### Layout and density

- Desktop: 72 px global rail, 272 px source navigator, centered document canvas, collapsible evidence rail.
- Topbar is minimal; document identity and provenance lead.
- Comfortable 44 px navigation rows and generous 12 px base rhythm in reading surfaces.
- Metadata appears as aligned marginalia, not scattered chips.

### Shape, icons, and motion

- 2–4 px document edges, 10 px dialogs, deliberate squared source panels.
- Phosphor icons could provide a more editorial character, but this adds a separate dependency decision.
- 160–260 ms transitions emphasize opening a source, revealing evidence, or returning to context.

### Responsive behavior

- Tablet overlays the evidence rail; source and note never compete side-by-side below 1024 px.
- Mobile uses a reading-first route with tabs for source, note, and evidence.
- Annotation actions use a contextual selection toolbar that remains keyboard accessible.

### Strengths and risks

- Strength: most distinctive and strongest fit for future Research/Writing modules.
- Strength: provenance becomes a visible product value.
- Risk: greater responsive and editor complexity.
- Risk: can overstate capabilities before annotation and research flows exist.

## Comparison

| Criterion | Ruang Studi | Campus Grid | Evidence Atelier |
|---|---:|---:|---:|
| First-use clarity | High | Medium | Medium |
| Large-library efficiency | High | Very high | Medium |
| Reading/writing comfort | Very high | Medium | Very high |
| Iteration 5 implementation fit | Very high | High | Medium |
| Future nine-module flexibility | Very high | High | High |
| Distinctiveness without ornament | High | Medium | Very high |
| Responsive complexity | Medium | Low–medium | High |

## Recommendation

Approve **Ruang Studi** as the foundation, borrowing Campus Grid's compact list behavior and Evidence Atelier's provenance treatment where the product genuinely supports it.

The recommendation is not a visual compromise. Its defining choice is stable academic context across organizing, reading, and editing modes. It is the best fit for Iteration 5 while remaining credible for Research, Writing, Study & Learn, and future native mobile surfaces.

## Product Owner approval checklist

- [ ] Approve Ruang Studi, Campus Grid, or Evidence Atelier as the primary direction.
- [ ] Approve typography families and external font delivery approach.
- [ ] Approve light theme first with dark theme parity in the same milestone, or explicitly sequence them.
- [ ] Approve the global navigation labels and whether Bahasa Indonesia ships as the default UI language.
- [ ] Approve whether a right contextual panel is part of the first slice or deferred.
- [ ] Confirm whether iconography should standardize on Lucide before dependency installation.
