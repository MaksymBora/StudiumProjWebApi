using FluentAssertions;
using Moq;
using LaptopsApi.Application.Commands;
using LaptopsApi.Infrastructure.Handlers;
using LopTopWebApi.Domain.Interfaces;

public class AddProductRatingCommandHandlerTests
{
    [Fact]
    public async Task Adds_rating_successfully_when_not_rated_before()
    {
        var pid = Guid.NewGuid();
        var uid = Guid.NewGuid();
        var newReviewId = Guid.NewGuid();

        var repo = new Mock<IReviewRepository>(MockBehavior.Strict);
        repo.Setup(r => r.HasUserRatedProductAsync(pid, uid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repo.Setup(r => r.AddRootReviewAsync(pid, uid, 5, "great", It.IsAny<CancellationToken>()))
            .ReturnsAsync(newReviewId);

        var handler = new AddProductRatingCommandHandler(repo.Object);
        var cmd = new AddProductRatingCommand { ProductId = pid, UserId = uid, Rating = 5, Comment = "great" };

        var id = await handler.Handle(cmd, CancellationToken.None);

        id.Should().Be(newReviewId);
        repo.VerifyAll();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task Throws_when_rating_out_of_range(int badRating)
    {
        var pid = Guid.NewGuid();
        var uid = Guid.NewGuid();

        var repo = new Mock<IReviewRepository>(MockBehavior.Loose);
        var handler = new AddProductRatingCommandHandler(repo.Object);
        var cmd = new AddProductRatingCommand { ProductId = pid, UserId = uid, Rating = badRating };

        var act = async () => await handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
                 .WithMessage("*Rating must be between 1 and 5*");
    }

    [Fact]
    public async Task Throws_when_user_already_rated_this_product()
    {
        var pid = Guid.NewGuid();
        var uid = Guid.NewGuid();

        var repo = new Mock<IReviewRepository>(MockBehavior.Strict);
        repo.Setup(r => r.HasUserRatedProductAsync(pid, uid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new AddProductRatingCommandHandler(repo.Object);
        var cmd = new AddProductRatingCommand { ProductId = pid, UserId = uid, Rating = 4, Comment = "ok" };

        var act = async () => await handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*already rated*");
        repo.VerifyAll();
    }
}
