import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { TimeFrames } from '../models/time-frames.enum';
import { FetchableItemTypes, TopItems } from '../models/top-items.model';
import { RecentlyPlayedTracks } from '../models/recently-played.model';

@Injectable({
  providedIn: 'root',
})
export class SpotifyStatsApi {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5105/api';

  public getLoginUrl(): Observable<{ authUrl: string }> {
    return this.httpClient.get<{ authUrl: string }>(`${this.apiUrl}/auth/login`);
  }

  public getTopItems<T>(type: FetchableItemTypes, timeRange: TimeFrames, limit: number = 10): Observable<TopItems<T>> {
    return this.httpClient.get<TopItems<T>>(`${this.apiUrl}/spotify/top/${type}`, {
      params: { timeRange: timeRange.toString(), limit: limit.toString() },
      withCredentials: true
    });
  }

  public getRecentTracks(limit: number = 50): Observable<RecentlyPlayedTracks> {
    return this.httpClient.get<RecentlyPlayedTracks>(`${this.apiUrl}/spotify/recently-played`, {
      params: { limit: limit.toString() },
      withCredentials: true
    });
  }
}
