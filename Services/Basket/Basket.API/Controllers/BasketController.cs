using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _repository;
        private readonly DiscountGrpcService _discountGrpcService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;
        private readonly ILogger<BasketController> _logger;

        public BasketController(IBasketRepository repository,
            DiscountGrpcService discountGrpcService,
            IPublishEndpoint publishEndpoint,
            ILogger<BasketController> logger,
            IMapper mapper)
        {
            _repository = repository;
            _discountGrpcService = discountGrpcService;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("{userName}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
        {
            var basket = await _repository.GetBasket(userName);
            return Ok(basket ?? new ShoppingCart(userName));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
        {
            foreach (var item in basket.ShoppingCartItems)
            {
                var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
                item.Price -= coupon.Amount;
            }

            return Ok(await _repository.UpdateBasket(basket));
        }

        [HttpDelete("{userName}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBasket(string userName)
        {
            await _repository.DeleteBasket(userName);
            return Ok();
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            var basket = new ShoppingCart();
            try
            {
                // get existing basket with total price 
                // Create basketCheckoutEvent -- Set TotalPrice on basketCheckout eventMessage
                // send checkout event to rabbitmq
                // remove the basket

                // get existing basket with total price
                basket = await _repository.GetBasket(basketCheckout.UserName);
                _logger.LogInformation(basket.UserName);

                if (basket == null)
                {
                    return BadRequest();
                }

                // send checkout event to rabbitmq
                var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
                eventMessage.TotalPrice = basket.TotalPrice;
                await _publishEndpoint.Publish(eventMessage);

                // remove the basket
                await _repository.DeleteBasket(basket.UserName);

                return Accepted();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Source);
                return BadRequest(new ErrorCLass
                {
                    Message = ex.Message,
                    Result = basket
                });
            }
        }
    }

    public class ErrorCLass
    {
        public string Message { get; set; }
        public object Result { get; set; }
    }
}