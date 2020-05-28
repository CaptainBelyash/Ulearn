﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.CourseRoles;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Ulearn.Common.Extensions;
using Ulearn.Web.Api.Models.Responses.TempCourses;
using Ionic.Zip;


namespace Ulearn.Web.Api.Controllers
{
	[Route("/tempCourses")]
	public class TempCourseController : BaseController
	{
		private readonly ICoursesRepo coursesRepo;
		private readonly ITempCoursesRepo tempCoursesRepo;
		private readonly INotificationsRepo notificationsRepo;
		private readonly ICourseRolesRepo courseRolesRepo;

		public TempCourseController(INotificationsRepo notificationsRepo, ICoursesRepo coursesRepo, ILogger logger, IWebCourseManager courseManager, UlearnDb db, [CanBeNull] IUsersRepo usersRepo, ITempCoursesRepo tempCoursesRepo, ICourseRolesRepo courseRolesRepo)
			: base(logger, courseManager, db, usersRepo)
		{
			this.notificationsRepo = notificationsRepo;
			this.coursesRepo = coursesRepo;
			this.tempCoursesRepo = tempCoursesRepo;
			this.courseRolesRepo = courseRolesRepo;
		}

		[Authorize]
		[HttpPost("create/{courseId}")]
		public async Task<TempCourseUpdateResponse> CreateCourse([FromRoute] string courseId)
		{
			var userId = User.Identity.GetUserId();
			var tmpCourseId = courseId + userId;
			// if (!await courseRolesRepo.HasUserAccessToCourseAsync(userId, courseId, CourseRoleType.CourseAdmin))
			// {
			// 	return new TempCourseUpdateResponse()
			// 	{
			// 		ErrorType = ErrorType.Forbidden,
			// 		Message = $"Your temp version of course {courseId} already exists with id {tmpCourseId}"
			// 	};
			// }

			var tmpCourse = tempCoursesRepo.Find(tmpCourseId);
			if (tmpCourse != null)
			{
				return new TempCourseUpdateResponse()
				{
					ErrorType = ErrorType.Conflict,
					Message = $"Ваша временная версия курса {courseId} уже существует с id {tmpCourseId}." +
							$" \nYour temp version of course {courseId} already exists with id {tmpCourseId}"
				};
			}

			var versionId = Guid.NewGuid();
			var courseTitle = "Temp course";
			if (!courseManager.TryCreateCourse(tmpCourseId, courseTitle, versionId))
				throw new Exception();

			//--delete if everything will work fine without versions
			//await coursesRepo.AddCourseVersionAsync(tmpCourseId, versionId, userId, null, null, null, null).ConfigureAwait(false);
			//await coursesRepo.MarkCourseVersionAsPublishedAsync(versionId).ConfigureAwait(false);
			//await NotifyAboutPublishedCourseVersion(tmpCourseId, versionId, userId).ConfigureAwait(false); 

			await tempCoursesRepo.AddTempCourse(tmpCourseId, userId);
			var loadingTime = tempCoursesRepo.Find(tmpCourseId).LoadingTime;
			await courseRolesRepo.ToggleRoleAsync(tmpCourseId, userId, CourseRoleType.CourseAdmin, userId);
			return new TempCourseUpdateResponse()
			{
				Message = $"Временный курс с id {tmpCourseId} успешно создан.\n Course with id {tmpCourseId} successfully created",
				LastUploadTime = loadingTime
			};
		}

		[Authorize]
		[HttpGet("hasCourse/{courseId}")]
		public async Task<HasTempCourseResponse> HasCourse([FromRoute] string courseId)
		{
			var userId = User.Identity.GetUserId();
			/*if (!await courseRolesRepo.HasUserAccessToCourseAsync(userId, courseId, CourseRoleType.CourseAdmin))
				return BadRequest($"You dont have a Course Admin access to {courseId} course");*/

			var tmpCourseId = courseId + userId;
			var tmpCourse = tempCoursesRepo.Find(tmpCourseId);
			var response = new HasTempCourseResponse();
			if (tmpCourse == null)
			{
				response.HasTempCourse = false;
			}
			else
			{
				response.HasTempCourse = true;
				response.LastUploadTime = tmpCourse.LoadingTime;
				response.MainCourseId = courseId;
				response.TempCourseId = tmpCourseId;
			}

			return response;
		}


		[HttpGet("isTempCourse/{courseId}")]
		public async Task<bool> IsTempCourse([FromRoute] string courseId)
		{
			var tmpCourse = tempCoursesRepo.Find(courseId);
			return tmpCourse != null;
		}

