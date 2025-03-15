export type SearchParameters<TSupported extends object = { [key: string]: string|number|undefined }> = TSupported & { query?: string };
export type SearchCallback = (params?: SearchParameters) => Promise<void>;
