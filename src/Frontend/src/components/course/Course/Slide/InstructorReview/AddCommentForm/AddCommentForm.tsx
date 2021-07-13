import React, { CSSProperties } from "react";
import { Button, ScrollContainer, Hint, Toast, Gapped, } from "ui";

import { Star2, Star, Delete, } from "icons";
import { SvgIconProps } from "@skbkontur/react-icons/icons/internal/SvgIcon";

import { countLines } from "src/utils/domExtensions";

import texts from './AddCommentForm.texts';
import styles from './AddCommentForm.less';
import { MarkdownDescription } from "src/consts/comments";
import MarkdownEditor from "src/components/comments/CommentSendForm/MarkdownEditor/MarkdownEditor";

export interface FavouriteComment {
	id: number;
	text: string;
	renderedText: string;
	isFavourite?: boolean | undefined;
	useCount: number;
}

interface FavouriteCommentWithStyles {
	id: number;
	overextended?: boolean;
	ref: React.RefObject<HTMLLIElement>;
}

export interface Props {
	coordinates: { left: number; top: number; bottom: number };
	comments: FavouriteComment[];
	value: string;
	valueCanBeAddedToFavourite: boolean;

	onValueChange: (comment: string) => void;
	addComment: (comment: string) => void;
	addCommentToFavourite: (comment: string,) => Promise<FavouriteComment>;
	toggleCommentFavourite: (commentId: number,) => void;
	onClose: () => void;
}

interface State {
	favouriteCommentsIds: FavouriteCommentWithStyles[];
	otherCommentsIds: FavouriteCommentWithStyles[];
	commentsById: { [id: number]: FavouriteComment };
}

const markupByOperation: MarkdownDescription = {
	bold: {
		markup: "**",
		description: "жирный",
		hotkey: {
			asText: "Ctrl + B",
			ctrl: true,
			key: ["b", "и"],
		},
		icon: <svg xmlns="http://www.w3.org/2000/svg" width="6" height="8" viewBox="0 0 6 8" fill="none">
			<path fillRule="evenodd" clipRule="evenodd"
				  d="M4.51875 3.88C4.97555 3.49714 5.29578 2.86857 5.29578 2.28571C5.29578 0.994286 4.47166 0 3.41206 0H0.46875V8H3.7841C4.76834 8 5.53125 7.02857 5.53125 5.83429C5.53125 4.96571 5.12625 4.22286 4.51875 3.88ZM1.88142 1.42815H3.29421C3.68508 1.42815 4.00061 1.811 4.00061 2.28529C4.00061 2.75958 3.68508 3.14243 3.29421 3.14243H1.88142V1.42815ZM1.88142 6.57168H3.52968C3.92055 6.57168 4.23607 6.18882 4.23607 5.71454C4.23607 5.24025 3.92055 4.85739 3.52968 4.85739H1.88142V6.57168Z"
				  fill="#808080"/>
		</svg>,
	},
	italic: {
		markup: "__",
		description: "курсив",
		hotkey: {
			asText: "Ctrl + I",
			ctrl: true,
			key: ["i", "ш"],
		},
		icon: <svg xmlns="http://www.w3.org/2000/svg" width="6" height="8" viewBox="0 0 6 8" fill="none">
			<path d="M2 0V1.71429H3.105L1.395 6.28571H0V8H4V6.28571H2.895L4.605 1.71429H6V0H2Z" fill="#808080"/>
		</svg>,
	},
	code: {
		markup: "```",
		description: "блок кода",
		hotkey: {
			asText: "Alt + Q",
			alt: true,
			key: ["q", "й"],
		},
		icon: <svg xmlns="http://www.w3.org/2000/svg" width="18" height="8" viewBox="0 0 18 8" fill="none">
			<path opacity="0.6"
				  d="M7.14844 7.56934L0.763672 4.65723V3.5498L7.14844 0.494141V2.12109L2.5957 4.06934V4.11035L7.14844 5.94238V7.56934ZM17.0469 4.56836L10.6621 7.48047V5.86035L15.2285 4.03516V4.00781L10.6621 2.05273V0.425781L17.0469 3.46094V4.56836Z"
				  fill="#808080"/>
		</svg>,
	},
};

class AddCommentForm extends React.Component<Props, State> {
	private maxRowCount = 7;
	private maxCommentLineCount = 2;
	private maxCommentHintWidth = 374;
	private wrapper = React.createRef<HTMLDivElement>();

