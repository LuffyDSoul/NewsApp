import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/news',
    pathMatch: 'full'
  },
  {
    path: 'news',
    loadComponent: () => import('./features/news/news-list/news-list.component').then(m => m.NewsListComponent)
  },
  {
    path: '**',
    redirectTo: '/news'
  }
];
