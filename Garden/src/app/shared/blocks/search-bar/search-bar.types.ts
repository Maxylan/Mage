export type SearchParameters<TSupported extends object> = TSupported & { query?: string };

// export type SearchCallback<TSupported extends object, TResult extends object> = (params?: SearchParameters<TSupported>) => Promise<TResult[]>;
export type SearchCallback = (params?: SearchParameters<{[key: string]: string|number|undefined}>) => Promise<void>;
