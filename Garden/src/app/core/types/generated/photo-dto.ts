export interface IPhotoDTO {
    id?: number | null,
    /**
     * Max Length: 127
     * Min Length: 0
     */
    slug?: string | null,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    title?: string | null,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    summary?: string | null,
    description?: string | null,
    uploadedBy?: number | null,
    uploadedAt: Date,
    updatedBy?: number | null,
    updatedAt: Date,
    createdAt: Date,
    isAnalyzed: boolean,
    analyzedAt?: Date | null,
    requiredPrivilege: number,
}