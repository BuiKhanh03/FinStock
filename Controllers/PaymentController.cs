using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;
        public PaymentController(IVnPayService vnPayService, IMomoService momoService)
        {
            _vnPayService = vnPayService;
            _momoService = momoService;
        }

        [HttpGet("vnpay")]
        public IActionResult PaymentCallbackVnPay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            return Json(response);
        }

        [HttpGet("momo")]
        public IActionResult PaymentCallbackMoMo()
        {
            var response = _momoService.PaymentExecuteAsync(Request.Query);
            return Json(response);
        }

        [HttpPost("vnpay")]
        [EnableCors("AllowAllOrigins")]
        public IActionResult CreatePaymentVnPayUrl([FromBody] PaymentInformation model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }
        [HttpPost("momo")]
        [EnableCors("AllowAllOrigins")]
        public async Task<IActionResult> CreatePaymentMoMoUrl([FromBody] OrderInfoModel model)
        {
            var url = await _momoService.CreatePaymentAsync(model);

            return Redirect(url.PayUrl);
        }

    }
}