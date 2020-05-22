import React from 'react';

import YouTube from 'react-youtube';
import { BlocksWrapper, Text, } from "src/components/course/Course/Slide/Blocks";
import { ArrowChevronUp, ArrowChevronDown, } from "@skbkontur/react-icons";
import { Link } from "@skbkontur/react-ui";

import PropTypes from 'prop-types';
import classNames from 'classnames';
import { withCookies } from 'react-cookie';

import styles from './Video.less';

const videoCookieName = 'youtube-video-rate';

class Video extends React.Component {
	constructor(props) {
		super(props);

		const { openAnnotation, } = this.props;

		this.state = {
			showedAnnotation: openAnnotation,
		}
	}

	componentDidUpdate(prevProps, prevState, snapshot) {
		if(prevProps.openAnnotation !== this.props.openAnnotation) {
			this.setState({ showedAnnotation: this.props.openAnnotation });
		}
	}

	render() {
		const { videoId, className, containerClassName, autoplay, googleDocLink, } = this.props;

		const containerClassNames = classNames(styles.videoContainer, { [containerClassName]: containerClassName });
		const frameClassNames = classNames(styles.frame, { [className]: className });

		const opts = {
			playerVars: {
				autoplay,
				/* Disable related videos */
				rel: 0,
			},
		};

		return (
			<React.Fragment>
				<YouTube
					containerClassName={ containerClassNames }
					className={ frameClassNames }
					videoId={ videoId }
					opts={ opts }
					onReady={ this.onReady }
					onPlaybackRateChange={ this.onPlaybackRateChange }
				/>
				{ googleDocLink && this.renderAnnotation() }
			</React.Fragment>
		);
	}


	onReady = (event) => {
		const { cookies } = this.props;

		this.ytPlayer = event.target;
		const rate = parseFloat(cookies.get(videoCookieName) || '1');
		this.ytPlayer.setPlaybackRate(rate);
	}

	onPlaybackRateChange = ({ data }) => {
		const { cookies } = this.props;

		cookies.set(videoCookieName, data);
	}

	renderAnnotation = () => {
		const { showedAnnotation } = this.state;
		const { annotation, googleDocLink, } = this.props;
		const titleClassName = showedAnnotation ? styles.opened : styles.closed;
		return (
			<BlocksWrapper withoutBottomPaddigns>
				<Text>
					<h3 className={ classNames(styles.annotationTitle, titleClassName) }>
						<Link onClick={ this.toggleAnnotation }>
							Содержание видео
							<span className={ styles.annotationArrow }>
								{ showedAnnotation
									? <ArrowChevronUp/>
									: <ArrowChevronDown/> }
							</span>
						</Link>
					</h3>
					{ showedAnnotation && (annotation
						? <React.Fragment>
							<p>{ annotation.text }</p>
							{ annotation.fragments.map(({ text, offset }) => {
								const [hours, minutes, seconds] = offset.split(':');
								const [hoursAsInt, minutesAsInt, secondsAsInt] = [hours, minutes, seconds].map(t => Number.parseInt(t));
								const timeInSeconds = hoursAsInt * 60 * 60 + minutesAsInt * 60 + secondsAsInt;
								return (
									<p key={ offset }>
										<Link onClick={ () => this.setVideoTime(timeInSeconds) }>
											{ hoursAsInt > 0 && `${ hours }:` }
											{ `${ minutes }:` }
											{ seconds }
										</Link>
										{ ` — ${ text }` }
									</p>
								)
							})
							}
							<p className={styles.withoutMargins}>
								Ошибка в содержании? <Link href={ googleDocLink }>Предложите исправление!</Link>
							</p>
						</React.Fragment>
						: <p className={styles.withoutMargins}>
							Помогите написать <Link href={ googleDocLink }>текстовое содержание</Link> этого видео.
						</p>)
					}
				</Text>
			</BlocksWrapper>
		);
	}

	toggleAnnotation = () => {
		this.setState({
			showedAnnotation: !this.state.showedAnnotation,
		});
	}

	setVideoTime = (seconds) => {
		this.ytPlayer.seekTo(seconds);
	}
}

Video.propTypes = {
	autoplay: PropTypes.bool,
	annotation: PropTypes.object,
	openAnnotation: PropTypes.bool,
	googleDocLink: PropTypes.string,
	videoId: PropTypes.string.isRequired,
	className: PropTypes.string,
	containerClassName: PropTypes.string,
}


export default withCookies(Video);