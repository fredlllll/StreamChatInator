export function isString(value: unknown): value is string {
    return typeof value === "string";
}

export function isStringOrWrapper(value: unknown): value is string {
    return (
        typeof value === "string" ||
        value instanceof String ||
        Object.prototype.toString.call(value) === "[object String]"
    );
}