		[HttpGet("getError/{courseId}")]
		public async Task<string> GetError([FromRoute] string courseId)
		{
			var tmpCourseError = tempCoursesRepo.GetCourseError(courseId);
			return tmpCourseError?.Error;
		}

		[HttpPost("uploadCourse/{courseId}")]
		[Authorize]
		public async Task<TempCourseUpdateResponse> UploadCourse([FromRoute] string courseId, List<IFormFile> files)
		{
			return await UploadCourse(courseId, files, false);
		}

		[HttpPost("uploadFullCourse/{courseId}")]
		[Authorize]
		public async Task<TempCourseUpdateResponse> UploadFullCourse([FromRoute] string courseId, List<IFormFile> files)
		{
			return await UploadCourse(courseId, files, true);
		}

		public async Task<TempCourseUpdateResponse> UploadCourse(string courseId, List<IFormFile> files, bool isFull)
		{
			var userId = User.Identity.GetUserId();
			// if (!await courseRolesRepo.HasUserAccessToCourseAsync(userId, courseId, CourseRoleType.CourseAdmin))
			// {
			// 	return new TempCourseUpdateResponse()
			// 		{
			// 			ErrorType = ErrorType.Forbidden,
			// 			Message = $"You dont have a Course Admin access to {courseId} course"
			// 		};
			// }
			var tmpCourseId = courseId + userId;
			var tmpCourse = tempCoursesRepo.Find(tmpCourseId);
			if (tmpCourse is null)
			{
				return new TempCourseUpdateResponse()
				{
					ErrorType = ErrorType.NotFound,
					Message = $"Вашей временной версии курса {courseId} не существует. Для создания испрользуйте метод Create"+
							$"\nYour temp version of course {courseId} does not exists. Use create method"
				};
			}

			if (files.Count != 1)
			{
				throw new Exception($"Expected count of files is 1, but was {files.Count}");
			}

			var file = files.Single();
			if (file == null || file.Length <= 0)
				throw new Exception("The file is empty");
			var fileName = Path.GetFileName(file.FileName);
			if (fileName == null || !fileName.ToLower().EndsWith(".zip"))
				throw new Exception("The file should have .zip extension");
			UploadChanges(tmpCourseId, file.OpenReadStream().ToArray());
			var filesToDelete = ExtractFileNamesToDelete(tmpCourseId);
			var error = await TryPublishChanges(tmpCourseId, filesToDelete, isFull);
			if (error != null)
			{
				await tempCoursesRepo.UpdateOrAddTempCourseError(tmpCourseId, error);
				return new TempCourseUpdateResponse()
				{
					Message = error,
					ErrorType = ErrorType.CourseError
				};
			}

			await tempCoursesRepo.MarkTempCourseAsNotErrored(tmpCourseId);
			await tempCoursesRepo.UpdateTempCourseLoadingTime(tmpCourseId);
			var loadingTime = tempCoursesRepo.Find(tmpCourseId).LoadingTime;
			return new TempCourseUpdateResponse()
			{
				Message = $"Временный курс {tmpCourseId} успешно обновлен\n.Temp course with id {tmpCourseId} successfully updated",
				LastUploadTime = loadingTime
			};
		}

		private List<string> ExtractFileNamesToDelete(string tmpCourseId)
		{
			var stagingFile = courseManager.GetStagingTempCourseFile(tmpCourseId);
			var filesToDelete = new List<string>();
			using (var zip = ZipFile.Read(stagingFile.FullName))
			{
				var e = zip["deleted.txt"];
				if (e is null)
					return new List<string>();
				var r = e.OpenReader();
				using var sr = new StreamReader(r);
				while (!sr.EndOfStream)
				{
					var line = sr.ReadLine();
					if (!string.IsNullOrEmpty(line))
						filesToDelete.Add(line);
				}
			}

			return filesToDelete.Select(x => x.Substring(1)).ToList();
		}

		private async Task<string> TryPublishChanges(string courseId, List<string> filesToDelete, bool isFull)
		{
			var revertStructure = GetRevertStructure(courseId, filesToDelete, isFull);
			DeleteFiles(revertStructure.DeletedFiles); //delete firstly
			courseManager.ExtractTempCourseChanges(courseId);

			var extractedCourseDirectory = courseManager.GetExtractedCourseDirectory(courseId);
			try
			{
				var updated = courseManager.ReloadCourseFromDirectory(extractedCourseDirectory);
				courseManager.UpdateCourseVersion(courseId, Guid.Empty); // todo do something with version
			}
			catch (Exception error)
			{
				var errorMessage = error.Message.ToLowerFirstLetter();
				while (error.InnerException != null)
				{
					errorMessage += $"\n\n{error.InnerException.Message}";
					error = error.InnerException;
				}

				RevertCourse(revertStructure);
				return errorMessage;
			}

			return null;
		}

