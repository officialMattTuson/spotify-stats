import { TestBed } from '@angular/core/testing';

import { SpotifyStatsApi } from './spotify-stats-api';

describe('SpotifyStatsApi', () => {
  let service: SpotifyStatsApi;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SpotifyStatsApi);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
