using LaptopsApi.Application.Commands;
using LopTopWebApi.Domain.Entities;
using LopTopWebApi.Domain.Interfaces;
using MediatR;

namespace LaptopsApi.Infrastructure.Handlers
{
    public class AddProductCommandHandler : IRequestHandler<AddProductCommand, Guid>
    {
        private readonly IProductRepository _productRepository;

        public AddProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Guid> Handle(AddProductCommand request, CancellationToken ct)
        {
            Specs? specs = null;

            if (request.Specs is not null)
            {
                specs = Specs.Create(
                    request.Specs.Processor,
                    request.Specs.RamGb,
                    request.Specs.RamType,
                    request.Specs.StorageGb,
                    request.Specs.StorageType,
                    request.Specs.StorageInterface,
                    request.Specs.Gpu,
                    request.Specs.GpuType,
                    request.Specs.BatteryCapacityWh,
                    request.Specs.BatteryLifeHours,
                    request.Specs.CoolingSystem,
                    request.Specs.DisplayResolution,
                    request.Specs.DisplayRefreshRate,
                    request.Specs.PortsDescription,
                    request.Specs.WeightKg,
                    request.Specs.Dimensions,
                    request.Specs.OperatingSystem,
                    request.Specs.WarrantyMonths,
                    request.Specs.AdditionalFeatures
                );
            }

            var product = Product.Create(
                request.Name,
                request.Price,
                request.Brand,
                request.ScreenSize,
                request.Description,
                request.UserId
            );

            if (specs is not null)
            {
                product.AttachSpecs(specs);
            }

            await _productRepository.AddAsync(product, ct);
            await _productRepository.SaveChangesAsync(ct);

            return product.ProductId;
        }
    }
}
