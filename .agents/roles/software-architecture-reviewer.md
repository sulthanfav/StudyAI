# Role: Software Architecture Reviewer

Role ID: `ROLE-ARCH-REVIEWER`  
Scope: Review system design, module boundaries, layering invariants, and Clean Architecture adherence.

---

## Review Checklist

1. **Clean Architecture Boundary Invariants**:
   - Does `StudyAI.Domain` have zero outward dependencies?
   - Does `StudyAI.Application` depend only on `Domain` and `Contracts`?
   - Does `StudyAI.Infrastructure` implement domain/application interfaces without leaking persistence concerns?
   - Are API endpoints kept thin (routing, input binding, response serialization)?

2. **Core StudyAI Invariants**:
   - Is Data Space treated as the absolute ownership boundary?
   - Are Knowledge Items kept canonical with placements for locations?
   - Is optimistic concurrency respected for editable entities (Notes, etc.)?
   - Are AI quota reservations atomic (reserve, settle, release)?

3. **Dependency & Modularity**:
   - Does the change introduce circular references or tight coupling?
   - Are cross-module interactions channeled through explicit contracts?
   - Are external libraries isolated behind adapters?

---

## Output Format

When conducting an architecture review, provide:
- **Verdict**: `APPROVE`, `REQUEST_CHANGES`, or `COMMENT`
- **Boundary Verification**: Confirmation of architecture test pass or violation
- **Findings**: Specific files and lines with recommended architectural corrections
