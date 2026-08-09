import { Collection } from "./Collection";
import type { EmoteFetcher } from "./EmoteFetcher";

export class Channel {
    public fetcher: EmoteFetcher;
    public id: string|null;
    public emotes: Collection;
    public format: string;

    public constructor(fetcher: EmoteFetcher, id: string|null) {
        this.fetcher = fetcher;
        this.id = id;
        this.emotes = new Collection();
        this.format = "";
    }
}