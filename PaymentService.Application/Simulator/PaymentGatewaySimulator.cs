using PaymentService.Application.DTOs;

namespace PaymentService.Application.Simulator
{
    public class PaymentGatewaySimulator
    {
        private readonly Random _random = new();

        // Probability distribution for payment statuses:
        // Success: 60%, Pending: 20%, Failed: 10%, Cancelled: 10%
        private string GetRandomStatus()
        {
            int roll = _random.Next(1, 101); // 1 to 100
            if (roll <= 60) return "Success";
            if (roll <= 80) return "Pending";
            if (roll <= 90) return "Failed";
            return "Cancelled";
        }

        private string MapStatusToCode(string status) => status switch
        {
            "Success" => "200",
            "Pending" => "102",
            "Failed" => "500",
            "Cancelled" => "400",
            _ => "000"
        };

        // Create a hosted checkout session
        public async Task<CheckoutSessionDTO> CreateCheckoutSessionAsync(Guid paymentId, decimal amount, string currency, int providerConfigurationId)
        {
            //Simulate the Server to Server Call
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Simulate provider order id and checkout url
            var providerOrderId = $"PG_Prov_{providerConfigurationId}_{paymentId:N}_{_random.Next(100000, 999999)}";
            var checkoutUrl = $"https://simulated.pg/checkout?order={providerOrderId}&pid={paymentId}&amt={amount}&cur={currency}";

            var dto = new CheckoutSessionDTO
            {
                CheckoutUrl = checkoutUrl,
                ProviderOrderId = providerOrderId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(20)
            };

            return dto;
        }

        // Query payment details at PG using provider reference
        public async Task<PaymentGatewayResponseDTO> GetPaymentDetailsAsync(string paymentId)
        {
            //Simulate the Server to Server Call
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Random outcome for simulation
            var status = GetRandomStatus();
            var resp = new PaymentGatewayResponseDTO
            {
                PaymentId = paymentId,
                TransactionId = Guid.NewGuid().ToString(),         // reuse as "provider ref"
                Status = status,
                StatusCode = MapStatusToCode(status),
                Message = status switch
                {
                    "Success" => "Payment captured at provider",
                    "Pending" => "Payment pending at provider",
                    "Failed" => "Payment failed at provider",
                    "Cancelled" => "Payment cancelled at provider",
                    _ => "Unknown"
                },
                ErrorMessage = status is "Failed" or "Cancelled" ? "Simulated provider issue" : null,
                Timestamp = DateTime.UtcNow,
                Amount = 1000,
                Currency = "INR"
            };

            return resp;
        }

        public async Task<RefundGatewayResponseDTO> ProcessRefundAsync(string refundid, decimal refundAmount)
        {
            //Simulate the Server to Server Call
            await Task.Delay(TimeSpan.FromSeconds(1));

            var status = GetRandomStatus();
            var response = new RefundGatewayResponseDTO
            {
                RefundId = refundid,
                TransactionId = Guid.NewGuid().ToString(),
                Status = status,
                StatusCode = MapStatusToCode(status),
                Message = status switch
                {
                    "Success" => "Refund Processed",
                    "Pending" => "Refund Pending",
                    "Failed" => "Refund Failed",
                    "Cancelled" => "Refund Cancelled",
                    _ => "Unknown Status"
                },
                ErrorMessage = status is "Failed" or "Cancelled" ? "Simulated refund error" : null,
                Timestamp = DateTime.UtcNow,
                Amount = refundAmount,
                Currency = "INR"
            };

            return response;
        }
    }
}
