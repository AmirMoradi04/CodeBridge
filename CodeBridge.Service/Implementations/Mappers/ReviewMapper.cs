using CodeBridge.Domain.Entities;
using CodeBridge.Service.DTOs.Review;

namespace CodeBridge.Service.Implementations.Mappers;

internal static class ReviewMapper
{
    public static ReviewDto ToDto(Review review) => new()
    {
        Id = review.Id,
        ProjectId = review.ProjectId,
        ProjectTitle = review.Project?.Title,
        ContractId = review.ContractId,
        ReviewerId = review.ReviewerId,
        ReviewerName = review.Reviewer is null ? null : FullName(review.Reviewer.FirstName, review.Reviewer.LastName),
        TargetUserId = review.TargetUserId,
        TargetUserName = review.TargetUser is null ? null : FullName(review.TargetUser.FirstName, review.TargetUser.LastName),
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAt = review.CreatedAt
    };

    private static string FullName(string firstName, string lastName)
        => $"{firstName} {lastName}".Trim();
}