		private void DeleteFiles(List<FileContent> filesToDelete)
		{
			filesToDelete.ForEach(file => System.IO.File.Delete(file.Path));
		}

		private RevertStructure GetRevertStructure(string courseId, List<string> filesToDeleteRelativePaths, bool isFull)
		{
			var staging = courseManager.GetStagingTempCourseFile(courseId);
			var courseDirectory = courseManager.GetExtractedCourseDirectory(courseId);
			var pathPrefix = courseDirectory.FullName;
			var zip = ZipFile.Read(staging.FullName, new ReadOptions { Encoding = Encoding.UTF8 });
			var filesToChangeRelativePaths = zip.Entries
				.Where(x => !x.IsDirectory)
				.Select(x => x.FileName)
				.Select(x => x.Replace('/', '\\'));
			var courseFileRelativePaths = Directory
				.EnumerateFiles(courseDirectory.FullName, "*.*", SearchOption.AllDirectories)
				.Select(file => file.Substring(file.IndexOf(pathPrefix) + pathPrefix.Length + 1))
				.ToHashSet();
			var revertStructure = GetRevertStructure(pathPrefix, filesToDeleteRelativePaths, filesToChangeRelativePaths.ToList(), courseFileRelativePaths, isFull);

			return revertStructure;
		}

		private static RevertStructure GetRevertStructure(string pathPrefix, List<string> filesToDeleteRelativePaths, List<string> filesToChangeRelativePaths, HashSet<string> courseFileRelativePaths, bool isFull)
		{
			if (isFull)
			{
				filesToDeleteRelativePaths.Clear();
				filesToDeleteRelativePaths.AddRange(courseFileRelativePaths);
			}

			var deletedFiles = filesToDeleteRelativePaths
				.Where(courseFileRelativePaths.Contains)
				.Select(relativePath => pathPrefix + "\\" + relativePath)
				.Select(path => new FileContent(path, System.IO.File.ReadAllBytes(path)))
				.ToList();
			return new RevertStructure
			{
				FilesBeforeChanges = filesToChangeRelativePaths
					.Where(courseFileRelativePaths.Contains)
					.Select(path => pathPrefix + "\\" + path)
					.Select(path => new FileContent(path, System.IO.File.ReadAllBytes(path)))
					.ToList(),
				AddedFiles = filesToChangeRelativePaths
					.Where(file => !courseFileRelativePaths.Contains(file))
					.Select(path => pathPrefix + "\\" + path)
					.ToList(),
				DeletedFiles = deletedFiles
			};
		}


		private void RevertCourse(RevertStructure revertStructure)
		{
			//todo lock course?
			revertStructure.FilesBeforeChanges
				.ForEach(editedFile => System.IO.File.WriteAllBytes(editedFile.Path, editedFile.Content));

			revertStructure.AddedFiles.ForEach(System.IO.File.Delete);

			revertStructure.DeletedFiles
				.ForEach(deletedFile => System.IO.File.WriteAllBytes(deletedFile.Path, deletedFile.Content));
		}


		private void UploadChanges(string courseId, byte[] content)
		{
			logger.Information($"Start upload course '{courseId}'");
			var stagingFile = courseManager.GetStagingTempCourseFile(courseId);
			System.IO.File.WriteAllBytes(stagingFile.FullName, content);
		}


		private async Task NotifyAboutCourseVersion(string courseId, Guid versionId, string userId)
		{
			var notification = new UploadedPackageNotification
			{
				CourseVersionId = versionId
			};
			await notificationsRepo.AddNotificationAsync(courseId, notification, userId);
		}

		private async Task NotifyAboutPublishedCourseVersion(string courseId, Guid versionId, string userId)
		{
			var notification = new PublishedPackageNotification
			{
				CourseVersionId = versionId,
			};
			await notificationsRepo.AddNotificationAsync(courseId, notification, userId);
		}
	}

	internal class RevertStructure
	{
		public List<FileContent> FilesBeforeChanges = new List<FileContent>();
		public List<string> AddedFiles = new List<string>();
		public List<FileContent> DeletedFiles = new List<FileContent>();
	}

	internal struct FileContent
	{
		public FileContent(string path, byte[] content)
		{
			Path = path;
			Content = content;
		}

		public readonly string Path;
		public readonly byte[] Content;
	}
}