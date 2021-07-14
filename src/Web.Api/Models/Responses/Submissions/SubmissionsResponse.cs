﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Database.Models;
using JetBrains.Annotations;
using Ulearn.Web.Api.Models.Responses.Exercise;

namespace Ulearn.Web.Api.Models.Responses.Submissions
{
	[DataContract]
	public class SubmissionsResponse
	{
		[DataMember]
		public List<SubmissionInfo> Submissions { get; set; }

		[DataMember]
		public Dictionary<int, int?> SubmissionsScores { get; set; }

		public static SubmissionsResponse Build(
			IEnumerable<UserExerciseSubmission> submissions,
			Dictionary<int, int?> checkPercentBySubmissions,
			[CanBeNull] Dictionary<int, IEnumerable<ExerciseCodeReviewComment>> reviewId2Comments,
			bool showCheckerLogs = false
		)
		{
			return new SubmissionsResponse
			{
				Submissions = submissions.Select(s => SubmissionInfo.Build(s, reviewId2Comments, showCheckerLogs)).ToList(),
				SubmissionsScores = checkPercentBySubmissions,
			};
		}
	}
}