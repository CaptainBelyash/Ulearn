import api from "./index";
import { Dispatch } from "redux";
import { CourseInfo, CoursesListResponse, TempCourseErrorsResponse } from "src/models/course";
import { courseUpdatedAction } from "src/actions/course";

export function getCourses() {
	return (dispatch: Dispatch): Promise<void> => {

		return api.get<{ courses: CourseInfo[] }>("courses")
			.then(json => {
				const courseById: { [courseId: string]: CourseInfo } = {};
				json.courses.forEach(c => courseById[c.id.toLowerCase()] = c);
				dispatch(courseUpdatedAction(courseById));
			});
	};
}

export function getCourse(courseId: string): Promise<CourseInfo> {
	return api.get(`courses/${ courseId }`);
}

export function getCourseErrors(courseId: string): Promise<TempCourseErrorsResponse> {
	return api.get(`tempCourses/${ courseId }/errors`);
}

export function getUserCourses(): Promise<CoursesListResponse> {
	return api.get('courses?role=instructor');
}
