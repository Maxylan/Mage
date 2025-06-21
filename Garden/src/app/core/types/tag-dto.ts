export interface ITagDTO {
    id?: number | null,
    /**
     * Max Length: 127
     * Min Length: 0
     */
    name?: string | null,
    description?: string | null,
    requiredPrivilege: number,
    readonly items: number,
}