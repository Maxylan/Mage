export type FilterBanEntries = {
    /**
     * <strong>ID</strong> of the banned <see cref="Client"/>.
     */
    clientId?: number,
    /**
     * <strong>Address</strong> of the banned <see cref="Client"/>.
     */
    address?: string,
    /**
     * <strong>User Agent</strong> of the banned <see cref="Client"/>.
     */
    userAgent?: string,
    /**
     * <strong>Account</strong> (user) <strong>ID</strong> associated with
     * the banned <see cref="Client"/>.
     */
    accountId?: number,
    /**
     * <strong>Username</strong> of the <see cref="Account"/> associated with
     * the banned <see cref="Client"/>.
     */
    username?: string,
    /**
     * Pagination: Limit
     */
    limit?: number,
    /**
     * Pagination: Offset
     */
    offset?: number
}
