import type { Channel } from "./Channel";
import Constants from "./Constants";
import { Emote } from "./Emote";

export class BTTVEmote extends Emote {
    /**
     * A BTTV emote.
     * @param {Channel} channel - Channel this emote belongs to.
     * @param {string} id - ID of the emote.
     * @param {data} data - The raw emote data.
     * 
     */
    
    constructor(channel:Channel, id:string, data:any) {
        super(channel, id, data);
        this.type = 'bttv';

        this.ownerName = 'user' in data ? data.user.name : null;
        this.animated = data.animated;
        this.imageType = 'webp';
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
     * @param {number} size - The size of the image, 0, 1, or 2.
     * @returns {string}
     */
    toLink(size = 0) {
        return Constants.BTTV.CDN(this.id, size);
    }
}
