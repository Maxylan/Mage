export type SearchParameters<TSupported extends object> = TSupported & { query?: string };

export type SearchCallback<TSupported extends object, TResult extends object> = (params?: SearchParameters<TSupported>) => Promise<TResult[]>;
