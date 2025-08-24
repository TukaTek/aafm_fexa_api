using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface ISubcontractorService
{
    Task<PagedResponse<Subcontractor>> GetSubcontractorsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<Subcontractor> GetSubcontractorAsync(int subcontractorId, CancellationToken cancellationToken = default);
    Task<PagedResponse<Subcontractor>> GetActiveSubcontractorsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<Subcontractor>> GetSubcontractorsByFacilityAsync(int facilityId, QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<Subcontractor>> GetSubcontractorsByRegionAsync(int regionId, QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<Subcontractor>> GetAssignableSubcontractorsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    
    // Pagination helpers - fetch all pages
    Task<List<Subcontractor>> GetAllSubcontractorsAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default);
    Task<List<Subcontractor>> GetAllActiveSubcontractorsAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default);
    Task<List<Subcontractor>> GetAllAssignableSubcontractorsAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default);
    
    // CRUD operations
    Task<Subcontractor> CreateSubcontractorAsync(CreateSubcontractorRequest request, CancellationToken cancellationToken = default);
    Task<Subcontractor> UpdateSubcontractorAsync(int subcontractorId, UpdateSubcontractorRequest request, CancellationToken cancellationToken = default);
    Task DeleteSubcontractorAsync(int subcontractorId, CancellationToken cancellationToken = default);
}

public class CreateSubcontractorRequest
{
    public int? EntityId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? Active { get; set; }
    public int? FacilityId { get; set; }
    public int? OrganizationEntityId { get; set; }
    public string? IvrId { get; set; }
    public string? AutoAccept { get; set; }
    public int? ComplianceRequirementId { get; set; }
    public bool? ComplianceRequirementMet { get; set; }
    public string? ContactDomain { get; set; }
    public bool? Assignable { get; set; }
    public bool? DiscountInvoicing { get; set; }
    public bool? OptsOutOfMassDispatches { get; set; }
    public bool? Distributor { get; set; }
    public Dictionary<string, object>? CustomFieldValues { get; set; }
}

public class UpdateSubcontractorRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? Active { get; set; }
    public int? FacilityId { get; set; }
    public string? IvrId { get; set; }
    public string? AutoAccept { get; set; }
    public int? ComplianceRequirementId { get; set; }
    public bool? ComplianceRequirementMet { get; set; }
    public string? ContactDomain { get; set; }
    public bool? Assignable { get; set; }
    public bool? DiscountInvoicing { get; set; }
    public bool? OptsOutOfMassDispatches { get; set; }
    public bool? Distributor { get; set; }
    public Dictionary<string, object>? CustomFieldValues { get; set; }
}
