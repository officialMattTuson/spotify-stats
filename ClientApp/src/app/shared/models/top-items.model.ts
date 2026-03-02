export interface TopItems<T> {
  href: string;
  limit: number;
  next: string;
  offset: number;
  previous: string;
  total: number;
  items: T[];
}

export enum FetchableItemTypes {
  Artists = "artists",
  Tracks = "tracks"
}

export enum ItemType {
  Artist = "artist",
  Track = "track",
  Album = "album"
}