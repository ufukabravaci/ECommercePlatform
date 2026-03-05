using ECommercePlatform.Application.Reviews;
using ECommercePlatform.Domain.Reviews;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Reviews;

public class ApproveReviewCommandHandlerTests
{
    private readonly Mock<IRepository<Review>> _reviewRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ApproveReviewCommandHandler _handler;

    public ApproveReviewCommandHandlerTests()
    {
        _reviewRepoMock = new Mock<IRepository<Review>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new ApproveReviewCommandHandler(_reviewRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_ReviewNotFound()
    {
        _reviewRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Review)null!);

        var command = new ApproveReviewCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Yorum bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldApproveReview_When_Valid()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5, "Harika bir ürün");
        // Yeni oluşturulan yorumun IsApproved = false'tur.

        _reviewRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(review);

        var command = new ApproveReviewCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Yorum onaylandı ve yayına alındı.");

        // Onaylandığını domain davranışı üzerinden doğruluyoruz.
        review.IsApproved.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
