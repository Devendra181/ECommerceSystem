namespace PaymentService.Domain.Enums
{
    public enum RefundMethodTypeEnum
    {
        Original = 1,        // send back to original instrument (PG flow)
        Wallet = 2,          // push to wallet
        BankTransfer = 3,    // manual NEFT/RTGS/IMPS
        Manual = 4           // Manual return (typical for COD)
    }
}
