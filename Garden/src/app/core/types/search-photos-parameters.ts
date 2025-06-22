export type SearchPhotosParameters = {
    /** [Required] */ limit: number,
    /** [Required] */ offset: number,
    /** [FromQuery] */ search?: string|null,
    /** [FromQuery] */ slug?: string|null,
    /** [FromQuery] */ title?: string|null,
    /** [FromQuery] */ summary?: string|null,
    /** [FromQuery] */ tags?: string[]|null,
    /** [FromQuery] */ uploadedBy?: number|null,
    /** [FromQuery] */ uploadedBefore?: string|null,
    /** [FromQuery] */ uploadedAfter?: string|null,
    /** [FromQuery] */ createdBefore?: string|null,
    /** [FromQuery] */ createdAfter?: Date|null
}
