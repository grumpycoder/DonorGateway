//event-url.directive.js

var app = angular.module('app');

app.directive('urlAvailable', function ($q, $timeout, $http) {
    return {
        require: 'ngModel', 
        scope: {
            initialValue: '@'
        },
        link: function (scope, element, attrs, ctrl) {
            ctrl.$asyncValidators.unique = function (modelValue, viewValue) {
                var def = $q.defer();
                var value = modelValue || viewValue || '';
                var initialValue = scope.initialValue || '';

                if (!ctrl || !element.val() || !ctrl.$dirty) {
                    def.resolve();
                    return def.promise;
                }
          
                if (value.toLowerCase() !== initialValue.toLowerCase()) {
                    $http.get('api/event/eventnameurlavailable/' + modelValue).then(function (r) {
                        if (r.data) { def.resolve(); }
                        else { def.reject('Url exists'); }
                    });
                }
                else {
                    def.resolve();
                }
                return def.promise;
            };
        }
    };
});


