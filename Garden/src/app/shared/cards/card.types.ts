export type CardDetails = {
    key: string;
    title: string;
    summary: string|null;
    description: string|null;
    tags: string[]|null;
};

export type CardSelectDetails = {
    selected: boolean;
    card: CardDetails;
};

export type CardLinkDetails = {
    link: string;
    card: CardDetails;
};
