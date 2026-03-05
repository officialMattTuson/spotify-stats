import { Track } from "./track.model";

export interface RecentlyPlayedTracks {
  items: TrackContext[];
  cursors: Cursor;
  href: string;
  limit: number;
  next: string;
}

export interface TrackContext {
  track: Track;
  context: object;
  played_at: string;
}

export interface Cursor {
  before: string;
  after: string;
}

export interface MostRecentlyPlayedTrack {
  track: Track;
  timesPlayed: number;
}