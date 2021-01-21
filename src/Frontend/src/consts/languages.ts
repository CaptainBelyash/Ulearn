export enum Language {
	cSharp = 'csharp',
	python2 = 'python2',
	python3 = 'python3',
	java = 'java',
	javaScript = 'javascript',
	html = 'html',
	typeScript = 'typescript',
	css = 'css',
	haskell = 'haskell',
	text = 'text',
	jsx = 'jsx',
	c = 'c',
	cpp = 'cpp',
}

export interface LanguageLaunchInfo {
	compiler: string;
	compileCommand: string;
	runCommand: string;
}
