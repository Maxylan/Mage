export type OllamaResponse = {
    response: string | null,
    done: boolean,
    model: string | null,
    createdAt: string | null,
    logId: string | null,
    error: string | null,
}