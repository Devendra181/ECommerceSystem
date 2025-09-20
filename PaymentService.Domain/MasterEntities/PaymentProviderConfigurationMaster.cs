using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace PaymentService.Domain.MasterEntities
{
    [Index(nameof(GatewayProviderId), Name = "IX_PaymentConfiguration_GatewayProviderId")]
    [Index(nameof(EnvironmentId), Name = "IX_PaymentConfiguration_EnvironmentId")]
    public class PaymentProviderConfigurationMaster
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GatewayProviderId { get; set; }

        [ForeignKey(nameof(GatewayProviderId))]
        public virtual GatewayProviderMaster GatewayProvider { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string ApiKey { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string ApiSecret { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string EndpointUrl { get; set; } = null!;

        [MaxLength(500)]
        public string? WebhookUrl { get; set; }

        [Required]
        public int EnvironmentId { get; set; }

        [ForeignKey(nameof(EnvironmentId))]
        public virtual EnvironmentMaster Environment { get; set; } = null!;

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
