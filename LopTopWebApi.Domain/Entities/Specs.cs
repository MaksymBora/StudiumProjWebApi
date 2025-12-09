namespace LopTopWebApi.Domain.Entities
{
    public class Specs
    {
        public Guid SpecsId { get; private set; } = Guid.NewGuid();

        public string? Processor { get; private set; }
        public int? RamGb { get; private set; }
        public string? RamType { get; private set; }
        public int? StorageGb { get; private set; }
        public string? StorageType { get; private set; }
        public string? StorageInterface { get; private set; }
        public string? Gpu { get; private set; }
        public string? GpuType { get; private set; }
        public decimal? BatteryCapacityWh { get; private set; }
        public decimal? BatteryLifeHours { get; private set; }
        public string? CoolingSystem { get; private set; }
        public string? DisplayResolution { get; private set; }
        public int? DisplayRefreshRate { get; private set; }
        public string? PortsDescription { get; private set; }
        public decimal? WeightKg { get; private set; }
        public string? Dimensions { get; private set; }
        public string? OperatingSystem { get; private set; }
        public int? WarrantyMonths { get; private set; }
        public string? AdditionalFeatures { get; private set; }

        public virtual Product? Product { get; private set; }

        public static Specs Create(
            string? processor,
            int? ramGb,
            string? ramType,
            int? storageGb,
            string? storageType,
            string? storageInterface,
            string? gpu,
            string? gpuType,
            int? batteryCapacityWh,
            int? batteryLifeHours,
            string? coolingSystem,
            string? displayResolution,
            int? displayRefreshRate,
            string? portsDescription,
            decimal? weightKg,
            string? dimensions,
            string? operatingSystem,
            int? warrantyMonths,
            string? additionalFeatures)
        {
            return new Specs
            {
                Processor = processor,
                RamGb = ramGb,
                RamType = ramType,
                StorageGb = storageGb,
                StorageType = storageType,
                StorageInterface = storageInterface,
                Gpu = gpu,
                GpuType = gpuType,
                BatteryCapacityWh = batteryCapacityWh,
                BatteryLifeHours = batteryLifeHours,
                CoolingSystem = coolingSystem,
                DisplayResolution = displayResolution,
                DisplayRefreshRate = displayRefreshRate,
                PortsDescription = portsDescription,
                WeightKg = weightKg,
                Dimensions = dimensions,
                OperatingSystem = operatingSystem,
                WarrantyMonths = warrantyMonths,
                AdditionalFeatures = additionalFeatures
            };
        }
    }
}
