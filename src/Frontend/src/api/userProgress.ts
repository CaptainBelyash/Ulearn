import api from "./index";
import { UsersProgressResponse } from "src/models/userProgress";

export function getUserProgressInCourse(courseId: string): Promise<UsersProgressResponse> {
	return api.post(`userProgress/${ courseId }`, api.createRequestParams({}));
}

export function updateUserProgressInCourse(courseId: string, slideId: string): Promise<UsersProgressResponse> {
	return api.post(`userProgress/${ courseId }/visit/${ slideId }`);
}
