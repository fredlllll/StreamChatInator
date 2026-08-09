import { BTTVEmote } from "./BTTVEmote";
import { Channel } from "./Channel";
import { Collection } from "./Collection";
import Constants from "./Constants";
import axios from "axios";
import { FFZEmote } from "./FFZEmote";
import { SevenTVEmote } from "./SevenTVEmote";

export class EmoteFetcher {
    public emotes: Collection;
    public channels: Collection;
    public ffzModifiersFetched: boolean;
    constructor() {

        /**
         * Cached emotes.
         * Collectionped by emote code to Emote instance.
         * @type {Collection<string, Emote>}
         */
        this.emotes = new Collection();

        /**
         * Cached channels.
         * Collectionped by name to Channel instance.
         * @type {Collection<string, Channel>}
         */
        this.channels = new Collection();

        /**
         * Save if we fetched FFZ's modifier emotes once.
         * @type {boolean}
         */
        this.ffzModifiersFetched = false;
    }

    /**
     * The global channel for Twitch, BTTV and 7TV.
     * @readonly
     * @type {?Channel}
     */
    get globalChannel() {
        return this.channels.get(null);
    }

    /**
     * Sets up a channel
     * @private
     * @param {int} channel_id - ID of the channel.
     * @param {string} [format=null] - The type file format to use (webp/avif).
     * @throws {Error} When Twitch Client ID or Client Secret were not provided.
     * @returns {Channel}
     */
    _setupChannel(channel_id: string|null, format: string | null = null) {
        let channel: Channel = this.channels.get(channel_id);
        if (!channel) {
            channel = new Channel(this, channel_id);
            this.channels.set(channel_id, channel);
        }
        if (format) {
            channel.format = format;
        }
        return channel;
    }

    /**
     * Gets the raw BTTV emotes data for a channel.
     * Use `null` for the global emotes channel.
     * @private
     * @param {int} [id=null] - ID of the channel.
     * @returns {Promise<object[]>}
     */
    _getRawBTTVEmotes(id:string|null) {
        const endpoint = !id
            ? Constants.BTTV.Global
            : Constants.BTTV.Channel(id); // eslint-disable-line new-cap

        return axios.get(endpoint).then(req => {
            // Global emotes
            if (req.data instanceof Array) return req.data;
            // Channel emotes
            return [...req.data.channelEmotes, ...req.data.sharedEmotes];
        });
    }

    /**
     * Converts and caches a raw BTTV emote.
     * @private
     * @param {int} channel_id - ID of the channel.
     * @param {object} data - Raw data.
     * @param {BTTVEmote} [existing_emote=null] - Existing emote to cache.
     * @returns {BTTVEmote}
     */
    _cacheBTTVEmote(channel_id:string|null, data:any, existing_emote = null) {
        const channel = this._setupChannel(channel_id);
        const emote = existing_emote || new BTTVEmote(channel, data.id, data);
        this.emotes.set(emote.code, emote);
        channel.emotes.set(emote.code, emote);
        return emote;
    }

    /**
     * Gets the raw FFZ emote data from a set.
     * @private
     * @param {int} id - ID of the set.
     * @returns {Promise<object[]>}
     */
    _getRawFFZEmoteSet(id:string) {
        const endpoint = Constants.FFZ.Set(id); // eslint-disable-line new-cap

        return axios.get(endpoint).then(req => {
            return req.data.set.emoticons;
        });
    }

    /**
     * Gets the raw FFZ emotes data for a channel.
     * @private
     * @param {int} id - ID of the channel.
     * @returns {Promise<object[]>}
     */
    _getRawFFZEmotes(id:string) {
        const endpoint = Constants.FFZ.Channel(id); // eslint-disable-line new-cap

        return axios.get(endpoint).then(req => {
            const emotes = [];
            for (const key of Object.keys(req.data.sets)) {
                const set = req.data.sets[key];
                emotes.push(...set.emoticons);
            }

            return emotes;
        });
    }

    /**
     * Converts and caches a raw FFZ emote.
     * @private
     * @param {int} channel_id - ID of the channel.
     * @param {object} data - Raw data.
     * @param {FFZEmote} [existing_emote=null] - Existing emote to cache.
     * @returns {FFZEmote}
     */
    _cacheFFZEmote(channel_id:string|null, data:any, existing_emote = null) {
        const channel = this._setupChannel(channel_id);
        const emote = existing_emote || new FFZEmote(channel, data.id, data);
        this.emotes.set(emote.code, emote);
        channel.emotes.set(emote.code, emote);
        return emote;
    }

