# Portfolio

> Jason-hub Main Site · Personal Homepage

A minimal personal homepage built with **Astro 6**, aggregating personal profile and project cards as the unified entry point for all Jason-hub sub-projects.

## Tech Stack

|  |  |
|------|------|
| Framework | Astro 6 |
| Styling | Vanilla CSS + CSS Custom Properties |
| Design | Minimal & clean · Blue-white palette |
| Responsive | Mobile-first (3 breakpoints) |

## Page Sections

- **Header** — Logo, navigation
- **Hero** — Avatar, name, tagline, bio, social links
- **ProjectGrid** — Data-driven project card grid
- **Footer** — Copyright & credits

## Data Source

Project card data lives in `src/data/projects.json`. Add a new entry whenever a new sub-project is created.

## Directory Structure

```
Portfolio/
├── public/images/          ← Static assets (avatar, logo, thumbnails)
├── src/
│   ├── pages/
│   │   └── index.astro     ← Single page entry
│   ├── layouts/
│   │   └── BaseLayout.astro
│   ├── components/
│   │   ├── Header.astro
│   │   ├── Hero.astro
│   │   ├── ProjectCard.astro
│   │   ├── ProjectGrid.astro
│   │   └── Footer.astro
│   ├── data/
│   │   └── projects.json
│   └── styles/
│       └── global.css
├── astro.config.mjs
├── tsconfig.json
└── package.json
```

## Local Development

```bash
npm install
npm run dev    # → http://localhost:8000
npm run build  # Build static output to dist/
```

## Git Branching

Portfolio is the main site and is developed directly on `main`. For larger changes, use a `feat/` branch:

```bash
git checkout -b feat/xxx
# After development
git checkout main
git merge feat/xxx
git branch -d feat/xxx
```

Versioning follows the root repo's semantic versioning. See [STYLE_GUIDE.md](../STYLE_GUIDE.md#git-分支与版本规范) for details.
