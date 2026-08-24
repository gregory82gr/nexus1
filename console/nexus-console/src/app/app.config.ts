import { ApplicationConfig, provideExperimentalZonelessChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';

import { routes } from './app.routes';

// Zoneless (Ch. 2): no zone.js in the served bundle, signals drive change
// detection directly. provideExperimentalZonelessChangeDetection() is the
// 18.2 API name for this — matches Angular's stated direction as of this
// version, per the book's own framing.
//
// withComponentInputBinding (Ch. 3): every route's `data` (title, chapter)
// binds directly onto PlaceholderComponent's own inputs — no resolver, no
// per-route wrapper component needed for the shared placeholder to read
// its route's own title/chapter.
// provideHttpClient (Ch. 5's own layer, used ahead of that chapter's full
// injected-config mechanism -- see core/api/reactor-fleet-api.ts's own doc
// comment for the named simplification): the typed client Plant Fleet
// calls the real Nexus1.Bff with.
export const appConfig: ApplicationConfig = {
  providers: [
    provideExperimentalZonelessChangeDetection(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(),
  ]
};
