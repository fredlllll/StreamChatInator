import type { int } from "../types";
import type { Channel } from "./Channel";
import type { EmoteFetcher } from "./EmoteFetcher";

export abstract class Emote {
    public channel: Channel;
    public id: string;
    public data: any;
    public fetcher: EmoteFetcher;
    public type: string;
    public code: string;
    public ownerName: string | null = "";
    public imageType: string = "";
    public animated: boolean = false;
    public modifier: boolean = false;
    public sizes: string[] = [];

    public constructor(channel: Channel, id: string, data: any) {
        this.fetcher = channel.fetcher;
        this.channel = channel;
        this.id = id;
        this.type = "none";
        this.code = data.code;
    }

    public toLink () :string{
        return "";
    }

    public toString(): string {
        return this.code;
    }

    //toObject?
}