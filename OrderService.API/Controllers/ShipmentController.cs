using Microsoft.AspNetCore.Mvc;
using OrderService.API.DTOs;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Shipments;
using OrderService.Application.Interfaces;

namespace OrderService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService ?? throw new ArgumentNullException(nameof(shipmentService));
        }

        // POST api/shipment/create
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<ShipmentResponseDTO>>> CreateShipment([FromBody] CreateShipmentRequestDTO request)
        {
            try
            {
                var shipment = await _shipmentService.CreateShipmentAsync(request);
                return Ok(ApiResponse<ShipmentResponseDTO>.SuccessResponse(shipment, "Shipment created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ShipmentResponseDTO>.FailResponse("Failed to create shipment.", new List<string> { ex.Message }));
            }
        }

        // GET api/shipment/{shipmentId}
        [HttpGet("{shipmentId:guid}")]
        public async Task<ActionResult<ApiResponse<ShipmentResponseDTO>>> GetShipmentById(Guid shipmentId)
        {
            try
            {
                var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
                if (shipment == null)
                    return NotFound(ApiResponse<ShipmentResponseDTO>.FailResponse("Shipment not found."));
                return Ok(ApiResponse<ShipmentResponseDTO>.SuccessResponse(shipment));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ShipmentResponseDTO>.FailResponse("Failed to get shipment.", new List<string> { ex.Message }));
            }
        }

        // GET api/shipment/order/{orderId}
        [HttpGet("order/{orderId:guid}")]
        public async Task<ActionResult<ApiResponse<List<ShipmentResponseDTO>>>> GetShipmentsByOrderId(Guid orderId)
        {
            try
            {
                var shipments = await _shipmentService.GetShipmentsByOrderIdAsync(orderId);
                return Ok(ApiResponse<List<ShipmentResponseDTO>>.SuccessResponse(shipments));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ShipmentResponseDTO>>.FailResponse("Failed to get shipments.", new List<string> { ex.Message }));
            }
        }

        // PUT api/shipment/update-status
        [HttpPut("update-status")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateShipmentStatus([FromBody] ShipmentStatusUpdateRequestDTO request)
        {
            try
            {
                await _shipmentService.UpdateShipmentStatusAsync(request);
                return Ok(ApiResponse<string>.SuccessResponse(null, "Shipment status updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.FailResponse("Failed to update shipment status.", new List<string> { ex.Message }));
            }
        }

        // POST api/shipment/track
        [HttpPost("track")]
        public async Task<ActionResult<ApiResponse<ShipmentTrackingResponseDTO>>> TrackShipment([FromBody] ShipmentTrackingRequestDTO request)
        {
            try
            {
                var trackingInfo = await _shipmentService.TrackShipmentAsync(request);
                return Ok(ApiResponse<ShipmentTrackingResponseDTO>.SuccessResponse(trackingInfo));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ShipmentTrackingResponseDTO>.FailResponse("Failed to track shipment.", new List<string> { ex.Message }));
            }
        }
    }
}
