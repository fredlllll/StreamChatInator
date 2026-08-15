import type { UserFlagName, UserTypeName } from "../types";

export type BadgeSlot = {
    set: string;
    version: string;
    fallback: string;
};

// Fallback labels shown (as text) when a badge's image can't be found in the
// fetched badge map. The message's `badges` tag is the ground truth; the
// flag/type fallbacks below are only used for badges Twitch didn't put there.
export const BADGE_LABELS: Record<string, string> = {
    broadcaster: "Streamer",
    moderator: "Mod",
    vip: "Vip",
    subscriber: "Sub",
    founder: "Founder",
    global_mod: "Global Mod",
    admin: "Admin",
    staff: "Staff",
    turbo: "Turbo",
    partner: "Partner",
    bits: "Bits",
    bits_charity: "Bits",
    bits_leaderboard: "Bits Leader",
    prediction: "Prediction",
    predictions: "Prediction",
    sub_gift_leaderboard: "Sub Gifter",
    sub_gifter: "Sub Gifter",
    premium: "Prime",
    no_audio: "No Audio",
    no_video: "No Video",
    uploader: "Uploader",
};

export const USER_TYPE_BADGES: Partial<Record<UserTypeName, BadgeSlot>> = {
    Broadcaster: { set: "broadcaster", version: "1", fallback: "Streamer" },
    Moderator: { set: "moderator", version: "1", fallback: "Mod" },
    GlobalModerator: { set: "global_mod", version: "1", fallback: "Global Mod" },
    Admin: { set: "admin", version: "1", fallback: "Admin" },
    Staff: { set: "staff", version: "1", fallback: "Staff" },
};

export const USER_FLAG_BADGES: Partial<Record<UserFlagName, BadgeSlot>> = {
    Moderator: { set: "moderator", version: "1", fallback: "Mod" },
    Subscriber: { set: "subscriber", version: "0", fallback: "Sub" },
    Vip: { set: "vip", version: "1", fallback: "Vip" },
    Partner: { set: "partner", version: "1", fallback: "Partner" },
    Turbo: { set: "turbo", version: "1", fallback: "Turbo" },
    Staff: { set: "staff", version: "1", fallback: "Staff" },
};

// Full descriptive names used for the fallback badges' tooltips. The short
// fallback labels above stay on screen; hovering reveals the full name.
export const BADGE_TITLES: Record<string, string> = {
    broadcaster: "Channel Broadcaster",
    moderator: "Moderator",
    vip: "VIP",
    subscriber: "Subscriber",
    founder: "Founder",
    global_mod: "Global Moderator",
    admin: "Twitch Admin",
    staff: "Twitch Staff",
    turbo: "Twitch Turbo",
    partner: "Twitch Partner",
    bits: "Cheer Badge",
    bits_charity: "Charity Cheer Badge",
    bits_leaderboard: "Bits Leaderboard",
    prediction: "Channel Predictions",
    predictions: "Channel Predictions",
    sub_gift_leaderboard: "Sub Gifter Leaderboard",
    sub_gifter: "Sub Gifter",
    premium: "Prime Gaming",
    no_audio: "No Audio",
    no_video: "No Video",
    uploader: "Video Uploader",
};