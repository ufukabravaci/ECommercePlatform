using ECommercePlatform.Application.Reviews;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Reviews;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Reviews;

public class DeleteReviewCommandHandlerTests
{
    private readonly Mock<IRepository<Review>> _reviewRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly DeleteReviewCommandHandler _handler;

    public DeleteReviewCommandHandlerTests()
    {
        _reviewRepoMock = new Mock<IRepository<Review>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userContextMock = new Mock<IUserContext>();

        _handler = new DeleteReviewCommandHandler(_reviewRepoMock.Object, _unitOfWorkMock.Object, _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_ReviewNotFound()
    {
        _reviewRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Review)null!);

        var command = new DeleteReviewCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Yorum bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserIsNotOwner_And_NotAdmin()
    {
        var ownerId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();

        var review = new Review(Guid.NewGuid(), ownerId, Guid.NewGuid(), 5, "Test");

        _reviewRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(review);

        // Sisteme giren kişi başkası
        _userContextMock.Setup(x => x.GetUserId()).Returns(anotherUserId);

        // Ve admin yetkisi YOK
        _userContextMock.Setup(x => x.HasPermissionAsync(PermissionConsts.ManageReview)).ReturnsAsync(false);

        var command = new DeleteReviewCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bu yorumu silmeye yetkiniz yok.");
    }

    [Fact]
    public async Task Handle_ShouldDeleteReview_When_UserIsOwner()
    {
        var ownerId = Guid.NewGuid();
        var review = new Review(Guid.NewGuid(), ownerId, Guid.NewGuid(), 5, "Test");

        _reviewRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(review);

        // Sisteme giren kişi, yorumun SAHİBİ
        _userContextMock.Setup(x => x.GetUserId()).Returns(ownerId);
        _userContextMock.Setup(x => x.HasPermissionAsync(PermissionConsts.ManageReview)).ReturnsAsync(false);

        var command = new DeleteReviewCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Yorum başarıyla silindi.");

        review.IsDeleted.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDeleteReview_When_UserIsAdmin()
    {
        var ownerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var review = new Review(Guid.NewGuid(), ownerId, Guid.NewGuid(), 5, "Test");

        _reviewRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(review);

        // Sisteme giren kişi yorum sahibi DEĞİL ama ADMİN
        _userContextMock.Setup(x => x.GetUserId()).Returns(adminId);
        _userContextMock.Setup(x => x.HasPermissionAsync(PermissionConsts.ManageReview)).ReturnsAsync(true);

        var command = new DeleteReviewCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        review.IsDeleted.Should().BeTrue();
    }
}
