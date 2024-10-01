import { SaleDetail } from "./sale-detail";

export interface Sale {
    idSale: number,
    numberDocument: string,
    paymentType: string,
    total_Text: string,
    dateRegistry: string,
    saleDetails: SaleDetail[]
}
