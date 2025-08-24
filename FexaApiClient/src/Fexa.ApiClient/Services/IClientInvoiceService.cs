using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface IClientInvoiceService
{
    Task<PagedResponse<ClientInvoice>> GetInvoicesAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<ClientInvoice> GetInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task<PagedResponse<ClientInvoice>> GetInvoicesByWorkOrderAsync(int workOrderId, QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<ClientInvoice>> GetInvoicesByWorkOrdersAsync(int[] workOrderIds, QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<ClientInvoice>> GetInvoicesByVendorAsync(int vendorId, QueryParameters? parameters = null, CancellationToken cancellationToken = default);
}