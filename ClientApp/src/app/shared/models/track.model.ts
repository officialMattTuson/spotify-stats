import { Album, Restriction } from "./album.model";
import { Artist, ExternalUrl } from "./artist.model";
import { ItemType } from "./top-items.model";

export interface Track {
  album: Album;
  artists: Artist[];
  available_markets: string[];
  disc_number: number;
  duration_ms: number;
  explicit: boolean;
  external_urls: ExternalUrl;
  href: string;
  id: string;
  is_playable: string;
  restrictions: Restriction;
  name: string;
  popularity: number;
  track_number: number;
  type: ItemType.Track;
  uri: string;
  is_local: boolean;
}
