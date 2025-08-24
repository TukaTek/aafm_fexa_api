using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class Subcontractor
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("entity_id")]
    public int? EntityId { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonPropertyName("start_date")]
    public DateTime? StartDate { get; set; }
    
    [JsonPropertyName("end_date")]
    public DateTime? EndDate { get; set; }
    
    [JsonPropertyName("active")]
    public bool? Active { get; set; }
    
    [JsonPropertyName("facility_id")]
    public int? FacilityId { get; set; }
    
    [JsonPropertyName("created_by")]
    public int? CreatedBy { get; set; }
    
    [JsonPropertyName("updated_by")]
    public int? UpdatedBy { get; set; }
    
    [JsonPropertyName("custom_field_values")]
    public Dictionary<string, object>? CustomFieldValues { get; set; }
    
    [JsonPropertyName("organization_entity_id")]
    public int? OrganizationEntityId { get; set; }
    
    [JsonPropertyName("ivr_id")]
    public string? IvrId { get; set; }
    
    [JsonPropertyName("import_id")]
    public int? ImportId { get; set; }
    
    [JsonPropertyName("import_date")]
    public DateTime? ImportDate { get; set; }
    
    [JsonPropertyName("auto_accept")]
    public string? AutoAccept { get; set; }
    
    [JsonPropertyName("compliance_requirement_id")]
    public int? ComplianceRequirementId { get; set; }
    
    [JsonPropertyName("compliance_requirement_met")]
    public bool? ComplianceRequirementMet { get; set; }
    
    [JsonPropertyName("contact_domain")]
    public string? ContactDomain { get; set; }
    
    [JsonPropertyName("overall_score")]
    public decimal? OverallScore { get; set; }
    
    [JsonPropertyName("total_workorder_count")]
    public int? TotalWorkorderCount { get; set; }
    
    [JsonPropertyName("assignable")]
    public bool? Assignable { get; set; }
    
    [JsonPropertyName("top_organization_role_id")]
    public int? TopOrganizationRoleId { get; set; }
    
    [JsonPropertyName("export_date")]
    public DateTime? ExportDate { get; set; }
    
    [JsonPropertyName("export_batch_id")]
    public int? ExportBatchId { get; set; }
    
    [JsonPropertyName("account_manager_id")]
    public int? AccountManagerId { get; set; }
    
    [JsonPropertyName("invoice_discount_schedule_id")]
    public int? InvoiceDiscountScheduleId { get; set; }
    
    [JsonPropertyName("discount_invoicing")]
    public bool? DiscountInvoicing { get; set; }
    
    [JsonPropertyName("opts_out_of_mass_dispatches")]
    public bool? OptsOutOfMassDispatches { get; set; }
    
    [JsonPropertyName("distributor")]
    public bool? Distributor { get; set; }
    
    [JsonPropertyName("organization")]
    public Organization? Organization { get; set; }
    
    [JsonPropertyName("default_dispatch_address")]
    public Address? DefaultDispatchAddress { get; set; }
    
    [JsonPropertyName("default_billing_address")]
    public Address? DefaultBillingAddress { get; set; }
    
    [JsonPropertyName("default_shipping_address")]
    public Address? DefaultShippingAddress { get; set; }
    
    [JsonPropertyName("object_state")]
    public ObjectState? ObjectState { get; set; }
}

public class Organization
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("oauth_application_id")]
    public int? OauthApplicationId { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonPropertyName("created_by")]
    public int? CreatedBy { get; set; }
    
    [JsonPropertyName("updated_by")]
    public int? UpdatedBy { get; set; }
    
    [JsonPropertyName("term_id")]
    public int? TermId { get; set; }
    
    [JsonPropertyName("parent_id")]
    public int? ParentId { get; set; }
    
    [JsonPropertyName("custom_field_values")]
    public Dictionary<string, object>? CustomFieldValues { get; set; }
    
    [JsonPropertyName("ein")]
    public string? Ein { get; set; }
    
    [JsonPropertyName("tax_classification_id")]
    public int? TaxClassificationId { get; set; }
    
    [JsonPropertyName("accounting_id")]
    public string? AccountingId { get; set; }
    
    [JsonPropertyName("import_id")]
    public int? ImportId { get; set; }
    
    [JsonPropertyName("import_date")]
    public DateTime? ImportDate { get; set; }
    
    [JsonPropertyName("is_1099")]
    public bool? Is1099 { get; set; }
    
    [JsonPropertyName("taxable")]
    public bool? Taxable { get; set; }
}

public class Address
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("entity_id")]
    public int? EntityId { get; set; }
    
    [JsonPropertyName("address_name")]
    public string? AddressName { get; set; }
    
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }
    
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }
    
    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }
    
    [JsonPropertyName("company")]
    public string? Company { get; set; }
    
    [JsonPropertyName("dba")]
    public string? Dba { get; set; }
    
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("department")]
    public string? Department { get; set; }
    
    [JsonPropertyName("address1")]
    public string? Address1 { get; set; }
    
    [JsonPropertyName("address2")]
    public string? Address2 { get; set; }
    
    [JsonPropertyName("address3")]
    public string? Address3 { get; set; }
    
    [JsonPropertyName("address4")]
    public string? Address4 { get; set; }
    
    [JsonPropertyName("address5")]
    public string? Address5 { get; set; }
    
    [JsonPropertyName("city")]
    public string? City { get; set; }
    
    [JsonPropertyName("state")]
    public string? State { get; set; }
    
    [JsonPropertyName("province")]
    public string? Province { get; set; }
    
    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }
    
    [JsonPropertyName("country")]
    public string? Country { get; set; }
    
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
    
    [JsonPropertyName("extension")]
    public string? Extension { get; set; }
    
    [JsonPropertyName("fax")]
    public string? Fax { get; set; }
    
    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }
    
    [JsonPropertyName("emergency")]
    public string? Emergency { get; set; }
    
    [JsonPropertyName("alternate_phone")]
    public string? AlternatePhone { get; set; }
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    
    [JsonPropertyName("communication_method")]
    public string? CommunicationMethod { get; set; }
    
    [JsonPropertyName("website")]
    public string? Website { get; set; }
    
    [JsonPropertyName("active")]
    public bool? Active { get; set; }
    
    [JsonPropertyName("long")]
    public string? Longitude { get; set; }
    
    [JsonPropertyName("lat")]
    public string? Latitude { get; set; }
    
    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }
    
    [JsonPropertyName("manager_type")]
    public string? ManagerType { get; set; }
    
    [JsonPropertyName("formerly_known_as")]
    public string? FormerlyKnownAs { get; set; }
    
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
    
    [JsonPropertyName("default_address")]
    public bool? DefaultAddress { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonPropertyName("facility_id")]
    public int? FacilityId { get; set; }
    
    [JsonPropertyName("center_id")]
    public int? CenterId { get; set; }
    
    [JsonPropertyName("created_by")]
    public int? CreatedBy { get; set; }
    
    [JsonPropertyName("updated_by")]
    public int? UpdatedBy { get; set; }
    
    [JsonPropertyName("custom_field_values")]
    public Dictionary<string, object>? CustomFieldValues { get; set; }
    
    [JsonPropertyName("same_as_facility")]
    public bool? SameAsFacility { get; set; }
    
    [JsonPropertyName("main_contact_id")]
    public int? MainContactId { get; set; }
    
    [JsonPropertyName("language")]
    public string? Language { get; set; }
    
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
    
    [JsonPropertyName("import_id")]
    public int? ImportId { get; set; }
    
    [JsonPropertyName("import_date")]
    public DateTime? ImportDate { get; set; }
}
