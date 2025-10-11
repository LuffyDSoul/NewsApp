import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/news',
    pathMatch: 'full'
  },
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'auth/register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'news',
    loadComponent: () => import('./features/news/news-list/news-list.component').then(m => m.NewsListComponent),
    canActivate: [AuthGuard]  // Proteger ruta de noticias
  },
  {
    path: 'reading-lists',
    loadComponent: () => import('./features/reading-lists/reading-lists.component').then(m => m.ReadingListsComponent),
    canActivate: [AuthGuard]  // Proteger ruta de listas de lectura
  },
  {
    path: 'profile',
    loadComponent: () => import('./features/user-profile/user-profile.component').then(m => m.UserProfileComponent),
    canActivate: [AuthGuard]  // Proteger ruta de perfil
  },
  {
    path: '**',
    redirectTo: '/news'
  }
];
