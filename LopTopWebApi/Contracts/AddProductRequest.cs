using System.ComponentModel.DataAnnotations;

namespace LopTopWebApi.Contracts
{
    public sealed class AddProductRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public string Brand { get; set; } = string.Empty;

        [Range(1, 50)]
        public decimal ScreenSize { get; set; }

        public string? Description { get; set; }

        public SpecsRequest? Specs { get; set; }
    }

    public sealed class AddSpecsRequest
    {
        public string? Processor { get; set; }
        public int? RamGb { get; set; }
        public string? RamType { get; set; }
        public int? StorageGb { get; set; }
        public string? StorageType { get; set; }
        public string? StorageInterface { get; set; }
        public string? Gpu { get; set; }
        public string? GpuType { get; set; }
        public int? BatteryCapacityWh { get; set; }
        public int? BatteryLifeHours { get; set; }
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
