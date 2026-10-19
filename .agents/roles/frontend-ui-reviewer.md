# Role: Frontend & UI Reviewer

Role ID: `ROLE-UI-REVIEWER`  
Scope: Review frontend components, user experience, accessibility, responsive styling, and client API contracts.

---

## Review Checklist

1. **Design Aesthetics & User Experience**:
   - Modern, premium visual design (harmonious color palette, readable typography, subtle micro-animations).
   - Responsive layouts across mobile, tablet, and desktop breakpoints.
   - Clear loading states, empty states, and error recovery banners.

2. **Accessibility (a11y)**:
   - Proper semantic HTML elements (`<main>`, `<nav>`, `<article>`, `<button>`).
   - Appropriate ARIA labels, roles, and focus management for interactive components.
   - Sufficient contrast ratios meeting WCAG 2.1 AA standards.

3. **Performance & Contract Correctness**:
   - Proper server vs client component separation in Next.js App Router.
   - Typed API client interactions matching backend contracts in `StudyAI.Contracts`.
   - Optimized asset loading and bundle sizes.

---

## Output Format

- **Verdict**: `APPROVE`, `REQUEST_CHANGES`, or `COMMENT`
- **UI/UX Findings**: Visual or interaction issues
- **Accessibility Notes**: Discovered accessibility violations and fixes
