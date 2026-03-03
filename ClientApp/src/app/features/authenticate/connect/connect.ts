import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { SpotifyStatsApi } from '../../../shared/services/spotify-stats-api';
import { Router } from '@angular/router';

@Component({
  selector: 'app-connect',
  imports: [MatCardModule, MatButtonModule, MatIconModule],
  templateUrl: './connect.html',
  styleUrl: './connect.scss',
})
export class Connect {
  private readonly spotifyStatsApiService = inject(SpotifyStatsApi);
  private readonly router = inject(Router);
  connectToSpotify() {
      // Redirect browser to backend login endpoint for OAuth
      window.location.href = 'http://localhost:5105/api/auth/login';
  }
}
