﻿TRUNCATE TABLE dbo.__EFMigrationsHistory

INSERT INTO dbo.__EFMigrationsHistory
(MigrationId, ProductVersion)
VALUES
('20180111150015_AddClientsSubmissionsAndSnippets', '2.2.1-servicing-10028'),
('20180111153840_AddTimeToSubmissionAndIsEnabledToClient', '2.2.1-servicing-10028'),
('20180111193436_RenameSnippetsOccurences', '2.2.1-servicing-10028'),
('20180114195922_AddLanguage', '2.2.1-servicing-10028'),
('20180115121144_RenamePositionIntoFirstTokenIndex', '2.2.1-servicing-10028'),
('20180116053421_CancelSnippetOccurenceIndexUnique', '2.2.1-servicing-10028'),
('20180118164333_AddTaskStatisticsParameters', '2.2.1-servicing-10028'),
('20180118191801_AddSubmissionTokensCount', '2.2.1-servicing-10028'),
('20180122162300_RemoveTauCoefficientFromTaskStatisticsParameters', '2.2.1-servicing-10028'),
('20180124194223_AddSnippetAuthorsCount', '2.2.1-servicing-10028'),
('20180129200336_AddSnippetStatistics', '2.2.1-servicing-10028'),
('20180202103914_InitialCreate', '2.0.1-rtm-125'),
('20180202105222_AddFullTextIndexes', '2.0.1-rtm-125'),
('20180202105253_AddCommonFeedNotificationTransportAndReplaceAdminRole', '2.0.1-rtm-125'),
('20180205190207_RenameTablesForBackwardCompatibility', '2.0.1-rtm-125'),
('20180212132751_AddAntiPlagiarismSubmissionId', '2.0.1-rtm-125'),
('20180216063554_AddSubmissionsCountToTaskStatisticsParameters', '2.2.1-servicing-10028'),
('20180219135109_AddUniqueTokenAndEnabledForClients', '2.2.1-servicing-10028'),
('20180226184621_AddExerciseCodeReviewComments', '2.0.1-rtm-125'),
('20180305190918_AddSubmissionReferenceToExerciseCodeReview', '2.0.1-rtm-125'),
('20180315185806_AddMassGroupOperationNotifications', '2.0.1-rtm-125'),
('20180413040407_AddSubmissionsLanguageAndAgentName', '2.0.1-rtm-125'),
('20180424055500_RemoveDeleteRestrictionForUser', '2.0.1-rtm-125'),
('20180426075048_CascadeDeletionForGroupAccess', '2.0.1-rtm-125'),
('20180426095138_ApplicationUserIsDeleted', '2.0.1-rtm-125'),
('20180426185339_AddLanguageIndexToSubmissionsAndSnippetIndexToOccurence', '2.2.1-servicing-10028'),
('20180427013632_RemoveRequiredAutomaticChecking', '2.0.1-rtm-125'),
('20180524205443_AddIpAddressToVisit', '2.0.1-rtm-125'),
('20180531022959_AddRecheckToPassedManualCheckingNotification', '2.0.1-rtm-125'),
('20180604180608_AddIndiciesToUserQuizzes', '2.0.1-rtm-125'),
('20180615024057_RenameColumnsForEfBackCompatibility', '2.0.1-rtm-125'),
('20180716124559_AddIndexToVisitsWithIncludeUser', '2.0.1-rtm-125'),
('20180724202211_AddCourseIdToLtiRequests', '2.0.1-rtm-125'),
('20180724204642_AddCourseIdToSlideHintIndex', '2.0.1-rtm-125'),
('20180724205919_AddCourseIdToUserQuizzesIndex', '2.0.1-rtm-125'),
('20180921194528_IncreaseQuizAnswerSize', '2.0.1-rtm-125'),
('20181011181051_RemoveUserExerciseSubmissionsApplicationUserField', '2.0.1-rtm-125'),
('20181023045805_AddCommentForInstructorsOnly', '2.0.1-rtm-125'),
('20181206181013_RemoveQuizVersions', '2.0.1-rtm-125'),
('201901212242411_RemoveIdentityFromQuizCheckingsIds', '2.0.1-rtm-125'),
('201901220511203_RenameUserQuizs', '2.0.1-rtm-125'),
('201901242025259_RemoveQuizSubmissionIsDropped', '2.0.1-rtm-125'),
('20190127110701_AddUserQuizSubmission', '2.0.1-rtm-125'),
('20190306141426_SynchronizeWithNotCore', '2.2.0-rtm-35687'),
('20190306141613_DeleteNotificationDeliveryOnDeleteNotification', '2.2.0-rtm-35687'),
('20190311111649_AddCourseFiles', '2.2.0-rtm-35687'),
('20190329125346_AddCertificateTemplateArchive', '2.2.0-rtm-35687'),
('20190425091311_CourseVersionGit', '2.2.0-rtm-35687'),
('20190426082348_CourseVersion_PathToCourseXml', '2.2.0-rtm-35687'),
('20190514091028_AddNotUploadedPackageNotification', '2.2.0-rtm-35687'),
('20190516084642_AddRepoUrl2NotUploadedPackageNotification', '2.2.0-rtm-35687'),
('20190522102812_AddCourseGitRepos', '2.2.0-rtm-35687'),
('20190524142331_AddGitCourseBranchField', '2.2.0-rtm-35687'),
('20190725081250_UpdateUserRoles', '2.2.0-rtm-35687')
