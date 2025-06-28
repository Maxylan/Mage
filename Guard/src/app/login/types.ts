export interface HashedUserDetails {
    username: string,
    hash: string
}

export type LoginBody = HashedUserDetails & {}

export type BanEntry = {
    id: number,
    clientId: number,
    expiresAt: Date,
    reason: string|null
}

export type Client = {
    id: number,
    trusted: boolean,
    address: string,
    userAgent: string,
    logins: number,
    failedLogins: number,
    createdAt: Date,
    lastVisit: Date,
    banEntries: BanEntry[],
    /*
    sessions: ISessionDTO[],
    accounts: []
    */
}

export type Account = {
    id: number,
    email: string,
    username: string,
    password: string,
    fullName: string,
    createdAt: Date,
    lastLogin: Date,
    privilege: number,
    avatarId: number
    /*
    avatar: null,
    albumsCreated: [],
    albumsUpdated: [],
    createdCategories: [],
    updatedCategories: [],
    favoriteAlbums: [],
    favoritePhotos: [],
    linksCreated: [],
    photosUpdated: [],
    photosUploaded": []
    */
}

export type Session = ISessionDTO & {
    id: number,
    accountId: number,
    clientId: number,
    code: string,
    createdAt: string,
    expiresAt: string,
    client: Client,
    account: Account
};

export interface ISessionDTO {
    id?: number,
    accountId: number,
    clientId: number,
    code: string,
    createdAt: string,
    expiresAt: string,
    client?: Client|null,
    account?: Account|null
}
