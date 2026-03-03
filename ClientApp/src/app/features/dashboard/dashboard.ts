import { Component, effect, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { SpotifyStatsApi } from '../../shared/services/spotify-stats-api';
import { take } from 'rxjs';
import { FetchableItemTypes } from '../../shared/models/top-items.model';
import { Track } from '../../shared/models/track.model';
import { Artist } from '../../shared/models/artist.model';
import { TimeFrames } from '../../shared/models/time-frames.enum';
import { MatButtonModule } from '@angular/material/button';
import { PluckPipe } from '../../shared/pipes/pluck.pipe';
import { DurationPipe } from '../../shared/pipes/duration-pipe';
import { MatIconModule } from '@angular/material/icon';
import { NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  imports: [MatButtonModule, PluckPipe, DurationPipe, MatIconModule, NgOptimizedImage],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  private readonly spotifyStatsApiService = inject(SpotifyStatsApi);
  private readonly router = inject(Router);
  private timeFrameSelected = signal(TimeFrames.ShortTerm);
  private selectionUpdatedEffect = effect(() => {
    this.getTopItemsByTimeFrame(this.timeFrameSelected(), this.itemTabSelected())
  })

  itemTypes = FetchableItemTypes;
  itemSetCache = new Map<string, Track[] | Artist[]>();
  cacheKey = '';
  tracks = signal<Track[]>([]);
  artists = signal<Artist[]>([]);
  recentlyPlayedTracks = signal<Track[]>([]);
  itemTabSelected = signal(FetchableItemTypes.Tracks);

  timeFrameOptions = [
    { label: '4 Weeks', value: TimeFrames.ShortTerm },
    { label: '6 Months', value: TimeFrames.MediumTerm },
    { label: '1 Year', value: TimeFrames.LongTerm },
  ];
  tabOptions = [
    { label: 'Top Tracks', value: FetchableItemTypes.Tracks },
    { label: 'Top Artists', value: FetchableItemTypes.Artists },
  ];

  getTopItemsByTimeFrame(timeFrame: TimeFrames, selectedItemType: FetchableItemTypes): void {
    if (selectedItemType === FetchableItemTypes.Tracks) {
      const tracks = this.getItemSetFromCache<Track>(this.getCacheKey(selectedItemType, timeFrame))
      tracks ? this.tracks.set(tracks) : this.getTopTracks(timeFrame);
    } else {
      const artists = this.getItemSetFromCache<Artist>(this.getCacheKey(selectedItemType, timeFrame))
      artists ? this.artists.set(artists) : this.getTopArtists(timeFrame);
    }
  }

  getTopTracks(timeFrame: TimeFrames): void {
    this.spotifyStatsApiService.getTopItems<Track>(FetchableItemTypes.Tracks, timeFrame)
      .pipe(take(1))
      .subscribe({
        next: (result) => {
          this.tracks.set(result.items);
          this.setItemSetInCache(FetchableItemTypes.Tracks, timeFrame, result.items);
        },
        error: () => this.router.navigate(['connect']),
      });
  }

  getTopArtists(timeFrame: TimeFrames): void {
    this.spotifyStatsApiService.getTopItems<Artist>(FetchableItemTypes.Artists, timeFrame)
      .pipe(take(1))
      .subscribe({
        next: (result) => {
          this.artists.set(result.items);
          this.setItemSetInCache(FetchableItemTypes.Artists, timeFrame, result.items);
        },
        error: () => this.router.navigate(['connect']),
      });
  }

  updateTimeFrame(timeFrame: TimeFrames): void {
    this.timeFrameSelected.set(timeFrame);
  }

  updateItemType(itemType: FetchableItemTypes): void {
    this.itemTabSelected.set(itemType);
  }

  getItemSetFromCache<T>(cacheKey: string): T[] | undefined {
    return this.itemSetCache.get(cacheKey) as T[] | undefined;
  }

  setItemSetInCache(itemType: FetchableItemTypes, timeFrame: TimeFrames, items: Track[] | Artist[]): void {
    this.itemSetCache.set(this.getCacheKey(itemType, timeFrame), items);
  }

  getCacheKey(itemType: FetchableItemTypes, timeFrame: TimeFrames): string {
    return `${itemType}-${timeFrame}`;
  }
}
