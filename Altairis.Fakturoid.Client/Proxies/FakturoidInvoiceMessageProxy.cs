using System;
using System.Collections.Generic;
using System.Text;

namespace Altairis.Fakturoid.Client.Proxies;

/// <summary>
/// Proxy class for sending messages related to invoices
/// </summary>
public class FakturoidInvoiceMessageProxy(FakturoidContext context) : FakturoidEntityProxy(context) {
    /// <summary>
    /// Sends message to the invoice's subject
    /// </summary>
    /// <param name="invoiceId">Identification of the invoice</param>
    /// <param name="message">Optional message, <see cref="https://www.fakturoid.cz/api/v3/invoice-messages"/></param>
    /// <returns></returns>
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public async Task SendAsync(int invoiceId, FakturoidInvoiceMessage? message = null) {
        var c = await this.Context.GetHttpClientAsync();
        var r = await c.FakturoidPostAsJsonAsync(string.Format("invoices/{0}/message.json", invoiceId), message ?? new());
        r.EnsureFakturoidSuccess();
    }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
}
