import type { Channel } from "./Channel";
import Constants from "./Constants";
import { Emote } from "./Emote";

export class SevenTVEmote extends Emote {
    /**
     * A 7TV emote.
     * @param {Channel} channel - Channel this emote belongs to.
     * @param {string} id - ID of the emote.
     * @param {data} data - The raw emote data.
     */
    constructor(channel: Channel, id:string, data:any) {
        super(channel, id, data);
        this.type = '7tv';

        this.code = data.name;

        this.ownerName = 'owner' in data.data ? data.data.owner.display_name : null;
        this.sizes = data.data.host.files
            .filter((el: any) => el.format === this.channel.format.toUpperCase())
            .map((el:any) => el.name);
        this.animated = data.data.animated;
        this.imageType = this.channel.format;
    }

    /**
     * The channel of this emote's creator.
     * Not guaranteed to contain the emote, or be cached.
     * @readonly
     * @type {?Channel}
     */
    get owner() {
        return this.fetcher.channels.get(this.ownerName);
    }

    /**
     * Gets the image link of the emote.
     * @param {number} size - The size of the image.
     * @returns {string}
     */
    toLink(size = 0) {
        size = this.sizes[size] as any;
        return Constants.SevenTV.CDN(this.id, size); // eslint-disable-line new-cap
    }
}