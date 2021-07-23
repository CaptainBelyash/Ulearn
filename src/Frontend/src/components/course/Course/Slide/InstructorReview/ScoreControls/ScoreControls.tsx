import React from "react";
import { Button, Switcher, Toggle, Gapped, } from "ui";

import styles from './ScoreControls.less';
import texts from './ScoreControls.texts';


const defaultScores = ['0', '25', '50', '75', '100'];

export interface Props {
	scores?: string[];
	exerciseTitle: string;
	prevReviewScore?: number | 0 | 25 | 50 | 75 | 100;
	curReviewScore?: number | 0 | 25 | 50 | 75 | 100;
	curReviewDate?: string;
	isCurReviewNew?: boolean;
	toggleChecked?: boolean;
	scoreSaved?: boolean;

	onSubmit: (score: number) => void;
	onToggleChange: (value: boolean) => void;
}

interface State {
	score?: number;
	scoreSaved: boolean;
	toggleChecked: boolean;
}

class ScoreControls extends React.Component<Props, State> {
	constructor(props: Props) {
		super(props);
		const {
			curReviewScore,
			toggleChecked,
			scoreSaved
		} = this.props;

		this.state = {
			score: curReviewScore,
			scoreSaved: scoreSaved || false,
			toggleChecked: !!toggleChecked,
		};
	}

	componentDidUpdate(prevProps: Readonly<Props>): void {
		const { curReviewScore, toggleChecked } = this.props;

		if(prevProps.curReviewScore !== curReviewScore) {
			this.setState({
				toggleChecked: !!toggleChecked,
				score: curReviewScore,
				scoreSaved: curReviewScore !== undefined,
			});
		}
	}

	render(): React.ReactNode {
		const {
			scores = defaultScores,
			exerciseTitle,
			prevReviewScore,
			isCurReviewNew,
			curReviewDate,
			curReviewScore,
		} = this.props;
		const {
			scoreSaved,
			score,
			toggleChecked,
		} = this.state;

		if(!isCurReviewNew && !curReviewScore) {
			return null;
		}

		return (
			<Gapped gap={ 24 } vertical>
				{ scoreSaved && score !== undefined
					? this.renderControlsAfterSubmit(score, isCurReviewNew, curReviewDate,)
					: this.renderControls(scores, score, prevReviewScore)
				}
				{ isCurReviewNew && this.renderKeepReviewingToggle(toggleChecked, exerciseTitle,) }
			</Gapped>
		);
	}

	renderControlsAfterSubmit = (score: number, isCurReviewNew?: boolean, date?: string,): React.ReactElement => {
		return (
			<Gapped gap={ 16 } vertical={ false }>
				<span className={ styles.successLabel }>
					{ texts.getScoreText(score, !isCurReviewNew ? date : undefined) }
				</span>
				{ isCurReviewNew && <Button
					size={ "medium" }
					use={ "link" }
					onClick={ this.resetScore }
				>
					{ texts.changeScoreText }
				</Button> }
			</Gapped>
		);
	};

	renderControls = (scores: string[], score?: number, prevReviewScore?: number,): React.ReactElement => {
		return (
			<Gapped gap={ 24 } vertical={ false } className={ styles.controlsWrapper }>
				{ this.renderSwitcherWithLastReviewMarker(scores, score, prevReviewScore,) }
				<Button
					size={ 'medium' }
					disabled={ score === undefined }
					use={ 'primary' }
					onClick={ this.onSubmitClick }
				>
					{ texts.submitButtonText }
				</Button>
			</Gapped>
		);
	};

	renderSwitcherWithLastReviewMarker = (scores: string[], score?: number,
		prevReviewScore?: number,
	): React.ReactElement => {
		if(prevReviewScore === undefined) {
			return (
				<Switcher
					className={ styles.scoresLabel }
					label={ texts.scoresText }
					size={ "medium" }
					value={ score?.toString() }
					items={ scores }
					onValueChange={ this.onValueChange }/>
			);
		}
		/*
			injecting/changing something down here? check injectPrevReviewScore first or you can break something
		*/
		return (
			<span ref={ (ref) =>
				prevReviewScore !== undefined
				&& ref?.children[0]
				&& this.injectPrevReviewScore(ref, scores.findIndex(s => s === prevReviewScore.toString())) }>
				<Switcher
					className={ styles.scoresLabel }
					label={ texts.scoresText }
					size={ "medium" }
					value={ score?.toString() }
					items={ scores }
					onValueChange={ this.onValueChange }/>
				</span>
		);
	};

	//hardcoded function which injecting an last review span in parent span using absolute position
	//[1] parent span should not have any childs except switcher, or it will not render last review span
	//[2] switcher should be the first child of span
	//[3] switcher should have buttons with correct positions
	injectPrevReviewScore(element: HTMLSpanElement, index: number): void {
		if(element.children.length > 1) { //[1]
			return;
		}
		const buttons = element.children[0].getElementsByTagName('button'); //[2]
		const button = buttons[index];

		const lastReviewNode = document.createElement('span');
		lastReviewNode.className = styles.lastReviewWrapper;
		lastReviewNode.textContent = texts.lastReviewScoreText;

		element.appendChild(lastReviewNode);

		//[3]
		lastReviewNode.style.top = `${ button.offsetHeight + 6 }px`; // 6 = 2px borders + 4px margin
		lastReviewNode.style.left = `${ button.offsetLeft + (button.offsetWidth - lastReviewNode.offsetWidth) / 2 }px`;
	}

	renderKeepReviewingToggle(toggleChecked: boolean, exerciseTitle: string,): React.ReactElement {
		return (
			<Toggle checked={ toggleChecked } captionPosition={ "right" } onValueChange={ this.onToggleValueChange }>
				<span className={ styles.toggleLabel }>
					{ texts.getCodeReviewToggleText(exerciseTitle) }
				</span>
			</Toggle>
		);
	}

	resetScore = (): void => {
		this.setState({
			scoreSaved: false,
		});
	};

	onValueChange = (score: string): void => {
		this.setState({
			score: parseInt(score),
		});
	};

	onToggleValueChange = (value: boolean): void => {
		this.props.onToggleChange(value);
		this.setState({
			toggleChecked: value,
		});
	};

	onSubmitClick = (): void => {
		const {
			score,
		} = this.state;
		const {
			onSubmit,
		} = this.props;
		if(score !== undefined) {
			onSubmit(score);
		}
		this.setState({
			scoreSaved: true,
		});
	};
}

export default ScoreControls;
