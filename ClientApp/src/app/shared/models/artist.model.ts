import { Image } from "./image.model";
import { ItemType } from "./top-items.model";

export interface Artist {
  external_urls: ExternalUrl; 
  image: Image;
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