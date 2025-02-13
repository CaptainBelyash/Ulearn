import React from "react";
import { Star2 } from "icons";

export default {
	preview: 'Превью',
	commentSectionHeaderText: 'Комментарий',
	favouriteSectionHeaderText: 'Избранные',
	instructorFavouriteSectionHeaderText: 'Комментарии других преподавателей',
	lastUsedReviewsSectionHeaderText: 'Ваши последние комментарии',
	addCommentButtonText: 'Добавить',
	addToFavouriteButtonText: 'Добавить комментарий в Избранные',
	noFavouriteCommentsText: (): React.ReactElement => (
		<>
			Чтобы добавить комментарий в Избранные,<br/> нажмите на <Star2/>
		</>),
};
