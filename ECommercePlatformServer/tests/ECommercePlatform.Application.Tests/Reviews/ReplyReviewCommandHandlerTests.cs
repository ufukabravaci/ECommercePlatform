using ECommercePlatform.Application.Reviews;
using ECommercePlatform.Domain.Reviews;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Reviews;

public class ReplyReviewCommandHandlerTests
{
    private readonly Mock<IRepository<Review>> _reviewRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ReplyReviewCommandHandler _handler;

    public ReplyReviewCommandHandlerTests()
    {
        _reviewRepoMock = new Mock<IRepository<Review>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new ReplyReviewCommandHandler(_reviewRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_ReviewNotFound()
    {
        _reviewRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Review)null!);

        var command = new ReplyReviewCommand(Guid.NewGuid(), "Teşekkürler");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Yorum bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldSetReplyAndApprove_When_Valid()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5, "Harika bir ürün");
        // Varsayılan IsApproved = false;

        _reviewRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(review);

        var command = new ReplyReviewCommand(Guid.NewGuid(), "Teşekkür ederiz!");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Yoruma cevap verildi.");

        // Domain Logic Testi
        review.SellerReply.Should().Be("Teşekkür ederiz!");
        review.SellerRepliedAt.Should().NotBeNull();
        review.IsApproved.Should().BeTrue(); // Domain "Reply" metodu otomatik true yapar.

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
