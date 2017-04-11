//mark.lawrence
//date-before.directive.js

(function () {
    angular.module('app').directive('dateBefore', function ($window) {
        return {
            require: '^ngModel',
            restrict: 'A',
            link: function (scope, elem, attrs, ngModel) {
                if (!ngModel) return; // do nothing if no ng-model
                debugger;
                var dateBefore = attrs.dateBefore;
                console.log('date', dateBefore);

                // watch own value and re-validate on change
                scope.$watch(attrs.ngModel, function () {
                    validate();
                });

                // observe the other value and re-validate on change
                attrs.$observe('dateGreaterAndEqual', function (val) {
                    validate();
                });

                var validate = function () {
                    // values
                    var dateTo = angular.isDefined(ngModel.$viewValue) === true && !_.isNull(ngModel.$viewValue) ? moment(ngModel.$viewValue).toDate() : null;
                    var dateFrom = attrs.dateGreaterAndEqual !== "" ? moment(attrs.dateGreaterAndEqual.replace('"', '').replace('\\', '').replace('"', '')).toDate() : null;
                    //passing date with braces around it causes and issue therfore we need to use replace

                };
            }
        };
    });


})();