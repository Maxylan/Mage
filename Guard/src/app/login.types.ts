export interface HashedUserDetails {
    username: string,
    hash: string
}

export type LoginBody = HashedUserDetails & {}

export type Session = {
    id: number;
    userId: number;
    code: string;
    userAgent?: string|null;
    createdAt: string;
    expiresAt: string;
}
