import { Routes } from '@angular/router';
import { CreateTour } from './tour-authoring/create-tour/create-tour';
import { MyTours } from './tour-authoring/my-tours/my-tours';
import { TourKeyPoints } from './tour-authoring/tour-key-points/tour-key-points';
import { TourList } from './tour-browsing/tour-list/tour-list';
import { TourPreview } from './tour-browsing/tour-preview/tour-preview';
import { MyPreferences } from './tour-preference-editing/my-preferences/my-preferences';

export const explorationRoutes: Routes = [
  { path: '', component: TourList },
  { path: 'mine', component: MyTours },
  { path: 'mine/:id/key-points', component: TourKeyPoints },
  { path: 'create', component: CreateTour },
  { path: 'preferences', component: MyPreferences },
  { path: 'tours/:id', component: TourPreview },
];
