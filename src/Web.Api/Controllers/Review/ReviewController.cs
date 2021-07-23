﻿using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Extensions;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Parameters.Review;
using Ulearn.Web.Api.Models.Responses.Exercise;

namespace Ulearn.Web.Api.Controllers.Review
{
	[Route("reviews")]
	public class ReviewController : BaseController
	{
		private readonly ISlideCheckingsRepo slideCheckingsRepo;
		private readonly IUserSolutionsRepo userSolutionsRepo;
		private readonly IGroupAccessesRepo groupAccessesRepo;

		public ReviewController(ICourseStorage courseStorage, UlearnDb db, IUsersRepo usersRepo,
			ISlideCheckingsRepo slideCheckingsRepo,
			IUserSolutionsRepo userSolutionsRepo,
			IGroupAccessesRepo groupAccessesRepo)
			: base(courseStorage, db, usersRepo)
		{
			this.slideCheckingsRepo = slideCheckingsRepo;
			this.userSolutionsRepo = userSolutionsRepo;
			this.groupAccessesRepo = groupAccessesRepo;
		}

		/// <summary>
		/// Добавить ревью к решению
		/// </summary>
		[HttpPost]
		[Authorize]
		[SwaggerResponse((int)HttpStatusCode.RequestEntityTooLarge, "Your comment is too large")]
		public async Task<ActionResult<ReviewInfo>> AddReview([FromQuery] int submissionId, [FromBody] ReviewCreateParameters parameters)
		{
			var submission = await userSolutionsRepo.FindSubmissionById(submissionId);

			if (submission == null)
				return NotFound(new ErrorResponse($"Submission {submissionId} not found"));

			if (!await groupAccessesRepo.CanInstructorViewStudentAsync(UserId, submission.UserId))
				return StatusCode((int)HttpStatusCode.Forbidden, "You don't have access to view this submission");

			if (parameters.StartLine > parameters.FinishLine || (parameters.StartLine == parameters.FinishLine && parameters.StartPosition > parameters.FinishPosition))
			{
				var tmp = parameters.StartLine;
				parameters.StartLine = parameters.FinishLine;
				parameters.FinishLine = tmp;

				tmp = parameters.StartPosition;
				parameters.StartPosition = parameters.FinishPosition;
				parameters.FinishPosition = tmp;
			}

			var review = await slideCheckingsRepo.AddExerciseCodeReview(
				submission,
				UserId,
				parameters.StartLine,
				parameters.StartPosition,
				parameters.FinishLine,
				parameters.FinishPosition,
				parameters.Text);

			return ReviewInfo.Build(review, null, false);
		}

		/// <summary>
		/// Удалить ревью
		/// </summary>
		[HttpDelete]
		[Authorize]
		public async Task<ActionResult> DeleteReview([FromQuery] int submissionId, [FromQuery] int reviewId)
		{
			var submission = await userSolutionsRepo.FindSubmissionById(submissionId);

			if (submission == null)
				return NotFound(new ErrorResponse($"Submission {submissionId} not found"));

			/* check below will return false if user is not sys admin and/or can't view student submission */
			if (!await groupAccessesRepo.CanInstructorViewStudentAsync(UserId, submission.UserId))
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You don't have access to view this submission"));

			var reviews = await userSolutionsRepo.FindSubmissionReviewsBySubmissionIdNoTracking(submissionId);
			var review = reviews.FirstOrDefault(r => r.Id == reviewId);

			if (review == null)
				return NotFound(new ErrorResponse($"Review {reviewId} not found"));

			/* instructor can't delete anyone else review, except ulearn bot */
			if (!review.Author.IsUlearnBot() && review.Author.Id != UserId)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You don't have access to delete this review"));

			await slideCheckingsRepo.DeleteExerciseCodeReview(review);

			return Ok(new SuccessResponseWithMessage($"Review {reviewId} successfully deleted"));
		}

		/// <summary>
		/// Изменить содержание ревью
		/// </summary>
		[HttpPatch]
		[Authorize]
		public async Task<ActionResult<ReviewInfo>> EditReview([FromQuery] int submissionId, [FromQuery] int reviewId, [FromBody] string text)
		{
			var submission = await userSolutionsRepo.FindSubmissionById(submissionId);

			if (submission == null)
				return NotFound(new ErrorResponse($"Submission {submissionId} not found"));

			/* check below will return false if user is not sys admin and/or can't view student submission */
			if (!await groupAccessesRepo.CanInstructorViewStudentAsync(UserId, submission.UserId))
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You don't have access to view this submission"));

			var reviews = await userSolutionsRepo.FindSubmissionReviewsBySubmissionIdNoTracking(submissionId);
			var review = reviews.FirstOrDefault(r => r.Id == reviewId);

			if (review == null)
				return NotFound(new ErrorResponse($"Review {reviewId} not found"));

			/* instructor can't edit anyone else review */
			if (review.Author.Id != UserId)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You don't have access to edit this review"));

			await slideCheckingsRepo.UpdateExerciseCodeReview(review, text);
			var newReview = await slideCheckingsRepo.FindExerciseCodeReviewById(reviewId);

			return ReviewInfo.Build(newReview, null, false);
		}
	}
}