# Jason-hub

> Personal Monorepo — Central hub & sub-project entry point

A personal project aggregation site built with [Astro 6](./Portfolio). The main page showcases a personal profile and project cards that link to independent sub-projects.

## Structure

```
Jason-hub/
├── Portfolio/     ← Astro 6 personal homepage (main site)
├── ...            ← Upcoming sub-projects (Vue 3 + TS)
├── AGENTS.md      ← AI agent instructions
├── ARCHITECTURE.md ← Architecture design
└── STYLE_GUIDE.md ← Coding standards
```

## Port Convention

| Type | Range |
|------|-------|
| Frontend | 8000–8049 |
| Backend API | 8050–8099 |

Portfolio runs on port **8000**.

## Branching & Versioning

- **main** — production-ready at all times
- **project/\<name\>** — sub-project development branches
- Versioning: `v1.0.0` → `v1.1.0` → `v1.1.1` ...

See [STYLE_GUIDE.md](./STYLE_GUIDE.md#git-分支与版本规范) for details.

## Quick Start

```bash
# Enter the main site
cd Portfolio

# Install dependencies
npm install

# Start dev server (opens browser automatically)
npm run dev

# Build static site
npm run build
```

---

See also [AGENTS.md](./AGENTS.md) · [ARCHITECTURE.md](./ARCHITECTURE.md) · [STYLE_GUIDE.md](./STYLE_GUIDE.md)
