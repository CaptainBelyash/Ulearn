import React from 'react';
import { Controlled, } from "react-codemirror2";
import Modal from "../../../../../../modal/Modal";

import CodeMirror, { Doc, Editor, EditorChange, EditorConfiguration, } from "codemirror";
import 'codemirror/addon/selection/active-line';
import 'codemirror/lib/codemirror.css';
import 'codemirror/theme/darcula.css';

import { loadLanguageStyles } from "../ExerciseUtils";
import { Language } from "../../../../../../../consts/languages";
import DataArea from "./DataArea";
import StepsCounter from "./StepsCounter";
import { Loader } from "@skbkontur/react-ui";

import styles from './Visualizer.less';
import Controls from "./Controls";

export interface Props {
	code: string;
}

function getCodeMirrorOptions(): EditorConfiguration {
	return {
		mode: loadLanguageStyles(Language.python3),
		lineNumbers: true,
		scrollbarStyle: 'null',
		lineWrapping: true,
		readOnly: false,
		matchBrackets: true,
		tabSize: 4,
		indentUnit: 4,
		indentWithTabs: true,
		theme: 'darcula',
	};
}

interface State {
	code: string;
	input: string;
	output: string;

	totalSteps: number;
	currentStep: number;

	trace: Array<Record<string, any>>;

	isLoading: boolean;
}

class Visualizer extends React.Component<Props, State> {
	constructor(props) {
		super(props);
		this.state = {
			code: props.code,
			input: props.input,
			output: props.output,
			totalSteps: 0,
			currentStep: 0,
			trace: [],
			isLoading: false,
		};
		this.updateInput = this.updateInput.bind(this);
		this.previousStep = this.previousStep.bind(this);
		this.nextStep = this.nextStep.bind(this);
		this.getRuntimeData = this.getRuntimeData.bind(this);
	}

	nextStep() : void {
		console.log('next step');
		this.setState({currentStep: this.state.currentStep + 1});
		this.showStep();
	}

	previousStep() : void {
		console.log('previous step');
		this.setState({currentStep: this.state.currentStep - 1});
		this.showStep();
	}

	getRuntimeData() : void {
		this.setState({isLoading: true});
		const xhr = new XMLHttpRequest();
		xhr.addEventListener('load', () => {
			this.run(xhr.responseText)
		});
		xhr.open('POST', 'https://python-visualizer-api.vercel.app/run');
		xhr.send(JSON.stringify({code: this.state.code, input_data: this.state.input}));
	}

	run(trace: string) : void {
		this.setState({isLoading: false, output: ''});
		const steps = JSON.parse(trace).message.trace;
		console.log('run');
		console.log(steps.length);
		this.setState({trace: steps, totalSteps: steps.length, currentStep: 1});
	}

	showStep() : void {
		const stepNumber = this.state.currentStep - 1;
		console.log('current step: ' + stepNumber);
		const currentStep = this.state.trace[stepNumber];
		console.log(JSON.stringify(currentStep));
		console.log('stdout: ' + currentStep.stdout);
		this.setState({output: currentStep.stdout});
	}

	updateInput(e) : void {
		this.setState({input: e.target.value});
	}

	render() : React.ReactElement {
		return (
			<div>
				<Modal header={ "Визуализатор" } onClose={ () => {return 0;} }>
					<Loader active={this.state.isLoading}>
						<StepsCounter totalSteps={this.state.totalSteps} currentStep={this.state.currentStep} />

						<Controlled
							options={getCodeMirrorOptions()}
							className={styles["editor"]}
							onBeforeChange={ (editor, data, value) =>
							{this.setState({code: value});} }
							onChange={ (editor, data, value) =>
							{this.setState({code: value});} }
							value={ this.state.code }
						/>

						<DataArea
							input={this.state.input}
							output={this.state.output}
							updateInput={this.updateInput}
						/>

						<Controls
							run={this.getRuntimeData}
							next={this.nextStep}
							previous={this.previousStep}
							currentStep={this.state.currentStep}
							totalSteps={this.state.totalSteps}
						/>
					</Loader>
				</Modal>
			</div>
		);
	}
}

export default Visualizer;
