# Frontend Execution Plan

## Architecture
- **Stack**: Vite + React + TypeScript for fast DX, Axios for HTTP, React Router for multi-view navigation, Zustand-like context via custom hooks for auth state, and CSS modules + global utility classes for styling.
- **State**: Auth context keeps JWT access + refresh tokens, decodes role/id information, and exposes helpers for login/logout/token refresh. Domain data (listings, buildings, viewings, etc.) fetched on-demand and cached per view using React Query–style hooks (implemented manually with `useEffect`).
- **API client**: Centralized Axios instance with base URL from `VITE_API_BASE_URL`, automatic Authorization header injection, 401 interceptor that uses refresh token endpoint.

## UI Zones (Lithuanian copy)
1. **"Atrask NT" landing** – hero banner, search/filter controls, listing cards (calls `GET /api/Listings` when role allows; otherwise shows friendly onboarding message), open-house schedule preview using `/api/Viewings`.
2. **"Pirkėjams" workspace** – identity verification widget (calls `GET/POST /api/Confirmations` scoped by buyer), contact reveal callouts tied to listing selection, private viewing booking form (provisions hooking into `/api/Viewings` + `/api/Availabilities`).
3. **"Brokeriams" valdymo skydelis** – tabs for Buildings, Apartments, Listings, Nuotraukos, and Lankstūs laikai. Each tab features data tables + side-drawer forms hitting the respective controllers (GET/POST/PUT/DELETE) with inline validation and localized feedback.
4. **"Administratoriui" priežiūra** – user directory table (GET `/api/Users`), broker approval toggles (PATCH `/api/Brokers/{id}`), buyer verification toggles (PATCH `/api/Buyers/{id}`), listing moderation actions (DELETE `/api/Listings/{id}`).

## Responsiveness & Polish
- Primary layout uses CSS grid with breakpoint at 960px; below that, navigation collapses into a slide-out drawer with floating action button. Card decks switch from 3-column to single column. Animations via `transition` and `box-shadow` to satisfy "pretty" requirement.
- Palette: gradient hero (deep violet → warm orange), neutral surfaces for cards, accent buttons (`#f97316`). Custom icons via `react-icons`.

## Data Contracts
- TypeScript interfaces mirror backend DTOs (camelCase). Date inputs converted to `yyyy-MM-dd HH:mm` format before POST/PUT to satisfy server validations.
- File uploads for pictures stubbed with URL entry (since API expects `Picture` entities referenced by ID).

## Docker & Dev Experience
- `frontend/` project with Vite. Dockerfile multi-stage builds static assets served by Nginx. Compose services: `frontend` depends_on API, shares `bridged` network, exposes port 4173→80, build arg `VITE_API_BASE_URL=http://ntskelbimusistemasaitynai:8080` for in-cluster calls.

## Stretch Enhancements (time permitting)
- Persisted filters in `localStorage` for visitors.
- Skeleton loaders and optimistic UI for broker actions.
- Inline timeline view for broker viewing approvals.
