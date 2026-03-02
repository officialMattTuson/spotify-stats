import { Artist, ExternalUrl } from "./artist.model";
import { Image } from "./image.model";
import { ItemType } from "./top-items.model";

export interface Album {
  album_type: AlbumType;
  total_tracks: number;
  available_markets: string[];
  external_urls: ExternalUrl;
  href: string;
  id: string;
  images: Image[];
  name: string;
  release_date: string;
  release_date_precision: string;
  restrictions: Restriction;
  type: ItemType.Album;
  url: string;
  artists: Artist[];
}

export enum AlbumType {
  Album = "album",
  Single = "single",
  Compilation = "compilation"
}

export interface Restriction {
  reason: RestrictionReason;
}

export enum RestrictionReason {
  Market = "market",
  Product = "product",
  Explicit = "explicit"
}