import React, { Component } from 'react';
import PropTypes from "prop-types";
import connect from "react-redux/es/connect/connect";
import {withRouter} from "react-router-dom";
import {Helmet} from "react-helmet";
import api from "../../../api/index";
import Tabs from "@skbkontur/react-ui/components/Tabs/Tabs";
import Button from "@skbkontur/react-ui/components/Button/Button";
import Toast from "@skbkontur/react-ui/components/Toast/Toast";
import GroupMembers from "../../../components/groups/GroupSettingsPage/GroupMembers/GroupMembers";
import GroupSettings from "../../../components/groups/GroupSettingsPage/GroupSettings/GroupSettings";

import styles from "./groupPage.less";

class GroupPage extends Component {

	state = {
		group: {},
		open: "settings",
		updatedFields: {},
		error: false,
		loadingAllSettings: false,
		loading: false,
		scores: [],
		scoresId: [],
	};

	componentDidMount() {
		let groupId = this.props.match.params.groupId;
		let courseId = this.props.match.params.courseId;

		this.props.enterToCourse(courseId);

		this.loadGroupScores(groupId);
		this.loadGroup(groupId);
	};

	loadGroup = (groupId) => {
		api.groups.getGroup(groupId)
			.then(group => {
			this.setState({
				group,
				loading: !this.state.scores ? true : false,
			});
		}).catch(console.error);

		this.setState({
			loading: true,
		});
	};

	loadGroupScores = (groupId) => {
		this.setState({
			loading: true,
		});

		api.groups.getGroupScores(groupId)
			.then(json => {
			let scores = json.scores;
			this.setState({
				scores: scores,
				loading: this.state.group.id ? false : true,
			});
		}).catch(console.error);
	};

	render() {
		let courseId = this.props.match.params.courseId;
		const { group, open } = this.state;

		return (
			<div className={styles.wrapper}>
				<Helmet>
					<title>{`Группа ${group.name}`}</title>
				</Helmet>
				<div className={styles["content-wrapper"]}>
					{ this.renderHeader() }
					<div className={styles.content}>
						{ (open === "settings") &&
							this.renderSettings() }
						{ (open === "members")  &&
							<GroupMembers
								courseId={courseId}
								group={group}
								onChangeGroupOwner={this.onChangeGroupOwner}
								onChangeSettings={this.onChangeSettings}/>
						}
					</div>
				</div>
			</div>
		)
	}

	renderHeader() {
		const { group, open } = this.state;

		return (
			<header className={styles["group-header"]}>
				<h2 className={styles["group-name"]}>{ group.name }</h2>
				<div className={styles["tabs-container"]}>
					<Tabs value={open} onChange={this.onChangeTab}>
						<Tabs.Tab id="settings">Настройки</Tabs.Tab>
						<Tabs.Tab id="members">Участники</Tabs.Tab>
					</Tabs>
				</div>
			</header>
		)
	}

	renderSettings() {
		const {group, loadingAllSettings, loading, scores, updatedFields, error } = this.state;
		return (
			<form onSubmit={this.sendSettings}>
				<GroupSettings
					loading={loading}
					name={updatedFields.name !== undefined ? updatedFields.name : group.name}
					group={group}
					scores={scores}
					error={error}
					onChangeName={this.onChangeName}
					onChangeSettings={this.onChangeSettings}
					onChangeScores={this.onChangeScores} />
				<Button
					size="medium"
					use="primary"
					type="submit"
					loading={loadingAllSettings}>
					Сохранить
				</Button>
			</form>
		)
	}

	onChangeTab = (_, v) => {
		this.setState({
			open: v,
		})
	};

	onChangeName = (value) => {
		const { updatedFields } = this.state.updatedFields;

		this.setState({
			updatedFields: {
				...updatedFields,
				name: value,
			}
		});
	};

	onChangeSettings = (field, value) => {
		const { group, updatedFields } = this.state;

		this.setState({
			group: {
				...group,
				[field]: value
			},
			updatedFields: {
				...updatedFields,
				[field]: value,
			}
		});
	};

	onChangeGroupOwner = (user, updatedGroupAccesses) => {
		const { group } = this.state;
		const updatedGroup = { ...group, owner: user, accesses: updatedGroupAccesses };
		this.setState({
			group: updatedGroup,
		});
	};

	onChangeScores = (key, field, value) => {
		const { scores } = this.state;
		const updatedScores = scores
			.map(item => item.id === key ? {...item, [field]: value } : item);

		const scoresInGroup = updatedScores
			.filter(item => item[field] === true)
			.map(item => item.id);

		this.setState({
			scores: updatedScores,
			scoresId: scoresInGroup,
		});
	};

	sendSettings = (e) => {
		const { group, updatedFields, scoresId } = this.state;
		let loadingScores = true;
		let loadingSettings = true;

		e.preventDefault();

		this.setState({
			loadingAllSettings: true,
			group: {
				...group,
				name: updatedFields.name,
			}
		});

		api.groups.saveGroupSettings(group.id, updatedFields)
			.then(group => {
				if (group) { loadingSettings = false; }

				this.setState({
					loadingAllSettings: Boolean(loadingSettings && loadingScores),
					group,
				});
			}).catch(console.error);

		api.groups.saveScoresSettings(group.id, scoresId)
			.then( response => {
				if (response) { loadingScores = false; }

				this.setState({
					loadingAllSettings: Boolean(loadingScores && loadingSettings),
				});
			}).catch(console.error);

		Toast.push('Настройки сохранены');
	};

	static mapStateToProps(state) {
		return {
			courses: state.courses,
		}
	}

	static mapDispatchToProps(dispatch) {
		return {
			enterToCourse: (courseId) => dispatch({
				type: 'COURSES__COURSE_ENTERED',
				courseId: courseId
			}),
		}
	}
}

GroupPage.propTypes = {
	history: PropTypes.object,
	location: PropTypes.object,
	match: PropTypes.object,
};

GroupPage = connect(GroupPage.mapStateToProps, GroupPage.mapDispatchToProps)(GroupPage);
export default withRouter(GroupPage);




