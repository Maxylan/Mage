export interface HashedUserDetails {
    username: string,
    password: string
}

export type LoginBody = {
    username: string,
    password: string
}

export type Session = {
    id: number;
    userId: number;
    code: string;
    userAgent?: string|null;
    createdAt: string;
    expiresAt: string;
}
