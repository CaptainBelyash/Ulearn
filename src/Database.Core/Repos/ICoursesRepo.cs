﻿using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;
using Ulearn.Common;

namespace Database.Repos
{
	public interface ICoursesRepo
	{
		[ItemCanBeNull]
		Task<CourseVersion> GetPublishedCourseVersion(string courseId);
		Task<List<CourseVersion>> GetCourseVersions(string courseId);

		Task<CourseVersion> AddCourseVersion(string courseId, Guid versionId, string authorId,
			string pathToCourseXml, string repoUrl, string commitHash, string description);

		Task MarkCourseVersionAsPublished(Guid versionId);
		Task DeleteCourseVersion(string courseId, Guid versionId);
		Task<List<CourseVersion>> GetPublishedCourseVersions();
		Task<CourseAccess> GrantAccess(string courseId, string userId, CourseAccessType accessType, string grantedById, string comment);
		Task<List<CourseAccess>> RevokeAccess(string courseId, string userId, CourseAccessType accessType, string grantedById, string comment);
		Task<List<CourseAccess>> GetCourseAccesses(string courseId);
		Task<List<CourseAccess>> GetCourseAccesses(string courseId, string userId);
		Task<DefaultDictionary<string, List<CourseAccess>>> GetCoursesAccesses(IEnumerable<string> coursesIds);
		Task<bool> HasCourseAccess(string userId, string courseId, CourseAccessType accessType);
		Task<List<CourseAccess>> GetUserAccesses(string userId);
		Task<List<string>> GetPublishedCourseIds();
		Task<List<string>> GetCoursesUserHasAccessTo(string userId, CourseAccessType accessType);
		Task AddCourseFile(string courseId, Guid versionId, byte[] content);
		Task<CourseFile> GetCourseFile(string courseId);
		Task<List<CourseFile>> GetCourseFiles(IEnumerable<string> exceptCourseIds);
	}
}