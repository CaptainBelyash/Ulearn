import React from 'react';
import { storiesOf } from '@storybook/react';
import { action } from '@storybook/addon-actions';
import CommentsWrapper from "./CommentsWrapper";

const comments = [
	{
		id: 1999,
		text: "Решать эти задачи **можно** прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
		renderedText: "Решать эти задачи <b>можно</b> прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
		author: {
			"id": "11",
			"visibleName": "Louisa",
			"avatarUrl": null,
		},
		publishTime: "2019-01-18T14:12:41.947",
		isApproved: false,
		isPinnedToTop: false,
		isLiked: true,
		likesCount: 10,
		replies: [
			{
				id: 2000,
				author: {
					"id": "10",
					"visibleName": "Maria",
					"avatarUrl": "https://staff.skbkontur.ru/content/images/default-user-woman.png",
				},
				text: "Я **не согласна**",
				replies: [	],
				renderedText: "Я <b>не согласна</b>",
				publishTime: "2019-02-18T14:12:41.947",
				isApproved: true,
				isCorrectAnswer: false,
				likesCount: 0,
				isLiked: false,
				parentCommentId: 1999
			},
			{
				id: 2001,
				author: {
					"id": "11",
					"visibleName": "Kate",
					"avatarUrl": "https://staff.skbkontur.ru/content/images/default-user-woman.png",
				},
				text: "Я **согласна**",
				replies: [],
				renderedText: "Я <b>согласна</b>",
				publishTime: "2019-02-18T14:12:41.947",
				isApproved: false,
				isCorrectAnswer: true,
				likesCount: 5,
				isLiked: true,
				parentCommentId: 1999
			}
		],
	},
	{
		id: 2002,
		text: "Решать эти задачи **можно** прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
		renderedText: "Решать эти задачи <b>можно</b> прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
		author: {
			"id": "13",
			"visibleName": "Henry",
			"avatarUrl": null,
		},
		publishTime: "2019-01-18T14:12:41.947",
		isApproved: false,
		isPinnedToTop: false,
		isLiked: true,
		likesCount: 8,
		replies: [],
	}
];

const user = {
	"is_authenticated": true,
	"id": "11",
	"visibleName": "Pavel",
	"avatarUrl": null,
};

const userRoles = {
	"isSystemAdministrator": true,
	"courseRole": "Instructor",
	"courseAccesses": ["editPinAndRemoveComments"],
};

storiesOf('Comments/CommentsContainer', module)
.add('список комментариев', () => (
	<CommentsWrapper
		user={user}
		userRoles={userRoles}
		slideId={'90bcb61e-57f0-4baa-8bc9-10c9cfd27f58'}
		courseId={'BasicProgramming'}
		threads={comments}/>
), { viewport: 'desktop' });