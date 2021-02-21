import React from "react";
import GroupMembers from "./GroupMembers.js";

import "./groupMembers.less";

export default {
	title: "Settings/GroupMembers",
};

export const Default = (): React.ReactNode =>
	<GroupMembers
		systemAccesses={ ["viewAllProfiles"] }
		group={ getGroup() }
	/>;

Default.storyName = "default";

function getGroup() {
	return {
		id: 17,
		name: "asdfasdfasdfasdf",
		isArchived: false,
		owner: {
			id: "4052ea63-34dd-4398-b8bb-ac4e6a85d1d0",
			visibleName: "paradeeva",
			avatarUrl: null,
		},
		inviteHash: "b7638c37-62c6-49a9-898c-38788169987c",
		isInviteLinkEnabled: true,
		isManualCheckingEnabled: false,
		isManualCheckingEnabledForOldSolutions: false,
		defaultProhibitFurtherReview: true,
		canStudentsSeeGroupProgress: true,
		studentsCount: 0,
		accesses: [],
		apiUrl: "/groups/17",
	};
}
