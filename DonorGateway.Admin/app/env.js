//mark.lawrence
//env.js

(function (window) {
    window.__env = window.__env || {};

    switch (window.location.hostname) {
        case 'donorgatewaystage.azurewebsites.net':
            window.__env.rsvpUrl = 'eventrsvpstage.azurewebsites.net';
            break;
        case 'donorgatewaystage.splcenter.org':
            window.__env.rsvpUrl = 'rsvpstage.splcenter.org';
            break;
        case 'donorgateway.splcenter.org':
            window.__env.rsvpUrl = 'rsvp.splcenter.org';
            break;
        case 'donorgateway.azurewebsites.net':
            window.__env.rsvpUrl = 'eventrsvp.azurewebsites.net';
            break;
        default:
            window.__env.rsvpUrl = 'localhost:53172';
    }

    // API url
    window.__env.apiUrl = 'http://' + window.location.host + '/api';

    // Base url
    window.__env.baseUrl = '/';

    // Whether or not to enable debug mode
    // Setting this to false will disable console output
    window.__env.enableDebug = true;
}(this));