	constructor(props: Props) {
		super(props);

		const otherComments = props.comments
			.filter(c => !c.isFavourite);
		otherComments.sort((c1, c2) => c2.useCount - c1.useCount);

		const favouriteComments = props.comments
			.filter(c => c.isFavourite);
		favouriteComments.sort((c1, c2) => c2.useCount - c1.useCount);

		this.state = {
			commentsById: props.comments.reduce((pv, cv) => ({ ...pv, [cv.id]: { ...cv } }), {}),
			otherCommentsIds: otherComments.map(c => ({ id: c.id, ref: React.createRef(), })),
			favouriteCommentsIds: favouriteComments.map(c => ({ id: c.id, ref: React.createRef() })),
		};
	}

	componentDidMount(): void {
		this.markOverExtendedComments();
		if(this.wrapper.current) {
			const rect = this.wrapper.current.getBoundingClientRect();
			if(
				rect.top <= 0 ||
				rect.bottom >= (window.innerHeight || document.documentElement.clientHeight)
			) {
				this.wrapper.current.scrollIntoView();
			}
		}
	}

	markOverExtendedComments = (): void => {
		const { otherCommentsIds, favouriteCommentsIds, } = this.state;

		const overextendedOtherComments = this.markOverextendedComments(otherCommentsIds);
		const overextendedFavouriteComments = this.markOverextendedComments(favouriteCommentsIds);

		if(overextendedOtherComments) {
			this.setState({
				otherCommentsIds: overextendedOtherComments,
			});
		}
		if(overextendedFavouriteComments) {
			this.setState({
				favouriteCommentsIds: overextendedFavouriteComments,
			});
		}
	};

	componentDidUpdate(prevProps: Readonly<Props>): void {
		const { comments, } = this.props;
		const { otherCommentsIds, favouriteCommentsIds, commentsById, } = this.state;
		const newCommentsById: { [id: number]: FavouriteComment } = comments.reduce(
			(pv, cv) => ({ ...pv, [cv.id]: { ...cv } }), {});

		if(JSON.stringify(newCommentsById) !== JSON.stringify(commentsById)) {
			this.setState({
				commentsById: newCommentsById,
			});
			if(comments.length !== prevProps.comments.length) {
				const newOtherCommentsIds = [...otherCommentsIds];
				const newFavouriteCommentsIds = [...favouriteCommentsIds];
				Object
					.values(newCommentsById)
					.filter(comment => !commentsById[comment.id])
					.forEach(comment => {
						if(comment.isFavourite) {
							newFavouriteCommentsIds.push(({ id: comment.id, ref: React.createRef(), }));
						} else {
							newOtherCommentsIds.push(({ id: comment.id, ref: React.createRef(), }));
						}
					});
				this.setState({
					otherCommentsIds: newOtherCommentsIds,
					favouriteCommentsIds: newFavouriteCommentsIds,
				}, this.markOverExtendedComments);
			}
		}
	}

	markOverextendedComments = (comments: FavouriteCommentWithStyles[]): FavouriteCommentWithStyles[] | null => {
		const newOtherComments = [...comments];
		let anyOverextended = false;
		for (let i = 0; i < newOtherComments.length; i++) {
			const comment = newOtherComments[i];
			if(comment.ref.current) {
				const linesCount = countLines(comment.ref.current);
				if(linesCount > this.maxCommentLineCount) {
					newOtherComments[i] = { ...comment, overextended: true };
					anyOverextended = true;
				}
			}
		}

		return anyOverextended
			? newOtherComments
			: null;
	};

	render = (): React.ReactElement => {
		const { coordinates, onClose, value, valueCanBeAddedToFavourite, } = this.props;
		const { otherCommentsIds, favouriteCommentsIds, } = this.state;

		const style: CSSProperties = {
			top: coordinates.top,
			left: 30,
		};

		return (
			<div ref={ this.wrapper } className={ styles.wrapper } style={ style }>
				<Delete className={ styles.closeFormButton } onClick={ onClose }/>
				{ this.renderTextareaSection(value, valueCanBeAddedToFavourite,) }
				<div className={ styles.divider }/>
				{ this.renderCommentSection(favouriteCommentsIds, otherCommentsIds) }
			</div>);
	};

	renderTextareaSection = (comment: string, canBeAddedToFavourite: boolean,): React.ReactElement => (
		<div className={ styles.addCommentSection }>
			<span className={ styles.commentsHeader }>
				{ texts.commentSectionHeaderText }
			</span>
			<MarkdownEditor
				className={ styles.addCommentTextArea }
				width={ '230px' }
				rows={ this.maxRowCount }
				maxRows={ this.maxRowCount }
				text={ comment }
				hasError={ false }
				isShowFocus={ false }
				handleChange={ this.props.onValueChange }
				handleSubmit={ this.onAddComment }
				hideDescription
				hidePlaceholder
				markupByOperation={ markupByOperation }>
				{ this.renderControls(comment.length === 0, canBeAddedToFavourite,) }
			</MarkdownEditor>
		</div>
	);

