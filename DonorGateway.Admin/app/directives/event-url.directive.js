//event-url.directive.js

var app = angular.module('app');

app.directive('eventurl', function ($q, $timeout, $http) {
    return {
        require: 'ngModel',
        link: function (scope, elm, attrs, ctrl) {
            ctrl.$asyncValidators.name = function (modelValue, viewValue) {

                if (ctrl.$isEmpty(modelValue)) {
                    // consider empty model valid
                    return $q.resolve();
                }

                var def = $q.defer();

                $http.get('api/event/eventnameurlavailable/' + modelValue).then(function (r) {
                    if (r.data) {
                        def.resolve();
                    } else {
                        def.reject();
                    }
                });

                return def.promise;
            };
        }
    };
});


