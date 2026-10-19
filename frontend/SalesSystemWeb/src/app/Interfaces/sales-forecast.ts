export interface RevenuePoint {
    month: string,          // yyyy-MM
    revenue: number,
    sales?: number | null,
    lower?: number | null,
    upper?: number | null
}

export interface SalesForecast {
    hasEnoughData: boolean,
    message?: string | null,
    method: string,
    history: RevenuePoint[],
    currentMonth?: RevenuePoint | null,
    forecast: RevenuePoint[]
}
