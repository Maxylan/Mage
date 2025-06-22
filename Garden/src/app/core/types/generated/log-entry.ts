import { Method } from './method';
import { Source } from './source';
import { Severity } from './severity';

export type LogEntry = {
    id: number,
    userId: number | null,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    userEmail: string | null,
    /**
     * Max Length: 127
     * Min Length: 0
     */
    userUsername: string | null,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    userFullName: string | null,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    requestAddress: string | null,
    /**
     * Max Length: 1023
     * Min Length: 0
     */
    requestUserAgent: string | null,
    createdAt: Date,
    logLevel: Severity,
    source: Source,
    method: Method,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    action: string | null,
    message: string | null,
}