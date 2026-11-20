import { Injectable, inject, signal } from '@angular/core';
import { ActivatedRouteSnapshot, NavigationEnd, Router } from '@angular/router';
import { Auth } from '../../core/auth/auth';
import { environment } from '../../../environments/environment';

const CONSENT_KEY = 'explorer.analytics-consent';

export type ConsentChoice = 'accepted' | 'rejected' | null;

// The parameters every hit carries. Nothing else is sent with a page view.
interface CommonParameters {
  role: string;
  page_location: string;
  page_referrer: string;
}

function readChoice(): ConsentChoice {
  try {
    const stored = localStorage.getItem(CONSENT_KEY);
    return stored === 'accepted' || stored === 'rejected' ? stored : null;
  } catch {
    return null;
  }
}

function writeChoice(choice: 'accepted' | 'rejected'): boolean {
  try {
    localStorage.setItem(CONSENT_KEY, choice);
    return true;
  } catch {
    return false;
  }
}

// The route template, e.g. /payment/wallets/:id, built from the matched route configs.
function routeTemplate(root: ActivatedRouteSnapshot): string {
  const segments: string[] = [];
  let route: ActivatedRouteSnapshot | null = root;
  while (route !== null) {
    const path = route.routeConfig?.path;
    if (path) {
      segments.push(path);
    }
    route = route.firstChild;
  }
  return '/' + segments.join('/');
}

// Deletes _ga and _ga_* on the host and on every dot-prefixed parent domain.
function deleteGaCookies(): void {
  try {
    const expired = 'expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/';
    const parts = location.hostname.split('.');
    for (const cookie of document.cookie.split(';')) {
      const name = cookie.split('=')[0].trim();
      if (name !== '_ga' && !name.startsWith('_ga_')) {
        continue;
      }
      document.cookie = `${name}=; ${expired}`;
      for (let i = 0; i < parts.length; i++) {
        document.cookie = `${name}=; ${expired}; domain=.${parts.slice(i).join('.')}`;
      }
    }
  } catch {
    // Deleting cookies is best effort and never breaks the app.
  }
}

@Injectable({ providedIn: 'root' })
export class Analytics {
  private readonly router = inject(Router);
  private readonly auth = inject(Auth);
  private readonly measurementId = environment.analyticsMeasurementId;
  private readonly page = window as unknown as Record<string, unknown>;
  private scriptLoaded = false;

  private readonly consent = signal<ConsentChoice>(readChoice());
  readonly choice = this.consent.asReadonly();

  constructor() {
    if (this.consent() === 'accepted') {
      this.start();
    }
    // Platform exception: the only RxJS subscription in the app, see frontend/AGENTS.md.
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.send('page_view', {});
      }
    });
  }

  accept(): void {
    this.consent.set(writeChoice('accepted') ? 'accepted' : null);
    if (this.consent() === 'accepted') {
      this.start();
    } else {
      this.stop();
    }
  }

  reject(): void {
    this.consent.set(writeChoice('rejected') ? 'rejected' : null);
    this.stop();
  }

  // The event list. Every event a module may send is one method here; see README.md.

  tourPublished(tourId: string): void {
    this.send('tour_published', { tour_id: tourId });
  }

  // Funnel step 2. The product belongs to the offer, not to one user (P11).
  productAddedToCart(productId: string): void {
    this.send('product_added_to_cart', { product_id: productId });
  }

  // Funnel step 3. No amount or balance is sent (P12).
  cartPurchased(): void {
    this.send('cart_purchased', {});
  }

  tourStarted(tourId: string): void {
    this.send('tour_started', { tour_id: tourId });
  }

  tourCompleted(tourId: string): void {
    this.send('tour_completed', { tour_id: tourId });
  }

  // One event for every kind of challenge; the kind tells them apart in GA4.
  challengeSolved(challengeId: string, challengeType: 'quiz' | 'location' | 'social'): void {
    this.send('challenge_solved', { challenge_id: challengeId, challenge_type: challengeType });
  }

  // GA4 itself. Nothing below runs without consent and a measurement ID.

  private isActive(): boolean {
    return this.consent() === 'accepted' && this.measurementId !== '';
  }

  private start(): void {
    if (!this.isActive()) {
      return;
    }
    try {
      this.page[`ga-disable-${this.measurementId}`] = false;
      if (!this.scriptLoaded) {
        this.page['dataLayer'] = this.page['dataLayer'] ?? [];
        const script = document.createElement('script');
        script.async = true;
        script.src = `https://www.googletagmanager.com/gtag/js?id=${this.measurementId}`;
        document.head.appendChild(script);
        this.scriptLoaded = true;
        this.gtag('js', new Date());
      }
      this.gtag('config', this.measurementId, {
        send_page_view: false,
        ...this.commonParameters(),
      });
    } catch {
      // A failure to load GA4 is silent.
    }
  }

  private stop(): void {
    if (this.measurementId === '') {
      return;
    }
    this.page[`ga-disable-${this.measurementId}`] = true;
    deleteGaCookies();
  }

  private send(name: string, parameters: Record<string, string>): void {
    if (!this.isActive()) {
      return;
    }
    try {
      const common = this.commonParameters();
      // Overrides the real URL that gtag would otherwise attach to every later hit.
      this.gtag('set', { page_location: common.page_location, page_referrer: '' });
      this.gtag('event', name, { ...parameters, ...common });
    } catch {
      // A failed send is silent.
    }
  }

  private commonParameters(): CommonParameters {
    const user = this.auth.user();
    const role = user === null ? 'guest' : [...user.roles].sort().join(',');
    const template = routeTemplate(this.router.routerState.snapshot.root);
    return { role, page_location: location.origin + template, page_referrer: '' };
  }

  // gtag.js reads the `arguments` object itself from dataLayer, not an array, so the
  // parameter only types the call and the body pushes `arguments`.
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  private gtag(...args: unknown[]): void {
    // eslint-disable-next-line prefer-rest-params
    (this.page['dataLayer'] as unknown[]).push(arguments);
  }
}
