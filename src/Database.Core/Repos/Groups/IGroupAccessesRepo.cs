using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using Ulearn.Common;

namespace Database.Repos.Groups
{
	public interface IGroupAccessesRepo
	{
		Task<GroupAccess> GrantAccessAsync(int groupId, string userId, GroupAccessType accessType, string grantedById);
		Task<bool> CanRevokeAccessAsync(int groupId, string userId, string revokedById);
		Task<List<GroupAccess>> RevokeAccessAsync(int groupId, string userId);
		Task<List<GroupAccess>> GetGroupAccessesAsync(int groupId);
		Task<bool> HasUserEditAccessToGroupAsync(int groupId, string userId);
		Task<DefaultDictionary<int, List<GroupAccess>>> GetGroupAccessesAsync(IEnumerable<int> groupsIds);
		Task<bool> HasUserGrantedAccessToGroupOrIsOwnerAsync(int groupId, string userId);
		Task<bool> IsGroupVisibleForUserAsync(int groupId, string userId);
		Task<bool> IsGroupVisibleForUserAsync(Group group, string userId);
		Task<List<Group>> GetAvailableForUserGroupsAsync(string courseId, string userId, bool needEditAccess, bool actual, bool archived);
		Task<List<Group>> GetAvailableForUserGroupsAsync(List<string> coursesIds, string userId, bool needEditAccess, bool actual, bool archived);
		Task<List<Group>> GetAvailableForUserGroupsAsync(string userId, bool needEditAccess, bool actual, bool archived);
		Task<bool> HasInstructorViewAccessToStudentGroup(string instructorId, string studentId);
		Task<bool> HasInstructorEditAccessToStudentGroup(string instructorId, string studentId);
		Task<List<string>> GetCoursesWhereUserCanSeeAllGroupsAsync(string userId, IEnumerable<string> coursesIds);
		Task<List<GroupMember>> GetMembersOfAllGroupsVisibleForUserAsync(string userId);
		Task<List<ApplicationUser>> GetInstructorsOfAllGroupsVisibleForUserAsync(string userId);
		Task<List<string>> GetInstructorsOfAllGroupsWhereUserIsMemberAsync(string courseId, string userId);
		Task<bool> CanUserSeeAllCourseGroupsAsync(string userId, string courseId, bool? isSystemAdministrator = null);
	}
}