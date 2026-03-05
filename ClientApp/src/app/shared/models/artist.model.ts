import { Image } from "./image.model";
import { ItemType } from "./top-items.model";

export interface Artist {
  external_urls: ExternalUrl; 
  images: Image[];
  name: string;
  popularity: number;
  type: ItemType.Artist;
  uri: string;
}

export interface ExternalUrl {
  spotify: string;
}

export interface Follower {
  href: string;
  total: number;
}