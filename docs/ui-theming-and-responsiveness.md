# UI Theming and Responsiveness

How colour and layout work in `UI/FrontendWebassembly`. Read this before adding a
stylesheet, adding a screen, or changing anything about dark mode.

## Design tokens

`wwwroot/css/theme.css` is **the only place a colour literal is allowed to live.** It
declares ~75 `--c-*` tokens under `:root` and redefines them under `html.dark`. Every
other sheet reads the tokens, so a screen goes dark for free.

It is linked first in `index.html`, before `app.css` and `ats.css`, so later sheets can
reference the tokens.

### Adding colour to a screen

Use a token directly:

```css
.my-card {
    background: var(--c-surface);
    color: var(--c-fg-strong);
    border: 1px solid var(--c-border);
}
```

Do **not** introduce a new hex value. If nothing fits, add a token to `theme.css` with
both a light and a dark value, and say in a comment what it is for.

### The alias pattern

Older sheets keep their own private token names (`--au-text-1`, `--management-text-1`,
`--users-border`, …) and re-point them at the shared set:

```css
.au-shell {
    --au-text-1: var(--c-fg-strong);
    --au-border: var(--c-border);
}
```

This exists because token **names collide across features** — `--navy-900` meant
`#0b1b3d` in ATS but `#0a1c3f` in PlatformLogs, and `--border` had four different
values. Hoisting the names to `:root` would have silently changed colours. Re-pointing
values per scope avoids that, and leaves the hundreds of `var(--au-text-1)` call sites
untouched. Keep this pattern when touching an existing sheet; use `--c-*` directly in
new ones.

### Tokens that must NOT invert

Some surfaces are dark in both themes and always carry white text: the ATS sidebar,
dialog headers, primary buttons, avatars. They must not be built from the brand ramp,
because the ramp inverts under `html.dark` and would turn them light with white text on
a pale background. Use these instead:

| Token | Use |
|---|---|
| `--c-on-dark-fg` | text/icons on an always-dark surface |
| `--c-sidebar-gradient` | ATS console sidebar |
| `--c-dialog-header-gradient` | Add/Edit dialog headers |
| `--c-primary-gradient` | primary buttons |
| `--c-avatar-gradient` | initials avatars |
| `--c-chip-solid-bg` | solid filled pills ("+3 more") |
| `--c-logo-*` | ATS sidebar logo facets |

`--c-navy-900` is the opposite case: it is mostly used as *text* on light cards, so it
deliberately becomes a light tint in dark mode.

### Colours that are not CSS

Chart series palettes are C# strings passed to MudBlazor chart options, so tokens can't
reach them. `ATSDashboardComponent.razor.cs` and `DashboardChartDetailComponent.razor.cs`
each hold a light and a dark array selected off `ThemeService.IsDarkMode`. Keep the two
arrays the same length and order — series identity is positional — and keep the two
files in sync, since the dialog is the zoomed view of the same charts.

### Bulk conversion helper

`Tools/tokenize-css.sh` replays the literal → token mapping over a stylesheet. Useful
for a sheet added later or missed. It refuses to run on `theme.css` (rewriting literals
there makes every token define itself and blanks the palette).

```bash
Tools/tokenize-css.sh path/to/Feature.razor.css
```

Anything it leaves behind needs a human decision — typically white that is *text on a
dark gradient* (`--c-on-dark-fg`) rather than a *surface* (`--c-surface`).

## Dark mode

### How the flag flows

1. `index.html` runs an inline script before Blazor boots, reads `isDarkMode` from
   localStorage and adds `dark` to `<html>`. This is the anti-FOUC guard — without it a
   dark user sees a white flash. (`dark-startup` is also written, and still matched by
   `theme.css`, only so cached copies of an older `index.html` keep working.)
2. `SharedService/ThemeService.cs` owns the flag at runtime: `IsDarkMode`,
   `ToggleAsync()`, and an `OnChanged` event. It also owns the single `MudTheme` for
   the whole app, so the MudBlazor palette and the CSS tokens stay in step.
3. Layouts call `Theme.InitializeAsync()` once and subscribe to `OnChanged`.
4. The preference survives logout — both `MainLayout.OnInitializedAsync` and
   `AuthService.LogoutAsync` read it back after `ClearAsync()`.

`ThemeService` is registered in `ServiceConfig/FrontendServiceConfig.cs`.

### Which layouts are themed

