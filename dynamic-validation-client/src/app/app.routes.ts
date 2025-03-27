import { Routes } from '@angular/router';
import { ModelListComponent } from './components/model-list/model-list.component';
import { ModelFormComponent } from './components/model-form/model-form.component';
import { ModelDetailComponent } from './components/model-detail/model-detail.component';
import { ValidationTestComponent } from './components/validation-test/validation-test.component';

export const routes: Routes = [
  { path: '', redirectTo: 'models', pathMatch: 'full' },
  { path: 'models', component: ModelListComponent },
  { path: 'models/new', component: ModelFormComponent },
  { path: 'models/edit/:id', component: ModelFormComponent },
  { path: 'models/view/:id', component: ModelDetailComponent },
  { path: 'validation-test/:id', component: ValidationTestComponent },
  { path: '**', redirectTo: 'models' }
];