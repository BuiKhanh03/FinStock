using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    public interface IVnPayService 
    {
        string CreatePaymentUrl(PaymentInformation model, HttpContext context);
        PaymentResponse PaymentExecute(IQueryCollection collections);
    }
}