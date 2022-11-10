using AirlineWeb.Data;
using AirlineWeb.Dtos;
using AirlineWeb.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirlineWeb.Controllers;

    [Route("api/[controller]")]
    [ApiController]
    public class WebhookSubscriptionController : ControllerBase
    {
        private readonly AirlineDbContext _context;
        private readonly IMapper _mapper;

        public WebhookSubscriptionController(AirlineDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("{secret}", Name = "GetSubscriptionBySecret")]
        public async Task<ActionResult<WebhookSubscriptionReadDto>> GetSubscriptionBySecret(string secret)
        {
            var subscription = await _context.WebhookSubscriptions.FirstOrDefaultAsync(s => s.Secret == secret);

            if (subscription == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<WebhookSubscriptionReadDto>(subscription));
        }

        [HttpPost]
        public async Task<ActionResult<WebhookSubscriptionReadDto>> CreateSubscription(WebhookSubscriptionCreateDto webhookSubscriptionCreateDto)
        {
           var subscription = await _context.WebhookSubscriptions.FirstOrDefaultAsync(s => s.WebhookURI == webhookSubscriptionCreateDto.WebhookURI);

           if (subscription != null) return NoContent();
           subscription = _mapper.Map<WebhookSubscription>(webhookSubscriptionCreateDto);

           subscription.Secret = Guid.NewGuid().ToString();
           subscription.WebhookPublisher = "PanAus";
           try
           {
               _context.WebhookSubscriptions.Add(subscription);
               await _context.SaveChangesAsync();
           }
           catch(Exception ex)
           {
               return BadRequest(ex.Message);
           }

           var webhookSubscriptionReadDto = _mapper.Map<WebhookSubscriptionReadDto>(subscription);

           return CreatedAtRoute(nameof(GetSubscriptionBySecret), new { secret = webhookSubscriptionReadDto.Secret}, webhookSubscriptionReadDto);

        }
        
    }
    