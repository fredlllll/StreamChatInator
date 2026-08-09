import type { Channel } from "./Channel";
import Constants from "./Constants";
import { Emote } from "./Emote";

export class FFZEmote extends Emote {
    /**
     * An FFZ emote.
     * @param {Channel} channel - Channel this emote belongs to.
     * @param {string} id - ID of the emote.
     * @param {data} data - The raw emote data.
     */
    constructor(channel:Channel, id:string, data:any) {
        super(channel, id, data);
        this.type = 'ffz';

        this.code = data.name;
        this.ownerName = 'owner' in data ? data.owner.name : null;
        this.sizes = 'animated' in data ? Object.keys(data.animated) : Object.keys(data.urls);
        this.animated = 'animated' in data;
        this.imageType = 'animated' in data ? 'webp' : 'png';
        this.modifier = data.modifier && (data.modifier_flags & 1) !== 0;
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
        size = this.sizes[size] as any; //idk wtf the library is doing here
        if (this.animated) return Constants.FFZ.CDNAnimated(this.id, size); // eslint-disable-line new-cap
        return Constants.FFZ.CDN(this.id, size); // eslint-disable-line new-cap
    }
}
