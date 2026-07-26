# Grill With Docs Documentation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Update the repository guidance for `grill-with-docs`, create a domain-only `CONTEXT.md`, and record the four accepted architecture decisions.

**Architecture:** Keep agent workflow rules in `AGENTS.md`, ubiquitous language in `CONTEXT.md`, and hard-to-reverse architectural choices in ADRs. Provide both a combined `ADR.md` review copy and canonical sequential files under `docs/adr/`.

**Tech Stack:** Markdown, ASP.NET Core 10, Clean Architecture, Codex agent skills.

## Global Constraints

- Preserve the existing AGENTS.md architecture, security, CodeGraph, and verification rules.
- `CONTEXT.md` must contain no implementation details.
- ADRs must be concise and limited to already-confirmed decisions.
- Use Traditional Chinese prose and retain canonical English domain terms.

---

### Task 1: Update AGENTS.md

**Files:**
- Modify: `AGENTS.md`

- [x] Preserve sections 1–17.
- [x] Add domain documentation paths to the repository structure.
- [x] Require relevant tasks to read `CONTEXT.md` and ADRs.
- [x] Replace `grill-me` guidance with `grill-with-docs` workflow and documentation boundaries.

### Task 2: Create CONTEXT.md

**Files:**
- Create: `CONTEXT.md`

- [x] Define the Northwind context in one paragraph.
- [x] Define canonical identity, party, catalog, sales, geography, and audit terms.
- [x] Add `_Avoid_` aliases where ambiguity is likely.
- [x] Exclude framework, persistence, deployment, and agent workflow details.

### Task 3: Create ADRs

**Files:**
- Create: `ADR.md`
- Create: `docs/adr/0001-use-aspnet-core-mvc-for-web-presentation.md`
- Create: `docs/adr/0002-do-not-use-dotnet-aspire.md`
- Create: `docs/adr/0003-use-mapster-for-object-mapping.md`
- Create: `docs/adr/0004-keep-data-protection-in-web-boundary.md`

- [x] Record each decision, context, rationale, and important consequences.
- [x] Mark all four decisions as accepted.
- [x] Keep the canonical ADR files individually readable.

### Task 4: Verify artifacts

**Files:**
- Verify all generated Markdown files.

- [x] Check that all expected files exist and are non-empty.
- [x] Check headings, fenced blocks, and relative links.
- [x] Search `CONTEXT.md` for prohibited implementation terms.
- [x] Package the deliverables into a ZIP archive.