| Layout | Themed | Hosts |
|---|---|---|
| `ConsoleLayout` | **yes** | ATS console (via `ATSLayout`), staff Employment Verification, auth screens, `/access-denied` |
| `MainLayout` | **yes** | OnePlatform home, AI chat, PlatformLogs, user management, PhilSys |
| `SSOLayout` | **yes** | `/sso/frontpage` |
| `GenericLayout` | **no — deliberately** | external token-link pages |

**`GenericLayout` is intentionally left light and must stay that way.** It hosts pages
reached by an emailed token link — the candidate application form,
employment-verification verify/reject, PhilSys liveness — plus the public API docs.
A staff member who toggles dark would otherwise hand a dark-themed branded page to an
external recipient in the same browser. `ConsoleLayout` is a themed sibling of
`GenericLayout` rather than a change to it, so that separation is structural.

If you add a page, pick the layout by audience: staff → `ConsoleLayout`, external →
`GenericLayout`.

### Toggle

Two entry points, one shared state: the OnePlatform topbar (`MainLayout.razor`) and the
ATS console topbar (`ATSLayout.razor`). Both call `ThemeService.ToggleAsync()`.

## Responsiveness

### Breakpoints

Match MudBlazor's ladder so CSS and the C# `Breakpoint` enum agree:

| Width | Meaning |
|---|---|
| 600px | MudBlazor `Xs` — phone; toolbars stack, dialog footers go full width |
| 960px | MudBlazor `Sm` — **MudTable card mode**; ATS sidebar goes off-canvas |
| 1280px | MudBlazor `Md` |

**`Breakpoint.Sm` card mode fires at 960px, not 600px** (`Xs` is the 600px one;
verified in `MudBlazor.min.css`). Several older comments in this repo claimed 600px and
were wrong — that mismatch is what left the 601–960px band rendering stacked cards
inside a still-880px-wide table. Don't reintroduce a private breakpoint; the set above
was consolidated from 26 distinct values.

`ATSLayout.MobileBreakpoint` (C#) must stay at 960 to match the sidebar CSS.

### Table width

**`TableComponent` owns table width.** It applies a `min-width` only *above* the
card-mode breakpoint:

```css
@media (min-width: 961px) {
    .responsive-table-container ::deep .mud-table-root { min-width: 55rem; }
}
```

Do not add a `min-width` to a table root in a feature stylesheet. Screens used to do
this (880/920/940/1100/1120px) and it was only neutralised inside the 960px query, so
the 601–960px band broke and any screen opting out of card mode kept the floor at every
width.

Related rules that must not come back:
- No `overflow-x: hidden` on `.mud-table-container` — paired with a surviving
  `min-width` it *clips* content unreachably instead of letting the user scroll.
- No global `.mud-table-cell { min-width: 150px !important }` — that put a ~1050px
  floor under every table in the app.
- Column `min-width`s belong inside a `@media (min-width: 961px)` block.

Every `MudTd` needs a `DataLabel` — card mode uses it as the row label.

### Checklist for a new screen

- [ ] Colours come from `--c-*` tokens; no new hex literals.
- [ ] Always-dark surfaces use the `--c-on-dark-*` / gradient tokens, not the ramp.
- [ ] Breakpoints are 600 / 960 / 1280.
- [ ] No `min-width` on a table root.
- [ ] Every `MudTd` has a `DataLabel`.
- [ ] Flex rows that hold buttons have `flex-wrap: wrap`.
- [ ] Cells that can hold long user input have `min-width: 0` and `overflow-wrap: anywhere`.
- [ ] Checked at 390px in both themes, and in the 601–960px band.

### Verifying

```powershell
dotnet build UI/FrontendWebassembly/FrontendWebassembly.csproj
dotnet run --project UI/FrontendWebassembly   # http://localhost:5134
```

Confirm every referenced token exists (should print nothing):

```bash
cd UI/FrontendWebassembly
grep -oE "^\s*--c-[a-z0-9-]+" wwwroot/css/theme.css | tr -d ' ' | sort -u > /tmp/d
grep -rhoE "var\(--c-[a-z0-9-]+" --include=*.css --include=*.razor . | sed 's/var(//' | sort -u > /tmp/u
comm -13 /tmp/d /tmp/u
```

And that no sheet defines a token as itself (a sign `tokenize-css.sh` was run over a
token source):

```bash
grep -rnE "^\s*(--[a-z0-9-]+): *var\(\1\)" --include=*.css .
```
