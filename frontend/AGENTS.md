# Frontend

Mandatory patterns for `frontend/`. The root `AGENTS.md` holds the project context and
the rules that apply to both tiers; this file adds the frontend-specific ones. If a
pattern you are about to write is not described here, stop and ask.

The guiding constraint: students are frontend novices who know HTML, CSS, JavaScript
and the basics of React, and nothing about Angular. Favour code they can read over code
that is clever. Do not mirror the backend layering — there are no invariants to protect
and no persistence to abstract. The architecture is three rules inside a module
(use-case groups, pages and presentational components, queries in the page and commands
in the group's service) plus one rule between modules: they never import from each
other. Nothing more.

## Shape

Single `ng new` workspace on the current Angular major (v22 at the time of writing).
One application. No Nx, no libraries, no `NgModule`, no component library. Standalone
components only, zoneless, `OnPush` by default (both are the `ng new` defaults; do not
opt out of either).

```
frontend/src/
  styles.scss                @use of the four partials below, nothing else   [platform]
  styles/
    _tokens.scss             colours, spacing, radius, type as CSS custom properties
    _base.scss               reset, body, headings, links
    _layout.scss             page container, stack, grid helpers
    _components.scss         global classes: button, card, form field, table
  app/
    core/                    auth (state, interceptor, login, register), home,
                             app.routes.ts                                    [platform]
    shared/
      util/
      api/                   shared types (PageResult, ProblemDetails,
                             ImageReferenceDto), mirror the server, no
                             client-only fields                                [platform]
      image-uploading/       image upload, see "Shared image upload" below     [platform]
        image-upload/        presentational section: thumbnails, picker, errors
        image-uploading.ts   service: upload, delete, deleteAll
    modules/
      <name>/
        api/                 this module's DTO types, mirror the server, no client-only fields
        tour-browsing/       one folder per use-case group, named as on the backend
          tour-list/         a page: named in the routes file, declares its resource
            tour-list.ts
            tour-list.html
            tour-list.scss
          tour-card/         a presentational component, used by the pages next to it
        tour-authoring/
          create-tour/
          my-tours/
          tour-authoring.ts  the group's service: its commands, nothing else
        <name>.routes.ts     the module's routes, lazy-loaded by core/app.routes.ts; the
                             only module file imported from outside the module
```

A module is a flat list of use-case groups. A group is a flat list of component
folders plus at most one service file. That is the whole layout, and it follows the
2025 Angular style guide: organise by feature, never by type, so there are no
`pages/`, `components/` or `services/` folders. Three rules place any file:

1. A group is the set of screens serving one user goal. Reuse the backend's
   application-layer group names (`blog-reading`, `blog-authoring`). A component that
   only appears inside another group's screen lives with that screen; the frontend has
   fewer groups than the backend because it groups screens, not operations. A page's
   commands go into its own group's service even when the backend serves them from a
   different group.
2. Every component gets its own folder inside its group, exactly what
   `ng generate component` produces: `tour-list.ts`, `tour-list.html`,
   `tour-list.scss`, no `.component` suffix. Generate with `ng g c tour-browsing/tour-list`
   from the module folder. A client-only type is exported from the file of the
   component whose input or output uses it; other components import it from there.
3. Queries live in the page as an `httpResource`; commands live in the group's
   service. A page that runs a command reloads its own resource afterwards.

The smart/dumb split is visible in the code rather than in folder names: the routes
file lists every page, a page may inject services, a presentational component injects
nothing.

## Rules

- A module may import from its own folder, from `shared/`, and from `core/auth` (the
  current user). Nothing else, and never anything from another module.
- Enforced by `no-restricted-imports` in `eslint.config.js` (`angular-eslint`, flat
  config), one block per module whose regex names the sibling modules. It runs locally with `npm run lint` and in CI on every push to
  `main` or `development` and every pull request. This is a lint rule, not a compiler guarantee. Unlike
  the backend, nothing structurally prevents a violation.
- Routes are lazy-loaded per module with `loadChildren` from `core/app.routes.ts`. That
  file is platform-owned and set up once.
- A module uses another module in exactly two ways. To show the other module's screen,
  navigate to its route with `routerLink`; a URL is plain text, so nothing is imported.
  To show the other module's data, the module's own backend composes it through
  `Contracts` and returns it in the module's own DTO, so the page reads only its own
  module's endpoints, and the page's DTO is mirrored in the module's own `api/`. There
  is no third way: a module never uses another module's component or type, never calls
  another module's endpoints, and never shares state with it.
- A module's route paths are its only surface toward other modules. Renaming or
  removing a path another module links to is a cross-team change; flag it.
- The one legitimately shared state is the current user (identity, roles) in `core/`.
  Modules read it; only the platform team writes it.

## The Angular subset

Students learn exactly this subset and the code uses nothing outside it. Adding a
construct means adding a lesson, so treat it as a platform decision.

- Components: standalone, `inject()` for dependencies (never constructor injection),
  `input()` / `output()` signals (never decorators), `host` property (never
  `@HostBinding`), `protected` members read by the template, `readonly` on
  framework-initialised properties.
- Templates: built-in control flow (`@if`, `@for`, `@switch`), `class` and `style`
  bindings. No `*ngIf`, `*ngFor`, `NgClass`, `NgStyle`. No template reference
  variables (`#name`); every value the user types goes through Signal Forms.
- State: `signal()` and `computed()`. `effect()` only when a signal must
  drive something outside the component tree, and it needs a reason. Page-scoped state, including its read resources, lives in the page.
  State that outlives a page lives in a service holding signals, `providedIn: 'root'`.
- HTTP: `httpResource()` in the page for reads (it carries loading and error state as
  signals, and a fresh one is created on every visit); `HttpClient` with
  `firstValueFrom` in the group's service for writes. Functional interceptors
  (`HttpInterceptorFn`, `withInterceptors`) in `core/`.
- Forms: Signal Forms (`form()`, `FormField`, validators from
  `@angular/forms/signals`). No Reactive Forms, no template-driven forms. A file input
  is not typed text: the component reads `files` from the `(change)` event's target.
- Leaving a page: `inject(DestroyRef).onDestroy(...)` and a `host` listener
  `'(window:pagehide)'`, only to delete images the page uploaded and did not save.
- Routing: `router-outlet`, `routerLink`, `loadChildren`, route parameters bound as
  signal inputs via `withComponentInputBinding`; `inject(Router)` only for navigation
  after a command.
- No RxJS in student code. `Observable` appears only as the return type of `HttpClient`
  writes, immediately awaited. No NgRx, no facades.

## Styling

No component library. Styling is plain SCSS, deliberately minimal: elegant defaults in
lean files that students extend, not a design system. The four global partials in
`src/styles/` are platform-owned and short. Theming means editing `_tokens.scss` and
nothing else; every colour, spacing and font value elsewhere refers to a custom property
defined there.

Where a style goes, in order:

1. A token value changes: `_tokens.scss`.
2. A class used by more than one module: `_components.scss` or `_layout.scss`. Adding a
   global class is shared code and follows the same promotion rule as `shared/`.
3. Everything else: the component's own `.scss`, under default (emulated) encapsulation.
   Never `::ng-deep`.

Do not hand-roll a design system, do not add utility-class frameworks, do not introduce
`shared/ui` wrappers unprompted. Visual consistency across modules is an open problem
assigned to the platform team, and misalignment is an accepted learning experience.

## Conventions

- In feature modules, DTO types mirror the backend's Application DTOs one to one: one
  file per module in `modules/<name>/api/`, shared envelope types in `shared/api/`, so
  the import boundary covers types as well. Client-only types follow rule 2 above.
  There is no `models/` folder; a client-only type with no single owning component is
  a case to raise, not a reason to invent one. There is no generator. The change that alters a DTO on the
  backend also updates the mirrored type in the same commit, and nothing else ever
  touches these files: no client-only fields, no renames, no extra types. Mapping
  rules: enum to a union of string literals, `Guid` and `DateTime` to `string`,
  nullable to `| null`, nested DTO to a nested interface, collection to an array.
  A field the client does not use is still mirrored.
- A page component owns the interaction: it declares its read resource, injects its
  group's service for commands, and passes plain values down to presentational
  components, which take `input()`s and raise `output()`s and inject nothing.
- An image is stored by a module as its reference string, `/api/shared/images/{fileName}`,
  and shown with `<img [src]="reference">`.
- Do not produce Vitest tests unless specifically instructed. Frontend testing is not
  taught; it is a platform-team assignment.

## Shared image upload

`shared/image-uploading/` is the one way a module form accepts images. The section
`<app-image-upload>` is presentational: inputs `images` (`UploadedImage`: reference and
`fileKey`, the key of the file it was uploaded from, or `null` for an image of saved
content), `pending` (`File[]` being uploaded), `errors` (`ImageUploadError`: file name and
reason) and `maxImages`; outputs `filesSelected` (`File[]`) and `removeImage` (a
reference). It rejects files over 5 MB and files beyond `maxImages` itself and lists them
with the other errors, so those never reach the page. It skips, silently, a file already
in `images` or `pending` (same name, size and last change, see the exported `fileKey`).
`ImageUploading` is a plain command service:
`upload(file)` returns the reference, `delete(reference)`, and `deleteAll(references, keepalive)`,
which fires one delete per reference without waiting and ignores failures, for leaving the
page; with `keepalive` the request survives a closed tab (needs `withFetch()`, set in
`app.config.ts`).

The page that embeds the section wires it:

- It injects `ImageUploading` and keeps `images`, `pending` and `errors` in its own signals,
  plus the list of references uploaded from this form (on a create form, that is `images`).
- Errors describe the latest action only: the section clears its own rejections on every
  selection and removal, and the page empties `errors` at the start of both handlers.
- On `filesSelected`, for each file: add the file to `pending`, `await upload(file)`, then
  remove that file from `pending` and add `{ reference, fileKey: fileKey(file) }` to
  `images`, or on failure add `{ fileName, reason: serverMessage(failure, ...) }` to
  `errors`. Images of already saved content enter `images` with `fileKey: null`.
- On `removeImage`: `await delete(reference)`, then remove it from `images`; on failure add
  an error and keep the thumbnail. An image the form did not upload is only removed from
  the list.
- The save button is disabled while `pending` is not empty. After a successful save the
  page empties the uploaded list, then navigates.
- `inject(DestroyRef).onDestroy(...)` calls `deleteAll(uploaded, false)`; "Cancel" only
  navigates away. Closing or reloading the tab does not destroy the page, so the page's
  `host` listens to `'(window:pagehide)': 'onPageHide()'`, which calls
  `deleteAll(uploaded, true)`.