    /**
     * Gets the raw 7TV emotes data for a channel.
     * @private
     * @param {int} [id=null] - ID of the channel.
     * @returns {Promise<object[]>}
     */
    _getRawSevenTVEmotes(id:string|null) {
        // If we have a channel ID, we'll need to find the user's emote-set ID,
        // and then we get the emotes from there.
        if (id) {
            return axios
                .get(Constants.SevenTV.Channel(id)) // eslint-disable-line new-cap
                .then(userReq => {
                    return axios
                        .get(Constants.SevenTV.EmoteSet(userReq.data.emote_set_id)) // eslint-disable-line new-cap
                        .then(setReq => setReq.data);
                });
        }

        // Otherwise, we can just fetch the global emotes directly.
        return axios.get(Constants.SevenTV.Global).then(req => req.data);
    }

    /**
     * Converts and caches a raw 7TV emote.
     * @private
     * @param {int} channel_id - ID of the channel.
     * @param {object} data - Raw data.
     * @param {string} format - The type file format to use (webp/avif).
     * @param {SevenTVEmote} [existing_emote=null] - Existing emote to cache.
     * @returns {SevenTVEmote}
     */
    _cacheSevenTVEmote(channel_id:string|null, data:any, format:string, existing_emote = null) {
        const channel = this._setupChannel(channel_id, format);
        const emote = existing_emote || new SevenTVEmote(channel, data.id, data);
        this.emotes.set(emote.code, emote);
        channel.emotes.set(emote.code, emote);
        return emote;
    }

    /**
     * Fetches the BTTV emotes for a channel.
     * Use `null` for the global emotes channel.
     * @param {int} [channel=null] - ID of the channel.
     * @returns {Promise<Collection<string, BTTVEmote>>}
     */
    fetchBTTVEmotes(channel:string|null = null) {
        return this._getRawBTTVEmotes(channel).then(rawEmotes => {
            for (const data of rawEmotes) {
                this._cacheBTTVEmote(channel, data);
            }

            return this.channels.get(channel).emotes.filter((e:any) => e.type === 'bttv');
        });
    }

    /**
     * Fetches the FFZ emotes for a channel.
     * @param {int} [channel=null] - ID of the channel.
     * @returns {Promise<Collection<string, FFZEmote>>}
     */
    async fetchFFZEmotes(channel:string|null = null) {
        // Fetch modifier emotes at least once
        if (!this.ffzModifiersFetched) {
            this.ffzModifiersFetched = true;

            await this._getRawFFZEmoteSet(Constants.FFZ.sets.Modifiers).then(rawEmotes => {
                for (const data of rawEmotes) {
                    this._cacheFFZEmote(null, data);
                }
            });
        }

        // If no channel specified, fetch the Global set
        if (!channel) {
            return this._getRawFFZEmoteSet(Constants.FFZ.sets.Global).then(rawEmotes => {
                for (const data of rawEmotes) {
                    this._cacheFFZEmote(channel, data);
                }

                return this.channels.get(channel).emotes.filter((e:any) => e.type === 'ffz');
            });
        }

        return this._getRawFFZEmotes(channel).then(rawEmotes => {
            for (const data of rawEmotes) {
                this._cacheFFZEmote(channel, data);
            }

            return this.channels.get(channel).emotes.filter((e:any) => e.type === 'ffz');
        });
    }

    /**
     * Fetches the 7TV emotes for a channel.
     * @param {int} [channel=null] - ID of the channel.
     * @param {('webp'|'avif')} [format='webp'] - The type file format to use (webp/avif).
     * @returns {Promise<Collection<string, SevenTVEmote>>}
     */
    fetchSevenTVEmotes(channel:string|null = null, format = 'webp') {
        return this._getRawSevenTVEmotes(channel).then(rawEmotes => {
            // We should have an 'emotes' property in our set
            if ('emotes' in rawEmotes) {
                for (const data of rawEmotes.emotes) {
                    this._cacheSevenTVEmote(channel, data, format);
                }
            }

            return this.channels.get(channel).emotes.filter((e:any) => e.type === '7tv');
        });
    }
}