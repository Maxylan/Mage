export type DisplayClient = {
    id: number | null,
    trusted: boolean,
    address: string | null,
    userAgent: string | null,
    logins: number,
    failedLogins: number,
    createdAt: Date,
    lastVisit: Date,
    readonly isBanned: boolean,
}