	renderControls = (sendButtonDisabled: boolean, canBeAddedToFavourite: boolean): React.ReactElement => (
		<div className={ styles.controlsWrapper }>
			<Gapped vertical={ false } gap={ 10 }>
				<Button disabled={ sendButtonDisabled }
						use={ "primary" }
						onClick={ this.onAddComment }>
					{ texts.addCommentButtonText }
				</Button>
				<Hint pos={ "top left" } text={ canBeAddedToFavourite && texts.addToFavouriteButtonText }>
					<Button disabled={ !canBeAddedToFavourite } use={ "primary" } onClick={ this.onAddToFavourite }>
						<Star/>
					</Button>
				</Hint>
			</Gapped>
		</div>
	);

	renderCommentSection = (favouriteCommentsIds: FavouriteCommentWithStyles[],
		otherCommentsIds: FavouriteCommentWithStyles[]
	): React.ReactElement => (
		<div className={ styles.favouriteSection }>
			{ this.renderCommentsHeader() }
			<ScrollContainer className={ styles.commentsScrollWrapper }>
				<div className={ styles.commentsWrapper }>
					{ this.renderFavouriteComments(favouriteCommentsIds) }
					{ this.renderOtherComments(otherCommentsIds) }
				</div>
			</ScrollContainer>
		</div>);

	renderCommentsHeader = (): React.ReactElement => (
		<header className={ styles.header }>
			{ texts.favouriteSectionHeaderText }
			{/*			<Link className={ styles.editFavouritesComments } TODO frozen due to lack of scenarios of using
				  onClick={ () => console.log("Open editing modal") }>
				<Edit/>
			</Link>*/ }
		</header>
	);

	renderFavouriteComments = (favouriteCommentsIds: FavouriteCommentWithStyles[]): React.ReactElement => (
		favouriteCommentsIds.length > 0
			? <ul className={ styles.commentsList }>
				{ favouriteCommentsIds.map(this.renderComment) }
			</ul>
			: <span className={ styles.noFavouriteCommentsText }>
				{ texts.noFavouriteCommentsText() }
			</span>);

	renderOtherComments = (otherCommentsIds: FavouriteCommentWithStyles[]): React.ReactElement => (
		<>
			<header className={ styles.header }>
				{ texts.instructorFavouriteSectionHeaderText }
			</header>
			<ul className={ styles.commentsList }>
				{ otherCommentsIds.map(this.renderComment) }
			</ul>
		</>
	);

	renderComment = (c: FavouriteCommentWithStyles): React.ReactElement => {
		const { commentsById, } = this.state;
		const { isFavourite, renderedText, } = commentsById[c.id];
		const Icon = isFavourite
			? (props: SvgIconProps) => <Star { ...props }/>
			: (props: SvgIconProps) => <Star2 { ...props }/>;
		const id = c.id.toString();

		if(c.overextended) {
			return (
				<li key={ id } ref={ c.ref }>
					<Icon
						id={ id }
						className={ isFavourite ? styles.favouriteIcon : styles.notSelectedFavouriteIcon }
						onClick={ this.onToggleFavouriteClick }/>
					<Hint pos={ "top left" } maxWidth={ this.maxCommentHintWidth }
						  text={ <span dangerouslySetInnerHTML={ { __html: renderedText } }/> }>
						<span
							className={ styles.commentTextElapsed }
							id={ id }
							onClick={ this.onCommentClick }
							dangerouslySetInnerHTML={ { __html: renderedText } }
						/>
					</Hint>
				</li>);
		}

		return (
			<li key={ id } ref={ c.ref }>
				<Icon
					id={ id }
					className={ isFavourite ? styles.favouriteIcon : styles.notSelectedFavouriteIcon }
					onClick={ this.onToggleFavouriteClick }/>
				<span
					id={ id }
					onClick={ this.onCommentClick }
					dangerouslySetInnerHTML={ { __html: renderedText } }
				/>
			</li>);
	};

	onAddComment = (): void => {
		const { addComment, value, } = this.props;

		addComment(value);
	};

	onAddToFavourite = (): void => {
		const { addCommentToFavourite, value, } = this.props;

		addCommentToFavourite(value);
	};

	onToggleFavouriteClick = (event: React.MouseEvent): void => {
		const { toggleCommentFavourite, } = this.props;

		const id = Number.parseInt(event.currentTarget.id);

		toggleCommentFavourite(id);
	};

	onCommentClick = (event: React.MouseEvent): void => {
		const { comments, onValueChange, } = this.props;
		const id = Number.parseInt(event.currentTarget.id);
		const commentText = comments.find(c => c.id === id)?.text;

		onValueChange(commentText || '');
	};
}

export default AddCommentForm;
