import React from "react";
import { Link } from "react-router-dom";
import { Button, } from "ui";

import cn from "classnames";
import { buildQuery } from "src/utils";

import { antiPlagiarismDetailsRoute } from "src/consts/routes";

import { AntiPlagiarismStatusResponse, } from "src/models/instructor";

import styles from './AntiplagiarismHeader.less';
import texts from './AntiplagiarismHeader.texts';


export interface Props {
	fixed: boolean;
	status?: AntiPlagiarismStatusResponse;
	onZeroScoreButtonPressed: () => void;
	courseId?: string;
	submissionId?: number;
}

function AntiplagiarismHeader({
	status,
	onZeroScoreButtonPressed,
	fixed,
	submissionId,
	courseId,
}: Props): React.ReactElement {
	let text: React.ReactNode;
	let color = '';

	if(status) {
		if(status.status === 'notChecked') {
			text = texts.notCheckingText;
			color = styles.notCheckingColor;
		} else {
			switch (status.suspicionLevel) {
				case "none": {
					text = texts.getSuspicionText(0);
					color = styles.noSuspicionColor;
					break;
				}
				case "strong": {
					text = texts.getSuspicionText(status.suspiciousAuthorsCount, true);
					color = styles.strongSuspicionColor;
					break;
				}
				case "faint": {
					text = texts.getSuspicionText(status.suspiciousAuthorsCount);
					color = styles.suspicionColor;
					break;
				}
			}
		}
	} else {
		text = texts.runningText;
		color = styles.runningColor;
	}

	return (
		<div className={ cn(styles.header, color, { [styles.sticky]: fixed }) }>
			<span className={ styles.text }>{ text }</span>
			{ status && status.suspicionLevel == 'strong' &&
			<>
				<Link className={ cn(styles.seeDetailsLink, styles.text) }
					  to={ antiPlagiarismDetailsRoute + buildQuery({ courseId, submissionId }) }>
					{ texts.antiPlagiarismDetailsLinkText }
				</Link>
				<Button
					className={ styles.scoreZeroButton }
					onClick={ onZeroScoreButtonPressed }
					use={ 'danger' }>
					{ texts.scoreZeroText }
				</Button>
			</> }
		</div>
	);
}

export default AntiplagiarismHeader;
