import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'connect',
        pathMatch: 'full'
    },
    {
        path: 'connect',
        loadComponent: () =>
            import('./features/authenticate/connect/connect').then(
                (m) => m.Connect 
            ),
        // canActivate: [AuthenticationGuard],
    },
    {
        path: 'dashboard',
        loadComponent: () =>
            import('./features/dashboard/dashboard').then(
                (m) => m.Dashboard
            ),
        // canActivate: [AuthenticationGuard],
    },
    {
        path: 'error',
        loadComponent: () =>
            import('./features/authenticate/connect/connect').then(
                (m) => m.Connect 
            ),
    },
];
