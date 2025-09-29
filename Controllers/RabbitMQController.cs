using System;
using Microsoft.AspNetCore.Mvc;
using ConsumerAPI.Models;
using ConsumerAPI.Services;

namespace ConsumerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RabbitMQController : ControllerBase
    {
        private readonly IRabbitMQService<ClassMessageRMQ> _rabbitMQService;

        public RabbitMQController(IRabbitMQService<ClassMessageRMQ> rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }

        [HttpGet("receive/{queueName?}")]
        public async Task<IActionResult> ReceiveMessages(string queueName)
        {
            try
            {
                var messages = await _rabbitMQService.ReceiveMessage(queueName);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("receiveWithAck")]
        public async Task<IActionResult> ReceiveMessagesWithAcknowledgment(string queueName)
        {
            try
            {
                var messages = await _rabbitMQService.ReceiveMessageWithAcknowledgment(queueName);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}