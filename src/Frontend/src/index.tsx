import React from 'react';
import ReactDOM from 'react-dom';
import UlearnApp from 'src/App';
import * as Sentry from "@sentry/react";
import { Integrations } from "@sentry/tracing";
import '../config/polyfills.js';
import "src/externalComponentRenderer";

Sentry.init({
	dsn: "https://62e9c6b9ae6a47399a2b79600f1cacc5@sentry.skbkontur.ru/781",
	integrations: [new Integrations.BrowserTracing()],
});

import 'moment/locale/ru';
import "moment-timezone";

const root = document.getElementById('root');

if (root) {
	ReactDOM.render((
		<UlearnApp />
	), root);
}



/* TODO (andgein):
* Replace with
*
* import { unregister } from './registerServiceWorker';
* unregister()
*
* in future. */
function unregisterServiceWorker() {
	if (window.navigator && navigator.serviceWorker) {
		navigator.serviceWorker.getRegistrations()
		.then(function (registrations) {
			for (const registration of registrations) {
				registration.unregister();
			}
		});
	}
}

unregisterServiceWorker();
