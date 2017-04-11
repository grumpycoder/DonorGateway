//mark.lawrence
//date-format.directive.js

(function () {
    angular.module('app')
        .directive('dateFormat',
            function (dateFilter, $parse) {
                return {
                    restrict: 'EAC',
                    require: '?ngModel',
                    link: function (scope, element, attrs, ngModel, ctrl) {
                        ngModel.$parsers.push(function (viewValue) {
                            return dateFilter(viewValue, 'MM/dd/yyyy h:mm a');
                        });
                    }
                }
            });
})();