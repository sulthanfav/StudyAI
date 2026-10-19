# StudyAI Specialized Review Roles

This directory defines lightweight review checklists and personas for evaluating changes against task requirements and StudyAI architectural invariants:

- [`software-architecture-reviewer.md`](software-architecture-reviewer.md): Clean Architecture boundaries, modular invariants, dependency directions, and domain purity.
- [`security-reviewer.md`](security-reviewer.md): Tenant/Data Space isolation, credential safety, log redaction, and input sanitization.
- [`code-quality-reviewer.md`](code-quality-reviewer.md): C# idiomatic patterns, test quality, async correctness, and performance.
- [`frontend-ui-reviewer.md`](frontend-ui-reviewer.md): UI/UX design quality, accessibility (WCAG AA), responsive styling, and client API contracts.

These review roles provide structured evaluation checklists. In single-agent environments, agents use these checklists before marking tasks complete.
