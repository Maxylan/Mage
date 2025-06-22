export interface IPublicLinkDTO {
    id?: number | null,
    photoId: number,
    /**
     * Max Length: 32
     * Min Length: 0
     */
    code?: string | null,
    createdBy?: number | null,
    createdAt: Date,
    expiresAt: Date,
    accessLimit?: number | null,
    accessed: number,
}