import {
	ExerciseAutomaticCheckingResponse,
	ReviewCommentResponse,
	ReviewInfo,
	SubmissionInfo
} from "./exercise";
import { RootState } from "src/redux/reducers";

interface ReviewCommentResponseRedux extends ReviewCommentResponse {
	isDeleted?: boolean,
	isLoading?: boolean,
}

interface ReviewInfoRedux extends ReviewInfo {
	comments: ReviewCommentResponseRedux[];
}

interface ExerciseAutomaticCheckingResponseRedux extends ExerciseAutomaticCheckingResponse {
	reviews: ReviewInfoRedux[] | null;
}

interface SubmissionInfoRedux extends SubmissionInfo {
	automaticChecking: ExerciseAutomaticCheckingResponseRedux | null; // null если задача не имеет автоматических тестов, это не отменяет возможности ревью.
	manualCheckingReviews: ReviewInfoRedux[];
}

export { SubmissionInfoRedux, ReviewInfoRedux, RootState, };
