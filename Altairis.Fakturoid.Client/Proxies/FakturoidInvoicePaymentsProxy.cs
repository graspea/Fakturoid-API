using System;
using System.Collections.Generic;
using System.Text;

namespace Altairis.Fakturoid.Client.Proxies;

/// <summary>
/// Proxy class for invoice payments
/// </summary>
public class FakturoidInvoicePaymentsProxy(FakturoidContext context) : FakturoidEntityProxy(context) {
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    /// <summary>
    /// Create payment for the invoice
    /// </summary>
    /// <param name="invoiceId">Identification of the invoice</param>
    /// <param name="payment"><see cref="https://www.fakturoid.cz/api/v3/invoice-payments"/></param>
    /// <returns></returns>
    public async Task<int> CreateAsync(int invoiceId, FakturoidInvoicePayment? payment = null)
        => await this.CreateEntityAsync(string.Format("invoices/{0}/payments.json", invoiceId), payment ?? new());

    /// <summary>
    /// Delete payment for the invoice
    /// </summary>
    /// <param name="invoiceId">Identification of the invoice</param>
    /// <param name="paymentId">Identification of the payment</param>
    /// <returns></returns>
    public async Task DeleteAsync(int invoiceId, int paymentId)
        => await this.DeleteSingleEntityAsync(string.Format("invoices/{0}/payments/{1}.json", invoiceId, paymentId));
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
}
