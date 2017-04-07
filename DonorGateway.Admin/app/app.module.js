//app.module.js
(function() {
        var module = angular.module('app',
            [
                //angular modules
                'ngMessages',
                'angularLocalStorage',
                'ngRoute',
                'ngAnimate',

                //third party modules
                'smart-table',
                'ui.bootstrap',
                'ngTagsInput',
                'ngFileUpload',
                'rzModule',
                'switcher',
                'gfl.textAvatar',
                'textAngular',
                'ui.bootstrap.datetimepicker',
                'angular-confirm'
            ]
        ); 
    }
)();