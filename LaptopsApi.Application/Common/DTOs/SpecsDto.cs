namespace LaptopsApi.Application.Common.DTOs
{
    public sealed class SpecsDto
    {
        public Guid SpecsId { get; set; }

        public string? Processor { get; set; }
        public int? RamGb { get; set; }
        public string? RamType { get; set; }
        public int? StorageGb { get; set; }
        public string? StorageType { get; set; }
        public string? StorageInterface { get; set; }
        public string? Gpu { get; set; }
        public string? GpuType { get; set; }
        public decimal? BatteryCapacityWh { get; set; }
        public decimal? BatteryLifeHours { get; set; }
        public string? CoolingSystem { get; set; }
        public string? DisplayResolution { get; set; }
        public int? DisplayRefreshRate { get; set; }
        public string? PortsDescription { get; set; }
        public decimal? WeightKg { get; set; }
        public string? Dimensions { get; set; }
        public string? OperatingSystem { get; set; }
        public int? WarrantyMonths { get; set; }
        public string? AdditionalFeatures { get; set; }
    }
}
