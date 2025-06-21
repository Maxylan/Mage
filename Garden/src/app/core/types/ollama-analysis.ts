import { Analysis } from './analysis';

export type OllamaAnalysis = {
    response: Analysis,
    done: boolean,
    model: string | null,
    createdAt: string | null,
    logId: string | null,
    error: string | null,
}