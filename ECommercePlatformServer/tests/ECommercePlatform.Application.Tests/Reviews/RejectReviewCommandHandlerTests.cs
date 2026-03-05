using ECommercePlatform.Application.Reviews;
using ECommercePlatform.Domain.Reviews;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Reviews;

public class RejectReviewCommandHandlerTests
{
    private readonly Mock<IRepository<Review>> _reviewRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RejectReviewCommandHandler _handler;

    public RejectReviewCommandHandlerTests()
    {
        _reviewRepoMock = new Mock<IRepository<Review>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RejectReviewCommandHandler(_reviewRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_ReviewNotFound()
    {
        _reviewRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Review)null!);

        var command = new RejectReviewCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Yorum bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldRejectReview_When_Valid()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, "Fena değil");

        _reviewRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(review);

        var command = new RejectReviewCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Contain("yayından kaldırıldı");

        // Reject() metodu Entity'nin base.Delete() metodunu çağırıyor.
        review.IsDeleted.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
