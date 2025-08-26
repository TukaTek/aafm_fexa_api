using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class Location
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    
    [JsonPropertyName("identifier")]
    public string? Identifier { get; set; }
    
    [JsonPropertyName("center_id")]
    public int? CenterId { get; set; }
    
    [JsonPropertyName("district_id")]
    public int? DistrictId { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonPropertyName("created_by")]
    public int? CreatedBy { get; set; }
    
    [JsonPropertyName("updated_by")]
    public int? UpdatedBy { get; set; }
    
    [JsonPropertyName("occupied_by")]
    public int? OccupiedBy { get; set; }
    
    [JsonPropertyName("owned_by")]
    public int? OwnedBy { get; set; }
    
    [JsonPropertyName("facility_code")]
    public string? FacilityCode { get; set; }
    
    [JsonPropertyName("brand_id")]
    public int? BrandId { get; set; }
    
    [JsonPropertyName("custom_field_values")]
    public Dictionary<string, object>? CustomFieldValues { get; set; }
    
    [JsonPropertyName("flag")]
    public bool? Flag { get; set; }
    
    [JsonPropertyName("remodel_date")]
    public DateTime? RemodelDate { get; set; }
    
    [JsonPropertyName("move_date")]
    public DateTime? MoveDate { get; set; }
    
    [JsonPropertyName("sq_footage")]
    public int? SqFootage { get; set; }
    
    [JsonPropertyName("warranty_start_date")]
    public DateTime? WarrantyStartDate { get; set; }
    
    [JsonPropertyName("warranty_end_date")]
    public DateTime? WarrantyEndDate { get; set; }
    
    [JsonPropertyName("open_date")]
    public DateTime? OpenDate { get; set; }
    
    [JsonPropertyName("close_date")]
    public DateTime? CloseDate { get; set; }
    
    [JsonPropertyName("cost_center")]
    public string? CostCenter { get; set; }
    
    [JsonPropertyName("import_id")]
    public object? ImportId { get; set; }  // Can be int, string, or null
    
    [JsonPropertyName("import_date")]
    public DateTime? ImportDate { get; set; }
    
    [JsonPropertyName("department_id")]
    public int? DepartmentId { get; set; }
    
    [JsonPropertyName("location_type")]
    public string? LocationType { get; set; }
    
    [JsonPropertyName("delivery_instruction")]
    public string? DeliveryInstruction { get; set; }
    
    [JsonPropertyName("export_date")]
    public DateTime? ExportDate { get; set; }
    
    [JsonPropertyName("export_batch_id")]
    public int? ExportBatchId { get; set; }
    
    [JsonPropertyName("object_assets")]
    public List<object>? ObjectAssets { get; set; }
    
    [JsonPropertyName("end_user_customer_role")]
    public EndUserCustomerRole? EndUserCustomerRole { get; set; }
    
    [JsonPropertyName("store_address")]
    public StoreAddress? StoreAddress { get; set; }
    
    [JsonPropertyName("facility_manager_roles")]
    public List<object>? FacilityManagerRoles { get; set; }
    
    [JsonPropertyName("district_manager_roles")]
    public List<object>? DistrictManagerRoles { get; set; }
    
    [JsonPropertyName("store_manager_roles")]
    public List<object>? StoreManagerRoles { get; set; }
    
    [JsonPropertyName("regional_manager_roles")]
    public List<object>? RegionalManagerRoles { get; set; }
    
    [JsonPropertyName("store_districts")]
    public List<object>? StoreDistricts { get; set; }
}

public class EndUserCustomerRole
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("default_address")]
    public EndUserAddress? DefaultAddress { get; set; }
}

public class EndUserAddress
{
    [JsonPropertyName("company")]
    public string? Company { get; set; }
    
    [JsonPropertyName("dba")]
    public string? Dba { get; set; }
}

public class StoreAddress
{
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
    
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    
    [JsonPropertyName("long")]
    public string? Longitude { get; set; }
    
    [JsonPropertyName("lat")]
    public string? Latitude { get; set; }
    
    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }
    
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
    
    [JsonPropertyName("default_address")]
    public bool? DefaultAddress { get; set; }
    
    [JsonPropertyName("main_contact_id")]
    public int? MainContactId { get; set; }
    
    [JsonPropertyName("language")]
    public string? Language { get; set; }
    
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}

// Response wrapper
internal class LocationsResponse
{
    [JsonPropertyName("stores")]
    public List<Location>? Stores { get; set; }
}

// Single location response
internal class SingleLocationResponse  
{
    [JsonPropertyName("stores")]  // Note: API returns "stores" not "store" for single location
    public Location? Store { get; set; }
}