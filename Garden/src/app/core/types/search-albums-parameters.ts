export type SearchAlbumsParameters = {
    /* [Required] */ limit: number,
    /* [Required] */ offset: number,
    /* [FromQuery] */ title?: string|null,
    /* [FromQuery] */ summary?: string|null,
    /* [FromQuery] */ tags?: string[]|null,
    /* [FromQuery] */ createdBy?: number|null,
    /* [FromQuery] */ createdBefore?: Date|null,
    /* [FromQuery] */ createdAfter?: Date|null
}
